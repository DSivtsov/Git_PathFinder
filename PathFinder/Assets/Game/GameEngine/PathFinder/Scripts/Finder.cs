using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using GameEngine.Environment;
using GMTools.Math;


namespace GameEngine.PathFinder
{
    public class Finder : IPathFinder
    {
        private const int FirstNumberEdge = 0;
        private Edge[] _arredges;
        private ISolution _currentSolutionForStartPoint;
        private ISolution _solutionForEndPoint;
        private int _numLastCrossingEdgeFromSolutionEnd;
        private Vector2 _startPointFindPath;
        private Vector2 _endPointFindPath;
        IEnumerable<Vector2> IPathFinder.GetPath(Vector2 startPointFindPath, Vector2 endPointFindPath, IEnumerable<Edge> edges)
        {
            DebugFinder.DebugTurnOn(active: false);

            _arredges = edges.ToArray();
            _startPointFindPath = startPointFindPath;
            _endPointFindPath = endPointFindPath;
            StoreInfoEdges.InitStoreInfoEdges(_arredges);

            ListDotsPath.InitListDotsPath(_arredges.Length);
            
            _currentSolutionForStartPoint = SolutionForDot.FindAndCreateSolutionForDot(_startPointFindPath, FirstNumberEdge, _arredges.Length - 1, SolutionSide.Start);


            _solutionForEndPoint = SolutionForDot.FindAndCreateSolutionForDot(_endPointFindPath, _arredges.Length - 1, FirstNumberEdge, SolutionSide.End);
            _numLastCrossingEdgeFromSolutionEnd = _solutionForEndPoint.NumLastCrossedEdgeBySolution;
            DebugFinder.DebugTurnOn(true);
            IEnumerable<Vector2> path;
            do
            {
                path = TryLinkCurrentBaseDotSolutionStartWithEndPoint();
                if (path != null) return path;

                path = TryDetectThatBothSolutionOnOneEdge();
                if (path != null) return path;

                path = TryCrossingCurrentSolutionWithSolutionForEndPoint();
                if (path != null) return path;

                path = TryCreateDirectLineLinkedDotsCurrentSolutionWithSolutionForEndPoint();
                if (path != null) return path;

                _currentSolutionForStartPoint = SolutionForEdgeForStartPoint.FindAndCreateNewSolutionForEdgeForStartPoint(_currentSolutionForStartPoint, _arredges.Length - 1);
                
            } while (true);
            throw new NotSupportedException("This is not possible, inaccessible point, we must exit early or get an exception");
        }

        /// <summary>
        /// Try find Link between current SolutionForStartPoint with SolutionForEndPoint
        /// </summary>
        /// <returns>if Link exist return path, in other case return null</returns>
        private IEnumerable<Vector2> TryLinkCurrentBaseDotSolutionStartWithEndPoint()
        {
            Debug.LogWarning("TryLinkCurrentBaseDotSolutionStartWithEndPoint");
            int numLastCrossingEdgeFromSolutionStart = _currentSolutionForStartPoint.NumLastCrossedEdgeBySolution;
            foreach (Vector2 baseDotCurrentStartSolution in _currentSolutionForStartPoint.GetListBasedDotsSolution())
            {
                (bool isPassedEdges, _) = Line.TryLinkTwoDotsThroughEdges(baseDotCurrentStartSolution, _endPointFindPath,
                    numLastCrossingEdgeFromSolutionStart, _numLastCrossingEdgeFromSolutionEnd);
                if (isPassedEdges)
                {
                    Debug.LogWarning($"We found the direct Line from current Solution For StartPath to EndPath without any turns");
                    DebugFinder.DebugDrawDot(_endPointFindPath);
                    ConnectionDot connectionDotEndPath = new ConnectionDot(_endPointFindPath, _currentSolutionForStartPoint.GetListConnectionDotsSolution());
                    ListDotsPath.AddConnectionDot(connectionDotEndPath);
                    return ListDotsPath.GetPath();
                }
            }
            return null;
        }

