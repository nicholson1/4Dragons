using System.Collections.Generic;
using System.Linq;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Map
{
    public class Node
    {
        public readonly Point point;
        public readonly List<Point> incoming = new List<Point>();
        public readonly List<Point> outgoing = new List<Point>();
        //[JsonConverter(typeof(StringEnumConverter))]
        public readonly NodeType nodeType;
        public readonly string blueprintName;
        public Vector2 position;
        public NodeStates State;
        public int specialNodeType = -1;
        public int dragonShape = -1;

        public int nodeSeed = -1;


        public Node(NodeType nodeType, string blueprintName, Point point, int seed)
        {
            this.nodeType = nodeType;
            this.blueprintName = blueprintName;
            this.point = point;
            this.nodeSeed = seed;
        }

        public void AddIncoming(Point p)
        {
            if (incoming.Any(element => element.Equals(p)))
                return;

            incoming.Add(p);
        }

        public void AddOutgoing(Point p)
        {
            if (outgoing.Any(element => element.Equals(p)))
                return;

            outgoing.Add(p);
        }

        public void RemoveIncoming(Point p)
        {
            incoming.RemoveAll(element => element.Equals(p));
        }

        public void RemoveOutgoing(Point p)
        {
            outgoing.RemoveAll(element => element.Equals(p));
        }

        public bool HasNoConnections()
        {
            return incoming.Count == 0 && outgoing.Count == 0;
        }

        public void SetState(NodeStates nodeState)
        {
            this.State = nodeState;
        }
    }
}