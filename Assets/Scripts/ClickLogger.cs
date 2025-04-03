using UnityEngine;

public class ClickLogger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Проверяем нажатие левой кнопки мыши
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Создаем луч из позиции мыши
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) // Проверяем, попал ли луч в объект
            {
                Debug.Log($"Клик по объекту: {hit.collider.gameObject.name}");
            }
            else
            {
                Debug.Log("Клик не попал ни в один объект");
            }
        }
    }
}
