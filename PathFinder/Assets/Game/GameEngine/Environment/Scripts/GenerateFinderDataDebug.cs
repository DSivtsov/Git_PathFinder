using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace GameEngine.Environment
{
    public class GenerateFinderDataDebug
    {
        [Conditional("GENERATEDEBUG")]
        public static void DebugLogUpdate(string str) => Debug.Log(str);

        [Conditional("GENERATEDEBUG")]
        public static void DebugLogWarningUpdate(string str) => Debug.LogWarning(str);
    }  
}