        /// <summary>
        /// Check that the Both Solution have dots on one Edge
        /// </summary>
        /// <returns>if Both Solution have dots on one Edge return path, in other case return null</returns>
        private IEnumerable<Vector2> TryDetectThatBothSolutionOnOneEdge()
        {
            Debug.LogWarning("TryDetectThatBothSolutionOnOneEdge");
            if (_numLastCrossingEdgeFromSolutionEnd == _currentSolutionForStartPoint.NumLastCrossedEdgeBySolution)
            {
                Debug.LogWarning($"We found the Both Solution have dots on one Edge");
                //Both Solution have dots on one Edge, the can be SolutionForDot (2 DotCross) and SolutionForEdfe (4 DotCross)
                IEnumerable<ConnectionDot> connectionDotsHaveDirectLinkWithEndPath = BothSolutionHaveDotsOnOneEdge();
                DebugFinder.DebugDrawDot(_endPointFindPath);
                ConnectionDot connectionDotEndPath = new ConnectionDot(_endPointFindPath, connectionDotsHaveDirectLinkWithEndPath);
                ListDotsPath.AddConnectionDot(connectionDotEndPath);
                return ListDotsPath.GetPath();
            }
            return null;
        }

        private IEnumerable<ConnectionDot> BothSolutionHaveDotsOnOneEdge()
        {
            Edge edge = _arredges[_currentSolutionForStartPoint.NumLastCrossedEdgeBySolution];
            IEnumerable<ConnectionDot> prevConnectionDots = _currentSolutionForStartPoint.GetListConnectionDotsSolution();
            DebugFinder.DebugDrawDot(edge.Start);
            ConnectionDot connectionDotEdgeStart = new ConnectionDot(edge.Start, prevConnectionDots);
            ListDotsPath.AddConnectionDot(connectionDotEdgeStart);
            DebugFinder.DebugDrawDot(edge.End);
            ConnectionDot connectionDotEdgeEnd = new ConnectionDot(edge.End, prevConnectionDots);
            ListDotsPath.AddConnectionDot(connectionDotEdgeEnd);
            return new List<ConnectionDot> { connectionDotEdgeStart, connectionDotEdgeEnd };
        }

