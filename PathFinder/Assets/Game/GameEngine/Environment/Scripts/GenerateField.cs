using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using GMTools.Common;

namespace GameEngine.Environment
{
    public class GenerateField : MonoBehaviour
    {
        [SerializeField] private PathFinderData _pathFinderData;
        [SerializeField] private FieldSettingSO _fieldSetting;
        [SerializeField] private DrawRectangle _drawRectangle;
        [Header("DEBUG")]
        [SerializeField] private bool _useSeedFromFieldSettingSO = false;
        [SerializeField] private bool _showUsedRandomSeed = false;
        [SerializeField] private bool _startAutomaticly = true;

        private System.Random _random;
        private int _widthHalfField;
        private int _heightHalfField;
        //To Pass Minimum number of Edges
        private int _maxWidthRectangle;
        private int _maxHeightRectangle;
        private bool _wasOutFromFieldLimit;
        private const int NUMEDGES = 4;
        private const int NUMEANGLE = 4;
        private int NumEdge;

        private NormalizedRectangle _firstRect;
        private NormalizedRectangle _secondRect;

        public FieldSettingSO FieldSetting => _fieldSetting;

        private void Injection()
        {
            CountFrame.DebugLogWarningUpdate("Injection()");
            if (!_drawRectangle)
            {
                _drawRectangle = gameObject.GetComponent<DrawRectangle>();
                if (!_drawRectangle)
                    throw new NotImplementedException("GenerateField:  Not linked to DrawRectangle");
            }
        }

        public void Awake()
        {
            Injection();
            InitCreateField();
        }

        private void Start()
        {
            if (_startAutomaticly)
            {
                CreateField(); 
            }
        }

        private void InitCreateField()
        {
            _widthHalfField = _fieldSetting.WidthField / 2;
            _heightHalfField = _fieldSetting.HeightField / 2;
            CountFrame.DebugLogUpdate($"Field {_widthHalfField} {_heightHalfField}");
            GetMaximumHeightWidthForRectangle();
            NormalizedRectangle.InitNormalizedRectangle(_widthHalfField, _heightHalfField, _drawRectangle);
            _pathFinderData.Init(_fieldSetting.MinNumberEdges);
        }

        [Button]
        public void CreateField()
        {
            CountFrame.DebugLogWarningUpdate("DEBUG called CreateField():");

            DeleteGameObjects();

            InitRandom();

            _wasOutFromFieldLimit = false;

            _firstRect = CreateInitialRec();
            _firstRect.Draw();

            _pathFinderData.StartPointFindPath = CreateStartEndPointFindPath(_firstRect);

            CreateEdges();

            _pathFinderData.EndPointFindPath = CreateStartEndPointFindPath(_secondRect);
        }

        private void DeleteGameObjects()
        {
            _drawRectangle.DeleteDrownRectangle();
            _pathFinderData.DeletePoints();
            _pathFinderData.SetInitialPoint();
            _pathFinderData.ClearPreviousResults();
        }

