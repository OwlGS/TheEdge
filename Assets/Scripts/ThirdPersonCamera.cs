using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 2f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    private void LateUpdate()
    {
        if (target == null) return;

        // Получаем ввод мыши
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // Вращаем камеру вверх-вниз
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        // Вращаем персонажа влево-вправо
        rotationY += mouseX;
        target.rotation = Quaternion.Euler(0f, rotationY, 0f);

        // Позиционируем камеру
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
        Vector3 targetPosition = target.position;
        targetPosition.y += height;

        Vector3 position = targetPosition - (rotation * Vector3.forward * distance);
        transform.position = position;
        transform.LookAt(targetPosition);
    }
} 