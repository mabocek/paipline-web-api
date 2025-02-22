using Microsoft.OpenApi.Models;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Todo API", Version = "v1" });
});

var app = builder.Build();

// Configure Swagger middleware
app.UseSwagger();
app.UseSwaggerUI();

// In-memory storage for todos
var todos = new List<Todo>();

// Define endpoints
app.MapGet("/todos", () => todos)
    .WithName("GetTodos")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Gets all todos",
        Description = "Returns a list of all todo items"
    });

app.MapGet("/todos/{id}", (int id) =>
    todos.FirstOrDefault(t => t.Id == id) is Todo todo
        ? Results.Ok(todo)
        : Results.NotFound())
    .WithName("GetTodoById")
    .WithOpenApi();

app.MapPost("/todos", (Todo todo) =>
{
    todo.Id = todos.Count + 1;
    todos.Add(todo);
    return Results.Created($"/todos/{todo.Id}", todo);
})
    .WithName("CreateTodo")
    .WithOpenApi();

app.MapPut("/todos/{id}", (int id, Todo inputTodo) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo == null) return Results.NotFound();

    todo.Title = inputTodo.Title;
    todo.IsComplete = inputTodo.IsComplete;
    return Results.NoContent();
})
    .WithName("UpdateTodo")
    .WithOpenApi();

app.MapDelete("/todos/{id}", (int id) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo == null) return Results.NotFound();

    todos.Remove(todo);
    return Results.Ok();
})
    .WithName("DeleteTodo")
    .WithOpenApi();

app.Run();
