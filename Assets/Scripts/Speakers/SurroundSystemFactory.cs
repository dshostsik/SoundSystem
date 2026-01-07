using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Visualization;

/// <summary>
/// Tworzy system g³oœników na podstawie konfiguracji SurroundSystemConfig.
/// Obs³uguje 5.1, 7.1 i dowolne inne systemy.
/// Ka¿dy wygenerowany g³oœnik:
/// - Dostaje komponent Speaker,
/// - Dostaje komponent MovableObject,
/// - Dostaje tag "Movable",
/// - Automatycznie uruchamia aktualizacjê symulacji po zmianach.
/// </summary>
public class SurroundSystemFactory : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Prefab g³oœnika. Musi zawieraæ komponent Speaker.")]
    public GameObject speakerPrefab;
    public GameObject wavePrefab;

    [Header("Available system configs")]
    public SurroundSystemConfig config51;
    public SurroundSystemConfig config71;
    public SurroundSystemConfig config91;

    private Dictionary<string, Speaker> createdSpeakers = new();
    private Dictionary<string, WaveVisualizer> createdWaves = new();

    public IReadOnlyDictionary<string, Speaker> CreatedSpeakers => createdSpeakers;
    public IReadOnlyDictionary<string, WaveVisualizer> CreatedWaves => createdWaves;

    /// <summary>
    /// Tworzy system surround zgodnie z podanym configiem.
    /// </summary>
    /// 
    private void Start()
    {
        Debug.Log("dedede");
    }
    
    public void CreateSystem(SurroundSystemConfig config)
    {
        ClearExistingSpeakers();

        if (speakerPrefab == null) throw new System.Exception("Speaker prefab not set!");
        if (speakerPrefab.GetComponent<Speaker>() == null) throw new System.Exception("Speaker prefab does not contain Speaker component!");
        
        for (int i = 0; i < config.defaultPositions.Length; i++)
        {
            GameObject obj = Instantiate(
                speakerPrefab,
                transform.localPosition + config.defaultPositions[i],
                Quaternion.Euler(config.defaultRotations.Length > i ? config.defaultRotations[i] : Vector3.zero)
            );

            GameObject wave = Instantiate(wavePrefab,
                obj.transform.position,
                Quaternion.LookRotation(obj.transform.forward));
            
            // Setup wave material for rendering
            wave.transform.parent = obj.transform;
            wave.transform.rotation = Quaternion.Euler(-90.0f, obj.transform.rotation.y, obj.transform.rotation.z);
            
            BoxCollider bc = obj.GetComponent<BoxCollider>();

            Vector3 worldCenter = obj.transform.TransformPoint(bc.center);
            Vector3 frontPos = worldCenter + obj.transform.forward * ((bc.size.z * 0.5f) * obj.transform.lossyScale.z);
            wave.transform.position = frontPos;
            
            
            
            // waveVisualizer.Amplitude = 0;
            // waveVisualizer.Frequency = 0;
            // waveVisualizer.Speed = 0;
            
            //obj.tag = "Movable";
            //obj.AddComponent<MovableObject>();

            Speaker sp = obj.GetComponent<Speaker>();
            sp.channelName = config.channelNames[i];

            createdSpeakers.Add(sp.channelName, sp);
            
            WaveVisualizer visualizer = sp.GetComponent<WaveVisualizer>();
            visualizer.Renderer = wave.GetComponent<Renderer>();
            createdWaves.Add(sp.channelName, visualizer);
        }
    }

    /// <summary>
    /// Usuwa poprzedni system, jeœli istnieje.
    /// </summary>
    public void ClearExistingSpeakers()
    {
        foreach (var sp in createdSpeakers.Values)
            if (sp != null) Destroy(sp.gameObject);

        createdSpeakers.Clear();
        
        foreach (var wave in createdWaves.Values)
            if (wave != null) Destroy(wave.gameObject);
        
        createdWaves.Clear();
    }

    public void Build51()
    {
        CreateSystem(config51);
    }

    public void Build71()
    {
        CreateSystem(config71);
    }

    public void Build91()
    {
        CreateSystem(config91);
    }
}
