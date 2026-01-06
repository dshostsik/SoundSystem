using System;
using UnityEngine;

/// <summary>
/// Reprezentuje jedn¹ powierzchniê pomieszczenia (np. œciana, sufit, pod³oga).
/// Przechowuje odniesienie do collidera i przypisanego AcousticMaterial.
/// Dostarcza pomocnicze metody geometryczne do obliczania lustrzanego Ÿród³a.
/// </summary>
[Serializable]
public class RoomSurface : MonoBehaviour
{
    [Tooltip("Nazwa powierzchni (np. Wall_Left)")]
    public string surfaceName = "Surface";

    [Tooltip("Collider opisuj¹cy powierzchniê (plane/mesh collider).")]
    public Collider surfaceCollider;

    [Tooltip("Materia³ akustyczny przypisany do tej powierzchni.")]
    public AcousticMaterial material;

    [Tooltip("Czy powierzchnia powinna byæ uwzglêdniana przy obliczaniu odbiæ.")]
    public bool enabled = true;

    private void Awake()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        meshRenderer.material = this.material.assignedMaterial;
    }

    /// <summary>
    /// Jeœli powierzchnia posiada wa¿ny collider i p³ask¹ geometriê,
    /// zwraca p³aszczyznê (Plane) odpowiadaj¹c¹ tej powierzchni w lokalnym œwiecie.
    /// W przypadku braku mo¿liwoœci wyci¹gniêcia p³aszczyzny, zwraca false.
    /// </summary>
    public bool TryGetPlane(out Plane planeWorld)
    {
        planeWorld = new Plane();
        if (!surfaceCollider) return false;

        // Obs³ugujemy najczêœciej BoxCollider i MeshCollider (przy za³o¿eniu planarnoœci)
        if (surfaceCollider is BoxCollider box)
        {
            // Najprostsze: traktujemy box jako p³aszczyznê przy jego transformie
            Vector3 normal = box.transform.up;
            Vector3 point = box.transform.TransformPoint(box.center);
            planeWorld = new Plane(normal, point);
            return true;
        }
        else if (surfaceCollider is MeshCollider meshCol && meshCol.sharedMesh != null)
        {
            // Przyjmujemy, ¿e mesh jest planar¹ powierzchni¹ i u¿ywamy transform.up
            Vector3 normal = meshCol.transform.up;
            Vector3 point = meshCol.transform.position;
            planeWorld = new Plane(normal, point);
            return true;
        }
        else if (surfaceCollider is TerrainCollider)
        {
            // Terrain nie obs³ugiwany jako p³aska powierzchnia tutaj
            return false;
        }

        // Fallback: u¿yj transform.up jako normalnej i transform.position jako punkt
        Vector3 fallbackNormal = surfaceCollider.transform.up;
        Vector3 fallbackPoint = surfaceCollider.transform.position;
        planeWorld = new Plane(fallbackNormal, fallbackPoint);
        return true;
    }

    /// <summary>
    /// Oblicza pozycjê wirtualnego (lustrzanego) Ÿród³a wzglêdem p³aszczyzny surface.
    /// Zwraca false, jeœli nie da siê obliczyæ p³aszczyzny.
    /// </summary>
    public bool TryGetReflectedSource(Vector3 sourcePos, out Vector3 reflectedSource)
    {
        reflectedSource = Vector3.zero;
        if (!enabled) return false;
        if (!TryGetPlane(out Plane plane)) return false;

        // Rzutowanie punktu na p³aszczyznê i odbicie lustrzane
        // Odleg³oœæ punktu do p³aszczyzny (d), normal = n
        float d = plane.GetDistanceToPoint(sourcePos);
        Vector3 n = plane.normal;

        // Punkt na p³aszczyŸnie:
        Vector3 pointOnPlane = sourcePos - n * d;
        // Odbity punkt = punkt na p³aszczyŸnie - (sourcePos - punktNaP³aszczyŸnie)
        reflectedSource = pointOnPlane - (sourcePos - pointOnPlane);
        return true;
    }
}