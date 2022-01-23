using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ProceduralLandmass;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColorMap,
        Mesh
    }

    public DrawMode drawMode;

    public const int mapChunkSize = 241;
    [Range(0,6)]
    public int levelOfDetail;
    
    public float noiseScale;
    public float meshHeight;

    public int octaves;
    public float persistence;
    public float lacunarity;

    public AnimationCurve meshCurve;

    public int seed;
    public Vector2 offset;

    public bool autoUpdate;

    public TerrainType[] regions;
    private Queue<MapThreadInfo<MapData>> mapThreadInfosQueue = new();

    public void DrawMapInEditor()
    {
        var mapData = GenerateMapData();
        var mapDisplay = FindObjectOfType<MapDisplay>();
        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                mapDisplay.DrawTexture(TextureGenerator.TextureFromNoiseMap(mapData.heightMap));
                break;
            case DrawMode.ColorMap:
                mapDisplay.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
                break;
            case DrawMode.Mesh:
                mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeight, meshCurve, levelOfDetail), 
                    TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
                break;
        }
    }

    public void RequestMapData(Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(callback);
        };
    }

    private void MapDataThread(Action<MapData> callback)
    {
        var mapData = GenerateMapData();
        lock (mapThreadInfosQueue) {
            mapThreadInfosQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    private void Update()
    {
        if (mapThreadInfosQueue.Count > 0)
        {
            foreach (var mapThreadInfo in mapThreadInfosQueue)
            {
                mapThreadInfo.callBack(mapThreadInfo.parameter);
            }
        }
    }

    private MapData GenerateMapData()
    {
        var noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistence,
            lacunarity, offset);

        var colorMap = new Color[mapChunkSize * mapChunkSize];
        for (var y = 0; y < mapChunkSize; y++)
        {
            for (var x = 0; x < mapChunkSize; x++)
            {
                var currentHeight = noiseMap[x, y];
                for (var i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colorMap);
    }

    void OnValidate()
    {
        if (lacunarity < 1)
            lacunarity = 1;
        if (octaves < 0)
            octaves = 1;
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callBack;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callBack, T parameter)
        {
            this.callBack = callBack;
            this.parameter = parameter;
        }
    }
}

[Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}