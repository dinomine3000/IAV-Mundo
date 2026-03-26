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
    public static BlockType SANDSTONE = registerBlock("sandstone", 
    new BlockType(new(0, 192))
    .WithFaceTexture(Block.CubeFace.Top, new(0, 176))
    .WithFaceTexture(Block.CubeFace.Bottom, new(0, 208)));
    public static BlockType SAND = registerBlock("sand", new BlockType(new(0, 176)));
    public static BlockType DEEPSLATE = registerBlock("deepslate", new BlockType(new(128, 128)));
    public static BlockType WATER = registerBlock("water", new WaterBlockType(new(208, 192)));
    public static BlockType FULL_WATER = registerBlock("full_water", new BlockType(new(208, 192), defaultTransparency: true));
    public static BlockType TALL_GRASS = registerBlock("tall_grass", new TallGrassBlockType(new(112, 32)));
    public static BlockType registerBlock(string blockId, BlockType type)
    { 
        type.WithId(blockId);
        BLOCKS_TYPES.Add(blockId, type);
        return type;
    }

    public static BlockType byId(String blockId)
    {
        if(!BLOCKS_TYPES.ContainsKey(blockId)) return null;
        return BLOCKS_TYPES[blockId];
    }
}