        /// <summary>
        /// Try find crossing of Lines from the SolutionStart with Lines from SolutionEndPoint
        /// </summary>
        /// <returns>if crossing exist return path, in other case return null</returns>
        private IEnumerable<Vector2> TryCrossingCurrentSolutionWithSolutionForEndPoint()
        {
            Debug.LogWarning("TryCrossingCurrentSolutionWithSolutionForEndPoint");
            int numLastCrossingEdgeFromStart = _currentSolutionForStartPoint.NumLastCrossedEdgeBySolution;
            int numRecBaseDotStart = _currentSolutionForStartPoint.NumRecBaseDotSolution;
            List<ConnectionDot> connectionDotsHaveDirectLinkWithEndPath = new List<ConnectionDot>(8);
            foreach (SectorSolutions currentSolutionStart in _currentSolutionForStartPoint.GetListSectorSolutions())
            {
                Vector2 baseDotStart = currentSolutionStart.baseDotSectorSolutions;

                IEnumerable<ConnectionDot> prevConnectionDots = _currentSolutionForStartPoint.GetListConnectionDotsSolution();
                foreach (Line lineSolutionStart in currentSolutionStart.GetListLines())
                {
                    foreach (Line lineSolutionEnd in _solutionForEndPoint.GetListLinesFromSectorSolutions())
                    {
                        (bool isCrossing, Vector2 dotCrossing) = CrossLineSolutionWithLine(lineSolutionStart, lineSolutionEnd);
                        if (isCrossing)
                        {
                            if (StoreInfoEdges.IsDotOnEdge(dotCrossing, numLastCrossingEdgeFromStart))
                            {
                                //Debug.Log("Exist DotCrossing On LastCrossingEdge FromCurrentSolution");
                                AddDotCrossing(dotCrossing, prevConnectionDots, connectionDotsHaveDirectLinkWithEndPath);
                            }
                            else if (StoreInfoEdges.IsDotOnEdge(dotCrossing, _numLastCrossingEdgeFromSolutionEnd))
                            {
                                //Debug.Log("Exist DotCrossing On _numLastCrossingEdge FromSolutionEnd");
                                AddDotCrossing(dotCrossing, prevConnectionDots, connectionDotsHaveDirectLinkWithEndPath);
                            }
                            else if (StoreInfoEdges.IsDotCrossingBetweenBaseDotAndEdge(dotCrossing, baseDotStart, numLastCrossingEdgeFromStart))
                            {//DotCrossing Between BaseDotStart And LastCrossingEdgeFromCurrentSolution
                                //Debug.Log("Check Existed DotCrossing Between BaseDotStart And LastCrossingEdgeFromCurrentSolution");
                                (bool dotInRec, int numRect) = StoreInfoEdges.IsDotInRectBetweenRecBaseDotAndRectEdge(dotCrossing, numRecBaseDotStart, numLastCrossingEdgeFromStart,
                                    SolutionSide.Start);
                                if (dotInRec && IsLinePassEdges(lineSolutionEnd, _numLastCrossingEdgeFromSolutionEnd, numRect, SolutionSide.Start))
                                    AddDotCrossing(dotCrossing, prevConnectionDots, connectionDotsHaveDirectLinkWithEndPath);
                            }
                            else if (StoreInfoEdges.IsDotCrossingBetweenBaseDotAndEdge(dotCrossing, _endPointFindPath, _numLastCrossingEdgeFromSolutionEnd))
                            {//DotCrossing Between _numLastCrossingEdgeFromSolutionEnd and BaseDotEnd
                                //Debug.Log("Check Existed DotCrossing Between _numLastCrossingEdgeFromSolutionEnd and BaseDotEnd");
                                (bool dotInRec, int numRect) = StoreInfoEdges.IsDotInRectBetweenRecBaseDotAndRectEdge(dotCrossing, _solutionForEndPoint.NumRecBaseDotSolution,
                                    _numLastCrossingEdgeFromSolutionEnd, SolutionSide.End);
                                if (dotInRec && IsLinePassEdges(lineSolutionStart, numLastCrossingEdgeFromStart, numRect, SolutionSide.End))
                                    AddDotCrossing(dotCrossing, prevConnectionDots, connectionDotsHaveDirectLinkWithEndPath);
                            }
                            else
                            {//Dot cross Between edges Solution Start and End
                                //Debug.Log("Check Existed Dot cross Between edges Solution Start and End");
                                (bool dotInRec, int numRect) = StoreInfoEdges.IsDotInRectBetweenEdges(dotCrossing, numLastCrossingEdgeFromStart, _numLastCrossingEdgeFromSolutionEnd);
                                if (dotInRec)
                                {
                                      bool rezLineSolutionStart = IsLinePassEdges(lineSolutionStart, numLastCrossingEdgeFromStart, numRect, SolutionSide.End);
                                    bool rezLineSolutionEnd = IsLinePassEdges(lineSolutionEnd, _numLastCrossingEdgeFromSolutionEnd, numRect, SolutionSide.Start);
                                    if (rezLineSolutionStart && rezLineSolutionEnd)
                                        AddDotCrossing(dotCrossing, prevConnectionDots, connectionDotsHaveDirectLinkWithEndPath);
                                }
                            }
                        }
                    }

                }
            }
            Debug.Log($"{this}: numDotHaveCrossingwithEndPoint[{connectionDotsHaveDirectLinkWithEndPath.Count}]");
            if (connectionDotsHaveDirectLinkWithEndPath.Count != 0)
            {
                //It means that we have found a Path all dots in _listDotCrossing
                DebugFinder.DebugDrawDot(_endPointFindPath);
                ConnectionDot connectionDotEndPath = new ConnectionDot(_endPointFindPath, connectionDotsHaveDirectLinkWithEndPath);
                ListDotsPath.AddConnectionDot(connectionDotEndPath);
                return ListDotsPath.GetPath();
            }
            return null;
        }

