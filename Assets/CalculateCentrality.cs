using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateCentrality : MonoBehaviour
{   
    private int gridSizeX, gridSizeY, gridSizeZ;

    private void Start(){

        gridSizeX = GetComponent<Gridi>().gridSizeX;
        gridSizeY = GetComponent<Gridi>().gridSizeY;
        gridSizeZ = GetComponent<Gridi>().gridSizeZ;
    }


    public List<int> CalculateDegreeCentrality(Node[,,] graph)
    {
        List<int> degreeCentrality = new List<int>();

        for(int x=0; x<gridSizeX; x++){
            for(int y=0; y<gridSizeY; y++){
                for(int z=0; z<gridSizeZ; z++){

                    if(graph[x, y, z].walkable)
                        degreeCentrality.Add(graph[x, y, z].neighbours.Count);

                }
            }
        }

        return degreeCentrality;
    }

    /*public List<float> CalculateClosenessCentrality(Node[,,] graph)
    {
        List<float> closenessCentrality = new List<float>();

        foreach (Node node in graph.nodes)
        {
            float totalDistance = 0f;

            foreach (Node other in graph.nodes)
            {
                if (node != other)
                {
                    totalDistance += ShortestPathLength(graph, node, other);
                }
            }

            closenessCentrality[node] = totalDistance > 0 ? 1f / totalDistance : 0f;
        }

        return closenessCentrality;
    }

    // Exemplo de função para calcular o caminho mais curto entre dois nós (BFS para grafos simples)
    private int ShortestPathLength(Node[,,] graph, Node start, Node end)
    {
        Queue<Node> queue = new Queue<Node>();
        Dictionary<Node, int> distances = new Dictionary<Node, int>();

        foreach (Node node in graph.nodes)
        {
            distances[node] = int.MaxValue;
        }

        distances[start] = 0;
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            Node current = queue.Dequeue();
            foreach (Node neighbour in current.neighbours)
            {
                if (distances[neighbour] == int.MaxValue)
                {
                    distances[neighbour] = distances[current] + 1;
                    queue.Enqueue(neighbour);
                }
            }
        }

        return distances[end];
    }*/

}
