using Assets.Code.Terrain;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TerrainManager : MonoBehaviour 
{ 
    /// <summary> 
    /// Show high detail around this object, lower further out. 
    /// </summary> 
    private Transform player;


    [SerializeField]
    private Vector3 scale = new Vector3(256, 1, 256);

    [SerializeField]
    private Material terrainMaterial;

    private IDictionary<int, IDictionary<GridCoords, Transform[]>> meshChunks;

    struct GridCoords
    {
        public int X;
        public int Y;

        public static GridCoords operator +(GridCoords a, GridCoords b) => 
            new GridCoords
            {
                X = a.X + b.X,
                Y = a.Y + b.Y
            };

        public static GridCoords operator -(GridCoords a, GridCoords b) => 
            new GridCoords
            {
                X = a.X - b.X,
                Y = a.Y - b.Y
            };

        float Magnitude() => 
            Mathf.Sqrt(X * X + Y * Y);
    }

    /// <summary>
    /// Convert a world-space position to grid coordinates. Ignores elevation (y axis).
    /// </summary>
    GridCoords WorldPositionToGridCoords(Vector3 pos)
    {
        return new GridCoords
        {
            X = (int)Math.Floor(pos.x / scale.x),
            Y = (int)Math.Floor(pos.z / scale.z)
        };
    }

    private void Start()
    {
        Assert.IsNotNull(player);

        meshChunks = new Dictionary<int, IDictionary<GridCoords, Transform[]>>();
    }

    private void Update() 
    { 
        // TODO: spawn chunks around the player
    }

    private void SpawnChunk(int size, int xPos, int yPos)
    {
        var chunk = new GameObject
        {
            name = string.Format("Terrain chunk [{0}, {1}]", xPos, yPos)
        };
        chunk.transform.parent = transform;

        var meshFilter = chunk.AddComponent<MeshFilter>();
        meshFilter.mesh = FlatMeshGenerator.GenerateFlatMesh(size, scale);

        var renderer = chunk.AddComponent<MeshRenderer>();
        renderer.material = terrainMaterial;
    }
} 
