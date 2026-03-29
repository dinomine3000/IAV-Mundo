using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
public class WorldManager : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public GameObject chunkPrefab;
    public Material chunkMaterial;
    [Header("Configuração")]
    public int renderDistance = 3;
    public int chunksGeneratedPerFrame = 2;
    public int chunksRenderedPerFrame = 5;

    [Header("Configuração de geração de Chunks")]
    public int octaves = 6;
    public float persistence = 0.3f;
    public float scale = 0.05f;
    public float densityScale = 0.2f;
    public float densityThreshold = 0.1f;
    public int initialGridSize = 5;
    public int chunkHeight = 32;
    public int maxSolidHeight = 12;
    public int waterLevel = 35;
    public int beachBreakHeight = 5;
    public int baseTerrainHeight = 7;
    public int mountainHeight = 25;
    private Coroutine buildRoutine;
    private Coroutine renderRoutine;
    private Dictionary<Vector2Int, GameObject> activeChunks = new();
    private Dictionary<Vector2Int, Dictionary<Vector3Int, BlockType>> chunkSavedData = new(); 
    private Vector2Int lastPlayerChunk = new Vector2Int(int.MinValue, int.MinValue);

    void Start()
    {
        lastPlayerChunk = GetPlayerChunk();
        float diff = (initialGridSize - 1)/2f;
        int min = -Mathf.FloorToInt(diff);
        int max = Mathf.CeilToInt(diff);

        for (int cx = min; cx < max; cx++)
        for (int cz = min; cz < max; cz++)
        {
            Vector2Int coord = new(lastPlayerChunk.x + cx, lastPlayerChunk.y + cz);
            SpawnChunk(coord);
            GenerateNeighborChunks(coord);
        }
        for (int cx = min; cx < max; cx++)
        for (int cz = min; cz < max; cz++)
        {
            Chunk chunk = GetChunk(new(lastPlayerChunk.x + cx, lastPlayerChunk.y + cz));
            if(chunk == null) continue;
            chunk.DrawChunk();
        }
    }
    void Update()
    {
        Vector2Int current = GetPlayerChunk();
        if(current != lastPlayerChunk)
        {
            lastPlayerChunk = current;
            //UpdateChunks();
            if(buildRoutine != null)
                StopCoroutine(buildRoutine);
            if(renderRoutine != null)
                StopCoroutine(renderRoutine);

            RemoveDistantChunks(current);
            List<Vector2Int> needed = GetNeededChunksNearestFirst(current);

            buildRoutine = StartCoroutine(BuildChunks(needed));
            //BuildAllChunks(needed);
            renderRoutine = StartCoroutine(RenderChunks(needed));   
        }
    }
    Vector2Int GetPlayerChunk()
    {
        Vector3 pos = player.position;
        return new Vector2Int(
            Mathf.FloorToInt(pos.x / Chunk.chunkSize),
            Mathf.FloorToInt(pos.z / Chunk.chunkSize));
    }
    List<Vector2Int> GetNeededChunksNearestFirst(Vector2Int currentCenterChunk)
    {
        List<Vector2Int> needed = new();
        Vector2Int center = currentCenterChunk;
        for(int i = 0; i <= renderDistance; i++)
        {
            for(int x = center.x - i; x <= center.x + i; x++)
            for(int z = center.y - i; z <= center.y + i; z++)
            {
                Vector2Int vec = new(x, z);
                if(!needed.Contains(vec))
                    needed.Add(vec);
            } 
        }  
        return needed;
    }
    void RemoveDistantChunks(Vector2Int currentCenterChunk)
    {
        
        HashSet<Vector2Int> toRemove = new HashSet<Vector2Int>();
        foreach(Vector2Int vec in activeChunks.Keys)
        {
            if(Math.Abs(vec.x - currentCenterChunk.x) > renderDistance*2.5f
                || Math.Abs(vec.y - currentCenterChunk.y) > renderDistance*2.5f ){
                toRemove.Add(vec);
            }
        }
        foreach(Vector2Int vec in toRemove)
        {
            if(GetChunk(vec).GetSavedData() != null)
                chunkSavedData[vec] = GetChunk(vec).GetSavedData();
            Destroy(activeChunks[vec]);
            activeChunks.Remove(vec);
        }   
    }
    
    IEnumerator BuildChunks(List<Vector2Int> chunksNeeded)
    {
        int count = 0;
        bool check = false;
        foreach (var coord in chunksNeeded)
        {
         
            
            if (SpawnChunk(coord)) check = true;
            if(GenerateNeighborChunks(coord)) check = true;
            if (check)
            {
                check = false;
                if(++count % chunksGeneratedPerFrame == 0)
                {
                    yield return null; //pausa ate o prox frame
                }
            }
        }
    }

    bool GenerateNeighborChunks(Vector2Int coord)
    {
        bool check = false;      
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                if (x != 0 && y != 0) continue;

                Vector2Int adjCoord = new(coord.x + x, coord.y + y);

                // Check if this specific neighbor is needed and not already active
                if (SpawnChunk(adjCoord))
                    check = true;
            }
        }
        return check;
    }
    
    IEnumerator RenderChunks(List<Vector2Int> chunksNeeded)
    {
        int count = 0;
        foreach (var coord in chunksNeeded)
        {
            Chunk chunk = GetChunk(coord);
            count++;
            if(chunk != null && !chunk.isDrawn)
                chunk.DrawChunk();
            
            if(++count % chunksRenderedPerFrame == 0)
            {
                yield return null; //pausa ate o prox frame
            }
        }
    }

    void BuildAllChunks(List<Vector2Int> chunksNeeded)
    {
        foreach (var coord in chunksNeeded)
        {
            SpawnChunk(coord);   
        }
    }

    //returns true if it generated the chunk, false otherwise
    bool SpawnChunk(Vector2Int coord)
    {
        if(activeChunks.ContainsKey(coord)) return false;
        Vector2Int chunkPos = coord * Chunk.chunkSize;
        GameObject chunkObj = Instantiate(chunkPrefab);
        chunkObj.transform.position = new Vector3(chunkPos.x, 0, chunkPos.y);
        
        Chunk chunk = chunkObj.GetComponent<Chunk>();
        
        chunk.Setup(scale, octaves, persistence, maxSolidHeight, chunkHeight, waterLevel, mountainHeight, baseTerrainHeight, beachBreakHeight);
        if(chunkSavedData.ContainsKey(coord))
            chunk.SetSavedData(chunkSavedData[coord]);
        chunk.Initialize(new(coord.x, coord.y), chunkMaterial, this);
        activeChunks.Add(coord, chunkObj);
        return true;
    }

    public Chunk GetChunk(Vector2Int chunkPos)
    {
        if(activeChunks.TryGetValue(chunkPos, out GameObject go))
        {
            return go.GetComponent<Chunk>();
        }
        return null;
    }
}