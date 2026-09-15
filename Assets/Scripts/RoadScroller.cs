using UnityEngine;

public class RoadScroller : MonoBehaviour
{
    public Transform road1;
    public Transform road2;
    public float scrollSpeed = 3f;

    private float roadHeight;

    private void Start()
    {
        // Берём реальную высоту спрайта, а не угадываем число
        roadHeight = road1.GetComponent<SpriteRenderer>().bounds.size.y;
    }

    private void Update()
    {
        Vector3 moveDown = Vector3.down * scrollSpeed * Time.deltaTime;

        road1.position += moveDown;
        road2.position += moveDown;

        if (road1.position.y <= -roadHeight)
        {
            road1.position = new Vector3(
                road1.position.x,
                road2.position.y + roadHeight,
                road1.position.z
            );
        }

        if (road2.position.y <= -roadHeight)
        {
            road2.position = new Vector3(
                road2.position.x,
                road1.position.y + roadHeight,
                road2.position.z
            );
        }
    }
}