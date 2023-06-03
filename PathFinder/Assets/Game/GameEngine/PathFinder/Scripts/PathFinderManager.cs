using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using GameEngine.Environment;


namespace GameEngine.PathFinder
{
    public class PathFinderManager : MonoBehaviour
    {
        [SerializeField] private PathFinderData _pathFinderData;
        [SerializeField] private CheckerInitialData _checkerInitialData;
        [SerializeField] private ShowPath _showPath;
        [SerializeField] private GenerateField _generateField;
        [SerializeField] private DebugPathFinderManager _debugPathFinderManager;
        [Header("DEBUG")]
        [SerializeField] private bool _createFieldAutoStart = false;
        [SerializeField] private bool _callGetPathAutoStart = false;
        [SerializeField] private bool _turnOnDebugPathFinderManager = false;


        private List<Vector2> _pathFounded;
        private IPathFinder _iFinder;

        private void Awake()
        {
            _checkerInitialData.InitialData(_pathFinderData);
            FieldSettingSO fieldSetting = _generateField.FieldSetting;
            if (fieldSetting)
                _debugPathFinderManager.InitDebugPathFinderManager(fieldSetting.WidthField, fieldSetting.HeightField); 
            else
                throw new NotSupportedException("FieldSettingSO is Null");
        }

        private void Start()
        {
            if (_createFieldAutoStart)
                _generateField.CreateField();
            if (_callGetPathAutoStart)
                CallGetPath();
        }

        [Button]
        public void CallGetPath()
        {
            if (_turnOnDebugPathFinderManager)
                DebugFinder.StartDebugFinder(_debugPathFinderManager);
            else
                DebugFinder.StartDebugFinder(_debugPathFinderManager,activateDebugPathFinder: false);

            if (_checkerInitialData.CheckData())
            {
                _iFinder = new Finder();
                _pathFounded = _iFinder.GetPath(_pathFinderData.StartPointFindPath, _pathFinderData.EndPointFindPath, _pathFinderData.ListEdges).ToList();
                _showPath.InitShowPath(_pathFounded);
                _showPath.Show(); 
            }
        }

        public override string ToString()
        {
            return $"StartPointFindPath{_pathFinderData.StartPointFindPath} EndPointFindPath{_pathFinderData.EndPointFindPath}" +
                $" _listEdges.Count[{_pathFinderData.ListEdges.Count}]";
        }
    }


}