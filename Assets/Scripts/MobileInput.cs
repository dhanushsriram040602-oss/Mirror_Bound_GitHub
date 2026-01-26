using UnityEngine;

public class MobileInput : MonoBehaviour
{
    public static float horizontal;
    public static bool jump;

    public void MoveLeft()
    {
        horizontal = -1;
    }

    public void MoveRight()
    {
        horizontal = 1;
    }

    public void StopMove()
    {
        horizontal = 0;
    }

    public void Jump()
    {
        jump = true;
    }

    public static void ResetJump()
    {
        jump = false;
    }
}
