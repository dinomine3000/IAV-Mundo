using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using NUnit.Compatibility;

public class Block
{

    //vertices
    //inverto o eixo do X comparado com o guia para manter o padrão:
    //X positivo -> direita; X negativo -> Esquerda
    //O Unity inverte este eixo (quando comparado com os guias)
    //Desta maneira, posso referenciar os vértices como na aula 1
    //Quando comparando com a scripts cubeGen.cs, resulta que estes são os valores que usei para cada vértice na aula 1
    public static readonly Vector3 v0 = new Vector3(0.5f, -0.5f, 0.5f);
    public static readonly Vector3 v1 = new Vector3(-0.5f, -0.5f, 0.5f);
    public static readonly Vector3 v2 = new Vector3(-0.5f, -0.5f, -0.5f);
    public static readonly Vector3 v3 = new Vector3(0.5f, -0.5f, -0.5f);
    public static readonly Vector3 v4 = new Vector3(0.5f, 0.5f, 0.5f);
    public static readonly Vector3 v5 = new Vector3(-0.5f, 0.5f, 0.5f);
    public static readonly Vector3 v6 = new Vector3(-0.5f, 0.5f, -0.5f);
    public static readonly Vector3 v7 = new(0.5f, 0.5f, -0.5f);

    //outra vez, a ordem destes vértices tá diferente dos guias/das aulas, mas mais abaixo
    //quando defino a ordem dos vértices, também difere para ajustar.
    //a ordem dos vértices segue a ordem de leitura, esquerda para a direita e cima para baixo, 
    //enquanto que os guias seguem ordem counterclockwise.
    public static readonly SortedDictionary<CubeFace, List<Vector3>> FaceVerticesMap = 
    new()
    {
        {CubeFace.Front, new List<Vector3>{v4, v5, v0, v1}},
        {CubeFace.Back, new List<Vector3>{v6, v7, v2, v3}},
        {CubeFace.Top, new List<Vector3>{v7, v6, v4, v5}},
        {CubeFace.Bottom, new List<Vector3>{v0, v1, v3, v2}},
        {CubeFace.Left, new List<Vector3>{v5, v6, v1, v2}},
        {CubeFace.Right, new List<Vector3>{v7, v4, v3, v0}},
    };


    public enum CubeFace {Front, Back, Top, Bottom, Left, Right, ALL}
    //public enum BlockType { AIR, GRASS, DIRT, STONE, DEEPSLATE, BEDROCK }

    public Vector3 position;

    public BlockType type;
    public Block(BlockType type, Vector3 position)
    {
        this.position = position;
        this.type = type;
    }

    public bool isSolid(){return type.isSolid;}

    public bool ObstructsFace(bool isWaterCalling){return (!IsWater() && isSolid()) || (IsWater() && isWaterCalling);}

    public bool IsWater(){return type.IsSameBlock(BlockTypes.WATER) || type.IsSameBlock(BlockTypes.FULL_WATER);}

    public void AddNonSolidFaceToMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        if(type.HasSingleFace())
            type.AddCustomFaceToMesh(CubeFace.ALL, vertices, triangles, uvs, position);
    }

    public void AddSolidFaceToMesh(CubeFace face,
        List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        if (type.HasCustomMesh())
        {
            type.AddCustomFaceToMesh(face, vertices, triangles, uvs, position);
            return;
        }
        int vertexIndex = vertices.Count;
        List<Vector3> faceVerts = FaceVerticesMap[face];
        List<Vector3> worldVerts = faceVerts.Select(v => v + position).ToList();
        vertices.AddRange(worldVerts);
        triangles.AddRange(new List<int>
        {
            0 + vertexIndex, 1 + vertexIndex, 2 + vertexIndex, 
            1 + vertexIndex, 3 + vertexIndex, 2 + vertexIndex
        });
        //uvs.AddRange(new List<Vector2>{new(0, 1), new(1, 1), new(0, 0), new(1, 0)});
        uvs.AddRange(GetUVs(face, type));
    }

    public static readonly int TEXTURE_WIDTH = 256;
    public static readonly int TEXTURE_HEIGHT = 256;
    public static readonly float BLOCK_TEX_WIDTH = 16f;
    public static readonly float BLOCK_TEX_HEIGHT = 16f;
    public static Vector2[] GetUVs(CubeFace face, BlockType type)
    {
        Vector2 ltc = type.GetUvTLC(face);
        //até agora, conta-se as coordenadas de cima para baixo. a partir de agora, conta-se de baixo para cima
        //de "y sobe para baixo" para "y sobe para cima"
        //minecraft tende a fazer da primeira maneira (e varios programas de edicao de imagem)
        //mas os UVs contam de baixo para cima
        ltc = new Vector2(ltc.x, TEXTURE_HEIGHT - ltc.y);
        Vector2 uv01 = ltc;
        Vector2 uv00 = ltc + new Vector2(0f, -BLOCK_TEX_HEIGHT); // inferior-esquerdo
        Vector2 uv10 = ltc + new Vector2(BLOCK_TEX_WIDTH, -BLOCK_TEX_HEIGHT); // inferior-direito
        Vector2 uv11 = ltc + new Vector2(BLOCK_TEX_WIDTH, 0f); // superior-direito
        Vector2 adjustVector = new(TEXTURE_WIDTH, TEXTURE_HEIGHT);
        return new[] { uv01/adjustVector, uv11/adjustVector, uv00/adjustVector, uv10/adjustVector };
    }
}
