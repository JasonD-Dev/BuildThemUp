using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DynamicHitboxController : MonoBehaviour
{
    private static readonly Dictionary<DynamicHitboxPart, float> partToDamageMultiplier =
        new Dictionary<DynamicHitboxPart, float>()
        {
            { DynamicHitboxPart.Head, 1.5f },
            { DynamicHitboxPart.Torso, 1f },
            { DynamicHitboxPart.Arm, 0.8f },
            { DynamicHitboxPart.Leg, 0.8f }
        };
    
    [SerializeField] public Health health;
    [SerializeField] private List<DynamicHitbox> mHitboxes;

    public void Awake()
    {
        foreach (var tBox in mHitboxes)
            tBox.SetParent(this);
    }

    // index = 0 should be torso collider!
    public Vector3 GetHitboxCenter(int index = 0)
    {
        var tPosition = transform.position;
        var tTorsoPos = mHitboxes[index].GetComponent<BoxCollider>().bounds.center;
        
        return tTorsoPos - tPosition;
    }

    public bool TakeDamage(DynamicHitboxPart aPart, float aAmount, Transform aAttackerTransform)
    {
        if (!partToDamageMultiplier.TryGetValue(aPart, out var tMult))
        {
            tMult = 1;
            Log.Warning(this, "Undefined part called TakeDamage! Make sure all dynamic hitboxes have a part set!");
        }

        return health.TakeDamage(aAmount * tMult, aAttackerTransform);
    }
}

public enum DynamicHitboxPart
{
    Undefined,
    Head,
    Torso,
    Arm,
    Leg
}