using UnityEngine;

[System.Serializable]
public class SurroundSystemConfig
{
    public string name;
    public Vector3[] defaultPositions;

    public SurroundSystemConfig system51 = new SurroundSystemConfig {
        name = "5.1",
        defaultPositions = new [] {
            new Vector3(-2, 1, 3), // L
            new Vector3(2, 1, 3),  // R
            new Vector3(0, 1, 3),  // C
            new Vector3(-3, 1, -1), // LS
            new Vector3(3, 1, -1),  // RS
            new Vector3(0, 1, 1) // SUB
        }
    };


    public SurroundSystemConfig system71 = new SurroundSystemConfig {
        name = "7.1",
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

}

