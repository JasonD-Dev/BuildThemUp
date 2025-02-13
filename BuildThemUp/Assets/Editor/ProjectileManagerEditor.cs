using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProjectileManager))]
public class ProjectileManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        if (GUILayout.Button("Clear Pool"))
        {
            // we don't worry about getting the actual script target,
            // if we have multiple instances... we got bigger problems
            // otherwise we can var x = target as [ProjectileManager]
            if (!ProjectileManager.Instance)
                Log.Warning(typeof(ProjectileManagerEditor), "Can't clear pool of a non-instantiated projectile manager!");
            else
                ProjectileManager.Instance.ClearPool();
        }
    }
}