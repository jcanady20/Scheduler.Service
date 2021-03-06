IF OBJECT_ID('[dbo].[PurgeJobHistoryByJobId]', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [dbo].[PurgeJobHistoryByJobId];
END
GO

CREATE PROCEDURE [dbo].[PurgeJobHistoryByJobId]
	@JobId INT,
	@MaxRecords INT
AS
BEGIN
	SET NOCOUNT ON;
	;WITH Records
	AS
	(
		SELECT
			ROW_NUMBER() OVER (ORDER BY RunDateTime DESC) RowId
			,*
		FROM [dbo].[JobHistory]
		WHERE JobId = @JobId
	)

	DELETE FROM Records WHERE RowId > @MaxRecords

END
GO


IF OBJECT_ID('[dbo].[PurgeJobHistory]', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [dbo].[PurgeJobHistory];
END
GO

CREATE PROCEDURE [dbo].[PurgeJobHistory]
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @PurgeRecords BIT, @MaxRecords INT, @MaxRecordsPerJob INT
	SET @PurgeRecords = (SELECT CAST(CASE ISNULL(LOWER([Value]), '1') WHEN '1' THEN 1 WHEN 'true' THEN 1 WHEN '0' THEN 0 WHEN 'false' THEN 0 END AS BIT) AS [Value] FROM [dbo].[Settings] WHERE (1=1) AND [Section] = 'JobHistory' AND [Key] = 'PurgeRecords');
	SET @MaxRecords = (SELECT Cast([Value] AS INT) FROM [dbo].[Settings] WHERE [Section] = 'JobHistory' AND [Key] = 'MaxRecords');
	SET @MaxRecordsPerJob = (SELECT Cast([Value] AS INT) FROM [dbo].[Settings] WHERE [Section] = 'JobHistory' AND [Key] = 'MaxRecordsPerJob');

	--	Purging of records is disabled
	--	Notihng to do so exit the procedure
	IF(@PurgeRecords = 0)
		RETURN;

	IF @MaxRecordsPerJob IS NOT NULL
	BEGIN
		DECLARE _cp CURSOR READ_ONLY
		FOR 
			SELECT DISTINCT [JobId]
			FROM [dbo].[JobHistory]

		DECLARE @id INT
		OPEN _cp

		FETCH NEXT FROM _cp INTO @id
		WHILE (@@fetch_status <> -1)
		BEGIN
			IF (@@fetch_status <> -2)
			BEGIN
				EXEC [dbo].[PurgeJobHistoryByJobId] @id, @MaxRecordsPerJob
			END
			FETCH NEXT FROM _cp INTO @id
		END

		CLOSE _cp
		DEALLOCATE _cp
	END

	IF @MaxRecords IS NOT NULL
	BEGIN
		;WITH Records
		AS
		(
			SELECT
				ROW_NUMBER() OVER (ORDER BY RunDateTime DESC) RowId
				,*
				FROM [dbo].[JobHistory]
		)

		DELETE FROM Records WHERE RowId > @MaxRecords
	END
END
GO