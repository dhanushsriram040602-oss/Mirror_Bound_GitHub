using UnityEngine;

/// <summary>
/// Static mobile input hub. Mobile buttons call methods on this component
/// (which lives on the same Canvas/prefab — never a cross-scene reference).
/// PlayerController reads only from the static fields, so no direct reference
/// to PlayerController is needed from any button.
/// </summary>
public class MobileInput : MonoBehaviour
{
    // Static fields — survive scene transitions, read by PlayerController each frame.
    public static float horizontal;
    public static bool jumpPressed;
    public static bool jumpHeld;
    public static bool shiftPressed;

    // ── Left button ── PointerDown
    public void MoveLeft() { horizontal = -1f; }

    // ── Right button ── PointerDown
    public void MoveRight() { horizontal = 1f; }

    // ── Left / Right button ── PointerUp  AND  PointerExit
    public void StopMove() { horizontal = 0f; }

    // ── Jump button ── PointerDown
    public void JumpDown() { jumpPressed = true; jumpHeld = true; }

    // ── Jump button ── PointerUp  AND  PointerExit
    public void JumpUp() { jumpHeld = false; }

    // ── Shift / Reality button ── PointerDown
    public void Shift() { shiftPressed = true; }

    // Legacy alias kept so any old EventTrigger entry wired to "Jump" still works.
    public void Jump() { jumpPressed = true; jumpHeld = true; }

    /// <summary>Clears jump state after PlayerController consumes it.</summary>
    public static void ConsumeJump() { jumpPressed = false; }

    /// <summary>Clears shift state after PlayerController consumes it.</summary>
    public static void ConsumeShift() { shiftPressed = false; }

    /// <summary>Resets all static input — call at the start of every level.</summary>
    public static void ResetAll()
    {
        horizontal    = 0f;
        jumpPressed   = false;
        jumpHeld      = false;
        shiftPressed  = false;
    }

    // Legacy name — kept for backward compatibility.
    public static void ResetJump() { ConsumeJump(); }
}
