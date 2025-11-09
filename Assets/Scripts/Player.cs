using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Rigidbody rb;
    private Vector2 moveSpeed;

    private Vector2 panningSpeed;
    [SerializeField]
    private float sensitivity = 2.5f;

    private float pitch;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnMove(InputValue value)
    {
        moveSpeed = value.Get<Vector2>();
    }

    void OnLook(InputValue value) 
    {
        panningSpeed = value.Get<Vector2>();
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, panningSpeed.x * sensitivity * Time.deltaTime);
        transform.Rotate(Vector3.right, -panningSpeed.y * sensitivity * Time.deltaTime);

        pitch -= panningSpeed.y * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -89f, 89f);
        transform.localEulerAngles = new Vector3(pitch, transform.localEulerAngles.y, 0f);
        //Camera.main.transform.Rotate(Vector3.right, -panningSpeed.y * sensitivity * Time.deltaTime);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(moveSpeed.x, 0, moveSpeed.y);
    }
}