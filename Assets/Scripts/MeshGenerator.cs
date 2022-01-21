using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap)
    {
        var width = heightMap.GetLength(0);
        var height = heightMap.GetLength(1);
        var topLeftX = (width - 1) / -2f;
        var topLeftZ = (height - 1) / 2f;

        var meshData = new MeshData(width, height);
        var vertIndex = 0;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                meshData.vertices[vertIndex] = new Vector3(topLeftX + x, 20*heightMap[x, y], topLeftZ - y);
                meshData.uvs[vertIndex] = new Vector2(x / (float) width, +y / (float) height);

                if (x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertIndex, vertIndex + width + 1, vertIndex + width);
                    meshData.AddTriangle(vertIndex + width + 1, vertIndex, vertIndex + 1);
                }

                vertIndex++;
            }
        }

        return meshData;
    }
}

public struct MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    private int _triangleIndex;

    public MeshData(int width, int height)
    {
        vertices = new Vector3[width * height];
        triangles = new int[(width - 1) * (height - 1) * 6];
        uvs = new Vector2[width * height];
        _triangleIndex = 0;
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[_triangleIndex++] = a;
        triangles[_triangleIndex++] = b;
        triangles[_triangleIndex++] = c;
    }

    public Mesh Mesh
    {
        get
        {
            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uvs
            };
            mesh.RecalculateNormals();

            return mesh;
        }
    }
}