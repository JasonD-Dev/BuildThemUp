using UnityEngine;

public static class UIBinds
{
    //General Binds
    public static KeyCode back { get; private set; } = KeyCode.Escape;


    //Main Base Binds
    public static KeyCode openMainBaseUI { get; private set; } = KeyCode.E;
    public static KeyCode closeMainBaseUI { get; private set; } = KeyCode.E;
    public static KeyCode upgradeKey { get; private set; } = KeyCode.F;


    //Zipline Binds
    public static KeyCode attachToZiplineKey { get; private set; } = KeyCode.E;
    public static KeyCode detachFromZiplineKey { get; private set; } = KeyCode.Space;

}
