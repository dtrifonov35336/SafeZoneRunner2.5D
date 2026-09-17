using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class ProfileSettingsPanel : MonoBehaviour
{
    public static ProfileSettingsPanel Instance { get; private set; }

    [Header("Панель")]
    public GameObject panel;

    [Header("Имя")]
    public TMP_InputField nameInputField;
    public Button saveNameButton;

    [Header("Аватары — пресеты (6 штук)")]
    public List<Image> presetAvatars = new List<Image>();
    public List<GameObject> presetSelectedBorders = new List<GameObject>();

    [Header("Загрузка с устройства")]
    public Button uploadButton;

    [Header("Авто по персонажу")]
    public Toggle autoToggle;

    [Header("Закрыть")]
    public Button closeButton;

    [Header("Внешние ссылки")]
    [Tooltip("Image в профиле, который нужно обновлять")]
    public Image profileAvatarInTopBar;

    private const string NAME_KEY = "Profile_Name";
    private const string AVATAR_PATH_KEY = "Profile_AvatarPath";
    private const string AVATAR_PRESET_KEY = "Profile_AvatarPreset";
    private const string AVATAR_AUTO_KEY = "Profile_AvatarAuto";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Скрываем панель при старте
        if (panel != null) panel.SetActive(false);

        // Привязка кнопок
        if (saveNameButton != null) saveNameButton.onClick.AddListener(OnSaveName);
        if (uploadButton != null) uploadButton.onClick.AddListener(OnUploadFromDevice);
        if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);

        // Привязка пресетов
        for (int i = 0; i < presetAvatars.Count; i++)
        {
            int idx = i;
            if (presetAvatars[i] != null)
            {
                Button b = presetAvatars[i].GetComponent<Button>();
                if (b == null) b = presetAvatars[i].gameObject.AddComponent<Button>();
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(() => OnSelectPreset(idx));
            }
        }

        // Привязка тогла
        if (autoToggle != null)
        {
            autoToggle.onValueChanged.RemoveAllListeners();
            autoToggle.onValueChanged.AddListener(OnAutoToggleChanged);
        }

        // Загружаем сохраненные значения
        LoadSaved();
    }

    // ==================================================
    // ОТКРЫТИЕ / ЗАКРЫТИЕ
    // ==================================================

    public void OpenPanel()
    {
        if (panel == null) return;

        panel.SetActive(true);
        LoadSaved();
        RefreshAvatarBorders();
    }

    public void ClosePanel()
    {
        if (panel == null) return;
        panel.SetActive(false);

        // Обновляем аватар в профиле
        if (MainMenuManager.Instance != null)
            MainMenuManager.Instance.RefreshProfileAvatar();

        // Сохраняем
        PlayerPrefs.Save();
    }

    // ==================================================
    // ЗАГРУЗКА СОХРАНЁННЫХ
    // ==================================================

    private void LoadSaved()
    {
        // Имя
        if (nameInputField != null)
        {
            string savedName = PlayerPrefs.GetString(NAME_KEY, "Выживший");
            nameInputField.text = savedName;
        }

        // Авто
        if (autoToggle != null)
        {
            bool auto = PlayerPrefs.GetInt(AVATAR_AUTO_KEY, 1) == 1;
            autoToggle.isOn = auto;
        }
    }

    // ==================================================
    // СОХРАНЕНИЕ ИМЕНИ
    // ==================================================

    private void OnSaveName()
    {
        if (nameInputField == null) return;

        string name = nameInputField.text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            if (ToastNotification.Instance != null)
                ToastNotification.Instance.Show("Имя не может быть пустым");
            return;
        }

        PlayerPrefs.SetString(NAME_KEY, name);
        PlayerPrefs.Save();

        if (MainMenuManager.Instance != null)
            MainMenuManager.Instance.RefreshProfileName();

        if (ToastNotification.Instance != null)
            ToastNotification.Instance.Show("Имя сохранено");
    }

    // ==================================================
    // ВЫБОР ПРЕСЕТА
    // ==================================================

    private void OnSelectPreset(int index)
    {
        // Пресет и фото — взаимоисключающие
        PlayerPrefs.SetInt(AVATAR_PRESET_KEY, index);
        PlayerPrefs.DeleteKey(AVATAR_PATH_KEY);  // ← ОЧИЩАЕМ путь к фото

        PlayerPrefs.SetInt(AVATAR_AUTO_KEY, 0);
        PlayerPrefs.Save();

        if (autoToggle != null) autoToggle.isOn = false;

        RefreshAvatarBorders();

        if (MainMenuManager.Instance != null)
            MainMenuManager.Instance.RefreshProfileAvatar();

        if (ToastNotification.Instance != null)
            ToastNotification.Instance.Show($"Аватар {index + 1} выбран");
    }

    // ==================================================
    // ЗАГРУЗКА С УСТРОЙСТВА
    // ==================================================

    private void OnUploadFromDevice()
    {
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (string.IsNullOrEmpty(path)) return;

            // Копируем фото в persistentDataPath, чтобы оно не потерялось
            string fileName = "avatar_custom.png";
            string destPath = Path.Combine(Application.persistentDataPath, fileName);

            try
            {
                // Надёжный способ — загрузить через NativeGallery и пересохранить
                Texture2D tex = NativeGallery.LoadImageAtPath(path, 1024, false);
                if (tex == null)
                {
                    if (ToastNotification.Instance != null)
                        ToastNotification.Instance.Show("Не удалось загрузить фото");
                    return;
                }

                byte[] pngBytes = tex.EncodeToPNG();
                Object.Destroy(tex);
                File.WriteAllBytes(destPath, pngBytes);

                // Пресет и фото — взаимоисключающие
                PlayerPrefs.SetString(AVATAR_PATH_KEY, destPath);
                PlayerPrefs.DeleteKey(AVATAR_PRESET_KEY);  // ← ОЧИЩАЕМ пресет

                PlayerPrefs.SetInt(AVATAR_AUTO_KEY, 0);
                PlayerPrefs.Save();

                if (autoToggle != null) autoToggle.isOn = false;

                RefreshAvatarBorders();

                if (MainMenuManager.Instance != null)
                    MainMenuManager.Instance.RefreshProfileAvatar();

                if (ToastNotification.Instance != null)
                    ToastNotification.Instance.Show("Фото установлено");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Profile] Ошибка загрузки фото: {e.Message}");
                if (ToastNotification.Instance != null)
                    ToastNotification.Instance.Show("Не удалось загрузить фото");
            }
        }, "Выберите фото профиля", "image/*");
    }

    // ==================================================
    // АВТО ПО ПЕРСОНАЖУ
    // ==================================================

    private void OnAutoToggleChanged(bool isOn)
    {
        PlayerPrefs.SetInt(AVATAR_AUTO_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();

        RefreshAvatarBorders();

        if (MainMenuManager.Instance != null)
            MainMenuManager.Instance.RefreshProfileAvatar();
    }

    // ==================================================
    // ВИЗУАЛЬНОЕ ВЫДЕЛЕНИЕ
    // ==================================================

    private void RefreshAvatarBorders()
    {
        bool isAuto = PlayerPrefs.GetInt(AVATAR_AUTO_KEY, 1) == 1;
        int selectedPreset = PlayerPrefs.GetInt(AVATAR_PRESET_KEY, -1);
        string photoPath = PlayerPrefs.GetString(AVATAR_PATH_KEY, "");

        for (int i = 0; i < presetSelectedBorders.Count; i++)
        {
            if (presetSelectedBorders[i] == null) continue;

            bool selected = !isAuto
                && string.IsNullOrEmpty(photoPath)
                && selectedPreset == i;

            presetSelectedBorders[i].SetActive(selected);
        }
    }

    // ==================================================
    // СТАТИЧЕСКИЙ МЕТОД — ПОЛУЧИТЬ АКТУАЛЬНЫЙ СПРАЙТ
    // ==================================================

    public static Sprite GetCurrentAvatarSprite()
    {
        string charId = ProfileManager.GetSelectedCharacterId();
        bool isAuto = PlayerPrefs.GetInt(AVATAR_AUTO_KEY, 1) == 1;

        // 1. Авто по персонажу
        if (isAuto)
        {
            Sprite s = Resources.Load<Sprite>($"Characters/{charId}_avatar");
            if (s == null) s = Resources.Load<Sprite>($"Characters/{charId}_front");
            if (s != null) return s;
        }

        // 2. Загруженное фото
        string photoPath = PlayerPrefs.GetString(AVATAR_PATH_KEY, "");
        if (!string.IsNullOrEmpty(photoPath) && File.Exists(photoPath))
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(photoPath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(bytes);

                Sprite s = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f)
                );
                return s;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Profile] Не удалось загрузить фото: {e.Message}");
            }
        }

        // 3. Пресет
        int preset = PlayerPrefs.GetInt(AVATAR_PRESET_KEY, -1);
        if (preset >= 0)
        {
            Sprite s = Resources.Load<Sprite>($"ProfileAvatars/avatar_{preset}");
            if (s != null) return s;
        }

        // 4. Fallback — аватар персонажа
        return Resources.Load<Sprite>($"Characters/{charId}_avatar");
    }

    public static string GetCurrentName()
    {
        return PlayerPrefs.GetString(NAME_KEY, "Выживший");
    }
}