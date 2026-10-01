using System.Collections.Generic;
using UnityEngine;

public class MainMenuModalManager3D : MonoBehaviour
{
    public static MainMenuModalManager3D Instance { get; private set; }

    [SerializeField]
    private GameObject background;

    [SerializeField]
    private GameObject safeArea;

    private readonly List<GameObject> hiddenObjects =
        new List<GameObject>();

    private GameObject activeModal;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ResolveReferences();
    }

    private void ResolveReferences()
    {
        if (background == null)
        {
            Transform found =
                transform.Find("Background");

            if (found != null)
                background = found.gameObject;
        }

        if (safeArea == null)
        {
            Transform found =
                transform.Find("SafeArea");

            if (found != null)
                safeArea = found.gameObject;
        }
    }

    public void OpenModal(GameObject modal)
    {
        if (modal == null)
            return;

        ResolveReferences();

        if (activeModal != null &&
            activeModal != modal)
        {
            CloseModal(activeModal);
        }

        hiddenObjects.Clear();

        for (int i = 0;
             i < transform.childCount;
             i++)
        {
            GameObject child =
                transform.GetChild(i).gameObject;

            if (child == background ||
                child == modal ||
                child == gameObject)
            {
                continue;
            }

            if (child.activeSelf)
            {
                hiddenObjects.Add(child);
                child.SetActive(false);
            }
        }

        activeModal = modal;

        modal.SetActive(true);
        modal.transform.SetAsLastSibling();
    }

    public void CloseModal(GameObject modal = null)
    {
        GameObject target =
            modal != null
                ? modal
                : activeModal;

        if (target != null)
            target.SetActive(false);

        for (int i = 0;
             i < hiddenObjects.Count;
             i++)
        {
            if (hiddenObjects[i] != null)
                hiddenObjects[i].SetActive(true);
        }

        hiddenObjects.Clear();
        activeModal = null;
    }

    public bool IsModalOpen()
    {
        return activeModal != null;
    }
}