using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// Play Mode проверки окна «КОНФИДЕНЦИАЛЬНОСТЬ» в настройках:
/// отображение сохранённых значений, независимые переключатели,
/// индикатор несохранённых изменений, отмена по крестику,
/// уведомление о составе решения и ссылки на документы согласий.
///
/// Проверяется реальный UI-путь EventSystem (рейкаст + клик), как
/// при нажатии пальцем на Android. Нативная реклама и сеть здесь
/// не участвуют.
/// </summary>
public class PrivacySettingsUiPlayModeTests
{
    private const BindingFlags StaticFlags =
        BindingFlags.Static |
        BindingFlags.Public |
        BindingFlags.NonPublic;

    private static readonly string[] IntKeys =
    {
        "SafeZone_AnalyticsConsent",
        "SafeZone_AdsConsent",
        "SafeZone_PrivacyConsentVersion",
        "SafeZone_PrivacyConsent"
    };

    private static readonly string[] StringKeys =
    {
        "SafeZone_AnalyticsConsentDate",
        "SafeZone_AdsConsentDate",
        "SafeZone_PrivacyConsentDate"
    };

    private Type policyType;
    private Type consentType;
    private Type toggleType;

    private MethodInfo consentReset;
    private MethodInfo consentInitialize;
    private PropertyInfo toggleIsOn;

    private GameObject eventSystemGo;

    private readonly Dictionary<string, int> savedInts =
        new Dictionary<string, int>();

    private readonly Dictionary<string, string> savedStrings =
        new Dictionary<string, string>();

    private readonly HashSet<string> absentInts =
        new HashSet<string>();

    private readonly HashSet<string> absentStrings =
        new HashSet<string>();

    [SetUp]
    public void SetUp()
    {
        policyType = Type.GetType(
            "PrivacyPolicy3D, Assembly-CSharp");

        consentType = Type.GetType(
            "PrivacyConsent3D, Assembly-CSharp");

        toggleType = Type.GetType(
            "PrivacyToggle3D, Assembly-CSharp");

        Assert.IsNotNull(policyType, "PrivacyPolicy3D не найден.");
        Assert.IsNotNull(consentType, "PrivacyConsent3D не найден.");
        Assert.IsNotNull(toggleType, "PrivacyToggle3D не найден.");

        consentReset = consentType.GetMethod(
            "ResetStaticState", StaticFlags);

        consentInitialize = consentType.GetMethod(
            "InitializeConsentState", StaticFlags);

        toggleIsOn = toggleType.GetProperty("IsOn");

        Assert.IsNotNull(consentReset);
        Assert.IsNotNull(consentInitialize);
        Assert.IsNotNull(toggleIsOn);

        SavePrefs();
        DestroyOverlays();

        if (EventSystem.current == null)
        {
            eventSystemGo = new GameObject("EventSystem_Test");
            eventSystemGo.AddComponent<EventSystem>();
        }
    }

    [TearDown]
    public void TearDown()
    {
        DestroyOverlays();

        if (eventSystemGo != null)
            UnityEngine.Object.DestroyImmediate(eventSystemGo);

        eventSystemGo = null;

        RestorePrefs();
        ReloadConsent();
    }

    // =========================================================
    // 1. СОХРАНЁННЫЕ ЗНАЧЕНИЯ
    // =========================================================

    [UnityTest]
    public IEnumerator Settings_ShowSavedValues()
    {
        yield return OpenSettings(true, false);

        GameObject overlay = Overlay();
        Assert.IsNotNull(overlay);

        Assert.IsTrue(
            ToggleOn(overlay, "Toggle_Разрешить аналитику"));
        Assert.IsFalse(
            ToggleOn(overlay, "Toggle_Разрешить рекламу"));

        bool hasActive = false;
        bool hasAbsent = false;

        foreach (TextMeshProUGUI text
            in overlay.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.gameObject.name != "ConsentStatus")
                continue;

            if (text.text == "Согласие действует")
                hasActive = true;

            if (text.text == "Согласие отсутствует или отозвано")
                hasAbsent = true;
        }

