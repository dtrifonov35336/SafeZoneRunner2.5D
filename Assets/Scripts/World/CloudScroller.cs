using UnityEngine;

public class CloudScroller : MonoBehaviour
{
    public float scrollSpeed = 0.3f;
    public float loopHeight = 8f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.position += new Vector3(0, -scrollSpeed * Time.deltaTime, 0);

        if (transform.position.y < startPosition.y - loopHeight)
        {
            transform.position = new Vector3(transform.position.x, startPosition.y, transform.position.z);
        }
    }
}