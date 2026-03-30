using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class RealityManager : MonoBehaviour
{
    public static RealityManager Instance;

    public enum Reality { A, B }

    public Reality currentReality = Reality.A;

    public event Action<Reality> OnRealityChange;

    [Header("Settings")]
    public float toggleCooldown = 0.5f;
    
    [Header("Effects")]
    public string shiftParticlePool = "RealityShift";
    public Transform playerTransform;

    [Header("Particle Colors")]
    public Color particleColorA = new Color(1f, 0.5f, 0f);
    public Color particleColorB = new Color(0f, 0.9f, 1f);

    private float lastToggleTime;

    // Input action resolved once from the global InputActionAsset.
    private InputAction shiftAction;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            currentReality = Reality.A;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Resolve the Sprint action from the global registered asset.
        // Bindings: LeftShift and Q are added below if not already present.
        shiftAction = InputSystem.actions.FindAction("Player/Sprint");
        if (shiftAction != null)
        {
            shiftAction.performed += OnShiftPerformed;
            shiftAction.Enable();
        }
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (shiftAction != null)
        {
            shiftAction.performed -= OnShiftPerformed;
        }
    }

    void Start()
    {
        ResetReality();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        playerTransform = null;
        ResetReality();
    }

    private void ResetReality()
    {
        currentReality = Reality.A;
        BroadcastReality();
    }

    private void BroadcastReality()
    {
        OnRealityChange?.Invoke(currentReality);
        
        if (OnRealityChange == null)
        {
            Invoke(nameof(ForceUpdate), 0.05f);
        }
    }

    private void ForceUpdate()
    {
        OnRealityChange?.Invoke(currentReality);
    }

    private void OnShiftPerformed(InputAction.CallbackContext ctx)
    {
        if (Time.time >= lastToggleTime + toggleCooldown)
        {
            ToggleReality();
        }
    }

    void Update()
    {
        // Update() is kept for mobile shift polling only.
        // Desktop shift is handled via the InputAction callback above.
#if UNITY_ANDROID || UNITY_IOS
        // Mobile shift is consumed by PlayerController and forwarded via
        // RealityManager.Instance.ToggleReality() directly — no polling needed here.
#endif
    }

    public void ToggleReality()
    {
        lastToggleTime = Time.time;

        currentReality = (currentReality == Reality.A) ? Reality.B : Reality.A;

        OnRealityChange?.Invoke(currentReality);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySwitch();
        }

        SpawnParticles();
    }

    private void SpawnParticles()
    {
        Vector3 spawnPos = GetPlayerPosition();

        if (ParticlePoolManager.Instance != null)
        {
            ParticleSystem ps = ParticlePoolManager.Instance.SpawnParticle(shiftParticlePool, spawnPos);

            if (ps != null)
            {
                var main = ps.main;
                main.startColor = (currentReality == Reality.A) ? particleColorA : particleColorB;
            }
        }
    }

    private Vector3 GetPlayerPosition()
    {
        if (playerTransform == null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        return playerTransform != null ? playerTransform.position : Vector3.zero;
    }
}