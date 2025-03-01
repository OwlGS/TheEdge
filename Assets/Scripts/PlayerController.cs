using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private LayerMask groundLayer; // Слои, считающиеся землёй
    [SerializeField] private float groundCheckDistance = 0.3f; // Расстояние проверки земли
    
    [Header("Components")]
    [SerializeField] private Transform cameraHolder;
    
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private float originalHeight;
    private bool isCrouching = false;
    private bool isGrounded;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        originalHeight = capsuleCollider.height;
        
        // Блокируем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Проверка земли
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        // Получаем ввод
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Создаем вектор движения относительно поворота камеры
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        moveDirection = moveDirection.normalized;

        // Применяем движение
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.deltaTime);

        // Останавливаем горизонтальное движение, если нет ввода
        if (horizontal == 0 && vertical == 0)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.x = 0;
            velocity.z = 0;
            rb.linearVelocity = velocity;
        }

        // Прыжок
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Приседание
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C))
        {
            if (!isCrouching)
            {
                capsuleCollider.height = crouchHeight;
                isCrouching = true;
            }
        }
        if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.C))
        {
            if (isCrouching)
            {
                capsuleCollider.height = originalHeight;
                isCrouching = false;
            }
        }
    }
} 