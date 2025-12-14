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
                transform.position = hit.point + Vector3.up * transform.localScale.y / 2;
            }

            if (listener != null)
            {
                // wywo³anie co 0.1s tylko gdy pozycja siê zmieni³a znacz¹co
                if ((transform.position - lastPosition).sqrMagnitude > 0.0001f && Time.time - lastSimTime > 0.1f)
                {
                    lastSimTime = Time.time;
                    RoomAcousticsManager.Instance?.RunSimulation();
                    lastPosition = transform.position;
                }
            }
        }

    }

    public void SetSelected()
    {
        selected = !selected;
        if (selected == false)
        {
            if (Physics.CheckSphere(transform.position, transform.localScale.y / 2.1f))
            {
                transform.position = startingPos;
            }
            else
            {
                startingPos = transform.position;
            }

            // po zakoñczeniu przesuwania — jeœli to Listener, natychmiastowo uruchamiamy pe³n¹ symulacjê
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