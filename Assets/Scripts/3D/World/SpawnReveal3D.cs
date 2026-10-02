using System.Collections;
using UnityEngine;

public class SpawnReveal3D : MonoBehaviour
{
    [Header("Reveal")]
    [SerializeField] private float revealZ = 40f;

    [Header("Fade")]
    public float fadeDuration = 0.35f;

    private Renderer[] renderers;
    private Collider[] colliders;
    private SpriteRenderer[] spriteRenderers;

    private Color[] originalSpriteColors;

    private bool revealed;
    private bool initialized;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        CacheComponents();
        PrepareHidden();
    }

    private void Start()
    {
        // Важно:
        // Initialize / InitializeSynchronized вызываются
        // спавнером сразу после AddComponent.
        //
        // Поэтому Start ничего дополнительно не меняет.
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        if (revealed)
        {
            return;
        }

        if (transform.position.z <= revealZ)
        {
            Reveal();
        }
    }

    // =========================================================
    // INITIALIZE
    // =========================================================

    public void Initialize(float targetRevealZ)
    {
        revealZ = targetRevealZ;

        initialized = true;
        revealed = false;

        PrepareHidden();
    }

    // =========================================================
    // SYNCHRONIZED INITIALIZE
    // =========================================================

    public void InitializeSynchronized(
        float objectSpawnZ,
        float referenceSpawnZ,
        float referenceRevealZ
    )
    {
        float offset =
            objectSpawnZ -
            referenceSpawnZ;

        revealZ =
            referenceRevealZ +
            offset;

        initialized = true;
        revealed = false;

        PrepareHidden();
    }

    // =========================================================
    // CACHE
    // =========================================================

    private void CacheComponents()
    {
        renderers =
            GetComponentsInChildren<Renderer>(true);

        colliders =
            GetComponentsInChildren<Collider>(true);

        spriteRenderers =
            GetComponentsInChildren<SpriteRenderer>(true);

        originalSpriteColors =
            new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null)
            {
                continue;
            }

            originalSpriteColors[i] =
                spriteRenderers[i].color;
        }
    }

    // =========================================================
    // HIDDEN
    // =========================================================

    private void PrepareHidden()
    {
        revealed = false;

        if (renderers != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                {
                    continue;
                }

                renderers[i].enabled = false;
            }
        }

        if (colliders != null)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == null)
                {
                    continue;
                }

                colliders[i].enabled = false;
            }
        }

        if (spriteRenderers != null)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] == null)
                {
                    continue;
                }

                Color color =
                    originalSpriteColors[i];

                color.a = 0f;

                spriteRenderers[i].color =
                    color;
            }
        }
    }

    // =========================================================
    // REVEAL
    // =========================================================

    private void Reveal()
    {
        if (revealed)
        {
            return;
        }

        revealed = true;

        if (renderers != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                {
                    continue;
                }

                renderers[i].enabled = true;
            }
        }

        if (colliders != null)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == null)
                {
                    continue;
                }

                colliders[i].enabled = true;
            }
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        if (
            fadeDuration > 0f &&
            spriteRenderers != null &&
            spriteRenderers.Length > 0
        )
        {
            fadeCoroutine =
                StartCoroutine(FadeIn());
        }
        else
        {
            RestoreOriginalColors();
        }
    }

    // =========================================================
    // FADE
    // =========================================================

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    fadeDuration
                );

            for (
                int i = 0;
                i < spriteRenderers.Length;
                i++
            )
            {
                if (spriteRenderers[i] == null)
                {
                    continue;
                }

                Color color =
                    originalSpriteColors[i];

                color.a *= t;

                spriteRenderers[i].color =
                    color;
            }

            yield return null;
        }

        RestoreOriginalColors();

        fadeCoroutine = null;
    }

    // =========================================================
    // RESTORE
    // =========================================================

    private void RestoreOriginalColors()
    {
        if (spriteRenderers == null)
        {
            return;
        }

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null)
            {
                continue;
            }

            spriteRenderers[i].color =
                originalSpriteColors[i];
        }
    }

    // =========================================================
    // PUBLIC
    // =========================================================

    public float GetRevealZ()
    {
        return revealZ;
    }

    public void SetRevealZ(float value)
    {
        revealZ = value;
        initialized = true;
    }

    public bool IsRevealed()
    {
        return revealed;
    }

    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
    }
}