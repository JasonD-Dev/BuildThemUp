using System;
using UnityEngine;

[Serializable]
public struct MainBaseIncrementUpgrade
{
    [field: SerializeField] public float connectionRange { get; private set; }
    [field: SerializeField] public float maxHealth { get; private set; }
    [field: SerializeField] public float radarRange { get; private set; }
    [field: SerializeField] public float radarDelay { get; private set; }
}