CREATE PROCEDURE PDD_GetProductSpecificationAttributeBySegmentId
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

	DECLARE @SpecificationAttributeIdsTable TABLE(
		[IndexId] int IDENTITY (1, 1) NOT NULL,
		SpecificationAttributeId INT
	)

	INSERT INTO @SpecificationAttributeIdsTable
	SELECT 
		DISTINCT
		sao.SpecificationAttributeId
	FROM Product_SpecificationAttribute_Mapping psa
	INNER JOIN PDD_GetProductIdBySegmentId(@ProductSegmentManagerId,@VendorId) ps on ps.ProductId = psa.ProductId
	INNER JOIN  SpecificationAttributeOption sao on sao.Id = psa.SpecificationAttributeOptionId
	
	--total records
	SELECT @TotalRecords = COUNT(1) from @SpecificationAttributeIdsTable

	--return products
	SELECT TOP (@RowsToReturn)
		sa.Id,
		sa.[Name],
		sa.DisplayOrder
	FROM
		SpecificationAttribute sa with (NOLOCK)
		INNER JOIN @SpecificationAttributeIdsTable psi on sa.Id = psi.[SpecificationAttributeId]
	WHERE
		psi.IndexId > @PageLowerBound AND 
		psi.IndexId < @PageUpperBound
	ORDER BY
		psi.IndexId
END
GO
