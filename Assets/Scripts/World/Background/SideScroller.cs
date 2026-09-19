using UnityEngine;

public class SideScroller : MonoBehaviour
{
    public enum Side { Left, Right }

    [Header("Направление")]
    public Side side = Side.Left;

    [Header("Скорость движения в сторону")]
    public float sideSpeed = 0.8f;

    [Header("Скорость движения вниз")]
    public float downSpeed = 1.5f;

    [Header("Зацикливание")]
    [Tooltip("Ширина экрана в юнитах. При уходе за эту границу — переставляется в центр")]
    public float wrapWidth = 3.5f;

    [Tooltip("Y-позиция, при которой спрайт возвращается наверх")]
    public float resetY = 8f;

    [Header("Ссылки")]
    public Transform playerTransform;

    private float startX;
    private float startY;

    private void Start()
    {
        startX = transform.position.x;
        startY = transform.position.y;

        if (playerTransform == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }
    }

    private void Update()
    {
        // Движение в сторону + вниз
        float xMove = sideSpeed * Time.deltaTime;
        float yMove = downSpeed * Time.deltaTime;

        // Влево или вправо
        if (side == Side.Left)
            xMove = -Mathf.Abs(xMove);
        else
            xMove = Mathf.Abs(xMove);

        transform.position += new Vector3(xMove, -yMove, 0);

        // Зацикливание по X
        if (side == Side.Left && transform.position.x < -wrapWidth)
        {
            transform.position = new Vector3(wrapWidth, resetY, transform.position.z);
        }
        else if (side == Side.Right && transform.position.x > wrapWidth)
        {
            transform.position = new Vector3(-wrapWidth, resetY, transform.position.z);
        }

        // Зацикливание по Y (если X не сработал)
        if (transform.position.y < -wrapWidth)
        {
            transform.position = new Vector3(
                side == Side.Left ? wrapWidth : -wrapWidth,
                resetY,
                transform.position.z
            );
        }
    }
}