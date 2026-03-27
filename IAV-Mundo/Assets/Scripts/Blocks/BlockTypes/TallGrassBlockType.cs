using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TallGrassBlockType : BlockType
{
    public TallGrassBlockType(Vector2Int uvCoords) : base(uvCoords, false, true, true, defaultTransparency: true)
    {}


    //TODO
    public override void AddCustomFaceToMesh(Block.CubeFace face, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, Vector3 position)
    {        
        if(face != Block.CubeFace.ALL) return;
        /*int vertexIndex = vertices.Count;
        List<Vector3> faceVerts = Block.FaceVerticesMap[face];
        List<Vector3> worldVerts = faceVerts.Select(v => v + position).ToList();
        vertices.AddRange(worldVerts);
        triangles.AddRange(new List<int>
        {
            0 + vertexIndex, 1 + vertexIndex, 2 + vertexIndex, 
            1 + vertexIndex, 3 + vertexIndex, 2 + vertexIndex
        });
        uvs.AddRange(Block.GetUVs(face, this));*/
        AddCrossQuadToMesh(Block.CubeFace.Front, vertices, triangles, uvs, position);
        //AddCrossQuadToMesh(Block.CubeFace.Back, vertices, triangles, uvs, position);
        AddCrossQuadToMesh(Block.CubeFace.Left, vertices, triangles, uvs, position);
        //AddCrossQuadToMesh(Block.CubeFace.Right, vertices, triangles, uvs, position);
    }

    private void AddCrossQuadToMesh(Block.CubeFace face, 
        List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, Vector3 position)
    {
        if(face == Block.CubeFace.ALL || face == Block.CubeFace.Top || face == Block.CubeFace.Bottom) return;
        int vertexIndex = vertices.Count;

        // First Diagonal: v6 (Top-Left) to v4 (Top-Right) / v2 (Bottom-Left) to v0 (Bottom-Right)
        // Second Diagonal: v7 (Top-Left) to v5 (Top-Right) / v3 (Bottom-Left) to v1 (Bottom-Right)
        List<Vector3> faceVerts = 
            face == Block.CubeFace.Front || face == Block.CubeFace.Back 
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
        uvs.AddRange(Block.GetUVs(face, this));
    }
}