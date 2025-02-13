
using System;
using UnityEngine;

public static class Log
{
    private static string Format(MonoBehaviour aCaller, string aMessage)
    {
        return $"[{aCaller.name}, {aCaller.GetType()}]: {aMessage}";
    }
    
    private static string Format(Type aCallerType, string aMessage)
    {
        return $"[{aCallerType.Name}]: {aMessage}";
    }
    
    private static string Format(string aCaller, string aMessage)
    {
        return $"[{aCaller}]: {aMessage}";
    }
    
    
    public static void Info(string aCaller, string aMessage) => Debug.Log(Format(aCaller, aMessage));
    public static void Warning(string aCaller, string aMessage) => Debug.LogWarning(Format(aCaller, aMessage));
    public static void Error(string aCaller, string aMessage) => Debug.LogError(Format(aCaller, aMessage));
    
    public static void Assert(string aCaller, bool aAssertion, string aMessage)
        => Debug.Assert(aAssertion, Format(aCaller, aMessage));
    
    public static void Info(Type aCallerType, string aMessage) => Debug.Log(Format(aCallerType, aMessage));
    public static void Warning(Type aCallerType, string aMessage) => Debug.LogWarning(Format(aCallerType, aMessage));
    public static void Error(Type aCallerType, string aMessage) => Debug.LogError(Format(aCallerType, aMessage));
    
    public static void Assert(Type aCallerType, bool aAssertion, string aMessage)
        => Debug.Assert(aAssertion, Format(aCallerType, aMessage));

    public static void Info(MonoBehaviour aCaller, string aMessage) => Debug.Log(Format(aCaller, aMessage));
    public static void Warning(MonoBehaviour aCaller, string aMessage) => Debug.LogWarning(Format(aCaller, aMessage));
    public static void Error(MonoBehaviour aCaller, string aMessage) => Debug.LogError(Format(aCaller, aMessage));
    
    public static void Assert(MonoBehaviour aCaller, bool aAssertion, string aMessage)
        => Debug.Assert(aAssertion, Format(aCaller, aMessage));
}