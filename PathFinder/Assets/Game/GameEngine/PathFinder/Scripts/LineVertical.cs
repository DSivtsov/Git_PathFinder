using System;
using UnityEngine;


namespace GameEngine.PathFinder
{
    public sealed class LineVertical : Line
    {// factorX * X +  factorY * Y = factorB
        private const float FactorXVerticalLine = 1f;
        private const float FactorYVerticalLine = 0;
        public LineVertical(float factorB) : base(FactorXVerticalLine, FactorYVerticalLine, factorB) { }

        internal override (Vector2 startDot, Vector2 endDot) GetDotsforScreen(int widthHalfField, int heightHalfField)
        {
            Vector2 startDot = new Vector2(_factorB, -heightHalfField);
            Vector2 endDot = new Vector2(_factorB, heightHalfField);
            return (startDot, endDot);
        }

        internal override bool TryIntersecLineWithEdge(int currentTestingNumEdge)
        {
            float x = _factorB;
            (float constValue, float minValue, float maxValue, LineType lineTypeEdge) = StoreInfoEdges.GetEdgeInfo(currentTestingNumEdge);
            switch (lineTypeEdge)
            {
                case LineType.Vertical:
                    if ((int)(x - constValue) == 0)
                        return true;
                    return false;
                case LineType.Horizontal:
                    if (StoreInfoEdges.InRange(x, minValue, maxValue))
                        return true;
                    return false;
                default:
                    throw new NotSupportedException($"Wrong [{lineTypeEdge}] Edge type");
            }
        }
    }
}