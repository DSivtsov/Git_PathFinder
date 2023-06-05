using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace GameEngine.Environment
{
    [Serializable]
    public struct Rectangle
    {
        public Vector2 Min;
        public Vector2 Max;

        public Rectangle(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }
    }
    [Serializable]
    public struct Edge
    {
        public Rectangle First;
        public Rectangle Second;
        public Vector2 Start;
        public Vector2 End;

        public Edge(Rectangle first, Rectangle second, Vector2 start, Vector2 end)
        {
            First = first;
            Second = second;
            Start = start;
            End = end;
        }
    }
    public interface IPathFinder
    {
        IEnumerable<Vector2> GetPath(Vector2 startPointFindPath, Vector2 endPointFindPath, IEnumerable<Edge> edges);
    }

    [Serializable]
    public class PathFinderData
    {
        [ShowInInspector] private Vector2 _startPointFindPath;
        [ShowInInspector] private Vector2 _endPointFindPath;
        [ShowInInspector] private List<Edge> _listEdges;

        private PathFinderDataShow _pathFinderDataShow;

        public PathFinderData(int minNumberEdges)
        {
            _listEdges = new List<Edge>(minNumberEdges);
            _pathFinderDataShow = UnityEngine.Object.FindObjectOfType<PathFinderDataShow>();
        }

        public List<Edge> ListEdges => _listEdges;

        public Vector2 StartPointFindPath
        {
            get => _startPointFindPath;
            set
            {
                _startPointFindPath = value;
                _pathFinderDataShow.DrawStartPoint(_startPointFindPath);
            }
        }

        public Vector2 EndPointFindPath
        {
            get => _endPointFindPath;
            set
            {
                _endPointFindPath = value;
                _pathFinderDataShow.DrawEndPoint(_endPointFindPath);
            }
        }

        public void SetInitialPoint()
        {
            _pathFinderDataShow.DrawInitialPoint();
        }


        public void AddEdge(NormalizedRectangle firstRect, NormalizedRectangle secondRect, Vector2Int startPointOnEdge, Vector2Int endPointEdge, int numEdge)
        {
            Rectangle first = new Rectangle(firstRect.BottomLeftAngel, firstRect.BottomLeftAngel + firstRect.SizeXY);
            Rectangle second = new Rectangle(secondRect.BottomLeftAngel, secondRect.BottomLeftAngel + secondRect.SizeXY);
            Edge edge = new Edge(first, second, startPointOnEdge, endPointEdge);
            _listEdges.Add(edge);
            _pathFinderDataShow.DrawEdgePoints(edge, numEdge);
        }

        public void ClearPreviousResults()
        {
            _listEdges.Clear();
        }


        public override string ToString()
        {
            return $"StartPointFindPath{_startPointFindPath} EndPointFindPath{_endPointFindPath}" +
                $" _listEdges.Count[{_listEdges.Count}]";
        }
    }
}