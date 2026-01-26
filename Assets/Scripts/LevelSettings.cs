using UnityEngine;

public class LevelSettings : MonoBehaviour
{
    [Header("Hint System")]
    [TextArea(3, 10)]
    public string levelHint = "Use SHIFT to toggle reality.";

    [Header("Star Rating Thresholds (Time-Based Only)")]
    [Tooltip("Complete under this time for 3 stars")]
    public float threeStarTime = 15f;

    [Tooltip("Complete under this time for 2 stars")]
    public float twoStarTime = 30f;
}
