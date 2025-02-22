using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;
using TodoApi.Models;

namespace TodoApi;

public class TodoFunctions
{
    private static readonly List<Todo> _todos = new();

    [Function("GetTodos")]
    public HttpResponseData GetTodos(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos")] HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.WriteAsJsonAsync(_todos);
        return response;
    }

    [Function("GetTodoById")]
    public HttpResponseData GetTodoById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos/{id}")] HttpRequestData req,
        int id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        var response = req.CreateResponse(todo == null ? HttpStatusCode.NotFound : HttpStatusCode.OK);
        if (todo != null)
        {
            response.WriteAsJsonAsync(todo);
        }
        return response;
    }

    [Function("CreateTodo")]
    public async Task<HttpResponseData> CreateTodo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todos")] HttpRequestData req)
    {
        var todo = await JsonSerializer.DeserializeAsync<Todo>(req.Body);
        if (todo == null)
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        todo.Id = _todos.Count + 1;
        _todos.Add(todo);

        var response = req.CreateResponse(HttpStatusCode.Created);
        response.Headers.Add("Location", $"/todos/{todo.Id}");
        await response.WriteAsJsonAsync(todo);
        return response;
    }

    [Function("UpdateTodo")]
    public async Task<HttpResponseData> UpdateTodo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todos/{id}")] HttpRequestData req,
        int id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo == null)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        var inputTodo = await JsonSerializer.DeserializeAsync<Todo>(req.Body);
        if (inputTodo == null)
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        todo.Title = inputTodo.Title;
        todo.IsComplete = inputTodo.IsComplete;

        var response = req.CreateResponse(HttpStatusCode.NoContent);
        return response;
    }

    [Function("DeleteTodo")]
    public HttpResponseData DeleteTodo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todos/{id}")] HttpRequestData req,
        int id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo == null)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        _todos.Remove(todo);
        return req.CreateResponse(HttpStatusCode.OK);
    }
}