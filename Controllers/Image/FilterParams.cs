namespace Controllers.Image;

public enum SortBy
{
    Date,
    Name,
    Views,
    Downloads,
    Shares,
    Likes,
}

public enum SortOrder
{
    Ascending,
    Descending,
}

public class FilterParams
{
    public string? SearchTerm { get; set; }
    private DateTime? _fromDate;
    public DateTime? FromDate
    {
        get => _fromDate;
        set =>
            _fromDate = value == null ? null : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
    }

    private DateTime? _toDate;
    public DateTime? ToDate
    {
        get => _toDate;
        set => _toDate = value == null ? null : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
    }
    public SortBy? SortBy { get; set; }
    public SortOrder? SortOrder { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public string? AuthorId { get; set; }
}
