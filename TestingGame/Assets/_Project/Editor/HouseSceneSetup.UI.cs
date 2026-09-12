#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class HouseSceneSetup
{
    public class UIRefs
    {
        public Canvas canvas;
        public Image crosshair;
        public Text promptText;
        public Text toolLabelText;
        public GameObject batteryContainer;
        public Image batteryFill;
        public Text objectivesText;
        public CanvasGroup messageGroup;
        public Text messageText;
        public GameObject endScreenPanel;
        public Text endTitle;
        public Text endSubtitle;
    }

    private static Font cachedFont;

    private static Font GetDefaultFont()
    {
        if (cachedFont != null) return cachedFont;
        cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (cachedFont == null) cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return cachedFont;
    }

    private static RectTransform NewUIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private static RectTransform SetRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return rt;
    }

    private static Text AddText(Transform parent, string name, string content, int fontSize, TextAnchor align, Color color)
    {
        var rt = NewUIObject(name, parent);
        var txt = rt.gameObject.AddComponent<Text>();
        txt.font = GetDefaultFont();
        txt.fontSize = fontSize;
        txt.alignment = align;
        txt.color = color;
        txt.text = content;
        return txt;
    }

    private static Image AddImage(Transform parent, string name, Color color)
    {
        var rt = NewUIObject(name, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        return img;
    }

    private static UIRefs BuildUI()
    {
        var refs = new UIRefs();

        var canvasGo = new GameObject("HUD_Canvas", typeof(RectTransform));
        refs.canvas = canvasGo.AddComponent<Canvas>();
        refs.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasGo.AddComponent<GraphicRaycaster>();

        refs.crosshair = AddImage(canvasGo.transform, "Crosshair", Color.white);
        SetRect(refs.crosshair.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(6f, 6f));

        refs.promptText = AddText(canvasGo.transform, "InteractionPrompt", "", 26, TextAnchor.MiddleCenter, Color.white);
        SetRect(refs.promptText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -50f), new Vector2(700f, 50f));
        refs.promptText.gameObject.SetActive(false);

        refs.toolLabelText = AddText(canvasGo.transform, "ToolLabel", "Aspiradora", 24, TextAnchor.LowerRight, Color.white);
        SetRect(refs.toolLabelText.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-140f, 60f), new Vector2(260f, 40f));

        refs.batteryContainer = NewUIObject("BatteryContainer", canvasGo.transform).gameObject;
        SetRect(refs.batteryContainer.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-140f, 30f), new Vector2(200f, 16f));
        var batteryBg = AddImage(refs.batteryContainer.transform, "BatteryBg", new Color(1f, 1f, 1f, 0.2f));
        SetRect(batteryBg.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        refs.batteryFill = AddImage(refs.batteryContainer.transform, "BatteryFill", Color.yellow);
        refs.batteryFill.type = Image.Type.Filled;
        refs.batteryFill.fillMethod = Image.FillMethod.Horizontal;
        refs.batteryFill.fillAmount = 1f;
        SetRect(refs.batteryFill.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        refs.objectivesText = AddText(canvasGo.transform, "ObjectivesText", "OBJETIVOS", 22, TextAnchor.UpperLeft, Color.white);
        SetRect(refs.objectivesText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(180f, -120f), new Vector2(420f, 300f));

        var msgRect = NewUIObject("MessagePanel", canvasGo.transform);
        refs.messageGroup = msgRect.gameObject.AddComponent<CanvasGroup>();
        SetRect(msgRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(1000f, 90f));
        refs.messageText = AddText(msgRect, "MessageText", "", 30, TextAnchor.MiddleCenter, Color.white);
        SetRect(refs.messageText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        refs.messageGroup.alpha = 0f;

        var endRect = NewUIObject("EndScreenPanel", canvasGo.transform);
        refs.endScreenPanel = endRect.gameObject;
        var endBg = endRect.gameObject.AddComponent<Image>();
        endBg.color = new Color(0f, 0f, 0f, 0.88f);
        SetRect(endRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        refs.endTitle = AddText(endRect, "EndTitle", "", 64, TextAnchor.MiddleCenter, Color.white);
        SetRect(refs.endTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(1200f, 100f));
        refs.endSubtitle = AddText(endRect, "EndSubtitle", "", 28, TextAnchor.MiddleCenter, new Color(0.85f, 0.85f, 0.85f));
        SetRect(refs.endSubtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(900f, 150f));
        refs.endScreenPanel.SetActive(false);

        return refs;
    }
}
#endif
