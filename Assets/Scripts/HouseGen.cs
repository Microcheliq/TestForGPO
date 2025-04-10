using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class HouseGen : MonoBehaviour
{
    public Vector2 position;
    public GameObject objectPrefab;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerateHouse()
    {
        Debug.Log($"GenerateHouse()");

        float ran_x = Random.Range(-500f, 500f);
        float ran_z = Random.Range(-500f, 500f);

        objectPrefab.transform.localScale = new Vector3(100, 100, 100);
        Quaternion rotation = Quaternion.Euler(-90, 90, 0);
        GameObject obj = Instantiate(objectPrefab, new Vector3(ran_x, 0, ran_z), rotation);

        obj.name = "TestOBJ";
        obj.tag = "GeneratedObject";
    }

    public void DeleteHouses()
    {
        GameObject[] previousObjects = GameObject.FindGameObjectsWithTag("GeneratedObject");

        Debug.Log($"Deleting {previousObjects.Length} objects");

        foreach (GameObject obj in previousObjects)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(obj);
            }
            else
            {
                Destroy(obj);
            }
        }
    }
}
