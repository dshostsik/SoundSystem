using UnityEngine;

public class Speaker : MonoBehaviour
{

    //[SerializeField] private new string name;
    //[SerializeField] private string model;

    //public string Name => name;
    //public string Model => model;
    
    [Header("Physical parameters")]
    public string channelName; // L, R, C, (LS, RS), LB, RB, SUB
    public float baseLevel = 1f;

    public float soundPressureLevel; //SPL
    private AnimationCurve directivity = AnimationCurve.Linear(0, 1, 180, 0.1f);

    //[SerializeField] private Directivity directivity = Directivity.Monopole;
    public float[] testSignal; // s(t)

    public Vector3 Position => transform.position;
    public Vector3 Forward => transform.forward;

    //public float BaseLevel
    //{
    //    get => baseLevel;
    //    set => baseLevel = value >= 0 ? value : -value;
    //}
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }




    public float GetDirectivityGain(Vector3 listenerPos)
    {
        Vector3 toListener = (listenerPos - Position).normalized;
        float angle = Vector3.Angle(Forward, toListener);
        return directivity.Evaluate(angle);
    }

    /// <summary>
    /// Metody do aktualizacji parametrów przez UI.
    /// </summary>
    public void SetBaseLevel(float newLevel)
    {
        baseLevel = newLevel;
        RoomAcousticsManager.Instance.RunSimulation();
    }

    public void SetDirectivityCurve(AnimationCurve curve)
    {
        directivity = curve;
        RoomAcousticsManager.Instance.RunSimulation();
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
        RoomAcousticsManager.Instance.RunSimulation();
    }

    public void SetRotation(Quaternion rot)
    {
        transform.rotation = rot;
        RoomAcousticsManager.Instance.RunSimulation();
    }
}
