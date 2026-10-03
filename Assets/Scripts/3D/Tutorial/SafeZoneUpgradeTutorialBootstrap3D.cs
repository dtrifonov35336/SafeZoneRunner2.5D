using UnityEngine;
using UnityEngine.SceneManagement;

public class SafeZoneUpgradeTutorialBootstrap3D : MonoBehaviour
{
    private static SafeZoneUpgradeTutorialBootstrap3D instance;

    private const string MAIN_ROAD =
        "MainRoad";

    private const string MAIN_MENU =
        "MainMenu";

    private string previousScene = "";

    private void Awake()
    {
        if (
            instance != null &&
            instance != this
        )
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);

        /*
         * ВАЖНО:
         * создаём Tutorial ДО подписки Bootstrap
         * на sceneLoaded.
         *
         * Благодаря этому сам Tutorial уже слушает
         * sceneLoaded к моменту перехода MainRoad → MainMenu.
         */
        EnsureTutorialExists();

        previousScene =
            SceneManager
                .GetActiveScene()
                .name;

        SceneManager.sceneLoaded +=
            OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -=
                OnSceneLoaded;

            instance = null;
        }
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        string loadedScene =
            scene.name;

        /*
         * Главное исправление:
         *
         * MainRoad → MainMenu
         *
         * Pending ставится непосредственно
         * в момент загрузки MainMenu, после чего
         * Tutorial запускается напрямую.
         */
        if (
            loadedScene ==
            MAIN_MENU &&
            previousScene ==
            MAIN_ROAD
        )
        {
            if (
                PlayerPrefs.GetInt(
                    "SafeZoneUpgradeTutorialCompleted",
                    0
                ) == 0
            )
            {
                SafeZoneUpgradeTutorial3D
                    .MarkTutorialPending();

                /*
                 * Не надеемся на порядок
                 * sceneLoaded-событий.
                 *
                 * Запускаем Tutorial напрямую.
                 */
                SafeZoneUpgradeTutorial3D
                    .StartPendingFromMainMenu();
            }
        }

        previousScene =
            loadedScene;
    }

    private void EnsureTutorialExists()
    {
        SafeZoneUpgradeTutorial3D tutorial =
            Object.FindFirstObjectByType<
                SafeZoneUpgradeTutorial3D
            >();

        if (tutorial != null)
        {
            return;
        }

        GameObject go =
            new GameObject(
                "SafeZoneUpgradeTutorial3D"
            );

        go.AddComponent<
            SafeZoneUpgradeTutorial3D
        >();
    }
}