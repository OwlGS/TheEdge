using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 0.8f, 0);
    [SerializeField] private float maxUpAngle = 80f;
    [SerializeField] private float maxDownAngle = 80f;

    private float rotationX = 0;
    private float rotationY = 0;
    
    // Получаем настройки чувствительности из PlayerPrefs
    private void Start()
    {
        if (playerTransform == null)
        {
            // Если игрок не назначен, пытаемся найти его в сцене
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("FirstPersonCamera: Player transform not assigned and player tag not found!");
            }
        }
        
        // Устанавливаем начальную позицию и вращение
        UpdateCameraPosition();
        
        // Получаем сохраненную чувствительность мыши
        float sensitivity = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
        mouseSensitivity *= sensitivity;
        
        // Скрываем и блокируем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;
        
        // Получаем движение мыши
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Поворачиваем камеру вверх/вниз
        rotationX -= mouseY;
        // Ограничиваем угол поворота, чтобы не переворачиваться
        rotationX = Mathf.Clamp(rotationX, -maxDownAngle, maxUpAngle);
        
        // Поворачиваем персонажа влево/вправо
        rotationY += mouseX;
        
        // Применяем вращение к камере (вверх/вниз)
        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
        
        // Применяем вращение к игроку только по горизонтали
        playerTransform.rotation = Quaternion.Euler(0, rotationY, 0);
        
        // Обновляем позицию камеры
        UpdateCameraPosition();
    }
    
    private void UpdateCameraPosition()
    {
        if (playerTransform == null) return;
        
        // Устанавливаем камеру в позицию "глаз" игрока
        transform.position = playerTransform.position + cameraOffset;
    }
    
    // Метод для установки чувствительности извне
    public void SetSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity * 2.0f; // Базовая чувствительность умножается на настройку
    }
} 