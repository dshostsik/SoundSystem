using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

#nullable enable

public class Player : MonoBehaviour
{
    private int amountOfSpeakers;
    public event Action<int>? AmountOfSpeakersChanged;
    private static Camera _cam;
    private FreeCamera freeCamComponent;
    private MovableObject? movableObj;
    private MovableObject? rotatingObj;



    void Start()
    {
        amountOfSpeakers = 5;
        _cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        freeCamComponent = _cam.GetComponent<FreeCamera>();
    }

    public void OnDrag(InputValue value)
    {

        if (value.isPressed)
        {
            Drag();
        } else {
            ReleaseMoving();
        }
            
    }

    private void ReleaseMoving()
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

    public void OnRotateSpeaker(InputValue value)
    {
        if (value.isPressed)
        {
            Rotate();
        } else
        {
            ReleaseRotating();
        }
    }

    private void Rotate()
    {
        // start rotating object under cursor (do not toggle selection)
        if (CameraToMouseRay(out RaycastHit hit))
        {
            GameObject targetHit = hit.transform.gameObject;
            if (!targetHit.CompareTag("Movable")) return;

            rotatingObj = targetHit.GetComponent<MovableObject>();

            rotatingObj.SetRotating();
        }
    }

    private void ReleaseRotating()
    {
        if (rotatingObj == null) return;
        rotatingObj.SetRotating();
        rotatingObj = null;
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