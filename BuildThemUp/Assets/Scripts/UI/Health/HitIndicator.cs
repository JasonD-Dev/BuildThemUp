using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class HitIndicator : MonoBehaviour
{
    [SerializeField] private Image indicatorCircle;

    public float fadeTime = 2f;

    private Transform mPlayerTransform;
    private Vector2 mHitXZ;
    private float mRotationOffset;

    private void Awake()
    {
        mPlayerTransform = GamePlayManager.instance.playerTransform;
    }

    public void Setup(float aDamage, float aMaxHp, Vector3 aHitPoint)
    {
        float tFillAmount = aDamage / aMaxHp; // scale fill as a percentage of max hp
        mRotationOffset = tFillAmount * 360 / 2; // rotation offset because fill starts from 12 oclock going clockwise
        mHitXZ = new Vector2(aHitPoint.x, aHitPoint.z); // convert to top-down point
        indicatorCircle.fillAmount = tFillAmount;
        
        // set rotation when we spawn it so we don't get a flash of unset rotation
        transform.localEulerAngles = new Vector3(0, 0, CalculateRotation());

        // tween fade out
        Tween.Alpha(indicatorCircle, 0, fadeTime, Ease.InExpo);
        // destroy after fading
        Destroy(gameObject, fadeTime);
    }
    
    public float CalculateRotation()
    {
        Vector3 tPlayerPos = mPlayerTransform.position;
        // convert player position to top-down position
        Vector2 tPlayerXZ = new Vector2(tPlayerPos.x, tPlayerPos.z);
        // get the top-down angle from the player to the hit point
        float tAngle = Mathf.Atan2(mHitXZ.y - tPlayerXZ.y, mHitXZ.x - tPlayerXZ.x) * Mathf.Rad2Deg;
        
        // note: this commented code absolutely does not work
        //var tAngle = Vector2.SignedAngle(tPlayerXZ, mHitXZ) + 180;
        //Log.Info(this, $"mHitXZ: {mHitXZ}, tPlayerXZ: {tPlayerXZ}, tAngle: {tAngle}");
        
        return tAngle + mPlayerTransform.localEulerAngles.y + mRotationOffset - 90;
    }
    
    public void FixedUpdate()
    {
        transform.localEulerAngles = new Vector3(0, 0, CalculateRotation());
    }
}