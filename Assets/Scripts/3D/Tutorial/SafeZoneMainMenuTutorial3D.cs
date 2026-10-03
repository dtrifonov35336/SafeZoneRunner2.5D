using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SafeZoneMainMenuTutorial3D : MonoBehaviour
{
    private const string COMPLETED_KEY =
        "SafeZoneMainMenuTutorialCompleted";

    private const string STARTED_KEY =
        "SafeZoneMainMenuTutorialStarted";

    private const string UPGRADE_COMPLETED_KEY =
        "SafeZoneUpgradeTutorialCompleted";

    private const string TUTORIAL_ACHIEVEMENT_ID =
        "tutorial_completed";

    private const string MAIN_MENU_SCENE =
        "MainMenu";

    private const string EQUIPMENT_SCENE =
        "Equipment";

    private const string HANGAR_SCENE =
        "Hangar";

    private const string SHOP_SCENE =
        "Shop";

    private const string CHARACTER_SCENE =
        "CharacterSelect";

    private static SafeZoneMainMenuTutorial3D instance;

    private Coroutine tutorialRoutine;

    private Canvas overlayCanvas;
    private RectTransform overlayRoot;

    private Image dimImage;
    private Image highlightImage;
    private Image instructionBackground;

    private TMP_Text instructionText;

    private Button currentTargetButton;

    private readonly List<SelectableState>
        savedSelectables =
            new List<SelectableState>();

    private class SelectableState
    {
        public Selectable selectable;
        public bool interactable;
    }

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (instance != null &&
            instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(
            DelayedStart()
        );
    }

    private IEnumerator DelayedStart()
    {
        yield return null;

        yield return new WaitForSecondsRealtime(
            0.5f
        );

        TryStart();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        DestroyOverlay();
    }

    // =========================================================
    // START
    // =========================================================

    public void TryStart()
    {
        if (tutorialRoutine != null)
        {
            return;
        }

        if (IsCompleted())
        {
            return;
        }

        if (!IsUpgradeTutorialCompleted())
        {
            return;
        }

        if (SceneManager.GetActiveScene().name !=
            MAIN_MENU_SCENE)
        {
            return;
        }

        tutorialRoutine =
            StartCoroutine(
                TutorialRoutine()
            );
    }

    private IEnumerator TutorialRoutine()
    {
        PlayerPrefs.SetInt(
            STARTED_KEY,
            1
        );

        PlayerPrefs.Save();

        // -----------------------------------------------------
        // ПОСЛЕДОВАТЕЛЬНОСТЬ
        // -----------------------------------------------------

        yield return RunEquipmentTutorial();

        yield return RunHangarTutorial();

        yield return RunShopTutorial();

        yield return RunCharactersTutorial();

        yield return RunDailyTutorial();

        yield return RunProfileTutorial();

        yield return RunSettingsTutorial();

        yield return RunAchievementsTutorial();

        yield return RunFinalTutorial();

        CompleteTutorial();

        tutorialRoutine = null;
    }

    // =========================================================
    // SCENE WAIT
    // =========================================================

    private IEnumerator WaitForScene(
        string sceneName,
        float timeout = 30f
    )
    {
        float timer = 0f;

        while (
            SceneManager.GetActiveScene().name !=
            sceneName
        )
        {
            timer += Time.unscaledDeltaTime;

            if (timer >= timeout)
            {
                yield break;
            }

            yield return null;
        }

        yield return null;
        yield return new WaitForEndOfFrame();

        yield return new WaitForSecondsRealtime(
            0.2f
        );
    }

    private IEnumerator WaitForMainMenu()
    {
        yield return WaitForScene(
            MAIN_MENU_SCENE
        );

        yield return new WaitForSecondsRealtime(
            0.2f
        );
    }

    private IEnumerator WaitForSceneObject<T>(
        float timeout = 15f
    )
        where T : UnityEngine.Object
    {
        float timer = 0f;

        while (
            FindObjectIncludingInactive<T>() == null
        )
        {
            timer += Time.unscaledDeltaTime;

            if (timer >= timeout)
            {
                yield break;
            }

            yield return null;
        }

        yield return null;
    }

    private T FindObjectIncludingInactive<T>()
        where T : UnityEngine.Object
    {
        T[] objects =
            Resources.FindObjectsOfTypeAll<T>();

        foreach (T obj in objects)
        {
            if (obj == null)
            {
                continue;
            }

            GameObject go =
                GetGameObject(obj);

            if (go == null)
            {
                continue;
            }

            if (!go.scene.IsValid())
            {
                continue;
            }

            return obj;
        }

        return null;
    }

    private GameObject GetGameObject(
        UnityEngine.Object obj
    )
    {
        Component component =
            obj as Component;

        if (component != null)
        {
            return component.gameObject;
        }

        return obj as GameObject;
    }

    // =========================================================
    // СНАРЯЖЕНИЕ
    // =========================================================

    private IEnumerator RunEquipmentTutorial()
    {
        yield return WaitForMainMenu();

        MainMenuManager menu =
            FindObjectIncludingInactive<
                MainMenuManager
            >();

        if (menu == null)
        {
            yield break;
        }

        Button button =
            FindButtonFromMember(
                menu,
                "equipmentButton"
            );

        if (button == null)
        {
            button =
                FindButtonByText(
                    "СНАРЯЖЕНИЕ"
                );
        }

        if (button == null)
        {
            yield break;
        }

        ShowTargetOverlay(
            button,
            "СНАРЯЖЕНИЕ\n\n" +
            "Здесь ты можешь улучшать экипировку.\n\n" +
            "Нажми на кнопку «Снаряжение»."
        );

        yield return WaitForTargetClick(
            button
        );

        yield return WaitForScene(
            EQUIPMENT_SCENE
        );

        yield return WaitForSceneObject<
            EquipmentManager
        >();

        EquipmentManager equipment =
            FindObjectIncludingInactive<
                EquipmentManager
            >();

        if (equipment != null)
        {
            ShowInfoOverlay(
                "СНАРЯЖЕНИЕ\n\n" +
                "Здесь находятся улучшения экипировки. " +
                "Они дают постоянные преимущества во время забегов."
            );

            yield return WaitForOk();

            Button upgrade =
                FindUpgradeButton(
                    equipment.gameObject
                );

            if (upgrade != null)
            {
                ShowTargetOverlay(
                    upgrade,
                    "УЛУЧШЕНИЕ\n\n" +
                    "Здесь можно улучшать экипировку.\n\n" +
                    "Покупать улучшение во время обучения не нужно."
                );

                yield return WaitForOk();
            }

            Button back =
                FindButtonFromMember(
                    equipment,
                    "backButton"
                );

            if (back != null)
            {
                ShowButtonOnlyHighlight(
                    back
                );

                yield return WaitForTargetClick(
                    back
                );
            }
        }

        // КРИТИЧЕСКОЕ:
        // не считаем выход завершённым,
        // пока реально не вернулись в MainMenu.
        yield return WaitForMainMenu();
    }

    // =========================================================
    // АНГАР
    // =========================================================

    private IEnumerator RunHangarTutorial()
    {
        yield return WaitForMainMenu();

        MainMenuManager menu =
            FindObjectIncludingInactive<
                MainMenuManager
            >();

        if (menu == null)
        {
            yield break;
        }

        Button button =
            FindButtonFromMember(
                menu,
                "hangarButton"
            );

        if (button == null)
        {
            button =
                FindButtonByText(
                    "АНГАР"
                );
        }

        if (button == null)
        {
            yield break;
        }

        ShowTargetOverlay(
            button,
            "АНГАР\n\n" +
            "Здесь находится развитие убежища " +
            "и его улучшения.\n\n" +
            "Нажми на кнопку «Ангар»."
        );

        yield return WaitForTargetClick(
            button
        );

        yield return WaitForScene(
            HANGAR_SCENE
        );

        yield return WaitForSceneObject<
            HangarManager
        >();

        HangarManager hangar =
            FindObjectIncludingInactive<
                HangarManager
            >();

        if (hangar != null)
        {
            ShowInfoOverlay(
                "АНГАР\n\n" +
                "Здесь можно развивать убежище " +
                "и улучшать его возможности."
            );

            yield return WaitForOk();

            Button upgrade =
                FindUpgradeButton(
                    hangar.gameObject
                );

            if (upgrade != null)
            {
                ShowTargetOverlay(
                    upgrade,
                    "УЛУЧШЕНИЕ\n\n" +
                    "Здесь находится улучшение ангара.\n\n" +
                    "Покупать его сейчас не требуется."
                );

                yield return WaitForOk();
            }

            Button back =
                FindButtonFromMember(
                    hangar,
                    "backButton"
                );

            if (back == null)
            {
                back =
                    FindButtonByText(
                        "НАЗАД"
                    );
            }

            if (back != null)
            {
                ShowButtonOnlyHighlight(
                    back
                );

                yield return WaitForTargetClick(
                    back
                );
            }
        }

        // ВАЖНО:
        // ждём фактический возврат.
        // После этого TutorialRoutine продолжит
        // выполнение RunShopTutorial().
        yield return WaitForMainMenu();
    }

    // =========================================================
    // МАГАЗИН
    // =========================================================

    private IEnumerator RunShopTutorial()
    {
        yield return WaitForMainMenu();

        MainMenuManager menu =
            FindObjectIncludingInactive<
                MainMenuManager
            >();

        if (menu == null)
        {
            yield break;
        }

        Button shop =
            FindButtonFromMember(
                menu,
                "shopButton"
            );

        if (shop == null)
        {
            shop =
                FindButtonByText(
                    "МАГАЗИН"
                );
        }

        if (shop == null)
        {
            yield break;
        }

        ShowTargetOverlay(
            shop,
            "МАГАЗИН\n\n" +
            "Здесь можно покупать доступные товары " +
            "за игровые ресурсы.\n\n" +
            "Нажми на кнопку «Магазин»."
        );

        yield return WaitForTargetClick(
            shop
        );

        yield return WaitForScene(
            SHOP_SCENE
        );

        yield return WaitForSceneObject<
            ShopManager
        >();

        ShopManager shopManager =
            FindObjectIncludingInactive<
                ShopManager
            >();

        if (shopManager != null)
        {
            ShowInfoOverlay(
                "МАГАЗИН\n\n" +
                "Здесь отображаются доступные покупки, " +
                "их стоимость и содержимое.\n\n" +
                "Покупать ничего не нужно."
            );

            yield return WaitForOk();

            Button item =
                FindFirstMeaningfulButton(
                    shopManager.gameObject
                );

            if (item != null)
            {
                ShowTargetOverlay(
                    item,
                    "ТОВАР\n\n" +
                    "Здесь можно посмотреть содержимое " +
                    "и стоимость товара.\n\n" +
                    "Покупать его во время обучения не требуется."
                );

                yield return WaitForOk();
            }

            Button back =
                FindButtonFromMember(
                    shopManager,
                    "backButton"
                );

            if (back == null)
            {
                back =
                    FindButtonByText(
                        "НАЗАД"
                    );
            }

            if (back != null)
            {
                ShowButtonOnlyHighlight(
                    back
                );

                yield return WaitForTargetClick(
                    back
                );
            }
        }

        yield return WaitForMainMenu();
    }

    // =========================================================
    // ПЕРСОНАЖИ
    // =========================================================

    private IEnumerator RunCharactersTutorial()
    {
        yield return WaitForMainMenu();

        MainMenuManager menu =
            FindObjectIncludingInactive<
                MainMenuManager
            >();

        if (menu == null)
        {
            yield break;
        }

        Button characters =
            FindButtonFromMember(
                menu,
                "charactersButton"
            );

        if (characters == null)
        {
            characters =
                FindButtonByText(
                    "ПЕРСОНАЖИ"
                );
        }

        if (characters == null)
        {
            yield break;
        }

        ShowTargetOverlay(
            characters,
            "ПЕРСОНАЖИ\n\n" +
            "Здесь можно выбирать персонажей, " +
            "открывать новых героев и смотреть их бонусы.\n\n" +
            "Нажми на кнопку «Персонажи»."
        );

        yield return WaitForTargetClick(
            characters
        );

        yield return WaitForScene(
            CHARACTER_SCENE
        );

        yield return new WaitForSecondsRealtime(
            0.3f
        );

        CharacterSelectManager manager =
            FindObjectIncludingInactive<
                CharacterSelectManager
            >();

        if (manager != null)
        {
            ShowInfoOverlay(
                "ПЕРСОНАЖИ\n\n" +
                "У каждого персонажа есть собственный бонус.\n\n" +
                "Выбери подходящего персонажа для забегов."
            );

            yield return WaitForOk();

            Button action =
                FindButtonFromMember(
                    manager,
                    "actionButton"
                );

            if (action != null)
            {
                ShowTargetOverlay(
                    action,
                    "ДЕЙСТВИЕ ПЕРСОНАЖА\n\n" +
                    "Здесь отображается действие " +
                    "для выбранного персонажа.\n\n" +
                    "Покупать персонажа сейчас не нужно."
                );

                yield return WaitForOk();
            }
        }

        Button back =
            FindButtonByText(
                "НАЗАД"
            );

        if (back == null)
        {
            back =
                FindButtonByText(
                    "ЗАКРЫТЬ"
                );
        }

        if (back != null)
        {
            ShowButtonOnlyHighlight(
                back
            );

            yield return WaitForTargetClick(
                back
            );
        }

        yield return WaitForMainMenu();
    }

    // =========================================================
    // ЕЖЕДНЕВНЫЙ ВХОД
    // =========================================================

    private IEnumerator RunDailyTutorial()
    {
        yield return WaitForMainMenu();

        Button daily =
            FindUtilityButton(
                "DailyLogin"
            );

        if (daily == null)
        {
            daily =
                FindButtonByText(
                    "ЕЖЕДНЕВНЫЙ"
                );
        }

        if (daily == null)
        {
            daily =
                FindButtonByText(
                    "ВХОД"
                );
        }

        if (daily == null)
        {
            yield break;
        }

        ShowTargetOverlay(
            daily,
            "ЕЖЕДНЕВНЫЙ ВХОД\n\n" +
            "Здесь каждый день доступна награда.\n\n" +
            "Нажми на кнопку ежедневного входа."
        );

        yield return WaitForTargetClick(
            daily
        );

        yield return new WaitForSecondsRealtime(
            0.3f
        );

        DailyLoginUI3D ui =
            FindObjectIncludingInactive<
                DailyLoginUI3D
            >();

        if (ui != null)
        {
            ShowInfoOverlay(
                "ЕЖЕДНЕВНЫЙ ВХОД\n\n" +
                "Каждый день ты можешь получать награду.\n\n" +
                "Регулярные входы продолжают серию."
            );

            yield return WaitForOk();

            Button claim =
                FindButtonByText(
                    "ЗАБРАТЬ"
                );

            if (claim != null &&
                claim.interactable)
            {
                ShowTargetOverlay(
                    claim,
                    "ЗАБРАТЬ НАГРАДУ\n\n" +
                    "Нажми «Забрать», чтобы получить " +
                    "доступную награду."
                );

                yield return WaitForTargetClick(
                    claim
                );
            }
            else
            {
                ShowInfoOverlay(
                    "НАГРАДА\n\n" +
                    "Когда награда будет доступна, " +
                    "её можно будет забрать этой кнопкой."
                );

                yield return WaitForOk();
            }

            Button close =
                FindButtonFromMember(
                    ui,
                    "closeButton"
                );

            if (close == null)
            {
                close =
                    FindButtonByText(
                        "ЗАКРЫТЬ"
                    );
            }

            if (close == null)
            {
                close =
                    FindButtonByText(
                        "НАЗАД"
                    );
            }

            if (close != null)
            {
                ShowButtonOnlyHighlight(
                    close
                );

                yield return WaitForTargetClick(
                    close
                );
            }
        }

        yield return new WaitForSecondsRealtime(
            0.2f
        );
    }

    // =========================================================
    // ПРОФИЛЬ
    // =========================================================

    private IEnumerator RunProfileTutorial()
    {
        yield return WaitForMainMenu();

        MainMenuManager menu =
            FindObjectIncludingInactive<
                MainMenuManager
            >();

        if (menu == null)
        {
            yield break;
        }

        Button profile =
            FindButtonFromMember(
                menu,
                "profileAvatarButton"
            );

        if (profile == null)
        {
            profile =
                FindButtonByText(
                    "ПРОФИЛЬ"
                );
        }

        if (profile == null)
        {
            yield break;
        }

        ShowTargetOverlay(
            profile,
            "ПРОФИЛЬ\n\n" +
            "Здесь можно настроить внешний вид и данные профиля.\n\n" +
            "Нажми на кнопку профиля."
        );

        yield return WaitForTargetClick(
            profile
        );

        yield return new WaitForSecondsRealtime(
            0.3f
        );

        ProfileSettingsPanel panel =
            FindObjectIncludingInactive<
                ProfileSettingsPanel
            >();

        if (panel != null)
        {
            // АВАТАР
            Button avatar =
                FindButtonFromMember(
                    panel,
                    "avatarButton"
                );

            if (avatar != null)
            {
                ShowTargetOverlay(
                    avatar,
                    "АВАТАР\n\n" +
                    "Здесь можно изменить аватар профиля."
                );

                yield return WaitForOk();
            }
            else
            {
                ShowInfoOverlay(
                    "АВАТАР\n\n" +
                    "Здесь можно изменить изображение профиля."
                );

                yield return WaitForOk();
            }

            // ИМЯ
            ShowInfoOverlay(
                "ИМЯ ПРОФИЛЯ\n\n" +
                "Здесь можно изменить имя, " +
                "которое отображается в профиле."
            );

            yield return WaitForOk();

            // АВТОПОДСТАНОВКА
            Toggle autoToggle =
                FindToggleFromMember(
                    panel,
                    "autoToggle"
                );

            if (autoToggle != null)
            {
                ShowTargetOverlayGeneric(
                    autoToggle,
                    "АВТОМАТИЧЕСКАЯ ПОДСТАНОВКА\n\n" +
                    "Эта галка автоматически подставляет " +
                    "аватар выбранного персонажа."
                );

                yield return WaitForOk();
            }
            else
            {
                ShowInfoOverlay(
                    "АВТОМАТИЧЕСКАЯ ПОДСТАНОВКА\n\n" +
                    "Эта функция позволяет автоматически " +
                    "использовать аватар выбранного персонажа."
                );

                yield return WaitForOk();
            }

            // ЗАГРУЗКА
            Button upload =
                FindButtonFromMember(
                    panel,
                    "uploadButton"
                );

            if (upload != null)
            {
                ShowTargetOverlay(
                    upload,
                    "ЗАГРУЗКА ФОТО\n\n" +
                    "Здесь можно загрузить собственную " +
                    "фотографию с телефона.\n\n" +
                    "Нажимать сейчас не нужно."
                );

                yield return WaitForOk();
            }
            else
            {
                ShowInfoOverlay(
                    "ФОТО С ТЕЛЕФОНА\n\n" +
                    "В профиле можно загрузить собственную " +
                    "фотографию с устройства."
                );

                yield return WaitForOk();
            }

            Button close =
                FindButtonFromMember(
                    panel,
                    "closeButton"
                );

            if (close == null)
            {
                close =
                    FindButtonByText(
                        "ЗАКРЫТЬ"
                    );
            }

            if (close == null)
            {
                close =
                    FindButtonByText(
                        "НАЗАД"
                    );
            }

            if (close != null)
            {
                ShowButtonOnlyHighlight(
                    close
                );

                yield return WaitForTargetClick(
                    close
                );
            }
        }

        yield return WaitForMainMenu();

        // ДОСТИЖЕНИЕ АКТИВИРУЕТСЯ ИМЕННО
        // ПОСЛЕ ВЫХОДА ИЗ ПРОФИЛЯ.
        UnlockTutorialAchievement();
    }

    // =========================================================
    // НАСТРОЙКИ
    // =========================================================

    private IEnumerator RunSettingsTutorial()
    {
        yield return WaitForMainMenu();

        Button settings =
            FindButtonByText(
                "НАСТРОЙКИ"
            );

        if (settings == null)
        {
            settings =
                FindButtonByText(
                    "НАСТРОЙКА"
                );
        }

        if (settings == null)
        {
            yield break;
        }

        ShowTargetOverlay(
            settings,
            "НАСТРОЙКИ\n\n" +
            "Здесь можно настроить звук и вибрацию игры.\n\n" +
            "Нажми на кнопку «Настройки»."
        );

        yield return WaitForTargetClick(
            settings
        );

        yield return new WaitForSecondsRealtime(
            0.3f
        );

        // ГРОМКОСТЬ
        Slider volume =
            FindSliderByText(
                "ГРОМКОСТЬ",
                "ЗВУК"
            );

        if (volume != null)
        {
            ShowTargetOverlayGeneric(
                volume,
                "ГРОМКОСТЬ\n\n" +
                "Ползунок позволяет изменить громкость игры."
            );

            yield return WaitForOk();
        }
        else
        {
            ShowInfoOverlay(
                "ГРОМКОСТЬ\n\n" +
                "С помощью ползунка можно изменить громкость игры."
            );

            yield return WaitForOk();
        }

        // ВИБРАЦИЯ
        Toggle vibration =
            FindToggleByText(
                "ВИБРАЦИЯ"
            );

        if (vibration != null)
        {
            ShowTargetOverlayGeneric(
                vibration,
                "ВИБРАЦИЯ\n\n" +
                "Эта настройка включает или отключает вибрацию."
            );

            yield return WaitForOk();
        }
        else
        {
            ShowInfoOverlay(
                "ВИБРАЦИЯ\n\n" +
                "Здесь можно включить или отключить вибрацию."
            );

            yield return WaitForOk();
        }

        Button close =
            FindButtonByText(
                "ЗАКРЫТЬ"
            );

        if (close == null)
        {
            close =
                FindButtonByText(
                    "НАЗАД"
                );
        }

        if (close != null)
        {
            ShowButtonOnlyHighlight(
                close
            );

            yield return WaitForTargetClick(
                close
            );
        }

        yield return WaitForMainMenu();
    }

    // =========================================================
    // ДОСТИЖЕНИЯ
    // =========================================================

    private IEnumerator RunAchievementsTutorial()
    {
        yield return WaitForMainMenu();

        Button achievementsButton =
            FindButtonByText(
                "ДОСТИЖЕНИЯ"
            );

        if (achievementsButton == null)
        {
            achievementsButton =
                FindButtonByText(
                    "ДОСТИЖЕНИЕ"
                );
        }

        if (achievementsButton == null)
        {
            yield break;
        }

        ShowTargetOverlay(
            achievementsButton,
            "ДОСТИЖЕНИЯ\n\n" +
            "Здесь находятся выполненные цели и награды.\n\n" +
            "Нажми на кнопку «Достижения»."
        );

        yield return WaitForTargetClick(
            achievementsButton
        );

        yield return new WaitForSecondsRealtime(
            0.3f
        );

        AchievementsUI3D achievements =
            FindObjectIncludingInactive<
                AchievementsUI3D
            >();

        if (achievements != null)
        {
            ShowInfoOverlay(
                "ДОСТИЖЕНИЯ\n\n" +
                "После выполнения достижения его награду " +
                "нужно забрать вручную."
            );

            yield return WaitForOk();

            Button shelter =
                FindButtonByText(
                    "ДО УБЕЖИЩА"
                );

            if (shelter == null)
            {
                shelter =
                    FindButtonByText(
                        "УБЕЖИЩЕ"
                    );
            }

            if (shelter != null)
            {
                ShowTargetOverlay(
                    shelter,
                    "ДО УБЕЖИЩА\n\n" +
                    "Здесь находятся достижения режима «До убежища»."
                );

                yield return WaitForTargetClick(
                    shelter
                );
            }

            Button endless =
                FindButtonByText(
                    "БЕСКОНЕЧНЫЙ"
                );

            if (endless == null)
            {
                endless =
                    FindButtonByText(
                        "БЕСКОНЕЧНОСТЬ"
                    );
            }

            if (endless != null)
            {
                ShowTargetOverlay(
                    endless,
                    "БЕСКОНЕЧНЫЙ\n\n" +
                    "Здесь находятся достижения бесконечного режима."
                );

                yield return WaitForTargetClick(
                    endless
                );
            }

            // Достижение уже активно после Профиля.
            UnlockTutorialAchievement();

            yield return new WaitForSecondsRealtime(
                0.3f
            );

            Button claim =
                FindButtonByText(
                    "ЗАБРАТЬ"
                );

            if (claim != null &&
                claim.interactable)
            {
                ShowTargetOverlay(
                    claim,
                    "НАГРАДА ЗА ОБУЧЕНИЕ\n\n" +
                    "Ты прошёл обучение.\n\n" +
                    "Нажми «Забрать», чтобы получить +50 монет."
                );

                yield return WaitForTargetClick(
                    claim
                );
            }
            else
            {
                ShowInfoOverlay(
                    "НАГРАДА ЗА ОБУЧЕНИЕ\n\n" +
                    "За прохождение обучения доступна награда +50 монет."
                );

                yield return WaitForOk();
            }

            Button close =
                FindButtonByText(
                    "ЗАКРЫТЬ"
                );

            if (close == null)
            {
                close =
                    FindButtonByText(
                        "НАЗАД"
                    );
            }

            if (close != null)
            {
                ShowButtonOnlyHighlight(
                    close
                );

                yield return WaitForTargetClick(
                    close
                );
            }
        }

        yield return WaitForMainMenu();
    }

    // =========================================================
    // FINAL
    // =========================================================

    private IEnumerator RunFinalTutorial()
    {
        ShowInfoOverlay(
            "ОБУЧЕНИЕ ЗАВЕРШЕНО!\n\n" +
            "Поздравляем! Ты прошёл обучение.\n\n" +
            "Теперь ты знаешь, как пользоваться " +
            "основными разделами игры, получать награды " +
            "и развивать персонажей, экипировку и убежище."
        );

        yield return WaitForOk();
    }

    // =========================================================
    // ACHIEVEMENT
    // =========================================================

    private void UnlockTutorialAchievement()
    {
        try
        {
            AchievementSystem3D.Unlock(
                TUTORIAL_ACHIEVEMENT_ID
            );
        }
        catch (Exception e)
        {
            Debug.LogWarning(
                "[MainMenuTutorial] " +
                "Не удалось активировать достижение: " +
                e.Message
            );
        }
    }

    // =========================================================
    // OVERLAY
    // =========================================================

    private void CreateOverlay()
    {
        DestroyOverlay();

        GameObject canvasObject =
            new GameObject(
                "SafeZoneTutorialOverlay",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );

        overlayCanvas =
            canvasObject.GetComponent<Canvas>();

        overlayCanvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        overlayCanvas.sortingOrder =
            5000;

        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(
                1080f,
                1920f
            );

        scaler.matchWidthOrHeight =
            0.5f;

        overlayRoot =
            canvasObject.GetComponent<
                RectTransform
            >();

        // -----------------------------------------------------
        // DIM
        // -----------------------------------------------------

        GameObject dimObject =
            new GameObject(
                "Dim",
                typeof(RectTransform),
                typeof(Image)
            );

        dimObject.transform.SetParent(
            overlayRoot,
            false
        );

        RectTransform dimRect =
            dimObject.GetComponent<
                RectTransform
            >();

        StretchFull(dimRect);

        dimImage =
            dimObject.GetComponent<Image>();

        dimImage.color =
            new Color(
                0f,
                0f,
                0f,
                0.72f
            );

        dimImage.raycastTarget =
            true;

        // -----------------------------------------------------
        // HIGHLIGHT
        // -----------------------------------------------------

        GameObject highlightObject =
            new GameObject(
                "Highlight",
                typeof(RectTransform),
                typeof(Image)
            );

        highlightObject.transform.SetParent(
            overlayRoot,
            false
        );

        highlightImage =
            highlightObject.GetComponent<Image>();

        highlightImage.sprite =
            RuntimeUISprite3D.GetRoundedSprite();

        highlightImage.type =
            Image.Type.Sliced;

        highlightImage.color =
            new Color(
                1f,
                0.82f,
                0.25f,
                0.28f
            );

        highlightImage.raycastTarget =
            false;

        // -----------------------------------------------------
        // PANEL
        // -----------------------------------------------------

        GameObject panelObject =
            new GameObject(
                "InstructionBackground",
                typeof(RectTransform),
                typeof(Image)
            );

        panelObject.transform.SetParent(
            overlayRoot,
            false
        );

        instructionBackground =
            panelObject.GetComponent<Image>();

        instructionBackground.sprite =
            RuntimeUISprite3D.GetRoundedSprite();

        instructionBackground.type =
            Image.Type.Sliced;

        instructionBackground.color =
            new Color(
                0f,
                0f,
                0f,
                0.78f
            );

        instructionBackground.raycastTarget =
            false;

        RectTransform panelRect =
            instructionBackground.rectTransform;

        panelRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        panelRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        panelRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        panelRect.sizeDelta =
            new Vector2(
                760f,
                330f
            );

        panelRect.anchoredPosition =
            new Vector2(
                0f,
                120f
            );

        // -----------------------------------------------------
        // TEXT
        // -----------------------------------------------------

        GameObject textObject =
            new GameObject(
                "InstructionText",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

        textObject.transform.SetParent(
            panelObject.transform,
            false
        );

        instructionText =
            textObject.GetComponent<
                TextMeshProUGUI
            >();

        RectTransform textRect =
            instructionText.rectTransform;

        textRect.anchorMin =
            Vector2.zero;

        textRect.anchorMax =
            Vector2.one;

        textRect.offsetMin =
            new Vector2(
                35f,
                25f
            );

        textRect.offsetMax =
            new Vector2(
                -35f,
                -25f
            );

        instructionText.color =
            Color.white;

        instructionText.fontSize =
            28f;

        instructionText.fontSizeMin =
            18f;

        instructionText.fontSizeMax =
            28f;

        instructionText.enableAutoSizing =
            true;

        instructionText.alignment =
            TextAlignmentOptions.Center;

        instructionText.textWrappingMode =
            TextWrappingModes.Normal;

        instructionText.overflowMode =
            TextOverflowModes.Truncate;

        instructionText.outlineWidth =
            0.18f;

        instructionText.outlineColor =
            Color.black;

        instructionText.raycastTarget =
            false;
    }

    private void DestroyOverlay()
    {
        RestoreSelectables();

        if (overlayCanvas != null)
        {
            Destroy(
                overlayCanvas.gameObject
            );
        }

        overlayCanvas = null;
        overlayRoot = null;
        dimImage = null;
        highlightImage = null;
        instructionBackground = null;
        instructionText = null;
        currentTargetButton = null;
    }

    // =========================================================
    // INFO
    // =========================================================

    private void ShowInfoOverlay(
        string text
    )
    {
        CreateOverlay();

        DisableAllSelectablesExcept(
            null
        );

        dimImage.raycastTarget =
            true;

        highlightImage.gameObject.SetActive(
            false
        );

        instructionBackground.gameObject.SetActive(
            true
        );

        ShowInstructionText(
            text
        );

        PositionInstructionPanel(
            new Vector2(
                0f,
                120f
            )
        );
    }

    private void ShowInstructionText(
        string text
    )
    {
        if (instructionText == null)
        {
            return;
        }

        instructionText.gameObject.SetActive(
            true
        );

        instructionText.text =
            text;

        Canvas.ForceUpdateCanvases();
    }

    // =========================================================
    // TARGET BUTTON
    // =========================================================

    private void ShowTargetOverlay(
        Button target,
        string text
    )
    {
        if (target == null)
        {
            ShowInfoOverlay(text);
            return;
        }

        CreateOverlay();

        currentTargetButton =
            target;

        DisableAllSelectablesExcept(
            target
        );

        dimImage.raycastTarget =
            false;

        highlightImage.gameObject.SetActive(
            true
        );

        PositionHighlight(
            target.transform as RectTransform
        );

        PositionInstructionNearTarget(
            target.transform as RectTransform
        );

        ShowInstructionText(
            text
        );
    }

    // =========================================================
    // GENERIC UI TARGET
    // =========================================================

    private void ShowTargetOverlayGeneric(
        Selectable target,
        string text
    )
    {
        if (target == null)
        {
            ShowInfoOverlay(text);
            return;
        }

        CreateOverlay();

        DisableAllSelectablesExcept(
            target
        );

        dimImage.raycastTarget =
            true;

        highlightImage.gameObject.SetActive(
            true
        );

        RectTransform targetRect =
            target.transform as RectTransform;

        PositionHighlight(
            targetRect
        );

        PositionInstructionNearTarget(
            targetRect
        );

        ShowInstructionText(
            text
        );
    }

    private void ShowButtonOnlyHighlight(
        Button target
    )
    {
        if (target == null)
        {
            return;
        }

        CreateOverlay();

        currentTargetButton =
            target;

        DisableAllSelectablesExcept(
            target
        );

        dimImage.raycastTarget =
            false;

        instructionBackground.gameObject.SetActive(
            false
        );

        instructionText.gameObject.SetActive(
            false
        );

        highlightImage.gameObject.SetActive(
            true
        );

        PositionHighlight(
            target.transform as RectTransform
        );
    }

    // =========================================================
    // HIGHLIGHT
    // =========================================================

    private void PositionHighlight(
        RectTransform target
    )
    {
        if (
            target == null ||
            highlightImage == null ||
            overlayCanvas == null
        )
        {
            return;
        }

        RectTransform canvasRect =
            overlayCanvas.GetComponent<
                RectTransform
            >();

        Vector3[] corners =
            new Vector3[4];

        target.GetWorldCorners(
            corners
        );

        Vector2 bl;
        Vector2 tr;

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility.WorldToScreenPoint(
                    null,
                    corners[0]
                ),
                null,
                out bl
            );

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility.WorldToScreenPoint(
                    null,
                    corners[2]
                ),
                null,
                out tr
            );

        Vector2 center =
            (bl + tr) * 0.5f;

        Vector2 size =
            new Vector2(
                Mathf.Abs(
                    tr.x - bl.x
                ),
                Mathf.Abs(
                    tr.y - bl.y
                )
            );

        RectTransform highlightRect =
            highlightImage.rectTransform;

        highlightRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        highlightRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        highlightRect.anchoredPosition =
            center;

        highlightRect.sizeDelta =
            size +
            new Vector2(
                16f,
                16f
            );
    }

    // =========================================================
    // TEXT POSITION
    // =========================================================

    private void PositionInstructionNearTarget(
        RectTransform target
    )
    {
        if (
            target == null ||
            instructionBackground == null ||
            overlayCanvas == null
        )
        {
            return;
        }

        RectTransform canvasRect =
            overlayCanvas.GetComponent<
                RectTransform
            >();

        Vector3[] corners =
            new Vector3[4];

        target.GetWorldCorners(
            corners
        );

        Vector2 bl;
        Vector2 tr;

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility.WorldToScreenPoint(
                    null,
                    corners[0]
                ),
                null,
                out bl
            );

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility.WorldToScreenPoint(
                    null,
                    corners[2]
                ),
                null,
                out tr
            );

        Vector2 center =
            (bl + tr) * 0.5f;

        float width =
            canvasRect.rect.width;

        float height =
            canvasRect.rect.height;

        float halfW =
            instructionBackground.rectTransform
                .rect.width *
            0.5f;

        float halfH =
            instructionBackground.rectTransform
                .rect.height *
            0.5f;

        float gap = 35f;

        Vector2 position;

        if (
            tr.x +
            gap +
            halfW <
            width * 0.5f
        )
        {
            position =
                new Vector2(
                    tr.x +
                    gap +
                    halfW,
                    center.y
                );
        }
        else if (
            bl.x -
            gap -
            halfW >
            -width * 0.5f
        )
        {
            position =
                new Vector2(
                    bl.x -
                    gap -
                    halfW,
                    center.y
                );
        }
        else if (
            tr.y +
            gap +
            halfH <
            height * 0.5f
        )
        {
            position =
                new Vector2(
                    center.x,
                    tr.y +
                    gap +
                    halfH
                );
        }
        else
        {
            position =
                new Vector2(
                    center.x,
                    bl.y -
                    gap -
                    halfH
                );
        }

        position.x =
            Mathf.Clamp(
                position.x,
                -width * 0.5f +
                halfW,
                width * 0.5f -
                halfW
            );

        position.y =
            Mathf.Clamp(
                position.y,
                -height * 0.5f +
                halfH,
                height * 0.5f -
                halfH
            );

        PositionInstructionPanel(
            position
        );
    }

    private void PositionInstructionPanel(
        Vector2 position
    )
    {
        RectTransform rect =
            instructionBackground.rectTransform;

        rect.anchoredPosition =
            position;
    }

    // =========================================================
    // OK
    // =========================================================

    private IEnumerator WaitForOk()
    {
        Button ok =
            CreateOkButton();

        if (ok == null)
        {
            yield return new WaitForSecondsRealtime(
                1f
            );

            DestroyOverlay();

            yield break;
        }

        bool clicked =
            false;

        UnityEngine.Events.UnityAction action =
            () =>
            {
                clicked = true;
            };

        ok.onClick.AddListener(
            action
        );

        while (
            !clicked
        )
        {
            yield return null;
        }

        ok.onClick.RemoveListener(
            action
        );

        DestroyOverlay();

        yield return null;
    }

    private Button CreateOkButton()
    {
        if (overlayRoot == null)
        {
            return null;
        }

        GameObject obj =
            new GameObject(
                "TutorialOK",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button)
            );

        obj.transform.SetParent(
            overlayRoot,
            false
        );

        RectTransform rect =
            obj.GetComponent<
                RectTransform
            >();

        rect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        rect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        rect.sizeDelta =
            new Vector2(
                250f,
                70f
            );

        rect.anchoredPosition =
            new Vector2(
                0f,
                -175f
            );

        Image image =
            obj.GetComponent<Image>();

        image.sprite =
            RuntimeUISprite3D.GetRoundedSprite();

        image.type =
            Image.Type.Sliced;

        image.color =
            new Color32(
                55,
                120,
                78,
                255
            );

        Button button =
            obj.GetComponent<Button>();

        button.targetGraphic =
            image;

        GameObject textObj =
            new GameObject(
                "Text",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

        textObj.transform.SetParent(
            obj.transform,
            false
        );

        RectTransform textRect =
            textObj.GetComponent<
                RectTransform
            >();

        StretchFull(
            textRect
        );

        TMP_Text text =
            textObj.GetComponent<
                TMP_Text
            >();

        text.text =
            "OK";

        text.fontSize =
            26f;

        text.alignment =
            TextAlignmentOptions.Center;

        text.color =
            Color.white;

        text.raycastTarget =
            false;

        return button;
    }

    // =========================================================
    // TARGET CLICK
    // =========================================================

    private IEnumerator WaitForTargetClick(
        Button target
    )
    {
        if (target == null)
        {
            yield break;
        }

        bool clicked =
            false;

        UnityEngine.Events.UnityAction action =
            () =>
            {
                clicked = true;
            };

        target.onClick.AddListener(
            action
        );

        while (
            !clicked &&
            target != null
        )
        {
            yield return null;
        }

        if (target != null)
        {
            target.onClick.RemoveListener(
                action
            );
        }

        DestroyOverlay();

        yield return null;
    }

    // =========================================================
    // SELECTABLES
    // =========================================================

    private void DisableAllSelectablesExcept(
        Selectable target
    )
    {
        savedSelectables.Clear();

        Selectable[] all =
            FindObjectsByType<Selectable>(
                FindObjectsSortMode.None
            );

        foreach (
            Selectable selectable
            in all
        )
        {
            if (selectable == null)
            {
                continue;
            }

            savedSelectables.Add(
                new SelectableState
                {
                    selectable =
                        selectable,

                    interactable =
                        selectable.interactable
                }
            );

            selectable.interactable =
                selectable == target;
        }

        if (target != null)
        {
            target.interactable =
                true;
        }
    }

    private void RestoreSelectables()
    {
        foreach (
            SelectableState state
            in savedSelectables
        )
        {
            if (
                state.selectable != null
            )
            {
                state.selectable.interactable =
                    state.interactable;
            }
        }

        savedSelectables.Clear();
    }

    // =========================================================
    // BUTTON SEARCH
    // =========================================================

    private Button FindButtonByText(
        params string[] values
    )
    {
        Button[] buttons =
            FindObjectsByType<Button>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (
            Button button
            in buttons
        )
        {
            if (button == null)
            {
                continue;
            }

            if (!button.gameObject.activeInHierarchy)
            {
                continue;
            }

            TMP_Text text =
                button.GetComponentInChildren<
                    TMP_Text
                >(true);

            if (text == null)
            {
                continue;
            }

            string current =
                text.text
                    .Trim()
                    .ToUpperInvariant();

            foreach (
                string value
                in values
            )
            {
                if (
                    current.Contains(
                        value.ToUpperInvariant()
                    )
                )
                {
                    return button;
                }
            }
        }

        return null;
    }

    private Button FindButtonFromMember(
        object owner,
        string memberName
    )
    {
        if (owner == null)
        {
            return null;
        }

        Type type =
            owner.GetType();

        FieldInfo field =
            type.GetField(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        if (field != null)
        {
            Button button =
                ConvertToButton(
                    field.GetValue(owner)
                );

            if (button != null)
            {
                return button;
            }
        }

        PropertyInfo property =
            type.GetProperty(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        if (
            property != null &&
            property.CanRead
        )
        {
            return ConvertToButton(
                property.GetValue(
                    owner,
                    null
                )
            );
        }

        return null;
    }

    private Button ConvertToButton(
        object value
    )
    {
        if (value == null)
        {
            return null;
        }

        Button button =
            value as Button;

        if (button != null)
        {
            return button;
        }

        GameObject go =
            value as GameObject;

        if (go != null)
        {
            return go.GetComponent<Button>();
        }

        Component component =
            value as Component;

        if (component != null)
        {
            return component.GetComponent<Button>();
        }

        return null;
    }

    // =========================================================
    // GENERIC UI SEARCH
    // =========================================================

    private Toggle FindToggleFromMember(
        object owner,
        string memberName
    )
    {
        if (owner == null)
        {
            return null;
        }

        Type type =
            owner.GetType();

        FieldInfo field =
            type.GetField(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        if (field != null)
        {
            return ConvertToToggle(
                field.GetValue(owner)
            );
        }

        return null;
    }

    private Toggle ConvertToToggle(
        object value
    )
    {
        if (value == null)
        {
            return null;
        }

        Toggle toggle =
            value as Toggle;

        if (toggle != null)
        {
            return toggle;
        }

        GameObject go =
            value as GameObject;

        if (go != null)
        {
            return go.GetComponent<Toggle>();
        }

        Component component =
            value as Component;

        if (component != null)
        {
            return component.GetComponent<Toggle>();
        }

        return null;
    }

    private Slider FindSliderByText(
        params string[] values
    )
    {
        Slider[] sliders =
            FindObjectsByType<Slider>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (
            Slider slider
            in sliders
        )
        {
            if (slider == null ||
                !slider.gameObject.activeInHierarchy)
            {
                continue;
            }

            TMP_Text[] texts =
                slider.GetComponentsInParent<
                    TMP_Text
                >(true);

            foreach (
                TMP_Text text
                in texts
            )
            {
                string value =
                    text.text
                        .Trim()
                        .ToUpperInvariant();

                foreach (
                    string search
                    in values
                )
                {
                    if (
                        value.Contains(
                            search.ToUpperInvariant()
                        )
                    )
                    {
                        return slider;
                    }
                }
            }
        }

        return null;
    }

    private Toggle FindToggleByText(
        params string[] values
    )
    {
        Toggle[] toggles =
            FindObjectsByType<Toggle>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (
            Toggle toggle
            in toggles
        )
        {
            if (toggle == null ||
                !toggle.gameObject.activeInHierarchy)
            {
                continue;
            }

            TMP_Text[] texts =
                toggle.GetComponentsInChildren<
                    TMP_Text
                >(true);

            foreach (
                TMP_Text text
                in texts
            )
            {
                string value =
                    text.text
                        .Trim()
                        .ToUpperInvariant();

                foreach (
                    string search
                    in values
                )
                {
                    if (
                        value.Contains(
                            search.ToUpperInvariant()
                        )
                    )
                    {
                        return toggle;
                    }
                }
            }
        }

        return null;
    }

    // =========================================================
    // UPGRADE SEARCH
    // =========================================================

    private Button FindUpgradeButton(
        GameObject root
    )
    {
        if (root == null)
        {
            return null;
        }

        Button[] buttons =
            root.GetComponentsInChildren<
                Button
            >(true);

        foreach (
            Button button
            in buttons
        )
        {
            if (button == null ||
                !button.gameObject.activeInHierarchy)
            {
                continue;
            }

            TMP_Text text =
                button.GetComponentInChildren<
                    TMP_Text
                >(true);

            if (text == null)
            {
                continue;
            }

            string value =
                text.text
                    .Trim()
                    .ToUpperInvariant();

            if (
                value.Contains("УЛУЧШ") ||
                value.Contains("ПРОКАЧ") ||
                value.Contains("UPGRADE")
            )
            {
                return button;
            }
        }

        return null;
    }

    private Button FindFirstMeaningfulButton(
        GameObject root
    )
    {
        if (root == null)
        {
            return null;
        }

        Button[] buttons =
            root.GetComponentsInChildren<
                Button
            >(true);

        foreach (
            Button button
            in buttons
        )
        {
            if (
                button != null &&
                button.gameObject.activeInHierarchy
            )
            {
                TMP_Text text =
                    button.GetComponentInChildren<
                        TMP_Text
                    >(true);

                if (text != null &&
                    !string.IsNullOrWhiteSpace(
                        text.text
                    ))
                {
                    return button;
                }
            }
        }

        return null;
    }

    private Button FindFirstCharacterButton(
        GameObject root
    )
    {
        if (root == null)
        {
            return null;
        }

        Button[] buttons =
            root.GetComponentsInChildren<
                Button
            >(true);

        foreach (
            Button button
            in buttons
        )
        {
            if (
                button == null ||
                !button.gameObject.activeInHierarchy
            )
            {
                continue;
            }

            TMP_Text text =
                button.GetComponentInChildren<
                    TMP_Text
                >(true);

            if (text == null)
            {
                continue;
            }

            string value =
                text.text
                    .Trim()
                    .ToUpperInvariant();

            if (
                value.Contains("ВЫБРАТЬ") ||
                value.Contains("КУПИТЬ")
            )
            {
                return button;
            }
        }

        return null;
    }

    // =========================================================
    // UTILITY BUTTON
    // =========================================================

    private Button FindUtilityButton(
        string requiredAction
    )
    {
        MainMenuUtilityButton3D[] utilities =
            FindObjectsByType<
                MainMenuUtilityButton3D
            >(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (
            MainMenuUtilityButton3D utility
            in utilities
        )
        {
            if (utility == null)
            {
                continue;
            }

            string action =
                GetActionValue(
                    utility
                );

            if (
                !string.Equals(
                    action,
                    requiredAction,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                continue;
            }

            Button button =
                utility.GetComponent<Button>();

            if (button == null)
            {
                button =
                    utility.GetComponentInChildren<
                        Button
                    >(true);
            }

            if (button != null)
            {
                return button;
            }
        }

        return null;
    }

    private string GetActionValue(
        object target
    )
    {
        Type type =
            target.GetType();

        FieldInfo[] fields =
            type.GetFields(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        foreach (
            FieldInfo field
            in fields
        )
        {
            if (
                !field.Name
                    .ToLowerInvariant()
                    .Contains("action")
            )
            {
                continue;
            }

            object value =
                field.GetValue(target);

            if (value != null)
            {
                return value.ToString();
            }
        }

        return string.Empty;
    }

    // =========================================================
    // PREFS
    // =========================================================

    private bool IsCompleted()
    {
        return PlayerPrefs.GetInt(
            COMPLETED_KEY,
            0
        ) == 1;
    }

    private bool IsUpgradeTutorialCompleted()
    {
        return PlayerPrefs.GetInt(
            UPGRADE_COMPLETED_KEY,
            0
        ) == 1;
    }

    private void CompleteTutorial()
    {
        RestoreSelectables();

        PlayerPrefs.SetInt(
            COMPLETED_KEY,
            1
        );

        PlayerPrefs.DeleteKey(
            STARTED_KEY
        );

        PlayerPrefs.Save();

        DestroyOverlay();

        Debug.Log(
            "[MainMenuTutorial] " +
            "Полное обучение меню завершено."
        );
    }

    // =========================================================
    // RECT
    // =========================================================

    private void StretchFull(
        RectTransform rect
    )
    {
        rect.anchorMin =
            Vector2.zero;

        rect.anchorMax =
            Vector2.one;

        rect.offsetMin =
            Vector2.zero;

        rect.offsetMax =
            Vector2.zero;

        rect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );
    }
}