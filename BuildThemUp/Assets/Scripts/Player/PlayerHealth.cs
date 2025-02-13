using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : Health
{
    public override bool isFriendly => true;
    
    [SerializeField] private HitIndicator hitIndicatorPrefab;

    [Header("Low Health Warning Vignette")]
    [Tooltip("What should the intensity of the vignette be at 0 health?")]
    public float maxIntensity = 0.75f;
    [Tooltip("When should the vignette appear (as a percentage of health)?")]
    public float startVignette = 0.5f;
    
    [Header("Vignette Pulsing")]
    [Tooltip("Should the vignette pulse?")]
    public bool pulseVignette = true;
    [Tooltip("Max frequency in cycles per second")]
    public float maxFreq = 3;
    [Tooltip("Approx much it should pulse (higher = more pulsing)")]
    public float pulseOffset = 0.05f;
    
    protected override void Start()
    {
        base.Start();
        
        onDamageTaken.AddListener(OnPlayerDamage);
    }

    private void OnPlayerDamage(float aDamage, Transform aAttackerTransform)
    {
        float tPerc = health / mMaxHealth;

        bool tDoVignette = tPerc <= startVignette;
        if (tDoVignette)
        {
            var tVPerc = 1 - tPerc / startVignette;
            // simple scaling function to account for the exponential onset of the vignette
            // e.g., up to 0.3 intensity is basically invisible, but 0.5 is too much
            var tVLerp = Mathf.Sqrt(4 * tVPerc) / 2;
            var tVIntensity = Mathf.Lerp(0, maxIntensity, tVLerp);

            if (pulseVignette)
            {
                var tVMinInt = Mathf.Lerp(0, maxIntensity - pulseOffset, tVLerp);
                var tVFreq = Mathf.Lerp(0, maxFreq, tVLerp);
                CameraController.Instance.SetVignette(tVMinInt, tVIntensity, tVFreq);
            }
            else
                CameraController.Instance.SetVignette(tVIntensity);
        }
        
        // we don't need to track hit indicator, it is self-managing (destroys itself)
        var tHitIndicator = Instantiate(hitIndicatorPrefab, Crosshair.instance.transform);
        tHitIndicator.Setup(aDamage, mMaxHealth, aAttackerTransform.position);
    }

    public override void Die()
    {
        Log.Info(this, $"Player '{name}' died. Game Over!");
        GamePlayManager.instance.SetGameOver();
    }
}
