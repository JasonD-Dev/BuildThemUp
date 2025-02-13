using PrimeTween;
using TMPro;
using UnityEngine;

public class ScrapFeedItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mText;
    [SerializeField] private Color mPositiveColor = Color.green;
    [SerializeField] private Color mNegativeColor = Color.red;

    public void Setup(int aAmount)
    {
        if (aAmount > 0)
        {
            mText.SetText($"+{aAmount}");
            mText.color = mPositiveColor;
        }
        else
        {
            mText.SetText($"{aAmount}");
            mText.color = mNegativeColor;
        }

        Tweens();
    }

    public void Setup(string aMessage)
    {
        mText.color = mNegativeColor;
        mText.SetText(aMessage);
        Tweens();
    }

    private void Tweens(float tTime = 2f)
    {
        Tween.LocalPosition(transform, new Vector3(0, 30, 0), tTime);
        Tween.Alpha(mText, 0, tTime, Ease.InCubic);
        Destroy(gameObject, tTime);
    }
}