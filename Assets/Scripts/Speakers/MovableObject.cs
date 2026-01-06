using UnityEngine;

public class MovableObject : MonoBehaviour
{
    private bool selected;
    private Vector3 startingPos;
    private Collider col;

    private Listener? listener;
    private Vector3 lastPosition;
    private float lastSimTime;

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider>();
        selected = false;
        startingPos = transform.position;

        listener = GetComponent<Listener>();
        lastPosition = transform.position;
        lastSimTime = -Mathf.Infinity;
    }

    // Update is called once per frame
    void Update()
    {
        if (selected)
        {
            RaycastHit hit;
            if (Player.CameraToMouseRay(out hit))
            {
                transform.position = hit.point;
            }

            if (!listener) return;
            
            // wywo³anie co 0.1s tylko gdy pozycja siê zmieni³a znacz¹co
            if (!((transform.position - lastPosition).sqrMagnitude > 0.0001f) ||
                !(Time.time - lastSimTime > 0.1f)) return;
            
            lastSimTime = Time.time;
            RoomAcousticsManager.Instance?.RunSimulation();
            lastPosition = transform.position;
        }
    }

    public void SetSelected()
    {
        selected = !selected;
        if (!selected)
        {
            if (!Physics.CheckSphere(transform.position, RoomAcousticsManager.Instance.room.height * 0.95f))
            {
                transform.position = startingPos;
            }
            else
            {
                startingPos = transform.position;
            }

            if (listener != null)
            {
                RoomAcousticsManager.Instance?.RunSimulation();
                lastPosition = transform.position;
                lastSimTime = Time.time;
            }
        }

        col.enabled = !col.enabled;
    }
}