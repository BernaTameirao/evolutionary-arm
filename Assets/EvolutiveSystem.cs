using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolutiveSystem : MonoBehaviour
{   

    public GameObject goal;
    public GameObject sphere;

    public int numPopulation;
    public int numGenerations;

    private int numSegments;
    private Vector3[] spawnRotation;
    private Vector3[][] population;

    private SpawnerCorpo instance;
    private PathFinding astarScript;
    private Queue<Node> pathQueue;

    private int generation = 0;
    public int generationsPerObjective = 200;

    void Awake()
    {
        astarScript = GetComponent<PathFinding>();
        instance = GetComponent<SpawnerCorpo>();
        numSegments = instance.numSegments;

        population = CreatePopulation(numPopulation);

        this.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        sphere = GameObject.Find("SphereRobot");
    }

    // Update is called once per frame
    void Update()
    {   
        
        if(generation % generationsPerObjective == 0) {
            if(pathQueue.Count != 0) {
                Node aux = pathQueue.Dequeue();
                goal.transform.position = aux.worldPosition;
            }
        }

        Evolution(numGenerations);
        
        generation++;
    }

    Vector3[][] CreatePopulation(int numPopulation){
        
        Vector3[][] population = new Vector3[numPopulation][];

        for(int counter=0; counter<numPopulation; counter++){

            population[counter] = new Vector3[numSegments];

            for(int counter2=0; counter2<numSegments; counter2++){

                float x = Random.Range(0f, 360f);
                float y = Random.Range(0f, 360f);
                float z = Random.Range(0f, 360f);

                population[counter][counter2] = new Vector3(x, y, z);
            }
        }

        return population;
    }

    void Evolution(int numGenerations
                    //, Vector3[][] population
    ){

        float bestScore = 999999;
        Vector3[] bestOfAll = new Vector3[numSegments];

        //Debug.Log(population[0][0]+"2");

        for(int counter=0; counter<numGenerations; counter++){

            for(int counter2=0; counter2<population.Length; counter2++){

                GetComponent<SpawnerCorpo>().spawnRotation = population[counter2];

                instance.updateObject();

                float score = Vector3.Distance(goal.transform.position, sphere.transform.position);

                if(score<bestScore)
                {
                    bestScore = score;
                    bestOfAll = population[counter2];
                }
            }

            population = Reproduction(population, bestOfAll);

            Mutation(population);
        }

        //Debug.Log(population[0][0]+"3");
    }

    Vector3[][] Reproduction(Vector3[][] population, Vector3[] bestOfAll){
        
        Vector3[][] newPopulation = new Vector3[population.Length][];

        for(int counter=0; counter<population.Length; counter++){
            
            newPopulation[counter] = new Vector3[numSegments];

            for(int counter2=0; counter2<numSegments; counter2++){

                newPopulation[counter][counter2] = (population[counter][counter2] + bestOfAll[counter2])/2;
            }
        }

        return newPopulation;
    }

    void Mutation(Vector3[][] population){
        
        for(int counter=0; counter<population.Length; counter++){

            int segment = Random.Range(0, numSegments);
            Vector3 value = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f));

            population[counter][segment] += value; 
        }
    }

    public void activate() {
        this.enabled = true;
        //gets the path found by the a* algorithm and enqueue them into the queue
        pathQueue = new Queue<Node>();
        List<Node> aux = astarScript.answerPath();
        aux.ForEach(pathQueue.Enqueue);
        //already sets the goal as the first element of the path
        goal.transform.position = pathQueue.Dequeue().worldPosition;
    }
}
