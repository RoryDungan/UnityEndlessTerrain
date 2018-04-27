using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Code.Terrain;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Generates terrain on the attached MeshFilter
/// </summary>
[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
    // Enum so that it can be easily shown in the inspector.
    enum TerrainResolution
    {
        Resolution_129,
        Resolution_65,
        Resolution_33,
        Resolution_17,
        Resolution_9
    }

    [SerializeField]
    private TerrainResolution meshResolution = TerrainResolution.Resolution_129;

    private TerrainResolution cachedMeshResolution;

    /// <summary>
    /// The X position of this chunk in the world grid.
    /// </summary>
    [SerializeField]
    private int posX;

    private int cachedPosX;

    /// <summary>
    /// The X position of this chunk in the world grid.
    /// </summary>
    public int PosX { get { return posX; } set { posX = value; } }

    /// <summary>
    /// The Y position of this chunk in the world grid.
    /// </summary>
    [SerializeField]
    private int posY;

    private int cachedPosY;

    /// <summary>
    /// The Y position of this chunk in the world grid.
    /// </summary>
    public int PosY { get { return posY; } set { posY = value; } }

    /// <summary>
    /// Size of terrrain chunk meshes.
    /// </summary>
    private int Size { get { return TerrainResolutionToSize(meshResolution); } }

    [SerializeField]
    private Vector3 scale = Vector3.one;

    /// <summary>
    /// The size of the resulting terrain object.
    /// </summary>
    public Vector3 Scale { get { return scale; } set { scale = value; } }

    private Vector3 cachedScale;

    [Serializable]
    struct Weighting : IEquatable<Weighting>
    {
        public int Level;
        public float Weight;

        public bool Equals(Weighting other)
        {
            return Level == other.Level && Weight == other.Weight;
        }
    }

    [SerializeField]
    private Weighting[] weightings = new[]
    { 
        new Weighting { Level = 1, Weight = 400f },
        new Weighting { Level = 4, Weight = 200f },
        new Weighting { Level = 8, Weight = 50f },
        new Weighting { Level = 16, Weight = 20f },
        new Weighting { Level = 32, Weight = 5f },
        new Weighting { Level = 64, Weight = 2f }, 
    };

    private Weighting[] cachedWeightings;

    [SerializeField]
    private bool autoUpdate = false;

    [SerializeField]
    private bool createCollisionMesh;


    private MeshFilter meshFilter;

    private MeshFilter MeshFilter
    {
        get
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
                Assert.IsNotNull(meshFilter);
            }
            return meshFilter;
        }
    }

    private MeshCollider meshCollider;

    private MeshCollider MeshCollider
    {
        get
        {
            if (meshCollider == null)
            {
                meshCollider = GetComponentInChildren<MeshCollider>();
                if (meshCollider == null)
                {
                    meshCollider = InitColliderObject();
                }
            }
            return meshCollider;
        }
    }

    public Material Material
    {
        get
        {
            var renderer = GetComponent<MeshRenderer>();
            Assert.IsNotNull(renderer, "No MeshRenderer assigned to TerrainGenerator object");

            return renderer.material;
        }
        set
        {
            var renderer = GetComponent<MeshRenderer>();
            Assert.IsNotNull(renderer, "No MeshRenderer assigned to TerrainGenerator object");

            renderer.material = value;
        }
    }

    private MeshCollider InitColliderObject()
    {
        var colliderObject = new GameObject
        {
            name = "Collider",
        };
        colliderObject.transform.parent = transform;
        colliderObject.transform.localPosition = Vector3.zero;

        // Move the collider object up to ensure that it doesn't intersect with the visual
        // mesh.
        colliderObject.transform.position += Vector3.up * 1.2f;

        var collider = colliderObject.AddComponent<MeshCollider>();
        return collider;
    }

    /// <summary>
    /// Ensure the game object is set up correctly.
    /// </summary>
    public void SetupGameObject()
    {
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        gameObject.isStatic = true;
    }

    private static float OctavePerlin(float x, float y, Weighting[] weightings)
    {
        var acc = 0f;
        for (var i = 0; i < weightings.Length; i++)
        {
            //acc += Mathf.PerlinNoise(x * weightings[i].Level, y * weightings[i].Level) * weightings[i].Weight;
            acc += Perlin.perlin(x * weightings[i].Level, y * weightings[i].Level, 0f) * weightings[i].Weight;
        }
        return acc;
    }

    private static Vector3[] GenerateVerts(
        int size, 
        int posX, 
        int posY, 
        Weighting[] weightings,
        Vector3 scale
    )
    {
        var v = new Vector3[size * size];
        for (var i = 0; i < size; i++)
        {
            for (var j = 0; j < size; j++)
            {
                v[i * size + j].x = 
                    i * scale.x / (size - 1);

                v[i * size + j].y =
                    scale.y * OctavePerlin(
                        (float)i / (size - 1) + posX, 
                        (float)j / (size - 1) + posY, 
                        weightings
                    );

                v[i * size + j].z = 
                    j * scale.z / (size - 1);
            }
        }
        return v;
    }

    private static int[] GenerateTris(
        int size
    )
    {
        var total = size - 1;

        var tris = new int[total * total * 6];
        for (var i = 0; i < total; i++)
        {
            for (var j = 0; j < total; j++)
            {
                var idx = i * total * 6 + j * 6;
                tris[idx] = i * size + j;

                tris[idx + 1] = i * size + j + 1;
                tris[idx + 2] = i * size + j + size;
                tris[idx + 3] = i * size +j + 1;
                tris[idx + 4] = i * size + j + size + 1;
                tris[idx + 5] = i * size + j + size;
            }
        }

        return tris;
    }

    private static Vector2[] GenerateUVs(int size)
    {
        var uvs = new Vector2[size * size];
        for (var i = 0; i < size; i++)
        {
            for (var j = 0; j < size; j++)
            {
                uvs[i * size + j].x = (float)i / size;
                uvs[i * size + j].y = (float)j / size;
            }
        }

        return uvs;
    }

    [ContextMenu("Update mesh")]
    public void UpdateMesh()
    {
        var sw = Stopwatch.StartNew();

        var mesh = MeshFilter.mesh = new Mesh
        {
            name = "TerrainChunk",
            vertices = GenerateVerts(Size, posX, posY, weightings, scale),
            triangles = GenerateTris(Size),
            uv = GenerateUVs(Size)
        };

        mesh.RecalculateNormals();

        sw.Stop();
        var elapsedMs = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1000D;
        UnityEngine.Debug.Log("UpdateMesh: " + elapsedMs + "ms");

        if (createCollisionMesh)
        {
            UpdateCollider();
        }
    }

    private void UpdateCollider()
    {
        var sw = Stopwatch.StartNew();

        var mesh = new Mesh
        {
            name = "TerrainCollider",
            vertices = GenerateVerts(17, posX, posY, weightings, scale),
            triangles = GenerateTris(17)
        };

        MeshCollider.sharedMesh = mesh;

        sw.Stop();
        var elapsedMs = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1000D;
        UnityEngine.Debug.Log("UpdateCollider: " + elapsedMs.ToString("##.##") + "ms");
    }


    private static int TerrainResolutionToSize(TerrainResolution res)
    {
        switch (res)
        {
            case TerrainResolution.Resolution_9:
                return 9;

            case TerrainResolution.Resolution_17:
                return 17;

            case TerrainResolution.Resolution_33:
                return 33;

            case TerrainResolution.Resolution_65:
                return 65;

            case TerrainResolution.Resolution_129:
                return 129;

            default:
                throw new ArgumentOutOfRangeException();
        }
    
    }

    private void Update()
    {
        if (!autoUpdate)
        {
            return;
        }

        var dirty = false;

        if (Input.GetKeyDown(KeyCode.T))
        {
            dirty = true;
        }

        // Check dirty
        if (scale != cachedScale)
        {
            cachedScale = scale;
            dirty = true;
        }

        if (meshResolution != cachedMeshResolution)
        {
            cachedMeshResolution = meshResolution;
            dirty = true;
        }

        if (weightings != null && cachedWeightings != null && weightings.Length == cachedWeightings.Length)
        {
            for (var i = 0; i < weightings.Length; i++)
            {
                if (!weightings[i].Equals(cachedWeightings[i]))
                {
                    dirty = true;
                    cachedWeightings = (Weighting[])weightings.Clone();
                    break;
                }
            }
        }
        else
        {
            dirty = true;
            cachedWeightings = (Weighting[])weightings.Clone();
        }

        if (posX != cachedPosX)
        {
            dirty = true;
            cachedPosX = posX;
        }

        if (posY != cachedPosY)
        {
            dirty = true;
            cachedPosY = posY;
        }

        if (dirty)
        {
            UpdateMesh();
        }
    }
}
