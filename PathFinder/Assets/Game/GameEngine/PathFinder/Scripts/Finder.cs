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
        private Vector2 _startPointFindPath;
        private Vector2 _endPointFindPath;
        private Edge[] _arredges;

        private const int FirstNumberEdge = 0;
        private const int MaximumCountDotsHaveDirectLinkWithEndPath = 8;
        private ISolution _currentSolutionForStartPoint;
        private ISolution _solutionForEndPoint;
        private int _numLastCrossingEdgeFromSolutionEnd;

        private List<ConnectionDot> _connectionDotsHaveDirectLinkWithEndPath;

        public Finder(PathFinderData pathFinderData)
        {
            if (pathFinderData != null)
            {
                _startPointFindPath = pathFinderData.StartPointFindPath;
                _endPointFindPath = pathFinderData.EndPointFindPath;
                _arredges = pathFinderData.ListEdges.ToArray();
            }
            else
                throw new NotSupportedException("PathFinderData is Null");
        }

        public IEnumerable<Vector2> CheckDataAndGetPath()
        {
            CheckerFinderData.InitCheckerFinderDataDebuger();
            if (!CheckerFinderData.CheckData(_startPointFindPath, _endPointFindPath, _arredges))
            {
                Debug.LogError("Initial Data not passed the check. GetCheckDataAndGetPath() stoped");
                return null;
            }

            return ((IPathFinder)this).GetPath(_startPointFindPath, _endPointFindPath, _arredges);
        }

        IEnumerable<Vector2> IPathFinder.GetPath(Vector2 startPointFindPath, Vector2 endPointFindPath, IEnumerable<Edge> edges)
        {
            //DebugFinder.DebugTurnOn(active: false);
            _connectionDotsHaveDirectLinkWithEndPath = new List<ConnectionDot>(MaximumCountDotsHaveDirectLinkWithEndPath);
            _arredges = edges.ToArray();
            _startPointFindPath = startPointFindPath;
            _endPointFindPath = endPointFindPath;
            StoreInfoEdges.InitStoreInfoEdges(_arredges);
            ListDotsPath.InitListDotsPath(_arredges.Length);
            
            _currentSolutionForStartPoint = SolutionForDot.FindAndCreateSolutionForDot(_startPointFindPath, FirstNumberEdge, _arredges.Length - 1, SolutionSide.Start);
            _solutionForEndPoint = SolutionForDot.FindAndCreateSolutionForDot(_endPointFindPath, _arredges.Length - 1, FirstNumberEdge, SolutionSide.End);
            _numLastCrossingEdgeFromSolutionEnd = _solutionForEndPoint.NumLastCrossedEdgeBySolution;
            //DebugFinder.DebugTurnOn(true);
            do
            {
                if (TryLinkCurrentBaseDotSolutionStartWithEndPoint())
                    break;

                if (TryDetectThatBothSolutionOnOneEdge())
                    break;

                if (TryCrossingCurrentSolutionWithSolutionForEndPoint())
                    break;

                if (TryCreateDirectLineLinkedDotsCurrentSolutionWithSolutionForEndPoint())
                    break;

                _currentSolutionForStartPoint = SolutionForEdgeForStartPoint.FindAndCreateNewSolutionForEdgeForStartPoint(_currentSolutionForStartPoint, _arredges.Length - 1);
                
            } while (true);
            AddConnectionDotForEndPoint();
            return ListDotsPath.GetPath();
        }

        /// <summary>
        /// Try find Link between current SolutionForStartPoint with EndPoint
        /// </summary>
        /// <returns>if Link exist return true, in other case return false</returns>
        private bool TryLinkCurrentBaseDotSolutionStartWithEndPoint()
        {
            DebugFinder.DebugLogWarning("TryLinkCurrentBaseDotSolutionStartWithEndPoint");
            int numLastCrossingEdgeFromSolutionStart = _currentSolutionForStartPoint.NumLastCrossedEdgeBySolution;
            foreach (Vector2 baseDotCurrentStartSolution in _currentSolutionForStartPoint.GetListBasedDotsSolution())
            {
                (bool isPassedEdges, _) = Line.TryLinkTwoDotsThroughEdges(baseDotCurrentStartSolution, _endPointFindPath,
                    numLastCrossingEdgeFromSolutionStart, _numLastCrossingEdgeFromSolutionEnd);
                if (isPassedEdges)
                {
                    DebugFinder.DebugLog($"We found the direct Line from current Solution For StartPath to EndPath without any turns and connection dots");
                    _connectionDotsHaveDirectLinkWithEndPath = _currentSolutionForStartPoint.GetListConnectionDotsSolution().ToList();
                    return true;
                }
            }
            return false;
        }

        private void AddConnectionDotForEndPoint()
        {
            DebugFinder.DebugDrawDot(_endPointFindPath);
            DebugFinder.DebugLog($"connectionDotsHaveDirectLinkWithEndPath={_connectionDotsHaveDirectLinkWithEndPath.Count}");
            ListDotsPath.AddConnectionDot(new ConnectionDot(_endPointFindPath, _connectionDotsHaveDirectLinkWithEndPath));
        }

        /// <summary>
        /// Check that the Both Solution have dots on one Edge
        /// </summary>
        /// <returns>if Both Solution have dots on one Edge return true, in other case return false</returns>
        private bool TryDetectThatBothSolutionOnOneEdge()
        {
            DebugFinder.DebugLogWarning("TryDetectThatBothSolutionOnOneEdge");
            if (_numLastCrossingEdgeFromSolutionEnd == _currentSolutionForStartPoint.NumLastCrossedEdgeBySolution)
            {
                _connectionDotsHaveDirectLinkWithEndPath.Clear();
                DebugFinder.DebugLog($"We found the Both Solution have dots on one Edge");
                //Both Solution have dots on one Edge, the can be SolutionForDot (2 DotCross) and SolutionForEdfe (4 DotCross)
                Edge edge = _arredges[_currentSolutionForStartPoint.NumLastCrossedEdgeBySolution];
                IEnumerable<ConnectionDot> prevConnectionDots = _currentSolutionForStartPoint.GetListConnectionDotsSolution();
                AddDotCrossing(edge.Start, prevConnectionDots);
                AddDotCrossing(edge.End, prevConnectionDots);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Try find crossing of Lines from the SolutionStart with Lines from SolutionEndPoint
        /// </summary>
        /// <returns>if crossing exist return true, in other case return false</returns>
        private bool TryCrossingCurrentSolutionWithSolutionForEndPoint()
        {
            DebugFinder.DebugLogWarning("TryCrossingCurrentSolutionWithSolutionForEndPoint");
            Dictionary<Vector2, List<ConnectionDot>> connectionDotsOnLastCrossingEdgeFromCurrentSolution = new Dictionary<Vector2, List<ConnectionDot>>(2);
            _connectionDotsHaveDirectLinkWithEndPath.Clear();
            int numLastCrossingEdgeFromStart = _currentSolutionForStartPoint.NumLastCrossedEdgeBySolution;
            int numRecBaseDotStart = _currentSolutionForStartPoint.NumRecBaseDotSolution;
            foreach ((SectorSolutions currentSolutionStart, ConnectionDot connectionDot) in _currentSolutionForStartPoint.GetSectorSolutionsWithConnectionDots())
            {
                Vector2 baseDotStart = currentSolutionStart.baseDotSectorSolutions;
                foreach (Line lineSolutionStart in currentSolutionStart.GetListLines())
                {
                    foreach (Line lineSolutionEnd in _solutionForEndPoint.GetListLinesFromSectorSolutions())
                    {
                        (bool isCrossing, Vector2 dotCrossing) = CrossLineSolutionWithLine(lineSolutionStart, lineSolutionEnd);
                        if (isCrossing)
                        {
                            if (StoreInfoEdges.IsDotOnEdge(dotCrossing, numLastCrossingEdgeFromStart))
                            {
                                DebugFinder.DebugLog($"Found DotCrossing On LastCrossingEdge[{numLastCrossingEdgeFromStart}] FromCurrentSolution");
                                //In this case the creation of DotCrossing will postpone till collect the all connectionDot with connect by this DotCrossing
                                //In case if _currentSolutionForStartPoint is SolutionForEdgeForStartPoint the DotCrossing will connect two DotCrossing from SolutionForEdge
                                //to exclude the case when the same DotCrossing will add twice to ListDotsPath (but with different prevDotCrossing)
                                PostponeAddDotCrossingOnLastCrossingEdgeFromCurrentSolution(dotCrossing, connectionDot,
                                    connectionDotsOnLastCrossingEdgeFromCurrentSolution);
                            }
                            else if (StoreInfoEdges.IsDotOnEdge(dotCrossing, _numLastCrossingEdgeFromSolutionEnd))
                            {
                                DebugFinder.DebugLog($"Found DotCrossing On LastCrossingEdge[{_numLastCrossingEdgeFromSolutionEnd}] FromSolutionEnd");
                                AddDotCrossing(dotCrossing, new List<ConnectionDot>() { connectionDot });
                            }
                            else if (StoreInfoEdges.IsDotCrossingBetweenBaseDotAndEdge(dotCrossing, baseDotStart, numLastCrossingEdgeFromStart))
                            {//DotCrossing Between BaseDotStart And LastCrossingEdgeFromCurrentSolution
                                DebugFinder.DebugLog("Will Check DotCrossing Between BaseDotStart And LastCrossingEdgeFromCurrentSolution");
                                (bool dotInRec, int numRect) = StoreInfoEdges.IsDotInRectBetweenRecBaseDotAndRectEdge(dotCrossing, numRecBaseDotStart, numLastCrossingEdgeFromStart,
                                    SolutionSide.Start);
                                if (dotInRec && IsLinePassEdges(lineSolutionEnd, _numLastCrossingEdgeFromSolutionEnd, numRect, SolutionSide.Start))
                                    AddDotCrossing(dotCrossing, new List<ConnectionDot>() { connectionDot });
                            }
                            else if (StoreInfoEdges.IsDotCrossingBetweenBaseDotAndEdge(dotCrossing, _endPointFindPath, _numLastCrossingEdgeFromSolutionEnd))
                            {//DotCrossing Between _numLastCrossingEdgeFromSolutionEnd and BaseDotEnd
                                DebugFinder.DebugLog("Will Check DotCrossing Between _numLastCrossingEdgeFromSolutionEnd and BaseDotEnd");
                                (bool dotInRec, int numRect) = StoreInfoEdges.IsDotInRectBetweenRecBaseDotAndRectEdge(dotCrossing, _solutionForEndPoint.NumRecBaseDotSolution,
                                    _numLastCrossingEdgeFromSolutionEnd, SolutionSide.End);
                                if (dotInRec && IsLinePassEdges(lineSolutionStart, numLastCrossingEdgeFromStart, numRect, SolutionSide.End))
                                    AddDotCrossing(dotCrossing, new List<ConnectionDot>() { connectionDot });
                            }
                            else
                            {//Dot cross Between edges Solution Start and End
                                (bool dotInRec, int numRect) = StoreInfoEdges.IsDotInRectBetweenEdges(dotCrossing, numLastCrossingEdgeFromStart, _numLastCrossingEdgeFromSolutionEnd);
                                if (dotInRec)
                                {
                                    DebugFinder.DebugLog($"Will Check DotCrossing in Rect[{numRect}] BetweenEdges");
                                    bool rezLineSolutionStart = IsLinePassEdges(lineSolutionStart, numLastCrossingEdgeFromStart, numRect, SolutionSide.End);
                                    bool rezLineSolutionEnd = IsLinePassEdges(lineSolutionEnd, _numLastCrossingEdgeFromSolutionEnd, numRect, SolutionSide.Start);
                                    if (rezLineSolutionStart && rezLineSolutionEnd)
                                        AddDotCrossing(dotCrossing, new List<ConnectionDot>() { connectionDot });
                                }
                            }
                        }
                    }

                }
            }
            if (connectionDotsOnLastCrossingEdgeFromCurrentSolution.Count != 0)
                CreatePostponedDotsCrossingOnLastCrossingEdgeFromCurrentSolution(connectionDotsOnLastCrossingEdgeFromCurrentSolution);
            return _connectionDotsHaveDirectLinkWithEndPath.Count != 0;
        }

        /// <summary>
        /// Create the postponed DotCrossing from collected information
        /// </summary>
        private void CreatePostponedDotsCrossingOnLastCrossingEdgeFromCurrentSolution(Dictionary<Vector2, List<ConnectionDot>> connectionDotsOnLastCrossingEdgeFromCurrentSolution)
        {
            foreach (KeyValuePair<Vector2,List<ConnectionDot>> item in connectionDotsOnLastCrossingEdgeFromCurrentSolution)
            {
                //Vector2 dotCrossing = item.Key;
                //List<ConnectionDot> prevConnectionDots = item.Value;
                AddDotCrossing(item.Key, item.Value);
            }
        }
        /// <summary>
        /// Collect Information and postone the creation of DotCrossing
        /// </summary>
        private void PostponeAddDotCrossingOnLastCrossingEdgeFromCurrentSolution(Vector2 dotCrossing, ConnectionDot preConnectionDot,
            Dictionary<Vector2, List<ConnectionDot>> connectionDotsOnLastCrossingEdgeFromCurrentSolution)
        {
            if (connectionDotsOnLastCrossingEdgeFromCurrentSolution.TryGetValue(dotCrossing, out List<ConnectionDot> listprevConnectDots))
                listprevConnectDots.Add(preConnectionDot);
            else
                connectionDotsOnLastCrossingEdgeFromCurrentSolution.Add(dotCrossing, new List<ConnectionDot>(2) { preConnectionDot });
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

        private void AddDotCrossing(Vector2 dotCrossing, IEnumerable<ConnectionDot> prevConnectionDots)
        {
            DebugFinder.DebugDrawDot(dotCrossing);
            ConnectionDot connectionDot = new ConnectionDot(dotCrossing, prevConnectionDots);
            ListDotsPath.AddConnectionDot(connectionDot);
            _connectionDotsHaveDirectLinkWithEndPath.Add(connectionDot);
        }

        private bool IsLinePassEdges(Line lineOtherSolution, int numLastCrossingEdgeByLineOtherSolution, int numRectWhereDotCrossing, SolutionSide solutionSide)
        {
            int numEgde = StoreInfoEdges.GetNumEdge(numRectWhereDotCrossing, solutionSide);
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
            for (int currentNumTestingEdge = numEdgeStart; currentNumTestingEdge <= numEdgeEnd; currentNumTestingEdge++)
            {
                if (!line.TryIntersecLineWithEdge(currentNumTestingEdge))
                {
                    directLineBTWdotsExist = false;
                    break;
                }
            }
            return directLineBTWdotsExist;
        }

        /*
         * Can support any possible case include next rules:
         * - the dots from currentSolutionForStartPoint can linked with any number of dots from SolutionEnd
         * - the dots from SolutionEnd can linked with any number of dots from currentSolutionForStartPoint
         * - any ConnectionDot must only one time be included in ListDotsPath
         * - ConnectionDot in ListDotsPath can be insert in any order (position in the list does not affect their relationship)
         */
        /// <summary>
        /// Try create direct Line between baseDot of SectorSolutions from current SolutionStart with baseDot of SectorSolutions from SolutionForEndPoint
        /// </summary>
        /// <returns>if the lines created return true, in other case return false</returns>
        private bool TryCreateDirectLineLinkedDotsCurrentSolutionWithSolutionForEndPoint()
        {
            DebugFinder.DebugLogWarning("TryCreateDirectLineLinkedDotsCurrentSolutionWithSolutionForEndPoint");
            int numEdgeCurrentSolutionStart = _currentSolutionForStartPoint.NumLastCrossedEdgeBySolution;
            IEnumerable<ConnectionDot> prevConnectionDotsForCurrentSolutionStart = _currentSolutionForStartPoint.GetListConnectionDotsSolution();
            DebugFinder.DebugLog($"Trying link dots of current Solution on edge[{numEdgeCurrentSolutionStart}] with SolutionForEndPoint on edge[{_numLastCrossingEdgeFromSolutionEnd}]");
            Dictionary<Vector2, ConnectionDot> connectionDotsSolutionStart = new Dictionary<Vector2, ConnectionDot>(2);
            List<ConnectionDot> connectionDotsForCurrentDotSolutionEnd;
            _connectionDotsHaveDirectLinkWithEndPath.Clear();
            foreach (Vector2 dotEdgeEnd in StoreInfoEdges.GetListDotsEdge(_numLastCrossingEdgeFromSolutionEnd))
            {
                connectionDotsForCurrentDotSolutionEnd = new List<ConnectionDot>(2);
                foreach (Vector2 dotEdgeStart in StoreInfoEdges.GetListDotsEdge(numEdgeCurrentSolutionStart))
                {
                    (bool isPassedEdges, _) = Line.TryLinkTwoDotsThroughEdges(dotEdgeStart, dotEdgeEnd, numEdgeCurrentSolutionStart,
                        _numLastCrossingEdgeFromSolutionEnd);
                    if (isPassedEdges)
                    {
                        // for "any ConnectionDot must only one time be included in ListDotsPath"
                        // if exist the Connection dots for dotEdgeStart not create additional ConnectionDots for this dotEdgeStart (in case if two dotEdgeEnd use the same)
                        // All ConnectionDots for dotEdgeStart collected in connectionDotsForCurrentDotSolutionEnd
                        // and use for create connection for dotEdgeEnd which will add to _connectionDotsHaveDirectLinkWithEndPath
                        // here created the connectionDots as for dotEdgeStart and also for dotEdgeEnd
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
                    DebugFinder.DebugLog($"We found [{connectionDotsForCurrentDotSolutionEnd.Count}] direct lines between the dotEndPath {dotEdgeEnd} and the dots of CurrentSolutionStart");
                    AddDotCrossing(dotEdgeEnd, connectionDotsForCurrentDotSolutionEnd);
                }
            }
            return _connectionDotsHaveDirectLinkWithEndPath.Count != 0;
        }
    }
}