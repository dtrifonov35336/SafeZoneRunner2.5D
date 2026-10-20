using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TutorialSkipConfirmation3D : MonoBehaviour
{
    private static TutorialSkipConfirmation3D current;

    private Action onConfirm;
    private Action onCancel;
    private bool resolved;

    public static void Show(
        string message,
        Action confirm,
        Action cancel = null)
    {
        if (current != null)
        {
            cancel?.Invoke();
            return;
        }

        GameObject root = new GameObject(
            "TutorialSkipConfirmation",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        TutorialSkipConfirmation3D dialog =
            root.AddComponent<TutorialSkipConfirmation3D>();

        current = dialog;
        dialog.onConfirm = confirm;
        dialog.onCancel = cancel;
        dialog.Build(message);
    }

    public static Button CreateSkipButton(
        Transform parent,
        string objectName,
        string label,
        Vector2 size,
        Vector2 position,
        UnityAction onClick)
    {
        if (parent == null)
            return null;

        GameObject go = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );

        go.transform.SetParent(parent, false);

        RectTransform rect =
            go.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.one;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = go.GetComponent<Image>();
        image.color = new Color32(45, 60, 67, 245);
        image.raycastTarget = true;

        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        button.navigation = new Navigation
        {
            mode = Navigation.Mode.None
        };

        GameObject textObject = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        textObject.transform.SetParent(go.transform, false);

        RectTransform textRect =
            textObject.GetComponent<RectTransform>();

        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(4f, 2f);
        textRect.offsetMax = new Vector2(-4f, -2f);

        TextMeshProUGUI text =
            textObject.GetComponent<TextMeshProUGUI>();

        text.text = label;
        text.fontSize = 20f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = 15f;
        text.fontSizeMax = 20f;
        text.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;

        button.onClick.AddListener(onClick);

        return button;
    }

    private void Build(string message)
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject background = new GameObject(
            "Background",
            typeof(RectTransform),
            typeof(Image)
        );

        background.transform.SetParent(transform, false);

        RectTransform backgroundRect =
            background.GetComponent<RectTransform>();

        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        Image backgroundImage = background.GetComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, 0.85f);
        backgroundImage.raycastTarget = true;

        GameObject card = new GameObject(
            "ConfirmationPanel",
            typeof(RectTransform),
            typeof(Image)
        );

        card.transform.SetParent(transform, false);

        RectTransform cardRect =
            card.GetComponent<RectTransform>();

        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(840f, 410f);
        cardRect.anchoredPosition = Vector2.zero;

        Image cardImage = card.GetComponent<Image>();
        cardImage.color = new Color32(27, 38, 44, 255);
        cardImage.raycastTarget = true;

        CreateText(
            card.transform,
            "Title",
            "ПРОПУСТИТЬ ОБУЧЕНИЕ?",
            new Vector2(0.06f, 0.72f),
            new Vector2(0.94f, 0.93f),
            29f
        );

        CreateText(
            card.transform,
            "Message",
            message,
            new Vector2(0.08f, 0.32f),
            new Vector2(0.92f, 0.72f),
            22f
        );

        CreateButton(
            card.transform,
            "Confirm",
            "ОК",
            new Vector2(0.07f, 0.08f),
            new Vector2(0.46f, 0.26f),
            new Color32(56, 117, 76, 255),
            () => Resolve(true)
        );

        CreateButton(
            card.transform,
            "Cancel",
            "ОТМЕНА",
            new Vector2(0.54f, 0.08f),
            new Vector2(0.93f, 0.26f),
            new Color32(62, 72, 78, 255),
            () => Resolve(false)
        );
    }

    private void CreateText(
        Transform parent,
        string objectName,
        string value,
        Vector2 anchorMin,
        Vector2 anchorMax,
        float fontSize)
    {
        GameObject go = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text =
            go.GetComponent<TextMeshProUGUI>();

        text.text = value;
        text.fontSize = fontSize;
        text.fontSizeMin = 16f;
        text.fontSizeMax = fontSize;
        text.enableAutoSizing = true;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;
    }

    private void CreateButton(
        Transform parent,
        string objectName,
        string label,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color,
        UnityAction action)
    {
        GameObject go = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );

        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = true;

        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        button.navigation = new Navigation
        {
            mode = Navigation.Mode.None
        };

        GameObject textObject = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        textObject.transform.SetParent(go.transform, false);

        RectTransform textRect =
            textObject.GetComponent<RectTransform>();

        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(5f, 3f);
        textRect.offsetMax = new Vector2(-5f, -3f);

        TextMeshProUGUI text =
            textObject.GetComponent<TextMeshProUGUI>();

        text.text = label;
        text.fontSize = 23f;
        text.fontSizeMin = 17f;
        text.fontSizeMax = 23f;
        text.enableAutoSizing = true;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;

        button.onClick.AddListener(action);
    }

    private void Resolve(bool confirm)
    {
        if (resolved)
            return;

        resolved = true;

        Action callback = confirm
            ? onConfirm
            : onCancel;

        onConfirm = null;
        onCancel = null;

        if (current == this)
            current = null;

        Destroy(gameObject);

        callback?.Invoke();
    }

    private void OnDestroy()
    {
        if (current == this)
            current = null;

        if (!resolved)
            onCancel?.Invoke();
    }
}