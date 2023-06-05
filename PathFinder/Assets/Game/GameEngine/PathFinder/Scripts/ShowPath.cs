using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using GameEngine.Environment;

namespace GameEngine.PathFinder
{
    public class ShowPath : MonoBehaviour
    {
        [SerializeField] private Transform _prefabStepPath;
        [Header("DEBUG")]
        [SerializeField] private bool _drawStepsPath = true;
        [SerializeField] private bool _debugLogPointsPath = true;
        [Space]
        [ReadOnly, ShowInInspector] private List<Vector2> _pathFounded;

        private Transform _parentTransformShowSteps;

        private void Awake() => _parentTransformShowSteps = transform;

        public void ShowStepsPath(List<Vector2> pathFounded)
        {
            _pathFounded = pathFounded;

            if (_pathFounded != null)
            {
                if (_pathFounded.Count > 0)
                {
                    if (_debugLogPointsPath)
                        DebugLogPointsPath();
                    if (_drawStepsPath)
                        DrawStepsPath(); 
                }
                else
                    Debug.LogWarning("Path not Found");
            }
            else
                throw new System.NotImplementedException("GetPath not run");
        }

        private void DrawStepsPath()
        {
            Vector2 startDot = _pathFounded[0];
            for (int i = 1; i < _pathFounded.Count; i++)
            {
                Vector2 endDot = _pathFounded[i];
                Transform transformLine = UnityEngine.Object.Instantiate<Transform>(_prefabStepPath, _parentTransformShowSteps);
                transformLine.position = new Vector3(startDot.x, startDot.y, transformLine.position.z);
                LineRenderer lineRenderer = transformLine.GetComponent<LineRenderer>();
                lineRenderer.SetPosition(1, endDot-startDot);
                transformLine.name = $"Line{i}";
                startDot = endDot;
            }
        }

        private void DebugLogPointsPath()
        {
            Debug.LogWarning("DebugLogPointsPath");
            for (int i = 0; i < _pathFounded.Count; i++)
            {
                Debug.Log($"[{i + 1}] {_pathFounded[i]}");
            }
        }

        public void DeletePreviousStepsPath()
        {
            foreach (Transform item in _parentTransformShowSteps)
            {
                UnityEngine.Object.Destroy(item.gameObject);
            }
        }
    } 
}
