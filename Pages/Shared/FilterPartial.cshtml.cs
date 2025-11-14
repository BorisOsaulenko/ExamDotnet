using Controllers;
using Controllers.Image;

public class FilterPartialModel
{
    public FilterPartialModel(
        FilterParams? filter = null,
        string? formAction = null,
        string? buttonLabel = null,
        string? applyButtonText = null,
        string? resetLinkText = null,
        string? formMethod = null
    )
    {
        Filter = filter ?? new FilterParams();
        FormAction = string.IsNullOrWhiteSpace(formAction) ? "/Index" : formAction;
        ButtonLabel = string.IsNullOrWhiteSpace(buttonLabel) ? "Filters" : buttonLabel;
        ApplyButtonText = string.IsNullOrWhiteSpace(applyButtonText) ? "Apply filters" : applyButtonText;
        ResetLinkText = string.IsNullOrWhiteSpace(resetLinkText) ? "Reset" : resetLinkText;
        FormMethod = string.IsNullOrWhiteSpace(formMethod) ? "get" : formMethod;
    }

    public FilterParams Filter { get; }
    public string FormAction { get; }
    public string ButtonLabel { get; }
    public string ApplyButtonText { get; }
    public string ResetLinkText { get; }
    public string FormMethod { get; }
}
