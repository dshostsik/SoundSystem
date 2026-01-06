using UnityEngine;

public class MovableObject : MonoBehaviour
{
    private bool selected;
    private Vector3 startingPos;
    private Collider col;

    private Listener? listener;
    private Vector3 lastPosition;
    private float lastSimTime;

    public float rotationSpeed = 180f;
    public bool snapWithShift = true;
    public float snapAngle = 15f;

    private bool rotating;
    private float lastRotationY;

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider>();
        selected = false;
        startingPos = transform.position;

        listener = GetComponent<Listener>();
        lastPosition = transform.position;
        lastSimTime = -Mathf.Infinity;

        rotating = false;
        lastRotationY = transform.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (rotating)
        {
            // projektujemy ray myszy na p³aszczyznê poziom¹ przechodz¹c¹ przez pozycjê obiektu
            if (Player.CameraToMouseRay(out RaycastHit hit))
            {
                Vector3 hitPoint = hit.point;
                Vector3 dir = hitPoint - transform.position;
                dir.y = 0f; // tylko obrót wokó³ Y

                if (dir.sqrMagnitude > 1e-6f)
                {
                    Quaternion target = Quaternion.LookRotation(dir.normalized, Vector3.up);

                    // p³ynna rotacja do celu:
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotationSpeed * Time.deltaTime);

                    //// snap gdy przytrzymany Shift (zmienia natychmiastowo do najbli¿szego k¹ta)
                    //if (snapWithShift && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                    //{
                    //    Vector3 e = transform.eulerAngles;
                    //    float snappedY = Mathf.Round(e.y / snapAngle) * snapAngle;
                    //    transform.eulerAngles = new Vector3(e.x, snappedY, e.z);
                    //}

                    // debounce symulacji podczas rotacji (tylko jeœli obiekt to Listener)
                    if (listener != null && Time.time - lastSimTime > 0.1f)
                    {
                        float currentY = transform.eulerAngles.y;
                        if (Mathf.Abs(Mathf.DeltaAngle(currentY, lastRotationY)) > 0.5f)
                        {
                            lastRotationY = currentY;
                            lastSimTime = Time.time;
                            RoomAcousticsManager.Instance?.RunSimulation();
                        }
                    }
                }
            }

            return;
        }

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

    public void SetRotating()
    {
        rotating = !rotating;
        if (!rotating)
        {
            // zakoñczenie rotacji — jeœli obiekt jest listenerem, uruchom pe³n¹ symulacjê
            if (listener != null)
            {
                RoomAcousticsManager.Instance?.RunSimulation();
                lastRotationY = transform.eulerAngles.y;
                lastSimTime = Time.time;
            }
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