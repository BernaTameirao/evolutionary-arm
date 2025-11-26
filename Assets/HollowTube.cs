using UnityEngine;

public class HollowTube : MonoBehaviour
{
    public GameObject cubePrefab;      // Prefab do cubo individual
    private Evolution EvScript;
    private RobotGrid script;
    private CreateScene SceneScript;

    void Awake(){

        EvScript = GetComponent<Evolution>();
        SceneScript = GetComponent<CreateScene>();
    }

    /// <summary>
    /// Creates a tube composed of obstacles.
    /// <parameters>
    /// position (Vector3): position of the entrance of the tunnel,
    /// radius (float): radius of the tunnel.
    /// height (float): height of the tunnel.
    /// segments (int): number of obstacles that compose the walls of the tunnel.
    /// dir (Vector3): direction that the tunnel is faced to.
    public void BuildTube(Vector3 position, float radius = 0.3f, float height = 1f, int segments = 8, Vector3? dir = null)
    {
        Vector3 direction = dir ?? Vector3.forward;
        direction.Normalize();

        // safely grasp the "cube size" (the collider's largest dimension)
        var col = cubePrefab.GetComponent<Collider>();
        float cubeSize = Mathf.Max(col.bounds.size.x, col.bounds.size.y, col.bounds.size.z);

        // guarantees at least one vertical segment (avoids division by zero))
        int heightSegments = Mathf.Max(1, Mathf.RoundToInt(height / cubeSize));
        float angleStep = 360f / segments;
        float heightStep = height / heightSegments;

        // base for the circle (tangent/bitangent)
        Vector3 tangent = Vector3.Cross(direction, Vector3.up);
        if (tangent.sqrMagnitude < 0.0001f)
            tangent = Vector3.Cross(direction, Vector3.right);
        tangent.Normalize();
        Vector3 bitangent = Vector3.Cross(direction, tangent);

        for (int h = 0; h < heightSegments; h++)
        {
            float t = h * heightStep;
            Vector3 center = position + direction * t;

            for (int i = 0; i < segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 offset = (Mathf.Cos(angle) * tangent + Mathf.Sin(angle) * bitangent) * radius;
                Vector3 worldPos = center + offset;

                GameObject cube = SceneScript.GetObstacleFromPool();
                cube.transform.position = worldPos;

                // Makes the cube "look" out of the cylinder.
                cube.transform.rotation = Quaternion.LookRotation(offset.normalized, direction);

                EvScript.addObstacle(cube);
            }
        }
    }

    /// <summary>
    /// Creates a "Tube System", that is, a tunnel formed by several tubes, that can be in different directions.
    /// <parameters>
    /// position (Vector3): position of the entrance of the tunnel.
    /// length (float): length of each tube
    /// numCorners (int): number of corners, that is, number of tubes + 1.
    public void BuildTubeSystem(Vector3 position, float length, int numCorners){

        Vector3 previousDirection = Vector3.zero;
        float radius = 0.3f;

        for(int i=0; i<numCorners+1; i++){
            
            // Gets a random direction.
            Vector3 direction = obtainRandomDirection(false);

            // If isn't the first iteration, creates a corner.
            if(previousDirection != Vector3.zero){

                position = BuildCorner(position, radius, length, 8, previousDirection, direction);
            }

            // Creates a tube and updates the position of the tube entrance.
            BuildTube(position, 0.3f, length, 8, direction);
            position += direction*(length);

            // Places the objective in the middle of the system.
            if(i == (numCorners+1)/2){

                EvScript.goal.transform.position = position;
            }

            // Updates the previous direction
            previousDirection = direction;
        }
    }

    private Vector3 BuildCorner(Vector3 position, float radius, float height, int segments, Vector3 dirA, Vector3 dirB, int steps = 8)
    {
        // validações básicas
        steps = Mathf.Max(1, steps);
        if (dirA.sqrMagnitude < 1e-6f || dirB.sqrMagnitude < 1e-6f)
            return position; 

        dirA.Normalize();
        dirB.Normalize();

        float angle = Vector3.Angle(dirA, dirB);

        // se as direções forem praticamente iguais, só cria um segmento reto
        if (angle < 0.01f)
        {
            BuildTube(position, radius, height, segments, dirA);
            return position;
        }

        // calcula axis; se for pequeno (parallel/antiparallel), escolhe um fallback
        Vector3 axis = Vector3.Cross(dirA, dirB);
        if (axis.sqrMagnitude < 1e-6f)
        {
            axis = Vector3.Cross(dirA, Vector3.up);
            if (axis.sqrMagnitude < 1e-6f)
                axis = Vector3.Cross(dirA, Vector3.right);
        }
        axis.Normalize();

        Vector3 currentPos = position;
        float subHeight = height / steps;

        for (int i = 0; i < steps; i++)
        {
            float t0 = i / (float)steps;
            float t1 = (i + 1) / (float)steps;

            Vector3 d0 = Quaternion.AngleAxis(angle * t0, axis) * dirA;
            Vector3 d1 = Quaternion.AngleAxis(angle * t1, axis) * dirA;

            // cria o pedaço do tubo apontando na direção d0
            BuildTube(currentPos, radius, subHeight, segments, d0);

            // avança a posição usando a direção média entre d0 e d1 para reduzir gaps
            Vector3 avgDir = (d0 + d1).normalized;
            currentPos += avgDir * subHeight;
        }

        return currentPos;
    }

    /// <summary>
    /// Function used to get a random direction.
    /// <parameters>
    /// everyDirection (bool): bool that defines if every direction can be returned or only the 3 normal ones.
    /// <returns>
    /// the random direction (Vector3)
    private Vector3 obtainRandomDirection(bool everyDirection){

        int randomNum = everyDirection ? Random.Range(0, 6) : Random.Range(0, 3);

        switch(randomNum){

            case 0:
                return Vector3.up;

            case 1:
                return Vector3.right;

            case 2:
                return Vector3.forward;

            case 3:
                return Vector3.down;

            case 4:
                return Vector3.left;

            case 5:
                return Vector3.back;

            default:
                return Vector3.zero;
        }
    }
}
