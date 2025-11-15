using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Najwa¿niejsza klasa steruj¹ca symulacj¹.
/// Odpowiada za:
/// - Zebranie danych wejœciowych (g³oœniki, s³uchacz, pokój),
/// - Uruchomienie raytracingu,
/// - Wyliczenie transfer function H(f),
/// - IFFT ? wygenerowanie odpowiedzi impulsowej h(t),
/// - Splot sygna³u g³oœnika z h(t),
/// - Aktualizacjê wizualizacji.
///
/// Klasa jest Singletonem (Instance) dla wygody.
/// </summary>
public class RoomAcousticsManager : MonoBehaviour
{
    public static RoomAcousticsManager Instance { get; private set; }

    [Header("References")]
    public Room room;
    public Player listener;
    public SurroundSystemFactory systemFactory;
    public AcousticVisualizer visualizer;

    [Header("Engine modules")]
    public RayAcousticTracer tracer = new RayAcousticTracer();
    public ImpulseResponseGenerator irGenerator = new ImpulseResponseGenerator();
    public AudioConvolver convolver = new AudioConvolver();
    public FourierAnalyzer fourier = new FourierAnalyzer();

    [Header("Simulation settings")]
    public float speedOfSound = 343.0f;
    public int fftSize = 4096;
    public float sampleRate = 44100.0f;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// G³ówna metoda symulacji.  
    /// 1. Pobiera listê g³oœników
    /// 2. Oblicza wszystkie œcie¿ki akustyczne
    /// 3. Wylicza H(f)
    /// 4. IFFT ? h(t)
    /// 5. (opcjonalnie) splot sygna³u testowego
    /// 6. Aktualizuje wizualizacjê
    /// </summary>
    public void RunSimulation()
    {
        var speakers = systemFactory.CreatedSpeakers;

        if (speakers.Count == 0 || listener == null || room == null)
        {
            Debug.LogWarning("Simulation aborted: missing speakers, listener or room.");
            return;
        }

        // 1. Geometria / tor akustyczny
        List<AcousticPath> allPaths = tracer.ComputePaths(room, speakers, listener);

        // 2. Transfer function H(f)
        var H = irGenerator.ComputeTransferFunction(allPaths, fftSize, sampleRate);

        // 3. h(t) = IFFT(H(f))
        var h = irGenerator.GenerateImpulseResponse(H);

        // 4. (opcjonalnie) wymuœ splot z sygna³em któregoœ g³oœnika
        Speaker referenceSpeaker = speakers[0];
        float[] referenceSignal = referenceSpeaker.testSignal;
        float[] outputSignal = null;

        if (referenceSignal != null && referenceSignal.Length > 0)
            outputSignal = convolver.Convolve(referenceSignal, h);

        // 5. Wizualizacja
        if (visualizer != null)
        {
            visualizer.VisualizeImpulseResponse(h);
            visualizer.VisualizeSoundField(allPaths);
        }

        Debug.Log($"Simulation complete: paths={allPaths.Count}, IR length={h.Length}, system={speakers.Count} speakers");
    }
}
