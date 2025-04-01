using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private Transform cameraTransform;
    
    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isGrounded;
    private float originalHeight;
    private bool isCrouching;
    private CapsuleCollider capsuleCollider;
    private Vector3 savedVelocity; // Сохраненная скорость для невесомости
    private GravityController gravityController;
    private FirstPersonCamera firstPersonCamera;

    // Флаг, чтобы знать, что скорость была только что обновлена
    private bool velocityJustUpdated = false;

    public void ResetSavedVelocity()
    {
        savedVelocity = Vector3.zero;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
        velocityJustUpdated = true;
        Debug.Log("Saved velocity reset");
    }

    public void UpdateSavedVelocity(Vector3 newVelocity)
    {
        savedVelocity = newVelocity;
        if (rb != null)
        {
            rb.linearVelocity = savedVelocity;
        }
        velocityJustUpdated = true;
        Debug.Log($"Saved velocity updated to: {savedVelocity}, magnitude: {savedVelocity.magnitude}");
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponentInChildren<CapsuleCollider>();
        originalHeight = capsuleCollider.height;
        gravityController = GetComponent<GravityController>();
        
        // Ищем камеру, если она не назначена
        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
                // Пытаемся получить компонент FirstPersonCamera
                firstPersonCamera = mainCamera.GetComponent<FirstPersonCamera>();
            }
        }
        else
        {
            firstPersonCamera = cameraTransform.GetComponent<FirstPersonCamera>();
        }
    }

    // Вызывается из GravityController при переключении гравитации
    private void OnGravityToggle(bool isGravityEnabled)
    {
        if (!isGravityEnabled)
        {
            // При отключении гравитации, сохраняем текущую скорость
            savedVelocity = rb.linearVelocity;
        }
    }

    private void Update()
    {
        // Проверка земли только при включенной гравитации
        isGrounded = rb.useGravity && Physics.Raycast(transform.position, Vector3.down, 1.1f);
        
        // Получаем входные данные для управления
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Вычисляем направление движения относительно камеры
        Vector3 forward = transform.forward; // В FPS используем направление персонажа
        Vector3 right = transform.right;
        
        // Обнуляем вертикальную составляющую, чтобы двигаться только в горизонтальной плоскости
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        moveDirection = (forward * verticalInput + right * horizontalInput).normalized;

        // Jump только при включенной гравитации и на земле
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Crouch
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (!isCrouching)
            {
                capsuleCollider.height = crouchHeight;
                isCrouching = true;
                
                // Если используем камеру от первого лица, смещаем её вниз
                if (firstPersonCamera != null)
                {
                    // Можно добавить логику для смещения камеры вниз при приседании
                }
            }
            else
            {
                capsuleCollider.height = originalHeight;
                isCrouching = false;
                
                // Возвращаем камеру в исходное положение
                if (firstPersonCamera != null)
                {
                    // Можно добавить логику для возврата камеры в исходное положение
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Корректируем сохраненную скорость при столкновении с стенами
        if (!rb.useGravity)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                float angle = Vector3.Angle(contact.normal, Vector3.up);
                if (angle > 45f)
                {
                    Vector3 normalVelocity = Vector3.Project(savedVelocity, contact.normal);
                    savedVelocity -= normalVelocity;
                    break;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (rb.useGravity)
        {
            if (isGrounded)
            {
                // На земле с гравитацией - полный контроль
                Vector3 movement = moveDirection * moveSpeed;
                rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
            }
            else
            {
                // В воздухе с гравитацией - НЕТ контроля, только инерция и гравитация
                // Ничего не меняем в скорости, физика сама все сделает
            }
        }
        else
        {
            // В невесомости - только если скорость не была только что обновлена
            if (!velocityJustUpdated)
            {
                rb.linearVelocity = savedVelocity;
            }
            else
            {
                // Сбрасываем флаг обновления скорости
                velocityJustUpdated = false;
            }
        }
    }
}
