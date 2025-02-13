
using System;
using UnityEngine;

public class DynamicHitbox : MonoBehaviour
{
    [field: SerializeField] public DynamicHitboxPart hitboxPart { get; private set; }
    
    // don't serialise just to reduce the sheer amount of references
    [NonSerialized] public DynamicHitboxController parent;

    // called by the parent controller
    public void SetParent(DynamicHitboxController aParent)
    {
        parent = aParent;
    }

    public bool TakeDamage(float aAmount, Transform aAttackerTransform)
        => parent.TakeDamage(hitboxPart, aAmount, aAttackerTransform);
}