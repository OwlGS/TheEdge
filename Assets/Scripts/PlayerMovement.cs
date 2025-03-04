using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private Transform cameraTransform; // Ссылка на камеру
    
    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isGrounded;
    private float originalHeight;
    private bool isCrouching;
    private CapsuleCollider capsuleCollider;
    private Vector3 airVelocity; // Сохраняем скорость в воздухе

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponentInChildren<CapsuleCollider>();
        originalHeight = capsuleCollider.height;
        
        // Если камера не назначена, найти главную камеру
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        // Ground check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
        
        // Movement input только если на земле
        if (isGrounded)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            // Преобразуем ввод в направление относительно камеры
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            
            // Обнуляем Y компоненту, чтобы движение было только в горизонтальной плоскости
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            moveDirection = (forward * verticalInput + right * horizontalInput).normalized;
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // Сохраняем текущую горизонтальную скорость перед прыжком
            airVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
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

    private void FixedUpdate()
    {
        if (isGrounded)
        {
            // Применяем движение только на земле
            Vector3 movement = moveDirection * moveSpeed;
            rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
            // Обновляем сохраненную скорость в воздухе
            airVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        }
        else
        {
            // В воздухе сохраняем горизонтальную скорость
            rb.linearVelocity = new Vector3(airVelocity.x, rb.linearVelocity.y, airVelocity.z);
        }
    }
}
