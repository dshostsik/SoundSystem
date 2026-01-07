using System;
using System.Collections.Generic;
using System.Linq;
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
    public Listener listener;
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

    private Speaker referenceSpeaker;
    private IReadOnlyDictionary<string, Speaker> speakers;
    
    private Dictionary<string, float> lastPerSpeakerDb = new Dictionary<string, float>(StringComparer.Ordinal);
    private float lastOverallDb;

    public float OverallDb => lastOverallDb;
    
    // referencyjne ciœnienie (Pa) do konwersji do dB (20 µPa)
    private const float ReferencePressure = 20e-6f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        speakers = systemFactory.CreatedSpeakers;
        AcousticUIController.ConfigurationChangedEvent += UpdateSpeakerConfigurationInfo;
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

        if (speakers.Count == 0 || !listener || !room)
        {
            Debug.LogWarning($"Simulation aborted: missing speakers ({speakers.Count} speakers found), listener ({listener == null}) or room ({room == null}).");
            return;
        }

        // 1. Geometria / tor akustyczny
        List<AcousticPath> allPaths = tracer.ComputePaths(room, speakers, listener);

        // 2. Transfer function H(f)
        var H = irGenerator.ComputeTransferFunction(allPaths, fftSize, sampleRate);

        // 3. h(t) = IFFT(H(f))
        var h = irGenerator.GenerateImpulseResponse(H);

        // 4. (opcjonalnie) wymuœ splot z sygna³em któregoœ g³oœnika
        float[] referenceSignal = referenceSpeaker.testSignal;
        float[] outputSignal = null;

        if (referenceSignal != null && referenceSignal.Length > 0)
            outputSignal = convolver.Convolve(referenceSignal, h);

        // 5. Wizualizacja
        if (visualizer)
        {
            visualizer.VisualizeImpulseResponse(h);
            visualizer.VisualizeSoundField(allPaths);
        }

        // 6. Wyliczamy przybli¿one poziomy SPL w punkcie listenera z listy œcie¿ek
        var (perSpeakerDb, overallDb) = ComputeListenerLevelsFromPaths(allPaths);
        lastPerSpeakerDb = perSpeakerDb.ToDictionary(k => k.Key, v => v.Value, StringComparer.Ordinal);
        lastOverallDb = overallDb;

        Debug.Log($"Simulation complete: paths={allPaths.Count}, IR length={h.Length}, system={speakers.Count} speakers");
        Debug.Log($"Listener overall level: {overallDb:F2} dB SPL");
        foreach (var kv in perSpeakerDb)
            Debug.Log($"  {kv.Key}: {kv.Value:F2} dB SPL");
        //Debug.Log($"Simulation complete: paths={allPaths.Count}, IR length={h.Length}, system={speakers.Count} speakers");
    }

    /// <summary>
    /// Method called when speaker configuration changes. Made just for fun and experiments with the Listener/Observer pattern.
    /// </summary>
    private void UpdateSpeakerConfigurationInfo()
    {
        speakers = systemFactory.CreatedSpeakers;
        referenceSpeaker = speakers.First().Value;
    }

    /// <summary>
    /// Przybli¿one wyliczenie poziomów w punkcie listenera na podstawie obliczonych œcie¿ek.
    /// - agregujemy wk³ad ka¿dej œcie¿ki per g³oœnik: amplitude (1/d) * directivity * œredni wspó³czynnik odbicia
    /// - mno¿ymy przez baseLevel g³oœnika
    /// - sumujemy energie (sqrt(sum(p_i^2))) aby uzyskaæ ca³kowite RMS (incoherent sum)
    /// - konwertujemy do dB SPL u¿ywaj¹c 20*log10(p/p_ref)
    /// Uwaga: metoda daje wzglêdne, przybli¿one wyniki. Dok³adnoœæ mo¿na poprawiæ generuj¹c IR per?source i licz¹c RMS h(t).
    /// </summary>
    private (Dictionary<string, float> perSpeakerDb, float overallDb) ComputeListenerLevelsFromPaths(List<AcousticPath> allPaths)
    {
        var speakersDict = systemFactory.CreatedSpeakers;
        var accum = new Dictionary<string, float>(StringComparer.Ordinal);

        // agregacja po g³oœnikach
        foreach (var path in allPaths)
        {
            if (path?.speaker == null) continue;
            string key = path.speaker.channelName ?? "unknown";

            float directivity = 1f;
            try
            {
                directivity = path.speaker.GetDirectivityGain(listener.Position);
            }
            catch
            {
                directivity = 1f;
            }

            float refl = 1f;
            if (path.reflectionPerBand != null && path.reflectionPerBand.Length > 0)
                refl = path.reflectionPerBand.Average();

            float gain = path.amplitude * directivity * refl;

            if (accum.TryGetValue(key, out var existing)) accum[key] = existing + gain;
            else accum[key] = gain;
        }

        // zamieniamy na "ciœnienie" per g³oœnik mno¿¹c przez baseLevel
        var perSpeakerPressure = new Dictionary<string, float>(StringComparer.Ordinal);
        foreach (var kv in accum)
        {
            if (!speakersDict.TryGetValue(kv.Key, out var sp))
            {
                // jeœli brak referencji do obiektu, traktujemy baseLevel = 1
                perSpeakerPressure[kv.Key] = kv.Value;
                continue;
            }

            float baseLevel = sp.baseLevel;
            perSpeakerPressure[kv.Key] = baseLevel * kv.Value;
        }

        // sumujemy energie (RMS) — zak³adamy niezale¿ne Ÿród³a (incoherent sum)
        float sumSquares = 0f;
        foreach (var p in perSpeakerPressure.Values)
            sumSquares += p * p;

        float overallPressure = Mathf.Sqrt(Mathf.Max(0f, sumSquares));

        // konwersja do dB SPL (wybór referencji 20 µPa)
        const float eps = 1e-12f;
        float overallDb = 20f * Mathf.Log10(overallPressure / ReferencePressure + eps);

        var perSpeakerDb = new Dictionary<string, float>(StringComparer.Ordinal);
        foreach (var kv in perSpeakerPressure)
            perSpeakerDb[kv.Key] = 20f * Mathf.Log10(kv.Value / ReferencePressure + eps);

        return (perSpeakerDb, overallDb);
    }

    public void Update()
    {
        // For testing purposes: run simulation on key press
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    RunSimulation();
        //}
    }
    public IReadOnlyDictionary<string, float> GetLastPerSpeakerDb() => lastPerSpeakerDb;
    public float GetLastOverallDb() => lastOverallDb;
}
