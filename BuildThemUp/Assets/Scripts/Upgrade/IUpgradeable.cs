
using System.Collections.Generic;

public interface IUpgradeable<T>
{
    public void OnUpgrade(T aOther);
    public KeyValuePair<string, string>[] GetNextUpgradeStats();
    public KeyValuePair<string, string>[] GetCurrentStats(bool includeCost = false);
}
