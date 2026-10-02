using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class UIButtonSound3D : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button =
            GetComponent<Button>();
    }

    private void OnEnable()
    {
        Bind();
    }

    private void OnDisable()
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(
            PlayClickSound
        );
    }

    public void Bind()
    {
        if (button == null)
        {
            button =
                GetComponent<Button>();
        }

        if (button == null)
            return;

        button.onClick.RemoveListener(
            PlayClickSound
        );

        button.onClick.AddListener(
            PlayClickSound
        );
    }

    private void PlayClickSound()
    {
        if (
            AudioManager3D.Instance == null
        )
        {
            return;
        }

        AudioManager3D.Instance
            .PlayMenuClick();
    }
}