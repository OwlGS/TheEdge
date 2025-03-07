using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityController : MonoBehaviour
{
    private Rigidbody rb;
    private bool isGravityEnabled = true;
    
    public bool IsGravityEnabled => isGravityEnabled;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleGravity();
        }
    }

    private void ToggleGravity()
    {
        isGravityEnabled = !isGravityEnabled;
        rb.useGravity = isGravityEnabled;
        
        // Сообщаем системе, что произошло переключение гравитации
        SendMessage("OnGravityToggle", isGravityEnabled, SendMessageOptions.DontRequireReceiver);
    }
}
