using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Mouse Settings")]
    [SerializeField] private float sensitivity = 2.5f;
    private float pitch = 0f;

    [Header("Camera")]
    [SerializeField] private Camera playerCamera;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnLook(InputValue value) 
    {
        lookInput = value.Get<Vector2>();
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, lookInput.x * sensitivity * Time.deltaTime);

        pitch -= lookInput.y * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
    }

    void FixedUpdate()
    {
        // Ruch względem kierunku patrzenia
        Vector3 forward = transform.forward * moveInput.y;
        Vector3 right = transform.right * moveInput.x;
        Vector3 move = (forward + right).normalized * moveSpeed;

        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }
}