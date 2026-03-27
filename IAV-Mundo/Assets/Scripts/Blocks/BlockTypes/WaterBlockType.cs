using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WaterBlockType : BlockType
{
    public WaterBlockType(Vector2Int uvPos) : base(uvPos, isSolid: false, customMesh: true, defaultTransparency: true)
    {}

    static readonly Vector3 v0 = new Vector3(0.5f, -0.5f, 0.5f);
    static readonly Vector3 v1 = new Vector3(-0.5f, -0.5f, 0.5f);
    static readonly Vector3 v2 = new Vector3(-0.5f, -0.5f, -0.5f);
    static readonly Vector3 v3 = new Vector3(0.5f, -0.5f, -0.5f);
    static readonly Vector3 v4 = new Vector3(0.5f, 0.25f, 0.5f);
    static readonly Vector3 v5 = new Vector3(-0.5f, 0.25f, 0.5f);
    static readonly Vector3 v6 = new Vector3(-0.5f, 0.25f, -0.5f);
    static readonly Vector3 v7 = new(0.5f, 0.25f, -0.5f);
    public static readonly SortedDictionary<Block.CubeFace, List<Vector3>> FaceVerticesMap = 
    new()
    {
        {Block.CubeFace.Front, new List<Vector3>{v4, v5, v0, v1}},
        {Block.CubeFace.Back, new List<Vector3>{v6, v7, v2, v3}},
        {Block.CubeFace.Top, new List<Vector3>{v7, v6, v4, v5}},
        {Block.CubeFace.Bottom, new List<Vector3>{v0, v1, v3, v2}},
        {Block.CubeFace.Left, new List<Vector3>{v5, v6, v1, v2}},
        {Block.CubeFace.Right, new List<Vector3>{v7, v4, v3, v0}},
    };

    //TODO
    public override void AddCustomFaceToMesh(Block.CubeFace face, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, Vector3 position)
    {        
        int vertexIndex = vertices.Count;
        List<Vector3> faceVerts = FaceVerticesMap[face];
        List<Vector3> worldVerts = faceVerts.Select(v => v + position).ToList();
        vertices.AddRange(worldVerts);
        triangles.AddRange(new List<int>
        {
            0 + vertexIndex, 1 + vertexIndex, 2 + vertexIndex, 
            1 + vertexIndex, 3 + vertexIndex, 2 + vertexIndex
        });
        uvs.AddRange(GetUVs(face));
    }
    
    public static readonly float BLOCK_TEX_WIDTH = 16f;
    public static readonly float BLOCK_TEX_HEIGHT = 12f;
    public Vector2[] GetUVs(Block.CubeFace face)
    {
        Vector2 ltc = GetUvTLC(face);

        //até agora, conta-se as coordenadas de cima para baixo. a partir de agora, conta-se de baixo para cima
        //de "y sobe para baixo" para "y sobe para cima"
        //minecraft tende a fazer da primeira maneira (e varios programas de edicao de imagem)
        //mas os UVs contam de baixo para cima
        ltc = new Vector2(ltc.x, Block.TEXTURE_HEIGHT - ltc.y);
        Vector2 uv01 = ltc;
        Vector2 uv00 = ltc + new Vector2(0f, -BLOCK_TEX_HEIGHT); // inferior-esquerdo
        Vector2 uv10 = ltc + new Vector2(BLOCK_TEX_WIDTH, -BLOCK_TEX_HEIGHT); // inferior-direito
        Vector2 uv11 = ltc + new Vector2(BLOCK_TEX_WIDTH, 0f); // superior-direito
        Vector2 adjustVector = new(Block.TEXTURE_WIDTH, Block.TEXTURE_HEIGHT);
        return new[] { uv01/adjustVector, uv11/adjustVector, uv00/adjustVector, uv10/adjustVector };
    }
}