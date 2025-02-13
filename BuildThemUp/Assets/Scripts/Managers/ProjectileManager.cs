using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance;
    
    private Dictionary<int, Queue<Projectile>> mProjectilePool;

    public void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);

        Instance = this;

        mProjectilePool = new Dictionary<int, Queue<Projectile>>();
    }

    // adds projectile to pool for reuse
    public void AddToPool(Projectile aProjectile)
    {
        aProjectile.OnPool();
        
        Queue<Projectile> tQueue;
        if (!mProjectilePool.TryGetValue(aProjectile.PoolingId, out tQueue))
        {
            tQueue = new Queue<Projectile>();
            mProjectilePool.Add(aProjectile.PoolingId, tQueue);
        }
        
        //Log.Info(this, $"Pooling projectile with id: {aProjectile.PoolingId}");
        tQueue.Enqueue(aProjectile);
    }

    //TODO This should only return the projectile and the projectile should be initialised by who ever is calling for it.
    public Projectile GetFromPool(Projectile aPrefab, bool aIsFriendly, Vector3 aOrigin, Vector3 aDirection, float aSpeed, float aDamage, bool aShowHitmarkerOnHit)
    {
        Projectile tInstance;

        var tPoolingId = aPrefab.GetInstanceID();
        
        // Check if projectile pool has a pool for this projectile:
        // FALSE - Instantiate a new projectile
        // TRUE - Check if the queue has an available projectile:
        //          FALSE - Instantiate a new projectile
        //          TRUE - Initialise the available projectile
        if (!mProjectilePool.TryGetValue(tPoolingId, out var tQueue)
            || !tQueue.TryDequeue(out tInstance))
            tInstance = Instantiate(aPrefab, transform);
        
        tInstance.SetPoolingId(tPoolingId);
        tInstance.Initialise(aIsFriendly, aOrigin, aDirection, aSpeed, aDamage, aShowHitmarkerOnHit);
        return tInstance;
    }

    public void ClearPool()
    {
        // go through pool and destroy all gameobjects
        foreach (var i in mProjectilePool)
            while (i.Value.TryDequeue(out var j))
                Destroy(j.gameObject);
        
        // clear the dict
        mProjectilePool.Clear();
    }
}