using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class LocationDefinition
{
    public string locationName;
    public string biome;
    public List<string> connectedLocations;
}

public class MapGeneratorManual : MonoBehaviour
{
    [Header("Map Settings")]
    [Range(50, 300)] public int mapWidth = 100;
    [Range(50, 300)] public int mapHeight = 100;
    public int chunkSize = 20;

    [Header("Noise & Biome")]
    public float noiseScale = 50f;
    public int octaves = 4;
    [Range(0, 1)] public float persistance = 0.5f;
    [Range(1, 10)] public float lacunarity = 2f;
    public int seed = 42;
    public Vector2 noiseOffset;
    public BiomeGenerator biomeGenerator;

    [Header("Debug Options")]
    public bool drawChunkBorders = true;
    public bool showChunkBiomes = true;
    public bool highlightDominantBiome = false;    // ? новая галочка

    [Header("Manual Locations")]
    public List<LocationDefinition> locations;
    public GameObject objectPrefab;

    private float[,] heightMap;
    private float[,] biomeNoiseMap;
    private Color[] colourMap;

    public void GenerateManualMap()
    {
        // 0) Чистка называний
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            if (child.name.StartsWith("BiomeLabel_") || child.name.StartsWith("BiomeBG_"))
                DestroyImmediate(child);
        }
        // 1) Шумы
        heightMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, noiseOffset);
        biomeNoiseMap = biomeGenerator.GenerateBiomeNoiseMap(mapWidth, mapHeight);

        // 2) Заливка биомов
        colourMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
                colourMap[y * mapWidth + x] = GetColorForBiome(GetBiome(heightMap[x, y], biomeNoiseMap[x, y]));

        // 2.5) Границы чанков
        if (drawChunkBorders)
        {
            for (int cx = 0; cx <= mapWidth; cx += chunkSize)
                for (int y = 0; y < mapHeight; y++)
                    if (cx < mapWidth) colourMap[y * mapWidth + cx] = Color.black;

            for (int cy = 0; cy <= mapHeight; cy += chunkSize)
                for (int x = 0; x < mapWidth; x++)
                    if (cy < mapHeight) colourMap[cy * mapWidth + x] = Color.black;
        }

        // 3) Определяем батю на районе
        int chunksX = Mathf.CeilToInt((float)mapWidth / chunkSize);
        int chunksY = Mathf.CeilToInt((float)mapHeight / chunkSize);
        string[,] chunkBiome = new string[chunksX, chunksY];

        for (int cx = 0; cx < chunksX; cx++)
        {
            for (int cy = 0; cy < chunksY; cy++)
            {
                var count = new Dictionary<string, int>();
                for (int dx = 0; dx < chunkSize; dx++)
                    for (int dy = 0; dy < chunkSize; dy++)
                    {
                        int x = cx * chunkSize + dx, y = cy * chunkSize + dy;
                        if (x >= mapWidth || y >= mapHeight) continue;
                        string b = GetBiome(heightMap[x, y], biomeNoiseMap[x, y]);
                        if (b == "Water" || b == "Deep Water") continue;
                        if (!count.ContainsKey(b)) count[b] = 0;
                        count[b]++;
                    }

                string mainBiome = null;
                int max = -1;
                foreach (var kv in count)
                    if (kv.Value > max) { max = kv.Value; mainBiome = kv.Key; }
                mainBiome ??= "Grassland";
                chunkBiome[cx, cy] = mainBiome;

                // подпись + фон
                if (showChunkBiomes)
                {
                    float centerX = cx * chunkSize + chunkSize / 2f;
                    float centerY = cy * chunkSize + chunkSize / 2f;
                    float worldX = -(centerX - mapWidth / 2f) * 10f;
                    float worldZ = -(centerY - mapHeight / 2f) * 10f;

                    // текст
                    var textObj = new GameObject($"BiomeLabel_{cx}_{cy}");
                    textObj.transform.SetParent(transform);
                    textObj.transform.position = new Vector3(worldX, 10f, worldZ);
                    textObj.transform.rotation = Quaternion.Euler(90, 0, 0);
                    textObj.transform.localScale = Vector3.one * 65f;
                    textObj.tag = "GeneratedObject";

                    var tm = textObj.AddComponent<TextMeshPro>();
                    tm.text = mainBiome;
                    tm.fontSize = 10f;
                    tm.enableAutoSizing = false;
                    tm.alignment = TextAlignmentOptions.Center;
                    tm.enableWordWrapping = false;
                    tm.overflowMode = TextOverflowModes.Overflow;
                    tm.ForceMeshUpdate();

                    // фон под текст
                    Vector2 ts = tm.GetRenderedValues(false);
                    float padX = 0.2f, padY = 0.1f;
                    var bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    bg.name = $"BiomeBG_{cx}_{cy}";
                    bg.transform.SetParent(textObj.transform);
                    bg.transform.localPosition = new Vector3(0, 0, 0.01f);
                    bg.transform.localRotation = Quaternion.identity;
                    bg.transform.localScale = new Vector3(ts.x + padX, ts.y + padY, 1f);
                    var mat = new Material(Shader.Find("Unlit/Color"));
                    mat.color = Color.black;
                    bg.GetComponent<MeshRenderer>().material = mat;
                }
            }
        }

        // 3.5) Подсветка главного биома на красный чтобы красиво
        if (highlightDominantBiome)
        {
            for (int cx = 0; cx < chunksX; cx++)
            {
                for (int cy = 0; cy < chunksY; cy++)
                {
                    string mainB = chunkBiome[cx, cy];
                    for (int dx = 0; dx < chunkSize; dx++)
                        for (int dy = 0; dy < chunkSize; dy++)
                        {
                            int x = cx * chunkSize + dx, y = cy * chunkSize + dy;
                            if (x >= mapWidth || y >= mapHeight) continue;
                            if (GetBiome(heightMap[x, y], biomeNoiseMap[x, y]) == mainB)
                                colourMap[y * mapWidth + x] = Color.red;
                        }
                }
            }
        }

        // 4) Локации 
        var placed = new Dictionary<string, Vector2>();
        for (int i = 0; i < locations.Count; i++)
        {
            var loc = locations[i];
            // ищем чанки...
            List<Vector2Int> valid = new();
            for (int cx = 0; cx < chunksX; cx++)
                for (int cy = 0; cy < chunksY; cy++)
                    if (chunkBiome[cx, cy] == loc.biome)
                        valid.Add(new Vector2Int(cx, cy));
            if (valid.Count == 0) { Debug.LogWarning($"Нет чанков с {loc.biome}"); continue; }
            var ch = valid[Random.Range(0, valid.Count)];
            int px = Mathf.Clamp(ch.x * chunkSize + Random.Range(0, chunkSize), 0, mapWidth - 1);
            int py = Mathf.Clamp(ch.y * chunkSize + Random.Range(0, chunkSize), 0, mapHeight - 1);
            placed[loc.locationName] = new Vector2(px, py);
            CreateLocationObject(loc.locationName, px, py);
        }

        // 5) Дороги, 6) Рендер
        foreach (var loc in locations)
            if (placed.TryGetValue(loc.locationName, out var s))
                foreach (var other in loc.connectedLocations)
                    if (placed.TryGetValue(other, out var e))
                        DrawSimpleRoad(s, e);

        FindObjectOfType<MapDisplay>()
            .DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
    }

    private string GetBiome(float height, float noise)
    {
        if (height <= 0.4f) return noise < 0.5f ? "Water" : "Deep Water";
        if (height <= 0.44f) return "Sand";
        if (height <= 0.6f) return noise < 0.5f ? "Grassland" : "Forest";
        if (height <= 0.69f) return noise < 0.5f ? "Forest" : "Jungle";
        if (height <= 0.8f) return "MountainBase";
        if (height <= 0.85f) return "MountainMid";
        if (height <= 0.9f) return "MountainHigh";
        return "MountainPeak";
    }


    //Пока что не уверен нужно ли эта *****(штуковина) снизу
    private Color GetColorForBiome(string biome)
    {
        return biome switch
        {
            "Water" => new Color(0.1f, 0.1f, 0.8f),
            "Deep Water" => new Color(0, 0, 0.5f),
            "Sand" => new Color(0.93f, 0.79f, 0.69f),
            "Grassland" => new Color(0.5f, 0.8f, 0.2f),
            "Forest" => new Color(0.1f, 0.5f, 0.1f),
            "Jungle" => new Color(0, 0.4f, 0),
            "MountainBase" => Color.gray,
            "MountainMid" => new Color(0.6f, 0.6f, 0.6f),
            "MountainHigh" => new Color(0.8f, 0.8f, 0.8f),
            "MountainPeak" => Color.white,
            _ => Color.magenta
        };
    }

    private void CreateLocationObject(string name, int x, int y)
    {
        float worldX = -(x - mapWidth / 2f) * 10f;
        float worldZ = -(y - mapHeight / 2f) * 10f;
        var obj = Instantiate(objectPrefab,
            new Vector3(worldX, 5f, worldZ),
            Quaternion.Euler(-90, 90, 0),
            transform);
        obj.name = name;
        obj.tag = "GeneratedObject";

        var textObj = new GameObject("Label");
        textObj.transform.SetParent(obj.transform);
        var tm = textObj.AddComponent<TextMeshPro>();
        tm.text = name;
        tm.fontSize = 200;
        tm.alignment = TextAlignmentOptions.Center;
        textObj.transform.localPosition = new Vector3(0, 2f, 0);
    }

    private void DrawSimpleRoad(Vector2 a, Vector2 b)
    {
        var open = new List<Vector2> { a };
        var came = new Dictionary<Vector2, Vector2>();
        var g = new Dictionary<Vector2, float> { [a] = 0f };
        var f = new Dictionary<Vector2, float> { [a] = Vector2.Distance(a, b) };

        while (open.Count > 0)
        {
            Vector2 cur = open[0];
            foreach (var n in open) if (f[n] < f[cur]) cur = n;
            if (cur == b) break;
            open.Remove(cur);
            foreach (var nb in new[] { Vector2.left, Vector2.right, Vector2.up, Vector2.down })
            {
                Vector2 w = cur + nb;
                int xi = Mathf.RoundToInt(w.x), yi = Mathf.RoundToInt(w.y);
                if (xi < 0 || xi >= mapWidth || yi < 0 || yi >= mapHeight) continue;
                float c = heightMap[xi, yi] < 0.41f || heightMap[xi, yi] > 0.79f ? float.MaxValue :
                          heightMap[xi, yi] > 0.6f ? 2f : 1f;
                if (c == float.MaxValue) continue;
                float ng = g[cur] + Vector2.Distance(cur, w) * c;
                if (!g.ContainsKey(w) || ng < g[w])
                {
                    came[w] = cur; g[w] = ng; f[w] = ng + Vector2.Distance(w, b);
                    if (!open.Contains(w)) open.Add(w);
                }
            }
        }

        // рисуем
        Vector2 p = b;
        while (came.ContainsKey(p))
        {
            int xi = Mathf.RoundToInt(p.x), yi = Mathf.RoundToInt(p.y);
            colourMap[yi * mapWidth + xi] = Color.red;
            p = came[p];
        }
        FindObjectOfType<MapDisplay>()
            .DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
    }
}
