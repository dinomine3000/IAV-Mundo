using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public class BlockInteraction : MonoBehaviour
{
    public WorldManager worldManager;
    public float maxDistance = 6f;

    public BlockType placeType = BlockTypes.STONE;

    public Image hotbar;
    private Image slots;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * maxDistance, Color.green);
        if (Mouse.current.leftButton.wasPressedThisFrame)
            BreakBlock();
        if (Mouse.current.rightButton.wasPressedThisFrame)
            PlaceBlock();
        changePlaceType();
    }

    void BreakBlock()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            ModifyBlock(hit.point - hit.normal * 0.5f, BlockTypes.AIR);
        }
    }

    void RedrawNeighbour(Vector2Int coord)
    {
        Chunk c = worldManager.GetChunk(coord);
        if (c != null) c.DrawChunk();
    }

    void PlaceBlock()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            ModifyBlock(hit.point - hit.normal * 0.5f, placeType);
        }
    }

    void ModifyBlock(Vector3 worldPos, BlockType type)
    {
        int cs = Chunk.chunkSize;
        int bx = Mathf.RoundToInt(worldPos.x);
        int by = Mathf.RoundToInt(worldPos.y);
        int bz = Mathf.RoundToInt(worldPos.z);

        Vector2Int chunkCoord = new Vector2Int(
                Mathf.FloorToInt((float)bx / cs),
                Mathf.FloorToInt((float)bz / cs));

        Chunk chunk = worldManager.GetChunk(chunkCoord);
        if (chunk == null) return;

        int localX = bx - chunkCoord.x * cs;
        int localY = by;
        int localZ = bz - chunkCoord.y * cs;

        if (localX < 0 || localX >= cs ||
                localY < 1 || localY >= cs ||
                localZ < 0 || localZ >= cs) return;

        Block block = chunk.chunkData[localX, localY, localZ];
        if ((block.type != BlockTypes.AIR) && type != BlockTypes.AIR) return;
        block.type = type;
        block.isSolid = (type != BlockTypes.AIR);

        chunk.DrawChunk();

        if (localX == 0) RedrawNeighbour(chunkCoord + Vector2Int.left);
        if (localX == cs - 1) RedrawNeighbour(chunkCoord + Vector2Int.right);
        if (localZ == 0) RedrawNeighbour(chunkCoord + Vector2Int.down);
        if (localZ == cs - 1) RedrawNeighbour(chunkCoord + Vector2Int.up);
    }

    void changePlaceType()
    {
        if (Keyboard.current.digit1Key.isPressed) placeType = BlockTypes.STONE;
        if (Keyboard.current.digit2Key.isPressed) placeType = BlockTypes.DIRT;
        if (Keyboard.current.digit3Key.isPressed) placeType = BlockTypes.DEEPSLATE;
    }
}
