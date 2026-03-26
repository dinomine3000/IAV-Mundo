using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockType
{

    public bool isSolid;
    private bool customMesh;
    private bool singleFace;
    private String blockId;
    public bool HasCustomMesh(){return customMesh;}
    public bool HasSingleFace(){return singleFace;}

    protected Dictionary<Block.CubeFace, Vector2Int> uvCoords = new Dictionary<Block.CubeFace, Vector2Int>();
    protected Dictionary<Block.CubeFace, bool> faceTransparency = new Dictionary<Block.CubeFace, bool>();

    public BlockType(Vector2Int uvCoords, bool isSolid = true, bool customMesh = false, bool singleFace = false, bool defaultTransparency = false)
    {
        this.uvCoords[Block.CubeFace.ALL] = uvCoords;
        faceTransparency[Block.CubeFace.ALL] = defaultTransparency;
        this.isSolid = isSolid;
        this.customMesh = customMesh;
        this.singleFace = singleFace;
    }

    public bool IsFaceTransparent(Block.CubeFace face)
    {
        if(faceTransparency.ContainsKey(face)) return faceTransparency[face];
        return faceTransparency[Block.CubeFace.ALL];
    }

    public BlockType WithId(String id)
    {
        blockId = id;
        return this;
    }

    public String GetId(){return blockId;}
    public bool IsSameBlock(String testId){return blockId == testId;}
    public bool IsSameBlock(BlockType testType){return blockId == testType.blockId;}

    public BlockType WithFaceTexture(Block.CubeFace face, Vector2Int uvCoords)
    {
        this.uvCoords[face] = uvCoords;
        return this;
    }    
    public BlockType WithFaceTransparency(Block.CubeFace face, bool transparency)
    {
        faceTransparency[face] = transparency;
        return this;
    }    

    public virtual void AddCustomFaceToMesh(Block.CubeFace face, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, Vector3 position)
    {
        Debug.Log("Warning: Entered method AddCustomFaceToMesh() on super type, but it does nothing");
    }

    public Vector2Int GetUvTLC(Block.CubeFace face)
    {
        if(uvCoords.ContainsKey(face)) return uvCoords[face];
        else return uvCoords[Block.CubeFace.ALL];
    }

}