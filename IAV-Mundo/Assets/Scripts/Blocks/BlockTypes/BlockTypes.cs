using System;
using System.Collections.Generic;

public class BlockTypes
{
    public static Dictionary<string, BlockType> BLOCKS_TYPES = new Dictionary<string, BlockType>();

    public static BlockType AIR = registerBlock("air", new BlockType(new(0, 0), false));

    public static BlockType STONE = registerBlock("stone", new BlockType(new(16, 0)));
    public static BlockType BEDROCK = registerBlock("bedrock", new BlockType(new(16, 16)));
    public static BlockType GRASS = registerBlock("grass",
         new BlockType(new(48, 0))
         .WithFaceTexture(Block.CubeFace.Top, new(128, 32))
         .WithFaceTexture(Block.CubeFace.Bottom, new(32, 0)));
    public static BlockType DIRT = registerBlock("dirt", new BlockType(new(32, 0)));
    public static BlockType DEEPSLATE = registerBlock("deepslate", new BlockType(new(128, 128)));

    public static BlockType registerBlock(String blockId, BlockType type)
    {
        BLOCKS_TYPES.Add(blockId, type);
        return type;
    }

    public static BlockType byId(String blockId)
    {
        if(!BLOCKS_TYPES.ContainsKey(blockId)) return null;
        return BLOCKS_TYPES[blockId];
    }
}