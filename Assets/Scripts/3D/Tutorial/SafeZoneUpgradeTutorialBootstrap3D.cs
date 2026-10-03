using UnityEngine;
using UnityEngine.SceneManagement;

public class SafeZoneUpgradeTutorialBootstrap3D : MonoBehaviour
{
    private static SafeZoneUpgradeTutorialBootstrap3D instance;

    private const string MAIN_ROAD =
        "MainRoad";

    private const string MAIN_MENU =
        "MainMenu";

    private string previousScene =
        "";

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

        SceneManager.sceneLoaded +=
            OnSceneLoaded;
    }

    private void Start()
    {
        previousScene =
            SceneManager.GetActiveScene().name;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -=
                OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        string loadedScene =
            scene.name;

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

                SafeZoneUpgradeTutorial3D tutorial =
                    FindObjectOfType<
                        SafeZoneUpgradeTutorial3D
                    >();

                if (tutorial == null)
                {
                    GameObject go =
                        new GameObject(
                            "SafeZoneUpgradeTutorial3D"
                        );

                    tutorial =
                        go.AddComponent<
                            SafeZoneUpgradeTutorial3D
                        >();
                }
            }
        }

        previousScene =
            loadedScene;
    }
}