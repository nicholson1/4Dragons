using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Map
{
    public static class MapGenerator
    {
        private static MapConfig config;
        
        private static readonly List<NodeType> RandomNodes = new List<NodeType>
        {NodeType.Mystery, NodeType.Store, NodeType.MinorEnemy, NodeType.EliteEnemy, NodeType.Treasure};

        private static List<float> layerDistances;
        private static List<List<Point>> paths;
        // ALL nodes by layer:
        private static readonly List<List<Node>> nodes = new List<List<Node>>();

        private static int ElitesThisMap = 0;
        private static int ChestsThisMap = 0;
        private static int shopsThisMap = 0;

        private static int maxElites = 3;
        private static int maxShop = 4;



        public static Map GetMap(MapConfig conf)
        {
            if (conf == null)
            {
                Debug.LogWarning("Config was null in MapGenerator.Generate()");
                return null;
            }
            
            config = conf;
            nodes.Clear();

            if (CombatController._instance.Difficulty >= 6)
            {
                maxElites = 5;
            }

            ElitesThisMap = 0;
            ChestsThisMap = 0;
            shopsThisMap = 0;

            GenerateLayerDistances();

            for (var i = 0; i < conf.layers.Count; i++)
                PlaceLayer(i);

            GeneratePaths();

            RandomizeNodePositions();

            SetUpConnections();

            RemoveCrossConnections();

            // select all the nodes with connections:
            var nodesList = nodes.SelectMany(n => n).Where(n => n.incoming.Count > 0 || n.outgoing.Count > 0).ToList();

            // pick a random name of the boss level for this map:
            var bossNodeName = config.nodeBlueprints.Where(b => b.nodeType == NodeType.Boss).ToList().Random().name;
            return new Map(conf.name, bossNodeName, nodesList, new List<Point>());
        }

        private static void GenerateLayerDistances()
        {
            layerDistances = new List<float>();
            foreach (var layer in config.layers)
                layerDistances.Add(layer.distanceFromPreviousLayer.GetValue());
        }

        private static float GetDistanceToLayer(int layerIndex)
        {
            if (layerIndex < 0 || layerIndex > layerDistances.Count) return 0f;

            return layerDistances.Take(layerIndex + 1).Sum();
        }

        private static void PlaceLayer(int layerIndex)
        {
            var layer = config.layers[layerIndex];
            var nodesOnThisLayer = new List<Node>();

            // offset of this layer to make all the nodes centered:
            var offset = layer.nodesApartDistance * config.GridWidth / 2f;

            for (var i = 0; i < config.GridWidth; i++)
            {
                var nodeType = Rand._i.Random.NextDouble() < layer.randomizeNodes ? GetRandomNode(nodesOnThisLayer) : layer.nodeType;

                if (layerIndex >= 4 && shopsThisMap == 0)
                {
                    nodeType = NodeType.Store;
                }
                if (layerIndex >= 7 && shopsThisMap <= 1)
                {
                    nodeType = NodeType.Store;
                }
                
                //try after force an elite after level 4, and after level 6 
                if (layerIndex >= 4 && ElitesThisMap == 0 && nodesOnThisLayer.Count > 0 )
                {
                    nodeType = NodeType.EliteEnemy;
                }
                
                if (layerIndex >= 7 && ElitesThisMap <= 1 && nodesOnThisLayer.Count > 0 )
                {
                    nodeType = NodeType.EliteEnemy;
                }
                
                if (layerIndex == 6 )
                {
                    nodeType = NodeType.Treasure;
                }
                
                if(nodeType == NodeType.EliteEnemy)
                    ElitesThisMap += 1;
                if (nodeType == NodeType.Treasure)
                    ChestsThisMap += 1;
                if (nodeType == NodeType.Store)
                    shopsThisMap += 1;
                

                var blueprintName = config.nodeBlueprints.Where(b => b.nodeType == nodeType).ToList().Random().name;
                int seed = Rand._i.Random.Next();
                var node = new Node(nodeType, blueprintName, new Point(i, layerIndex), seed)
                {
                    position = new Vector2(-offset + i * layer.nodesApartDistance, GetDistanceToLayer(layerIndex))
                };
                
                nodesOnThisLayer.Add(node);
                
            }

            nodes.Add(nodesOnThisLayer);
        }

        private static void RandomizeNodePositions()
        {
            for (var index = 0; index < nodes.Count; index++)
            {
                var list = nodes[index];
                var layer = config.layers[index];
                var distToNextLayer = index + 1 >= layerDistances.Count
                    ? 0f
                    : layerDistances[index + 1];
                var distToPreviousLayer = layerDistances[index];

                foreach (var node in list)
                {
                    var xRnd = Random.Range(-1f, 1f);
                    var yRnd = Random.Range(-1f, 1f);

                    var x = xRnd * layer.nodesApartDistance / 2f;
                    var y = yRnd < 0 ? distToPreviousLayer * yRnd / 2f : distToNextLayer * yRnd / 2f;

                    node.position += new Vector2(x, y) * layer.randomizePosition;
                }
            }
        }

        private static void SetUpConnections()
        {
            foreach (var path in paths)
            {
                for (var i = 0; i < path.Count - 1; ++i)
                {
                    var node = GetNode(path[i]);
                    var nextNode = GetNode(path[i + 1]);
                    node.AddOutgoing(nextNode.point);
                    nextNode.AddIncoming(node.point);
                }
            }
        }

        private static void RemoveCrossConnections()
        {
            for (var i = 0; i < config.GridWidth - 1; ++i)
                for (var j = 0; j < config.layers.Count - 1; ++j)
                {
                    var node = GetNode(new Point(i, j));
                    if (node == null || node.HasNoConnections()) continue;
                    var right = GetNode(new Point(i + 1, j));
                    if (right == null || right.HasNoConnections()) continue;
                    var top = GetNode(new Point(i, j + 1));
                    if (top == null || top.HasNoConnections()) continue;
                    var topRight = GetNode(new Point(i + 1, j + 1));
                    if (topRight == null || topRight.HasNoConnections()) continue;

                    // Debug.Log("Inspecting node for connections: " + node.point);
                    if (!node.outgoing.Any(element => element.Equals(topRight.point))) continue;
                    if (!right.outgoing.Any(element => element.Equals(top.point))) continue;

                    // Debug.Log("Found a cross node: " + node.point);

                    // we managed to find a cross node:
                    // 1) add direct connections:
                    node.AddOutgoing(top.point);
                    top.AddIncoming(node.point);

                    right.AddOutgoing(topRight.point);
                    topRight.AddIncoming(right.point);

                    var rnd = Random.Range(0f, 1f);
                    if (rnd < 0.2f)
                    {
                        // remove both cross connections:
                        // a) 
                        node.RemoveOutgoing(topRight.point);
                        topRight.RemoveIncoming(node.point);
                        // b) 
                        right.RemoveOutgoing(top.point);
                        top.RemoveIncoming(right.point);
                    }
                    else if (rnd < 0.6f)
                    {
                        // a) 
                        node.RemoveOutgoing(topRight.point);
                        topRight.RemoveIncoming(node.point);
                    }
                    else
                    {
                        // b) 
                        right.RemoveOutgoing(top.point);
                        top.RemoveIncoming(right.point);
                    }
                }
        }

        private static Node GetNode(Point p)
        {
            if (p.y >= nodes.Count) return null;
            if (p.x >= nodes[p.y].Count) return null;

            return nodes[p.y][p.x];
        }

        private static Point GetFinalNode()
        {
            var y = config.layers.Count - 1;
            if (config.GridWidth % 2 == 1)
                return new Point(config.GridWidth / 2, y);

            return Random.Range(0, 2) == 0
                ? new Point(config.GridWidth / 2, y)
                : new Point(config.GridWidth / 2 - 1, y);
        }

        private static void GeneratePaths()
        {
            var finalNode = GetFinalNode();
            paths = new List<List<Point>>();
            var numOfStartingNodes = config.numOfStartingNodes.GetValue();
            var numOfPreBossNodes = config.numOfPreBossNodes.GetValue();

            var candidateXs = new List<int>();
            for (var i = 0; i < config.GridWidth; i++)
                candidateXs.Add(i);

            candidateXs.Shuffle();
            var startingXs = candidateXs.Take(numOfStartingNodes);
            var startingPoints = (from x in startingXs select new Point(x, 0)).ToList();

            candidateXs.Shuffle();
            var preBossXs = candidateXs.Take(numOfPreBossNodes);
            var preBossPoints = (from x in preBossXs select new Point(x, finalNode.y - 1)).ToList();

            int numOfPaths = Mathf.Max(numOfStartingNodes, numOfPreBossNodes) + Mathf.Max(0, config.extraPaths);
            for (int i = 0; i < numOfPaths; ++i)
            {
                Point startNode = startingPoints[i % numOfStartingNodes];
                Point endNode = preBossPoints[i % numOfPreBossNodes];
                var path = Path(startNode, endNode);
                path.Add(finalNode);
                paths.Add(path);
            }
        }

        // Generates a random path bottom up.
        private static List<Point> Path(Point fromPoint, Point toPoint)
        {
            int toRow = toPoint.y;
            int toCol = toPoint.x;

            int lastNodeCol = fromPoint.x;

            var path = new List<Point> { fromPoint };
            var candidateCols = new List<int>();
            for (int row = 1; row < toRow; ++row)
            {
                candidateCols.Clear();

                int verticalDistance = toRow - row;
                int horizontalDistance;

                int forwardCol = lastNodeCol;
                horizontalDistance = Mathf.Abs(toCol - forwardCol);
                if (horizontalDistance <= verticalDistance)
                    candidateCols.Add(lastNodeCol);

                int leftCol = lastNodeCol - 1;
                horizontalDistance = Mathf.Abs(toCol - leftCol);
                if (leftCol >= 0 && horizontalDistance <= verticalDistance)
                    candidateCols.Add(leftCol);

                int rightCol = lastNodeCol + 1;
                horizontalDistance = Mathf.Abs(toCol - rightCol);
                if (rightCol < config.GridWidth && horizontalDistance <= verticalDistance)
                    candidateCols.Add(rightCol);

                int RandomCandidateIndex = Random.Range(0, candidateCols.Count);
                int candidateCol = candidateCols[RandomCandidateIndex];
                var nextPoint = new Point(candidateCol, row);

                path.Add(nextPoint);

                lastNodeCol = candidateCol;
            }

            path.Add(toPoint);

            return path;
        }

        private static NodeType GetRandomNode(List<Node> nodesOnThisLayer)
        {
            //{NodeType.Mystery, NodeType.Store, NodeType.MinorEnemy, NodeType.EliteEnemy, NodeType.Treasure};

            int[] nodeTypeWeights = new[] { 2, 1, 5, 2, 0};
            int totalWeight = 0;
            foreach (var item in nodeTypeWeights)
            {
                totalWeight += item;
            }

            if (CombatController._instance.Difficulty >= 6)
                nodeTypeWeights[3] = 3;

            NodeType nt = NodeType.MinorEnemy;
            int roll = Rand._i.Random.Next(0, totalWeight + 1);

            int sum = 0;
            for (int i = 0; i < nodeTypeWeights.Length; i++)
            {
                sum += nodeTypeWeights[i];
                if (sum >= roll)
                {
                    nt = RandomNodes[i];
                    //Debug.Log(roll + " "+ nt);
                    break;
                }
            }
            
            


            // if there is only 1 node width no elites allowed, if elite placed dont place another
            if (nt == NodeType.EliteEnemy && (nodesOnThisLayer.Count == 0 || ThisLayerHasElite(nodesOnThisLayer) || ElitesThisMap >= maxElites))
            {
                nt =RandomNodes[Rand._i.Random.Next(0, RandomNodes.Count-1)];
            }
            
            if (nt == NodeType.Store)
            {
                if (shopsThisMap >= maxShop)
                    nt = NodeType.Mystery;
            }

            if (nt == NodeType.Treasure)
            {
                //if (ChestsThisMap > 0)
                nt = NodeType.Mystery;
            }

            return nt;
        }

        private static bool ThisLayerHasElite(List<Node> nodesOnThisLayer)
        {
            foreach (var node in nodesOnThisLayer)
            {
                if (node.nodeType == NodeType.EliteEnemy)
                {
                    return true;
                }
            }

            return false;
        }
    }
}