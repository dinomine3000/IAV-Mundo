using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    public const int chunkSize = 16;
    public Block[,,] chunkData;
    public Material chunkMaterial;
    [Header("Cave Config")]
    public float caveScale = 0.1f;
    public float caveThreshold = 0.65f;
    public int caveSurfaceProtectionMargin = 4;
    [Header("Cave Worm Config")]

    public int wormSteps = 5;
    public float wormRadius = 4;
    public float wormStepSize = 2f;
    public float wormDirectionScale = 2f;

    private float scale = 0.05f;

    private int octaves = 1;

    private float persistence = 0.5f;

    private float densityScale = 0.1f;
    private float densityThreshold = 0f;
    private int maxSolidHeight = 12;

    public WorldManager worldManager;

    public Vector2Int worldOffset; // coordenada do chunk no mundo (em chunks)
    public void Initialize(Vector2Int offset, Material mat, WorldManager manager)
    {
        worldOffset = offset;
        chunkMaterial = mat;
        worldManager = manager;
        InitializeChunk(); // usa worldOffset para coordenadas globais
        //DrawChunk();

    }

    public void Setup(float scale, int octaves, float persistence, float densityScale, float densityThreshold, int maxSolidHeight)
    {
        this.octaves = octaves;
        this.scale = scale;
        this.persistence = persistence;
        this.densityScale = densityScale;
        this.densityThreshold = densityThreshold;
        this.maxSolidHeight = maxSolidHeight;
    }

    public void CarveChunk()
    {
        /*int caveLookupMargin = 1;
        for(int x = -caveLookupMargin*chunkSize; x < (1 + caveLookupMargin)*chunkSize; x++)
        for(int z = -caveLookupMargin*chunkSize; z < (1 + caveLookupMargin)*chunkSize; z++)
        {
            int blockX = x + worldOffset.x*chunkSize; 
            int blockZ = z + worldOffset.y*chunkSize; 
            for(int y = maxSolidHeight; y > 1; y--)
            {
                //cave gen
                //if(solid y > 1 && y < maxSolidHeight - caveSurfaceProtectionMargin)
                float cx = blockX * caveScale;
                float cy = y * caveScale;
                float cz = blockZ * caveScale;

                float caveNoise = NoiseUtil.Perlin3D(cx, cy, cz);
                if(caveNoise > caveThreshold)
                {
                    CarveWorm(this, chunkSize, worldOffset, new(blockX, y, blockZ),
                        wormSteps, wormRadius, wormStepSize, wormDirectionScale);
                }
            } 
        }*/
        
        //cave gen
        for(int x = 0; x < chunkSize; x++)
        for(int z = 0; z < chunkSize; z++)
        {
            int blockX = x + worldOffset.x*chunkSize; 
            int blockZ = z + worldOffset.y*chunkSize; 
            for(int y = maxSolidHeight - caveSurfaceProtectionMargin; y > 1; y--)
            {
                if(!chunkData[x, y, z].isSolid) continue;
                //if(solid y > 1 && y < maxSolidHeight - caveSurfaceProtectionMargin)
                float cx = blockX * caveScale;
                float cy = y * caveScale;
                float cz = blockZ * caveScale;

                float caveNoise = NoiseUtil.Perlin3D(cx, cy, cz);
                if(caveNoise > caveThreshold)
                {
                    chunkData[x, y, z].type = BlockTypes.AIR;
                    chunkData[x, y, z].isSolid = false;
                }
            } 
        }

    }
    public void PaintChunk()
    {
        
        for(int x = 0; x < chunkSize; x++)
        for(int z = 0; z < chunkSize; z++)
        {
            int surfaceBlockCount = 0;
            for(int y = chunkSize - 1; y >= 0; y--)
            {
                bool solid = chunkData[x, y, z].isSolid;

                //block attribution
                BlockType type = BlockTypes.AIR;
                //continua a contar desde que tenha gerado um bloco na superficie
                if(surfaceBlockCount > 0) surfaceBlockCount++;
                if (solid)
                {
                    type = BlockTypes.STONE;  
                    if(surfaceBlockCount == 0)
                    {
                        if(y > 6) type = BlockTypes.GRASS;
                        surfaceBlockCount++;  
                    } 
                    
                    //if its right below grass, paint dirt.
                    if(y < chunkSize - 1 && chunkData[x, y+1, z].type == BlockTypes.GRASS) 
                        type = BlockTypes.DIRT;   
                    
                    if(surfaceBlockCount > 7 || y < 3) type = BlockTypes.DEEPSLATE;
                    
                } 
                if(y == 0) type = BlockTypes.BEDROCK;
                /*if(y < h) type = Block.BlockType.STONE;
                if (y == h - 1)
                    type = Block.BlockType.GRASS;
                else if (y > h - 3)
                    type = Block.BlockType.DIRT;
                if(h - y > 6)
                    type = Block.BlockType.DEEPSLATE;
                if(y == 0)
                    type = Block.BlockType.BEDROCK;
                if(y >= h || !solid)
                    type = Block.BlockType.AIR;*/
                chunkData[x, y, z].type = type;
            } 
        }
    }

    void InitializeChunk()
    {
        chunkData = new Block[chunkSize, chunkSize, chunkSize];

        for(int x = 0; x < chunkSize; x++)
        for(int z = 0; z < chunkSize; z++)
        {
            int blockX = x + worldOffset.x*chunkSize; 
            int blockZ = z + worldOffset.y*chunkSize; 
            
            int symmetryMitigation = 16;
            float noise = 0.5f * NoiseUtil.FBm(blockX, blockZ, octaves, scale, persistence) 
                            + 0.5f * NoiseUtil.FBm(blockX + symmetryMitigation, blockZ + symmetryMitigation, octaves, scale, persistence);
            float heightNoise = Mathf.Pow(noise , 0.5f) * chunkSize;
            
            for(int y = chunkSize - 1; y >= 0; y--)
            {
                float densityNoise = NoiseUtil.Perlin3D(blockX * densityScale, y * densityScale, blockZ * densityScale) * 2f - 1f;
                float finalDensity = heightNoise - y + densityNoise;
                bool solid = finalDensity > densityThreshold;

                //block attribution
                BlockType type = BlockTypes.AIR;
                if (solid) type = BlockTypes.STONE;
                chunkData[x, y, z] = new Block(type, new Vector3(x, y, z));
            } 
        }
    }
    public static void CarveWorm(Chunk centerChunk, int chunkSize,
                                    Vector2Int worldOffset,
                                    Vector3 start, int steps, float radius,
                                    float stepSize, float directionScale)
    {
        Vector3 pos = start;
        for (int i = 0; i < steps; i++)
        {
            // Direcção determinada por noise
            float nx = NoiseUtil.Perlin3D(pos.x * directionScale,
                                            pos.y * directionScale,
                                            pos.z * directionScale) * 2f - 1f;
            float ny = NoiseUtil.Perlin3D(pos.y * directionScale + 100f,
                                            pos.z * directionScale + 100f,
                                            pos.x * directionScale + 100f) * 2f - 1f;
            float nz = NoiseUtil.Perlin3D(pos.z * directionScale + 200f,
                                            pos.x * directionScale + 200f,
                                            pos.y * directionScale + 200f) * 2f - 1f;
            Vector3 dir = new Vector3(nx, ny * 0.5f, nz).normalized;
            pos += dir * stepSize;
            // Escavar esfera à volta da posição actual
            CarveAt(centerChunk, chunkSize, worldOffset, pos, radius);
        }
    }
    static void CarveAt(Chunk centerChunk, int chunkSize,
                        Vector2Int worldOffset, Vector3 center, float radius)
    {
        int localX = Mathf.RoundToInt(center.x) - worldOffset.x * chunkSize;
        int localY = Mathf.RoundToInt(center.y);
        int localZ = Mathf.RoundToInt(center.z) - worldOffset.y * chunkSize;
        int r = Mathf.CeilToInt(radius);
        for (int dx = -r; dx <= r; dx++)
        for (int dy = -r; dy <= r; dy++)
        for (int dz = -r; dz <= r; dz++)
        {
            if (dx * dx + dy * dy + dz * dz > radius * radius) continue;
            int bx = localX + dx;
            int by = localY + dy;
            int bz = localZ + dz;
            if (bx >= 0 && bx < chunkSize 
                && by > 1 && by < chunkSize 
                && bz >= 0 && bz < chunkSize)
            {
                centerChunk.chunkData[bx, by, bz].type = BlockTypes.AIR;
            }
        }
    }


    public void DrawChunk()
    {
        // 1 - criar listas partilhadas
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();

        // 2 - para cada bloco, adicionar todas as faces
        for(int x = 0; x < chunkSize; x++)
        for(int y = 0; y < chunkSize; y++)
        for(int z = 0; z < chunkSize; z++)
        {
            Block block = chunkData[x, y, z];
            if(!block.isSolid)
                {
                    block.AddNonSolidFaceToMesh(vertices, triangles, uvs);
                    continue;   
                }
            
            if(!HasSolidNeighbour(x, y, z + 1))
                block.AddSolidFaceToMesh(Block.CubeFace.Front, vertices, triangles, uvs);
            if(!HasSolidNeighbour(x, y, z - 1))
                block.AddSolidFaceToMesh(Block.CubeFace.Back, vertices, triangles, uvs);
            if(!HasSolidNeighbour(x, y + 1, z))
                block.AddSolidFaceToMesh(Block.CubeFace.Top, vertices, triangles, uvs);
            if(!HasSolidNeighbour(x, y - 1, z))
                block.AddSolidFaceToMesh(Block.CubeFace.Bottom, vertices, triangles, uvs);
            if(!HasSolidNeighbour(x - 1, y, z))
                block.AddSolidFaceToMesh(Block.CubeFace.Left, vertices, triangles, uvs);
            if(!HasSolidNeighbour(x + 1, y, z))
                block.AddSolidFaceToMesh(Block.CubeFace.Right, vertices, triangles, uvs);
        }   

        // 3 - criar mesh e atribuir arrays
        Mesh mesh = new()
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            uv = uvs.ToArray()
        };

        // 4 - recalcular normals e bounds
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // 5 - atribuir ao mesh filter e renderer
        gameObject.GetOrAddComponent<MeshFilter>().mesh = mesh;
        gameObject.GetOrAddComponent<MeshRenderer>().material = chunkMaterial;
        gameObject.GetOrAddComponent<MeshCollider>().sharedMesh = mesh;
        gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }
    
    bool HasSolidNeighbour(int x, int y, int z)
    {
        if(y > chunkSize - 1 || y < 0 ) return false;
        Vector2Int offset = Vector2Int.zero;
        if(x > chunkSize - 1) offset = new Vector2Int(1, 0);
        if(x < 0) offset = new Vector2Int(-1, 0);
        if(z > chunkSize - 1) offset = new Vector2Int(0, 1);
        if(z < 0) offset = new Vector2Int(0, -1);
        if(Vector2Int.zero == offset) return chunkData[x, y, z].isSolid;

        Chunk neighborChunk = worldManager.GetChunk(worldOffset + offset);
        if(null == neighborChunk) return false;
        int newX = (x + chunkSize) % chunkSize;
        int newZ = (z + chunkSize) % chunkSize;
        return neighborChunk.chunkData[newX, y, newZ].isSolid;
    }
}