using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ModularBuildingGenerator : MonoBehaviour
{

    const float WALL_LENGTH = 2f;    // ����� ����� (��� Z)
    const float WALL_HEIGHT = 2f;    // ������ ����� (��� Y)
    const float WALL_THICKNESS = 0.5f; // ������ ����� (��� X)


    [Header("Foundation Settings")]
    public Vector2 minMaxSize = new Vector2(2, 11);
    public GameObject floorPrefab;

    [Header("Wall Modules")]
    public GameObject wallPrefab;
    public GameObject windowPrefab;
    public GameObject doorPrefab;

    [Header("Roof Modules")]
    public GameObject roofPrefab;

    [Header("Generation Settings")]
    [Range(1, 5)] public int minFloors = 1;
    [Range(1, 5)] public int maxFloors = 2;

    private Vector3 buildingSize;
    private int floorsCount;

    private float doorChance = 0.2f;
    private float wallHeight = 2f;
    private float windowChance = 0.3f;

    private Transform currentBuilding;

    private GameObject northWallGroup;
    private GameObject southWallGroup;
    private GameObject westWallGroup;
    private GameObject eastWallGroup;

    public void GenerateBuilding()
    {
        if (currentBuilding != null)
        {
            DestroyImmediate(currentBuilding.gameObject);
        }

        // ������� ��������� ��� ������
        CreateGroups();

        // ���������� ���������
        floorsCount = Random.Range(minFloors, maxFloors + 1);
        buildingSize = new Vector3(
            (int)(Random.Range(minMaxSize.x, minMaxSize.y)),
            0,
            (int)(Random.Range(minMaxSize.x, minMaxSize.y))
        );

        CreateFoundation();
        CreateFloors();
        CreateRoof();
    }

    void CreateGroups()
    {
        currentBuilding = new GameObject("Generated Building").transform;

        northWallGroup = new GameObject("North Wall");
        southWallGroup = new GameObject("South Wall");
        westWallGroup = new GameObject("West Wall");
        eastWallGroup = new GameObject("East Wall");

        northWallGroup.transform.SetParent(currentBuilding);
        southWallGroup.transform.SetParent(currentBuilding);
        westWallGroup.transform.SetParent(currentBuilding);
        eastWallGroup.transform.SetParent(currentBuilding);
    }

    void CreateFoundation()
    {
        GameObject foundationGroup = new GameObject("Foundation");
        foundationGroup.transform.SetParent(currentBuilding);

        var foundation = Instantiate(
            floorPrefab,
            Vector3.zero,
            Quaternion.identity,
            foundationGroup.transform
        );

        foundation.transform.localScale = new Vector3(
            buildingSize.x,
            0.4f,
            buildingSize.z
        );
    }

    void CreateFloor(int floorNumber)
    {
        float yPos = floorNumber * wallHeight + 0.2f;

        GameObject floorGroup = new GameObject($"Floor_{floorNumber}");
        floorGroup.transform.SetParent(currentBuilding);

        // ��������� ���� �� ���������
        CreateWallRing(new Vector3(0, yPos, 0), buildingSize, floorNumber);
    }

    void CreateFloors()
    {
        for (int floor = 0; floor < floorsCount; floor++)
        {
            CreateFloor(floor);
        }
    }

    void CreateWallRing(Vector3 position, Vector3 size, int floorNumber)
    {
        // ������������ ������� ��� ����
        float halfX = (buildingSize.x + WALL_THICKNESS * 2) * 0.5f;
        float halfZ = (buildingSize.y + WALL_THICKNESS * 2) * 0.5f;

        // ������� ����� ��� ������ �������
        CreateWallSegment(new Vector3(-buildingSize.x * 2, position.y, -buildingSize.z * 2), 180, size.x, floorNumber, northWallGroup.transform);   // ��������
        CreateWallSegment(new Vector3(-buildingSize.x / 2, position.y, -buildingSize.z / 2), 0, size.x, floorNumber, southWallGroup.transform);   // �����
        CreateWallSegment(new Vector3(-halfX, position.y, 0), 90, size.z, floorNumber, westWallGroup.transform);  // ��������
        CreateWallSegment(new Vector3(halfX, position.y, 0), -90, size.z, floorNumber, eastWallGroup.transform);   // ���������
    }

    void CreateWallSegment(Vector3 position, float rotation, float length, int floorNumber, Transform group)
    {
        int wallsCount = Mathf.CeilToInt(length);
        bool doorSpawned = false;

        if (Mathf.Abs(rotation) == 90)
        {
            for (int i = 0; i < wallsCount; i++)
            {
                GameObject prefabOfWallDoorWindow;
                float rand = Random.value;

                if (floorNumber == 0 && !doorSpawned && rand < doorChance)
                {
                    prefabOfWallDoorWindow = doorPrefab;
                    doorSpawned = true;
                }
                else if (rand < windowChance)
                {
                    prefabOfWallDoorWindow = windowPrefab;
                }
                else // ����� ����� ������ � ����� � ���
                {
                    prefabOfWallDoorWindow = wallPrefab;
                }

                Vector3 wallPosition = position + new Vector3(0, 0, (i * 2) - length / 2);

                Debug.Log($"{group.name} | CNT: {wallsCount} | I: {i}\n WallPos: {wallPosition} | Pos: {position}");

                GameObject wall = Instantiate(
                    prefabOfWallDoorWindow,
                    wallPosition,
                    Quaternion.Euler(0, rotation, 0),
                    group
                );
            }
        }
        else
        {
            for (int i = 0; i < wallsCount; i++)
            {
                GameObject prefabOfWallDoorWindow;
                float rand = Random.value;

                if (floorNumber == 0 && !doorSpawned && rand < doorChance)
                {
                    prefabOfWallDoorWindow = doorPrefab;
                    doorSpawned = true;
                }
                else if (rand < windowChance)
                {
                    prefabOfWallDoorWindow = windowPrefab;
                }
                else // ����� ����� ������ � ����� � ���
                {
                    prefabOfWallDoorWindow = wallPrefab;
                }

                Vector3 wallPosition = position + new Vector3((i * 2), 0, 0);

                Debug.Log($"{group.name} | CNT: {wallsCount} | I: {i}\n WallPos: {wallPosition} | Pos: {position}");

                GameObject wall = Instantiate(
                    prefabOfWallDoorWindow,
                    wallPosition,
                    Quaternion.Euler(0, rotation, 0),
                    group
                );
            }
        }
    }

    void CreateRoof()
    {
        GameObject roofGroup = new GameObject("Roof");
        roofGroup.transform.SetParent(currentBuilding);

        Vector3 roofPosition = new Vector3(
            0,
            floorsCount * wallHeight + 0.2f,
            0
        );

        GameObject roof = Instantiate(
            roofPrefab,
            roofPosition,
            Quaternion.identity,
            roofGroup.transform
        );

        // ��������� ������ ����� ��� ������
        roof.transform.localScale = new Vector3(
            buildingSize.x,
            1,
            buildingSize.z
        );
    }

    public void DestroyBuilding()
    {
        if (currentBuilding != null)
        {
            DestroyImmediate(currentBuilding.gameObject);
        }
    }
}