
IF OBJECT_ID('[Scheduler].[Job_Delete]', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [Scheduler].[Job_Delete];
END
GO

CREATE PROCEDURE [Scheduler].[Job_Delete]
	@id UNIQUEIDENTIFIER
AS
BEGIN

	DELETE FROM [Scheduler].[Activity] WHERE [JobId] = @id;

	DELETE FROM [Scheduler].[JobHistory] WHERE [JobId] = @id;
	
	DELETE FROM [Scheduler].[JobSteps] WHERE [JobId] = @id;
	
	DELETE FROM [Scheduler].[JobSchedules] WHERE [JobId] = @id;

	DELETE FROM [Scheduler].[Jobs] WHERE [Id] = @id;
END
GO

IF OBJECT_ID('[Scheduler].[Job_Insert]', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [Scheduler].[Job_Insert];
END
GO

CREATE PROCEDURE [Scheduler].[Job_Insert]
	@name varchar(60),
	@description varchar(500),
	@enabled bit
AS
BEGIN

	SET NOCOUNT ON;
	IF OBJECT_ID('tempdb..#tmp', 'U') IS NOT NULL
	BEGIN
		DROP TABLE #tmp;
	END
	/*	Create Temp Table */
	SELECT * INTO #tmp FROM [Scheduler].[Jobs] WHERE 1=0

	INSERT INTO [Scheduler].[Jobs] ([Name], [Description], [Enabled])
	OUTPUT inserted.* INTO #tmp
	VALUES (@name, @description, @enabled);
	SELECT * FROM #tmp;
END
GO

IF OBJECT_ID('[Scheduler].[Job_Update]', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [Scheduler].[Job_Update];
END
GO

CREATE PROCEDURE [Scheduler].[Job_Update]
	@id UNIQUEIDENTIFIER,
	@name varchar(60),
	@description varchar(500),
	@enabled bit
AS
BEGIN
	
	UPDATE [Scheduler].[Jobs]
		SET
			 [Name] = @name
			,[Description] = @description
			,[Enabled] = @enabled
	WHERE [Id] = @id;

END
GO


IF OBJECT_ID('[Scheduler].[TR_Job_Insert]', 'TR') IS NOT NULL
BEGIN
	DROP TRIGGER [Scheduler].[TR_Job_Insert];
END
GO

CREATE TRIGGER [Scheduler].[TR_Job_Insert] ON [Scheduler].[Jobs]
FOR INSERT
AS
BEGIN
	INSERT INTO [Scheduler].[Activity] ([JobId])
	SELECT
		[Id]
	FROM inserted;
END
GO