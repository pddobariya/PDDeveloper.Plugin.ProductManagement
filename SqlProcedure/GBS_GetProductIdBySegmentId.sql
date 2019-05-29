CREATE PROCEDURE GBS_GetProductIdBySegmentId
(
	@ProductSegmentManagerId INT  = 0,
	@PageIndex			int = 0, 
	@PageSize			int = 2147483644,
	@TotalRecords		int = null OUTPUT
)
AS
BEGIN

	--paging
	DECLARE @PageLowerBound int
	DECLARE @PageUpperBound int
	DECLARE @RowsToReturn int
	SET @RowsToReturn = @PageSize * (@PageIndex + 1)	
	SET @PageLowerBound = @PageSize * @PageIndex
	SET @PageUpperBound = @PageLowerBound + @PageSize + 1

	DECLARE @tmpTable TABLE (
		Id INT IDENTITY(1,1) PRIMARY KEY,
		[BeginsWith] [nvarchar](max) NULL,
		[EndsWith] [nvarchar](max) NULL,
		[DoesNotEndWith] [nvarchar](max) NULL,
		[Contains] [nvarchar](max) NULL
	)

	DECLARE @ProductIdsTable TABLE(
		[IndexId] int IDENTITY (1, 1) NOT NULL,
		ProductId INT
	)

	INSERT INTO @tmpTable
		SELECT 
			[BeginsWith],
			[EndsWith],
			[DoesNotEndWith],
			[Contains]
		FROM ProductFilterOptions 
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
	FROM Product_Include_Exclude WITH(NOLOCK) WHERE ProductType = 1		-- 1 For Include Product

	DELETE FROM @ProductIdsTable  WHERE ProductId in (SELECT ProductId FROM Product_Include_Exclude WITH(NOLOCK) WHERE ProductType = 2)

	--total records
	SELECT @TotalRecords = COUNT(1) from @ProductIdsTable

	--return products
	SELECT TOP (@RowsToReturn)
		p.*
	FROM
		Product p with (NOLOCK)
		INNER JOIN @ProductIdsTable [pi] on p.Id = [pi].[ProductId]
	WHERE
		[pi].IndexId > @PageLowerBound AND 
		[pi].IndexId < @PageUpperBound
	ORDER BY
		[pi].IndexId
END
GO
