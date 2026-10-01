using System.Collections.Generic;
using UnityEngine;

public class MainMenuModalManager3D : MonoBehaviour
{
    public static MainMenuModalManager3D Instance
    {
        get;
        private set;
    }

    [Header("Main Menu")]
    [SerializeField]
    private GameObject background;

    [Header("Safe Area")]
    [SerializeField]
    private GameObject safeArea;

    private readonly List<GameObject> hiddenObjects =
        new List<GameObject>();

    private GameObject activeModal;

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }

        Instance =
            this;

        ResolveReferences();
    }

    private void Start()
    {
        ResolveReferences();
    }

    private void ResolveReferences()
    {
        if (background == null)
        {
            Transform found =
                transform.Find(
                    "Background"
                );

            if (found != null)
            {
                background =
                    found.gameObject;
            }
        }

        if (safeArea == null)
        {
            Transform found =
                transform.Find(
                    "SafeArea"
                );

            if (found != null)
            {
                safeArea =
                    found.gameObject;
            }
        }
    }

    // =========================================================
    // OPEN
    // =========================================================

    public void OpenModal(
        GameObject modal
    )
    {
        if (modal == null)
            return;

        ResolveReferences();

        // Если уже открыто другое окно,
        // сначала полностью закрываем его.
        if (
            activeModal != null &&
            activeModal != modal
        )
        {
            CloseModal();
        }

        hiddenObjects.Clear();

        // =====================================================
        // СКРЫВАЕМ ОСНОВНОЕ МЕНЮ
        // =====================================================

        if (safeArea != null)
        {
            safeArea.SetActive(false);
        }

        // =====================================================
        // СКРЫВАЕМ ВСЕ ПРЯМЫЕ ДЕТИ CANVAS,
        // КРОМЕ ФОНА И ОТКРЫВАЕМОГО ОКНА
        // =====================================================

        for (
            int i = 0;
            i < transform.childCount;
            i++
        )
        {
            GameObject child =
                transform.GetChild(i).gameObject;

            if (
                child == background ||
                child == modal
            )
            {
                continue;
            }

            if (
                child.activeSelf
            )
            {
                hiddenObjects.Add(
                    child
                );

                child.SetActive(false);
            }
        }

        activeModal =
            modal;

        modal.SetActive(true);

        modal.transform.SetAsLastSibling();
    }

    // =========================================================
    // CLOSE
    // =========================================================

    public void CloseModal(
        GameObject modal = null
    )
    {
        GameObject target =
            modal != null
                ? modal
                : activeModal;

        if (target != null)
        {
            target.SetActive(false);
        }

        for (
            int i = 0;
            i < hiddenObjects.Count;
            i++
        )
        {
            GameObject objectToRestore =
                hiddenObjects[i];

            if (
                objectToRestore != null
            )
            {
                objectToRestore.SetActive(true);
            }
        }

        hiddenObjects.Clear();

        activeModal =
            null;

        if (safeArea != null)
        {
            safeArea.SetActive(true);
        }
    }

    public bool IsModalOpened()
    {
        return activeModal != null;
    }
}