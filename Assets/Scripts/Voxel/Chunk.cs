using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    private const int Width = 32;
    private const int Height = 128;
    private const int Depth = 32;
    public int2 chunkCoordinate;
    private Material blockMaterial;

    private BlockType[,,] blocks;
    enum  BlockType
    {
        Empty = 0,
        Bedrock = 1
    }
    
    void Start()
    {
        blocks = new BlockType[Width, Height, Depth];
        blocks[0, 0, 0] = BlockType.Bedrock;
        blocks[2, 0, 3] = BlockType.Bedrock;
        blocks[5, 0, 1] = BlockType.Bedrock;

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;
        
        // i want to use my texture ignore the fact that its sand
         if (blockMaterial == null)
             blockMaterial = Resources.Load<Material>("Material/Sand");
         
        meshRenderer.material = blockMaterial;
        BuildBlockMesh(mesh);
    }

    void BuildBlockMesh(Mesh mesh)
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
                    if (blocks[x, y, z] == BlockType.Bedrock)
                    {
                        AddFace(new Vector3(0 + x, 0 + y, 1 + z), new Vector3(1 + x, 0 + y, 1 + z), new Vector3(1 + x, 1 + y, 1 + z), new Vector3(0 + x, 1 + y, 1 + z));
                        AddFace(new Vector3(1 + x, 0 + y, 0 + z), new Vector3(0 + x, 0 + y, 0 + z), new Vector3(0 + x, 1 + y, 0 + z), new Vector3(1 + x, 1 + y, 0 + z));
                        AddFace(new Vector3(0 + x, 0 + y, 0 + z), new Vector3(0 + x, 0 + y, 1 + z), new Vector3(0 + x, 1 + y, 1 + z), new Vector3(0 + x, 1 + y, 0 + z));
                        AddFace(new Vector3(1 + x, 0 + y, 1 + z), new Vector3(1 + x, 0 + y, 0 + z), new Vector3(1 + x, 1 + y, 0 + z), new Vector3(1 + x, 1 + y, 1 + z));
                        AddFace(new Vector3(0 + x, 0 + y, 0 + z), new Vector3(1 + x, 0 + y, 0 + z), new Vector3(1 + x, 0 + y, 1 + z), new Vector3(0 + x, 0 + y, 1 + z));
                        AddFace(new Vector3(0 + x, 1 + y, 1 + z), new Vector3(1 + x, 1 + y, 1 + z), new Vector3(1 + x, 1 + y, 0 + z), new Vector3(0 + x, 1 + y, 0 + z));
                    }
                }
            }
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        
        mesh.RecalculateNormals();
    }
}
