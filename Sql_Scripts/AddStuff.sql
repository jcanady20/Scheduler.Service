/*	Test JOb that Executes and Empty Task (Waits 10seconds then reports Success) */
SET NOCOUNT ON;
DECLARE @JobId UNIQUEIDENTIFIER, @ScheduleId UNIQUEIDENTIFIER

SELECT
	@JobId = NEWID(),
	@ScheduleId = NEWID();

INSERT INTO [Scheduler].[Jobs] ([Id], [Name], [Description], [StartStep])
VALUES (@JobId, 'Test Task', 'Test Task', 1);

INSERT INTO [Scheduler].[JobSteps] ([JobId], [StepId], [Name], [Subsystem], [Command], [databasename])
VALUES (@JobId, 1, 'Step 1', 'EmptyTask', '', 'ApplicationCenter')

INSERT INTO [Scheduler].[JobSchedules] ([JobId], [Name], [Type], [Interval], [SubdayType], [SubdayInterval], [RelativeInterval], [RecurrenceFactor], [StartDateTime], [EndDateTime],  [LastRunOutCome])
VALUES (@JobId, 'Daily Schedule',  4, 1, 4, 5, 0, 0, '02/26/2015 09:00:00', '12/31/2099 21:00:00', 5);
GO

/*	Purge Job History */
SET NOCOUNT ON;
DECLARE @JobId UNIQUEIDENTIFIER, @ScheduleId UNIQUEIDENTIFIER

SELECT
	@JobId = NEWID(),
	@ScheduleId = NEWID();

INSERT INTO [Scheduler].[Jobs] ([Id], [Name], [Description], [StartStep])
VALUES (@JobId, 'Purge Job History', 'Purges Job History based on Specified settings', 1);

INSERT INTO [Scheduler].[JobSteps] ([JobId], [StepId], [Name], [Subsystem], [Command], [databasename], [isVisShipped])
VALUES (@JobId, 1, 'Step 1', 'SqlTask', 'EXEC [Scheduler].[PurgeJobHistory]', 'ApplicationCenter', 1)

INSERT INTO [Scheduler].[JobSchedules] ([JobId], [Name], [Type], [Interval], [SubdayType], [SubdayInterval], [RelativeInterval], [RecurrenceFactor], [StartDateTime], [EndDateTime], [LastRunOutCome])
VALUES (@JobId,'On StartUp Schedule',  64, 1, 0, 0, 0, 0, '02/26/2015 09:00:00', '12/31/2099 23:59:59', 5);
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

MERGE [Scheduler].[Settings] AS [trg]
USING [Settings] AS [src]
ON ([trg].[Section] = [src].[Section] AND [trg].[Key] = [src].[Key])
WHEN MATCHED THEN
UPDATE SET
	[Value] = [src].[Value]
WHEN NOT MATCHED THEN
INSERT ([Section], [Key], [Value])
VALUES ([src].[Section], [src].[Key], [src].[Value]);
GO
