using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MenuButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Targets")]
    public Image backgroundFill;
    public TextMeshProUGUI buttonText;

    [Header("Style Settings (Video Match)")]
    public Color normalBgColor = Color.black;
    public Color normalTextColor = Color.white;

    [Header("Hover Glow")]
    [ColorUsage(true, true)]
    public Color hoverBgColor = new Color(6f, 6f, 6f, 1f); // Intensity 6, Alpha 1
    public Color hoverTextColor = Color.black;

    [Header("Optional Glow Material")]
    public Material glowMaterial;
    private Material defaultMaterial;

    [Header("Audio Settings")]
    public bool playHoverSound = false;
    public bool playClickSound = true;

    void Start()
    {
        // --- AUTO-SETUP ---
        if (backgroundFill == null) backgroundFill = GetComponent<Image>();
        if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();

        // Save default material
        if (backgroundFill != null) defaultMaterial = backgroundFill.material;

        // --- FIX INTERACTION ---
        if (buttonText != null) buttonText.raycastTarget = false;
        if (backgroundFill != null) backgroundFill.raycastTarget = true;

        // --- AUTO-FIX GLOW ---
        // If no material assigned, create a temporary one that supports HDR colors (Glow)
        if (glowMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                glowMaterial = new Material(shader);
                glowMaterial.name = "AutoGlow_Mat";
            }
            else
            {
                Debug.LogWarning("Could not find 'Sprites/Default' shader. Glow might not work without a custom material.");
            }
        }

        ResetVisuals();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (backgroundFill != null)
        {
            backgroundFill.color = hoverBgColor;

            if (glowMaterial != null) backgroundFill.material = glowMaterial;
        }

        if (buttonText != null) buttonText.color = hoverTextColor;

        if (playHoverSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySwitch();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (playClickSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClick();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetVisuals();
    }

    void OnDisable()
    {
        ResetVisuals();
    }

    void ResetVisuals()
    {
        if (backgroundFill != null)
        {
            backgroundFill.color = normalBgColor;
            // Restore default (usually non-glowing UI material)
            backgroundFill.material = defaultMaterial;
        }
        if (buttonText != null) buttonText.color = normalTextColor;
    }
}