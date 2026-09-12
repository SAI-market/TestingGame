using UnityEngine;
using UnityEngine.UI;

/// <summary>Ties the crosshair, interaction prompt, tool label and flashlight battery to their sources.</summary>
public class HUDController : MonoBehaviour
{
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private ToolController toolController;

    [SerializeField] private Image crosshairImage;
    [SerializeField] private Color crosshairNormalColor = Color.white;
    [SerializeField] private Color crosshairHighlightColor = Color.yellow;

    [SerializeField] private Text promptText;
    [SerializeField] private Text toolLabelText;

    [SerializeField] private GameObject batteryContainer;
    [SerializeField] private Image batteryFillImage;

    private void Update()
    {
        UpdateInteractionPrompt();
        UpdateToolLabel();
    }

    private void UpdateInteractionPrompt()
    {
        if (interactor == null) return;
        bool hasTarget = interactor.CurrentInteractable != null;

        if (crosshairImage != null) crosshairImage.color = hasTarget ? crosshairHighlightColor : crosshairNormalColor;

        if (promptText != null)
        {
            promptText.gameObject.SetActive(hasTarget);
            if (hasTarget) promptText.text = interactor.CurrentInteractable.InteractPrompt;
        }
    }

    private void UpdateToolLabel()
    {
        if (toolController == null) return;

        if (toolLabelText != null) toolLabelText.text = ToolName(toolController.CurrentTool);

        bool showBattery = toolController.CurrentTool == ToolType.Flashlight && toolController.Flashlight != null;
        if (batteryContainer != null) batteryContainer.SetActive(showBattery);
        if (showBattery && batteryFillImage != null) batteryFillImage.fillAmount = toolController.Flashlight.BatteryFraction;
    }

    private static string ToolName(ToolType tool)
    {
        switch (tool)
        {
            case ToolType.Vacuum: return "Aspiradora";
            case ToolType.Broom: return "Escoba";
            case ToolType.Cloth: return "Trapo";
            case ToolType.Flashlight: return "Linterna";
            default: return "";
        }
    }
}
