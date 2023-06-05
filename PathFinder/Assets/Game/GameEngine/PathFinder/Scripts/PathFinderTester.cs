using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using GameEngine.Environment;


namespace GameEngine.PathFinder
{
    [RequireComponent(typeof(ShowPath))]
    public class PathFinderTester : MonoBehaviour
    {
        [SerializeField] private GeneratePathFinderData _generatePathFinderData;
        
        [Header("DEBUG")]
        [SerializeField] private bool _autoRun = false;
        [Tooltip("Will override setting in GeneratePathFinderData class")]
        [SerializeField] private bool _useSeedFromFieldSettingSO = false;
        [SerializeField] private bool _showGraphPath = true;
        [Header("DEBUGER PathFinder")]
        [SerializeField] private bool _turnOnDebugPathFinder = false;
        [SerializeField] private DebugPathFinder _prefabDebugPathFinder;


        private IEnumerable<Vector2> _stepsPath;
        private ShowPath _showPath;
        private DebugPathFinder _debugPathFinder;

        private void Awake()
        {
            _debugPathFinder = FindObjectOfType<DebugPathFinder>();
            _showPath = FindObjectOfType<ShowPath>();
            if (!_generatePathFinderData)
                throw new NotSupportedException("Object GeneratePathFinderData is Null");
        }

        private void Start()
        {
            if (_autoRun)
                GenerateDataAndTryGetPath();
        }

        [Button,HideInEditorMode]
        public void GenerateDataAndTryGetPath()
        {
            TurnDebugPathFinder();

            _showPath.DeletePreviousStepsPath();

            _generatePathFinderData.GenerateData(_useSeedFromFieldSettingSO);

            Finder finder = new Finder(_generatePathFinderData.PathFinderData);
            _stepsPath = finder.CheckDataAndGetPath();

            if (_showGraphPath)
            {
                ListDotsPath.ShowGraphPath(); 
            }

            if (_stepsPath != null)
                _showPath.ShowStepsPath(_stepsPath.ToList());
        }

        /// <summary>
        /// In case set the key "DEBUGFINDER" in Player Setting give a possibility to use the class DebugFinder to show debug lines which used in process of finding Path
        /// </summary>
        private void TurnDebugPathFinder()
        {
            if (_turnOnDebugPathFinder)
            {
#if DEBUGFINDER
                if (!_debugPathFinder)
                {
                    if (_prefabDebugPathFinder)
                        _debugPathFinder = Instantiate(_prefabDebugPathFinder);
                    else
                        throw new NotSupportedException("_prefabDebugPathFinder Is null Can't TurnOn DebugPathFinder");
                }
                _debugPathFinder.gameObject.SetActive(true);
                GenerationSettingSO fieldSetting = _generatePathFinderData.FieldSetting;
                if (fieldSetting)
                    _debugPathFinder.InitDebugPathFinder(fieldSetting.WidthField, fieldSetting.HeightField);
                else
                    throw new NotSupportedException("FieldSettingSO in Object GeneratePathFinderData is Null Can't TurnOn DebugPathFinder");
                DebugFinder.StartDebugFinder(_debugPathFinder);
#else
                Debug.LogError("Key DEBUGFINDER not active in Player Setting. Can't TurnOn DebugPathFinder");
#endif
            }
            else
            {
                if (_debugPathFinder)
                {
                    _debugPathFinder.gameObject.SetActive(false);
                }
                DebugFinder.StopDebugFinder(); 
            }
        }
    }
}