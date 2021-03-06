IF OBJECT_ID('[dbo].[Jobs]', 'U') IS NOT NULL
BEGIN
	DROP TABLE [dbo].[Jobs]
END
GO

IF OBJECT_ID('[dbo].[Schedules]', 'U') IS NOT NULL
BEGIN
	DROP TABLE [dbo].[Schedules]
END
GO

IF OBJECT_ID('[dbo].[JobSchedules]', 'U') IS NOT NULL
BEGIN
	DROP TABLE [dbo].[JobSchedules]
END
GO

IF OBJECT_ID('[dbo].[JobSteps]', 'U') IS NOT NULL
BEGIN
	DROP TABLE [dbo].[JobSteps]
END
GO

IF OBJECT_ID('[dbo].[JobHistory]', 'U') IS NOT NULL
BEGIN
	DROP TABLE [dbo].[JobHistory]
END
GO

IF OBJECT_ID('[dbo].[Settings]', 'U') IS NOT NULL
BEGIN
	DROP TABLE [dbo].[Settings];
END
GO

IF OBJECT_ID('[dbo].[Activity]', 'U') IS NOT NULL
BEGIN
	DROP TABLE [dbo].[Activity];
END
GO

CREATE TABLE [dbo].[Jobs]
(
	[Id] INT NOT NULL IDENTITY(1,1),
	[Name] varchar(60) NOT NULL,
	[Enabled] Bit NOT NULL Constraint [DF_Enabled_Jobs] DEFAULT(1),
	[Description] varchar(500),
	[StartStepId] INT NOT NULL CONSTRAINT [DF_StartStepId_Jobs] DEFAULT(1),
	Constraint [PK_Jobs] PRIMARY KEY NONCLUSTERED
	(
		[Id]
	)
)
GO


CREATE TABLE [dbo].[Schedules]
(
	[Id] INT NOT NULL  IDENTITY(1,1),
	[Name] varchar(60) NOT NULL,
	[Enabled] BIT NOT NULL Constraint [DF_Enabled_Schedules] DEFAULT(1),
	[Type] INT NOT NULL CONSTRAINT [DF_Type_Schedules] DEFAULT(4),
	[Interval] INT NOT NULL,
	[SubdayType] INT NOT NULL CONSTRAINT [DF_SubdayType_Schedules] DEFAULT(8),
	[SubdayInterval] INT NOT NULL,
	[RelativeInterval] INT NOT NULL,
	[RecurrenceFactor] INT NOT NULL CONSTRAINT [DF_RecurrenceFactor_Schedules] DEFAULT(0),
	[StartDate] DATETIME NOT NULL CONSTRAINT [DF_StartDate_Schedules] DEFAULT('01/01/1980'),
	[StartTime] TIME NOT NULL CONSTRAINT [DF_StartTime_Schedules] DEFAULT('00:00:00'),
	[EndDate] DATE NOT NULL CONSTRAINT [DF_EndDate_Schedules] DEFAULT('12/31/9999'),
	[EndTime] TIME NOT NULL CONSTRAINT [DF_EndTime_Schedules] DEFAULT('23:59:59'),
	CONSTRAINT [PK_Schedules] PRIMARY KEY CLUSTERED
	(
		[Id]
	)
)
GO

CREATE TABLE [dbo].[JobSchedules]
(
	[JobId] INT NOT NULL,
	[ScheduleId] INT NOT NULL,
	[LastRunDateTime] DATETIME NOT NULL CONSTRAINT [DF_LastRunDateTime] DEFAULT('01/01/1980 00:00:00'),
	CONSTRAINT [PK_JobSchedules] PRIMARY KEY CLUSTERED
	(
		[JobId], [ScheduleId]
	)
)
GO

CREATE TABLE [dbo].[JobHistory]
(
	[Id] INT NOT NULL IDENTITY(1,1),
	[JobId] INT NOT NULL,
	[StepId] INT NULL,
	[StepName] varchar(128) NOT NULL,
	[Message] varchar(1024) NULL,
	[RunStatus] INT NOT NULL,
	[RunDateTime] DATETIME NULL,
	[RunDuration] TIME NULL,
	Constraint [PK_JobHistory] PRIMARY KEY CLUSTERED
	(
		[Id]
	)
)
GO

CREATE TABLE [dbo].[JobSteps]
(
	[Id] INT NOT NULL IDENTITY(1,1),
	[JobId] INT NOT NULL,
	[StepId] INT NOT NULL,
	[Enabled] BIT NOT NULL CONSTRAINT [DF_Enabled_JobSteps] DEFAULT(1),
	[Name] varchar(128) NOT NULL,
	[Subsystem] VARCHAR(40) NOT NULL,
	[Command] NVARCHAR(MAX),
	[DataSource] varchar(250),
	[DatabaseName] VARCHAR(128),
	[UserName] varchar(128),
	[Password] varbinary(500),
	[IsUserDefined] BIT NOT NULL CONSTRAINT [DF_IsUserDefined_JobSteps] DEFAULT(1),
	[RetryAttempts] INT NOT NULL CONSTRAINT [DF_RetryAttempts_JobSteps] DEFAULT(0),
	[RetryInterval] INT NOT NULL CONSTRAINT [DF_RetryInterval_JobSteps] DEFAULT(0),
	CONSTRAINT [PK_JobSteps] PRIMARY KEY  CLUSTERED
	(
		[Id] ASC
	)
)
GO

