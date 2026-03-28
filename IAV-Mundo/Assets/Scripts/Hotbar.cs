using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Hotbar : MonoBehaviour
{
    public static int displaySize = 35;
    private float atlasBlockSize = Block.BLOCK_TEX_HEIGHT;
    public GameObject[] slots;
    public Texture2D textures;
        
    private BlockInteraction blockInteraction;
    private List<BlockType> itemOrder = new List<BlockType>();
    private Color selectedColor = Color.gray2;
    private Color normalColor = Color.white;
    private int currentIdx = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        blockInteraction = GetComponent<BlockInteraction>();
        AddItemToSlot(BlockTypes.STONE);
        AddItemToSlot(BlockTypes.DIRT);
        AddItemToSlot(BlockTypes.DEEPSLATE);
        AddItemToSlot(BlockTypes.GRASS);
        AddItemToSlot(BlockTypes.SANDSTONE);
        AddItemToSlot(BlockTypes.SAND);
        AddItemToSlot(BlockTypes.BEDROCK);
        HighlightSlot(0);
    }

    // Update is called once per frame
    void Update()
    {
        changeSlot();
    }

    public void AddItemToSlot(BlockType type)
    {
        itemOrder.Add(type);

        Vector2 textureCoords = type.GetUvTLC(Block.CubeFace.Front);

        float ajustedY = Block.TEXTURE_HEIGHT - textureCoords.y - atlasBlockSize;
        Rect selectionRect = new Rect(textureCoords.x, ajustedY, atlasBlockSize, atlasBlockSize);
        Sprite itemSprite = Sprite.Create(textures, selectionRect, new Vector2(0.5f, 0.5f), 100.0f);

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].transform.childCount == 0)
            {
                CreateIconInSlot(slots[i], itemSprite);
                return;
            }
        }
    }
    void CreateIconInSlot(GameObject slot, Sprite sprite)
    {
        GameObject icon = new GameObject("ItemIcon");
        icon.transform.SetParent(slot.transform);

        Image img = icon.AddComponent<Image>();
        img.sprite = sprite;

        RectTransform rt = icon.GetComponent<RectTransform>();
        rt.localPosition = Vector3.zero;
        rt.localScale = Vector3.one;

        rt.sizeDelta = new Vector2(displaySize, displaySize);
    }
    void changeSlot()
    {
        for (int i = 0; i < itemOrder.Count; i++)
        {
            if (GetDigitPressed(i + 1))
            {
                if (i < itemOrder.Count)
                {
                    currentIdx = i;
                    blockInteraction.changePlaceType(itemOrder[currentIdx]);
                    HighlightSlot(currentIdx);
                }
            }
        }
        if(Mouse.current.scroll.ReadValue().y > 0)
        {
            currentIdx = (currentIdx + itemOrder.Count - 1) % itemOrder.Count;
            blockInteraction.changePlaceType(itemOrder[currentIdx]);
            HighlightSlot(currentIdx);
            
        } else if (Mouse.current.scroll.ReadValue().y < 0)
        {
            currentIdx = (currentIdx + 1) % itemOrder.Count;
            blockInteraction.changePlaceType(itemOrder[currentIdx]);
            HighlightSlot(currentIdx);
        }
        
    }
    bool GetDigitPressed(int digit)
    {
        switch (digit)
        {
            case 1: return Keyboard.current.digit1Key.wasPressedThisFrame;
            case 2: return Keyboard.current.digit2Key.wasPressedThisFrame;
            case 3: return Keyboard.current.digit3Key.wasPressedThisFrame;
            case 4: return Keyboard.current.digit4Key.wasPressedThisFrame;
            case 5: return Keyboard.current.digit5Key.wasPressedThisFrame;
            case 6: return Keyboard.current.digit6Key.wasPressedThisFrame;
            case 7: return Keyboard.current.digit7Key.wasPressedThisFrame;
            case 8: return Keyboard.current.digit8Key.wasPressedThisFrame;
            default: return false;
        }
    }

    void HighlightSlot(int index)
    {
        foreach (GameObject slot in slots)
        {
            slot.GetComponent<Image>().color = normalColor;
        }
        if (index >= 0 && index < slots.Length)
        {
            slots[index].GetComponent<Image>().color = selectedColor;
        }
    }
}
