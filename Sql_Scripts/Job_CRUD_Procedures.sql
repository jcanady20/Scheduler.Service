
IF OBJECT_ID('[dbo].[Job_Delete]', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [dbo].[Job_Delete];
END
GO

CREATE PROCEDURE [dbo].[Job_Delete]
	@id UNIQUEIDENTIFIER
AS
BEGIN

	DELETE FROM [dbo].[Activity] WHERE [JobId] = @id;

	DELETE FROM [dbo].[JobHistory] WHERE [JobId] = @id;
	
	DELETE FROM [dbo].[JobSteps] WHERE [JobId] = @id;
	
	DELETE FROM [dbo].[JobSchedules] WHERE [JobId] = @id;

	DELETE FROM [dbo].[Jobs] WHERE [Id] = @id;
END
GO

IF OBJECT_ID('[dbo].[Job_Insert]', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [dbo].[Job_Insert];
END
GO

CREATE PROCEDURE [dbo].[Job_Insert]
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
	SELECT * INTO #tmp FROM [dbo].[Jobs] WHERE 1=0

	INSERT INTO [dbo].[Jobs] ([Name], [Description], [Enabled])
	OUTPUT inserted.* INTO #tmp
	VALUES (@name, @description, @enabled);
	SELECT * FROM #tmp;
END
GO

IF OBJECT_ID('[dbo].[Job_Update]', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [dbo].[Job_Update];
END
GO

CREATE PROCEDURE [dbo].[Job_Update]
	@id UNIQUEIDENTIFIER,
	@name varchar(60),
	@description varchar(500),
	@enabled bit
AS
BEGIN
	
	UPDATE [dbo].[Jobs]
		SET
			 [Name] = @name
			,[Description] = @description
			,[Enabled] = @enabled
	WHERE [Id] = @id;

END
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