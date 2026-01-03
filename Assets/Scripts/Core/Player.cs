using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

#nullable enable

public class Player : MonoBehaviour
{
    private int amountOfSpeakers;
    public event Action<int>? AmountOfSpeakersChanged;
    private static Camera? _cam;
    private FreeCamera freeCamComponent;
    //private GameObject obj;
    private MovableObject? movableObj;
    //public Vector3 Position => transform.position;
    //public Vector3 Forward => transform.forward;


    void Start()
    {
        amountOfSpeakers = 5;
        PlayerInstanceManager.Player = this;
        _cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        freeCamComponent = _cam.GetComponent<FreeCamera>();
    }

    void Update()
    {

    }

    public void OnDrag(InputValue value)
    {

        if (value.isPressed)
        {
            Drag();
        } else {
            Release();
        }
            
    }

    private void Release()
    {
        if (movableObj == null) return;
        movableObj.SetSelected();
        movableObj = null;
    }

    private void Drag()
    {   
        if (CameraToMouseRay(out RaycastHit hit))
        {
            GameObject targetHit = hit.transform.gameObject;

            if (!targetHit.CompareTag("Movable")) { return; }

            movableObj = targetHit.GetComponent<MovableObject>();
            movableObj.SetSelected();
        }
    }


    void FixedUpdate()
    {

    }

    void ChangeAmountOfSpeakers(int newAmount) {
        if (amountOfSpeakers == newAmount) return;
        amountOfSpeakers = newAmount;
        AmountOfSpeakersChanged?.Invoke(newAmount);
    }

    public static bool CameraToMouseRay(out RaycastHit hit) {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = _cam.ScreenPointToRay(mousePos);
        return Physics.Raycast(ray, out hit);
    }

    public void OnCollisionEnter(Collision other)
    {
        Debug.LogError("Collision happened");
        
        if (other.gameObject.CompareTag("RoomParts"))
        {
            Debug.LogError("Collision with room part"); 
            // Make camera stop
            // freeCamComponent
        }
    }
}