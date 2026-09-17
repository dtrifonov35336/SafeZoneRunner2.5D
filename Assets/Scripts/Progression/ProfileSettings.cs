using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO; // Нужно для работы с файлами

public class ProfileSettings : MonoBehaviour
{
    [Header("UI Элементы")]
    public Button changeAvatarButton;
    public TMP_InputField nameInputField;
    public Button saveNameButton;
    public Image profileAvatarImage; // Ссылка на изображение аватара в профиле

    private string selectedImagePath;

    private void Start()
    {
        // 1. Загружаем сохраненное имя при старте
        string savedName = PlayerPrefs.GetString("Profile_Name", "Выживший");
        if (nameInputField != null) nameInputField.text = savedName;

        // 2. Пытаемся загрузить сохраненный аватар
        LoadSavedAvatar();

        // 3. Привязываем кнопки
        if (changeAvatarButton != null)
            changeAvatarButton.onClick.AddListener(OnChangeAvatarClicked);

        if (saveNameButton != null)
            saveNameButton.onClick.AddListener(OnSaveNameClicked);
    }

    // === Смена аватара ===
    private void OnChangeAvatarClicked()
    {
        // Вызываем галерею. Колбэк выполнится после выбора фото.
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                // 1. Запоминаем путь к выбранному файлу
                selectedImagePath = path;

                // 2. Загружаем текстуру для отображения
                Texture2D texture = NativeGallery.LoadImageAtPath(path, 512); // 512 - макс. размер
                if (texture != null)
                {
                    // 3. Создаем Sprite и присваиваем его аватару
                    Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    if (profileAvatarImage != null)
                    {
                        profileAvatarImage.sprite = newSprite;
                        profileAvatarImage.preserveAspect = true; // Сохраняем пропорции
                    }

                    // 4. Сохраняем путь к файлу в PlayerPrefs для будущих запусков
                    PlayerPrefs.SetString("Profile_AvatarPath", selectedImagePath);
                    PlayerPrefs.Save();

                    // 5. Показываем тост об успехе
                    if (ToastNotification.Instance != null)
                        ToastNotification.Instance.Show("Аватар обновлен!");
                }
            }
        });
    }

    // === Сохранение имени ===
    private void OnSaveNameClicked()
    {
        if (nameInputField != null && !string.IsNullOrEmpty(nameInputField.text))
        {
            PlayerPrefs.SetString("Profile_Name", nameInputField.text);
            PlayerPrefs.Save();

            if (ToastNotification.Instance != null)
                ToastNotification.Instance.Show("Имя сохранено!");
        }
    }

    // === Загрузка аватара при старте ===
    private void LoadSavedAvatar()
    {
        string savedPath = PlayerPrefs.GetString("Profile_AvatarPath", "");

        // Если путь сохранен и файл существует
        if (!string.IsNullOrEmpty(savedPath) && File.Exists(savedPath))
        {
            Texture2D texture = NativeGallery.LoadImageAtPath(savedPath, 512);
            if (texture != null)
            {
                Sprite savedSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                if (profileAvatarImage != null)
                {
                    profileAvatarImage.sprite = savedSprite;
                    profileAvatarImage.preserveAspect = true;
                }
            }
        }
        else
        {
            // Если пути нет, используем автоматическую иконку по персонажу (как было раньше)
            string charId = ProfileManager.GetSelectedCharacterId();
            Sprite charSprite = Resources.Load<Sprite>($"Characters/{charId}_avatar");
            if (charSprite != null && profileAvatarImage != null)
            {
                profileAvatarImage.sprite = charSprite;
                profileAvatarImage.preserveAspect = true;
            }
        }
    }
}