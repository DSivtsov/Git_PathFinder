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
        [SerializeField] Transform _prefabStepPath;
        [ReadOnly, ShowInInspector] private List<Vector2> _pathFounded;

        private Transform _parentTransformShowSteps;

        public void InitShowPath(List<Vector2> pathFounded)
        {
            _pathFounded = pathFounded;
            _parentTransformShowSteps = transform;
        }

        public List<Vector2> PathFounded => _pathFounded;

        [Button]
        public void Show()
        {
            DeleteStepsPath();
            if (_pathFounded != null)
            {
                if (_pathFounded.Count > 0)
                {
                    ShowPointsPath();
                    ShowStepsPath();
                }
                else
                    Debug.LogWarning("Path not Found");
            }
            else
                throw new System.NotImplementedException("GetPath not run");
        }

        private void ShowStepsPath()
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

        private void ShowPointsPath()
        {
            for (int i = 0; i < _pathFounded.Count; i++)
            {
                Debug.Log($"[{i + 1}] {_pathFounded[i]}");
            }
        }

        public void DeleteStepsPath()
        {
            foreach (Transform item in _parentTransformShowSteps)
            {
                UnityEngine.Object.Destroy(item.gameObject);
            }
        }
    } 
}
