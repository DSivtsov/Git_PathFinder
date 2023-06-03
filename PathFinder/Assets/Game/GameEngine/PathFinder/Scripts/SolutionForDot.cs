using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace GameEngine.PathFinder
{
    public class SolutionForDot : ISolution
    {

        private readonly SectorSolutions _sectorSolutions;
        private readonly int _numLastCrossedEdge;
        private readonly int _numRecBaseDot;
        private readonly ConnectionDot _connectionDot;

        int ISolution.NumLastCrossedEdgeBySolution => _numLastCrossedEdge;

        int ISolution.NumRecBaseDotSolution => _numRecBaseDot;

        public SolutionForDot(SectorSolutions sectorSolutions, int numLastEdge, int numRecBaseDot, ConnectionDot connectionDot)
        {
            _sectorSolutions = sectorSolutions;
            _numLastCrossedEdge = numLastEdge;
            _numRecBaseDot = numRecBaseDot;
            _connectionDot = connectionDot;
        }

        IEnumerable<SectorSolutions> ISolution.GetListSectorSolutions()
        {
            yield return _sectorSolutions;
        }

        public IEnumerable<Line> GetListLinesFromSectorSolutions()
        {
            yield return _sectorSolutions.LineB;
            yield return _sectorSolutions.LineA;
        }

        internal List<Vector2> GetListEdgeDotsLastCrossingEdge()
        {
            //Dots on Edge where LineB & LineA cross this edge
            throw new NotImplementedException();
        }

        IEnumerable<Vector2> ISolution.GetListBasedDotsSolution()
        {
            yield return _sectorSolutions.baseDotSectorSolutions;
        }

        IEnumerable<ConnectionDot> ISolution.GetListConnectionDotsSolution()
        {
            if (_connectionDot != null)
            {
                yield return _connectionDot;
            }
            else
                throw new NotImplementedException("GetListConnectionDotsSolution for Solution (SolutionSide == End)");
        }
        

        internal static SolutionForDot FindAndCreateSolutionForDot(Vector2 baseDotSolution, int closestNumEdge, int farthestNumEdge, SolutionSide solutionSide)
        {
            Debug.LogWarning($"FindAndCreateSolutionForDot(SolutionSide[{solutionSide}], closeEdge[{closestNumEdge}], farEdge[{farthestNumEdge}])");
            int numRecBaseDot = StoreInfoEdges.GetNumRectWithEdgeForSolution(closestNumEdge, solutionSide);
            List<Line> listLines = new List<Line>(2);

            foreach ((int currentTestingNumEdge, int nextEdgeAftercurrent) in StoreInfoEdges.GetOrderedListNumEdges(closestNumEdge, farthestNumEdge))
            {
                Debug.Log($"Trying link baseDotSolution with Edge[{currentTestingNumEdge}]");

                foreach (Vector2 dotEdge in StoreInfoEdges.GetListDotsEdge(currentTestingNumEdge))
                {
                    //Test possibility to create line with baseDot and dot from other edge with can pass all edges between them
                    //int nextEdgeAftercurrent = currentTestingNumEdge + step;    will skip current edge fot testing pass
                    (bool isPassedEdges, Line lineBTWBaseDotAndEdge) = Line.TryLinkTwoDotsThroughEdges(baseDotSolution, dotEdge, closestNumEdge,
                        nextEdgeAftercurrent);
                    if (isPassedEdges)
                    {
                        listLines.Add(lineBTWBaseDotAndEdge);
                    }
                }
                Debug.Log($"Was found {listLines.Count()} Lines, linked the {baseDotSolution} with Edge[{currentTestingNumEdge}] ");
                switch (listLines.Count())
                {
                    case 1:
                        Debug.Log("SKIPPED: Can linked only by One Line");
                        break;
                    case 2:
                        return CreateSolutionForDot(baseDotSolution, numRecBaseDot, listLines, currentTestingNumEdge, solutionSide);
                    default:
                        break;
                }
                listLines.Clear();
            }
            foreach (Vector2 dotEdge in StoreInfoEdges.GetListDotsEdge(closestNumEdge))
            {
                //link the baseDotSolution with the same edge where it exist, it will always be possible
                listLines.Add(Line.CreateLine(baseDotSolution, dotEdge));
            }
            Debug.Log("Will create a new {SolutionForDot} on closest Edge");
            return CreateSolutionForDot(baseDotSolution, numRecBaseDot, listLines, closestNumEdge, solutionSide);
        }

        private static SolutionForDot CreateSolutionForDot(Vector2 baseDotSolution, int numRecBaseDot, List<Line> listLines, int numLastTestedEdge, SolutionSide solutionSide)
        {
            DebugFinder.DebugTurnOn(true);
            Debug.Log($"New SolutionForDot({solutionSide}) numRecBaseDot={numRecBaseDot} numLastTestedEdge={numLastTestedEdge}");
            DebugFinder.DebugDrawLine(listLines, $"SolutionForDot{solutionSide}[{numLastTestedEdge}]");
            ConnectionDot initialConnectionDot;
            if (solutionSide != SolutionSide.End)
            {
                //To ListDotsPath will be added ConnectionDot only for Start, all other (include the ConnectionDot for End) will be added in procees of Path Finding
                //List<ConnectionDot> _initialPreviousConnectionDots = new List<ConnectionDot> { };
                initialConnectionDot = new ConnectionDot(baseDotSolution, new List<ConnectionDot> { });
                DebugFinder.DebugDrawDot(baseDotSolution, $"DotForSolution{solutionSide}");
                ListDotsPath.AddConnectionDot(initialConnectionDot);
            }
            else
                initialConnectionDot = null;
            DebugFinder.DebugTurnOn(false);
            return new SolutionForDot(new SectorSolutions(listLines, baseDotSolution), numLastTestedEdge, numRecBaseDot, initialConnectionDot);
        }
    }
}