
IF OBJECT_ID('[Scheduler].[JobStatus]', 'FN') IS NOT NULL
BEGIN
	DROP FUNCTION [Scheduler].[JobStatus];
END
GO

CREATE FUNCTION [Scheduler].[JobStatus] (@Status INT)
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

IF OBJECT_ID('[Scheduler].[JobStepOutCome]', 'FN') IS NOT NULL
BEGIN
	DROP FUNCTION [Scheduler].[JobStepOutCome];
END
GO
CREATE FUNCTION [Scheduler].[JobStepOutCome] (@OutCome INT)
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

IF OBJECT_ID('[Scheduler].[RunRequestSource]', 'FN') IS NOT NULL
BEGIN
	DROP FUNCTION [Scheduler].[RunRequestSource];
END
GO

CREATE FUNCTION [Scheduler].[RunRequestSource] (@source INT)
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