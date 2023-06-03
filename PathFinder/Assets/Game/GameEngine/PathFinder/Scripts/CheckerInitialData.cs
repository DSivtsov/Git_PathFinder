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
    public class CheckerInitialData : MonoBehaviour
    {
        [SerializeField] Transform _prefabPOI;
        [SerializeField] Transform _prefabPOILine;
        private PathFinderData _pathFinderData;

        private Transform _pathFinderDataTransform;

        public void InitialData(PathFinderData pathFinderData)
        {
            this._pathFinderData = pathFinderData;
            _pathFinderDataTransform = _pathFinderData.transform;
        }

        [Button]
        public bool CheckData()
        {
            if (_pathFinderData == null || _pathFinderData.StartPointFindPath == null || _pathFinderData.EndPointFindPath == null
                || _pathFinderData.ListEdges == null)
            {
                Debug.LogError("Initial Data not intialized. GetPath() stoped.");
                return false;
            }

            if (_pathFinderData.ListEdges.Count == 0)
            {
                Debug.LogError("Absent Edges in Initial Data. GetPath() stoped.");
                return false;
            }

            if (CheckExistOverlapingRectangle())
            {
                Debug.LogError("Exist overlaping rectangles in Initial Data. GetPath() stoped.");
                return false;
            }

            Debug.Log($"CheckData() passed. No detected errors. Found [{_pathFinderData.ListEdges.Count}] Edges");
            return true;
        }

        /// <summary>
        /// Does exist the overlaping of Rectangles
        /// </summary>
        /// <returns>true if it exist</returns>
        private bool CheckExistOverlapingRectangle()
        {
            Rectangle[] allRectangles = GetListAllRectangles(_pathFinderData.ListEdges);
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

        private bool ExistOverlapingRectangle(Rectangle checkedRec, Rectangle otherRec)
        {
            Line[] lines = GetLinesFromRectangle(otherRec);
            for (int line = 0; line < lines.Length; line++)
            {
                Line checkedLine = lines[line];
                if (checkedLine.IsLineCrossingRect(checkedRec))
                {
                    Debug.Log($"DEMO!!! EndgeType[{((PathFinder.LineType)line)}] IsLineCrossingRect[true]");
                    Debug.Log($"checkedRec[{ShowRect(checkedRec)}] lineOtherRec {checkedLine} ");
                    ShowPoILine(checkedLine);
                    return true;
                }
            }
            return false;
        }

        private void ShowPoILine(Line checkedLine)
        {
            Transform transformLine = Instantiate(_prefabPOILine, _pathFinderDataTransform);
            transformLine.position = new Vector3(checkedLine.StartPoint.x, checkedLine.StartPoint.y, transformLine.position.z);
            LineRenderer lineRenderer = transformLine.GetComponent<LineRenderer>();
            if (lineRenderer)
            {
                lineRenderer.SetPosition(1, checkedLine.GetEndPoILine());
            }
            else
                throw new NotImplementedException("Absent LindeRnder in PoILine"); 
        }

        private void ShowPoI(Vector2 checkedAngle)
        {
            Transform transform = Instantiate(_prefabPOI, _pathFinderDataTransform);
            transform.position = checkedAngle;
        }

        private string ShowRect(Rectangle rec) => $"Min{rec.Min} Max{rec.Max}";

        private Line[] GetLinesFromRectangle(Rectangle otherRec)
        {
            //Order Points in Line given related to standard Unity Axis line corespondce to these directions
            return new Line[]
            {
                new Line(new Vector2(otherRec.Min.x,otherRec.Max.y), new Vector2(otherRec.Max.x,otherRec.Max.y), LineType.Horizintal), //Top = 0,
                new Line(new Vector2(otherRec.Max.x,otherRec.Min.y), new Vector2(otherRec.Max.x,otherRec.Max.y), LineType.Vertical), //Right = 1,
                new Line(new Vector2(otherRec.Min.x,otherRec.Min.y), new Vector2(otherRec.Max.x,otherRec.Min.y), LineType.Horizintal), //Bottom = 2,
                new Line(new Vector2(otherRec.Min.x,otherRec.Min.y), new Vector2(otherRec.Min.x,otherRec.Max.y), LineType.Vertical), //Left = 3,
            };
        }

        private Vector2[] GetAnglesFromRectangle(Rectangle otherRec)
        {
            return new Vector2[]
            {
                new Vector2(otherRec.Min.x,otherRec.Max.y), //TopLeft = 0,
                new Vector2(otherRec.Max.x,otherRec.Max.y), //TopRight = 1,
                new Vector2(otherRec.Max.x,otherRec.Min.y), //BottomRight = 2,
                new Vector2(otherRec.Min.x,otherRec.Min.y), //BottomLeft = 3,
            };
        }

        private Rectangle[] GetListAllRectangles(List<Edge> listEdges)
        {
            Rectangle[] allRectangles = new Rectangle[listEdges.Count + 1];
            allRectangles[0] = listEdges[0].First;
            for (int i = 0; i < listEdges.Count; i++)
            {
                allRectangles[i + 1] = listEdges[i].Second;
            }
            return allRectangles;
        }

        private class Line
        {
            private Vector2 _startPoint;
            private Vector2 _endPoint;
            private LineType _lineType;

            public Vector2 StartPoint => _startPoint;
            public Vector2 EndPoint => _endPoint;

            public Line(Vector2 startPoint, Vector2 endPoint, LineType lineType)
            {
                _startPoint = startPoint;
                _endPoint = endPoint;
                _lineType = lineType;
            }

            //it checks as full overlaping and partial also than crossing only one rectanle side
            public bool IsLineCrossingRect(Rectangle checkedRec)
            {
                switch (_lineType)
                {
                    //"Good Variant' only if Line end before rectanle or start after it
                    case LineType.Horizintal:
                        return !(_endPoint.x <= checkedRec.Min.x || _startPoint.x >= checkedRec.Max.x)
                                && _startPoint.y > checkedRec.Min.y && _startPoint.y < checkedRec.Max.y;
                    case LineType.Vertical:
                        return !(_endPoint.y <= checkedRec.Min.y || _startPoint.y >= checkedRec.Max.y)
                                && _startPoint.x > checkedRec.Min.x && _startPoint.x < checkedRec.Max.x;
                    default:
                        throw new NotSupportedException($"Wrong [{_lineType}] line type");
                }
            }

            public override string ToString()
            {
                return $"start({_startPoint}) end({_endPoint})";
            }

            public Vector3 GetEndPoILine()
            {
                switch (_lineType)
                {
                    case LineType.Horizintal:
                        return new Vector3(_endPoint.x - _startPoint.x, 0, 0);
                    case LineType.Vertical:
                        return new Vector3(0, _endPoint.y - _startPoint.y, 0);
                    default:
                        throw new NotSupportedException($"Wrong [{_lineType}] line type");
                }
            }
        }
        public enum LineType
        {
            Horizintal = 0,
            Vertical = 1,
        }
    }
    
}