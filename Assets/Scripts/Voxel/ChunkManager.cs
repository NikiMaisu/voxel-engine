using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

public class ChunkManager : MonoBehaviour
{
    private const int ChunkWidth = 32;
    private const int ChunkDepth = 32;
    private const int MaxChunks = 100;
    private const int renderDistance = 16;
    private const int innerZoneRadius = 3;
    public Dictionary<int2, Chunk> loadedChunks;
    void Start()
    { 
        loadedChunks = new Dictionary<int2, Chunk>();
    }

void Update()
{
    int2 cameraChunk = new int2(
        Mathf.FloorToInt(Camera.main.transform.position.x / ChunkWidth),
        Mathf.FloorToInt(Camera.main.transform.position.z / ChunkDepth)
    );

    float highestUnloadedPriority = float.MinValue;
    int2 bestUnloadedCoordinate = default;

    float lowestLoadedPriority = float.MaxValue;
    int2 worstLoadedCoordinate = default;

    for (int x = cameraChunk.x - renderDistance; x <= cameraChunk.x + renderDistance; x++) {
        for (int z = cameraChunk.y - renderDistance; z <= cameraChunk.y + renderDistance; z++)
        {
            int2 candidateCoordinate = new int2(x, z);
            Vector3 chunkWorldPosition = new Vector3(candidateCoordinate.x * ChunkWidth, 0, candidateCoordinate.y * ChunkDepth);
            
            float distance = Vector3.Distance(Camera.main.transform.position, chunkWorldPosition);
            distance = Mathf.Max(distance, 0.1f);

            Vector3 directionToChunk = (chunkWorldPosition - Camera.main.transform.position);
            directionToChunk.y = 0;
            directionToChunk = directionToChunk.normalized;

            Vector3 cameraForwardFlat = Camera.main.transform.forward;
            cameraForwardFlat.y = 0;
            cameraForwardFlat = cameraForwardFlat.normalized;

            float dot = Vector3.Dot(cameraForwardFlat, directionToChunk);
            float priority = (1f / distance) * dot;

            
            // i want this so there is always a circle around you loaded regardless of where youre faing, fixes issue of chunk under u vanishing
            if (distance < ChunkWidth * innerZoneRadius)
            {
                priority = 1f / distance;
            }

            if (!loadedChunks.ContainsKey(candidateCoordinate))
            {
                if (priority > highestUnloadedPriority)
                {
                    highestUnloadedPriority = priority;
                    bestUnloadedCoordinate = candidateCoordinate;
                }
            }
            else
            {
                if (priority < lowestLoadedPriority)
                {
                    lowestLoadedPriority = priority;
                    worstLoadedCoordinate = candidateCoordinate;
                }
            }
        }
    }

    if (loadedChunks.Count < MaxChunks)
    {
        LoadChunk(bestUnloadedCoordinate);
    }
    else if (highestUnloadedPriority > lowestLoadedPriority)
    {
        UnloadChunk(worstLoadedCoordinate);
        LoadChunk(bestUnloadedCoordinate);
    }
}

    void LoadChunk(int2 coordinate) {
        GameObject chunkObject = new GameObject("Chunk" + coordinate.x + "_" + coordinate.y);
        Chunk chunk = chunkObject.AddComponent<Chunk>();
        chunk.chunkCoordinate = coordinate;
        chunk.chunkManager = this;
        loadedChunks.Add(coordinate, chunk); 
    }
    
    public void RebuildNeighbours(int2 coordinate) {
        int2[] neighbours = {
            new int2(coordinate.x + 1, coordinate.y),
            new int2(coordinate.x - 1, coordinate.y),
            new int2(coordinate.x, coordinate.y + 1),
            new int2(coordinate.x, coordinate.y - 1)
        };

        foreach (int2 neighbourCoord in neighbours) {
            if (loadedChunks.TryGetValue(neighbourCoord, out Chunk neighbour)) {
                neighbour.RebuildMesh();
            }
        }
    }

    void UnloadChunk(int2 coordinate) {
        Destroy(loadedChunks[coordinate].gameObject);
        loadedChunks.Remove(coordinate);
    }
}
