using UnityEngine;
using System;
using GMTools.Common;

namespace GameEngine.Environment
{
    [Serializable]
    public class NormalizedRectangle
    {
        private Vector2Int _bottomLeftAngel;
        private Vector2Int _sizeXY;

        private static DrawRectangle _drawRectangle;
        private static bool _islinkedToDrawRectangle;

        public Vector2Int BottomLeftAngel => _bottomLeftAngel;
        public Vector2Int SizeXY => _sizeXY;

        private static int _widthField;
        private static int _heightField;
        private static bool _isFieldLimiInited;

        private static ushort _countRect = 0;

        public static void InitNormalizedRectangle(int widthField, int heightField, DrawRectangle drawRectangle)
        {
            LinkToDrawRectangle(drawRectangle);

            _widthField = widthField;
            _heightField = heightField;
            _isFieldLimiInited = true;
        }

        private static void LinkToDrawRectangle(DrawRectangle drawRectangle)
        {
            if (drawRectangle)
            {
                _drawRectangle = drawRectangle;
                _islinkedToDrawRectangle = true;
            }
            else
                throw new NotImplementedException("NormalizedRectangl.ctor():  Not have link to DrawRectangle");
        }

        public NormalizedRectangle()
        {
            _bottomLeftAngel = Vector2Int.zero;
            _sizeXY = Vector2Int.one;
            _islinkedToDrawRectangle = false;
        }

        public static void ClearNumRect() => _countRect = 0;

        public NormalizedRectangle(Vector2Int basePoint, Vector2Int shiftToOtherAngleRectangel)
        {
            Vector2Int otherPoint = basePoint + shiftToOtherAngleRectangel;

            int bottomLeftX = Math.Min(basePoint.x, otherPoint.x);
            int bottomLeftY = Math.Min(basePoint.y, otherPoint.y);

            int topRightX = Math.Max(basePoint.x, otherPoint.x);
            int topRightY = Math.Max(basePoint.y, otherPoint.y);

            int width = topRightX - bottomLeftX;
            int height = topRightY - bottomLeftY;

            _bottomLeftAngel = new Vector2Int(bottomLeftX, bottomLeftY);
            _sizeXY = new Vector2Int(width, height);
            _countRect++;
            CountFrame.DebugLogUpdate(this.ToString()); 
        }

        public override string ToString()
        {
            return $"bottomLeftAngel[{_bottomLeftAngel}] sizeXY[{_sizeXY}]";
        }

        public void Draw()
        {
            if (_islinkedToDrawRectangle)
            {
                _drawRectangle.Draw(this,$"Rect{_countRect-1}"); 
            }
            else
                throw new NotImplementedException("NormalizedRectangle:  Not linked to DrawRectangle");

        }

        /// <summary>
        /// Cut boundaries Rect if it was out from field Limit
        /// </summary>
        /// <param name="usedBasePointAngleType">BasePointAngleType which was used for creation secondRect</param>
        /// <returns>true if was out from FieldLimit</returns>
        public bool CutRectByFieldLimit(AngleType usedBasePointAngleType)
        {
            if (_isFieldLimiInited)
            {
                switch (usedBasePointAngleType)
                {
                    case AngleType.TopLeft:
                        Vector2Int bottomRightAngle = new Vector2Int(_bottomLeftAngel.x + _sizeXY.x, _bottomLeftAngel.y);
                        return CheckXMax(bottomRightAngle) | CheckYMin(bottomRightAngle);

                    case AngleType.TopRight:
                        return CheckXMin(_bottomLeftAngel) | CheckYMin(_bottomLeftAngel);

                    case AngleType.BottomRight:
                        Vector2Int topLeftAngle = new Vector2Int(_bottomLeftAngel.x, _bottomLeftAngel.y + _sizeXY.y);
                          return CheckXMin(topLeftAngle) | CheckYMax(topLeftAngle);

                    case AngleType.BottomLeft:
                        Vector2Int topRightAngle = _bottomLeftAngel +  _sizeXY;
                         return CheckXMax(topRightAngle) | CheckYMax(topRightAngle);

                    default:
                        throw new NotSupportedException($"Not supported AngleType[{usedBasePointAngleType}]");
                }
            }
            throw new NotImplementedException("FieldLimit not inited");
        }

        private bool CheckYMin(Vector2Int checkAngle)
        {
            int delta = -_heightField - checkAngle.y;
            //if (checkAngle.y < -_heightField)
            if (delta > 0)
            {
                //float delta = -_heightField - checkAngle.y;
                _bottomLeftAngel.y = -_heightField;
                _sizeXY.y -= delta;
                CountFrame.DebugLogUpdate("CheckYMin() = true");
                return true;
            }
            else
                return false;
        }

        private bool CheckYMax(Vector2Int checkAngle)
        {
            if (checkAngle.y > _heightField)
            {
                _sizeXY.y = _heightField - _bottomLeftAngel.y;
                CountFrame.DebugLogUpdate("CheckYMax() = true");
                return true;
            }
            else
                return false;
        }

        private bool CheckXMax(Vector2Int checkAngle)
        {
            if (checkAngle.x > _widthField)
            {
                _sizeXY.x = _widthField - _bottomLeftAngel.x;
                CountFrame.DebugLogUpdate("CheckXMax() = true");
                return true;
            }
            else
                return false;
        }

        private bool CheckXMin(Vector2Int checkAngle)
        {
            int delta = -_widthField - checkAngle.x;
            //if (checkAngle.x < -_widthField)
            if (delta > 0)
            {
                //float delta = -_widthField - checkAngle.x;
                _bottomLeftAngel.x = -_widthField;
                _sizeXY.x -= delta;
                CountFrame.DebugLogUpdate("CheckXMin() = true");
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Select point ednEdge point from edge connected it and new rect created with BasePointAngleType
        /// </summary>
        /// <param name="firstRect"></param>
        /// <param name="edgeWasUsed">edge of First Rect connected to second Rect</param>
        /// <param name="angleTypeBasePoint">BasePointAngleType was used to create second Rect</param>
        /// <returns></returns>
        public Vector2Int GetEndPointEdge(EdgeType edgeWasUsed, AngleType angleTypeBasePoint)
        {
            Vector2Int topRightAngle = _bottomLeftAngel + _sizeXY;
            int x, y;
            switch (edgeWasUsed)
            {
                case EdgeType.Top:
                    y = topRightAngle.y;
                    x = (angleTypeBasePoint == AngleType.BottomRight)? _bottomLeftAngel.x : topRightAngle.x;
                    break;
                case EdgeType.Right:
                    x = topRightAngle.x;
                    y = (angleTypeBasePoint == AngleType.TopLeft) ? _bottomLeftAngel.y : topRightAngle.y;
                    break;
                case EdgeType.Bottom:
                    y = _bottomLeftAngel.y;
                    x = (angleTypeBasePoint == AngleType.TopRight) ? _bottomLeftAngel.x : topRightAngle.x;
                    break;
                case EdgeType.Left:
                    x = _bottomLeftAngel.x;
                    y = (angleTypeBasePoint == AngleType.TopRight) ? _bottomLeftAngel.y : topRightAngle.y;
                    break;
                case EdgeType.Nothing:
                default:
                    throw new NotSupportedException($"Wrong EdgeType [{edgeWasUsed}]");
            }
            return new Vector2Int(x,y);
        }
    }
}
