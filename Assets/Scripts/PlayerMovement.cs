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

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponentInChildren<CapsuleCollider>();
        originalHeight = capsuleCollider.height;
        gravityController = GetComponent<GravityController>();
        
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
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
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        
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
            }
            else
            {
                capsuleCollider.height = originalHeight;
                isCrouching = false;
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
                // На земле с гравитацией - обычное управление
                Vector3 movement = moveDirection * moveSpeed;
                rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
            }
            else
            {
                // В воздухе с гравитацией - сохраняем горизонтальную инерцию, но можем немного управлять
                Vector3 movement = moveDirection * (moveSpeed * 0.2f); // Уменьшенный контроль в воздухе
                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x + movement.x * Time.fixedDeltaTime,
                    rb.linearVelocity.y,
                    rb.linearVelocity.z + movement.z * Time.fixedDeltaTime
                );
            }
        }
        else
        {
            // В невесомости - сохраняем инерцию и добавляем немного управления
            if (moveDirection != Vector3.zero)
            {
                // Небольшая сила для маневрирования в невесомости
                rb.AddForce(moveDirection * (moveSpeed * 0.5f), ForceMode.Acceleration);
                
                // Ограничиваем максимальную скорость
                if (rb.linearVelocity.magnitude > moveSpeed * 1.5f)
                {
                    rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed * 1.5f;
                }
            }
            else
            {
                // Если не управляем, сохраняем существующую скорость
                rb.linearVelocity = savedVelocity;
            }
            
            // Обновляем сохраненную скорость
            savedVelocity = rb.linearVelocity;
        }
    }
}
