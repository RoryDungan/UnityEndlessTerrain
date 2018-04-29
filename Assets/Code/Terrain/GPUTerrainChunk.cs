using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class GPUTerrainChunk : MonoBehaviour
{
    /// <summary>
    /// Size of terrrain chunk meshes.
    /// </summary>
    private int Size = 256;

    [SerializeField]
    private Vector3 scale = Vector3.one;

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

    private static Vector3[] GenerateVerts(
        int size,
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

                v[i * size + j].y = 0f;

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
            vertices = GenerateVerts(Size, scale),
            triangles = GenerateTris(Size),
            uv = GenerateUVs(Size)
        };

        mesh.RecalculateNormals();

        sw.Stop();
        var elapsedMs = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1000D;
        UnityEngine.Debug.Log("UpdateMesh: " + elapsedMs + "ms");
    }
}
