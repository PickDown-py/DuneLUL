using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail) {
        AnimationCurve heightCurve = new AnimationCurve (_heightCurve.keys);

        int width = heightMap.GetLength (0);
        int height = heightMap.GetLength (1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int meshSimplificationIncrement = (levelOfDetail == 0)?1:levelOfDetail * 2;
        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData (verticesPerLine, verticesPerLine);
        int vertexIndex = 0;

        for (int y = 0; y < height; y += meshSimplificationIncrement) {
            for (int x = 0; x < width; x += meshSimplificationIncrement) {
                meshData.vertices [vertexIndex] = new Vector3 (topLeftX + x, heightCurve.Evaluate (heightMap [x, y]) * heightMultiplier, topLeftZ - y);
                meshData.uvs [vertexIndex] = new Vector2 (x / (float)width, y / (float)height);

                if (x < width - 1 && y < height - 1) {
                    meshData.AddTriangle (vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle (vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
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