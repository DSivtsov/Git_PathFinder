using UnityEngine;
using Sirenix.OdinInspector;

namespace GameEngine.Environment
{
    [CreateAssetMenu(
        fileName = "GenerationSettingSO",
        menuName = "PathFinder/New GenerationSetting"
    )]
    public class GenerationSettingSO : ScriptableObject
    {
        public int CurrentSeed = 123456;
        public int CenterX = 0;
        public int CenterY = 0;
        public int WidthField = 1920;
        public int HeightField = 1080;
        [Tooltip("Will decrease the Max Number Edges to put Rectangles in field only")]
        public bool NotOutFromFieldSize = true;
        [Tooltip("Will decrease the Min Rectangle Size to put demanded Number in Field")]
        public int MinNumberEdges = 2;
        [Tooltip("Will try to put maximum number Edges in Field")]
        public bool TryPutMaxNumberEdge = true;
        [ShowIf("TryPutMaxNumberEdge")]
        public int MaxNumberEdges = 2;
        public int MinWidthRectangle = 150;
        public int MinHeightRectangle = 150;
        public int MinPercentEdge = 10;
        public int MaxPercentEdge = 60;

        private void OnValidate()
        {
            if (TryPutMaxNumberEdge && MaxNumberEdges < MinNumberEdges)
            {
                MaxNumberEdges = MinNumberEdges + 1;
            }
        }
    }
}
