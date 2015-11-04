/*	Test JOb that Executes and Empty Task (Waits 10seconds then reports Success) */
SET NOCOUNT ON;
DECLARE @JobId UNIQUEIDENTIFIER, @ScheduleId UNIQUEIDENTIFIER
DECLARE @dbname sysname = DB_NAME();
SELECT
	@JobId = NEWID(),
	@ScheduleId = NEWID();

INSERT INTO [dbo].[Jobs] ([Id], [Name], [Description])
VALUES (@JobId, 'Test Task', 'Test Task');

INSERT INTO [dbo].[JobSteps] ([JobId], [StepId], [Name], [Subsystem], [Command], [databasename])
VALUES (@JobId, 1, 'Step 1', 'EmptyTask', '', @dbname)

INSERT INTO [dbo].[JobSchedules] ([JobId], [Name], [Type], [Interval], [SubdayType], [SubdayInterval], [RelativeInterval], [RecurrenceFactor], [StartDate], [StartTime], [EndDate], [EndTime])
VALUES (@JobId, 'Daily Schedule',  4, 1, 4, 5, 0, 0, '02/26/2015', ' 09:00:00', '12/31/2099', '21:00:00');
GO

/*	Purge Job History */
SET NOCOUNT ON;
DECLARE @JobId UNIQUEIDENTIFIER, @ScheduleId UNIQUEIDENTIFIER
DECLARE @dbname sysname = DB_NAME();
SELECT
	@JobId = NEWID(),
	@ScheduleId = NEWID();

INSERT INTO [dbo].[Jobs] ([Id], [Name], [Description])
VALUES (@JobId, 'Purge Job History', 'Purges Job History based on Specified settings');

INSERT INTO [dbo].[JobSteps] ([JobId], [StepId], [Name], [Subsystem], [Command], [databasename], [isUserDefined])
VALUES (@JobId, 1, 'Step 1', 'SqlTask', 'EXEC [dbo].[PurgeJobHistory]', @dbname, 0)

INSERT INTO [dbo].[JobSchedules] ([JobId], [Name], [Type], [Interval], [SubdayType], [SubdayInterval], [RelativeInterval], [RecurrenceFactor], [StartDate], [StartTime], [EndDate], [EndTime])
VALUES (@JobId,'On StartUp Schedule',  64, 1, 0, 0, 0, 0, '02/26/2015', '09:00:00', '12/31/2099', '23:59:59');
GO

SET NOCOUNT ON;
DECLARE @tmp TABLE ([Section] varchar(30), [Key] varchar(30), [value] varchar(max) )
INSERT INTO @tmp ([Section], [Key], [Value])
VALUES
	('JobHistory', 'PurgeRecords', '1'),
	('JobHistory', 'MaxRecords', '1000'),
	('JobHistory', 'MaxRecordsPerJob', '100');

;WITH Settings
AS
(
	SELECT * FROM @tmp
)

MERGE [dbo].[Settings] AS [trg]
USING [Settings] AS [src]
ON ([trg].[Section] = [src].[Section] AND [trg].[Key] = [src].[Key])
WHEN MATCHED THEN
UPDATE SET
	[Value] = [src].[Value]
WHEN NOT MATCHED THEN
INSERT ([Section], [Key], [Value])
VALUES ([src].[Section], [src].[Key], [src].[Value]);
GO
