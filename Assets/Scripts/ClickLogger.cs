using UnityEngine;

public class ClickLogger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // ��������� ������� ����� ������ ����
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // ������� ��� �� ������� ����
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) // ���������, ����� �� ��� � ������
            {
                Debug.Log($"���� �� �������: {hit.collider.gameObject.name}");
            }
            else
            {
                Debug.Log("���� �� ����� �� � ���� ������");
            }
        }
    }
}
