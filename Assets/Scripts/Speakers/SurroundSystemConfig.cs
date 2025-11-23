using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SurroundSystemConfig", menuName = "Acoustics/SurroundSystemConfig", order = 20)]
public class SurroundSystemConfig : ScriptableObject
{
    public string systemName = "5.1";

    [Tooltip("Domyœlne pozycje g³oœników w uk³adzie wspó³rzêdnych œwiata.")]
    public Vector3[] defaultPositions;

    [Tooltip("Domyœlne rotacje g³oœników (opcjonalnie).")]
    public Vector3[] defaultRotations;

    [Tooltip("Nazwy kana³ów: L, R, C, LS, RS, LB, RB, SUB (odpowiednio do pozycji).")]
    public string[] channelNames;
}