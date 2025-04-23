using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ModularBuildingGenerator : MonoBehaviour
{
    [Header("Foundation Settings")]
    public Vector2 minMaxSize = new Vector2(5, 15);
    public GameObject foundationPrefab;

    [Header("Wall Modules")]
    public GameObject wallPrefab;
    public GameObject windowWallPrefab;
    public GameObject doorWallPrefab;
    public float wallHeight = 2f;

    [Header("Roof Modules")]
    public GameObject[] roofPrefabs;

    [Header("Generation Settings")]
    [Range(1, 5)] public int minFloors = 1;
    [Range(1, 5)] public int maxFloors = 1;
    [Range(0, 1)] public float windowChance = 0.3f;

    private Vector3 buildingSize;
    private int floorsCount;
    private Transform currentBuilding;

    [ContextMenu("Generate Building Now!")]
    public void GenerateBuilding()
    {
        if (currentBuilding != null) Destroy(currentBuilding.gameObject);

        // ������� ��������� ��� ������
        currentBuilding = new GameObject("Generated Building").transform;

        // ���������� ���������
        floorsCount = Random.Range(minFloors, maxFloors + 1);
        buildingSize = new Vector3(
            Random.Range(minMaxSize.x, minMaxSize.y),
            0,
            Random.Range(minMaxSize.x, minMaxSize.y)
        );

        // ������ ���������
        CreateFoundation();

        // ������ �����
        for (int floor = 0; floor < floorsCount; floor++)
        {
            CreateFloor(floor);
        }

        // ��������� �����
        CreateRoof();
    }

    void CreateFoundation()
    {
        var foundation = Instantiate(
            foundationPrefab,
            Vector3.zero,
            Quaternion.identity,
            currentBuilding
        );

        foundation.transform.localScale = new Vector3(
            buildingSize.x,
            0.2f,
            buildingSize.z
        );
    }

    void CreateFloor(int floorNumber)
    {
        float yPos = floorNumber * wallHeight + 0.2f;

        // ��������� ���� �� ���������
        CreateWallRing(new Vector3(0, yPos, 0), buildingSize);
    }

    void CreateWallRing(Vector3 position, Vector3 size)
    {
        // ������������ ������� ��� ����
        float halfX = size.x;
        float halfZ = size.z;

        // ������� ����� ��� ������ �������
        CreateWallSegment(new Vector3(-halfX, position.y, 0), 90, size.z);  // ��������
        //CreateWallSegment(new Vector3(halfX, position.y, 0), -90, size.z);   // ���������
        //CreateWallSegment(new Vector3(0, position.y, -halfZ), 0, size.x);   // ��������
        //CreateWallSegment(new Vector3(0, position.y, halfZ), 180, size.x);   // �����
    }

    void CreateWallSegment(Vector3 position, float rotation, float length)
    {
        int wallsCount = Mathf.CeilToInt(length);

        for (int i = 0; i < wallsCount; i++)
        {
            GameObject wall = Instantiate(
                wallPrefab,
                position + new Vector3(0, 0, i - length),
                Quaternion.Euler(0, rotation, 0),
                currentBuilding
            );

            //// ��� ������ - ��������� ������ ���� � ������ �������
            //if (floorNumber == 0 && Random.value < 0.2f)
            //{
            //    ReplaceWithDoor(wall);
            //}
        }
    }

    void ReplaceWithDoor(GameObject wallSegment)
    {
        Destroy(wallSegment);
        Instantiate(
            doorWallPrefab,
            wallSegment.transform.position,
            wallSegment.transform.rotation,
            currentBuilding
        );
    }

    void CreateRoof()
    {
        if (roofPrefabs.Length == 0) return;

        Vector3 roofPosition = new Vector3(
            0,
            floorsCount * wallHeight + 0.2f,
            0
        );

        GameObject roof = Instantiate(
            roofPrefabs[Random.Range(0, roofPrefabs.Length)],
            roofPosition,
            Quaternion.identity,
            currentBuilding
        );

        // ��������� ������ ����� ��� ������
        roof.transform.localScale = new Vector3(
            buildingSize.x,
            1,
            buildingSize.z
        );
    }

    [ContextMenu("Destroy Building")]
    public void DestroyBuilding()
    {
        if (currentBuilding != null)
            DestroyImmediate(currentBuilding.gameObject);
    }
}