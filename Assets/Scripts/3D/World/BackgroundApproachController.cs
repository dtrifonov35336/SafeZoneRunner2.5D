using UnityEngine;

public class BackgroundApproachController : MonoBehaviour
{
    [Header("Run Manager")]
    public RunManager runManager;

    [Header("Фоны")]
    public Transform skyQuad;
    public Transform forestLeft;
    public Transform forestRight;

    [Header("Приближение SkyQuad")]
    [Tooltip("На сколько единиц SkyQuad приблизится к камере за весь забег.")]
    public float skyApproachDistance = 15f;

    [Header("Приближение леса")]
    [Tooltip("На сколько единиц левый и правый лес приблизятся к камере.")]
    public float forestApproachDistance = 30f;

    [Header("Плавность")]
    [Tooltip("0 = линейное движение, 1 = плавное ускорение/замедление.")]
    [Range(0f, 1f)]
    public float smoothing = 0.75f;

    private Vector3 skyStartPosition;
    private Vector3 forestLeftStartPosition;
    private Vector3 forestRightStartPosition;

    private float skyStartZ;
    private float forestLeftStartZ;
    private float forestRightStartZ;

    private float skyStartY;
    private float forestLeftStartY;
    private float forestRightStartY;

    private void Start()
    {
        if (skyQuad != null)
        {
            skyStartPosition = skyQuad.position;

            skyStartZ = skyQuad.position.z;
            skyStartY = skyQuad.position.y;
        }

        if (forestLeft != null)
        {
            forestLeftStartPosition =
                forestLeft.position;

            forestLeftStartZ =
                forestLeft.position.z;

            forestLeftStartY =
                forestLeft.position.y;
        }

        if (forestRight != null)
        {
            forestRightStartPosition =
                forestRight.position;

            forestRightStartZ =
                forestRight.position.z;

            forestRightStartY =
                forestRight.position.y;
        }
    }

    private void Update()
    {
        if (runManager == null)
            return;

        // Бесконечный забег:
        // фон вообще не приближается.
        if (runManager.infiniteRun)
        {
            return;
        }

        if (runManager.runDuration <= 0f)
            return;

        float progress =
            runManager.GetRunTime() /
            runManager.runDuration;

        progress =
            Mathf.Clamp01(progress);

        // Плавное движение:
        // медленно начинается,
        // плавно ускоряется,
        // плавно замедляется.
        float smoothProgress =
            Mathf.SmoothStep(
                0f,
                1f,
                progress
            );

        smoothProgress =
            Mathf.Lerp(
                progress,
                smoothProgress,
                smoothing
            );

        UpdateSky(smoothProgress);
        UpdateForest(smoothProgress);
    }

    private void UpdateSky(float progress)
    {
        if (skyQuad == null)
            return;

        Vector3 position =
            skyQuad.position;

        // Только Z.
        // X и Y остаются неизменными.
        position.z =
            skyStartZ -
            skyApproachDistance *
            progress;

        position.y =
            skyStartY;

        skyQuad.position =
            position;
    }

    private void UpdateForest(float progress)
    {
        if (forestLeft != null)
        {
            Vector3 position =
                forestLeft.position;

            position.z =
                forestLeftStartZ -
                forestApproachDistance *
                progress;

            position.y =
                forestLeftStartY;

            forestLeft.position =
                position;
        }

        if (forestRight != null)
        {
            Vector3 position =
                forestRight.position;

            position.z =
                forestRightStartZ -
                forestApproachDistance *
                progress;

            position.y =
                forestRightStartY;

            forestRight.position =
                position;
        }
    }

    public void ResetBackground()
    {
        if (skyQuad != null)
            skyQuad.position =
                skyStartPosition;

        if (forestLeft != null)
            forestLeft.position =
                forestLeftStartPosition;

        if (forestRight != null)
            forestRight.position =
                forestRightStartPosition;
    }
}