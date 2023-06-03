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
    public class PathFinderData : MonoBehaviour
    {

        [SerializeField] private Transform _prefabInitialPoint;
        [SerializeField] private Transform _findPathStartPoint;
        [SerializeField] private Transform _findPathEndPoint;
        [SerializeField] private Transform _prefabStartEdge;
        [SerializeField] private Transform _prefabEndEdge;
        [Header("RESULT")]
        //[ShowInInspector, ReadOnly] private Vector2 _startPointFindPath;
        //[ShowInInspector, ReadOnly] private Vector2 _endPointFindPath;
        //[ShowInInspector, ReadOnly] private List<Edge> _listEdges;
        [SerializeField] private Vector2 _startPointFindPath;
        [SerializeField] private Vector2 _endPointFindPath;
        [SerializeField] private List<Edge> _listEdges;

        private Transform _parentTransformAllPoint;

        public List<Edge> ListEdges => _listEdges;

        public Vector2 StartPointFindPath
        {
            get => _startPointFindPath;
            set
            {
                _startPointFindPath = value;
                SetAndActivatePoint(_findPathStartPoint, _startPointFindPath);
            }
        }

        public Vector2 EndPointFindPath
        {
            get => _endPointFindPath;
            set
            {
                _endPointFindPath = value;
                SetAndActivatePoint(_findPathEndPoint, _endPointFindPath);
            }
        }

        private void SetAndActivatePoint(Transform prefabPoint, Vector2 pointPosition, string nameEdge="")
        {
            Transform transform = Instantiate<Transform>(prefabPoint, _parentTransformAllPoint);
            transform.position = pointPosition;
            if (nameEdge != "")
                transform.name = nameEdge;
        }

        public void Init(int minNumberEdges)
        {
            _listEdges = new List<Edge>(minNumberEdges);
            _parentTransformAllPoint = transform;
        }

        public void SetInitialPoint()
        {
            SetAndActivatePoint(_prefabInitialPoint, Vector2Int.zero);
        }

        [Button]
        public void DeletePoints()
        {
            foreach (Transform item in _parentTransformAllPoint)
            {
                UnityEngine.Object.Destroy(item.gameObject);
            }
        }
        public Edge AddEdge(NormalizedRectangle firstRect, NormalizedRectangle secondRect, Vector2Int startPointOnEdge, Vector2Int endPointEdge)
        {
            Rectangle first = new Rectangle(firstRect.BottomLeftAngel, firstRect.BottomLeftAngel + firstRect.SizeXY);
            Rectangle second = new Rectangle(secondRect.BottomLeftAngel, secondRect.BottomLeftAngel + secondRect.SizeXY);
            Edge edge = new Edge(first, second, startPointOnEdge, endPointEdge);
            _listEdges.Add(edge);
            return edge;
        }

        public void ClearPreviousResults()
        {
            _listEdges.Clear();
        }

        public void CreateDebugEdgePoints(Edge edge, int numEdge)
        {
            SetAndActivatePoint(_prefabStartEdge, (Vector2)edge.Start, $"StartPointEdge{numEdge}");
            SetAndActivatePoint(_prefabEndEdge, edge.End, $"EndPointEdge{numEdge}");
        }
    }
}