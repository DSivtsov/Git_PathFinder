using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace GameEngine.Environment
{
    public class GeneratePathFinderData : MonoBehaviour
    {
        [SerializeField] private GenerationSettingSO _generationSetting;
        [Header("DEBUG")]
        [SerializeField] private PathFinderDataShow _pathFinderDataShow;
        [SerializeField] private bool _useSeedFromFieldSettingSO = false;
        [SerializeField] private bool _showUsedRandomSeed = false;
        [Space]
        [ShowInInspector] private PathFinderData _pathFinderData;

        private System.Random _random;
        private int _widthHalfField;
        private int _heightHalfField;
        //To Pass Minimum number of Edges
        private int _maxWidthRectangle;
        private int _maxHeightRectangle;
        private const int NUMEDGES = 4;
        private const int NUMEANGLE = 4;
        private int numEdge;

        private NormalizedRectangle _firstRect;
        private NormalizedRectangle _secondRect;

        public GenerationSettingSO FieldSetting => _generationSetting;

        public PathFinderData PathFinderData => _pathFinderData;

        public void Awake()
        {
            _pathFinderDataShow = FindObjectOfType<PathFinderDataShow>();
            Init();
        }

        private void Init()
        {
            _widthHalfField = _generationSetting.WidthField / 2;
            _heightHalfField = _generationSetting.HeightField / 2;
            GenerateFinderDataDebug.DebugLogUpdate($"Field {_widthHalfField} {_heightHalfField}");
            GetMaximumHeightWidthForRectangle();
            NormalizedRectangle.InitNormalizedRectangle(_widthHalfField, _heightHalfField, _pathFinderDataShow);

            _pathFinderData = new PathFinderData(_generationSetting.MinNumberEdges);
        }
 
        public void GenerateData(bool overrideUseSeedFromField = false)
        {
            GenerateFinderDataDebug.DebugLogWarningUpdate("DEBUG called GenerateNewData()");

            DeleteGenratedGameObjects();

            _pathFinderData.SetInitialPoint();

            InitRandom(overrideUseSeedFromField);

            _firstRect = CreateRandomInitialRec();

            _pathFinderData.StartPointFindPath = CreateRandomStartEndPointFindPath(_firstRect);

            CreateRandomEdgesAndRectangles();

            _pathFinderData.EndPointFindPath = CreateRandomStartEndPointFindPath(_secondRect);
        }

        private void DeleteGenratedGameObjects()
        {
            _pathFinderDataShow.DeleteDrownObjects();
            _pathFinderData.ClearPreviousResults();
        }

        private void InitRandom(bool overrideUseSeedFromField)
        {
            if (overrideUseSeedFromField || _useSeedFromFieldSettingSO)
            {
                Debug.Log($"{this}: Will used the SEED={_generationSetting.CurrentSeed} from SO [{_generationSetting.name}] ");
                _random = new System.Random(_generationSetting.CurrentSeed); 
            }
            else
            {
                _random = new System.Random();
                if (_showUsedRandomSeed)
                {
                    int newSeed = _random.Next();
                    Debug.LogWarning($"{this}: Will used new SEED={newSeed}");
                    _random = new System.Random(newSeed);
                }
                else
                    _random = new System.Random();
            }
        }

        private Vector2Int CreateRandomStartEndPointFindPath(NormalizedRectangle rect)
        {
            EdgeType edgeTypeWhereWillStartPointFindPath = SelectRandomAnyEdgeType();
            GenerateFinderDataDebug.DebugLogUpdate($"StartEndPointFindPath EdgeType[{edgeTypeWhereWillStartPointFindPath}]");
            return GetRandomPointOnEdge(rect, edgeTypeWhereWillStartPointFindPath);
        }

        private void CreateRandomEdgesAndRectangles()
        {
            numEdge = 0;
            EdgeType prevUsedEdgeType = EdgeType.Nothing;
            bool rectanglesWasOutField = false;

            while ((numEdge < _generationSetting.MinNumberEdges) ||
                ((_generationSetting.TryPutMaxNumberEdge && _generationSetting.MaxNumberEdges > numEdge) && !(_generationSetting.NotOutFromFieldSize && rectanglesWasOutField)))
            {
                GenerateFinderDataDebug.DebugLogUpdate($"NumEdge={numEdge}");

                EdgeType edgeTypeWhereWillNextRect = (numEdge == 0) ? SelectRandomAnyEdgeType() : SelectRandomEdgeType(prevUsedEdgeType);
                GenerateFinderDataDebug.DebugLogUpdate($"Next Rec will at [{edgeTypeWhereWillNextRect}] Edge");

                Vector2Int startPointOnEdge = GetRandomPointOnEdge(_firstRect, edgeTypeWhereWillNextRect);

                EdgeType usedEdgeType = edgeTypeWhereWillNextRect;
                AngleType selectedAngleTypeBasePoint = SelectAngleTypeBasePoint(usedEdgeType);
                GenerateFinderDataDebug.DebugLogUpdate($"basePoint={startPointOnEdge} selectedAngleType[{selectedAngleTypeBasePoint}]");

                _secondRect = GetRandomNewNormalizedRectangle(startPointOnEdge, selectedAngleTypeBasePoint);

                rectanglesWasOutField = _generationSetting.NotOutFromFieldSize ? _secondRect.CutRectByFieldLimit(selectedAngleTypeBasePoint) : false;

                _secondRect.Draw();

                Vector2Int endPointEdge = FindEndPointEdge(startPointOnEdge, edgeTypeWhereWillNextRect, selectedAngleTypeBasePoint);

                _pathFinderData.AddEdge(_firstRect, _secondRect, startPointOnEdge, endPointEdge);

                _firstRect = _secondRect;
                prevUsedEdgeType = edgeTypeWhereWillNextRect;
                numEdge++;
            }
        }

        /// <summary>
        /// Select the position of the EndPointEdge. It can be determined by the shorter length of FirstRect or SecondRect
        /// </summary>
        /// <param name="startPointOnEdge">of the FirstRect</param>
        /// <param name="edgeTypeOnFirstRect">edgeType of the FirstRect</param>
        /// <param name="selectedAngleTypeBasePoint">selectedAngleTypeBasePointNewRect (SecondRect) </param>
        /// <returns></returns>
        private Vector2Int FindEndPointEdge(Vector2Int startPointOnEdge, EdgeType edgeTypeOnFirstRect, AngleType selectedAngleTypeBasePointForSecRect)
        {
            Vector2Int endPointEdgeFirstRect = _firstRect.GetEndPointEdge(edgeTypeOnFirstRect, selectedAngleTypeBasePointForSecRect);

            (EdgeType edgeTypeOnSecondRect, AngleType selectedAngleTypeBasePointForFirstRect) =
                GetDataToGetEndPointEdgeOnSecRect(edgeTypeOnFirstRect, selectedAngleTypeBasePointForSecRect);

            Vector2Int endPointEdgeSecondRect = _secondRect.GetEndPointEdge(edgeTypeOnSecondRect, selectedAngleTypeBasePointForFirstRect);

            return Vector2.SqrMagnitude(startPointOnEdge - endPointEdgeFirstRect) < Vector2.SqrMagnitude(startPointOnEdge - endPointEdgeSecondRect) ?
                endPointEdgeFirstRect : endPointEdgeSecondRect;
        }

        private (EdgeType edgeTypeOnSecondRect, AngleType selectedAngleTypeBasePointForFirstRect) GetDataToGetEndPointEdgeOnSecRect(EdgeType edgeTypeOnFirstRect,
            AngleType selectedAngleTypeBasePointForSecRect)
        {
            try
            {
                switch (edgeTypeOnFirstRect)
                {
                    case EdgeType.Top:
                        return (EdgeType.Bottom, verticalMirroringAngleType[selectedAngleTypeBasePointForSecRect]);
                    case EdgeType.Bottom:
                        return (EdgeType.Top, verticalMirroringAngleType[selectedAngleTypeBasePointForSecRect]);
                    case EdgeType.Right:
                        return (EdgeType.Left, horizontalMirroringAngleType[selectedAngleTypeBasePointForSecRect]);
                    case EdgeType.Left:
                        return (EdgeType.Right, horizontalMirroringAngleType[selectedAngleTypeBasePointForSecRect]);
                    case EdgeType.Nothing:
                    default:
                        throw new NotSupportedException($"Wrong edgeTypeOnFirstRect [{edgeTypeOnFirstRect}]");
                }
            }
            catch (KeyNotFoundException)
            {

                throw new NotSupportedException($"Wrong selectedAngleTypeBasePointForSecRect [{selectedAngleTypeBasePointForSecRect}]");
            }
        }

        Dictionary<AngleType, AngleType> verticalMirroringAngleType = new Dictionary<AngleType, AngleType>()
        {
            {AngleType.TopLeft, AngleType.BottomLeft},
            {AngleType.TopRight, AngleType.BottomRight},
            {AngleType.BottomLeft, AngleType.TopLeft},
            {AngleType.BottomRight, AngleType.TopRight},
        };

        Dictionary<AngleType, AngleType> horizontalMirroringAngleType = new Dictionary<AngleType, AngleType>()
        {
            {AngleType.TopLeft, AngleType.TopRight},
            {AngleType.TopRight, AngleType.TopLeft},
            {AngleType.BottomLeft, AngleType.BottomRight},
            {AngleType.BottomRight, AngleType.BottomLeft},
        };

        /// <summary>
        /// The Edge where was placed a basedPoint of new Rect limiti the possible AngleType for that new Rect
        /// </summary>
        Dictionary<EdgeType, List<AngleType>> possibleAngleTypeOnUsedEdge = new Dictionary<EdgeType, List<AngleType>>()
        {
            {EdgeType.Top, new List<AngleType>{AngleType.BottomRight, AngleType.BottomLeft} },
            {EdgeType.Bottom, new List<AngleType>{AngleType.TopRight, AngleType.TopLeft} },
            {EdgeType.Right, new List<AngleType>{AngleType.TopLeft, AngleType.BottomLeft} },
            {EdgeType.Left, new List<AngleType>{AngleType.BottomRight, AngleType.TopRight} },
        };

        private AngleType SelectAngleTypeBasePoint(EdgeType usedEdgeType)
        {
            int randomFromTwo = _random.Next(0, 2);
            if (!possibleAngleTypeOnUsedEdge.TryGetValue(usedEdgeType, out List<AngleType> possibleAngleTypesBasePoint))
                throw new NotSupportedException($"Wrong value [{usedEdgeType}]");
            return possibleAngleTypesBasePoint.ElementAt(randomFromTwo);
        }

        private NormalizedRectangle CreateRandomInitialRec()
        {
            Vector2Int basePoint = new Vector2Int(_generationSetting.CenterX, _generationSetting.CenterY);

            AngleType selectedAngleTypeBasePoint = SelectAnyAngleTypeForBasePoint();

            GenerateFinderDataDebug.DebugLogUpdate($"basePoint={basePoint} selectedAngleTypeForBasePoint[{selectedAngleTypeBasePoint}]");
            NormalizedRectangle newNormalizedRectangle = GetRandomNewNormalizedRectangle(basePoint, selectedAngleTypeBasePoint);
            newNormalizedRectangle.Draw();

            return newNormalizedRectangle;
        }

        private Vector2Int GetRandomPointOnEdge(NormalizedRectangle currentRec, EdgeType edgeTypeWhereWillRandomPoint)
        {
            Vector2Int RandomPointOnEdge = SelectPointOnEdge(edgeTypeWhereWillRandomPoint, currentRec);
            return RandomPointOnEdge;
        }

        private Vector2Int SelectPointOnEdge(EdgeType edgeTypeWhereWasStartPoint, NormalizedRectangle rec)
        {
            int randomPointOnHorizontal = rec.BottomLeftAngel.x + _random.Next(0, rec.SizeXY.x);
            int randomPointOnVertical = rec.BottomLeftAngel.y + _random.Next(0, rec.SizeXY.y);
            switch (edgeTypeWhereWasStartPoint)
            {
                case EdgeType.Top:
                    return new Vector2Int(randomPointOnHorizontal, rec.BottomLeftAngel.y + rec.SizeXY.y);

                case EdgeType.Right:
                        return new Vector2Int(rec.BottomLeftAngel.x + rec.SizeXY.x, randomPointOnVertical); 

                case EdgeType.Bottom:
                    return new Vector2Int(randomPointOnHorizontal, rec.BottomLeftAngel.y);

                case EdgeType.Left:
                    return new Vector2Int(rec.BottomLeftAngel.x, randomPointOnVertical);

                case EdgeType.Nothing:
                default:
                    throw new NotSupportedException($"Wrong value [{edgeTypeWhereWasStartPoint}]");
            }
        }

        private AngleType SelectAnyAngleTypeForBasePoint()
        {
            return (AngleType)_random.Next(0, NUMEANGLE);
        }

        private EdgeType SelectRandomAnyEdgeType()
        {
            return (EdgeType)_random.Next(0, NUMEDGES);
        }

        /// <summary>
        /// Randomly selection of edge for new Rect based on used Edge of previous Rect
        /// </summary>
        /// <param name="prevUsedEdge">Edge a previous Rect</param>
        /// <returns></returns>
        private EdgeType SelectRandomEdgeType(EdgeType prevUsedEdge)
        {
            // Get list of indexes of EdgeType exclude the index _prevEdgeType and EdgeType.Nothing
            IEnumerable<int> modlistIdx = Enumerable.Range(0, NUMEDGES).Where((edge) => edge != (int)GetOpositeEdgeType(prevUsedEdge));
            // Get from this list a random position, and get corespondent value at this position and convert to EdgeType
            int rndPositionInList = _random.Next(0, modlistIdx.Count());
            int valueRNDPosition = modlistIdx.ElementAt(rndPositionInList);
            return (EdgeType)valueRNDPosition;
        }

        /// <summary>
        /// The Edge where was placed a basedPoint of new Rect limiti the possible AngleType for that new Rect
        /// </summary>
        Dictionary<EdgeType, EdgeType> possibleOpositeEdge = new Dictionary<EdgeType, EdgeType>()
        {
            {EdgeType.Top, EdgeType.Bottom},
            {EdgeType.Bottom, EdgeType.Top},
            {EdgeType.Right, EdgeType.Left},
            {EdgeType.Left, EdgeType.Right},
        };

        private EdgeType GetOpositeEdgeType(EdgeType edge)
        {
            if (!possibleOpositeEdge.TryGetValue(edge, out EdgeType opositeEdgeType))
                throw new NotSupportedException($"Wrong value [{edge}]");
            return opositeEdgeType;
        }

        /// <summary>
        /// Get Normalized Rectangle
        /// </summary>
        /// <param name="basePoint"></param>
        /// <param name="selectedBasePointAngleType"></param>
        /// <returns></returns>
        private NormalizedRectangle GetRandomNewNormalizedRectangle(Vector2Int basePoint, AngleType selectedBasePointAngleType)
        {
            Vector2Int shiftToOtherAngleRectangel = GetRandomShiftToOtherAngleRectangle(selectedBasePointAngleType);
            NormalizedRectangle newNormalizedRectangle = new NormalizedRectangle(basePoint, shiftToOtherAngleRectangel);
            return newNormalizedRectangle;
        }

        private Vector2Int GetRandomShiftToOtherAngleRectangle(AngleType basePointAngleType)
        {
            int widthRectangle = _random.Next(_generationSetting.MinWidthRectangle, (_maxWidthRectangle + 1));
            int heightRectangle = _random.Next(_generationSetting.MinHeightRectangle, (_maxHeightRectangle + 1));
            return new Vector2Int(widthRectangle, heightRectangle) * GetDirectionShiftToOtherAngleRectangle(basePointAngleType);
        }

        private Vector2Int GetDirectionShiftToOtherAngleRectangle(AngleType basePointAngleType)
        {
            switch (basePointAngleType)
            {
                case AngleType.TopLeft:
                    return new Vector2Int(1, -1);
                case AngleType.TopRight:
                    return new Vector2Int(-1, -1);
                case AngleType.BottomRight:
                    return new Vector2Int(-1, 1);
                case AngleType.BottomLeft:
                    return new Vector2Int(1, 1);
                default:
                    throw new System.NotSupportedException($"Wrong value [{basePointAngleType}]");
            }
        }    

        /// <summary>
        /// For Instantiated New Rectangles to have a demanded number of Edges
        /// </summary>
        private void GetMaximumHeightWidthForRectangle()
        {
            _maxWidthRectangle = _widthHalfField / (_generationSetting.MinNumberEdges + 1);
            CheckCalculatedRectangleSize(ref _maxWidthRectangle, _generationSetting.MinWidthRectangle);
            _maxHeightRectangle = _heightHalfField / (_generationSetting.MinNumberEdges + 1);
            CheckCalculatedRectangleSize(ref _maxHeightRectangle, _generationSetting.MinHeightRectangle);

            GenerateFinderDataDebug.DebugLogUpdate($"_maxWidthRectangle[{_maxWidthRectangle}] _maxHeightRectangle[{_maxHeightRectangle}]");
        }

        private void CheckCalculatedRectangleSize(ref int calculatedRectangleSize, int minRectangleSize)
        {
            if (calculatedRectangleSize < minRectangleSize)
            {
                Debug.LogWarning($"CONFLICT: Calculated Rectangle Size ={_maxHeightRectangle} based on demanding number edges [{_generationSetting.MinNumberEdges}]" +
                $" less than Set Minimum Rectangle Size ={minRectangleSize}. Calculated Rectangle Size will be overrided  by the Minimum Rectangle Size");
                calculatedRectangleSize = minRectangleSize;
            }
        }
    }
}
