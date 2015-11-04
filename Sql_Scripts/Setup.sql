IF NOT EXISTS(SELECT [name] FROM [sys].[schemas] WHERE [name] = 'Scheduler')
BEGIN
	EXEC ('CREATE SCHEMA [Scheduler];');
END
GO

IF OBJECT_ID('[Scheduler].[Jobs]', 'U') IS NOT NULL
BEGIN
	DROP TABLE [Scheduler].[Jobs]
END
GO

IF OBJECT_ID('[Scheduler].[Schedules]', 'U') IS NOT NULL
BEGIN
	DROP TABLE [Scheduler].[Schedules]
END
GO

IF OBJECT_ID('[Scheduler].[JobSchedules]', 'U') IS NOT NULL
BEGIN
	DROP TABLE [Scheduler].[JobSchedules]
END
GO

IF OBJECT_ID('[Scheduler].[JobSteps]', 'U') IS NOT NULL
BEGIN
	DROP TABLE [Scheduler].[JobSteps]
END
GO

IF OBJECT_ID('[Scheduler].[JobHistory]', 'U') IS NOT NULL
BEGIN
	DROP TABLE [Scheduler].[JobHistory]
END
GO

IF OBJECT_ID('[Scheduler].[Settings]', 'U') IS NOT NULL
BEGIN
	DROP TABLE [Scheduler].[Settings];
END
GO

IF OBJECT_ID('[Scheduler].[Activity]', 'U') IS NOT NULL
BEGIN
	DROP TABLE [Scheduler].[Activity];
END
GO

CREATE TABLE [Scheduler].[Jobs]
(
	[Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_Id_Jobs] DEFAULT(NEWID()),
	[Name] varchar(60) NOT NULL,
	[Enabled] Bit NOT NULL Constraint [DF_Enabled_Jobs] DEFAULT(1),
	[Description] varchar(500),
	Constraint [PK_Jobs] PRIMARY KEY NONCLUSTERED
	(
		[Id]
	)
)
GO

CREATE TABLE [Scheduler].[JobSchedules]
(
	[Id] UNIQUEIDENTIFIER NOT NULL  CONSTRAINT [DF_Id_JobSchedules] DEFAULT(NEWID()),
	[JobId] UNIQUEIDENTIFIER NOT NULL,
	[Name] varchar(60) NOT NULL,
	[Enabled] BIT NOT NULL Constraint [DF_Enabled_Schedule] DEFAULT(1),
	[Type] INT NOT NULL CONSTRAINT [DF_Type_Schedules] DEFAULT(4),
	[Interval] INT NOT NULL,
	[SubdayType] INT NOT NULL CONSTRAINT [DF_SubdayType_Schedules] DEFAULT(8),
	[SubdayInterval] INT NOT NULL,
	[RelativeInterval] INT NOT NULL,
	[RecurrenceFactor] INT NOT NULL CONSTRAINT [DF_RecurrenceFactor_Schedules] DEFAULT(0),
	[StartDateTime] DATETIME NOT NULL CONSTRAINT [DF_StartDate_Schedule] DEFAULT('01/01/1980 00:00:00'),
	[EndDateTime] DATETIME NOT NULL CONSTRAINT [DF_StartTime_Schedule] DEFAULT('12/31/9999 23:59:59'),
	[LastRunDateTime] DATETIME NOT NULL CONSTRAINT [DF_LastRunDateTime_JobSchedules] DEFAULT('01/01/1980 00:00:00'),
	CONSTRAINT [PK_JobSchedules] PRIMARY KEY NONCLUSTERED
	(
		[Id]
	)
)
GO

CREATE TABLE [Scheduler].[JobHistory]
(
	[Id] INT NOT NULL IDENTITY(1,1),
	[JobId] UNIQUEIDENTIFIER NOT NULL,
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

CREATE TABLE [Scheduler].[JobSteps]
(
	[Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_Id_JobSteps] DEFAULT(NEWID()),
	[JobId] UNIQUEIDENTIFIER NOT NULL,
	[StepId] INT NOT NULL,
	[Enabled] BIT NOT NULL CONSTRAINT [DF_Enabled_JobSteps] DEFAULT(1),
	[Name] varchar(128) NOT NULL,
	[Subsystem] VARCHAR(40) NOT NULL,
	[Command] NVARCHAR(MAX),
	[DatabaseName] VARCHAR(128),
	[UserName] varchar(128),
	[Password] varbinary(500),
	[IsVisShipped] BIT NOT NULL CONSTRAINT [DF_IsVisShipped_JobSteps] DEFAULT(0),
	CONSTRAINT [PK_JobSteps] PRIMARY KEY  NONCLUSTERED
	(
		[Id] ASC
	)
)
GO

CREATE TABLE [Scheduler].[Settings]
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

CREATE UNIQUE INDEX [UIDX_SectionKey_Scheduler_Settings] ON [Scheduler].[Settings] ([Section], [Key]);
GO

CREATE TABLE [Scheduler].[Activity]
(
	[JobId] UNIQUEIDENTIFIER NOT NULL,
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
	CONSTRAINT [PK_Activity] PRIMARY KEY NONCLUSTERED
	(
		[JobId] ASC
	)
);
GO

IF OBJECT_ID('[Scheduler].[JobActivity]', 'V') IS NOT NULL
BEGIN
	DROP VIEW [Scheduler].[JobActivity];
END
GO


CREATE VIEW [Scheduler].[JobActivity]
AS
	SELECT
		 [j].[Name]
		,[j].[Id]
		,[j].[Enabled]
		,[Scheduler].[RunREquestSource]([a].[RunSource]) AS [RunSource]
		,[Scheduler].[JobStatus]([a].[Status]) AS [Status]
		,[a].[LastExecutedStep]
		,[a].[LastStepExecutedDateTime]
		,[a].[LastStepDuration]
		,[a].[QueuedDateTime]
		,[a].[StartDateTime]
		,[a].[CompletedDateTime]
		,[a].[NextRunDateTime]
		,[Scheduler].[JobStepOutCome]([a].[LastRunOutCome]) AS [LastRunOutCome]
		,[a].[LastOutComeMessage]
		,[a].[LastRunDateTime]
		,[a].[LastRunDuration]
	FROM [Scheduler].[Jobs] AS [j]
	LEFT JOIN [Scheduler].[Activity] AS [a] ON [a].[JobId] = [j].[Id]
GO