using System;
using UnityEngine;


namespace GameEngine.PathFinder
{
    public sealed class LineHorizontal : Line
    {// factorX * X +  factorY * Y = factorB
        private const float FactorXVerticalLine = 0;
        private const float FactorYVerticalLine = 1f;
        public LineHorizontal(float factorB) : base(FactorXVerticalLine, FactorYVerticalLine, factorB) { }

        internal override (Vector2 startDot, Vector2 endDot) GetDotsforScreen(int widthHalfField, int heightHalfField)
        {
            Vector2 startDot = new Vector2(-widthHalfField, _factorB);
            Vector2 endDot = new Vector2(widthHalfField, _factorB);
            return (startDot, endDot);
        }

        internal override bool TryIntersecLineWithEdge(int currentTestingNumEdge)
        {
            float y = _factorB;
            (float constValue, float minValue, float maxValue, LineType lineTypeEdge) = StoreInfoEdges.GetEdgeInfo(currentTestingNumEdge);
            switch (lineTypeEdge)
            {
                case LineType.Horizontal:
                    if ((int)(y - constValue) == 0)
                        return true;
                    return false;
                case LineType.Vertical:
                    if (StoreInfoEdges.InRange(y, minValue, maxValue))
                        return true;
                    return false;
                default:
                    throw new NotSupportedException($"Wrong [{lineTypeEdge}] Edge type");
            }
        }
    }
}