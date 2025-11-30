using System.Collections.Generic;
using UnityEngine;

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

    [Header("Available system configs")]
    public SurroundSystemConfig config51;
    public SurroundSystemConfig config71;
    public SurroundSystemConfig config91;

    private List<Speaker> createdSpeakers = new List<Speaker>();

    public IReadOnlyList<Speaker> CreatedSpeakers => createdSpeakers;

    /// <summary>
    /// Tworzy system surround zgodnie z podanym configiem.
    /// </summary>
    /// 
    private void Start()
    {
        Debug.Log("dedede");
    }
    public List<Speaker> CreateSystem(SurroundSystemConfig config)
    {
        ClearExistingSpeakers();

        for (int i = 0; i < config.defaultPositions.Length; i++)
        {
            GameObject obj = Instantiate(
                speakerPrefab,
                config.defaultPositions[i],
                Quaternion.Euler(config.defaultRotations.Length > i ? config.defaultRotations[i] : Vector3.zero)
            );

            obj.tag = "Movable";
            obj.AddComponent<MovableObject>();

            Speaker sp = obj.GetComponent<Speaker>();
            sp.channelName = config.channelNames[i];

            createdSpeakers.Add(sp);
        }

        return createdSpeakers;
    }

    /// <summary>
    /// Usuwa poprzedni system, jeœli istnieje.
    /// </summary>
    public void ClearExistingSpeakers()
    {
        foreach (var sp in createdSpeakers)
            if (sp != null) Destroy(sp.gameObject);

        createdSpeakers.Clear();
    }

    public void Build51()
    {
        CreateSystem(config51);
        RoomAcousticsManager.Instance.RunSimulation();
    }

    public void Build71()
    {
        CreateSystem(config71);
        RoomAcousticsManager.Instance.RunSimulation();
    }

    public void Build91()
    {
        CreateSystem(config91);
        RoomAcousticsManager.Instance.RunSimulation();
    }
}
