using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MazeSolver : MonoBehaviour
{
    [HideInInspector] public Vector3 startCoordinates = new Vector3(0, 0, 0);
    [HideInInspector] public Vector3 endCoordinates = new Vector3(0, 0, 0);

    int[] maze;
    int height, width;

    Node[,] graph;

    List<Node> openList;
    List<Node> closedList;

    public void StartSolution(List<char> mazeList, int h, int w)
    {
        height = h;
        width = w;

        startCoordinates = Vector3.zero;
        endCoordinates = Vector3.zero;

        maze = new int[height * width];
        graph = new Node[width, height];

        ConvertMaze(mazeList);
        FindStartEndCoordinates();
    }

    void ConvertMaze(List<char> mazeList)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (mazeList[x + width * y] == '-' || mazeList[x + width * y] == '+' || mazeList[x + width * y] == '|')
                {
                    maze[x + width * y] = 1;
                }

                if (mazeList[x + width * y] == ' ')
                {
                    maze[x + width * y] = 0;
                }
            }

        }

    }

    void FindStartEndCoordinates()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if ((x > 0 && x < width - 1) && y == 0)
                {
                    if (maze[x + width * y] == 0)
                    {
                        CheckStartOrEnd(x, y);
                    }
                }

                if ((x > 0 && x < width - 1) && y == height - 1)
                {
                    if (maze[x + width * y] == 0)
                    {
                        CheckStartOrEnd(x, y);
                    }
                }

                if ((y > 0 && y < height - 1) && x == 0)
                {
                    if (maze[x + width * y] == 0)
                    {
                        CheckStartOrEnd(x, y);
                    }
                }

                if ((y > 0 && y < height - 1) && x == width - 1)
                {
                    if (maze[x + width * y] == 0)
                    {
                        CheckStartOrEnd(x, y);
                    }
                }
            }
        }
    }

    public void CheckStartOrEnd(int x, int y)
    {
        if (startCoordinates == Vector3.zero)
        {
            startCoordinates.x = x;
            startCoordinates.y = -y;
        }
        else
        {
            if (endCoordinates == Vector3.zero)
            {
                endCoordinates.x = x;
                endCoordinates.y = -y;
            }
        }
    }

    public List<Vector3> FindPath(Vector3 startPosition, Vector3 endPosition)
    {
        int startX = (int)startPosition.x;
        int startY = (int)startPosition.y;

        int endX = (int)endPosition.x;
        int endY = (int)endPosition.y;

        //find the path to the end
        List<Node> path = FindPath(startX, startY, endX, endY);

        if(path == null)
        {
            return null;
        }
        else
        {
            List<Vector3> vectorPath = new List<Vector3>();

            foreach(Node node in path)
            {
                vectorPath.Add(new Vector3(node.X, node.Y, 0f));
            }

            return vectorPath;
        }
    }

    public List<Node> FindPath(int startX, int startY, int endX, int endY)
    {
        openList = new List<Node>();
        closedList = new List<Node>();

        Node startNode = new Node(startX, startY);
        graph[startX, -startY] = startNode;

        Node endNode = new Node(endX, endY);
        graph[endX, -endY] = endNode;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Node node = new Node(x, -y);
                node.gCost = int.MaxValue;
                node.CalculateFCost();
                node.previous = null;

                if (maze[x + width * y] == 1)
                {
                    node.isWalkable = false;
                }
                else if (maze[x + width * y] == 0)
                {
                    node.isWalkable = true;
                }

                //if we are in debug mode
                if (GameManager.instance.debugMode)
                {
                    GameManager.instance.maze[x, y].GetComponent<Tile>().SetGText(node.isWalkable.ToString());

                }
                
                graph[x, y] = node;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = Heuristic(startNode, endNode);
        startNode.CalculateFCost();

        if (GameManager.instance.debugMode)
        {
            GameManager.instance.maze[startNode.X, Mathf.Abs(startNode.Y)].GetComponent<Tile>().SetGText(startNode.gCost.ToString());
            GameManager.instance.maze[startNode.X, Mathf.Abs(startNode.Y)].GetComponent<Tile>().SetHText(startNode.hCost.ToString());
            GameManager.instance.maze[startNode.X, Mathf.Abs(startNode.Y)].GetComponent<Tile>().SetFText(startNode.fCost.ToString());

        }

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = GetCheapestNode();


            if (currentNode.X == endX && currentNode.Y == endY)
            {
                return CalculatePath(currentNode);
            }

            closedList.Add(currentNode);
            openList.Remove(currentNode);

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (closedList.Contains(neighbor))
                {
                    continue;
                }

                if (neighbor.isWalkable == false)
                {
                    closedList.Add(neighbor);
                    continue;
                }

                int cost = currentNode.gCost + Heuristic(currentNode, neighbor);

                if(cost < neighbor.gCost)
                {
                    neighbor.previous = currentNode;
                    neighbor.gCost = cost;
                    neighbor.hCost = Heuristic(neighbor, endNode);
                    neighbor.CalculateFCost();

                    if(!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);

                        if (GameManager.instance.debugMode)
                        {
                            GameManager.instance.maze[neighbor.X, Mathf.Abs(neighbor.Y)].GetComponent<Tile>().SetGText(neighbor.gCost.ToString());
                            GameManager.instance.maze[neighbor.X, Mathf.Abs(neighbor.Y)].GetComponent<Tile>().SetHText(neighbor.hCost.ToString());
                            GameManager.instance.maze[neighbor.X, Mathf.Abs(neighbor.Y)].GetComponent<Tile>().SetFText(neighbor.fCost.ToString());

                        }
                    }
                }
            }
        }

        Debug.LogError("THERE ARE NO MORE NODES TO CHECK. IS THE END ACCESSIBLE?");
        return null;
    }

    Node GetCheapestNode()
    {
        Node cheapestNode = openList[0];

        for(int i = 1; i < openList.Count; i++)
        {
            if(openList[i].fCost < cheapestNode.fCost)
            {
                cheapestNode = openList[i];
            }
        }

        return cheapestNode;
    }

    int Heuristic(Node a, Node b)
    {
        
        int xDifference = Mathf.Abs(a.X - b.X);
        int yDifference = Mathf.Abs(a.Y - b.Y);

        return xDifference + yDifference;
    }

    List<Node> GetNeighbors(Node currentNode)
    {
        List<Node> neighbors = new List<Node>();

        if (currentNode.X - 1 >= 0)
        {
            neighbors.Add(GetNode(currentNode.X - 1, currentNode.Y));
        }

        if (currentNode.X + 1 < width)
        {
            neighbors.Add(GetNode(currentNode.X + 1, currentNode.Y));
        }

        if (currentNode.Y - 1 > -height)
        {
            neighbors.Add(GetNode(currentNode.X, currentNode.Y - 1));
        }

        if (currentNode.Y + 1 <= 0)
        {
            neighbors.Add(GetNode(currentNode.X, currentNode.Y + 1));
        }

        return neighbors;
    }

    Node GetNode(int x, int y)
    {
        return graph[x, Mathf.Abs(y)];
    }

    List<Node> CalculatePath(Node endNode)
    {
        //this calculates our path backwards from the end node back to the begining

        List<Node> path = new List<Node>();

        path.Add(endNode);

        Node currentNode = endNode;

        while (currentNode.previous != null)
        {
            path.Add(currentNode.previous);
            currentNode = currentNode.previous;
        }

        path.Reverse();

        return path;
    }
}
