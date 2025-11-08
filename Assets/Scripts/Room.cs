using System;
using UnityEngine;

/// <summary>
/// Script for room
/// </summary>
public class Room : MonoBehaviour
{
    [Header("Parameters of room")]
    [SerializeField] private float width;
    [SerializeField] private float depth;
    [SerializeField] private float height;
    [SerializeField] private WallMaterial wallMaterial;
    
    public float Width
    {
        get => width;
        // Values less than 0 will be negated. In case it is just a typo.
        set => width = value >= 0 ?  value : -value;
    }
    
    public float Depth
    {
        get => depth;
        // Values less than 0 will be negated. In case it is just a typo.
        set => depth = value >= 0 ?  value : -value;
    }
    
    public float Height
    {
        get => height;
        // Values less than 0 will be negated. In case it is just a typo.
        set => height = value >= 0 ?  value : -value;
    }
    
    private void Start()
    {
        
    }

    void Update()
    {
        
    }

}