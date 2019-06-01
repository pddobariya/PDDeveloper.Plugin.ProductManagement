CREATE PROCEDURE GBS_GetProductAttributeBySegmentId
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

	DECLARE @ProductAttributeIdsTable TABLE(
		[IndexId] int IDENTITY (1, 1) NOT NULL,
		ProductAttributeId INT
	)

	INSERT INTO @ProductAttributeIdsTable
	SELECT 
		DISTINCT 
		pm.ProductAttributeId
	FROM Product_ProductAttribute_Mapping pm
	INNER JOIN  GBS_GetProductIdBySegmentId(@ProductSegmentManagerId) ps on pm.ProductId = ps.ProductId
	
	--total records
	SELECT @TotalRecords = COUNT(1) from @ProductAttributeIdsTable

	--return products
	SELECT TOP (@RowsToReturn)
		pa.Id,
		pa.[Name],
		pa.[Description]
	FROM
		ProductAttribute pa with (NOLOCK)
		INNER JOIN @ProductAttributeIdsTable pai on pa.Id = pai.[ProductAttributeId]
	WHERE
		pai.IndexId > @PageLowerBound AND 
		pai.IndexId < @PageUpperBound
	ORDER BY
		pai.IndexId
END
GO
