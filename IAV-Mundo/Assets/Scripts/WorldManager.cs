using System;
using System.Collections;
using System.Collections.Generic;
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
    private Vector2Int lastPlayerChunk = new Vector2Int(int.MinValue, int.MinValue);

    void Start()
    {
        lastPlayerChunk = GetPlayerChunk();
        for (int cx = 0; cx < initialGridSize; cx++)
        for (int cz = 0; cz < initialGridSize; cz++)
        {
            SpawnChunk(new Vector2Int(lastPlayerChunk.x + cx, lastPlayerChunk.y + cz));
        }
        for (int cx = 0; cx < initialGridSize; cx++)
        for (int cz = 0; cz < initialGridSize; cz++)
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
            renderRoutine = StartCoroutine(RenderChunks(new(needed)));   
        }
    }
    Vector2Int GetPlayerChunk()
    {
        Vector3 pos = player.position;
        return new Vector2Int(
            Mathf.FloorToInt(pos.x / Chunk.chunkSize),
            Mathf.FloorToInt(pos.z / Chunk.chunkSize));
    }
    HashSet<Vector2Int> GetNeededChunks(Vector2Int currentCenterChunk)
    {
        HashSet<Vector2Int> needed = new HashSet<Vector2Int>();
        Vector2Int center = currentCenterChunk;
        for(int x = center.x - renderDistance; x <= center.x + renderDistance; x++)
        for(int z = center.y - renderDistance; z <= center.y + renderDistance; z++)
        {
            needed.Add(new Vector2Int(x, z));
        }   
        return needed;
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
         
            if (!activeChunks.ContainsKey(coord))
            {
                SpawnChunk(coord);
                check = true;
            }   
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    if (x != 0 && y != 0) continue;

                    Vector2Int adjCoord = new Vector2Int(coord.x + x, coord.y + y);

                    // Check if this specific neighbor is needed and not already active
                    if (chunksNeeded.Contains(adjCoord) && !activeChunks.ContainsKey(adjCoord))
                    {
                        SpawnChunk(adjCoord);
                        check = true;
                    }
                }
            }
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
    
    IEnumerator RenderChunks(List<Vector2Int> chunksNeeded)
    {
        int count = 0;
        foreach (var coord in chunksNeeded)
        {
            Chunk chunk = GetChunk(coord);
            if(chunk == null) continue;
            chunk.DrawChunk();
            
            if(++count % chunksRenderedPerFrame == 0)
            {
                yield return null; //pausa ate o prox frame
            }
        }
    }

    void UpdateChunks()
    {
        // TODO: 1. Construir HashSet<Vector2Int> com os chunks necessarios
        // (todos os (cx,cz) dentro de renderDistance do centro)
        HashSet<Vector2Int> needed = GetNeededChunks(GetPlayerChunk());
        // TODO: 2. Remover chunks que já não são necessarios
        // Atenção: não modificar o Dictionary enquanto se itera!
        // Sugestão: recolher as chaves a remover numa lista separada
        RemoveDistantChunks(GetPlayerChunk());
        // TODO: 3. Spawnar os chunks de 'needed' que ainda não existem
        foreach(Vector2Int chunkPos in needed)
        {
            if(activeChunks.ContainsKey(chunkPos)) continue;
            SpawnChunk(chunkPos);
        }

        //D.4 Desenhar todos os chunks
        foreach(GameObject chunk in activeChunks.Values)
        {
            chunk.GetComponent<Chunk>().DrawChunk();
        }
    }

    void SpawnChunk(Vector2Int coord)
    {
        Vector2Int chunkPos = coord * Chunk.chunkSize;
        GameObject chunkObj = Instantiate(chunkPrefab);
        chunkObj.transform.position = new Vector3(chunkPos.x, 0, chunkPos.y);
        
        Chunk chunk = chunkObj.GetComponent<Chunk>();
        
        chunk.Setup(scale, octaves, persistence, maxSolidHeight, chunkHeight, waterLevel, mountainHeight, baseTerrainHeight, beachBreakHeight);
        chunk.Initialize(new(coord.x, coord.y), chunkMaterial, this);
        // TODO: Registar no Dictionary activeChunks
        activeChunks.Add(coord, chunkObj);
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