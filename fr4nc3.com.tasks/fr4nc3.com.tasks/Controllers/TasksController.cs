using fr4nc3.com.tasks.Common;
using fr4nc3.com.tasks.CustomSettings;
using fr4nc3.com.tasks.Data;
using fr4nc3.com.tasks.DTO;
using fr4nc3.com.tasks.ExtensionMethods;
using fr4nc3.com.tasks.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace fr4nc3.com.tasks.Controllers
{
    /// <summary>
    /// Provides implementation for the task resource
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Produces("application/json")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        /// <summary>
        /// The get task by identifier route
        /// </summary>
        private const string GetTaskByIdRoute = "GetTaskByIdRoute";

        /// <summary>
        /// The database context
        /// </summary>
        private readonly MyDatabaseContext _context;

        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The configuration instance
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// The tasks limits settings
        /// </summary>
        private readonly TaskLimits _taskLimits;

        /// <summary>
        /// Initializes a new instance of the <see cref="TasksController"/> class.
        /// </summary>
        public TasksController(ILogger<TasksController> logger,
                                   MyDatabaseContext context,
                                   IConfiguration configuration,
                                   IOptions<TaskLimits> taskLimits)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _taskLimits = taskLimits.Value;

        }
        /// <summary>
        /// Determines whether this more tasks can be added.
        /// </summary>
        /// <returns>
        ///   true if more tasks can be added false if not
        /// </returns>
        private bool CanAddMoreTasks()
        {
            long totaltasks = (from c in _context.Tasks select c).Count();

            // SETTINGS:
            if (_taskLimits.MaxTaskEntrys > totaltasks)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Create a task
        /// </summary>
        /// <param name="tasksCreatePayload">The task.</param>
        /// <returns>An IAction result indicating HTTP 201 created if success otherwise BadRequest if the input is not valid.</returns>
        [ProducesResponseType(typeof(TaskResult), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [Route("tasks")]
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TasksCreatePayload tasksCreatePayload)
        {
            Tasks taskEntity = new Tasks();

            try
            {
                if (ModelState.IsValid)
                {
                    // First verify that there are not the max task allready
                    if (!CanAddMoreTasks())
                    {
                        _logger.LogWarning(LoggingEvents.MaxItem, $"TasksController CreateTask max items reached");
                        ErrorResponse errorResponse = new ErrorResponse();
                        (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.MaxEntitiesReached}");
                        errorResponse.parameterName = null;
                        return StatusCode((int)HttpStatusCode.Forbidden, errorResponse);
                    }

                    taskEntity.TaskName = tasksCreatePayload.taskName;
                    taskEntity.IsCompleted = tasksCreatePayload.isCompleted;
                    taskEntity.DueDate = DateTime.Parse(tasksCreatePayload.dueDate);
                    _logger.LogInformation(LoggingEvents.InsertItem, $"TasksController CreateTask inserted {taskEntity}");
                    _context.Tasks.Add(taskEntity);
                    _context.SaveChanges();
                }
                else
                {
                    List<ErrorResponse> errorResponses = new List<ErrorResponse>();

                    // collect validation error
                    // Enable multi-stream read
                    // The EnableMultipleStreamReadMiddleware is needed for reading from the
                    // Request Body a second time, the first time the Request.Body is read
                    // is in the middleware for deserializing the task Input

                    // This allows us access to the raw input
                    using StreamReader sr = new StreamReader(Request.Body);
                    Request.Body.Seek(0, SeekOrigin.Begin);
                    string inputJsonString = await sr.ReadToEndAsync();

                    using (JsonDocument jsonDocument = JsonDocument.Parse(inputJsonString))
                    {
                        // This is an approach for determining which properties have errors and knowing the
                        // property name as its the key value
                        foreach (string key in ModelState.Keys)
                        {
                            if (ModelState[key].ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                            {
                                foreach (Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error in ModelState[key].Errors)
                                {
                                    string cleansedKey = key.CleanseModelStateKey();
                                    string camelCaseKey = cleansedKey.ToCamelCase();

                                    System.Diagnostics.Trace.WriteLine($"MODEL ERROR: key:{cleansedKey} attemtedValue:{jsonDocument.RootElement.GetProperty(camelCaseKey)}, errorMessage:{error.ErrorMessage}");
                                    _logger.LogWarning(LoggingEvents.InvalidItem, $"TasksController CreateTask invalid item {key}");
                                    ErrorResponse errorResponse = new ErrorResponse();
                                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage(error.ErrorMessage);
                                    errorResponse.parameterName = camelCaseKey;
                                    errorResponse.parameterValue = jsonDocument.RootElement.GetProperty(camelCaseKey).ToString();
                                    errorResponses.Add(errorResponse);
                                }
                            }
                        }
                    }

                    return BadRequest(errorResponses);
                }
            }

            catch (Exception ex)
            {
                // sql error try to insert
                _logger.LogError(LoggingEvents.InternalError, ex, $"TaskController Create task caused an internal. paypload {tasksCreatePayload} task entity {taskEntity}");
                ErrorResponse errorResponse = new ErrorResponse();
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.EntityAlreadyExist}");
                errorResponse.parameterName = "taskName";
                errorResponse.parameterValue = tasksCreatePayload.taskName;
                return StatusCode((int)HttpStatusCode.Conflict, errorResponse);
            }
            return CreatedAtRoute(GetTaskByIdRoute, new { id = taskEntity.Id }, new TaskResult(taskEntity));
        }

        /// <summary>
        /// Updates a task
        /// </summary>
        /// <param name="id">The identifier of the task .</param>
        /// <param name="tasksCreatePayload">The task.</param>
        /// <returns>An IAction result indicating HTTP 204 no content if success update
        /// HTTP 201 if successful create
        /// otherwise BadRequest if the input is not valid.</returns>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [Route("tasks/{id}")]
        [HttpPatch]
        public async Task<IActionResult> UpdateTask(long id, [FromBody] TasksCreatePayload tasksCreatePayload)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Tasks taskEntity = (from c in _context.Tasks where c.Id == id select c).SingleOrDefault();

                    // if the task entity was not found
                    if (taskEntity == null)
                    {
                        _logger.LogWarning(LoggingEvents.UpdateItemNotFound, $"TasksControllerUpdateTaskByI(id=[{id}]) task doesn't exist.", id);
                        ErrorResponse errorResponse = new ErrorResponse();
                        (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.EntityNoFound}");
                        errorResponse.parameterName = "id";
                        errorResponse.parameterValue = $"{id}";
                        return StatusCode((int)HttpStatusCode.NotFound, errorResponse);
                    }

                    // Update the entity specified by the caller.
                    taskEntity.TaskName = tasksCreatePayload.taskName;
                    taskEntity.IsCompleted = tasksCreatePayload.isCompleted;
                    taskEntity.DueDate = DateTime.Parse(tasksCreatePayload.dueDate);
                    _logger.LogInformation(LoggingEvents.UpdateItem, $"TasksController Update task {taskEntity}");
                    _context.SaveChanges();
                }
                else
                {
                    List<ErrorResponse> errorResponses = new List<ErrorResponse>();

                    // collect error from validation of the pauyload
                    // Enable multi-stream read
                    // The EnableMultipleStreamReadMiddleware is needed for reading from the
                    // Request Body a second time, the first time the Request.Body is read
                    // is in the middleware for deserializing the task Input

                    // This allows us access to the raw input
                    using StreamReader sr = new StreamReader(Request.Body);
                    Request.Body.Seek(0, SeekOrigin.Begin);
                    string inputJsonString = await sr.ReadToEndAsync();

                    using (JsonDocument jsonDocument = JsonDocument.Parse(inputJsonString))
                    {
                        // This is an approach for determining which properties have errors and knowing the
                        // property name as its the key value
                        foreach (string key in ModelState.Keys)
                        {
                            if (ModelState[key].ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                            {
                                foreach (Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error in ModelState[key].Errors)
                                {
                                    string cleansedKey = key.CleanseModelStateKey();
                                    string camelCaseKey = cleansedKey.ToCamelCase();

                                    System.Diagnostics.Trace.WriteLine($"MODEL ERROR: key:{cleansedKey} attemtedValue:{jsonDocument.RootElement.GetProperty(camelCaseKey)}, errorMessage:{error.ErrorMessage}");
                                    _logger.LogWarning(LoggingEvents.InvalidItem, $"TasksController CreateTask invalid item");
                                    ErrorResponse errorResponse = new ErrorResponse();
                                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage(error.ErrorMessage);
                                    errorResponse.parameterName = camelCaseKey;
                                    errorResponse.parameterValue = jsonDocument.RootElement.GetProperty(camelCaseKey).ToString();
                                    errorResponses.Add(errorResponse);
                                }
                            }
                        }
                    }

                    return BadRequest(errorResponses);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, $"TaskController update task caused an internal. paypload {tasksCreatePayload} task entity");
                ErrorResponse errorResponse = new ErrorResponse();
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.EntityAlreadyExist}");
                errorResponse.parameterName = "taskName";
                errorResponse.parameterValue = tasksCreatePayload.taskName;
                return StatusCode((int)HttpStatusCode.Conflict, errorResponse);
            }

            return NoContent();
        }

        /// <summary>
        /// Delete a task
        /// </summary>
        /// <param name="id">The identifier of the task </param>        
        /// <returns>An IAction result indicating HTTP 204 no content if success otherwise BadRequest if the input is not valid.</returns>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [Route("tasks/{id}")]
        [HttpDelete]
        public IActionResult DeleteTaskById(long id)
        {
            try
            {
                Tasks taskEntity = (from c in _context.Tasks where c.Id == id select c).SingleOrDefault();

                if (taskEntity == null) // if entity doesn't exist
                {
                    _logger.LogWarning(LoggingEvents.DeleteItemNotFound, $"TasksController DeleteTaskByI(id=[{id}]) task doesn't exist.", id);
                    ErrorResponse errorResponse = new ErrorResponse();
                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.EntityNoFound}");
                    errorResponse.parameterName = "id";
                    errorResponse.parameterValue = $"{id}";
                    return StatusCode((int)HttpStatusCode.NotFound, errorResponse);
                }

                _context.Tasks.Remove(taskEntity);
                _logger.LogInformation(LoggingEvents.DeleteItem, $"TasksControllerDeleteTaskByI(id=[{id}]) task was delete.", id);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // default server error 
                _logger.LogError(LoggingEvents.InternalError, ex, $"TaskController delete task caused an error. paypload {id}");
                ErrorResponse errorResponse = new ErrorResponse();
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ServerError}");
                errorResponse.parameterName = null;
                return StatusCode((int)HttpStatusCode.InternalServerError, errorResponse);
            }

            return NoContent();
        }


        /// <summary>
        /// Gets the specified tasks resource based on the id parameter.
        /// </summary>
        /// <param name="id">The task's id.</param>
        /// <returns>The Task resource</returns>
        /// <remarks>
        /// Demo Notes:
        /// An Id of 0 will generate a Server Error result.
        ///</remarks>
        [HttpGet]
        [ProducesResponseType(typeof(TaskResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [Route("tasks/{id}", Name = GetTaskByIdRoute)]
        public IActionResult GetTaskById(long id)
        {

            try
            {
                Tasks taskEntity = (from c in _context.Tasks where c.Id == id select c).SingleOrDefault();

                if (taskEntity == null)
                {
                    // cannot find the entity
                    _logger.LogWarning(LoggingEvents.GetItemNotFound, $"TasksController GetTaskById(id=[{id}]) task doesn't exist.", id);
                    ErrorResponse errorResponse = new ErrorResponse();
                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.EntityNoFound}");
                    errorResponse.parameterName = "id";
                    errorResponse.parameterValue = $"{id}";
                    return StatusCode((int)HttpStatusCode.NotFound, errorResponse);
                }

                return new ObjectResult(new TaskResult(taskEntity));
            }
            catch (Exception ex)
            {
                // any server error
                _logger.LogError(LoggingEvents.InternalError, ex, $"TasksController GetTaskById(id=[{id}]) caused an internal error.", id);
                ErrorResponse errorResponse = new ErrorResponse();
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ServerError}");
                errorResponse.parameterName = null;
                errorResponse.errorDescription += ex.ToString();
                return StatusCode((int)HttpStatusCode.InternalServerError, errorResponse);

            }
        }
        /// <summary>
        /// validate url paramters very old fashion
        /// </summary>
        /// <param name="orderByDate"></param>
        /// <param name="taskStatus"></param>
        /// <returns></returns>
        private List<ErrorResponse> ValidateFilters(string orderByDate, string taskStatus)
        {

            List<ErrorResponse> errorResponses = new List<ErrorResponse>();

            if (!String.IsNullOrEmpty(taskStatus) && taskStatus.Length > 100) // if status is too long
            {
                ErrorResponse errorResponse = new ErrorResponse();
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ParameterTooLarge}");
                errorResponse.parameterName = "taskStatus";
                errorResponse.parameterValue = taskStatus;
                errorResponses.Add(errorResponse);

            }
            // if status is not the expected variables
            if (!String.IsNullOrEmpty(taskStatus) && (taskStatus.ToLower() != "notcompleted" && taskStatus.ToLower() != "completed" && taskStatus.ToLower() != "all"))
            {
                ErrorResponse errorResponse = new ErrorResponse();
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ParameterNoValid}");
                errorResponse.parameterName = "taskStatus";
                errorResponse.parameterValue = taskStatus;
                errorResponses.Add(errorResponse);
            }

            // if orderby date is too long
            if (!String.IsNullOrEmpty(orderByDate) && orderByDate.Length > 100)
            {
                ErrorResponse errorResponse = new ErrorResponse();
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ParameterTooLarge}");
                errorResponse.parameterName = "orderByDate";
                errorResponse.parameterValue = orderByDate;
                errorResponses.Add(errorResponse);
            }
            // if orderby date is not the valid parameter
            if (!String.IsNullOrEmpty(orderByDate) && (orderByDate.ToLower() != "asc" && orderByDate.ToLower() != "desc"))
            {
                ErrorResponse errorResponse = new ErrorResponse();
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ParameterNoValid}");
                errorResponse.parameterName = "orderByDate";
                errorResponse.parameterValue = orderByDate;
                errorResponses.Add(errorResponse);
            }

            return errorResponses;
        }
        /// <summary>
        /// list of all task by filters
        /// </summary>
        /// <param name="orderByDate"></param>
        /// <param name="taskStatus"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(TasksResults), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
        [Route("tasks")]
        public IActionResult GetAllTasks(string orderByDate = "asc", string taskStatus = "all") // if the variables are not comming we set default
        {
            List<ErrorResponse> errorResponses = ValidateFilters(orderByDate, taskStatus);
            if (errorResponses.Count() > 0)
            {
                return BadRequest(errorResponses); // list of error doens't pass
            }

            try
            {
                var tasksResults = (from c in _context.Tasks select c);

                if (!String.IsNullOrEmpty(taskStatus) && taskStatus.ToLower() != "all") // filter by complete status 
                {
                    bool status = taskStatus.ToLower() == "completed";
                    tasksResults = tasksResults.Where(x => x.IsCompleted == status);
                }
                if (!String.IsNullOrEmpty(orderByDate) && orderByDate.ToLower() == "desc")
                {

                    tasksResults = tasksResults.OrderByDescending(x => x.DueDate);

                }
                else // default sort
                {
                    tasksResults = tasksResults.OrderBy(x => x.DueDate);
                }
                TaskResult[] list = tasksResults.Select(x => new TaskResult(x)).ToList().ToArray();

                return new ObjectResult(new TasksResults(list));
            }
            catch (Exception ex)
            {
                // any server errror 
                _logger.LogError(LoggingEvents.InternalError, ex, $"TasksController  tkas caused an internal error.");
                ErrorResponse errorResponse = new ErrorResponse();
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ServerError}");
                errorResponse.parameterName = null;
                errorResponse.errorDescription += ex.ToString();
                return StatusCode((int)HttpStatusCode.InternalServerError, errorResponse);
            }
        }

    }
}
