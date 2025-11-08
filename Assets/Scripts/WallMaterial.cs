using UnityEngine;

/// <summary>
/// Represents material of wall
/// </summary>
public class WallMaterial : MonoBehaviour
{
    [Header("Material of walls")]
    [SerializeField] private string name;
    [SerializeField] private float absorptionCoefficient;

    public WallMaterial(string name, float absorptionCoefficient)
    {
        this.name = name;
        this.absorptionCoefficient = absorptionCoefficient;
    }

    public string Name => name;
    public float AbsorptionCoefficient => absorptionCoefficient;
}