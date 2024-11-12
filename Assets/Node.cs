//This code defines the Node class, which is the basic component of the grid

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public int gridZ;

    public int gCost;   //In A*, gCost is the distance between the starting point and the node
    public int hCost;   //In A*, hCost is the distance between the arrival point and the node

    public Node parent;

    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _gridZ)
    {
        this.walkable = _walkable;
        this.worldPosition = _worldPos;
        this.gridX = _gridX;
        this.gridY = _gridY;
        this.gridZ = _gridZ;
    }

    public int fCost
    {
        get{
            return gCost + hCost;
        }
    }
}