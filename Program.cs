using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ToDoDB"),
        new MySqlServerVersion(new Version(8, 0, 0))
    ));
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

//check if needed
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo API");
        c.RoutePrefix = string.Empty;
    });
}

app.MapGet("/", () => "Hello World!");

app.MapGet("/items/", async (ToDoDbContext db) =>
{
    var items = await db.Items.ToListAsync();
    return Results.Ok(items);
});

app.MapGet("/items/{id}", async (int id, ToDoDbContext db) =>
{
    return await db.Items.FindAsync(id) is Item item ? Results.Ok(item) : Results.NotFound();
});

app.MapPost("/items/", async (Item item, ToDoDbContext db) =>
{
    db.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});

app.MapPut("/items/{id}", async (int id, bool iscomplete, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null)
        return Results.NotFound();
    item.Iscomplete = iscomplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/items/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null)
        return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
