using System.Collections.Generic;
using UnityEngine;


namespace GameEngine.PathFinder
{
    public enum SolutionSide
    {
        Start = 0,
        End = 1,
    }

    public interface ISolution
    {
        IEnumerable<Line> GetListLinesFromSectorSolutions();
        IEnumerable<Vector2> GetListBasedDotsSolution();
        int NumLastCrossedEdgeBySolution { get; }
        int NumRecBaseDotSolution { get; }
        IEnumerable<ConnectionDot> GetListConnectionDotsSolution();
        IEnumerable<(SectorSolutions,ConnectionDot)> GetSectorSolutionsWithConnectionDots();
    }
}