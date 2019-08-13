CREATE PROCEDURE PDD_GetProductBySegmentId
(
	@ProductSegmentManagerId INT  = 0,
	@PageIndex			int = 0, 
	@PageSize			int = 2147483644,
	@VendorId			INT = 0,
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

	DECLARE @ProductIdsTable TABLE(
		[IndexId] int,
		ProductId INT
	)

	INSERT INTO @ProductIdsTable
	SELECT 
		IndexId,
		ProductId 
	FROM PDD_GetProductIdBySegmentId(@ProductSegmentManagerId,@VendorId)
	
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
