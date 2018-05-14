using Assets.Code.Terrain;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class GPUTerrainChunk : MonoBehaviour
{
    /// <summary>
    /// Size of terrrain chunk meshes.
    /// </summary>
    [SerializeField]
    private int Size = 10;

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

    [ContextMenu("Update mesh")]
    public void UpdateMesh()
    {
        var sw = Stopwatch.StartNew();

        var mesh = MeshFilter.mesh = FlatMeshGenerator.GenerateFlatMesh(Size, scale);
            
        mesh.RecalculateNormals();

        sw.Stop();
        var elapsedMs = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1000D;
        UnityEngine.Debug.Log("UpdateMesh: " + elapsedMs + "ms");
    }
}
