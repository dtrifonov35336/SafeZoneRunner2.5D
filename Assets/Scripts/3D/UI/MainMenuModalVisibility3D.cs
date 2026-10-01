using System.Collections.Generic;
using UnityEngine;

public class MainMenuModalVisibility3D :
    MonoBehaviour
{
    public static MainMenuModalVisibility3D Instance
    {
        get;
        private set;
    }

    [Header("Фон")]
    [SerializeField]
    private string backgroundObjectName =
        "Background";

    private readonly List<
        GameObject
    > hiddenObjects =
        new List<GameObject>();

    private bool modalOpened;

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
    }

    // =========================================================
    // OPEN
    // =========================================================

    public void OpenModal(
        GameObject modalWindow
    )
    {
        if (modalWindow == null)
            return;

        hiddenObjects.Clear();

        Transform canvas =
            transform;

        for (
            int i = 0;
            i < canvas.childCount;
            i++
        )
        {
            Transform child =
                canvas.GetChild(i);

            if (
                child == null ||
                child.gameObject == null
            )
            {
                continue;
            }

            GameObject childObject =
                child.gameObject;

            // -------------------------------------------------
            // ФОН НЕ ТРОГАЕМ
            // -------------------------------------------------

            if (
                childObject.name ==
                backgroundObjectName
            )
            {
                continue;
            }

            // -------------------------------------------------
            // ОТКРЫТОЕ ОКНО НЕ ТРОГАЕМ
            // -------------------------------------------------

            if (
                childObject ==
                modalWindow
            )
            {
                continue;
            }

            // -------------------------------------------------
            // СКРЫВАЕМ ОСТАЛЬНОЙ UI
            // -------------------------------------------------

            if (
                childObject.activeSelf
            )
            {
                hiddenObjects.Add(
                    childObject
                );

                childObject.SetActive(
                    false
                );
            }
        }

        modalOpened =
            true;

        modalWindow.SetActive(
            true
        );

        modalWindow.transform.SetAsLastSibling();
    }

    // =========================================================
    // CLOSE
    // =========================================================

    public void CloseModal()
    {
        if (!modalOpened)
            return;

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
                objectToRestore.SetActive(
                    true
                );
            }
        }

        hiddenObjects.Clear();

        modalOpened =
            false;
    }

    public bool IsModalOpened()
    {
        return modalOpened;
    }
}