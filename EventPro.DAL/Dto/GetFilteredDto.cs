namespace EventPro.DAL.Dto
{
    public record GetFilteredDto(
        int Skip,
        int PageSize,
        string SearchValue,
        string SortColumnIndex,
        string SortColumn,
        string SortColumnDirection
    );
}
