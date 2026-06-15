using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class Chunk : MonoBehaviour
{
    private const int Width = 32;
    private const int Height = 128;
    private const int Depth = 32;
    public int2 chunkCoordinate;
    private Material[] blockMaterials;
    public ChunkManager chunkManager;
    private BlockType[,,] blocks;
    enum  BlockType
    {
        Empty = 0,
        Bedrock = 1,
        Stone = 2,
        Dirt = 3
    }
    
    void Start()
    {
        blocks = new BlockType[Width, Height, Depth];
        blockMaterials = new Material[System.Enum.GetValues(typeof(BlockType)).Length];
        
        transform.position = new Vector3(chunkCoordinate.x * Width, 0, chunkCoordinate.y * Depth);
        generateChunk();
        
        // i want to use my texture ignore the fact that its sand
        blockMaterials[1] = Resources.Load<Material>("Material/Sand");
        blockMaterials[2] = Resources.Load<Material>("Material/Stone");
        blockMaterials[3] = Resources.Load<Material>("Material/Sand");
        
        for (int i = 1; i < blockMaterials.Length; i++) {
            GameObject child = new GameObject("Block_" + (BlockType)i);
            child.transform.SetParent(this.transform, false); // SetParent(false) keeps the block's local position instead of its world position

            MeshFilter meshFilter = child.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = child.AddComponent<MeshRenderer>();
            
            Mesh mesh = new Mesh();
            meshFilter.mesh = mesh;
            meshRenderer.material = blockMaterials[i];
            BuildBlockMesh(mesh, (BlockType)i);
        }
        
        if (chunkManager != null)
            chunkManager.RebuildNeighbours(chunkCoordinate);
    }

    float GetStoneHeight(int x, int z) {
        float height = Mathf.PerlinNoise(((x + chunkCoordinate.x * Width) * 0.04f), (z + chunkCoordinate.y * Depth) * 0.04f);
        return height * 20 + 5;
    }

    float GetDirtThickness(int x, int z) {
        float height = Mathf.PerlinNoise((((x + chunkCoordinate.x * Width) + 1000) * 0.07f), ((z + chunkCoordinate.y * Depth) * 0.07f));
        return height * 8;
    }
    
    
    void BuildBlockMesh(Mesh mesh, BlockType blockType)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        void AddFace(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int startIndex = vertices.Count;
            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 1));

            triangles.Add(startIndex + 0);
            triangles.Add(startIndex + 1);
            triangles.Add(startIndex + 2);
            
            triangles.Add(startIndex + 0);
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 3);
        }

        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                for (int z = 0; z < Depth; z++)
                {
                    if (blocks[x, y, z] == blockType)
                    {
                        // Front (+z)
                        if (!IsSolid(x, y, z + 1)) {
                            AddFace(new Vector3(0 + x, 0 + y, 1 + z), new Vector3(1 + x, 0 + y, 1 + z), new Vector3(1 + x, 1 + y, 1 + z), new Vector3(0 + x, 1 + y, 1 + z));
                        }
                        // Back (-z)
                        if (!IsSolid(x, y, z - 1)) {
                            AddFace(new Vector3(1 + x, 0 + y, 0 + z), new Vector3(0 + x, 0 + y, 0 + z), new Vector3(0 + x, 1 + y, 0 + z), new Vector3(1 + x, 1 + y, 0 + z));
                        }
                        // Left (-x)
                        if (!IsSolid(x - 1, y, z)) {
                            AddFace(new Vector3(0 + x, 0 + y, 0 + z), new Vector3(0 + x, 0 + y, 1 + z), new Vector3(0 + x, 1 + y, 1 + z), new Vector3(0 + x, 1 + y, 0 + z));
                        }
                        // Right (+x)
                        if (!IsSolid(x + 1, y, z)) {
                            AddFace(new Vector3(1 + x, 0 + y, 1 + z), new Vector3(1 + x, 0 + y, 0 + z), new Vector3(1 + x, 1 + y, 0 + z), new Vector3(1 + x, 1 + y, 1 + z));
                        }
                        // Bottom (-y)
                        if (!IsSolid(x, y - 1, z)) {
                            AddFace(new Vector3(0 + x, 0 + y, 0 + z), new Vector3(1 + x, 0 + y, 0 + z), new Vector3(1 + x, 0 + y, 1 + z), new Vector3(0 + x, 0 + y, 1 + z));
                        }
                        // Top (+y)
                        if (!IsSolid(x, y + 1, z)) {
                            AddFace(new Vector3(0 + x, 1 + y, 1 + z), new Vector3(1 + x, 1 + y, 1 + z), new Vector3(1 + x, 1 + y, 0 + z), new Vector3(0 + x, 1 + y, 0 + z));
                        }
                    }
                }
            }
        }
        
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        
        mesh.RecalculateNormals();
    }

    bool IsSolid(int x, int y, int z) {
        if (y < 0 || y >= Height) return true;
        
        if (x >= 0 && x < Width && z >= 0 && z < Depth) {
            return blocks[x, y, z] != BlockType.Empty;
        }

        int2 neighbourCoordinate = chunkCoordinate;
        int localX = x;
        int localZ = z;

        if (x < 0) {
            neighbourCoordinate += new int2(-1, 0);
            localX = x + Width;
        } else if (x >= Width) {
            neighbourCoordinate += new int2(1, 0);
            localX = x - Width;
        }

        if (z < 0) {
            neighbourCoordinate += new int2(0, -1);
            localZ = z + Depth;
        } else if (z >= Depth) {
            neighbourCoordinate += new int2(0, 1);
            localZ = z - Depth;
        }

        if (chunkManager != null && chunkManager.loadedChunks.TryGetValue(neighbourCoordinate, out Chunk neighbour)) {
            if (neighbour.blocks == null) return true;
            return neighbour.blocks[localX, y, localZ] != BlockType.Empty;
        }
        
        return true;
    }
    
    public void RebuildMesh() {
        for (int i = 1; i < blockMaterials.Length; i++) {
            Transform existing = transform.Find("Block_" + (BlockType)i);
            if (existing != null) {
                Mesh mesh = new Mesh();
                existing.GetComponent<MeshFilter>().mesh = mesh;
                BuildBlockMesh(mesh, (BlockType)i);
            }
        }
    }
    
    void generateChunk() {
        for (int x = 0; x < Width; x++) {
            for (int z = 0; z < Depth; z++) {
                int bedrockHeight = Random.Range(1, 4);
                int stoneHeight = (int)GetStoneHeight(x, z);
                int dirtThickness = (int)GetDirtThickness(x, z);
                for (int y = 0; y < bedrockHeight; y++) {
                    blocks[x, y, z] = BlockType.Bedrock;
                }
                for (int y = bedrockHeight; y < stoneHeight; y++) {
                    blocks[x, y, z] = BlockType.Stone;
                }

                for (int y = stoneHeight; y < (stoneHeight + dirtThickness); y++)
                {
                    blocks[x, y, z] = BlockType.Dirt;
                }
            }
        }
    }
}
