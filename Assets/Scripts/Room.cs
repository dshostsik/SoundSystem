using System;
using UnityEngine;

/// <summary>
/// Script for room
/// </summary>
public class Room : MonoBehaviour
{
    [Header("Room geometry (meters)")]
    public float width = 6.0f;
    public float length = 4.0f;
    public float height = 2.5f;
    public RoomSurface[] surfaces = new RoomSurface[6];
    
    public float Width
    {
        get => width;
        // Values less than 0 will be negated. In case it is just a typo.
        set => width = value >= 0 ?  value : -value;
    }
    
    public float Lenght
    {
        get => length;
        // Values less than 0 will be negated. In case it is just a typo.
        set => length = value >= 0 ?  value : -value;
    }
    
    public float Height
    {
        get => height;
        // Values less than 0 will be negated. In case it is just a typo.
        set => height = value >= 0 ?  value : -value;
    }
    public float Volume => Mathf.Max(0.0001f, width * length * height);


    private void Start()
    {
        GenerateRoom();
    }

    /// <summary>
    /// Generates 6 walls (floor, ceiling, 4 sides) to form a rectangular room.
    /// </summary>
    private void GenerateRoom()
    {
        // 🧱 Helper function for creating walls
        GameObject CreateWall(string name, Vector3 position, Vector3 scale, Quaternion rotation)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(transform);
            wall.transform.localPosition = position;
            wall.transform.localRotation = rotation;
            wall.transform.localScale = scale;

            //if (wallMaterial != null)
            //    wall.GetComponent<Renderer>().material = wallMaterial;

            // Culling (backface) — jeśli chcesz, żeby wnętrze było widoczne, odwracamy normale
            wall.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

            return wall;
        }

        float w = Width;
        float l = Lenght;
        float h = Height;

        // 🧱 Floor
        CreateWall("Floor", new Vector3(0, 0, 0), new Vector3(w, 0.1f, l), Quaternion.identity);

        //// 🧱 Ceiling
        //CreateWall("Ceiling", new Vector3(0, h, 0), new Vector3(w, 0.1f, d), Quaternion.identity);

        //// 🧱 Back Wall
        //CreateWall("BackWall", new Vector3(0, h / 2f, -d / 2f), new Vector3(w, h, 0.1f), Quaternion.identity);

        //// 🧱 Front Wall
        //CreateWall("FrontWall", new Vector3(0, h / 2f, d / 2f), new Vector3(w, h, 0.1f), Quaternion.identity);

        //// 🧱 Left Wall
        //CreateWall("LeftWall", new Vector3(-w / 2f, h / 2f, 0), new Vector3(0.1f, h, d), Quaternion.identity);

        //// 🧱 Right Wall
        //CreateWall("RightWall", new Vector3(w / 2f, h / 2f, 0), new Vector3(0.1f, h, d), Quaternion.identity);
    }

    void Update()
    {
        
    }

}