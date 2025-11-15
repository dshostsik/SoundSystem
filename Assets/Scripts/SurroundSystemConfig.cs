using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SurroundSystemConfig", menuName = "Acoustics/SurroundSystemConfig", order = 20)]
public class SurroundSystemConfig
{
    public string systemName; // "5.1", "7.1"
    public Vector3[] defaultPositions;
    public Vector3[] defaultRotations;
    public string[] channelNames; //L, R, C, LS, RS, LB, RB, SUB 

    public SurroundSystemConfig system51 = new SurroundSystemConfig
    {
        systemName = "5.1",
        defaultPositions = new[] {
        new Vector3(-2, 1, 3), // L
        new Vector3(2, 1, 3),  // R
        new Vector3(0, 1, 3),  // C
        new Vector3(-3, 1, -1), // LS
        new Vector3(3, 1, -1),  // RS
        new Vector3(0, 1, 1) // SUB
        }
    };


    public SurroundSystemConfig system71 = new SurroundSystemConfig
    {
        systemName = "7.1",
        defaultPositions = new[] {
            new Vector3(-2, 1, 3),
            new Vector3(2, 1, 3),
            new Vector3(0, 1, 3),
            new Vector3(-3, 1, 1),
            new Vector3(3, 1, 1),
            new Vector3(-3, 1, -2),
            new Vector3(3, 1, -2),
            new Vector3(0, 1, 1)
        }
    };
};