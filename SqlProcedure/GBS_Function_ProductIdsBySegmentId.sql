CREATE FUNCTION GBS_GetProductIdBySegmentId
(
	@ProductSegmentManagerId INT  = 0
)
RETURNS @ProductIdsTable TABLE(
	[IndexId] int IDENTITY (1, 1) NOT NULL,
	ProductId INT
)
BEGIN
	
	DECLARE @tmpTable TABLE (
		Id INT IDENTITY(1,1) PRIMARY KEY,
		[BeginsWith] [nvarchar](max) NULL,
		[EndsWith] [nvarchar](max) NULL,
		[DoesNotEndWith] [nvarchar](max) NULL,
		[Contains] [nvarchar](max) NULL
	)
	INSERT INTO @tmpTable
		SELECT 
			[BeginsWith],
			[EndsWith],
			[DoesNotEndWith],
			[Contains]
		FROM GBS_ProductFilterOptions 
	WHERE ProductSegmentManagerId = @ProductSegmentManagerId



	DECLARE @id INT = 1,
			@totalRows INT = 0,
			@BeginsWith NVARCHAR(MAX),
			@EndsWith NVARCHAR(MAX),
			@DoesNotEndWith NVARCHAR(MAX),
			@Contains NVARCHAR(MAX)

	SELECT @totalRows = COUNT(1) FROM @tmpTable

	WHILE @id <= @totalRows
	BEGIN
		SELECT 
			@BeginsWith = BeginsWith,
			@EndsWith = EndsWith,
			@DoesNotEndWith = DoesNotEndWith,
			@Contains = [Contains]
		FROM @tmpTable WHERE Id = @id

		INSERT INTO @ProductIdsTable 
		SELECT 
			p.Id
		FROM Product p WITH(NOLOCK)
		WHERE (ISNULL(@BeginsWith,'') = '' OR p.Sku like @BeginsWith + '%')
		AND (ISNULL(@EndsWith,'') = '' OR  p.Sku like '%' + @EndsWith)
		AND (ISNULL(@DoesNotEndWith,'') = '' OR  p.Sku not like '%' + @DoesNotEndWith)
		AND (ISNULL(@Contains,'') = '' OR  p.Sku not like '%' + @Contains + '%')
		AND p.Published = 1 AND p.Deleted  = 0

		SET @id = @id + 1
	END
	INSERT INTO @ProductIdsTable 
	SELECT 
		ProductId 
	FROM GBS_Product_Include_Exclude WITH(NOLOCK) WHERE ProductType = 1		-- 1 For Include Product

	DELETE FROM @ProductIdsTable  WHERE ProductId in (SELECT ProductId FROM GBS_Product_Include_Exclude WITH(NOLOCK) WHERE ProductType = 2)

	RETURN
END
GO