using System.Collections.Generic;

public static class InteractionStateRegistry
{
    static readonly Dictionary<string, bool> boolStates = new Dictionary<string, bool>();
    static readonly Dictionary<string, int> intStates = new Dictionary<string, int>();

    public static void SetBool(string key, bool value)
    {
        if (string.IsNullOrEmpty(key))
            return;
        boolStates[key] = value;
    }

    public static bool GetBool(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;
        boolStates.TryGetValue(key, out bool value);
        return value;
    }

    public static void ClearBool(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;
        boolStates.Remove(key);
    }

    public static void SetInt(string key, int value)
    {
        if (string.IsNullOrEmpty(key))
            return;
        intStates[key] = value;
    }

    public static int GetInt(string key)
    {
        if (string.IsNullOrEmpty(key))
            return 0;
        intStates.TryGetValue(key, out int value);
        return value;
    }

    public static void ClearAll()
    {
        boolStates.Clear();
        intStates.Clear();
    }
}
