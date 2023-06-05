using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using GameEngine.Environment;


namespace GameEngine.PathFinder
{
    [Serializable]
    public class CheckerFinderData
    {
        private static CheckerFinderDataDebuger _checkerFinderDataDebuger;

        public static void InitCheckerFinderDataDebuger()
        {
            _checkerFinderDataDebuger = UnityEngine.Object.FindObjectOfType<CheckerFinderDataDebuger>();
            //_checkerFinderDataDebuger.InitCheckerInitialDataDebuger();
        }

        public static bool CheckData(Vector2 startPointFindPath, Vector2 endPointFindPath, Edge[] arrEdges)
        {

            if (startPointFindPath == null || endPointFindPath == null || arrEdges == null)
            {
                Debug.LogError("Initial Data not intialized. GetPath() stoped.");
                return false;
            }

            if (arrEdges.Length == 0)
            {
                Debug.LogError("Absent Edges in Initial Data. GetPath() stoped.");
                return false;
            }

            if (CheckExistOverlapingRectangle(arrEdges))
            {
                Debug.LogError("Exist overlaping rectangles in Initial Data. GetPath() stoped.");
                return false;
            }

            Debug.Log($"CheckData() passed. No detected errors. Found [{arrEdges.Length}] Edges");
            return true;
        }

        /// <summary>
        /// Does exist the overlaping of Rectangles
        /// </summary>
        /// <returns>true if it exist</returns>
        private static bool CheckExistOverlapingRectangle(Edge[] arrEdges)
        {
            Rectangle[] allRectangles = GetListAllRectangles(arrEdges);
            for (int idxCheckedRec = 0; idxCheckedRec < allRectangles.Length - 1; idxCheckedRec++)
            {
                Rectangle checkedRec = allRectangles[idxCheckedRec];
                for (int idxOtherRec = idxCheckedRec + 1; idxOtherRec < allRectangles.Length; idxOtherRec++)
                {
                    if (ExistOverlapingRectangle(checkedRec, allRectangles[idxOtherRec]))
                    {
                        Debug.Log($"idxCheckedRec[{idxCheckedRec}] OtherRect[{idxOtherRec}] ExistOverlapingRectangle[true]");
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool ExistOverlapingRectangle(Rectangle checkedRec, Rectangle otherRec)
        {
            RectEdge[] edges = GetEdgesFromRectangle(otherRec);
            for (int numEdge = 0; numEdge < edges.Length; numEdge++)
            {
                RectEdge currentEdge = edges[numEdge];
                if (currentEdge.IsEdgeCrossingCheckedRect(checkedRec))
                {
                    Debug.Log($"EndgeType[{currentEdge.EdgeType}] Is CrossingRect[{_checkerFinderDataDebuger.ShowRect(checkedRec)}]");
                    _checkerFinderDataDebuger.ShowPoILine(currentEdge);
                    return true;
                }
            }
            return false;
        }

        private static RectEdge[] GetEdgesFromRectangle(Rectangle otherRec)
        {
            //Order Points in Line given related to standard Unity Axis line corespondce to these directions
            return new RectEdge[]
            {
                new RectEdge(new Vector2(otherRec.Min.x,otherRec.Max.y), new Vector2(otherRec.Max.x,otherRec.Max.y), EdgeType.Horizintal), //Top = 0,
                new RectEdge(new Vector2(otherRec.Max.x,otherRec.Min.y), new Vector2(otherRec.Max.x,otherRec.Max.y), EdgeType.Vertical), //Right = 1,
                new RectEdge(new Vector2(otherRec.Min.x,otherRec.Min.y), new Vector2(otherRec.Max.x,otherRec.Min.y), EdgeType.Horizintal), //Bottom = 2,
                new RectEdge(new Vector2(otherRec.Min.x,otherRec.Min.y), new Vector2(otherRec.Min.x,otherRec.Max.y), EdgeType.Vertical), //Left = 3,
            };
        }

        //private static Vector2[] GetAnglesFromRectangle(Rectangle otherRec)
        //{
        //    return new Vector2[]
        //    {
        //        new Vector2(otherRec.Min.x,otherRec.Max.y), //TopLeft = 0,
        //        new Vector2(otherRec.Max.x,otherRec.Max.y), //TopRight = 1,
        //        new Vector2(otherRec.Max.x,otherRec.Min.y), //BottomRight = 2,
        //        new Vector2(otherRec.Min.x,otherRec.Min.y), //BottomLeft = 3,
        //    };
        //}

        private static Rectangle[] GetListAllRectangles(Edge[] arrEdges)
        {
            Rectangle[] allRectangles = new Rectangle[arrEdges.Length + 1];
            allRectangles[0] = arrEdges[0].First;
            for (int i = 0; i < arrEdges.Length; i++)
            {
                allRectangles[i + 1] = arrEdges[i].Second;
            }
            return allRectangles;
        }

        public class RectEdge
        {
            private Vector2 _startPoint;
            private Vector2 _endPoint;
            private EdgeType _edgeType;

            public Vector2 StartPoint => _startPoint;
            public Vector2 EndPoint => _endPoint;
            public EdgeType EdgeType => _edgeType;
            public RectEdge(Vector2 startPoint, Vector2 endPoint, EdgeType lineType)
            {
                _startPoint = startPoint;
                _endPoint = endPoint;
                _edgeType = lineType;
            }

            //it checks as full overlaping and partial also than crossing only one rectanle side
            public bool IsEdgeCrossingCheckedRect(Rectangle checkedRec)
            {
                switch (_edgeType)
                {
                    //"Good Variant' only if Line end before rectanle or start after it
                    case EdgeType.Horizintal:
                        return !(_endPoint.x <= checkedRec.Min.x || _startPoint.x >= checkedRec.Max.x)
                                && _startPoint.y > checkedRec.Min.y && _startPoint.y < checkedRec.Max.y;
                    case EdgeType.Vertical:
                        return !(_endPoint.y <= checkedRec.Min.y || _startPoint.y >= checkedRec.Max.y)
                                && _startPoint.x > checkedRec.Min.x && _startPoint.x < checkedRec.Max.x;
                    default:
                        throw new NotSupportedException($"Wrong [{_edgeType}] line type");
                }
            }

            public Vector3 GetEndPoILine()
            {
                switch (_edgeType)
                {
                    case EdgeType.Horizintal:
                        return new Vector3(_endPoint.x - _startPoint.x, 0, 0);
                    case EdgeType.Vertical:
                        return new Vector3(0, _endPoint.y - _startPoint.y, 0);
                    default:
                        throw new NotSupportedException($"Wrong [{_edgeType}] line type");
                }
            }
        }
        public enum EdgeType
        {
            Horizintal = 0,
            Vertical = 1,
        }
    }
    
}