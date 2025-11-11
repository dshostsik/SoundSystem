using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class conatining and managing speakers
/// </summary>
public class SpeakerContainer : MonoBehaviour
{
    private static List<Speaker> speakers;

    /// <summary>
    /// Container of speakers
    /// </summary>
    public static List<Speaker> Speakers
    {
        get => speakers;
    }

    /// <summary>
    /// Prepare container to prevent race condition while executing Start() on Speaker
    /// </summary>
    private void Awake()
    {
        //Default value for 5.1
        speakers = new(6);
    }

    void Start() {
        PlayerInstanceManager.Player.AmountOfSpeakersChanged += OnSpeakerAmountChanged;
    }

    //Event delegate in case the GUI tells to update amount of speakers
    void OnSpeakerAmountChanged(int amount) {
        int difference = amount - speakers.Count;

        if (difference > 0)
        {
            speakers.Capacity = amount;
            return;
        }

        for (int i = 0; i < -difference; i++) {
            speakers.RemoveAt(speakers.Count - 1);
        }

        speakers.Capacity = amount;
    }
}