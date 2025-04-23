using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColourMap }
    public DrawMode drawMode;

    [Header("Noise Settings")]
    [Range(50, 300)] public int mapWidth;
    [Range(50, 300)] public int mapHeight;
    public float noiseScale;
    public int octaves;
    [Range(0, 1)] public float persistance;
    [Range(1, 10)] public float lacunarity;
    public int seed;
    public Vector2 offset;
    public bool randomizeBiomeNoise = true;

    [Header("Auto Update")]
    public bool autoUpdate = false;

    [Header("Placement Settings")]
    public int chunkSize = 10;
    public int placementIterations = 5000;
    public List<LocationData> locations;

    [Header("Rendering")]
    public GameObject objectPrefab;

    [Header("Randomization")]
    public int randomLocationCount = 10;
    public int minRoadsPerLocation = 1;
    public int maxRoadsPerLocation = 3;
    public float minRoadDistance = 5f;
    public float maxRoadDistance = 40f;

    private float[,] noiseMap;
    private Color[] colourMap;

    private void Start()
    {
        RandomizeLocations();
        GenerateMap();
    }

    public void GenerateMap()
    {
        DeletePreviousObjects();

        noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
        BiomeGenerator biomeGen = FindObjectOfType<BiomeGenerator>();
        if (biomeGen == null) { Debug.LogError("BiomeGenerator не найден!"); return; }
        if (randomizeBiomeNoise)
            biomeGen.offset = new Vector2(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        float[,] biomeNoise = biomeGen.GenerateBiomeNoiseMap(mapWidth, mapHeight);

        string[,] biomeMap = new string[mapWidth, mapHeight];
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                biomeMap[x, y] = GetBiome(noiseMap[x, y], biomeNoise[x, y]);

        colourMap = new Color[mapWidth * mapHeight];
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                colourMap[y * mapWidth + x] = GetColorForBiome(biomeMap[x, y]);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        else
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));

        var placed = PlacementOptimizer.Optimize(locations, biomeMap, chunkSize, placementIterations);

        foreach (var loc in placed)
        {
            CreateObjectAtChunk(loc.AssignedChunk, loc.Id);
            foreach (var kv in loc.DesiredRoads)
                DrawRoad(loc.AssignedChunk.center, kv.Key.AssignedChunk.center);
        }
    }

    private void RandomizeLocations()
    {
        string[] biomeNames = new string[] {
            "Grassland", "Forest", "Jungle", "Sand", "MountainBase", "MountainMid", "MountainHigh", "MountainPeak"
        };

        locations = new List<LocationData>();
        for (int i = 0; i < randomLocationCount; i++)
        {
            var loc = new LocationData
            {
                Id = $"Loc{i + 1}",
                Biome = biomeNames[Random.Range(0, biomeNames.Length)]
            };
            locations.Add(loc);
        }

        foreach (var loc in locations)
        {
            int roadCount = Random.Range(minRoadsPerLocation, maxRoadsPerLocation + 1);
            for (int i = 0; i < roadCount; i++)
            {
                var target = locations[Random.Range(0, locations.Count)];
                if (target == loc || loc.Roads.Exists(r => r.target == target)) continue;
                float distance = Random.Range(minRoadDistance, maxRoadDistance);
                loc.Roads.Add(new RoadConnection { target = target, distance = distance });
            }
        }

        Debug.Log($"[Randomize] {locations.Count} locations and random roads created.");
    }

    private string GetBiome(float h, float n)
    {
        if (h <= 0.4f) return n < 0.5f ? "Water" : "Deep Water";
        if (h <= 0.44f) return "Sand";
        if (h <= 0.6f) return n < 0.5f ? "Grassland" : "Forest";
        if (h <= 0.69f) return n < 0.5f ? "Forest" : "Jungle";
        if (h <= 0.8f) return "MountainBase";
        if (h <= 0.85f) return "MountainMid";
        if (h <= 0.9f) return "MountainHigh";
        return "MountainPeak";
    }

    private Color GetColorForBiome(string biome)
    {
        switch (biome)
        {
            case "Water": return new Color(0.1f, 0.1f, 0.8f);
            case "Deep Water": return new Color(0.0f, 0.0f, 0.5f);
            case "Sand": return new Color(0.93f, 0.79f, 0.69f);
            case "Grassland": return new Color(0.5f, 0.8f, 0.2f);
            case "Forest": return new Color(0.1f, 0.5f, 0.1f);
            case "Jungle": return new Color(0.0f, 0.4f, 0.0f);
            case "MountainBase": return Color.gray;
            case "MountainMid": return new Color(0.6f, 0.6f, 0.6f);
            case "MountainHigh": return new Color(0.8f, 0.8f, 0.8f);
            case "MountainPeak": return Color.white;
            default: return Color.magenta;
        }
    }

    private void CreateObjectAtChunk(ChunkManager.Chunk chunk, string id)
    {
        float xOffset = mapWidth / 2f, yOffset = mapHeight / 2f, scale = 10f, hOff = 5f;
        Vector3 pos = new Vector3(
            (-(chunk.center.x - xOffset) * scale - 5),
            hOff,
            (-(chunk.center.y - yOffset) * scale - 5)
        );
        var obj = Instantiate(objectPrefab, pos, Quaternion.Euler(-90, 90, 0));
        obj.name = id;
        obj.tag = "GeneratedObject";
        obj.AddComponent<BoxCollider>().isTrigger = true;
        CreateNameLabel(obj, id);
    }

    private void DrawRoad(Vector2 start, Vector2 end)
    {
        List<Vector2> path = FindPath(start, end);
        foreach (var p in path)
        {
            int x = Mathf.RoundToInt(p.x), y = Mathf.RoundToInt(p.y);
            if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
                colourMap[y * mapWidth + x] = Color.red;
        }
        FindObjectOfType<MapDisplay>().DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
    }

    private List<Vector2> FindPath(Vector2 start, Vector2 end)
    {
        var openSet = new List<Vector2> { start };
        var cameFrom = new Dictionary<Vector2, Vector2>();
        var gScore = new Dictionary<Vector2, float> { [start] = 0f };
        var fScore = new Dictionary<Vector2, float> { [start] = Vector2.Distance(start, end) };

        while (openSet.Count > 0)
        {
            Vector2 current = openSet[0];
            foreach (var n in openSet)
                if (fScore.ContainsKey(n) && fScore[n] < fScore[current]) current = n;

            if (current == end) break;

            openSet.Remove(current);
            foreach (var neigh in GetNeighbors(current))
            {
                float cost = GetTerrainCost(neigh);
                if (cost == float.MaxValue) continue;
                float tg = gScore[current] + Vector2.Distance(current, neigh) * cost;
                if (!gScore.ContainsKey(neigh) || tg < gScore[neigh])
                {
                    cameFrom[neigh] = current;
                    gScore[neigh] = tg;
                    fScore[neigh] = tg + Vector2.Distance(neigh, end);
                    if (!openSet.Contains(neigh)) openSet.Add(neigh);
                }
            }
        }

        var path = new List<Vector2>();
        Vector2 cur = end;
        while (cameFrom.ContainsKey(cur))
        {
            path.Add(cur);
            cur = cameFrom[cur];
        }
        path.Reverse();
        return path;
    }

    private IEnumerable<Vector2> GetNeighbors(Vector2 cell)
    {
        int x = Mathf.RoundToInt(cell.x), y = Mathf.RoundToInt(cell.y);
        Vector2[] dirs = { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
        foreach (var d in dirs)
        {
            int nx = x + (int)d.x, ny = y + (int)d.y;
            if (nx < 0 || nx >= mapWidth || ny < 0 || ny >= mapHeight) continue;
            float h = noiseMap[nx, ny];
            if (h < 0.41f || h > 0.79f) continue;
            yield return new Vector2(nx, ny);
        }
    }

    private float GetTerrainCost(Vector2 pos)
    {
        float h = noiseMap[Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)];
        if (h < 0.41f || h > 0.79f) return float.MaxValue;
        return h > 0.6f ? 2f : 1f;
    }

    private void DeletePreviousObjects()
    {
        foreach (var o in GameObject.FindGameObjectsWithTag("GeneratedObject"))
            if (Application.isEditor) DestroyImmediate(o); else Destroy(o);
    }

    private void CreateNameLabel(GameObject parent, string text)
    {
        var go = new GameObject("NameLabel");
        go.transform.SetParent(parent.transform);
        var tm = go.AddComponent<TextMeshPro>();
        tm.text = text;
        tm.fontSize = 500;
        tm.color = Color.black;
        tm.alignment = TextAlignmentOptions.Center;
        tm.fontStyle = FontStyles.Bold;
        go.transform.localPosition = new Vector3(-2f, 0, 2f);
        go.transform.localRotation = Quaternion.Euler(180, 0, 90);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 20);
    }
}
