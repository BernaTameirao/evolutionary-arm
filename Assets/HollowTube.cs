using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HollowTube : MonoBehaviour
{
    public int numElements = 10;
    public float innerRadius = 0.2f;
    public float outerRadius = 0.3f;
    public float height = 1f;
    public int segments = 15;

    void Awake()
    {

        for(int counter=0; counter<numElements; counter++){
            
            height = Random.Range(0.2f, 3f);

            SpawnHollowTube(new Vector3(Random.Range(-5f, 5f), Random.Range(0f, 2.5f), Random.Range(-5f, 5f)),
                            Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f)
            ));
        }
    }

    void SpawnHollowTube(Vector3 position, Quaternion rotation)
    {
        GameObject tube = new GameObject("HollowTube");
        tube.transform.position = position;
        tube.transform.rotation = rotation;

        MeshFilter meshFilter = tube.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = tube.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard")); // Default material

        Mesh mesh = GenerateTube();
        meshFilter.mesh = mesh;

        BoxCollider boxCollider = tube.AddComponent<BoxCollider>();
        boxCollider.enabled = true;

    }

    Mesh GenerateTube()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[segments * 4 * 2];  // Top and bottom faces
        int[] triangles = new int[segments * 12 * 2];        // Walls and caps

        float angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.Deg2Rad * angleStep * i;
            float nextAngle = Mathf.Deg2Rad * angleStep * (i + 1);

            int vertIndex = i * 8;

            // Bottom face
            vertices[vertIndex] = new Vector3(Mathf.Cos(angle) * innerRadius, 0, Mathf.Sin(angle) * innerRadius);
            vertices[vertIndex + 1] = new Vector3(Mathf.Cos(angle) * outerRadius, 0, Mathf.Sin(angle) * outerRadius);
            vertices[vertIndex + 2] = new Vector3(Mathf.Cos(nextAngle) * innerRadius, 0, Mathf.Sin(nextAngle) * innerRadius);
            vertices[vertIndex + 3] = new Vector3(Mathf.Cos(nextAngle) * outerRadius, 0, Mathf.Sin(nextAngle) * outerRadius);

            // Top face (same positions, but with height)
            vertices[vertIndex + 4] = vertices[vertIndex] + Vector3.up * height;
            vertices[vertIndex + 5] = vertices[vertIndex + 1] + Vector3.up * height;
            vertices[vertIndex + 6] = vertices[vertIndex + 2] + Vector3.up * height;
            vertices[vertIndex + 7] = vertices[vertIndex + 3] + Vector3.up * height;

            int triIndex = i * 24;

            // Outer wall
            triangles[triIndex] = vertIndex + 1;
            triangles[triIndex + 1] = vertIndex + 5;
            triangles[triIndex + 2] = vertIndex + 3;

            triangles[triIndex + 3] = vertIndex + 5;
            triangles[triIndex + 4] = vertIndex + 7;
            triangles[triIndex + 5] = vertIndex + 3;

            // Inner wall
            triangles[triIndex + 6] = vertIndex;
            triangles[triIndex + 7] = vertIndex + 2;
            triangles[triIndex + 8] = vertIndex + 4;

            triangles[triIndex + 9] = vertIndex + 2;
            triangles[triIndex + 10] = vertIndex + 6;
            triangles[triIndex + 11] = vertIndex + 4;

            // Bottom cap
            triangles[triIndex + 12] = vertIndex;
            triangles[triIndex + 13] = vertIndex + 1;
            triangles[triIndex + 14] = vertIndex + 2;

            triangles[triIndex + 15] = vertIndex + 1;
            triangles[triIndex + 16] = vertIndex + 3;
            triangles[triIndex + 17] = vertIndex + 2;

            // Top cap
            triangles[triIndex + 18] = vertIndex + 4;
            triangles[triIndex + 19] = vertIndex + 6;
            triangles[triIndex + 20] = vertIndex + 5;

            triangles[triIndex + 21] = vertIndex + 5;
            triangles[triIndex + 22] = vertIndex + 6;
            triangles[triIndex + 23] = vertIndex + 7;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}
