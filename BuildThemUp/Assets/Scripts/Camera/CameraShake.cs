using System;
using UnityEngine;

[Serializable]
public struct CameraShake
{
    [field: SerializeField] public Vector3 intensity { get; private set; }
    [field: SerializeField] public float duration { get; private set; }
    [field: SerializeField] public float effectDistance { get; private set; }

    [SerializeField, Range(0, 1f)] float minAmplitude;
    [SerializeField, Range(0, 1f)] float maxAmplitude;
    [SerializeField, Range(0, 100)] float minFrequency;
    [SerializeField, Range(0, 100)] float maxFrequency;
    [SerializeField, Range(1, 4)] int mNumberOfSinWaves;

    public CameraShake(
        Vector3 aIntensity,
        float aDuration, 
        float aEffectDistance,
        int aNumberOfSinWaves,
        float aMinAmplitude = 0.25f,
        float aMaxAmplitude = 0.75f,
        float aMinFrequency = 20,
        float aMaxFrequency = 85
    )
    {
        intensity = aIntensity;
        minAmplitude = aMinAmplitude;
        maxAmplitude = aMaxAmplitude;
        minFrequency = aMinFrequency;
        maxFrequency = aMaxFrequency;
        duration = aDuration;
        effectDistance = aEffectDistance;
        mNumberOfSinWaves = aNumberOfSinWaves;
    }

    public void AddShakeEvent(Vector3 aPosition)
    {
        CameraShakeController.instance.AddShakeEvent(
            aIntensity: intensity, 
            aAmplitude: new Vector2(minAmplitude, maxAmplitude), 
            aFrequency: new Vector2(minFrequency, maxFrequency), 
            aImpactPosition: aPosition, 
            aDuration: duration, 
            aEffectDistance: effectDistance, 
            aNumberOfSinWaves: mNumberOfSinWaves
        );
    }
}
