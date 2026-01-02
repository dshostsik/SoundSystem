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

    public IReadOnlyDictionary<string, Speaker> CreatedSpeakers => createdSpeakers;

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
        // IDK why it returned the list of speakers. we did not use it at all,
        // so I decided to make it void
        ClearExistingSpeakers();

        // TODO delete when complete
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
            
            Renderer waveRenderer = wave.GetComponent<Renderer>();

            WaveVisualizerFactory.Visualizer.Renderer = waveRenderer;
            // waveVisualizer.Amplitude = 0;
            // waveVisualizer.Frequency = 0;
            // waveVisualizer.Speed = 0;
            
            //obj.tag = "Movable";
            //obj.AddComponent<MovableObject>();

            Speaker sp = obj.GetComponent<Speaker>();
            sp.channelName = config.channelNames[i];

            createdSpeakers.Add(sp.channelName, sp);
        }

        //return createdSpeakers;
    }

    /// <summary>
    /// Usuwa poprzedni system, jeœli istnieje.
    /// </summary>
    public void ClearExistingSpeakers()
    {
        foreach (var sp in createdSpeakers.Values)
            if (sp != null) Destroy(sp.gameObject);

        createdSpeakers.Clear();
    }

    public void Build51()
    {
        CreateSystem(config51);
        // Do we really need this here?
        //RoomAcousticsManager.Instance.RunSimulation();
    }

    public void Build71()
    {
        CreateSystem(config71);
        //RoomAcousticsManager.Instance.RunSimulation();
    }

    public void Build91()
    {
        CreateSystem(config91);
        //RoomAcousticsManager.Instance.RunSimulation();
    }
}
