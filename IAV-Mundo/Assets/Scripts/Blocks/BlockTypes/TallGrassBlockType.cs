using System.Collections.Generic;
using UnityEngine;

public class TallGrassBlockType : BlockType
{
    public TallGrassBlockType() : base(new(0, 0))
    {}


    //TODO
    public new void AddNonSolidFaceToMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {        
        /*int vertexIndex = vertices.Count;
        List<Vector3> faceVerts = Block.FaceVerticesMap[face];
        List<Vector3> worldVerts = faceVerts.Select(v => v + position).ToList();
        vertices.AddRange(worldVerts);
        triangles.AddRange(new List<int>
        {
            0 + vertexIndex, 1 + vertexIndex, 2 + vertexIndex, 
            1 + vertexIndex, 3 + vertexIndex, 2 + vertexIndex
        });
        //uvs.AddRange(new List<Vector2>{new(0, 1), new(1, 1), new(0, 0), new(1, 0)});
        uvs.AddRange(GetUVs(face, type));*/
    }

    public static List<int> GetCrossBlockTriangles(int startIndex)
    {
        List<int> tris = new();

        // Quad A (v5, v3, v1, v7)
        int a = startIndex + 0;
        int b = startIndex + 1;
        int c = startIndex + 2;
        int d = startIndex + 3;

        // Front
        tris.Add(a); tris.Add(b); tris.Add(c);
        tris.Add(c); tris.Add(b); tris.Add(d);

        // Back (reverse winding)
        tris.Add(c); tris.Add(b); tris.Add(a);
        tris.Add(d); tris.Add(b); tris.Add(c);

        // Quad B (v4, v2, v0, v6)
        int e = startIndex + 4;
        int f = startIndex + 5;
        int g = startIndex + 6;
        int h = startIndex + 7;

        // Front
        tris.Add(e); tris.Add(f); tris.Add(g);
        tris.Add(g); tris.Add(f); tris.Add(h);

        // Back
        tris.Add(g); tris.Add(f); tris.Add(e);
        tris.Add(h); tris.Add(f); tris.Add(g);

        return tris;
    }
}