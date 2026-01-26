using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class TouchResponsivenessBoost : MonoBehaviour
{
    [Header("Touch Settings")]
    [SerializeField] private bool enableMultiTouch = true;
    [SerializeField] private int maxTouchPoints = 10;

    void Awake()
    {
        OptimizeTouchSettings();
        EnableEnhancedTouch();
    }

    void OptimizeTouchSettings()
    {
        if (enableMultiTouch)
        {
            Input.multiTouchEnabled = true;
            
            #if UNITY_ANDROID || UNITY_IOS
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            #endif

            Debug.Log($"[TouchBoost] Multi-touch enabled with {maxTouchPoints} max touch points");
        }
    }

    void EnableEnhancedTouch()
    {
        if (!EnhancedTouchSupport.enabled)
        {
            EnhancedTouchSupport.Enable();
            Debug.Log("[TouchBoost] Enhanced Touch Support enabled");
        }
    }

    void OnApplicationQuit()
    {
        if (EnhancedTouchSupport.enabled)
        {
            EnhancedTouchSupport.Disable();
        }
    }
}
