using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int gCost;
    public int hCost;
    public int fCost;

    public bool isWalkable = true;

    public Node previous;

    int x, y;


    //PROPERTIES
    public int X
    {
        get
        {
            return x;
        }

        set
        {
            x = value;
        }
    }

    public int Y
    {
        get
        {
            return y;
        }

        set
        {
            y = value;
        }
    }


    public Node(int x, int y)
    {
        //constructor sets the x, y and walkable on creation
        this.x = x;
        this.y = y;
        isWalkable = true;
    }

    public void CalculateFCost()
    {
        //calculates the f cost
        fCost = gCost + hCost;
    }
}
