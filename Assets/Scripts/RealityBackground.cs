using UnityEngine;

public class RealityGradientBackground : MonoBehaviour
{
    [Header("Reality A (Warm)")]
    public Color topColorA = new Color32(42, 21, 5, 255);
    public Color bottomColorA = new Color32(74, 37, 5, 255);

    [Header("Reality B (Cool)")]
    public Color topColorB = new Color32(3, 24, 33, 255);
    public Color bottomColorB = new Color32(5, 40, 56, 255);

    [Header("Settings")]
    public float transitionSpeed = 3.0f;
    public int sortingOrder = -100;

    private Color currentTop;
    private Color currentBottom;
    private Color targetTop;
    private Color targetBottom;

    private Texture2D backgroundTexture;
    private SpriteRenderer backgroundSprite;
    private Camera cam;

    void Reset()
    {
        topColorA = new Color32(42, 21, 5, 255);
        bottomColorA = new Color32(74, 37, 5, 255);
        topColorB = new Color32(3, 24, 33, 255);
        bottomColorB = new Color32(5, 40, 56, 255);
    }

    void Start()
    {
        InitializeGradient();
        SubscribeToReality();
    }

    void OnDestroy()
    {
        UnsubscribeFromReality();
    }

    void Update()
    {
        ScaleBackground();
        UpdateColors();
        ApplyTexture();
    }

    private void InitializeGradient()
    {
        GameObject bgObject = new GameObject("ProceduralGradient");
        bgObject.transform.SetParent(transform);

        backgroundSprite = bgObject.AddComponent<SpriteRenderer>();
        backgroundSprite.sortingOrder = sortingOrder;

        backgroundTexture = new Texture2D(1, 2)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Sprite sprite = Sprite.Create(backgroundTexture, new Rect(0, 0, 1, 2), new Vector2(0.5f, 0.5f), 1f);
        backgroundSprite.sprite = sprite;

        cam = GetComponent<Camera>();
    }

    private void SubscribeToReality()
    {
        if (RealityManager.Instance != null)
        {
            RealityManager.Instance.OnRealityChange += UpdateTargets;
            UpdateTargets(RealityManager.Instance.currentReality);

            currentTop = targetTop;
            currentBottom = targetBottom;
        }
    }

    private void UnsubscribeFromReality()
    {
        if (RealityManager.Instance != null)
        {
            RealityManager.Instance.OnRealityChange -= UpdateTargets;
        }
    }

    private void UpdateTargets(RealityManager.Reality reality)
    {
        if (reality == RealityManager.Reality.A)
        {
            targetTop = topColorA;
            targetBottom = bottomColorA;
        }
        else
        {
            targetTop = topColorB;
            targetBottom = bottomColorB;
        }
    }

    private void ScaleBackground()
    {
        if (cam == null || backgroundSprite == null) return;

        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;

        backgroundSprite.transform.localScale = new Vector3(width, height / 2f, 1);
        backgroundSprite.transform.localPosition = new Vector3(0, 0, 10);
    }

    private void UpdateColors()
    {
        currentTop = Color.Lerp(currentTop, targetTop, Time.deltaTime * transitionSpeed);
        currentBottom = Color.Lerp(currentBottom, targetBottom, Time.deltaTime * transitionSpeed);
    }

    private void ApplyTexture()
    {
        backgroundTexture.SetPixel(0, 0, currentBottom);
        backgroundTexture.SetPixel(0, 1, currentTop);
        backgroundTexture.Apply();
    }
}