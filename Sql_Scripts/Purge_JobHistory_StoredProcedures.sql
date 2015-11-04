IF OBJECT_ID('[Scheduler].[PurgeJobHistoryByJobId]', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [Scheduler].[PurgeJobHistoryByJobId];
END
GO

CREATE PROCEDURE [Scheduler].[PurgeJobHistoryByJobId]
	@JobId UniqueIdentifier,
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
		FROM [Scheduler].[JobHistory]
		WHERE JobId = @JobId
	)

	DELETE FROM Records WHERE RowId > @MaxRecords

END
GO


IF OBJECT_ID('[Scheduler].[PurgeJobHistory]', 'P') IS NOT NULL
BEGIN
	DROP PROCEDURE [Scheduler].[PurgeJobHistory];
END
GO

CREATE PROCEDURE [Scheduler].[PurgeJobHistory]
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @PurgeRecords BIT, @MaxRecords INT, @MaxRecordsPerJob INT
	SET @PurgeRecords = (SELECT CAST(CASE ISNULL(LOWER([Value]), '1') WHEN '1' THEN 1 WHEN 'true' THEN 1 WHEN '0' THEN 0 WHEN 'false' THEN 0 END AS BIT) AS [Value] FROM [Scheduler].[Settings] WHERE (1=1) AND [Section] = 'JobHistory' AND [Key] = 'PurgeRecords');
	SET @MaxRecords = (SELECT Cast([Value] AS INT) FROM [Scheduler].[Settings] WHERE [Section] = 'JobHistory' AND [Key] = 'MaxRecords');
	SET @MaxRecordsPerJob = (SELECT Cast([Value] AS INT) FROM [Scheduler].[Settings] WHERE [Section] = 'JobHistory' AND [Key] = 'MaxRecordsPerJob');

	--	Purging of records is disabled
	--	Notihng to do so exit the procedure
	IF(@PurgeRecords = 0)
		RETURN;

	IF @MaxRecordsPerJob IS NOT NULL
	BEGIN
		DECLARE _cp CURSOR READ_ONLY
		FOR 
			SELECT DISTINCT [JobId]
			FROM [Scheduler].[JobHistory]

		DECLARE @id UNIQUEIDENTIFIER
		OPEN _cp

		FETCH NEXT FROM _cp INTO @id
		WHILE (@@fetch_status <> -1)
		BEGIN
			IF (@@fetch_status <> -2)
			BEGIN
				EXEC [Scheduler].[PurgeJobHistoryByJobId] @id, @MaxRecordsPerJob
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
				FROM [Scheduler].[JobHistory]
		)

		DELETE FROM Records WHERE RowId > @MaxRecords
	END
END
GO