CREATE TABLE [dbo].[Settings]
(
	[Id] INT NOT NULL IDENTITY(1,1),
	[Section] Varchar(100),
	[Key] Varchar(50),
	[Value] varchar(max),
	CONSTRAINT [PK_Scheduler_Options] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	)
)
GO

CREATE UNIQUE INDEX [UIDX_SectionKey_Scheduler_Settings] ON [dbo].[Settings] ([Section], [Key]);
GO

CREATE TABLE [dbo].[Activity]
(
	[JobId] INT NOT NULL,
	[RunSource] INT NOT NULL CONSTRAINT [DF_RunSource_Activity] DEFAULT(1),
	[Status] INT NOT NULL CONSTRAINT [DF_Status_Activity] DEFAULT(5),
	[LastExecutedStep] INT NOT NULL CONSTRAINT [DF_LastExecutedStep_Activity] DEFAULT(0),
	[LastStepExecutedDateTime] DateTime,
	[LastStepDuration] Time,
	[QueuedDateTime] DateTime,
	[StartDateTime] DateTime,
	[CompletedDateTime] DateTime,
	[NextRunDateTime] DateTime,
	[LastRunOutCome] INT NOT NULL CONSTRAINT [DF_LastRunOutCome_Activity] DEFAULT(5),
	[LastOutComeMessage] varchar(1024),
	[LastRunDateTime] DATETIME,
	[LastRunDuration] TIME,
	CONSTRAINT [PK_Activity] PRIMARY KEY CLUSTERED
	(
		[JobId] ASC
	)
);
GO


IF OBJECT_ID('[dbo].[TR_Job_Insert]', 'TR') IS NOT NULL
BEGIN
	DROP TRIGGER [dbo].[TR_Job_Insert];
END
GO

CREATE TRIGGER [dbo].[TR_Job_Insert] ON [dbo].[Jobs]
FOR INSERT
AS
BEGIN
	INSERT INTO [dbo].[Activity] ([JobId])
	SELECT
		[Id]
	FROM inserted;
END
GO


IF OBJECT_ID('[dbo].[JobStatus]', 'FN') IS NOT NULL
BEGIN
	DROP FUNCTION [dbo].[JobStatus];
END
GO

CREATE FUNCTION [dbo].[JobStatus] (@Status INT)
RETURNS varchar(50)
AS
BEGIN
	IF @Status IS NULL
	RETURN NULL;

	DECLARE @tmp AS VARCHAR(50);

	SET @tmp = CASE @Status
		WHEN 1 THEN 'Executing'
		WHEN 2 THEN 'Waiting for Worker Thread'
		WHEN 3 THEN 'Between Retries'
		WHEN 4 THEN 'Idle'
		WHEN 5 THEN 'Suspended'
		WHEN 6 THEN 'Waiting for Step to Finish'
		WHEN 7 THEN 'Performing Completion Action'
		ELSE 'Unknown'
	END

	RETURN @tmp

END
GO

IF OBJECT_ID('[dbo].[JobStepOutCome]', 'FN') IS NOT NULL
BEGIN
	DROP FUNCTION [dbo].[JobStepOutCome];
END
GO
CREATE FUNCTION [dbo].[JobStepOutCome] (@OutCome INT)
RETURNS varchar(50)
AS
BEGIN
	IF @OutCome IS NULL
	RETURN NULL;

	DECLARE @tmp AS VARCHAR(50);

	SET @tmp = CASE @OutCome
		WHEN 0 THEN 'Failed'
		WHEN 1 THEN 'Succeeded'
		WHEN 2 THEN 'Retry'
		WHEN 3 THEN 'Cancelled'
		ELSE 'Unknown'
	END

	RETURN @tmp

END
GO

IF OBJECT_ID('[dbo].[RunRequestSource]', 'FN') IS NOT NULL
BEGIN
	DROP FUNCTION [dbo].[RunRequestSource];
END
GO

CREATE FUNCTION [dbo].[RunRequestSource] (@source INT)
RETURNS varchar(50)
AS
BEGIN
	DECLARE @tmp varchar(50);
	SET @tmp = CASE @source
		WHEN 1 THEN 'Scheduler'
		WHEN 3 THEN 'Boot'
		WHEN 4 THEN 'User'
		WHEN 6 THEN 'OnIdle'
		ELSE 'Unknown'
	END

	RETURN @tmp;
END
GO


IF OBJECT_ID('[dbo].[JobActivity]', 'V') IS NOT NULL
BEGIN
	DROP VIEW [dbo].[JobActivity];
END
GO


CREATE VIEW [dbo].[JobActivity]
AS
	SELECT
		 [j].[Name]
		,[j].[Id]
		,[j].[Enabled]
		,[dbo].[RunRequestSource]([a].[RunSource]) AS [RunSource]
		,[dbo].[JobStatus]([a].[Status]) AS [Status]
		,[a].[LastExecutedStep]
		,[a].[LastStepExecutedDateTime]
		,[a].[LastStepDuration]
		,[a].[QueuedDateTime]
		,[a].[StartDateTime]
		,[a].[CompletedDateTime]
		,[a].[NextRunDateTime]
		,[dbo].[JobStepOutCome]([a].[LastRunOutCome]) AS [LastRunOutCome]
		,[a].[LastOutComeMessage]
		,[a].[LastRunDateTime]
		,[a].[LastRunDuration]
	FROM [dbo].[Jobs] AS [j]
	LEFT JOIN [dbo].[Activity] AS [a] ON [a].[JobId] = [j].[Id]
GO