using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public static Crosshair instance;

    [SerializeField] ImageFillAnimation[] mCrosshairComponents;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Log.Warning(this, $"There were multiple instances of {typeof(Crosshair)} found. Destroying this one!)");
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public void Animate()
    {
        foreach (ImageFillAnimation tImageFillAnimation in mCrosshairComponents)
        {
            tImageFillAnimation.Animate(); 
        }
    }
}