        private (bool isCrossing, Vector2 dotCrossing) CrossLineSolutionWithLine(Line currentLineSolution, Line currentLineEndPoint)
        {
            (Matrix2x2 matrix, VectorB2 b) = CreateDataForLinearSystemEquation(currentLineSolution, currentLineEndPoint);
            VectorB2 vector = LinearSystemEquation.GetSolutionLinearSystemEquation(matrix, b);
            if (vector == null)
            {
                //Debug.LogError($"Can't find Solution for crossing line({currentLineSolution}) with line ({currentLineEndPoint})");
                return (false, Vector2.zero);
            }
            return (true, (Vector2)vector);
        }
        private (Matrix2x2 matrixFactors, VectorB2 b) CreateDataForLinearSystemEquation(Line currentLineSolution, Line currentLineEndPoint)
        {
            (float a11, float a12, float b1) = currentLineSolution.GetDataForMatrix2x2();
            (float a21, float a22, float b2) = currentLineEndPoint.GetDataForMatrix2x2();
            return (new Matrix2x2(a11, a12, a21, a22), new VectorB2(b1, b2));
        }

        private void AddDotCrossing(Vector2 dotCrossing, IEnumerable<ConnectionDot> prevConnectionDots, List<ConnectionDot> connectionDotsHaveDirectLinkWithEndPath)
        {
            DebugFinder.DebugDrawDot(dotCrossing);
            ConnectionDot connectionDot = new ConnectionDot(dotCrossing, prevConnectionDots);
            connectionDotsHaveDirectLinkWithEndPath.Add(connectionDot);
            ListDotsPath.AddConnectionDot(connectionDot);
        }

        private bool IsLinePassEdges(Line lineOtherSolution, int numLastCrossingEdgeByLineOtherSolution, int numRectWhereDotCrossing, SolutionSide solutionSide)
        {
            int numEgde = StoreInfoEdges.GetNumEdge(numRectWhereDotCrossing, solutionSide);
            //Debug.Log($"numRectWhereDotCrossing[{numRectWhereDotCrossing}] numEgde={numEgde} numLastCrossingEdgeFromSolution[{numLastCrossingEdgeByLineOtherSolution}] " +
            //    $"solutionSide[{solutionSide}]");
            switch (solutionSide)
            {
                case SolutionSide.Start:
                    return IsLinePassedThroughEdges(lineOtherSolution, numEgde, numLastCrossingEdgeByLineOtherSolution - 1);
                case SolutionSide.End:
                    return IsLinePassedThroughEdges(lineOtherSolution, numLastCrossingEdgeByLineOtherSolution + 1, numEgde);
                default:
                    throw new NotSupportedException($"Value [{solutionSide}] is not supported");
            }
        }

        private static bool IsLinePassedThroughEdges(Line line, int numEdgeStart, int numEdgeEnd)
        {
            bool directLineBTWdotsExist = true;
            //Debug.Log($"IsLinePassedThroughEdges() numEdgeStart[{numEdgeStart}] numEdgeEnd[{numEdgeEnd}]");
            for (int currentNumTestingEdge = numEdgeStart; currentNumTestingEdge <= numEdgeEnd; currentNumTestingEdge++)
            {
                if (!line.TryIntersecLineWithEdge(currentNumTestingEdge))
                {
                    //Debug.Log($"Intersec Line Not crossing Edge[{currentNumTestingEdge}]");
                    directLineBTWdotsExist = false;
                    break;
                }
            }
            return directLineBTWdotsExist;
        }

