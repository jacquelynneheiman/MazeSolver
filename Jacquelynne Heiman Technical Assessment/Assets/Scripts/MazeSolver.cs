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
        //save our height and width for easy access
        height = h;
        width = w;

        //set the start and end coordinates to 0
        startCoordinates = Vector3.zero;
        endCoordinates = Vector3.zero;

        //create a new maze and node graph based on our current height and width
        maze = new int[height * width];
        graph = new Node[width, height];

        //convert the maze to 1's and 0's and find our start & end coordinates
        ConvertMaze(mazeList);
        FindStartEndCoordinates();
    }

    void ConvertMaze(List<char> mazeList)
    {

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                //if the position we are on is a wall symbol
                if (mazeList[x + width * y] == '-' || mazeList[x + width * y] == '+' || mazeList[x + width * y] == '|')
                {
                    //save it as a one in our maze
                    maze[x + width * y] = 1;
                }

                //if the position we are on is an empty symbol
                if (mazeList[x + width * y] == ' ')
                {
                    //save it as a zero in our maze
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
                //if we are in the first row, but not in the first or last columns
                if ((x > 0 && x < width - 1) && y == 0)
                {
                    //if the spot is empty
                    if (maze[x + width * y] == 0)
                    {
                        //check to see if we should save the start or end coordinate
                        CheckStartOrEnd(x, y);
                    }
                }

                //if we are in last row, but not in the first or last column
                if ((x > 0 && x < width - 1) && y == height - 1)
                {
                    //if this is an empty spot
                    if (maze[x + width * y] == 0)
                    {
                        //check to see if we should save the start or end coordinate
                        CheckStartOrEnd(x, y);
                    }
                }

                //if we are in the first column but not the first or last row
                if ((y > 0 && y < height - 1) && x == 0)
                {
                    //if this spot is empty
                    if (maze[x + width * y] == 0)
                    {
                        //check to see if we should save the start or end coordinate
                        CheckStartOrEnd(x, y);
                    }
                }

                //if we are in the last column but not the first or last row
                if ((y > 0 && y < height - 1) && x == width - 1)
                {
                    //and this is an empty spot
                    if (maze[x + width * y] == 0)
                    {
                        //check to see if we should save the start or end coordinate
                        CheckStartOrEnd(x, y);
                    }
                }
            }
        }
    }

    public void CheckStartOrEnd(int x, int y)
    {
        //if we don't have a start position
        if (startCoordinates == Vector3.zero)
        {
            //save it
            startCoordinates.x = x;
            startCoordinates.y = -y;
        }
        else
        {
            //if we do have a start position, but not an end position
            if (endCoordinates == Vector3.zero)
            {
                endCoordinates.x = x;
                endCoordinates.y = -y;
            }
        }
    }

    public List<Vector3> FindPath(Vector3 startPosition, Vector3 endPosition)
    {
        //save our start and end positions as integars
        int startX = (int)startPosition.x;
        int startY = (int)startPosition.y;

        int endX = (int)endPosition.x;
        int endY = (int)endPosition.y;

        //find the path to the end
        List<Node> path = FindPath(startX, startY, endX, endY);

        //if we don't have a path
        if(path == null)
        {
            return null;
        }
        else
        {
            //if we do have a path, convert it from nodes to vector3's
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

        //create our start and end node
        Node startNode = new Node(startX, startY);
        graph[startX, -startY] = startNode;

        Node endNode = new Node(endX, endY);
        graph[endX, -endY] = endNode;

        //create & initialize all the nodes and add them to our graph
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Node node = new Node(x, -y);
                node.gCost = int.MaxValue;
                node.CalculateFCost();
                node.previous = null;

                //if this position is blocked
                if (maze[x + width * y] == 1)
                {
                    //set not walkable
                    node.isWalkable = false;
                }
                //if this position is empty
                else if (maze[x + width * y] == 0)
                {
                    //set walkable
                    node.isWalkable = true;
                }

                //if we are in debug mode
                if (GameManager.instance.debugMode)
                {
                    //set the gCost text to say if the position is walkable or not
                    GameManager.instance.maze[x, y].GetComponent<Tile>().SetGText(node.isWalkable.ToString());

                }
                
                //add the node to the graph
                graph[x, y] = node;
            }
        }

        //initialize the start node 
        startNode.gCost = 0;
        startNode.hCost = Heuristic(startNode, endNode);
        startNode.CalculateFCost();

        //if we are in debug mode
        if (GameManager.instance.debugMode)
        {
            //set the gCost, hCost and fCost text to show them
            GameManager.instance.maze[startNode.X, Mathf.Abs(startNode.Y)].GetComponent<Tile>().SetGText(startNode.gCost.ToString());
            GameManager.instance.maze[startNode.X, Mathf.Abs(startNode.Y)].GetComponent<Tile>().SetHText(startNode.hCost.ToString());
            GameManager.instance.maze[startNode.X, Mathf.Abs(startNode.Y)].GetComponent<Tile>().SetFText(startNode.fCost.ToString());

        }

        //add the start node to the open list
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            //get the cheapest node in the open list
            Node currentNode = GetCheapestNode();


            //if this node is in the end position
            if (currentNode.X == endX && currentNode.Y == endY)
            {
                //We found the goal
                return CalculatePath(currentNode);
            }

            //add the node to the closed list and take it off the open lsit, we are done looking at it
            closedList.Add(currentNode);
            openList.Remove(currentNode);

            //get all the neighbors and look at each one
            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                //if we have already checked this node
                if (closedList.Contains(neighbor))
                {
                    //skip it
                    continue;
                }

                //if this not is not walkable
                if (neighbor.isWalkable == false)
                {
                    //add it to the closed list and skip it
                    closedList.Add(neighbor);
                    continue;
                }

                //get the new cost of the neighbor node
                int cost = currentNode.gCost + Heuristic(currentNode, neighbor);

                //if the new cost is less than the previous cost
                if(cost < neighbor.gCost)
                {
                    //set the nodes properties
                    neighbor.previous = currentNode;
                    neighbor.gCost = cost;
                    neighbor.hCost = Heuristic(neighbor, endNode);
                    neighbor.CalculateFCost();

                    //if the node isn't on the open list
                    if(!openList.Contains(neighbor))
                    {
                        //add it
                        openList.Add(neighbor);

                        //if we are in debug mode
                        if (GameManager.instance.debugMode)
                        {
                            //add the gCode, fCost and hCost to the nodes UI
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
        //set the first node to be the cheapest by default
        Node cheapestNode = openList[0];

        //loop through the open list
        for(int i = 1; i < openList.Count; i++)
        {
            //if the node we are on is cheaper
            if(openList[i].fCost < cheapestNode.fCost)
            {
                //set it as the cheapest
                cheapestNode = openList[i];
            }
        }

        return cheapestNode;
    }

    int Heuristic(Node a, Node b)
    {
        //this calculates the distance between two nodes. It is very optimistic!
        int xDifference = Mathf.Abs(a.X - b.X);
        int yDifference = Mathf.Abs(a.Y - b.Y);

        return xDifference + yDifference;
    }

    List<Node> GetNeighbors(Node currentNode)
    {
        //create the neighbor list
        List<Node> neighbors = new List<Node>();

        //if there is a neighbor to our left
        if (currentNode.X - 1 >= 0)
        {
            //add the left node
            neighbors.Add(GetNode(currentNode.X - 1, currentNode.Y));
        }

        //if there is a neighbor to our right
        if (currentNode.X + 1 < width)
        {
            //add the right node
            neighbors.Add(GetNode(currentNode.X + 1, currentNode.Y));
        }

        //if there is a neighbor below us
        if (currentNode.Y - 1 > -height)
        {
            //add the down node
            neighbors.Add(GetNode(currentNode.X, currentNode.Y - 1));
        }

        //if there is a neighbor above us
        if (currentNode.Y + 1 <= 0)
        {
            //add the up node
            neighbors.Add(GetNode(currentNode.X, currentNode.Y + 1));
        }

        return neighbors;
    }

    Node GetNode(int x, int y)
    {
        //returns a specified node from the graph
        return graph[x, Mathf.Abs(y)];
    }

    List<Node> CalculatePath(Node endNode)
    {
        //this calculates our path backwards from the end node back to the begining
        List<Node> path = new List<Node>();

        //add the end node to our path
        path.Add(endNode);

        Node currentNode = endNode;

        //while we have a previous node
        while (currentNode.previous != null)
        {
            //add them to the list
            path.Add(currentNode.previous);
            currentNode = currentNode.previous;
        }

        //reverse the path since it was added backwards
        path.Reverse();

        return path;
    }
}
