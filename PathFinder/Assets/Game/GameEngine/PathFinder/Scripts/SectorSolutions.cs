using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using GameEngine.Environment;


namespace GameEngine.PathFinder
{
    /// <summary>
    /// It's s sector of possible solutions limited by two line, both of which start from the point baseDotSectorSolutions
    /// </summary>
    public class SectorSolutions
    {
        public readonly Line LineB;
        public readonly Line LineA;
        public readonly Vector2 baseDotSectorSolutions;

        public SectorSolutions(List<Line> lines, Vector2 baseDotSectorSolutions)
        {
            if (lines.Count == 2)
            {
                LineB = lines[0];
                LineA = lines[1];
            }
            else
                throw new NotSupportedException($"Wrong number lines in {lines}");
            this.baseDotSectorSolutions = baseDotSectorSolutions;
        }

        public IEnumerable<Line> GetListLines()
        {
            yield return LineB;
            yield return LineA;
        }
    }
}