using UnityEngine;
using Sirenix.OdinInspector;

namespace GameEngine.Environment
{
    [CreateAssetMenu(
        fileName = "FieldSettingSO",
        menuName = "PathFinder/New FieldSetting"
    )]
    public class FieldSettingSO : ScriptableObject
    {
        public int CurrentSeed = 123456;
        public int CenterX = 0;
        public int CenterY = 0;
        public int WidthField = 1920;
        public int HeightField = 1080;
        public int MinNumberEdges = 2;
        //if IsLimitedMaxNumberEdge = true the MaxNumberEdges = MinNumberEdges
        public bool IsLimitedMaxNumberEdge = true;
        [ShowIf("IsLimitedMaxNumberEdge")]
        public int MaxNumberEdges = 2;
        public int MinWidthRectangle = 150;
        public int MinHeightRectangle = 150;
        public int MinPercentEdge = 10;
        public int MaxPercentEdge = 60;

        private void OnValidate()
        {
            if (IsLimitedMaxNumberEdge && MaxNumberEdges < MinNumberEdges)
            {
                MaxNumberEdges = MinNumberEdges + 1;
            }
        }
    }
}
