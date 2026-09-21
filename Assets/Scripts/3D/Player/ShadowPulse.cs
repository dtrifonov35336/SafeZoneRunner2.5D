using UnityEngine;

public class ShadowPulse : MonoBehaviour
{
    public Transform target;
    public float baseScaleX = 0.7f;
    public float baseScaleY = 1.5f;
    public float pulseAmount = 0.1f;
    public float pulseSpeed = 8f;

    private Material mat;

    private void Start()
    {
        if (target == null) target = transform.parent;
        mat = GetComponent<Renderer>().material;
    }

    private void Update()
    {
        if (target == null || mat == null) return;

        // Двигается за игроком, но без прыжковой Y
        Vector3 pos = target.position;
        pos.y = 0.01f;
        transform.position = pos;

        // Пульсация размера
        float p = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = new Vector3(baseScaleX * p, baseScaleY * p, 1f);

        // Прозрачность по высоте игрока
        float h = target.position.y - 0.65f;
        float alpha = Mathf.Clamp01(1f - h * 2f) * 120f / 255f;
        Color c = mat.color;
        c.a = alpha;
        mat.color = c;
    }
}