        Assert.IsTrue(
            hasActive,
            "Для включённого согласия должен быть статус «действует».");
        Assert.IsTrue(
            hasAbsent,
            "Для выключенного согласия должен быть статус «отозвано».");
    }

    // =========================================================
    // 2. ОТДЕЛЬНОЕ ОТКЛЮЧЕНИЕ АНАЛИТИКИ
    // =========================================================

    [UnityTest]
    public IEnumerator AnalyticsToggle_Independent_AndSaves()
    {
        yield return OpenSettings(true, true);

        GameObject overlay = Overlay();
        Component toggle = FindToggle(
            overlay,
            "Toggle_Разрешить аналитику");

        Assert.IsNotNull(toggle);

        Click(toggle.gameObject);

        Assert.IsFalse(ToggleIsOn(toggle));

        TextMeshProUGUI pending = FindText(
            overlay.transform, "PendingStatus");

        Assert.IsNotNull(pending);
        StringAssert.Contains(
            "ожидают сохранения", pending.text);

        TextMeshProUGUI status = StatusAfter(toggle.transform);

        Assert.IsNotNull(status);
        Assert.AreEqual(
            "Согласие отсутствует или отозвано",
            status.text);

        Click(FindByName(
            overlay.transform,
            "СОХРАНИТЬ ИЗМЕНЕНИЯ").gameObject);

        yield return null;
        yield return null;

        Assert.AreEqual(
            0,
            PlayerPrefs.GetInt("SafeZone_AnalyticsConsent", -1));

        Assert.AreEqual(
            1,
            PlayerPrefs.GetInt("SafeZone_AdsConsent", -1),
            "Реклама должна остаться включённой.");
    }

    // =========================================================
    // 3. ОТДЕЛЬНОЕ ОТКЛЮЧЕНИЕ РЕКЛАМЫ
    // =========================================================

    [UnityTest]
    public IEnumerator AdsToggle_Independent_AndSaves()
    {
        yield return OpenSettings(true, true);

        GameObject overlay = Overlay();
        Component toggle = FindToggle(
            overlay,
            "Toggle_Разрешить рекламу");

        Assert.IsNotNull(toggle);

        Click(toggle.gameObject);

        Assert.IsFalse(ToggleIsOn(toggle));

        Click(FindByName(
            overlay.transform,
            "СОХРАНИТЬ ИЗМЕНЕНИЯ").gameObject);

        yield return null;
        yield return null;

        Assert.AreEqual(
            1,
            PlayerPrefs.GetInt("SafeZone_AnalyticsConsent", -1),
            "Аналитика должна остаться включённой.");

        Assert.AreEqual(
            0,
            PlayerPrefs.GetInt("SafeZone_AdsConsent", -1));
    }

    // =========================================================
    // 4. ОТКЛЮЧЕНИЕ ОБОИХ
    // =========================================================

    [UnityTest]
    public IEnumerator BothTogglesOff_DisableBoth()
    {
        yield return OpenSettings(true, true);

        GameObject overlay = Overlay();

        Click(FindToggle(
            overlay,
            "Toggle_Разрешить аналитику").gameObject);

        Click(FindToggle(
            overlay,
            "Toggle_Разрешить рекламу").gameObject);

        Click(FindByName(
            overlay.transform,
            "СОХРАНИТЬ ИЗМЕНЕНИЯ").gameObject);

        yield return null;
        yield return null;

        Assert.AreEqual(
            0,
            PlayerPrefs.GetInt("SafeZone_AnalyticsConsent", -1));

        Assert.AreEqual(
            0,
            PlayerPrefs.GetInt("SafeZone_AdsConsent", -1));
    }

    // =========================================================
    // 5. ЗАКРЫТИЕ БЕЗ СОХРАНЕНИЯ
    // =========================================================

    [UnityTest]
    public IEnumerator CloseWithoutSave_DiscardsDraft()
    {
        yield return OpenSettings(true, true);

        GameObject overlay = Overlay();

        Click(FindToggle(
            overlay,
            "Toggle_Разрешить аналитику").gameObject);

        Click(FindByName(
            overlay.transform,
            "ClosePrivacy").gameObject);

        yield return null;
        yield return null;

        Assert.IsNull(
            Overlay(),
            "Окно должно закрыться без сохранения.");

        Assert.AreEqual(
            1,
            PlayerPrefs.GetInt("SafeZone_AnalyticsConsent", -1));

        Assert.AreEqual(
            1,
            PlayerPrefs.GetInt("SafeZone_AdsConsent", -1));
    }

    // =========================================================
    // 6. ССЫЛКИ НА ДОКУМЕНТЫ
    // =========================================================

    [UnityTest]
    public IEnumerator Links_OpenMatchingConsentDocuments()
    {
        yield return OpenSettings(true, true);

        GameObject overlay = Overlay();
        ScrollToBottom(overlay);

        yield return null;
        Canvas.ForceUpdateCanvases();

        Click(FindByName(
            overlay.transform,
            "Link_Прочитать согласие на обработку данных для аналитики")
            .gameObject);

        yield return null;
        Canvas.ForceUpdateCanvases();

        GameObject document = GameObject.Find("PrivacyDocument");

        Assert.IsNotNull(document);
        Assert.IsTrue(
            HasText(document, "СОГЛАСИЕ НА АНАЛИТИКУ"),
            "Должен открыться документ согласия на аналитику.");
        Assert.IsTrue(
            HasText(document, "AppMetrica"),
            "Документ аналитики должен упоминать AppMetrica.");

        Click(FindByName(
            document.transform,
            "ЗАКРЫТЬ").gameObject);

        yield return null;
        yield return null;

        overlay = Overlay();
        Assert.IsNotNull(overlay);

        Click(FindByName(
            overlay.transform,
            "Link_Прочитать согласие на обработку данных для рекламы")
            .gameObject);

        yield return null;
        Canvas.ForceUpdateCanvases();

        document = GameObject.Find("PrivacyDocument");

        Assert.IsNotNull(document);
        Assert.IsTrue(
            HasText(document, "СОГЛАСИЕ НА РЕКЛАМУ"),
            "Должен открыться документ согласия на рекламу.");
        Assert.IsTrue(
            HasText(document, "Yandex Mobile Ads"),
            "Документ рекламы должен упоминать Yandex Mobile Ads.");
    }

    [UnityTest]
    public IEnumerator Settings_HasSinglePolicyReference()
    {
        yield return OpenSettings(true, true);

        GameObject overlay = Overlay();
        Assert.IsNotNull(overlay);

        int policyReferences = 0;

        foreach (TextMeshProUGUI text
            in overlay.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.text != null &&
                text.text.IndexOf(
                    "политик",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                policyReferences++;
            }
        }

        Assert.AreEqual(
            1,
            policyReferences,
            "В окне настроек должна остаться одна ссылка на политику.");

        Assert.IsNotNull(
            FindByName(
                overlay.transform,
                "ПОЛИТИКА КОНФИДЕНЦИАЛЬНОСТИ"),
            "Нижняя кнопка политики должна существовать.");
    }

    // =========================================================
    // ВСПОМОГАТЕЛЬНОЕ
    // =========================================================

    private IEnumerator OpenSettings(
        bool analytics,
        bool ads)
    {
        SetConsentPrefs(analytics, ads);
        DestroyOverlays();

        policyType
            .GetMethod(
                "ShowConsentFromSettings",
                StaticFlags)
            .Invoke(null, null);

        yield return null;
        Canvas.ForceUpdateCanvases();
        yield return null;
    }

    private static GameObject Overlay()
    {
        return GameObject.Find("PrivacySettings");
    }

    private static void ScrollToBottom(GameObject overlay)
    {
        ScrollRect scroll =
            overlay.GetComponentInChildren<ScrollRect>(true);

        if (scroll != null)
            scroll.verticalNormalizedPosition = 0f;
    }

    private Component FindToggle(
        GameObject overlay,
        string name)
    {
        foreach (Component component
            in overlay.GetComponentsInChildren(toggleType, true))
        {
            if (component.gameObject.name == name)
                return component;
        }

        return null;
    }

    private bool ToggleOn(
        GameObject overlay,
        string name)
    {
        Component toggle = FindToggle(overlay, name);

        Assert.IsNotNull(toggle, "Не найден переключатель " + name);

        return ToggleIsOn(toggle);
    }

    private bool ToggleIsOn(Component toggle)
    {
        return (bool)toggleIsOn.GetValue(toggle);
    }

    private static TextMeshProUGUI StatusAfter(
        Transform toggle)
    {
        Transform parent = toggle.parent;
        int index = toggle.GetSiblingIndex() + 1;

        if (index >= parent.childCount)
            return null;

        return parent.GetChild(index)
            .GetComponent<TextMeshProUGUI>();
    }

    private static TextMeshProUGUI FindText(
        Transform root,
        string objectName)
    {
        foreach (TextMeshProUGUI text
            in root.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.gameObject.name == objectName)
                return text;
        }

        return null;
    }

    private static Transform FindByName(
        Transform root,
        string name)
    {
        foreach (Transform child
            in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                return child;
        }

        return null;
    }

    private static bool HasText(
        GameObject root,
        string value)
    {
        foreach (TextMeshProUGUI text
            in root.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.text != null && text.text.Contains(value))
                return true;
        }

        return false;
    }

    private static void Click(GameObject target)
    {
        RectTransform rect = target.transform as RectTransform;

        Assert.IsNotNull(rect, "У цели клика нет RectTransform.");

        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        Vector2 probe = (new Vector2(
            corners[0].x,
            corners[0].y) + new Vector2(
            corners[2].x,
            corners[2].y)) * 0.5f;

        EventSystem system = EventSystem.current;

        Assert.IsNotNull(system, "EventSystem не найден.");

        PointerEventData eventData =
            new PointerEventData(system);

        eventData.position = probe;
        eventData.button = PointerEventData.InputButton.Left;
        eventData.clickCount = 1;

        List<RaycastResult> results =
            new List<RaycastResult>();

        system.RaycastAll(eventData, results);

        Assert.Greater(
            results.Count,
            0,
            "Под точкой клика нет UI: " + target.name);

        GameObject handler =
            ExecuteEvents.GetEventHandler<IPointerClickHandler>(
                results[0].gameObject);

        Assert.IsNotNull(
            handler,
            "Нет обработчика клика под " + target.name);

        ExecuteEvents.Execute(
            handler,
            eventData,
            ExecuteEvents.pointerClickHandler);
    }

    private static void DestroyOverlays()
    {
        foreach (string name in new[]
        {
            "PrivacyConsent",
            "PrivacySettings",
            "PrivacyDocument"
        })
        {
            GameObject overlay = GameObject.Find(name);

            if (overlay != null)
                UnityEngine.Object.DestroyImmediate(overlay);
        }
    }

    private void SetConsentPrefs(
        bool analytics,
        bool ads)
    {
        PlayerPrefs.SetInt(
            "SafeZone_AnalyticsConsent",
            analytics ? 1 : 0);

        PlayerPrefs.SetInt(
            "SafeZone_AdsConsent",
            ads ? 1 : 0);

        PlayerPrefs.SetInt(
            "SafeZone_PrivacyConsentVersion",
            5);

        PlayerPrefs.Save();

        ReloadConsent();
    }

    private void ReloadConsent()
    {
        consentReset.Invoke(null, null);
        consentInitialize.Invoke(null, null);
    }

    private void SavePrefs()
    {
        savedInts.Clear();
        savedStrings.Clear();
        absentInts.Clear();
        absentStrings.Clear();

        foreach (string key in IntKeys)
        {
            if (PlayerPrefs.HasKey(key))
                savedInts[key] = PlayerPrefs.GetInt(key, 0);
            else
                absentInts.Add(key);
        }

        foreach (string key in StringKeys)
        {
            if (PlayerPrefs.HasKey(key))
                savedStrings[key] = PlayerPrefs.GetString(
                    key, string.Empty);
            else
                absentStrings.Add(key);
        }
    }

    private void RestorePrefs()
    {
        foreach (string key in IntKeys)
        {
            if (absentInts.Contains(key))
                PlayerPrefs.DeleteKey(key);
            else
                PlayerPrefs.SetInt(key, savedInts[key]);
        }

        foreach (string key in StringKeys)
        {
            if (absentStrings.Contains(key))
                PlayerPrefs.DeleteKey(key);
            else
                PlayerPrefs.SetString(key, savedStrings[key]);
        }

        PlayerPrefs.Save();
    }
}
