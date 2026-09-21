using UnityEngine;

public class SideDecorationMover : MonoBehaviour
{
    public float speed = 15f;
    public float despawnZ = -10f;

    private void Update()
    {
        Vector3 p = transform.position;
        p.z -= speed * Time.deltaTime;
        transform.position = p;

        if (p.z <= despawnZ)
            Destroy(gameObject);
    }
}