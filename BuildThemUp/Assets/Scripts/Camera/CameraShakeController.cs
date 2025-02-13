using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraShakeController : MonoBehaviour
{
    public static CameraShakeController instance;

    [Header("Components")]
    [SerializeField] private Transform shakingCameraTransform;
    [SerializeField] private float mMaxEffectDistance;

    [Header("Reovery")]
    [SerializeField, Range(0f, 5f)] private float mRecoverySpeed = 3f;

    private List<CameraShakeData> allShakesData = new List<CameraShakeData>();

    struct CameraShakeData
    {
        private float timeStampOfShake;
        private Vector3 intensity;
        private Vector3 impactPosition;
        private float duration;
        private float effectDistance;

        //X = Random Amplitude and Y = Random wave Frequency
        private Vector2[] mAmplitudeAndFrequency;

        public CameraShakeData(
            float aTimeStampOfShake,
            Vector3 aIntensity,
            Vector2 aAmplitude,
            Vector2 aFrequency,
            Vector3 aImpactPosition, 
            float aDuration, 
            float aEffectDistance, 
            int aNumberOfSinWaves
        )
        {
            timeStampOfShake = aTimeStampOfShake;
            intensity = aIntensity;
            impactPosition = aImpactPosition;
            duration = aDuration;
            effectDistance = aEffectDistance;

            mAmplitudeAndFrequency = new Vector2[aNumberOfSinWaves];
            for (int i = 0; i < aNumberOfSinWaves; i++) 
            {
                mAmplitudeAndFrequency[i] = new Vector2(Random.Range(aAmplitude.x, aAmplitude.y), Random.Range(aFrequency.x, aFrequency.y));
            }
        }

        public Vector3 GetCameraShakeEulerAmount(Vector3 aCameraPosition)
        {
            float tNormalisedTime = (Time.time - timeStampOfShake) / duration;
            float tDistanceFromCamera = Vector3.Distance(aCameraPosition, impactPosition);
            if (tDistanceFromCamera > effectDistance || tNormalisedTime >= 1)
            {
                return Vector3.zero;
            }

            float tTotalSinValues = 0;
            foreach(Vector2 tSinData in mAmplitudeAndFrequency)
            {
                tTotalSinValues += tSinData.x * Mathf.Sin(tSinData.y * tNormalisedTime);
            }
            //Maybe use Curve instead
            float tSmoothingFunction = (-4 * Mathf.Pow(tNormalisedTime - 0.5f, 2) + 1);
            float tSmoothedSinValue = (tTotalSinValues / mAmplitudeAndFrequency.Length) * tSmoothingFunction;

            //Calculates Smoothed Intensity, includig linear drop off by distance
            return intensity * (tSmoothedSinValue * (1 - tDistanceFromCamera / effectDistance));
        }

        public bool IsDone()
        {
            return (Time.time - timeStampOfShake >= duration);
        }
    }


    private void Awake()
    {
        if (instance)
        {
            Destroy(instance.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        if (shakingCameraTransform == null)
        {
            Log.Error(this, "shaking camera is not assigned. What the heck is suppose to shake dudes.");
        }
    }
    
    void FixedUpdate()
    {
        Vector3 tTotalShakeEulerAngles = Vector3.zero;
        for (int i = 0; i < allShakesData.Count; i++)
        {
            CameraShakeData tCameraShakeData = allShakesData[i];

            if (tCameraShakeData.IsDone() == true)
            {
                allShakesData.Remove(tCameraShakeData);
                i--;
            }

            tTotalShakeEulerAngles += tCameraShakeData.GetCameraShakeEulerAmount(transform.position);
        }

        if (tTotalShakeEulerAngles != Vector3.zero)
        {
            shakingCameraTransform.localEulerAngles += tTotalShakeEulerAngles;
        }
        else if (shakingCameraTransform.localEulerAngles.z != 0 )
        {
            Quaternion tCurrentRotation = shakingCameraTransform.rotation;
            Quaternion tTargetRotation = Quaternion.Euler(tCurrentRotation.eulerAngles.x, tCurrentRotation.eulerAngles.y, 0);

            shakingCameraTransform.rotation = Quaternion.Lerp(tCurrentRotation, tTargetRotation, Time.deltaTime * mRecoverySpeed);
        }
    }

    public void AddShakeEvent(Vector3 aIntensity, Vector2 aAmplitude, Vector2 aFrequency, Vector3 aImpactPosition, float aDuration, float aEffectDistance, int aNumberOfSinWaves)
    {
        CameraShakeData tCameraShakeData = new CameraShakeData(
            aTimeStampOfShake: Time.time,
            aIntensity: aIntensity,
            aAmplitude: aAmplitude,
            aFrequency: aFrequency,
            aImpactPosition: aImpactPosition, 
            aDuration: aDuration, 
            aEffectDistance: aEffectDistance, 
            aNumberOfSinWaves: aNumberOfSinWaves
        );

        allShakesData.Add(tCameraShakeData);
    }
}
