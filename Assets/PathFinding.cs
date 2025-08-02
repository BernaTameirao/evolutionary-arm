// This code is responsible for calculating the A* pathfinding

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    //the destination
    public Transform target;
    //the evolution script
    private Evolution EvScript;
    private bool pathFound = false;
    private Gridi usedGrid2;
    private RobotGrid usedGrid1;
    private Vector3 startPosition;
    private Vector3 endPosition;

    //public Evolution evolutionScript;
    public void Awake()
    {
        EvScript = GetComponent<Evolution>();

        usedGrid1 = GetComponent<RobotGrid>();
        usedGrid2 = GetComponent<Gridi>();
        //if not already, makes the seeker position go to the origin
        startPosition = new Vector3(0, 0.1f, 0);

        pathFound = false;
        this.enabled = false;
    }

    public void activate() {
        if(!GetComponent<MainUIController>().isCollectingData || GetComponent<CreateScene>().isCreatingTunnels){
            endPosition = target.position;
        }

        this.enabled = true;
    }

    private void Update()
    {
        if(!pathFound)
            FindPath(endPosition, startPosition);   
    } 

    public List<Node> answerPath() {
        return EvScript.isShowingFog ? usedGrid1.path : usedGrid2.path;
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = EvScript.isShowingFog ? usedGrid1.NodeFromWorldPoint(startPos) : usedGrid2.NodeFromWorldPoint(startPos);
        Node targetNode = EvScript.isShowingFog ? usedGrid1.NodeFromWorldPoint(targetPos) : usedGrid2.NodeFromWorldPoint(targetPos);
        
        // we use the openSet list to search for nodes that are yet to be explored
        List<Node> openSet = new List<Node>();

        // We use the hashset to store the nodes that are not being explored, as it has better performance to 
        // search for specific elements within itself and it does not allow duplicates
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // this loop searches for the best term within openSet to be explored
            Node currentNode = openSet[0];
            for(int i = 1; i <openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode); 
            closedSet.Add(currentNode);

            if(currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                pathFound = true;
                EvScript.activate();
                return;
            }

            List<Node> neighbours = EvScript.isShowingFog ? usedGrid1.GetNeighbours(currentNode, false) : usedGrid2.GetNeighbours(currentNode, false);
            foreach (Node neighbour in neighbours)
            {
                if(closedSet.Contains(neighbour))
                {
                    continue;
                }

                float newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);

                    // It is necessary to define the node's parent to be able to retrace the path to it
                    neighbour.parent= currentNode;
                    
                    if(!openSet.Contains(neighbour)) openSet.Add(neighbour);
                }
            }
        }
    }

    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        // The path is traced from end to beginning, so the array is reversed
        //path.Reverse();
        if(EvScript.isShowingFog){

            usedGrid1.path = path;
        } else {

            usedGrid2.path = path;
        }

    }

    // In A* 2d the distance formula is based on the module of the vectors (0, 10) and (10, 0) which have module 10. 
    // The vector (10,10) which has an approximate module of 14 is also used.
    int GetDistance(Node nodeA, Node nodeB)
    {
        int[] sortedValues = { Mathf.Abs(nodeA.gridX - nodeB.gridX), Mathf.Abs(nodeA.gridY - nodeB.gridY), Mathf.Abs(nodeA.gridZ - nodeB.gridZ)};
        Array.Sort(sortedValues);

        // In A* 3D, the distance formula is based on the modulus of the vectors: (0, 0, 10), (0, 10, 0) 
        // and (10, 0, 0) which have modulus 10; The vectors (10,10, 0), (0, 10, 10) and (0, 10, 0) 
        // which have an approximate modulus of 14; And the vector (10, 10, 10) 
        // which has an approximate modulus of 17.

        return sortedValues[0] * 17 + (sortedValues[1] - sortedValues[0]) * 14 + (sortedValues[2] - sortedValues[1]) * 10;
    }

    public void ChangePath(Vector3 newStartPosition){

        startPosition = newStartPosition;
        pathFound = false;

    }
}