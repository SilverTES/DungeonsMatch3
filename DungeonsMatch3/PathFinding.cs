using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Dynamic;
using Mugen.Core;
using Mugen.AI;

namespace DungeonsMatch3
{
    static class PathFinding
    {
        // Structure pour un nœud dans A*
        private class Node
        {
            public Point Position;
            public float G; // Coût depuis le départ
            public float H; // Heuristique vers l'objectif
            public float F => G + H; // Coût total
            public Node Parent;

            public Node(Point position)
            {
                Position = position;
                G = 0;
                H = 0;
                Parent = null;
            }
        }

        // Pathfinding A* pour unité multi-cases
        public static List<Point> FindPath<T>(Grid2D<T> grid, Point start, Point goal, Point unitSize, int passLevel = 1) where T : Mugen.Core.Node
        {
            List<Node> openList = new List<Node>();
            HashSet<Point> closedList = new HashSet<Point>();
            Node startNode = new Node(start);
            Node goalNode = new Node(goal);

            openList.Add(startNode);

            while (openList.Count > 0)
            {
                // Trouver le nœud avec le plus petit F
                Node currentNode = openList[0];
                for (int i = 1; i < openList.Count; i++)
                    if (openList[i].F < currentNode.F)
                        currentNode = openList[i];

                openList.Remove(currentNode);
                closedList.Add(currentNode.Position);

                // Objectif atteint ?
                if (currentNode.Position == goalNode.Position)
                    return ReconstructPath(currentNode);

                // Explorer les voisins (haut, bas, gauche, droite)
                Point[] directions = 
                { 
                    new Point(0, -1), 
                    new Point(0, 1), 
                    new Point(-1, 0), 
                    new Point(1, 0),
                    //new Point(-1, -1),
                    //new Point(1, -1),
                    //new Point(1, 1),
                    //new Point(-1, 1),
                };

                foreach (var dir in directions)
                {
                    Point newPos = currentNode.Position + dir;

                    // Vérifier si la nouvelle position est valide pour l'unité entière
                    if (!IsValidPosition(grid, newPos, unitSize, passLevel))
                        continue;

                    if (closedList.Contains(newPos))
                        continue;

                    float newG = currentNode.G + 1; // Coût de déplacement = 1 par case
                    float newH = Vector2.Distance(newPos.ToVector2(), goalNode.Position.ToVector2()); // Heuristique
                    float newF = newG + newH;

                    Node neighbor = openList.Find(n => n.Position == newPos);
                    if (neighbor == null)
                    {
                        neighbor = new Node(newPos);
                        neighbor.G = newG;
                        neighbor.H = newH;
                        neighbor.Parent = currentNode;
                        openList.Add(neighbor);
                    }
                    else if (newG < neighbor.G)
                    {
                        neighbor.G = newG;
                        neighbor.Parent = currentNode;
                    }
                }
            }

            return null; // Aucun chemin trouvé
        }

        // Vérifier si une position est valide pour l'unité entière
        private static bool IsValidPosition<T>(Grid2D<T> grid, Point position, Point unitSize, int passLevel) where T : Mugen.Core.Node
        {
            int x = position.X;
            int y = position.Y;
            int width = unitSize.X;
            int height = unitSize.Y;

            // Vérifier les limites de la grille
            if (x < 0 || x + width - 1 >= grid.Width || y < 0 || y + height - 1 >= grid.Height)
                return false;

            // Vérifier chaque case occupée par l'unité
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var cell = grid.Get(x + i, y + j);
                    if (cell != null)
                    {
                        if (passLevel < cell._passLevel) // Obstacle
                            return false;
                    }
                }
            }

            return true;
        }

        // Reconstruire le chemin depuis le nœud final
        private static List<Point> ReconstructPath(Node node)
        {
            List<Point> path = new List<Point>();
            while (node != null)
            {
                path.Add(node.Position);
                node = node.Parent;
            }
            path.Reverse();
            return path;
        }

    }
}
