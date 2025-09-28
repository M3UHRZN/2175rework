using System.Collections.Generic;

namespace Interactions.Core
{
    public static class InteractionStateStore
    {
        static readonly Dictionary<string, bool> boolFlags = new Dictionary<string, bool>();
        static readonly Dictionary<string, int> counters = new Dictionary<string, int>();

        public static bool GetFlag(string key)
        {
            boolFlags.TryGetValue(key, out bool value);
            return value;
        }

        public static void SetFlag(string key, bool value)
        {
            if (string.IsNullOrEmpty(key))
                return;
            boolFlags[key] = value;
        }

        public static void ClearFlag(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;
            boolFlags.Remove(key);
        }

        public static int GetCounter(string key)
        {
            counters.TryGetValue(key, out int value);
            return value;
        }

        public static int IncrementCounter(string key)
        {
            if (string.IsNullOrEmpty(key))
                return 0;
            int value = GetCounter(key) + 1;
            counters[key] = value;
            return value;
        }

        public static void ResetCounter(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;
            counters.Remove(key);
        }
    }
}
