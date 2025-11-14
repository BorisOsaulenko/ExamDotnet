using System;

public class AddCollectionPopupPartialModel
{
    public AddCollectionPopupPartialModel(
        string? activationButtonId = null,
        string? modalId = null,
        string? formId = null,
        string? triggerButtonText = null
    )
    {
        ActivationButtonId = string.IsNullOrWhiteSpace(activationButtonId)
            ? "add-collection-trigger"
            : activationButtonId!;

        ModalId = string.IsNullOrWhiteSpace(modalId) ? $"{ActivationButtonId}-modal" : modalId!;
        FormId = string.IsNullOrWhiteSpace(formId) ? $"{ModalId}-form" : formId!;
        TriggerButtonText = string.IsNullOrWhiteSpace(triggerButtonText)
            ? "New collection"
            : triggerButtonText!;
    }

    public string ActivationButtonId { get; }
    public string ModalId { get; }
    public string FormId { get; }
    public string TriggerButtonText { get; }
}
