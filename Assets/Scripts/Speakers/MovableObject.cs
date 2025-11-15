using UnityEngine;

public class MovableObject : MonoBehaviour
{

    private bool selected;
    private Vector3 startingPos;
    private Collider col;

    // Start is called before the first frame update
    void Start()
    {

        col = GetComponent<Collider>();
        selected = false;
        startingPos = transform.position;
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
        }
        col.enabled = !col.enabled;
    }
}