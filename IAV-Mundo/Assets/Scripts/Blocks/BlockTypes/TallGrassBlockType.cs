using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TallGrassBlockType : BlockType
{
    public TallGrassBlockType() : base(new(0, 0), false, true, true)
    {}


    //TODO
    public new void AddCustomFaceToMesh(Block.CubeFace face, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, Vector3 position)
    {        
        if(face != Block.CubeFace.ALL) return;
        int vertexIndex = vertices.Count;
        List<Vector3> faceVerts = Block.FaceVerticesMap[face];
        List<Vector3> worldVerts = faceVerts.Select(v => v + position).ToList();
        vertices.AddRange(worldVerts);
        triangles.AddRange(new List<int>
        {
            0 + vertexIndex, 1 + vertexIndex, 2 + vertexIndex, 
            1 + vertexIndex, 3 + vertexIndex, 2 + vertexIndex
        });
        //uvs.AddRange(new List<Vector2>{new(0, 1), new(1, 1), new(0, 0), new(1, 0)});
        uvs.AddRange(Block.GetUVs(face, this));
    }

    public void AddCrossQuadToMesh(bool isFirstDiagonal, 
        List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, Vector3 position)
    {
        int vertexIndex = vertices.Count;

        // Determine diagonal vertices based on your v0-v7 definitions
        // First Diagonal: v6 (Top-Left) to v4 (Top-Right) / v2 (Bottom-Left) to v0 (Bottom-Right)
        // Second Diagonal: v7 (Top-Left) to v5 (Top-Right) / v3 (Bottom-Left) to v1 (Bottom-Right)
        List<Vector3> faceVerts = isFirstDiagonal 
            ? new List<Vector3> { Block.v6, Block.v4, Block.v2, Block.v0 } 
            : new List<Vector3> { Block.v7, Block.v5, Block.v3, Block.v1 };

        List<Vector3> worldVerts = faceVerts.Select(v => v + position).ToList();
        vertices.AddRange(worldVerts);

        // Side 1 (Forward)
        triangles.AddRange(new List<int>
        {
            0 + vertexIndex, 1 + vertexIndex, 2 + vertexIndex,
            1 + vertexIndex, 3 + vertexIndex, 2 + vertexIndex
        });

        // Side 2 (Backward) - Reversed winding order to render the back face
        triangles.AddRange(new List<int>
        {
            2 + vertexIndex, 1 + vertexIndex, 0 + vertexIndex,
            2 + vertexIndex, 3 + vertexIndex, 1 + vertexIndex
        });

        // Use a specific face index or type for UV mapping
        uvs.AddRange(Block.GetUVs(Block.CubeFace.Front, this));
    }
}