using System;

public class AddImagePopupPartialModel
{
    public AddImagePopupPartialModel(
        string? activationButtonId = null,
        string? modalId = null,
        string? formId = null,
        string? triggerButtonText = null
    )
    {
        ActivationButtonId = string.IsNullOrWhiteSpace(activationButtonId)
            ? "add-image-trigger"
            : activationButtonId!;

        ModalId = string.IsNullOrWhiteSpace(modalId) ? $"{ActivationButtonId}-modal" : modalId!;
        FormId = string.IsNullOrWhiteSpace(formId) ? $"{ModalId}-form" : formId!;
        TriggerButtonText = string.IsNullOrWhiteSpace(triggerButtonText)
            ? "Upload image"
            : triggerButtonText!;
    }

    public string ActivationButtonId { get; }
    public string ModalId { get; }
    public string FormId { get; }
    public string TriggerButtonText { get; }
}
