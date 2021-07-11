using Microsoft.AspNetCore.Http;
using Microsoft.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestClientSDKLibrary;
using RestClientSDKLibrary.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FunctionalTestProject
{
    [TestClass]
    public class FunctionalTests
    {
        // DEMO: Local testing

        /// <summary>
        ///  API endpoint
        /// </summary>
        const string EndpointUrlString = "https://localhost:5001/";
        // const string EndpointUrlString = "http://friesco.azurewebsites.net/";
        /// <summary>
        /// Cliente Service credential
        /// </summary>
        public ServiceClientCredentials serviceClientCredentials;
        /// <summary>
        /// RestClientSDKLibraryClient
        /// </summary>
        public RestClientSDKLibraryClient client;
        /// <summary>
        /// Create payload
        /// </summary>

        public TasksCreatePayload taskPayload;

        /// <summary>
        /// initilize the variables used on all the test cases
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            serviceClientCredentials = new TokenCredentials("FakeTokenValue");
            client = new RestClientSDKLibraryClient(new Uri(EndpointUrlString), serviceClientCredentials);
        }
        /// <summary>
        /// test invalid payload
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task InValidatePayload()
        {
            // Arrange
            taskPayload = new TasksCreatePayload()
            {
                TaskName = "",
                IsCompleted = false,
                DueDate = "nodate"
            };
            // Act
            var resultObject = await client.CreateTaskWithHttpMessagesAsync(body: taskPayload);

            //Assert
            Assert.AreEqual(StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
            IList<ErrorResponse> resultPayload = resultObject.Body as IList<ErrorResponse>;
            Assert.IsTrue(resultPayload.Count() > 0);
        }
        /// <summary>
        /// invalid payload taskname too long
        /// </summary>
        /// <returns>succees</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task InValidateTooLongName()
        {
            // Arrange
            taskPayload = new TasksCreatePayload()
            {
                TaskName = "lalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalala",
                IsCompleted = false,
                DueDate = "2021-01-01"
            };
            // Act
            var resultObject = await client.CreateTaskWithHttpMessagesAsync(body: taskPayload);

            //Assert
            Assert.AreEqual(StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
            IList<ErrorResponse> resultPayload = resultObject.Body as IList<ErrorResponse>;
            Assert.IsTrue(resultPayload.Count() > 0);
        }
        /// <summary>
        /// invalid payload taskname too short
        /// </summary>
        /// <returns>succees</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task InValidateTooshortName()
        {
            // Arrange
            taskPayload = new TasksCreatePayload()
            {
                TaskName = "",
                IsCompleted = false,
                DueDate = "2021-01-01"
            };
            // Act
            var resultObject = await client.CreateTaskWithHttpMessagesAsync(body: taskPayload);

            //Assert
            Assert.AreEqual(StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
            IList<ErrorResponse> resultPayload = resultObject.Body as IList<ErrorResponse>;
            Assert.IsTrue(resultPayload.Count() > 0);
        }
        /// <summary>
        /// invalid payload duedate invalid ISO
        /// </summary>
        /// <returns>succees</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task InValidateDueDate()
        {
            // Arrange
            taskPayload = new TasksCreatePayload()
            {
                TaskName = "Buy a pad",
                IsCompleted = false,
                DueDate = "March 10"
            };
            // Act
            var resultObject = await client.CreateTaskWithHttpMessagesAsync(body: taskPayload);

            //Assert
            Assert.AreEqual(StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
            IList<ErrorResponse> resultPayload = resultObject.Body as IList<ErrorResponse>;
            Assert.IsTrue(resultPayload.Count() > 0);
        }
        /// <summary>
        /// invalid payload create taskname already exist
        /// </summary>
        /// <returns>succees</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task ConflictName()
        {
            // Arrange
            taskPayload = new TasksCreatePayload()
            {
                TaskName = "Buy groceries",
                IsCompleted = false,
                DueDate = "2021-02-05"
            };
            // Act
            var resultObject = await client.CreateTaskWithHttpMessagesAsync(body: taskPayload);

            //Assert
            Assert.AreEqual(StatusCodes.Status409Conflict, (int)resultObject.Response.StatusCode);
            ErrorResponse resultPayload = resultObject.Body as ErrorResponse;
            Assert.IsTrue(resultPayload.ErrorNumber == 1);
            Assert.IsTrue(resultPayload.ParameterName == "taskName");
            Assert.IsTrue(resultPayload.ParameterValue == "Buy groceries");
        }
        /// <summary>
        /// Task Id doesn't exist
        /// </summary>
        /// <returns>succees</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task GetTaskIdNoFoundTask()
        {
            // Arrange
            taskPayload = new TasksCreatePayload()
            {
                TaskName = "Update",
                IsCompleted = false,
                DueDate = "2021-02-05"
            };
            // Act
            var resultObject = await client.GetTaskByIdWithHttpMessagesAsync(id: 5);

            //Assert
            Assert.AreEqual(StatusCodes.Status404NotFound, (int)resultObject.Response.StatusCode);
            ErrorResponse resultPayload = resultObject.Body as ErrorResponse;
            Assert.IsTrue(resultPayload.ErrorNumber == 5);
            Assert.IsTrue(resultPayload.ParameterName == "id");
            Assert.IsTrue(resultPayload.ParameterValue == "5");
        }
        /// <summary>
        /// Task Id doesn't exist to delete
        /// </summary>
        /// <returns>succees</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task NoFoundDeleteTask()
        {
            // Arrange
            taskPayload = new TasksCreatePayload()
            {
                TaskName = "Update",
                IsCompleted = false,
                DueDate = "2021-02-05"
            };
            // Act
            var resultObject = await client.DeleteTaskByIdWithHttpMessagesAsync(id: 5);

            //Assert
            Assert.AreEqual(StatusCodes.Status404NotFound, (int)resultObject.Response.StatusCode);
            ErrorResponse resultPayload = resultObject.Body as ErrorResponse;
            Assert.IsTrue(resultPayload.ErrorNumber == 5);
            Assert.IsTrue(resultPayload.ParameterName == "id");
            Assert.IsTrue(resultPayload.ParameterValue == "5");
        }
        /// <summary>
        /// Task Id doesn't exist no update
        /// </summary>
        /// <returns>succees</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task NoFoundUpdateTask()
        {
            // Arrange
            taskPayload = new TasksCreatePayload()
            {
                TaskName = "Update",
                IsCompleted = false,
                DueDate = "2021-02-05"
            };
            // Act
            var resultObject = await client.UpdateTaskWithHttpMessagesAsync(id: 8, body: taskPayload);
            Debug.WriteLine(resultObject.Response.StatusCode);
            //Assert
            Assert.AreEqual(StatusCodes.Status404NotFound, (int)resultObject.Response.StatusCode);
            ErrorResponse resultPayload = resultObject.Body as ErrorResponse;
            Assert.IsTrue(resultPayload.ErrorNumber == 5);
            Assert.IsTrue(resultPayload.ParameterName == "id");
            Assert.IsTrue(resultPayload.ParameterValue == "8");
        }
        /// <summary>
        /// all tasks fail filters
        /// </summary>
        /// <returns>succees</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task AllTaskWithFiltersFails()
        {
            // Arrange
            string orderByDate = "decending";
            string taskStatus = "all true";
            // Act
            var resultObject = await client.GetAllTasksWithHttpMessagesAsync(orderByDate, taskStatus);

            //Assert
            Assert.AreEqual(StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
            IList<ErrorResponse> resultPayload = resultObject.Body as IList<ErrorResponse>;
            Assert.IsTrue(resultPayload.Count() > 0);
        }
        /// <summary>
        /// all task too long filters
        /// </summary>
        /// <returns>succees</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task AllTaskWithFiltersTooLong()
        {
            // Arrange
            string orderByDate = "lalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalala";
            string taskStatus = "lalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalala";
            // Act
            var resultObject = await client.GetAllTasksWithHttpMessagesAsync(orderByDate, taskStatus);
            //Assert
            Assert.AreEqual(StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
            IList<ErrorResponse> resultPayload = resultObject.Body as IList<ErrorResponse>;
            Assert.IsTrue(resultPayload.Count() > 0);
        }
        /// <summary>
        /// all tasks no filters
        /// </summary>
        /// <returns>succees</returns>
        [TestCategory("Positive")]
        [TestMethod]
        public async Task AllTaskWithoutFilters()
        {
            // Arrange
            // no filters
            // Act
            var resultObject = await client.GetAllTasksWithHttpMessagesAsync();

            //Assert
            Assert.AreEqual(StatusCodes.Status200OK, (int)resultObject.Response.StatusCode);
            TasksResults resultPayload = resultObject.Body as TasksResults;
            Assert.IsTrue(resultPayload.Tasks.Count() > 1);
        }
        /// <summary>
        /// all tasks task status filter completed
        /// </summary>
        /// <returns>succees</returns>
        [TestCategory("Positive")]
        [TestMethod]
        public async Task AllTaskWithStatusFilter()
        {
            // Arrange
            var status = "completed";
            // Act
            var resultObject = await client.GetAllTasksWithHttpMessagesAsync(taskStatus: status);
            //Assert
            Assert.AreEqual(StatusCodes.Status200OK, (int)resultObject.Response.StatusCode);
            TasksResults resultPayload = resultObject.Body as TasksResults;
            Assert.IsTrue(resultPayload.Tasks.Count() == resultPayload.Tasks.Count(x => x.IsCompleted == true));
        }
        /// <summary>
        /// all tasks task status filter notcompleted
        /// </summary>
        /// <returns>succees</returns>
        [TestCategory("Positive")]
        [TestMethod]
        public async Task AllTaskWithNoCompletedStatusFilter()
        {
            // Arrange
            var status = "notcompleted";
            // Act
            var resultObject = await client.GetAllTasksWithHttpMessagesAsync(taskStatus: status);
            //Assert
            Assert.AreEqual(StatusCodes.Status200OK, (int)resultObject.Response.StatusCode);
            TasksResults resultPayload = resultObject.Body as TasksResults;
            Assert.IsTrue(resultPayload.Tasks.Count() == resultPayload.Tasks.Count(x => x.IsCompleted == false));
        }
        /// <summary>
        /// all tasks task status filter all
        /// </summary>
        /// <returns>succees</returns>
        [TestCategory("Positive")]
        [TestMethod]
        public async Task AllTaskWithAllStatusFilter()
        {
            // Arrange
            var status = "all";
            // Act
            var resultObject = await client.GetAllTasksWithHttpMessagesAsync(taskStatus: status);
            //Assert
            Assert.AreEqual(StatusCodes.Status200OK, (int)resultObject.Response.StatusCode);
            TasksResults resultPayload = resultObject.Body as TasksResults;
            Assert.IsTrue(resultPayload.Tasks.Count() == resultPayload.Tasks.Count(x => x.IsCompleted == false || x.IsCompleted == true));
        }
        /// <summary>
        /// all tasks task due date desc
        /// </summary>
        /// <returns>succees</returns>
        [TestCategory("Positive")]
        [TestMethod]
        public async Task AllTaskWithSortDesc()
        {
            // Arrange
            var order = "desc";
            // Act
            var resultObject = await client.GetAllTasksWithHttpMessagesAsync(orderByDate: order);
            //Assert
            Assert.AreEqual(StatusCodes.Status200OK, (int)resultObject.Response.StatusCode);

        }
        /// <summary>
        /// all tasks task due date asc
        /// </summary>
        /// <returns>succees</returns>
        [TestCategory("Positive")]
        [TestMethod]
        public async Task AllTaskWithSortAsc()
        {
            // Arrange
            var order = "asc";
            // Act
            var resultObject = await client.GetAllTasksWithHttpMessagesAsync(orderByDate: order);
            //Assert
            Assert.AreEqual(StatusCodes.Status200OK, (int)resultObject.Response.StatusCode);

        }
        /// <summary>
        /// create task success
        /// </summary>
        /// <returns>succees</returns>
        [TestCategory("Positive")]
        [TestMethod]
        public async Task CreateTaskValid()
        {
            // Arrange
            Random randomName = new Random(); //only required once
            var number = randomName.Next(1, 101);
            var newName = $"new task {number}";
            taskPayload = new TasksCreatePayload()
            {
                TaskName = newName,
                IsCompleted = false,
                DueDate = "2021-12-31"
            };

            // Act
            var resultObject = await client.CreateTaskWithHttpMessagesAsync(body: taskPayload);
            //Assert
            Assert.AreEqual(StatusCodes.Status201Created, (int)resultObject.Response.StatusCode);
            TaskResult resultPayload = resultObject.Body as TaskResult;
            Assert.IsTrue(resultPayload.TaskName == newName);
        }
        /// <summary>
        /// update task valid
        /// </summary>
        /// <returns>succees</returns>
        [TestMethod]
        public async Task UpdateTaskValid()
        {
            // Arrange
            Random randomName = new Random(); //only required once
            var number = randomName.Next(200, 300);
            var newName = $"new task {number}";
            taskPayload = new TasksCreatePayload()
            {
                TaskName = newName,
                IsCompleted = false,
                DueDate = "2021-12-31"
            };

            // Act
            var resultObject = await client.UpdateTaskWithHttpMessagesAsync(id: 12, body: taskPayload);
            //Assert
            Assert.AreEqual(StatusCodes.Status204NoContent, (int)resultObject.Response.StatusCode);

        }
    }
}
