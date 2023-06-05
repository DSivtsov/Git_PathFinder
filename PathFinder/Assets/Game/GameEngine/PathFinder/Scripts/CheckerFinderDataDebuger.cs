using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEngine.Environment;

namespace GameEngine.PathFinder
{
    public class CheckerFinderDataDebuger : MonoBehaviour
    {
        [SerializeField] Transform _prefabPOI;
        [SerializeField] Transform _prefabPOILine;

        private Transform _pathFinderDataTransform;

        private void Awake()
        {
            _pathFinderDataTransform = transform;
        }

        public void InitCheckerInitialDataDebuger(Transform pathFinderDataTransform)
        {
            if (pathFinderDataTransform)
            {
                _pathFinderDataTransform = pathFinderDataTransform;
            }
            else
                throw new NotImplementedException("Absent pathFinderDataTransform");
        }

        public void ShowPoILine(CheckerFinderData.RectEdge checkedLine)
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

        public void ShowPoI(Vector2 checkedAngle)
        {
            Transform transform = Instantiate(_prefabPOI, _pathFinderDataTransform);
            transform.position = checkedAngle;
        }

        public string ShowRect(Rectangle rec) => $"Min{rec.Min} Max{rec.Max}";
    } 
}
