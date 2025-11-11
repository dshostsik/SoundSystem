using System;
using UnityEngine;

#nullable enable

public class Player : MonoBehaviour
{
    private int amoutOfSpeakers;
    public event Action<int>? AmountOfSpeakersChanged;

    void Start()
    {
        amoutOfSpeakers = 5;
        PlayerInstanceManager.Player = this;
    }

    private void Update()
    {
    }

    void FixedUpdate()
    {
    }

    void ChangeAmountOfSpeakers(int newAmount) {
        if (amoutOfSpeakers == newAmount) return;
        amoutOfSpeakers = newAmount;
        AmountOfSpeakersChanged?.Invoke(newAmount);
    }
}