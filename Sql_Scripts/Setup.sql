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

CREATE TABLE [dbo].[JobSchedules]
(
	[Id] UNIQUEIDENTIFIER NOT NULL  CONSTRAINT [DF_Id_JobSchedules] DEFAULT(NEWID()),
	[JobId] UNIQUEIDENTIFIER NOT NULL,
	[Name] varchar(60) NOT NULL,
	[Enabled] BIT NOT NULL Constraint [DF_Enabled_JobSchedules] DEFAULT(1),
	[Type] INT NOT NULL CONSTRAINT [DF_Type_JobSchedules] DEFAULT(4),
	[Interval] INT NOT NULL,
	[SubdayType] INT NOT NULL CONSTRAINT [DF_SubdayType_JobSchedules] DEFAULT(8),
	[SubdayInterval] INT NOT NULL,
	[RelativeInterval] INT NOT NULL,
	[RecurrenceFactor] INT NOT NULL CONSTRAINT [DF_RecurrenceFactor_JobSchedules] DEFAULT(0),
	[StartDate] DATETIME NOT NULL CONSTRAINT [DF_StartDate_JobSchedules] DEFAULT('01/01/1980'),
	[StartTime] TIME NOT NULL CONSTRAINT [DF_StartTime_JobSchedules] DEFAULT('00:00:00'),
	[EndDate] DATE NOT NULL CONSTRAINT [DF_EndDate_JobSchedules] DEFAULT('12/31/9999'),
	[EndTime] TIME NOT NULL CONSTRAINT [DF_EndTime_JobSchedules] DEFAULT('23:59:59'),
	[LastRunDateTime] DATETIME NOT NULL CONSTRAINT [DF_LastRunDateTime_JobSchedules] DEFAULT('01/01/1980 00:00:00'),
	CONSTRAINT [PK_JobSchedules] PRIMARY KEY NONCLUSTERED
	(
		[Id]
	)
)
GO

CREATE TABLE [dbo].[JobHistory]
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

CREATE TABLE [dbo].[JobSteps]
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
	[IsUserDefined] BIT NOT NULL CONSTRAINT [DF_IsUserDefined_JobSteps] DEFAULT(1),
	CONSTRAINT [PK_JobSteps] PRIMARY KEY  NONCLUSTERED
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