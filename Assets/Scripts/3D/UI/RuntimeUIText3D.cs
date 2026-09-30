using TMPro;
using UnityEngine;

public static class RuntimeUIText3D
{
    public static void Apply(
        TMP_Text text)
    {
        if (text == null)
            return;

        TMP_FontAsset font =
            FindBestFont();

        if (font != null)
        {
            text.font =
                font;

            if (font.material != null)
            {
                text.fontSharedMaterial =
                    font.material;
            }
        }

        text.raycastTarget =
            false;

        text.transform.localScale =
            Vector3.one;
    }

    private static TMP_FontAsset FindBestFont()
    {
        TextMeshProUGUI[] texts =
            Object.FindObjectsByType<
                TextMeshProUGUI
            >(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        TMP_FontAsset anyFont =
            null;

        foreach (
            TextMeshProUGUI candidate
            in texts)
        {
            if (candidate == null ||
                candidate.font == null)
            {
                continue;
            }

            if (anyFont == null)
            {
                anyFont =
                    candidate.font;
            }

            if (ContainsCyrillic(
                    candidate.text))
            {
                return candidate.font;
            }
        }

        if (TMP_Settings.defaultFontAsset != null)
        {
            return
                TMP_Settings.defaultFontAsset;
        }

        return anyFont;
    }

    private static bool ContainsCyrillic(
        string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        for (int i = 0;
            i < value.Length;
            i++)
        {
            char c =
                value[i];

            if ((c >= '\u0400' &&
                 c <= '\u04FF') ||
                (c >= '\u0500' &&
                 c <= '\u052F'))
            {
                return true;
            }
        }

        return false;
    }
}