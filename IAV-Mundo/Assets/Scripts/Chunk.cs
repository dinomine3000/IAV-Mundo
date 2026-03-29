using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    public const int chunkSize = 16;
    public int chunkHeight = 16;
    public Block[,,] chunkData;
    public int waterLevel = 35;
    public int mountainHeight = 25;
    public int baseTerrainHeight = 7;
    public int beachBreakHeight = 5;
    private int maxSolidHeight = 12;
    public float vegetationThreshold = 0.3f;
    public Material chunkMaterial;
    public Material chunkTransparentMaterial;
    [Header("Cave Config")]
    public float caveScale = 0.1f;
    public float caveMinThreshold = 0.65f;
    public float caveMaxThreshold = 0.35f;
    public int caveSurfaceProtectionMargin = 4;
    private float scale = 0.05f;

    private int octaves = 1;

    private float persistence = 0.5f;

    public WorldManager worldManager;

    public bool isDrawn = false;

    private Dictionary<Vector3Int, BlockType> savedData = null;

    public Vector2Int worldOffset; // coordenada do chunk no mundo (em chunks)
    public void Initialize(Vector2Int offset, Material mat, Material transparentMat, WorldManager manager)
    {
        worldOffset = offset;
        chunkMaterial = mat;
        chunkTransparentMaterial = transparentMat;
        worldManager = manager;
        InitializeChunk(); // usa worldOffset para coordenadas globais
        LoadData();
        //DrawChunk();

    }

    public void SetSavedData(Dictionary<Vector3Int, BlockType> data)
    {
        savedData = data;
    }

    public Dictionary<Vector3Int, BlockType> GetSavedData()
    {
        return savedData;
    }

    public void ChangeBlock(Vector3Int coord, BlockType type)
    {
        if(savedData == null) savedData = new();
        chunkData[coord.x, coord.y, coord.z].type = type;
        savedData[coord] = type;
    }

    public void Setup(float scale, int octaves, float persistence, 
    int maxSolidHeight, int chunkHeight, int waterLevel, int mountainHeight, int baseTerrainHeight, int beachBreakHeight)
    {
        this.octaves = octaves;
        this.scale = scale;
        this.persistence = persistence;
        this.maxSolidHeight = maxSolidHeight;
        this.chunkHeight = chunkHeight;
        this.waterLevel = waterLevel;
        this.mountainHeight = mountainHeight;
        this.baseTerrainHeight = baseTerrainHeight;
        this.beachBreakHeight = beachBreakHeight;
    }

    void InitializeChunk()
    {
        chunkData = new Block[chunkSize, chunkHeight, chunkSize];

        for(int x = 0; x < chunkSize; x++)
        for(int z = 0; z < chunkSize; z++)
        {
            int blockX = x + worldOffset.x*chunkSize; 
            int blockZ = z + worldOffset.y*chunkSize; 
        
            float heightNoise = NoiseUtil.FBm(blockX, blockZ, octaves, scale, persistence);
            float caveOpeningNoise = Mathf.Pow(NoiseUtil.FBm(blockX, blockZ, 2, 0.1f), 1.2f);
            float oceanNoise = Mathf.Pow(NoiseUtil.FBm(blockX, blockZ, 2, 0.005f), 0.5f);
            float mountainNoise = Mathf.Pow(NoiseUtil.FBm(blockX + 525, blockZ - 240, 2, 0.005f), 0.5f);
            float vegetationNoise = NoiseUtil.FBm(blockX, blockZ, 2, 0.5f);

            oceanNoise = (float)math.atan((oceanNoise - 0.65)/0.02f) * (1/math.PI + 0.01f) + 0.5f;
            oceanNoise = math.clamp(oceanNoise, 0, 1);
            mountainNoise = (float)math.atan((mountainNoise - 0.7)/0.01f) * (1/math.PI + 0f) + 0.5f;
            mountainNoise = math.clamp(mountainNoise, 0, 1);

            float terrainHeight = maxSolidHeight 
            + oceanNoise * (baseTerrainHeight * heightNoise + (mountainHeight - baseTerrainHeight)*heightNoise*mountainNoise)
            + (waterLevel - maxSolidHeight + beachBreakHeight) * oceanNoise;

            int surfaceBlockCount = 0;
            for(int y = chunkHeight - 1; y >= 0; y--)
            {
                /*float densityNoise = NoiseUtil.Perlin3D(blockX * densityScale, y * densityScale, blockZ * densityScale) * 2f - 1f;
                float finalDensity = heightNoise - y + densityNoise;
                bool solid = finalDensity > densityThreshold;*/
                bool solid = y < terrainHeight;

                //cave  
                if(solid && y > 1 && surfaceBlockCount >= math.floor(caveSurfaceProtectionMargin*(1-caveOpeningNoise)))
                    {
                        float cx = blockX * caveScale;
                        float cy = y * caveScale;
                        float cz = blockZ * caveScale;

                        float caveNoise = NoiseUtil.Perlin3D(cx, cy, cz);
                        if(caveNoise > caveMinThreshold && caveNoise < caveMaxThreshold)
                        {
                            solid = false;
                        }
                    }

                //block attribution
                BlockType type = BlockTypes.AIR;
                //continua a contar desde que tenha gerado um bloco na superficie
                if(surfaceBlockCount > 0) surfaceBlockCount++;
                if (solid)
                {
                    type = BlockTypes.STONE;  
                    if(surfaceBlockCount == 0)
                    {
                        if(oceanNoise > 0.8)
                        {
                            type = BlockTypes.GRASS;
                            if(vegetationNoise > vegetationThreshold
                                && y < chunkHeight - 1 
                                && y < waterLevel + 10
                                && chunkData[x, y+1, z].type.IsSameBlock(BlockTypes.AIR))
                                {
                                    chunkData[x, y+1, z].type = BlockTypes.TALL_GRASS;
                                } 
                        }
                        else type = BlockTypes.SAND;
                        surfaceBlockCount++;  
                    } 
                    
                    if(y < waterLevel + 15)
                    {
                        
                        //if its right below grass, paint dirt.
                        if(y < chunkHeight - 1 && chunkData[x, y+1, z].type == BlockTypes.GRASS) 
                            type = BlockTypes.DIRT;  
                        if(y < chunkHeight - 2 && chunkData[x, y+2, z].type == BlockTypes.GRASS) 
                            type = BlockTypes.DIRT;   
                            
                        //if its right below sand, paint sandstone.
                        if(y < chunkHeight - 1 && chunkData[x, y+1, z].type == BlockTypes.SAND) 
                            type = BlockTypes.SANDSTONE;  
                        if(y < chunkHeight - 2 && chunkData[x, y+2, z].type == BlockTypes.SAND) 
                            type = BlockTypes.SANDSTONE;   
                        
                    }
                    if(surfaceBlockCount > 16 || y < 3) type = BlockTypes.DEEPSLATE;
                    
                } else if(surfaceBlockCount == 0 && y <= waterLevel && oceanNoise < 0.75)
                {
                    type = BlockTypes.WATER;
                    if(y < chunkHeight - 1 && chunkData[x, y+1, z].IsWater()) 
                        type = BlockTypes.FULL_WATER;  
                }
                if(y == 0) type = BlockTypes.BEDROCK;
                chunkData[x, y, z] = new Block(type, new(x, y, z));
            } 
        }
    }
    private void LoadData()
    {
        if(savedData == null) return;
        foreach(Vector3Int blockPos in savedData.Keys)
        {
            chunkData[blockPos.x, blockPos.y, blockPos.z].type = savedData[blockPos];
        }
    }

    public void DrawChunk()
    {
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();

        for(int x = 0; x < chunkSize; x++)
        for(int y = 0; y < chunkHeight; y++)
        for(int z = 0; z < chunkSize; z++)
        {
            Block block = chunkData[x, y, z];
            
            if(!block.isSolid() && !block.IsWater())
            {
                block.AddNonSolidFaceToMesh(vertices, triangles, uvs);
                continue;   
            }
            
            if(!HasSolidNeighbour(x, y, z + 1, block.IsWater()))
                block.AddSolidFaceToMesh(Block.CubeFace.Front, vertices, triangles, uvs);
                
            if(!HasSolidNeighbour(x, y, z - 1, block.IsWater()))
                block.AddSolidFaceToMesh(Block.CubeFace.Back, vertices, triangles, uvs);
                
            if(!HasSolidNeighbour(x, y + 1, z, block.IsWater()))
                block.AddSolidFaceToMesh(Block.CubeFace.Top, vertices, triangles, uvs);
                
            if(!HasSolidNeighbour(x, y - 1, z, block.IsWater()))
                block.AddSolidFaceToMesh(Block.CubeFace.Bottom, vertices, triangles, uvs);
                
            if(!HasSolidNeighbour(x - 1, y, z, block.IsWater()))
                block.AddSolidFaceToMesh(Block.CubeFace.Left, vertices, triangles, uvs);
                
            if(!HasSolidNeighbour(x + 1, y, z, block.IsWater()))
                block.AddSolidFaceToMesh(Block.CubeFace.Right, vertices, triangles, uvs);
        }

        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            uv = uvs.ToArray()
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        gameObject.GetOrAddComponent<MeshFilter>().mesh = mesh;
        gameObject.GetOrAddComponent<MeshRenderer>().material = chunkMaterial; 
        gameObject.GetOrAddComponent<MeshCollider>().sharedMesh = mesh;
        gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        isDrawn = true;
    }


    /*
    public void DrawChunk()
    {
        // 1 - criar listas partilhadas
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();
        
        List<Vector3> transparentVertices = new();
        List<int> transparentTriangles = new();
        List<Vector2> transparentUvs = new();

        // 2 - para cada bloco, adicionar todas as faces
        for(int x = 0; x < chunkSize; x++)
        for(int y = 0; y < chunkHeight; y++)
        for(int z = 0; z < chunkSize; z++)
        {
            Block block = chunkData[x, y, z];
            if(!block.isSolid() && !block.IsWater())
                {
                    if(block.IsFaceTransparent(Block.CubeFace.ALL)) block.AddNonSolidFaceToMesh(transparentVertices, transparentTriangles, transparentUvs);
                    else block.AddNonSolidFaceToMesh(vertices, triangles, uvs);
                    continue;   
                }
            
            if(!HasSolidNeighbour(x, y, z + 1, block.IsWater()))
            {
                if(block.IsFaceTransparent(Block.CubeFace.Front)) block.AddSolidFaceToMesh(Block.CubeFace.Front, transparentVertices, transparentTriangles, transparentUvs);       
                else block.AddSolidFaceToMesh(Block.CubeFace.Front, vertices, triangles, uvs);
            }
            if(!HasSolidNeighbour(x, y, z - 1, block.IsWater()))
            {
                if(block.IsFaceTransparent(Block.CubeFace.Back)) block.AddSolidFaceToMesh(Block.CubeFace.Back, transparentVertices, transparentTriangles, transparentUvs);       
                else block.AddSolidFaceToMesh(Block.CubeFace.Back, vertices, triangles, uvs);
            }
            if(!HasSolidNeighbour(x, y + 1, z, block.IsWater()))
            {
                if(block.IsFaceTransparent(Block.CubeFace.Top)) block.AddSolidFaceToMesh(Block.CubeFace.Top, transparentVertices, transparentTriangles, transparentUvs);       
                else block.AddSolidFaceToMesh(Block.CubeFace.Top, vertices, triangles, uvs);
            }
            if(!HasSolidNeighbour(x, y - 1, z, block.IsWater()))
            {
                if(block.IsFaceTransparent(Block.CubeFace.Bottom)) block.AddSolidFaceToMesh(Block.CubeFace.Bottom, transparentVertices, transparentTriangles, transparentUvs);       
                else block.AddSolidFaceToMesh(Block.CubeFace.Bottom, vertices, triangles, uvs);
            }
            if(!HasSolidNeighbour(x - 1, y, z, block.IsWater()))
            {
                if(block.IsFaceTransparent(Block.CubeFace.Left)) block.AddSolidFaceToMesh(Block.CubeFace.Left, transparentVertices, transparentTriangles, transparentUvs);       
                else block.AddSolidFaceToMesh(Block.CubeFace.Left, vertices, triangles, uvs);
            }
            if(!HasSolidNeighbour(x + 1, y, z, block.IsWater()))
            {
                if(block.IsFaceTransparent(Block.CubeFace.Right)) block.AddSolidFaceToMesh(Block.CubeFace.Right, transparentVertices, transparentTriangles, transparentUvs);       
                else block.AddSolidFaceToMesh(Block.CubeFace.Right, vertices, triangles, uvs);
            }
        }   

        // 3 - criar mesh e atribuir arrays
        Mesh mesh = new()
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            uv = uvs.ToArray()
        };
        Mesh transparentMesh = new()
        {
            vertices = transparentVertices.ToArray(),
            triangles = transparentTriangles.ToArray(),
            uv = transparentUvs.ToArray()
        };

        // 4 - recalcular normals e bounds
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        transparentMesh.RecalculateNormals();
        transparentMesh.RecalculateBounds();

        // 5 - atribuir ao mesh filter e renderer
        gameObject.GetOrAddComponent<MeshFilter>().mesh = mesh;
        gameObject.GetOrAddComponent<MeshRenderer>().material = chunkMaterial;
        gameObject.GetOrAddComponent<MeshCollider>().sharedMesh = mesh;
        gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        GameObject childMesh;
        if(transform.childCount == 0)
        {
            
            childMesh = new GameObject("TransparentMesh");
            childMesh.transform.SetParent(transform);
            //reset local transform to align with the parent
            childMesh.transform.localPosition = Vector3.zero;
            childMesh.transform.localRotation = Quaternion.identity;
        } else
        {
            childMesh = transform.Find("TransparentMesh").gameObject;
        }
        childMesh.GetOrAddComponent<MeshFilter>().mesh = transparentMesh;
        childMesh.GetOrAddComponent<MeshRenderer>().material = chunkTransparentMaterial;

        isDrawn = true;
    }*/
    
    bool HasSolidNeighbour(int x, int y, int z, bool isWaterCalling)
    {
        if(y > chunkHeight - 1 || y < 0 ) return false;
        Vector2Int offset = Vector2Int.zero;
        if(x > chunkSize - 1) offset = new Vector2Int(1, 0);
        if(x < 0) offset = new Vector2Int(-1, 0);
        if(z > chunkSize - 1) offset = new Vector2Int(0, 1);
        if(z < 0) offset = new Vector2Int(0, -1);
        if(Vector2Int.zero == offset) return chunkData[x, y, z].ObstructsFace(isWaterCalling);

        Chunk neighborChunk = worldManager.GetChunk(worldOffset + offset);
        if(null == neighborChunk) return false;
        int newX = (x + chunkSize) % chunkSize;
        int newZ = (z + chunkSize) % chunkSize;
        return neighborChunk.chunkData[newX, y, newZ].ObstructsFace(isWaterCalling);
    }
}