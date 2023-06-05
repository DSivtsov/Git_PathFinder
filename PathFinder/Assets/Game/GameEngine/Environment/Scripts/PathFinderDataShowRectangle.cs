using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace GameEngine.Environment
{
    public class PathFinderDataShowRectangle : MonoBehaviour
    {
        [SerializeField] private Transform _rectanglePrefab;
        
        private Transform _transformParent;

        private void Awake()
        {
            _transformParent = transform;
        }
        /// <summary>
        /// Build new Rectangle GameObject 
        /// </summary>
        /// <param name="normalizedRectangle">NormalizedRectangle</param>
        /// <param name="nameNewRectangle">name new GameObject</param>
        //public void DrawRectangle(NormalizedRectangle normalizedRectangle, string nameNewRectangle)
        //{
        //    Transform transformRectangle = Instantiate<Transform>(_rectanglePrefab, _transformParent);
        //    transformRectangle.position = (Vector2)normalizedRectangle.BottomLeftAngel;
        //    transformRectangle.localScale = (Vector2)normalizedRectangle.SizeXY;
        //    transformRectangle.name = nameNewRectangle;
        //    transformRectangle.GetComponent<LineRenderer>().widthMultiplier = 5;
        //}

        //public void DeleteDrownRectangle()
        //{
        //    foreach (Transform item in _transformParent)
        //    {
        //        Object.Destroy(item.gameObject);
        //    }
        //    NormalizedRectangle.ClearNumRect();
        //}
    } 
}
