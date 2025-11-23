using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;

/// <summary>
/// Represents material of surface
/// </summary>
[CreateAssetMenu(fileName = "AcousticMaterial", menuName = "Acoustics/AcousticMaterial", order = 10)]

public class AcousticMaterial : ScriptableObject
{
    [Header("Material of walls")]
    [SerializeField] private string materialName;

    //[SerializeField] private Dictionary<float, float> absorptionCoefficient;
    
    // 6–8 pasm oktawowych
    public float[] absorptionBands = new float[6] { 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f}; // α(f)
    public bool clampToZeroOne = true;

    //public WallMaterial(string name, float absorptionCoefficient)
    //{
    //    this.name = name;
    //    this.absorptionCoefficient = absorptionCoefficient;
    //}

    //public float GetAbsorption(int band) =>
    //    absorptionBands[Mathf.Clamp(band, 0, absorptionBands.Length - 1)];

    //public float GetReflection(int band) =>
    //    Mathf.Sqrt(1f - GetAbsorption(band));

    public float GetAbsorptionByIndex(int bandIndex)
    {
        if (absorptionBands == null || absorptionBands.Length == 0)
            return 0f;

        int idx = Mathf.Clamp(bandIndex, 0, absorptionBands.Length - 1);
        return absorptionBands[idx];
    }

    public float GetAbsorptionForFrequency(float freqHz)
    {
        if (absorptionBands == null || absorptionBands.Length == 0)
            return 0f;

        int n = absorptionBands.Length;
        int[] bands = (n == 8) ? Default8Bands : Default6Bands;

        if (n != 6 && n != 8)
        {
            // fallback: nearest index
            int idx = Mathf.RoundToInt((n - 1) * (Mathf.Log(freqHz) / Mathf.Log(2000f)));
            idx = Mathf.Clamp(idx, 0, n - 1);
            return absorptionBands[idx];
        }

        // if freq is below first or above last, clamp
        if (freqHz <= bands[0]) return absorptionBands[0];
        if (freqHz >= bands[n - 1]) return absorptionBands[n - 1];

        // find neighboring bands for interpolation
        for (int i = 0; i < n - 1; i++)
        {
            int f0 = bands[i];
            int f1 = bands[i + 1];
            if (freqHz >= f0 && freqHz <= f1)
            {
                float t = (freqHz - f0) / (float)(f1 - f0);
                return Mathf.Lerp(absorptionBands[i], absorptionBands[i + 1], t);
            }
        }

        return absorptionBands[n - 1];
    }

    /// <summary>
    /// Zwraca współczynnik odbicia R = sqrt(1 - α) dla danego indeksu pasma.
    /// </summary>
    public float GetReflectionByIndex(int bandIndex)
    {
        float a = GetAbsorptionByIndex(bandIndex);
        return Mathf.Sqrt(Mathf.Clamp01(1f - a));
    }

    public float GetReflectionForFrequency(float freqHz)
    {
        float a = GetAbsorptionForFrequency(freqHz);
        return Mathf.Sqrt(Mathf.Clamp01(1f - a));
    }

    public string Name => name;

    public static readonly int[] Default6Bands = new int[] { 125, 250, 500, 1000, 2000, 4000 };
    public static readonly int[] Default8Bands = new int[] { 63, 125, 250, 500, 1000, 2000, 4000, 8000 };

    private void OnValidate()
    {
        if (clampToZeroOne && absorptionBands != null)
        {
            for (int i = 0; i < absorptionBands.Length; i++)
                absorptionBands[i] = Mathf.Clamp01(absorptionBands[i]);
        }
    }
}