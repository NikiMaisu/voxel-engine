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
        Dirt = 3,
        Grass = 4,
        Obsidian = 5
    }
    
    void Start()
    {
        blocks = new BlockType[Width, Height, Depth];
        blockMaterials = new Material[System.Enum.GetValues(typeof(BlockType)).Length];
        
        transform.position = new Vector3(chunkCoordinate.x * Width, 0, chunkCoordinate.y * Depth);
        generateChunk();
        
        blockMaterials[1] = Resources.Load<Material>("Material/Sand");
        blockMaterials[2] = Resources.Load<Material>("Material/Stone");
        blockMaterials[3] = Resources.Load<Material>("Material/Dirt");
        blockMaterials[4] = Resources.Load<Material>("Material/Grass");
        blockMaterials[5] = Resources.Load<Material>("Material/Obsidian");
        
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
        
        GameObject waterChild = new GameObject("Water");
        waterChild.transform.SetParent(this.transform, false);
        MeshFilter waterMeshFilter = waterChild.AddComponent<MeshFilter>();
        MeshRenderer waterMeshRenderer = waterChild.AddComponent<MeshRenderer>();
        Mesh waterMesh = generateTessellatedPlane(16, 16);
        waterMeshFilter.mesh = waterMesh;
        waterChild.transform.localPosition = new Vector3(0, 11.75f, 0);
        waterMeshRenderer.material = Resources.Load<Material>("Material/Water");
        
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
        if (blockMaterials == null) return;
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
                int dirtTop = stoneHeight + dirtThickness - 1;
                if (dirtThickness > 0 && dirtTop > 13)
                {
                    blocks[x, dirtTop, z] = BlockType.Grass;
                }
            }
        }
        
        if (chunkCoordinate.x % 10 == 0 && chunkCoordinate.y % 10 == 0)
        {
            int surfaceHeight = (int)GetStoneHeight(14, 14) + (int)GetDirtThickness(14, 14);
            GeneratePortal(14, surfaceHeight + 1, 14);
        }
    }

    void GeneratePortal(int baseX, int baseY, int baseZ)
    {
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                bool isFrame = x == 0 || x == 3 || y == 0 || y == 4;
                if (isFrame)
                {
                    blocks[baseX + x, baseY + y, baseZ] = BlockType.Obsidian;
                }
            }
        }
        
        GameObject portalFX = new GameObject("PortalFX");
        portalFX.transform.SetParent(this.transform, false);
        portalFX.transform.localPosition = new Vector3(baseX + 0.75f, baseY + 1, baseZ + 0.5f);
        portalFX.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        portalFX.transform.localScale = new Vector3(2.75f/32f, 2.25f/32f, 3.25f/32f);

        MeshFilter pf = portalFX.AddComponent<MeshFilter>();
        MeshRenderer pr = portalFX.AddComponent<MeshRenderer>();
        pf.mesh = generateTessellatedPlane(10, 15);
        pr.material = Resources.Load<Material>("Material/Portal");
    }

    Mesh generateTessellatedPlane(int xSquares, int ySquares)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int j = 0; j <= ySquares; j++)
        {
            for (int i = 0; i <= xSquares; i++)
            {
                float x = (float)i / xSquares * Width;
                float z = (float)j / ySquares * Depth;
                vertices.Add(new Vector3(x, 0, z));
                uvs.Add(new Vector2((float)i / xSquares, (float)j / ySquares));
            }
        }

        for (int j = 0; j <= ySquares - 1; j++)
        {
            for (int i = 0; i <= xSquares - 1; i++)
            {
                int bl = j * (xSquares + 1) + i;
                int br = j * (xSquares + 1) + i + 1;
                int tl = (j + 1) * (xSquares + 1) + i;
                int tr = (j + 1) * (xSquares + 1) + i + 1;

                triangles.Add(bl);
                triangles.Add(tl);
                triangles.Add(br);
                
                triangles.Add(tl);
                triangles.Add(tr);
                triangles.Add(br);
            }
        }
        
        Mesh mesh = new Mesh();
        
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        
        mesh.RecalculateNormals();

        return mesh;
    }
    
}
