using UnityEngine;

public class MouseHover : MonoBehaviour
{
    private void Update()
    {
        // 1. ���������, ������ �� ����� ������ ����
        if (Input.GetMouseButtonDown(0))
        {
            // 2. ������� ��� (ray) �� ������� ����
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 3. ���������, ����� �� ��� � ������
            if (Physics.Raycast(ray, out hit))
            {
                // 4. ���� �� ������ � ������ ����� (��� ����� ������, ������� �� ��������)
                if (hit.collider != null && hit.collider.CompareTag("BlackPoint"))
                {
                    // 5. �������� ���������� �����
                    Vector3 position = hit.point;
                    Debug.Log($"Black point clicked at position: {position.x}, {position.y}, {position.z}");
                }
            }
        }
    }
}