        /// <summary>
        /// Try create direct Line between baseDot of SectorSolutions from current SolutionStart with baseDot of SectorSolutions from SolutionForEndPoint
        /// </summary>
        /// <returns>if the lines created return path, in other case return null</returns>
        private IEnumerable<Vector2> TryCreateDirectLineLinkedDotsCurrentSolutionWithSolutionForEndPoint()
        {
            Debug.LogWarning("TryCreateDirectLineLinkedDotsCurrentSolutionWithSolutionForEndPoint");
            int numEdgeCurrentSolutionStart = _currentSolutionForStartPoint.NumLastCrossedEdgeBySolution;
            IEnumerable<ConnectionDot> prevConnectionDotsForCurrentSolutionStart = _currentSolutionForStartPoint.GetListConnectionDotsSolution();
            Debug.Log($"Trying link dots of current Solution on edge[{numEdgeCurrentSolutionStart}] with SolutionForEndPoint on edge[{_numLastCrossingEdgeFromSolutionEnd}]");

            Dictionary<Vector2, ConnectionDot> connectionDotsSolutionStart = new Dictionary<Vector2, ConnectionDot>(2);
            /*
             * Can support any possible case include next rules:
             * - the dots from currentSolutionForStartPoint can linked with any number of dots from SolutionEnd
             * - the dots from SolutionEnd can linked with any number of dots from currentSolutionForStartPoint
             * - any ConnectionDot must only one time be included in ListDotsPath
             * - ConnectionDot in ListDotsPath can be insert in any order (position in the list does not affect their relationship)
             */
            List<ConnectionDot> connectionDotsForCurrentDotSolutionEnd;
            List<ConnectionDot> connectionDotsHaveDirectLinkWithEndPath = new List<ConnectionDot>(2);
            foreach (Vector2 dotEdgeEnd in StoreInfoEdges.GetListDotsEdge(_numLastCrossingEdgeFromSolutionEnd))
            {
                connectionDotsForCurrentDotSolutionEnd = new List<ConnectionDot>(2);
                foreach (Vector2 dotEdgeStart in StoreInfoEdges.GetListDotsEdge(numEdgeCurrentSolutionStart))
                {
                    (bool isPassedEdges, _) = Line.TryLinkTwoDotsThroughEdges(dotEdgeStart, dotEdgeEnd, numEdgeCurrentSolutionStart,
                        _numLastCrossingEdgeFromSolutionEnd);
                    if (isPassedEdges)
                    {
                        bool existKey = connectionDotsSolutionStart.TryGetValue(dotEdgeStart, out ConnectionDot connectionDotOnEdgeCurrentSolutionStart);
                        if (!existKey)
                        {
                            DebugFinder.DebugDrawDot(dotEdgeStart);
                            connectionDotOnEdgeCurrentSolutionStart = new ConnectionDot(dotEdgeStart, prevConnectionDotsForCurrentSolutionStart);
                            ListDotsPath.AddConnectionDot(connectionDotOnEdgeCurrentSolutionStart);
                            connectionDotsSolutionStart.Add(dotEdgeStart, connectionDotOnEdgeCurrentSolutionStart);
                        }
                        connectionDotsForCurrentDotSolutionEnd.Add(connectionDotOnEdgeCurrentSolutionStart);
                    }
                }
                if (connectionDotsForCurrentDotSolutionEnd.Count != 0 )
                {
                    DebugFinder.DebugDrawDot(dotEdgeEnd);
                    ConnectionDot connectionDotFromFromSolutionEnd = new ConnectionDot(dotEdgeEnd, connectionDotsForCurrentDotSolutionEnd);
                    Debug.Log($"We found [{connectionDotsForCurrentDotSolutionEnd.Count}] direct lines between the dotEndPath {dotEdgeEnd} and the dots of CurrentSolutionStart");
                    ListDotsPath.AddConnectionDot(connectionDotFromFromSolutionEnd);
                    connectionDotsHaveDirectLinkWithEndPath.Add(connectionDotFromFromSolutionEnd);
                }
            }

            if (connectionDotsHaveDirectLinkWithEndPath.Count != 0)
            {
                //It means that we have found a direct lines between the edge of currentSolution and the solution for PointEndPath
                DebugFinder.DebugDrawDot(_endPointFindPath);
                ConnectionDot connectionDotEndPath = new ConnectionDot(_endPointFindPath, connectionDotsHaveDirectLinkWithEndPath);
                ListDotsPath.AddConnectionDot(connectionDotEndPath);
                return ListDotsPath.GetPath();
            }
            return null;
        }
    }
}