using System;
using UnityEngine;

#nullable enable

public class Player : MonoBehaviour
{
    private int amoutOfSpeakers;
    public event Action<int>? AmountOfSpeakersChanged;
    private static Camera cam;
    private GameObject obj;
    private MovableObject movableObj = null;

    void Start()
    {
        amoutOfSpeakers = 5;
        PlayerInstanceManager.Player = this;
        cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) == true)
        {
            RaycastHit hit;
            if (CameraToMouseRay(out hit))
            {
                GameObject targetHit = hit.transform.gameObject;

                if (targetHit.tag != "Movable") { return; }

                movableObj = targetHit.GetComponent<MovableObject>();
                movableObj.SetSelected();
            }
        }
        else if (Input.GetMouseButtonUp(1) == true && movableObj != null)
        {
            movableObj.SetSelected();
            movableObj = null;
        }
        else if (Input.GetKeyDown(KeyCode.Mouse0) == true)
        {
            RaycastHit hit;
            if (CameraToMouseRay(out hit))
            {
                GameObject targetHit = hit.transform.gameObject;
                Vector3 hitPos = hit.point;

                if (targetHit != null) {
                    hitPos = hitPos + Vector3.up * obj.transform.localScale.y / 2;
                    Instantiate(obj, hitPos, Quaternion.identity);

                }

                movableObj = targetHit.GetComponent<MovableObject>();
                movableObj.SetSelected();
            }
        }
    }

    void FixedUpdate()
    {
    }

    void ChangeAmountOfSpeakers(int newAmount) {
        if (amoutOfSpeakers == newAmount) return;
        amoutOfSpeakers = newAmount;
        AmountOfSpeakersChanged?.Invoke(newAmount);
    }

    public static bool CameraToMouseRay(out RaycastHit hit) {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hit);
    }
}