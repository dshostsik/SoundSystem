using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Oblicza œcie¿ki akustyczne:
/// - œcie¿kê bezpoœredni¹,
/// - odbicia pierwszego rzêdu.
/// 
/// Wynik: lista AcousticPath.
/// </summary>
public class RayAcousticTracer
{
    public float speedOfSound = 343.0f;

    /// <summary>
    /// G³ówna metoda: oblicza wszystkie œcie¿ki dla wszystkich g³oœników.
    /// </summary>
    public List<AcousticPath> ComputePaths(Room room, IReadOnlyDictionary<string, Speaker> speakers, Listener listener)
    {
        List<AcousticPath> paths = new List<AcousticPath>();

        foreach (var sp in speakers.Values)
        {
            // 1) Path direct
            AcousticPath direct = ComputeDirectPath(sp, listener);
            if (direct != null)
                paths.Add(direct);

            // 2) First order reflections
            foreach (var surf in room.surfaces)
            {
                if (!surf.enabled) continue;

                var refl = ComputeFirstOrderPath(room, sp, listener, surf);
                if (refl != null)
                    paths.Add(refl);
            }
        }

        return paths;
    }

    /// <summary>
    /// Tor bezpoœredni: prosta linia od g³oœnika do s³uchacza.
    /// </summary>
    private AcousticPath ComputeDirectPath(Speaker sp, Listener listener)
    {
        Vector3 p0 = sp.Position;
        Vector3 p1 = listener.Position;
        float d = Vector3.Distance(p0, p1);

        var path = new AcousticPath
        {
            speaker = sp,
            startPoint = p0,
            endPoint = p1,
            distance = d,
            delay = d / speedOfSound,
            amplitude = 1f / Mathf.Max(0.001f, d),
            order = 0,
            reflectionPoint = Vector3.zero,
            reflectionPerBand = null
        };

        return path;
    }

    /// <summary>
    /// Odbicie pierwszego rzêdu:
    /// 1. obliczamy "mirror source"
    /// 2. linia mirror ? listener
    /// 3. sprawdzamy przeciêcie z dan¹ powierzchni¹
    /// </summary>
    private AcousticPath ComputeFirstOrderPath(Room room, Speaker sp, Listener listener, RoomSurface surf)
    {
        if (!surf.enabled || surf.material == null)
            return null;

        // plane of surface
        if (!surf.TryGetPlane(out Plane plane))
            return null;

        // 1. Mirror source
        if (!surf.TryGetReflectedSource(sp.Position, out Vector3 mirror))
            return null;

        // 2. Ray mirror ? listener
        Vector3 dir = (listener.Position - mirror).normalized;
        Ray ray = new Ray(mirror, dir);

        // 3. Check intersection
        if (!plane.Raycast(ray, out float hitDist))
            return null;

        Vector3 hitPoint = ray.GetPoint(hitDist);

        // Validate: hitPoint must be on collider
        if (!surf.surfaceCollider.bounds.Contains(hitPoint))
            return null;

        // 4. Compute total path
        float d1 = Vector3.Distance(sp.Position, hitPoint);
        float d2 = Vector3.Distance(hitPoint, listener.Position);
        float d = d1 + d2;

        // 5. Absorption ? reflection coefficient per band
        int bands = surf.material.absorptionBands.Length;
        float[] reflection = new float[bands];
        for (int i = 0; i < bands; i++)
            reflection[i] = surf.material.GetReflectionByIndex(i);

        return new AcousticPath
        {
            speaker = sp,
            startPoint = sp.Position,
            endPoint = listener.Position,
            reflectionPoint = hitPoint,
            distance = d,
            delay = d / speedOfSound,
            amplitude = 1f / Mathf.Max(0.001f, d),
            order = 1,
            reflectionPerBand = reflection
        };
    }
}
