using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuWindowVisualPolish3D : MonoBehaviour
{
    private Sprite roundedSprite;
    private int lastContentCount = -1;

    private const float PanelLeft = 0.065f;
    private const float PanelRight = 0.935f;
    private const float PanelBottom = 0.07f;
    private const float PanelTop = 0.93f;

    private readonly Color panelColor =
        new Color32(
            15,
            24,
            31,
            250
        );

    private readonly Color innerColor =
        new Color32(
            8,
            15,
            20,
            235
        );

    private readonly Color goldColor =
        new Color32(
            229,
            188,
            69,
            255
        );

    private readonly Color mutedTextColor =
        new Color32(
            164,
            174,
            171,
            255
        );

    private void Start()
    {
        CreateRoundedSprite();
        Apply();
    }

    private void Update()
    {
        if (roundedSprite == null)
        {
            CreateRoundedSprite();
        }

        Transform content =
            FindContent();

        int contentCount =
            content != null
                ? content.childCount
                : 0;

        if (
            contentCount !=
            lastContentCount
        )
        {
            Apply();
        }
    }

    public void Apply()
    {
        CreateRoundedSprite();

        GameObject panel =
            FindChildObject(
                "Panel"
            );

        if (panel == null)
            return;

        StylePanel(
            panel
        );

        StyleTitle(
            panel
        );

        StyleClose(
            panel
        );

        if (
            gameObject.name.Contains(
                "Achievements"
            )
        )
        {
            StyleAchievements(
                panel
            );
        }
        else if (
            gameObject.name.Contains(
                "DailyLogin"
            )
        )
        {
            StyleDailyLogin(
                panel
            );
        }
        else if (
            gameObject.name.Contains(
                "Settings"
            )
        )
        {
            StyleSettings(
                panel
            );
        }

        Transform content =
            FindContent();

        lastContentCount =
            content != null
                ? content.childCount
                : 0;
    }

    // =========================================================
    // BASE PANEL
    // =========================================================

    private void StylePanel(
        GameObject panel
    )
    {
        RectTransform rect =
            panel.GetComponent<
                RectTransform
            >();

        if (rect != null)
        {
            rect.anchorMin =
                new Vector2(
                    PanelLeft,
                    PanelBottom
                );

            rect.anchorMax =
                new Vector2(
                    PanelRight,
                    PanelTop
                );

            rect.offsetMin =
                Vector2.zero;

            rect.offsetMax =
                Vector2.zero;
        }

        Image image =
            panel.GetComponent<Image>();

        if (image != null)
        {
            image.sprite =
                roundedSprite;

            image.type =
                Image.Type.Sliced;

            image.color =
                panelColor;

            image.raycastTarget =
                true;
        }

        Outline outline =
            GetOrAdd<
                Outline
            >(panel);

        outline.effectColor =
            new Color32(
                51,
                82,
                93,
                210
            );

        outline.effectDistance =
            new Vector2(
                2f,
                -2f
            );

        Shadow shadow =
            GetOrAdd<
                Shadow
            >(panel);

        shadow.effectColor =
            new Color(
                0f,
                0f,
                0f,
                0.55f
            );

        shadow.effectDistance =
            new Vector2(
                0f,
                -8f
            );

        shadow.useGraphicAlpha =
            true;

        CreateAccentLine(
            panel
        );
    }

    // =========================================================
    // TITLE
    // =========================================================

    private void StyleTitle(
        GameObject panel
    )
    {
        Transform titleTransform =
            panel.transform.Find(
                "Title"
            );

        if (titleTransform == null)
            return;

        TMP_Text title =
            titleTransform.GetComponent<
                TMP_Text
            >();

        if (title == null)
            return;

        RectTransform rect =
            title.rectTransform;

        rect.anchorMin =
            new Vector2(
                0.12f,
                0.885f
            );

        rect.anchorMax =
            new Vector2(
                0.78f,
                0.955f
            );

        rect.offsetMin =
            Vector2.zero;

        rect.offsetMax =
            Vector2.zero;

        rect.pivot =
            new Vector2(
                0f,
                0.5f
            );

        title.alignment =
            TextAlignmentOptions.Left;

        title.fontSize =
            28f;

        title.color =
            new Color32(
                240,
                238,
                225,
                255
            );

        title.fontStyle =
            FontStyles.Bold;

        title.textWrappingMode =
            TextWrappingModes.NoWrap;

        title.raycastTarget =
            false;
    }

    private void CreateAccentLine(
        GameObject panel
    )
    {
        Transform existing =
            panel.transform.Find(
                "TitleAccent"
            );

        if (existing != null)
            return;

        GameObject line =
            new GameObject(
                "TitleAccent",
                typeof(RectTransform),
                typeof(Image)
            );

        line.transform.SetParent(
            panel.transform,
            false
        );

        RectTransform rect =
            line.GetComponent<
                RectTransform
            >();

        rect.anchorMin =
            new Vector2(
                0.065f,
                0.89f
            );

        rect.anchorMax =
            new Vector2(
                0.075f,
                0.95f
            );

        rect.offsetMin =
            Vector2.zero;

        rect.offsetMax =
            Vector2.zero;

        Image image =
            line.GetComponent<
                Image
            >();

        image.sprite =
            RuntimeUISprite3D.GetSolidSprite();

        image.color =
            goldColor;

        image.raycastTarget =
            false;
    }

    // =========================================================
    // CLOSE
    // =========================================================

    private void StyleClose(
        GameObject panel
    )
    {
        Transform closeTransform =
            panel.transform.Find(
                "Close"
            );

        if (closeTransform == null)
            return;

        RectTransform rect =
            closeTransform.GetComponent<
                RectTransform
            >();

        if (rect != null)
        {
            rect.anchorMin =
                new Vector2(
                    0.875f,
                    0.885f
                );

            rect.anchorMax =
                new Vector2(
                    0.955f,
                    0.955f
                );

            rect.offsetMin =
                Vector2.zero;

            rect.offsetMax =
                Vector2.zero;
        }

        Image image =
            closeTransform.GetComponent<
                Image
            >();

        if (image != null)
        {
            image.sprite =
                roundedSprite;

            image.type =
                Image.Type.Sliced;

            image.color =
                new Color32(
                    28,
                    42,
                    49,
                    255
                );

            image.raycastTarget =
                true;
        }

        TMP_Text text =
            closeTransform.GetComponentInChildren<
                TMP_Text
            >(
                true
            );

        if (text != null)
        {
            text.fontSize =
                18f;

            text.color =
                new Color32(
                    218,
                    221,
                    215,
                    255
                );

            text.alignment =
                TextAlignmentOptions.Center;

            text.textWrappingMode =
                TextWrappingModes.NoWrap;
        }
    }

    // =========================================================
    // ACHIEVEMENTS
    // =========================================================

    private void StyleAchievements(
        GameObject panel
    )
    {
        StyleScrollView(
            panel
        );

        Transform content =
            FindContent();

        if (content == null)
            return;

        for (
            int i = 0;
            i < content.childCount;
            i++
        )
        {
            Transform card =
                content.GetChild(
                    i
                );

            StyleAchievementCard(
                card.gameObject,
                i
            );
        }
    }

    private void StyleAchievementCard(
        GameObject card,
        int index
    )
    {
        RectTransform rect =
            card.GetComponent<
                RectTransform
            >();

        if (rect != null)
        {
            rect.sizeDelta =
                new Vector2(
                    rect.sizeDelta.x,
                    106f
                );

            rect.anchoredPosition =
                new Vector2(
                    rect.anchoredPosition.x,
                    -8f -
                    index *
                    114f
                );
        }

        Image image =
            card.GetComponent<
                Image
            >();

        if (image != null)
        {
            image.sprite =
                roundedSprite;

            image.type =
                Image.Type.Sliced;

            bool green =
                card.name.Contains(
                    "Completed"
                ) ||
                FindText(
                    card,
                    "ПОЛУЧЕНО"
                ) != null;

            image.color =
                green
                    ? new Color32(
                        25,
                        50,
                        39,
                        255
                    )
                    : new Color32(
                        21,
                        32,
                        40,
                        255
                    );

            image.raycastTarget =
                false;
        }

        CreateCardAccent(
            card,
            index
        );

        TMP_Text title =
            FindTextObject(
                card,
                "Title"
            );

        if (title != null)
        {
            title.fontSize =
                17f;

            title.color =
                new Color32(
                    231,
                    233,
                    226,
                    255
                );

            title.fontStyle =
                FontStyles.Bold;
        }

        TMP_Text description =
            FindTextObject(
                card,
                "Description"
            );

        if (description != null)
        {
            description.fontSize =
                12f;

            description.color =
                mutedTextColor;
        }

        TMP_Text target =
            FindTextObject(
                card,
                "Target"
            );

        if (target != null)
        {
            target.fontSize =
                11f;

            target.color =
                new Color32(
                    120,
                    136,
                    133,
                    255
                );
        }

        TMP_Text reward =
            FindTextObject(
                card,
                "Reward"
            );

        if (reward != null)
        {
            reward.fontSize =
                14f;

            reward.color =
                goldColor;

            reward.fontStyle =
                FontStyles.Bold;
        }

        Transform status =
            card.transform.Find(
                "Status"
            );

        if (status != null)
        {
            Image statusImage =
                status.GetComponent<Image>();

            if (statusImage != null)
            {
                statusImage.sprite =
                    roundedSprite;

                statusImage.type =
                    Image.Type.Sliced;

                statusImage.color =
                    new Color32(
                        32,
                        48,
                        55,
                        255
                    );
            }

            TMP_Text statusText =
                status.GetComponentInChildren<
                    TMP_Text
                >(
                    true
                );

            if (statusText != null)
            {
                statusText.fontSize =
                    11f;

                statusText.fontStyle =
                    FontStyles.Bold;
            }
        }
    }

    // =========================================================
    // DAILY LOGIN
    // =========================================================

    private void StyleDailyLogin(
        GameObject panel
    )
    {
        StyleScrollView(
            panel
        );

        Transform content =
            FindContent();

        if (content == null)
            return;

        for (
            int i = 0;
            i < content.childCount;
            i++
        )
        {
            StyleDailyCard(
                content
                    .GetChild(i)
                    .gameObject
            );
        }
    }

    private void StyleDailyCard(
        GameObject card
    )
    {
        RectTransform rect =
            card.GetComponent<
                RectTransform
            >();

        if (rect != null)
        {
            rect.sizeDelta =
                new Vector2(
                    rect.sizeDelta.x,
                    126f
                );
        }

        Image image =
            card.GetComponent<
                Image
            >();

        if (image != null)
        {
            image.sprite =
                roundedSprite;

            image.type =
                Image.Type.Sliced;

            image.color =
                new Color32(
                    22,
                    34,
                    41,
                    255
                );
        }

        TMP_Text[] texts =
            card.GetComponentsInChildren<
                TMP_Text
            >(
                true
            );

        foreach (
            TMP_Text text
            in texts
        )
        {
            text.raycastTarget =
                false;

            text.textWrappingMode =
                TextWrappingModes.NoWrap;
        }

        TMP_Text day =
            FindTextObject(
                card,
                "Day"
            );

        if (day != null)
        {
            day.fontSize =
                15f;

            day.fontStyle =
                FontStyles.Bold;

            day.color =
                goldColor;
        }

        TMP_Text reward =
            FindTextObject(
                card,
                "Reward"
            );

        if (reward != null)
        {
            reward.fontSize =
                18f;

            reward.fontStyle =
                FontStyles.Bold;

            reward.color =
                new Color32(
                    230,
                    224,
                    197,
                    255
                );
        }

        Transform state =
            card.transform.Find(
                "StateButton"
            );

        if (state != null)
        {
            Image stateImage =
                state.GetComponent<
                    Image
                >();

            if (stateImage != null)
            {
                stateImage.sprite =
                    roundedSprite;

                stateImage.type =
                    Image.Type.Sliced;
            }

            TMP_Text stateText =
                state.GetComponentInChildren<
                    TMP_Text
                >(
                    true
                );

            if (stateText != null)
            {
                stateText.fontSize =
                    11f;

                stateText.fontStyle =
                    FontStyles.Bold;
            }
        }
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    private void StyleSettings(
        GameObject panel
    )
    {
        CreateSettingsRow(
            panel,
            "SettingsVolumeRow",
            "VolumeLabel",
            "VolumeValue",
            0.64f,
            0.75f
        );

        CreateSettingsRow(
            panel,
            "SettingsVibrationRow",
            "VibrationLabel",
            null,
            0.45f,
            0.56f
        );

        CreateSettingsRow(
            panel,
            "SettingsFPSRow",
            null,
            null,
            0.24f,
            0.36f
        );

        StyleSettingsHeader(
            panel,
            "SoundHeader"
        );

        StyleSettingsHeader(
            panel,
            "ControlHeader"
        );

        StyleSettingsHeader(
            panel,
            "PerformanceHeader"
        );

        StyleSettingsText(
            panel,
            "VolumeLabel"
        );

        StyleSettingsText(
            panel,
            "VibrationLabel"
        );

        StyleFPSButtons(
            panel
        );

        TMP_Text footer =
            FindTextObject(
                panel,
                "Footer"
            );

        if (footer != null)
        {
            footer.fontSize =
                11f;

            footer.color =
                new Color32(
                    106,
                    120,
                    119,
                    255
                );
        }
    }

    private void StyleSettingsHeader(
        GameObject panel,
        string objectName
    )
    {
        TMP_Text text =
            FindTextObject(
                panel,
                objectName
            );

        if (text == null)
            return;

        text.fontSize =
            14f;

        text.fontStyle =
            FontStyles.Bold;

        text.color =
            goldColor;

        text.alignment =
            TextAlignmentOptions.Left;
    }

    private void StyleSettingsText(
        GameObject panel,
        string objectName
    )
    {
        TMP_Text text =
            FindTextObject(
                panel,
                objectName
            );

        if (text == null)
            return;

        text.fontSize =
            15f;

        text.color =
            new Color32(
                220,
                224,
                218,
                255
            );

        text.fontStyle =
            FontStyles.Normal;

        text.alignment =
            TextAlignmentOptions.Left;
    }

    private void StyleFPSButtons(
        GameObject panel
    )
    {
        StyleButton(
            panel,
            "FPS30"
        );

        StyleButton(
            panel,
            "FPS60"
        );
    }

    private void StyleButton(
        GameObject panel,
        string objectName
    )
    {
        Transform button =
            FindDeepChild(
                panel.transform,
                objectName
            );

        if (button == null)
            return;

        Image image =
            button.GetComponent<
                Image
            >();

        if (image != null)
        {
            image.sprite =
                roundedSprite;

            image.type =
                Image.Type.Sliced;

            image.color =
                new Color32(
                    28,
                    42,
                    49,
                    255
                );
        }

        TMP_Text text =
            button.GetComponentInChildren<
                TMP_Text
            >(
                true
            );

        if (text != null)
        {
            text.fontSize =
                14f;

            text.fontStyle =
                FontStyles.Bold;

            text.color =
                new Color32(
                    222,
                    225,
                    219,
                    255
                );
        }
    }

    private void CreateSettingsRow(
        GameObject panel,
        string rowName,
        string labelName,
        string valueName,
        float minY,
        float maxY
    )
    {
        Transform existing =
            panel.transform.Find(
                rowName
            );

        if (existing != null)
            return;

        GameObject row =
            new GameObject(
                rowName,
                typeof(RectTransform),
                typeof(Image)
            );

        row.transform.SetParent(
            panel.transform,
            false
        );

        RectTransform rect =
            row.GetComponent<
                RectTransform
            >();

        rect.anchorMin =
            new Vector2(
                0.07f,
                minY
            );

        rect.anchorMax =
            new Vector2(
                0.93f,
                maxY
            );

        rect.offsetMin =
            Vector2.zero;

        rect.offsetMax =
            Vector2.zero;

        Image image =
            row.GetComponent<
                Image
            >();

        image.sprite =
            roundedSprite;

        image.type =
            Image.Type.Sliced;

        image.color =
            innerColor;

        image.raycastTarget =
            false;

        row.transform.SetAsFirstSibling();
    }

    // =========================================================
    // SCROLL
    // =========================================================

    private void StyleScrollView(
        GameObject panel
    )
    {
        Transform scrollView =
            panel.transform.Find(
                "ScrollView"
            );

        if (scrollView == null)
            return;

        Image image =
            scrollView.GetComponent<
                Image
            >();

        if (image != null)
        {
            image.sprite =
                roundedSprite;

            image.type =
                Image.Type.Sliced;

            image.color =
                new Color32(
                    7,
                    14,
                    19,
                    230
                );

            image.raycastTarget =
                false;
        }
    }

    // =========================================================
    // ACCENT
    // =========================================================

    private void CreateCardAccent(
        GameObject card,
        int index
    )
    {
        Transform existing =
            card.transform.Find(
                "Accent"
            );

        if (existing != null)
            return;

        GameObject accent =
            new GameObject(
                "Accent",
                typeof(RectTransform),
                typeof(Image)
            );

        accent.transform.SetParent(
            card.transform,
            false
        );

        RectTransform rect =
            accent.GetComponent<
                RectTransform
            >();

        rect.anchorMin =
            new Vector2(
                0f,
                0.17f
            );

        rect.anchorMax =
            new Vector2(
                0f,
                0.83f
            );

        rect.pivot =
            new Vector2(
                0f,
                0.5f
            );

        rect.anchoredPosition =
            new Vector2(
                5f,
                0f
            );

        rect.sizeDelta =
            new Vector2(
                3f,
                0f
            );

        Image image =
            accent.GetComponent<
                Image
            >();

        image.sprite =
            RuntimeUISprite3D.GetSolidSprite();

        image.color =
            index % 2 == 0
                ? goldColor
                : new Color32(
                    75,
                    122,
                    130,
                    255
                );

        image.raycastTarget =
            false;
    }

    // =========================================================
    // FIND HELPERS
    // =========================================================

    private Transform FindContent()
    {
        Transform viewport =
            FindDeepChild(
                transform,
                "Viewport"
            );

        if (viewport == null)
            return null;

        return
            viewport.Find(
                "Content"
            );
    }

    private GameObject FindChildObject(
        string name
    )
    {
        Transform child =
            FindDeepChild(
                transform,
                name
            );

        return
            child != null
                ? child.gameObject
                : null;
    }

    private TMP_Text FindTextObject(
        GameObject parent,
        string name
    )
    {
        Transform child =
            FindDeepChild(
                parent.transform,
                name
            );

        if (child == null)
            return null;

        return
            child.GetComponent<TMP_Text>();
    }

    private TMP_Text FindText(
        GameObject parent,
        string value
    )
    {
        TMP_Text[] texts =
            parent.GetComponentsInChildren<
                TMP_Text
            >(
                true
            );

        foreach (
            TMP_Text text
            in texts
        )
        {
            if (
                text.text.Contains(
                    value
                )
            )
            {
                return text;
            }
        }

        return null;
    }

    private Transform FindDeepChild(
        Transform parent,
        string childName
    )
    {
        if (
            parent.name ==
            childName
        )
        {
            return parent;
        }

        foreach (
            Transform child
            in parent
        )
        {
            Transform result =
                FindDeepChild(
                    child,
                    childName
                );

            if (result != null)
                return result;
        }

        return null;
    }

    private T GetOrAdd<T>(
        GameObject go
    )
        where T : Component
    {
        T component =
            go.GetComponent<T>();

        if (component == null)
        {
            component =
                go.AddComponent<T>();
        }

        return component;
    }

    // =========================================================
    // ROUNDED SPRITE
    // =========================================================

    private void CreateRoundedSprite()
    {
        if (roundedSprite != null)
            return;

        const int size = 64;
        const float radius = 12f;

        Texture2D texture =
            new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false
            );

        texture.name =
            "RuntimeUIRoundedTexture";

        texture.wrapMode =
            TextureWrapMode.Clamp;

        texture.filterMode =
            FilterMode.Bilinear;

        Color[] pixels =
            new Color[
                size *
                size
            ];

        for (
            int y = 0;
            y < size;
            y++
        )
        {
            for (
                int x = 0;
                x < size;
                x++
            )
            {
                float dx =
                    Mathf.Max(
                        radius -
                        x,
                        0f
                    );

                float dxRight =
                    Mathf.Max(
                        x -
                        (size - 1 - radius),
                        0f
                    );

                float dy =
                    Mathf.Max(
                        radius -
                        y,
                        0f
                    );

                float dyTop =
                    Mathf.Max(
                        y -
                        (size - 1 - radius),
                        0f
                    );

                float cornerDistance =
                    Mathf.Sqrt(
                        dx * dx +
                        dy * dy
                    );

                float cornerDistanceRight =
                    Mathf.Sqrt(
                        dxRight * dxRight +
                        dy * dy
                    );

                float cornerDistanceTop =
                    Mathf.Sqrt(
                        dx * dx +
                        dyTop * dyTop
                    );

                float cornerDistanceBoth =
                    Mathf.Sqrt(
                        dxRight * dxRight +
                        dyTop * dyTop
                    );

                float distance =
                    Mathf.Max(
                        cornerDistance,
                        cornerDistanceRight
                    );

                distance =
                    Mathf.Max(
                        distance,
                        cornerDistanceTop
                    );

                distance =
                    Mathf.Max(
                        distance,
                        cornerDistanceBoth
                    );

                float alpha =
                    distance >
                    radius
                        ? 0f
                        : 1f;

                pixels[
                    y * size +
                    x
                ] =
                    new Color(
                        1f,
                        1f,
                        1f,
                        alpha
                    );
            }
        }

        texture.SetPixels(
            pixels
        );

        texture.Apply();

        roundedSprite =
            Sprite.Create(
                texture,
                new Rect(
                    0f,
                    0f,
                    size,
                    size
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                100f,
                0u,
                SpriteMeshType.FullRect,
                new Vector4(
                    14f,
                    14f,
                    14f,
                    14f
                )
            );

        roundedSprite.name =
            "RuntimeUIRoundedSprite";
    }
}