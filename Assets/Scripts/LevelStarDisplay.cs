using UnityEngine;
using UnityEngine.UI;

public class LevelStarDisplay : MonoBehaviour
{
    [Header("Star References")]
    public Image star1;
    public Image star2;
    public Image star3;

    [Header("Colors")]
    public Color filledColor = Color.yellow;
    public Color emptyColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    public void SetStars(int starCount)
    {
        if (star1 != null) star1.color = starCount >= 1 ? filledColor : emptyColor;
        if (star2 != null) star2.color = starCount >= 2 ? filledColor : emptyColor;
        if (star3 != null) star3.color = starCount >= 3 ? filledColor : emptyColor;
    }

    public void HideStars()
    {
        if (star1 != null) star1.gameObject.SetActive(false);
        if (star2 != null) star2.gameObject.SetActive(false);
        if (star3 != null) star3.gameObject.SetActive(false);
    }

    public void ShowStars()
    {
        if (star1 != null) star1.gameObject.SetActive(true);
        if (star2 != null) star2.gameObject.SetActive(true);
        if (star3 != null) star3.gameObject.SetActive(true);
    }
}
