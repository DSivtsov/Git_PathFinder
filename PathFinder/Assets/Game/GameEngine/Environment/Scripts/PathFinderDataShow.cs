using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace GameEngine.Environment
{
    public class PathFinderDataShow : MonoBehaviour
    {
        [Header("ShowPoints")]
        [SerializeField] private Transform _prefabInitialPoint;
        [SerializeField] private Transform _findPathStartPoint;
        [SerializeField] private Transform _findPathEndPoint;
        [SerializeField] private Transform _prefabStartEdge;
        [SerializeField] private Transform _prefabEndEdge;
        [Header("ShowRectangle")]
        [SerializeField] private Transform _rectanglePrefab;

        private Transform _parentTransform;

        private void Awake() => _parentTransform = transform;

        /// <summary>
        /// Build new Rectangle GameObject 
        /// </summary>
        /// <param name="normalizedRectangle">NormalizedRectangle</param>
        /// <param name="nameNewRectangle">name new GameObject</param>
        public void DrawRectangle(NormalizedRectangle normalizedRectangle, string nameNewRectangle)
        {
            Transform transformRectangle = Instantiate<Transform>(_rectanglePrefab, _parentTransform);
            transformRectangle.position = (Vector2)normalizedRectangle.BottomLeftAngel;
            transformRectangle.localScale = (Vector2)normalizedRectangle.SizeXY;
            transformRectangle.name = nameNewRectangle;
            transformRectangle.GetComponent<LineRenderer>().widthMultiplier = 5;
        }

        public void DeleteDrownObjects()
        {
            foreach (Transform item in _parentTransform)
            {
                Destroy(item.gameObject);
            }
            NormalizedRectangle.ClearNumRect();
        }

        //public void DeleteEdgeAndBasePoints()
        //{
        //    foreach (Transform item in _parentTransform)
        //    {
        //        UnityEngine.Object.Destroy(item.gameObject);
        //    }
        //}

        public void DrawStartPoint(Vector2 startPointFindPath)
        {
            SetAndActivatePoint(_findPathStartPoint, startPointFindPath);
        }
        
        public void DrawEndPoint(Vector2 endPointFindPath)
        {
            SetAndActivatePoint(_findPathEndPoint, endPointFindPath);
        }
        
        public void DrawInitialPoint()
        {
            SetAndActivatePoint(_prefabInitialPoint, Vector2Int.zero);
        }

        public void DrawEdgePoints(Edge edge, int numEdge)
        {
            SetAndActivatePoint(_prefabStartEdge, (Vector2)edge.Start, $"StartPointEdge{numEdge}");
            SetAndActivatePoint(_prefabEndEdge, edge.End, $"EndPointEdge{numEdge}");
        }


        private void SetAndActivatePoint(Transform prefabPoint, Vector2 pointPosition, string nameEdge = "")
        {
            Transform transform = Instantiate<Transform>(prefabPoint, _parentTransform);
            transform.position = pointPosition;
            if (nameEdge != "")
                transform.name = nameEdge;
        }
    }
}