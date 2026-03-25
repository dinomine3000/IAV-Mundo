using System.Collections.Generic;
using UnityEngine;

public class BlockType
{

    public bool isSolid;
    private Dictionary<Block.CubeFace, Vector2Int> uvCoords = new Dictionary<Block.CubeFace, Vector2Int>();

    public BlockType(Vector2Int uvCoords, bool isSolid = true)
    {
        this.uvCoords[Block.CubeFace.ALL] = uvCoords;
        this.isSolid = isSolid;
    }

    public BlockType WithFaceTexture(Block.CubeFace face, Vector2Int uvCoords)
    {
        this.uvCoords[face] = uvCoords;
        return this;
    }    


    public void AddNonSolidFaceToMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {}

    public Vector2Int GetUvTLC(Block.CubeFace face)
    {
        if(uvCoords.ContainsKey(face)) return uvCoords[face];
        else return uvCoords[Block.CubeFace.ALL];
    }

}