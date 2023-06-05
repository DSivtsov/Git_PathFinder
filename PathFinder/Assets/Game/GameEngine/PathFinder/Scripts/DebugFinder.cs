using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.PathFinder
{
    public static class DebugFinder
    {
        private static DebugPathFinder _debugPathFinder;
        private static bool _activateDebugPathFinder = false;
        private static bool _debugOn;

        [System.Diagnostics.Conditional("DEBUGFINDER")]
        public static void StartDebugFinder(DebugPathFinder debugPathFinder)
        {
            _debugPathFinder = debugPathFinder;
            _debugPathFinder.DeleteDebugFinderLines();
            _activateDebugPathFinder = true;
            _debugOn = true;
        }

        [System.Diagnostics.Conditional("DEBUGFINDER")]
        public static void StopDebugFinder()
        {
            _debugPathFinder = null;
            _activateDebugPathFinder = false;
        }

        [System.Diagnostics.Conditional("DEBUGFINDER")]
        public static void DebugDrawDot(Vector2 dot)
        {
            if (_activateDebugPathFinder && _debugOn)
                _debugPathFinder.ShowDotCross(dot, null);
        }

        [System.Diagnostics.Conditional("DEBUGFINDER")]
        public static void DebugDrawDot(Vector2 dot, string nameDot)
        {
            if (_activateDebugPathFinder && _debugOn)
                _debugPathFinder.ShowDotCross(dot, nameDot);
        }

        [System.Diagnostics.Conditional("DEBUGFINDER")]
        public static void DebugDrawLine(Line line)
        {
            if (_activateDebugPathFinder && _debugOn)
                _debugPathFinder.ShowLine(line, null);
        }

        [System.Diagnostics.Conditional("DEBUGFINDER")]
        public static void DebugDrawLine(Line line, string nameLine)
        {
            if (_activateDebugPathFinder && _debugOn)
                _debugPathFinder.ShowLine(line, nameLine);
        }

        [System.Diagnostics.Conditional("DEBUGFINDER")]
        public static void DebugDrawLineSegment(Vector2 start, Vector2 end, string nameLine)
        {
            if (_activateDebugPathFinder && _debugOn)
                _debugPathFinder.ShowLine(start, end, nameLine);
        }


        [System.Diagnostics.Conditional("DEBUGFINDER")]
        public static void DebugDrawLine(List<Line> lines, string nameGroupLine)
        {
            if (_activateDebugPathFinder && _debugOn)
                _debugPathFinder.ShowLine(lines, nameGroupLine);
        }

        [System.Diagnostics.Conditional("DEBUGFINDER")]
        public static void DebugTurnOn(bool active) => _debugOn = active;
    }
}

