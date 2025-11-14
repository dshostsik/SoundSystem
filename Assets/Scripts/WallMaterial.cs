using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents material of wall
/// </summary>
public class WallMaterial : MonoBehaviour
{
    [Header("Material of walls")]
    [SerializeField] private new string name;
    [SerializeField] private Dictionary<float, float> absorptionCoefficient;

    //public WallMaterial(string name, float absorptionCoefficient)
    //{
    //    this.name = name;
    //    this.absorptionCoefficient = absorptionCoefficient;
    //}

    public string Name => name;
    //public float AbsorptionCoefficient => absorptionCoefficient;
}