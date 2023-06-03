using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace GameEngine.PathFinder
{
    /*
     *  Store the connection between different Solutions (connection between "baseDotSectorSolutions")
     *  
     *  Note.
     *  For simplify the ConnectionDot.baseDot separated from SectorSolutions.baseDotSectorSolutions (but in most cases it is one point, but not in all cases)
     */
    /// <summary>
    /// Store the connection between different Solutions
    /// </summary>
    public class ConnectionDot
    {
        public readonly Vector2 baseDot;
        /// <summary>
        /// can connected to more than one of other ConnectionDot
        /// </summary>
        public readonly IEnumerable<ConnectionDot> prevConnectionDots;

        public ConnectionDot(Vector2 connectionDot, IEnumerable<ConnectionDot> prevConnectionDots)
        {
            this.baseDot = connectionDot;
            this.prevConnectionDots = prevConnectionDots;
        }

        public override string ToString()
        {
            IEnumerable<string> values = prevConnectionDots
                .Select<ConnectionDot, string>((connectionDots) => (connectionDots == null) ? "NULL":connectionDots.baseDot.ToString()) ;
            var listDots = string.Join(" ", values);
            return $"connectionDot{baseDot} prevConnectionDots.Count[{prevConnectionDots.Count()}] listDots[{listDots}]";
        }
    }

    public static class ListDotsPath
    {
        //Intersect will be at twice more than edge
        private const int FactorIntersectToEdge = 2;

        private static List<ConnectionDot> _list;
        //private static int _numDotHaveCrossingwithEndPath;
        private static Vector2 _endPointFindPath;
        private static List<Vector2> _path;
        private static IEnumerable<ConnectionDot> _connectionDotsHaveDirectLinkWithEndPath;

        internal static void InitListDotsPath(int numEdges)
        {
            _list = new List<ConnectionDot>(numEdges * FactorIntersectToEdge);
        }

        internal static void AddConnectionDot(ConnectionDot connectionDot)
        {
            _list.Add(connectionDot);
        }

        private const int INCLUDESTARTANDENDPATH = 2;
        /// <summary>
        /// Return dots in Path if it exist. Order from StartPath
        /// </summary>
        /// <returns>order from start</returns>
        internal static IEnumerable<Vector2> GetPath()
        {
            //Always in path include minimum two dots - startPath and endPath
            _path = new List<Vector2>(_list.Count + INCLUDESTARTANDENDPATH);
            for (int i = 0; i < _list.Count; i++)
            {
                Debug.Log($"[{i}] {_list[i]}");
            }
            SelectAnyPathWithBeginLastDotCrossing();
            return _path.Reverse<Vector2>();
        }

        //Not have special optimization for selecting Dots to Path
        private static void SelectAnyPathWithBeginLastDotCrossing()
        {
            Debug.LogWarning("Will build the path through the lastDotCrossing");
            //in case of absent special demands to optimize the selection of Dots for Path, Let's just start from the last
            ConnectionDot connectionDotEndPath = _list.Last();
            _path.Add(connectionDotEndPath.baseDot);
            IEnumerable<ConnectionDot> colectionPreviousConnectionDots = connectionDotEndPath.prevConnectionDots;
            //The dot of StartPath always incluided in list and have .prevConnectionDots  IEnumerable<ConnectionDot>.Count() == 0
            do
            {
                //use the simply algorithm always take the first connectionDot in list
                connectionDotEndPath = colectionPreviousConnectionDots.ElementAt(0);
                _path.Add(connectionDotEndPath.baseDot);
                colectionPreviousConnectionDots = connectionDotEndPath.prevConnectionDots;
            } while (colectionPreviousConnectionDots.Count() != 0);
        }
    }
}