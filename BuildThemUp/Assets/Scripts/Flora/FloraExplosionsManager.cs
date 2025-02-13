using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FloraExplosionsManager : MonoBehaviour
{
    public static FloraExplosionsManager instance { get; private set; }

    [SerializeField] Material mFloraMaterial;

    List<Vector4> mExplosions; //x,y,z = position, w = distance.

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        mFloraMaterial.SetInteger("m_explosionsCount", 0);
    }

    void Update()
    {
        
    }

    public void AddExplosion(Vector3 aPosition, float aStrength)
    {
        mExplosions.Add(new Vector4(aPosition.x, aPosition.y, aPosition.z, aStrength));
        mFloraMaterial.SetVectorArray("m_explosions", mExplosions);
    }
}
