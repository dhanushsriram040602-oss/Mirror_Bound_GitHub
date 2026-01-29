using UnityEngine;
using UnityEngine.UI;
using Input = UnityEngine.Input;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput Instance { get; private set; }

    // ================= PUBLIC SNAPSHOT =================
    public float InputX { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpPressed { get; private set; }   // ONE FRAME
    public bool RealityTogglePressed { get; private set; }

#if UNITY_ANDROID || UNITY_IOS
    float mobileHorizontal;
    bool mobileJumpHeld;
    bool mobileJumpPressed;
    bool mobileTogglePressed;
#endif

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void Update()
    {
        // reset one-frame pulses
        JumpPressed = false;
        RealityTogglePressed = false;

#if UNITY_ANDROID || UNITY_IOS
    InputX = mobileHorizontal;
    JumpHeld = mobileJumpHeld;

    if (mobileJumpPressed)
    {
        JumpPressed = true;
        mobileJumpPressed = false;
    }

    if (mobileTogglePressed)
    {
        RealityTogglePressed = true;
        mobileTogglePressed = false;
    }
#else
        InputX = Input.GetAxisRaw("Horizontal");
        JumpHeld = Input.GetButton("Jump");
        JumpPressed = Input.GetButtonDown("Jump");
        RealityTogglePressed = Input.GetKeyDown(KeyCode.R);
#endif
    }

    // ================= MOBILE CALLBACKS =================

    public void OnLeftDown()
    {
#if UNITY_ANDROID || UNITY_IOS
        mobileHorizontal = -1;
#endif
    }

    public void OnRightDown()
    {
#if UNITY_ANDROID || UNITY_IOS
        mobileHorizontal = 1;
#endif
    }

    public void OnHorizontalUp()
    {
#if UNITY_ANDROID || UNITY_IOS
        mobileHorizontal = 0;
#endif
    }

    public void OnJumpDown()
    {
#if UNITY_ANDROID || UNITY_IOS
    mobileJumpHeld = true;
    mobileJumpPressed = true;
#endif
    }

    public void OnJumpUp()
    {
#if UNITY_ANDROID || UNITY_IOS
    mobileJumpHeld = false;   // THIS MUST HAPPEN
#endif
    }


    public void MobileToggleReality()
    {
#if UNITY_ANDROID || UNITY_IOS
        mobileTogglePressed = true;
#endif
    }
}
