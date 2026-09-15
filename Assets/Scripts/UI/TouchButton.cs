using UnityEngine;
using UnityEngine.EventSystems;

public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum ButtonType { Left, Right, Jump }

    public ButtonType type;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (TouchControls.Instance == null)
        {
            Debug.LogWarning("[TouchButton] TouchControls не найден в сцене!");
            return;
        }

        switch (type)
        {
            case ButtonType.Left: TouchControls.Instance.SetLeft(true); break;
            case ButtonType.Right: TouchControls.Instance.SetRight(true); break;
            case ButtonType.Jump: TouchControls.Instance.TriggerJump(); break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (TouchControls.Instance == null) return;

        switch (type)
        {
            case ButtonType.Left: TouchControls.Instance.SetLeft(false); break;
            case ButtonType.Right: TouchControls.Instance.SetRight(false); break;
        }
    }
}