        private void InitRandom()
        {
            if (_useSeedFromFieldSettingSO)
            {
                Debug.Log($"{this}: Will used the SEED={_fieldSetting.CurrentSeed} from SO [{_fieldSetting.name}] ");
                _random = new System.Random(_fieldSetting.CurrentSeed); 
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

        private Vector2Int CreateStartEndPointFindPath(NormalizedRectangle rect)
        {
            EdgeType edgeTypeWhereWillStartPointFindPath = SelectRandomAnyEdgeType();
            CountFrame.DebugLogUpdate($"StartEndPointFindPath EdgeType[{edgeTypeWhereWillStartPointFindPath}]");
            return GetRandomPointOnEdge(rect, edgeTypeWhereWillStartPointFindPath);
        }

        private void CreateEdges()
        {
            NumEdge = 0;
            EdgeType prevUsedEdgeType = EdgeType.Nothing;

            while (!_wasOutFromFieldLimit && (_fieldSetting.IsLimitedMaxNumberEdge && NumEdge < _fieldSetting.MaxNumberEdges))
            {
                CountFrame.DebugLogUpdate($"NumEdge={NumEdge}");

                EdgeType edgeTypeWhereWillNextRect = (NumEdge == 0) ? SelectRandomAnyEdgeType() : SelectRandomEdgeType(prevUsedEdgeType);
                CountFrame.DebugLogUpdate($"Next Rec will at [{edgeTypeWhereWillNextRect}] Edge");

                Vector2Int startPointOnEdge = GetRandomPointOnEdge(_firstRect, edgeTypeWhereWillNextRect);

                EdgeType usedEdgeType = edgeTypeWhereWillNextRect;
                AngleType selectedAngleTypeBasePoint = SelectAngleTypeBasePoint(usedEdgeType);
                CountFrame.DebugLogUpdate($"basePoint={startPointOnEdge} selectedAngleType[{selectedAngleTypeBasePoint}]");

                _secondRect = GetNewNormalizedRectangle(startPointOnEdge, selectedAngleTypeBasePoint);

                _wasOutFromFieldLimit = _secondRect.CutRectByFieldLimit(selectedAngleTypeBasePoint);

                _secondRect.Draw();

                Vector2Int endPointEdge = FindEndPointEdge(startPointOnEdge, edgeTypeWhereWillNextRect, selectedAngleTypeBasePoint);

                Edge edge = _pathFinderData.AddEdge(_firstRect, _secondRect, startPointOnEdge, endPointEdge);
                _pathFinderData.CreateDebugEdgePoints(edge, NumEdge);

                _firstRect = _secondRect;
                prevUsedEdgeType = edgeTypeWhereWillNextRect;
                NumEdge++;
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

        private NormalizedRectangle CreateInitialRec()
        {
            Vector2Int basePoint = new Vector2Int(_fieldSetting.CenterX, _fieldSetting.CenterY);

            AngleType selectedAngleTypeBasePoint = SelectAnyAngleTypeForBasePoint();

            CountFrame.DebugLogUpdate($"basePoint={basePoint} selectedAngleTypeForBasePoint[{selectedAngleTypeBasePoint}]");
            NormalizedRectangle newNormalizedRectangle = GetNewNormalizedRectangle(basePoint, selectedAngleTypeBasePoint);
            
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
        private NormalizedRectangle GetNewNormalizedRectangle(Vector2Int basePoint, AngleType selectedBasePointAngleType)
        {
            Vector2Int shiftToOtherAngleRectangel = GetShiftToOtherAngleRectangle(selectedBasePointAngleType);
            //CountFrame.DebugLogUpdate($"initialBasePoint: [{selectedBasePointAngleType}]{basePoint} shift:{shiftToOtherAngleRectangel}");
            NormalizedRectangle newNormalizedRectangle = new NormalizedRectangle(basePoint, shiftToOtherAngleRectangel);
            return newNormalizedRectangle;
        }

        private Vector2Int GetShiftToOtherAngleRectangle(AngleType basePointAngleType)
        {
            int widthRectangle = _random.Next(_fieldSetting.MinWidthRectangle, (_maxWidthRectangle + 1));
            int heightRectangle = _random.Next(_fieldSetting.MinHeightRectangle, (_maxHeightRectangle + 1));
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
            _maxWidthRectangle = _widthHalfField / (_fieldSetting.MinNumberEdges + 1);
            CheckCalculatedRectangleSize(ref _maxWidthRectangle, _fieldSetting.MinWidthRectangle);
            _maxHeightRectangle = _heightHalfField / (_fieldSetting.MinNumberEdges + 1);
            CheckCalculatedRectangleSize(ref _maxHeightRectangle, _fieldSetting.MinHeightRectangle);

            CountFrame.DebugLogUpdate($"_maxWidthRectangle[{_maxWidthRectangle}] _maxHeightRectangle[{_maxHeightRectangle}]");
        }

        private void CheckCalculatedRectangleSize(ref int calculatedRectangleSize, int minRectangleSize)
        {
            if (calculatedRectangleSize < minRectangleSize)
            {
                Debug.LogWarning($"CONFLICT: Calculated Rectangle Size ={_maxHeightRectangle} based on demanding number edges [{_fieldSetting.MinNumberEdges}]" +
                $" less than Set Minimum Rectangle Size ={minRectangleSize}. Calculated Rectangle Size will be overrided  by the Minimum Rectangle Size");
                calculatedRectangleSize = minRectangleSize;
            }
        }
    }
}
