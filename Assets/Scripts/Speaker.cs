using UnityEngine;

public class Speaker : MonoBehaviour
{

    [Header("Identification of speaker")]
    [SerializeField] private new string name;
    [SerializeField] private string model;

    public string Name => name;
    public string Model => model;
    
    [Header("Physical parameters")]
    [SerializeField] private float power;
    [SerializeField] private float soundAbsorptionCoefficient;
    [SerializeField] private Directivity directivity = Directivity.Monopole;
    // TODO: Add more parameters
    
    public float Power
    {
        get => power;
        set => power = value >= 0 ? value : -value;
    }

    private new Transform transform;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
