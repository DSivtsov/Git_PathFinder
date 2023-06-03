using System;
using System.Collections.Generic;
using UnityEngine;


namespace GameEngine.PathFinder
{
    public struct LineConnection
    {
        float angleTurn;
        float lenghtStep;
        Vector2 dot;

        public Vector2 Dot => dot;

        public override string ToString() => $"dot{dot} angleTurn={angleTurn:f1} lenghtStep={lenghtStep:f1}";
    }

    public class Path
    {
        List<LineConnection> connections;
        private Vector2 startPointFindPath;
        private Vector2 endPointFindPath;

        private const int NUMSTARTENDPOINTS = 2;

        public Path(Vector2 startPointFindPath, Vector2 endPointFindPath, int numEdges)
        {
            this.startPointFindPath = startPointFindPath;
            this.endPointFindPath = endPointFindPath;
            this.connections = new List<LineConnection>(numEdges);
        }

        public List<Vector2> GetPath()
        {
            List<Vector2> path = new List<Vector2>(NUMSTARTENDPOINTS + connections.Count);
            Debug.LogWarning($"Path will include Start & End points [{NUMSTARTENDPOINTS}]");
            path.Add(startPointFindPath);
            if (connections.Count != 0)
            {
                for (int i = 0; i < connections.Count; i++)
                    path.Add(connections[i].Dot);
            }
            path.Add(endPointFindPath);
            return path;
        }

        public void SetLineConnections(List<LineConnection> lineConnections) => connections = lineConnections;

        public void ShowLPath()
        {
            Debug.Log($"[startPointFindPath] {startPointFindPath}");
            for (int i = 0; i < connections.Count; i++)
            {
                Debug.Log($"[{i}] {connections[i]}");
            }
            Debug.Log($"[endPointFindPath] {endPointFindPath}");
        }
    }
}