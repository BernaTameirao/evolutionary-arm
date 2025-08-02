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

    public void InitializeTube(Vector3 position, float radius =0.3f, float height=1f, int segments=8)
    {
        transform.position = position;
        BuildTube(radius, height, segments);
    }

    private void BuildTube(float radius, float height, int segments)
    {   

        int heightSegments = Mathf.RoundToInt(height/cubePrefab.GetComponent<Collider>().bounds.size.y);
        float angleStep = 360f / segments;
        float heightStep = height / heightSegments;

        for (int h = 0; h < heightSegments; h++)
        {
            float y = h * heightStep;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                Vector3 localPos = new Vector3(x, z, y);
                Vector3 worldPos = transform.position + localPos;
                //GameObject cube = Instantiate(cubePrefab, worldPos, Quaternion.identity, this.transform);
                GameObject cube = SceneScript.GetObstacleFromPool();
                cube.transform.position = worldPos;
                cube.transform.rotation = Quaternion.identity;

                //cube.layer = 3;
                //cube.AddComponent<CheckMovement>();
                EvScript.addObstacle(cube);
            }
        }
    }

    //TO-DO:
    //- Fazer o A* ter uma barreina invisível, feita dinanicamente para impedir que leve o braço até um lugar que ele seja muito curto para alcançar.
    //- Construir uma parede física.
    //- Definir funções de fitness mais refinadas.
}
