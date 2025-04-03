using UnityEngine;

public class MouseHover : MonoBehaviour
{
    private void Update()
    {
        // 1. Проверяем, нажата ли левая кнопка мыши
        if (Input.GetMouseButtonDown(0))
        {
            // 2. Создаем луч (ray) из позиции мыши
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 3. Проверяем, попал ли луч в объект
            if (Physics.Raycast(ray, out hit))
            {
                // 4. Если мы попали в черную точку (или любой объект, который мы создадим)
                if (hit.collider != null && hit.collider.CompareTag("BlackPoint"))
                {
                    // 5. Получаем координаты точки
                    Vector3 position = hit.point;
                    Debug.Log($"Black point clicked at position: {position.x}, {position.y}, {position.z}");
                }
            }
        }
    }
}
