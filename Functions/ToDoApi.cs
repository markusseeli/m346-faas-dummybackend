using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Functions
{
    public static class ToDoApi
    {
        static List<ToDo> items = new List<ToDo>();

        [FunctionName("CreateToDo")]
        public static async Task<IActionResult> CreateToDo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Creating a new todo list item");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<ToDoCreateModel>(requestBody);

            var todo = new ToDo() { TaskDescription = input.TaskDescription };
            items.Add(todo);
            return new OkObjectResult(todo);
        }

        [FunctionName("GetToDos")]
        public static IActionResult GetToDos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req, ILogger log)
        {
            log.LogInformation("Getting todo list items");
            return new OkObjectResult(items);
        }

        [FunctionName("GetTodoById")]
        public static IActionResult GetTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            var todo = items.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(todo);
        }

        [FunctionName("UpdateTodo")]
        public static async Task<IActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            log.LogInformation($"Updating todo with the id {id}");

            var todo = items.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                log.LogInformation($"Could not find a todo with the id {id} to update");
                return new NotFoundResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<ToDoUpdateModel>(requestBody);

            todo.IsCompleted = updated.IsCompleted;
            if (!string.IsNullOrEmpty(updated.TaskDescription))
            {
                todo.TaskDescription = updated.TaskDescription;
            }

            return new OkObjectResult(todo);
        }

        [FunctionName("DeleteTodo")]
        public static IActionResult DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            log.LogInformation($"Deleting todo with the id {id}");
            var todo = items.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                log.LogInformation($"Could not find a todo with the id {id} to delete");
                return new NotFoundResult();
            }
            items.Remove(todo);
            return new OkResult();
        }
    }
}
