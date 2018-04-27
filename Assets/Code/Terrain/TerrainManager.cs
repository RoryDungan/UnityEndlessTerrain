using System.Collections.Generic;
using UnityEngine; 
 
public class TerrainManager : MonoBehaviour 
{ 
    /// <summary> 
    /// Show high detail around this object, lower further out. 
    /// </summary> 
    private Transform player; 
 
    /// <summary> 
    /// The index of the first chunk. Chosen as 50000 because negative perlin noise values 
    /// are mirrored, so (0,0) should be avoided. 
    /// </summary> 
    private const int InitialChunkPosition = 50000; 
 
    [SerializeField] 
    private Vector3 scale = Vector3.one;

    [SerializeField]
    private int nChunks;

    [SerializeField]
    private Material terrainMaterial;

    private void Start()
    {
        for (var i = 0; i < nChunks; i++)
        {
            for (var j = 0; j < nChunks; j++)
            {
                SpawnChunk(i, j);
            }
        }
    }

    private void Update() 
    { 
        // TODO: spawn chunks around the player
    }

    private void SpawnChunk(int xPos, int yPos)
    {
        var chunk = new GameObject
        {
            name = string.Format("Terrain chunk [{0}, {1}]", xPos, yPos)
        };
        chunk.transform.parent = transform;

        chunk.transform.position = new Vector3(
            (xPos - nChunks / 2) * scale.x, 
            0f, 
            (yPos - nChunks / 2) * scale.z
        );

        var terrainGen = chunk.AddComponent<TerrainGenerator>();
        terrainGen.Material = terrainMaterial;
        terrainGen.PosX = xPos + InitialChunkPosition;
        terrainGen.PosY = yPos + InitialChunkPosition;
        terrainGen.Scale = scale;
        terrainGen.SetupGameObject();
        terrainGen.UpdateMesh();
    }
} 
