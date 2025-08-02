// D* Lite-inspired pathfinding (simplified)
// Assumes Node has g, rhs, h, and parent fields, and usedGrid provides GetNeighbours()

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class DStarPathFinding : MonoBehaviour
{
    PriorityQueue<Node> openSet = new PriorityQueue<Node>(); // min-heap based on priority = min(g, rhs) + h

    Node startNode, goalNode;
    //the destination
    public Transform target;
    //the evolution script
    private Evolution EvScript;
    private RobotGrid usedGrid;
    private Vector3 startPosition;
    private Vector3 endPosition;

    public void Awake(){

        EvScript = GetComponent<Evolution>();
        usedGrid = GetComponent<RobotGrid>();
    }

    public void Initialize()
    {
        //if not already, makes the seeker position go to the origin
        startPosition = new Vector3(0, 0.1f, 0);
        endPosition = target.position;
        startNode = usedGrid.NodeFromWorldPoint(startPosition);
        goalNode = usedGrid.NodeFromWorldPoint(endPosition);

        foreach (Node n in usedGrid.GetAllNodes())
        {
            n.gCost = Mathf.Infinity;
            n.rhs = Mathf.Infinity;
            n.hCost = GetDistance(n, startNode);
            n.parent = null;
        }

        goalNode.rhs = 0;
        openSet.Enqueue(goalNode, CalculatePriority(goalNode));

        ComputeShortestPath();

        EvScript.activate();
    }

    void ComputeShortestPath()
    {
        while (openSet.Count > 0 && (startNode.gCost != startNode.rhs || openSet.PeekPriority() < CalculatePriority(startNode)))
        {
            Node u = openSet.Dequeue();

            if (u.gCost > u.rhs)
            {
                u.gCost = u.rhs;
                foreach (Node neighbor in usedGrid.GetNeighbours(u, false))
                {
                    UpdateVertex(neighbor);
                }
            }
            else
            {
                u.gCost = Mathf.Infinity;
                UpdateVertex(u);
                foreach (Node neighbor in usedGrid.GetNeighbours(u, false))
                {
                    UpdateVertex(neighbor);
                }
            }
        }
    }

    void UpdateVertex(Node u)
    {
        if (u != goalNode)
        {
            float minRHS = Mathf.Infinity;
            Node bestPred = null;

            foreach (Node pred in usedGrid.GetNeighbours(u, false))
            {
                float tentative = pred.gCost + GetDistance(u, pred);
                if (tentative < minRHS)
                {
                    minRHS = tentative;
                    bestPred = pred;
                }
            }

            u.rhs = minRHS;
            u.parent = bestPred;
        }

        openSet.Remove(u);
        if (u.gCost != u.rhs)
        {
            openSet.Enqueue(u, CalculatePriority(u));
        }
    }

    float CalculatePriority(Node n)
    {
        return Mathf.Min(n.gCost, n.rhs) + n.hCost;
    }

    public void MoveAndUpdate(Vector3 newStartPos, GameObject obj)
    {
        startNode = usedGrid.NodeFromWorldPoint(newStartPos);
        foreach (Node changed in usedGrid.GetObstacleNodes(obj))
        {
            UpdateVertex(changed);
            foreach (Node neighbor in usedGrid.GetNeighbours(changed, false))
            {
                UpdateVertex(neighbor);
            }
        }
        ComputeShortestPath();
    }

    int GetDistance(Node a, Node b)
    
    {
        int[] sortedValues = { Mathf.Abs(a.gridX - b.gridX), Mathf.Abs(a.gridY - b.gridY), Mathf.Abs(a.gridZ - b.gridZ)};
        Array.Sort(sortedValues);
        return sortedValues[0] * 17 + (sortedValues[1] - sortedValues[0]) * 14 + (sortedValues[2] - sortedValues[1]) * 10;
    }

    public List<Node> GetPath()
    {
        List<Node> path = new List<Node>();
        Node current = startNode;
        while (current != goalNode && current != null)
        {
            path.Add(current);
            current = current.parent;
        }
        if (current == goalNode) path.Add(goalNode);
        usedGrid.path = path;
        return path;
    }
}
