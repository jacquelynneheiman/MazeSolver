using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class MazeGenerator : MonoBehaviour
{
    [HideInInspector] public List<char> maze;
    [HideInInspector] public int height;
    [HideInInspector] public int width;

    [SerializeField] GameObject wallPrefab, floorPrefab;
    [SerializeField] CameraScaler cameraScaler;

    private void Start()
    {
        //create a new maze
        maze = new List<char>();
    }

    public void ReadMaze(string path)
    {
        StreamReader reader = new StreamReader(path);

        string entireFile = reader.ReadToEnd();

        reader.Close();

        string[] lines = entireFile.Split('\n');

        GenerateMaze(lines);
    }

    void GenerateMaze(string[] lines)
    {
        List<char> newMaze = new List<char>();

        //set the height and width of the maze
        height = lines.Length;
        width = lines[lines.Length - 1].Length;

        //keep track of how much to remove from the width after we create the maze
        int subtractFromWidth = 0;

        //keep track of which index to skip on lines after the first line
        int[] removeIndex = new int[width];

        for(int y = 0; y < height; y++)
        {
            char[] symbols = lines[y].ToCharArray();

            for (int x = 0; x < width; x++)
            {
                if(y == 0)
                {
                    if(x == 0)
                    {
                        newMaze.Add(symbols[x]);
                    }
                    else
                    {
                        if(symbols[x] == '\n')
                        {
                            continue;
                        }

                        if(symbols[x - 1] == '-' && symbols[x] == '-')
                        {
                            subtractFromWidth++;

                            removeIndex[x] = 1;

                            continue;
                        }
                        else if(symbols[x - 1] == ' ' && symbols[x] == ' ')
                        {
                            subtractFromWidth++;

                            removeIndex[x] = 1;

                            continue;
                        }
                        else
                        {
                            newMaze.Add(symbols[x]);
                        }
                    }
                }
                else
                {
                    if(removeIndex[x] == 1)
                    {
                        continue;
                    }
                    else
                    {
                        newMaze.Add(symbols[x]);
                    }
                }
            }
        }

        width -= subtractFromWidth;

        SpawnMaze(newMaze);
        
    }

    void SpawnMaze(List<char> maze)
    {
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                if(maze[x + width * y] == '-' || maze[x + width * y] == '+' || maze[x + width * y] == '|')
                {
                    GameObject wall = Instantiate(wallPrefab);

                    wall.transform.SetParent(transform, false);
                    wall.transform.localPosition = new Vector2(x, -y);

                    wall.name = "(" + x + ", " + y + ")";

                    //if we are in debug mode
                    if(GameManager.instance.debugMode)
                    {
                        GameManager.instance.maze[x, y] = wall;
                    }
                    
                }

                if (maze[x + width * y] == ' ')
                {
                    GameObject floor = Instantiate(floorPrefab);

                    floor.transform.SetParent(transform, false);
                    floor.transform.localPosition = new Vector2(x,  -y);

                    floor.name = "(" + x + ", " + y + ")";

                    if (GameManager.instance.debugMode)
                    {
                        GameManager.instance.maze[x, y] = floor;
                    }

                }
            }
        }

        this.maze = maze;

        //resize the camera so the whole maze is in view
        cameraScaler.ScaleCamera(new Vector2(width, -height));
    }
}
