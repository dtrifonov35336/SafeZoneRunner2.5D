using UnityEngine;

public class Billboard : MonoBehaviour
{
    [Tooltip("Галочка: если спрайт повёрнут задом, включи это")]
    public bool flip180 = false;

    private Camera cam;

    private void Start() { cam = Camera.main; }

    private void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 dir = transform.position - cam.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
        if (flip180) look *= Quaternion.Euler(0, 180f, 0);

        transform.rotation = look;
    }
}