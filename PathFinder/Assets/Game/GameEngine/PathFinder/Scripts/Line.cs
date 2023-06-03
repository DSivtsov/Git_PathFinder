using System.Collections.Generic;
using System;
using UnityEngine;
using GameEngine.Environment;


namespace GameEngine.PathFinder
{
    public enum LineType
    {
        Horizontal = 0,
        Vertical = 1,
    }

    public class Line
    {
        private const float FactorYNormolizedLine = 1f;
        // factorX * X +  1 * Y = factorB
        protected readonly float _factorX;   //k factor || Tang(alfa)
        protected readonly float _factorY;
        protected readonly float _factorB;

        protected Line(float factorX, float factorY, float factorB)
        {
            _factorX = factorX;
            _factorB = factorB;
            _factorY = factorY;
        }

        internal static Line CreateLine(Vector2 dotA, Vector2 dotB)
        {
            float deltaX = dotB.x - dotA.x;
            float deltaY = dotB.y - dotA.y;
            float factorX, factorB;
            if ((int)deltaY == 0)
            {
                //factorX = 0;
                //factorY = 1f;
                factorB = dotA.y;
                return new LineHorizontal(factorB);
            }
            else
            {
                if ((int)deltaX == 0)
                {
                    //factorX = 1f;
                    //factorY = 0;
                    factorB = dotA.x;
                    return new LineVertical(factorB);
                }
                else
                {
                    factorX = -deltaY / deltaX;
                    //factorY = 1f;
                    factorB = dotA.y + factorX * dotA.x;
                    return new Line(factorX, FactorYNormolizedLine, factorB);
                }
            }
        }

        internal virtual (Vector2 startDot, Vector2 endDot) GetDotsforScreen(int widthHalfField, int heightHalfField)
        {
            Vector2 startDot = new Vector2(-widthHalfField, (float)FindYForX(-widthHalfField));
            Vector2 endDot = new Vector2(widthHalfField, (float)FindYForX(widthHalfField));
            return (startDot, endDot);
        }

        /// <summary>
        /// Get from Line the  factors for linear system equation, factorY always = 1
        /// </summary>
        /// <returns>factors for {i} equation</returns>
        internal virtual (float ai1, float ai2, float bi) GetDataForMatrix2x2() => (_factorX, _factorY, _factorB);

        private float FindXForY(float y) => (_factorB - y) / _factorX;

        private float FindYForX(float x) => (_factorB - _factorX * x);

        //Currently all edges is Vertical or is Horizontal in this case the detection crossing Line with them can be simplify by use special structure
        /// <summary>
        /// Try Intersec Line With Edge
        /// </summary>
        /// <param name="currentTestingNumEdge"></param>
        /// <returns>true if Line interesect the edge</returns>
        internal virtual bool TryIntersecLineWithEdge(int currentTestingNumEdge)
        {
            float x, y;
            (float constValue, float minValue, float maxValue, LineType type) = StoreInfoEdges.GetEdgeInfo(currentTestingNumEdge);
            switch (type)
            {
                case LineType.Horizontal:
                    y = constValue;
                    if (StoreInfoEdges.InRange(FindXForY(y), minValue, maxValue))
                        return true;
                    return false;
                case LineType.Vertical:
                    x = constValue;
                    if (StoreInfoEdges.InRange(FindYForX(x), minValue, maxValue))
                        return true;
                    return false;
                default:
                    throw new NotSupportedException($"Wrong [{type}] Edge type");
            }
        }

        public override string ToString()
        {
            return $"_factorX[{_factorX}] _factorY[{_factorY}] _factorB[{_factorB}]";
        }

        /// <summary>
        /// Test possibility to create line between dotA and dotB which can pass the edges from startNumEdge till endNumEdge
        /// </summary>
        /// <param name="dotA"></param>
        /// <param name="dotB"></param>
        /// <param name="startNumEdge"></param>
        /// <param name="endNumEdge"></param>
        /// <returns> if linked (true, Line) otherwise (false, Line)</returns>
        internal static (bool isPassedEdge, Line line) TryLinkTwoDotsThroughEdges(Vector2 dotA, Vector2 dotB, int startNumEdge, int endNumEdge)
        {
            CorrectOrderEdgeNumbers(ref startNumEdge, ref endNumEdge);
            Debug.Log($"TryLinkTwoDots({dotA}, {dotB}), check Edges from [{startNumEdge}] till [{endNumEdge}]");
            Line lineBTWDots = Line.CreateLine(dotA, dotB);
            bool directLineBTWdotsExist = true;
            int currentNumTestingEdge = startNumEdge;
            for (; currentNumTestingEdge <= endNumEdge; currentNumTestingEdge++)
            {
                if (!lineBTWDots.TryIntersecLineWithEdge(currentNumTestingEdge))
                {
                    DebugFinder.DebugTurnOn(false);
                    DebugFinder.DebugDrawLineSegment(dotA, dotB, $"Not crossing Edge[{currentNumTestingEdge}]");
                    DebugFinder.DebugTurnOn(true);
                    Debug.Log($"Intersec Line Not crossing Edge[{currentNumTestingEdge}]");
                    directLineBTWdotsExist = false;
                    break;
                }
            }
            return (directLineBTWdotsExist, lineBTWDots);
        }

        private static void CorrectOrderEdgeNumbers(ref int startNumEdge, ref int endNumEdge)
        {
            if (startNumEdge == endNumEdge)
                return;
            if (startNumEdge > endNumEdge)
            {
                int temp = endNumEdge;
                endNumEdge = startNumEdge;
                startNumEdge = temp;
            }
        }
    }
}