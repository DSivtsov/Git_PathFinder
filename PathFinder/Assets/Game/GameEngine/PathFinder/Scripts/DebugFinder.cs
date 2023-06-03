using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.PathFinder
{
    public static class DebugFinder
    {
#if DEBUGFINDER
        private static DebugPathFinderManager _debugPathFinderManager;
        private static bool _activateDebugPathFinder = false;
        private static bool _debugOn;
#endif
        [System.Diagnostics.Conditional("DEBUGFINDER")]
        public static void StartDebugFinder(DebugPathFinderManager debugPathFinderManager, bool activateDebugPathFinder = true)
        {
            _debugPathFinderManager = debugPathFinderManager;
            _debugPathFinderManager.DeleteDebugFinderLines();
            _activateDebugPathFinder = activateDebugPathFinder;
            _debugOn = true;
        }

        [System.Diagnostics.Conditional("DEBUGFINDER")]
        public static void DebugDrawDot(Vector2 dot)
        {
            if (_activateDebugPathFinder && _debugOn)
                _debugPathFinderManager.ShowDotCross(dot, null);
        }

        [System.Diagnostics.Conditional("DEBUGFINDER")]
        public static void DebugDrawDot(Vector2 dot, string nameDot)
        {
            if (_activateDebugPathFinder && _debugOn)
                _debugPathFinderManager.ShowDotCross(dot, nameDot);
        }

        [System.Diagnostics.Conditional("DEBUGFINDER")]
        public static void DebugDrawLine(Line line)
        {
            if (_activateDebugPathFinder && _debugOn)
                _debugPathFinderManager.ShowLine(line, null);
        }

        [System.Diagnostics.Conditional("DEBUGFINDER")]
        public static void DebugDrawLine(Line line, string nameLine)
        {
            if (_activateDebugPathFinder && _debugOn)
                _debugPathFinderManager.ShowLine(line, nameLine);
        }

        [System.Diagnostics.Conditional("DEBUGFINDER")]
        public static void DebugDrawLineSegment(Vector2 start, Vector2 end, string nameLine)
        {
            if (_activateDebugPathFinder && _debugOn)
                _debugPathFinderManager.ShowLine(start, end, nameLine);
        }


        [System.Diagnostics.Conditional("DEBUGFINDER")]
        public static void DebugDrawLine(List<Line> lines, string nameGroupLine)
        {
            if (_activateDebugPathFinder && _debugOn)
                _debugPathFinderManager.ShowLine(lines, nameGroupLine);
        }

        [System.Diagnostics.Conditional("DEBUGFINDER")]
        public static void DebugTurnOn(bool active) => _debugOn = active;
    }
}

