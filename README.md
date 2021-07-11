# csharp_tasks_api

record task and status using C# CORE 5 .NET

Solution name fr4nc3.com.tasks
open project and build no error or warning

## Solution files

- controller
  taskController
- DTO
  ErrorResponse
  TasksCreatePayload
  TasksRessult
- Middleware (to read the payload body)
  enableMultipleStreamReadMiddleware
  MiddlewareEntensions
- Models
  Enum
  Tasks
- Development configuration
- Product Configuration without variables
- swagger

## Azure sql created with Visual Studio 2019

```SQL
/****** Object:  Table [dbo].[Tasks]    Script Date: 2/24/2021 2:38:24 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Tasks](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[TaskName] [nvarchar](100) NOT NULL,
	[IsCompleted] [bit] NOT NULL,
	[DueDate] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Tasks] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
```

## Default data from project

Id TaskName IsCompleted DueDate

1.  Buy groceries 0 2021-02-02 00:00:00.0000000
2.  Workout 1 2021-01-01 00:00:00.0000000
3.  Paint fence 0 2021-03-15 00:00:00.0000000
4.  Mow Lawn 0 2021-06-11 00:00:00.0000000

## MSTests

18 MStest checking all several status

## References

https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlexception.number?redirectedfrom=MSDN&view=dotnet-plat-ext-5.0#System_Data_SqlClient_SqlException_Number

https://stackoverflow.com/questions/30231877/how-to-validate-get-url-parameters-through-modelstate-with-data-annotation/30254484
