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
        //read the file
        StreamReader reader = new StreamReader(path);

        //save the file 
        string entireFile = reader.ReadToEnd();

        //close the file
        reader.Close();

        //split the file into lines
        string[] lines = entireFile.Split('\n');

        //generate the maze
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
            //split the contents of the line into an array
            char[] symbols = lines[y].ToCharArray();

            for (int x = 0; x < width; x++)
            {
                //if we are on the first row
                if(y == 0)
                {
                    //and we are on the first column
                    if(x == 0)
                    {
                        //add the symbol because there are no symbols before this one on this line
                        newMaze.Add(symbols[x]);
                    }
                    else
                    {
                        //otherwise, if the symbol we are on is a new line character
                        if(symbols[x] == '\n')
                        {
                            //skip it
                            continue;
                        }

                        //if the previous symbol is a - and the symbol we are on is a -
                        if(symbols[x - 1] == '-' && symbols[x] == '-')
                        {
                            //add one to the amount we need to subtract from the width
                            subtractFromWidth++;

                            //mark down that we need to remove this index
                            removeIndex[x] = 1;

                            //then skip this symbol
                            continue;
                        }
                        else
                        {
                            //otherwise, add the symbol to the maze
                            newMaze.Add(symbols[x]);
                        }
                    }
                }
                else
                {
                    //otherise, check if we need to skip this symbol
                    if(removeIndex[x] == 1)
                    {
                        //if so, skip it
                        continue;
                    }
                    else
                    {
                        //otherwise, add the symbol to the maze
                        newMaze.Add(symbols[x]);
                    }
                }
            }
        }

        //subtract all the ones we skipped from the width
        width -= subtractFromWidth;

        //then spawn the maze
        SpawnMaze(newMaze);
        
    }

    void SpawnMaze(List<char> maze)
    {
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                //if the position we are on is any of the wall symbols
                if(maze[x + width * y] == '-' || maze[x + width * y] == '+' || maze[x + width * y] == '|')
                {
                    //spawn a wall 
                    GameObject wall = Instantiate(wallPrefab);

                    //position the wall in the world
                    wall.transform.SetParent(transform, false);
                    wall.transform.localPosition = new Vector2(x, -y);

                    //rename the wall to something easy for testing
                    wall.name = "(" + x + ", " + y + ")";

                    //if we are in debug mode
                    if(GameManager.instance.debugMode)
                    {
                        //add the wall to the maze list in the game manager
                        GameManager.instance.maze[x, y] = wall;
                    }
                    
                }

                //if this position is an empty position
                if (maze[x + width * y] == ' ')
                {
                    //spawn a floor 
                    GameObject floor = Instantiate(floorPrefab);

                    //position the floor in the world
                    floor.transform.SetParent(transform, false);
                    floor.transform.localPosition = new Vector2(x,  -y);

                    //rename the floor to something easy for testing
                    floor.name = "(" + x + ", " + y + ")";

                    //if we are in debug mode
                    if (GameManager.instance.debugMode)
                    {
                        //add the floor to the maze in the game manager
                        GameManager.instance.maze[x, y] = floor;
                    }

                }
            }
        }

        //set this finished maze to the maze in this file
        this.maze = maze;

        //resize the camera so the whole maze is in view
        cameraScaler.ScaleCamera(new Vector2(width, -height));
    }
}
