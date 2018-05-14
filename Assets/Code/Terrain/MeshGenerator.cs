using UnityEngine;

namespace Assets.Code.Terrain
{
    /// <summary>
    /// Functions for generating flat meshes
    /// </summary>
    class FlatMeshGenerator
    {
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
                        i * scale.x / (size - 1) - scale.x / 2f;

                    v[i * size + j].y = 0f;

                    v[i * size + j].z =
                        j * scale.z / (size - 1) - scale.z / 2f;
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

        public static Mesh GenerateFlatMesh(int size, Vector3 scale) => 
            new Mesh
            {
                name = "TerrainChunk",
                vertices = GenerateVerts(size, scale),
                triangles = GenerateTris(size),
                uv = GenerateUVs(size)
            };
    }
}
