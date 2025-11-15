using UnityEngine;

public class AcousticPath
{
    public Speaker speaker;

    public Vector3 startPoint;
    public Vector3 endPoint;
    public Vector3 reflectionPoint;

    public float distance;
    public float delay;      // ? = d/c
    public float amplitude;  // 1/d
    public float[] reflectionPerBand; // R(f) dla ka¿dej czêstotliwoœci
    public int order;        // rz¹d odbicia

    public override string ToString()
    {
        return $"AcousticPath: order={order}, d={distance:F2}m, tau={delay:F4}s, amp={amplitude:F3}";
    }
}
