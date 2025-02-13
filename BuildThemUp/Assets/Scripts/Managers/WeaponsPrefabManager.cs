using System.Collections.Generic;
using UnityEngine;

public class WeaponsPrefabManager : MonoBehaviour
{
    public static WeaponsPrefabManager Instance { get; private set; }

    [field: SerializeField] public Weapon[] weapons { get; private set; }
    //Create another list for other item types.

    private Dictionary<string, Weapon> mNameToWeapon;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        
        Instance = this;
        // make sure that the PrefabManager is persistent, so we're not reloading stuff all the time
        DontDestroyOnLoad(gameObject);
        
        mNameToWeapon = new Dictionary<string, Weapon>();
        foreach (var i in weapons)
            if (!mNameToWeapon.TryAdd(i.Name.ToLowerInvariant(), i))
                Log.Warning(this, "Duplicate weapon name: " + i.Name);
    }

    public Weapon GetWeaponsPrefab(string aName)
    {
        var tName = aName.ToLowerInvariant();
        
        if (!mNameToWeapon.TryGetValue(tName, out var tWeapon))
        {
            Log.Error(this,"Tried to get weapon with name " + aName + " that doesn't exist!");
            return null;
        }

        return tWeapon;
    }
}