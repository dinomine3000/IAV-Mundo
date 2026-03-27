using System;
using UnityEngine;

public class cubeGen : MonoBehaviour
{
    public int cubeCount = 5;
    public Material material;
    
    void Start()
    {
        CombineMeshes();
        CombineQuads();
    }

    void CombineQuads()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }
        // Destruir os quads temporários
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        mf.mesh = new Mesh();
        mf.mesh.CombineMeshes(combine);
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mr.material = material;
    }
    void CombineMeshes()
    {
        
        //front
        Vector3[] frontVerts = {            
            new Vector3(0.5f, 0.5f, 0.5f), // 0: top-left
            new Vector3( -0.5f, 0.5f, 0.5f), // 1: top-right
            new Vector3(0.5f, -0.5f, 0.5f), // 2: bottom-left
            new Vector3( -0.5f, -0.5f, 0.5f), // 3: bottom-right  
        };
        CreateQuad(frontVerts, Vector3.forward);

        //back
        Vector3[] backVerts = {            
            new Vector3(-0.5f, 0.5f, -0.5f), // 0: top-left
            new Vector3(0.5f, 0.5f, -0.5f), // 1: top-right
            new Vector3(-0.5f, -0.5f, -0.5f), // 2: bottom-left
            new Vector3(0.5f, -0.5f, -0.5f), // 3: bottom-right  
        };
        CreateQuad(backVerts, Vector3.back);

        //top
        Vector3[] topVerts = {            
            new Vector3(0.5f, 0.5f, -0.5f), // 0: top-left
            new Vector3(-0.5f, 0.5f, -0.5f), // 1: top-right
            new Vector3(0.5f, 0.5f, 0.5f), // 2: bottom-left
            new Vector3(-0.5f, 0.5f, 0.5f), // 3: bottom-right  
        };
        CreateQuad(topVerts, Vector3.up);

        //bottom
        Vector3[] bottomVerts = {            
            new Vector3(0.5f, -0.5f, 0.5f), // 0: top-left
            new Vector3(-0.5f, -0.5f, 0.5f), // 1: top-right
            new Vector3(0.5f, -0.5f, -0.5f), // 2: bottom-left
            new Vector3(-0.5f, -0.5f, -0.5f), // 3: bottom-right  
        };
        CreateQuad(bottomVerts, Vector3.down);

        //left / negative X
        Vector3[] leftVerts = {            
            new Vector3(-0.5f, 0.5f, 0.5f), // 0: top-left
            new Vector3(-0.5f, 0.5f, -0.5f), // 1: top-right
            new Vector3(-0.5f, -0.5f, 0.5f), // 2: bottom-left
            new Vector3(-0.5f, -0.5f, -0.5f), // 3: bottom-right  
        };
        CreateQuad(leftVerts, Vector3.left, "leftQuad");

        //right / positive x
        Vector3[] rightVerts = {            
            new Vector3(0.5f, 0.5f, -0.5f), // 0: top-left
            new Vector3(0.5f, 0.5f, 0.5f), // 1: top-right
            new Vector3(0.5f, -0.5f, -0.5f), // 2: bottom-left
            new Vector3(0.5f, -0.5f, 0.5f), // 3: bottom-right  
        };
        CreateQuad(rightVerts, Vector3.right, "rightQuad");
    }

    void CreateQuad(Vector3[] faceVertices, Vector3 normal, String name = "") { 
        Mesh mesh = new Mesh();
        //vertices
        Vector3[] vertices = faceVertices;
        mesh.vertices = vertices;
        //triangulos
        int[] triangles =
        {
            0, 1, 2, 1, 3, 2
        };
        mesh.triangles = triangles;
        //normais
        Vector3[] normals =
        {
            normal,
            normal,
            normal,
            normal
        };
        mesh.normals = normals;
        //UV coords
        Vector2[] uv =
        {
            new Vector2(0f, 1f),
            new Vector2(1f, 1f), 
            new Vector2(0f, 0f),
            new Vector2(1f, 0f)
        };
        mesh.uv = uv;

        //adicionar e renderizar componente
        //gameObject.AddComponent<MeshFilter>().mesh = mesh;
        //gameObject.AddComponent<MeshRenderer>().material = material;

        //dar à luz ao filho
        GameObject quad = new GameObject("Quad");
        if(name != "")
            quad.name = name;
        quad.transform.parent = transform;
        quad.AddComponent<MeshFilter>().mesh = mesh;
        quad.AddComponent<MeshRenderer>().material = material;
    }
}
