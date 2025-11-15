using System.Linq;
using UnityEngine;

/// <summary>
/// MonoBehaviour reprezentujące pokój/salę symulacji.
/// Trzyma kolekcję RoomSurface (ściany, sufit, podłoga) oraz podstawowe metody akustyczne:
/// - objętość V,
/// - obliczenie T60 z formuły Sabine (dla zadanych pasm),
/// - sumowanie powierzchni i średniego pochłaniania.
/// </summary>
public class Room : MonoBehaviour
{
    [Header("Room geometry (meters)")]
    public float width = 6.0f;
    public float length = 4.0f;
    public float height = 2.5f;

    [Header("Room surfaces")]
    [Tooltip("Lista powierzchni (ściany, sufit, podłoga). Każda powierzchnia powinna mieć przypisany AcousticMaterial.")]
    public RoomSurface[] surfaces = new RoomSurface[6];

    /// <summary>
    /// Objętość pomieszczenia V = width * length * height (m^3)
    /// </summary>
    public float Volume => Mathf.Max(0.0001f, width * length * height);

    /// <summary>
    /// Oblicza sumaryczną powierzchnię (S) dla powierzchni zdefiniowanych w surfaces.
    /// Jeżeli powierzchnia ma przypisany collider Box lub Mesh z wielkością, próbuje oszacować S;
    /// w przeciwnym razie wymaga ręcznego wypełnienia przez użytkownika (surface area = 0).
    /// </summary>
    public float TotalSurfaceArea()
    {
        float total = 0f;
        foreach (var s in surfaces)
        {
            if (s == null || s.surfaceCollider == null) continue;

            // Próba oszacowania powierzchni z BoxCollider
            if (s.surfaceCollider is BoxCollider box)
            {
                Vector3 worldScale = box.transform.lossyScale;
                Vector3 size = Vector3.Scale(box.size, worldScale);
                // Przyjmujemy jedna strona powierzchni (box jako płaska płyta): width * height (największe dwie osie)
                float area = Mathf.Abs(size.x * size.y);
                total += area;
            }
            else if (s.surfaceCollider is MeshCollider meshCol && meshCol.sharedMesh != null)
            {
                // Przybliżenie: sumujemy wszystkie tri area / transform scale (nieidealne, ale użyteczne)
                var mesh = meshCol.sharedMesh;
                float meshArea = 0f;
                var verts = mesh.vertices;
                var tris = mesh.triangles;
                for (int i = 0; i < tris.Length; i += 3)
                {
                    Vector3 a = verts[tris[i]];
                    Vector3 b = verts[tris[i + 1]];
                    Vector3 c = verts[tris[i + 2]];
                    meshArea += Vector3.Cross(b - a, c - a).magnitude * 0.5f;
                }
                // skalowanie - przybliżenie
                Vector3 sScale = meshCol.transform.lossyScale;
                meshArea *= Mathf.Abs(sScale.x * sScale.y); // uproszczenie
                total += meshArea;
            }
            else
            {
                // brak danych: pomin
            }
        }
        return total;
    }

    /// <summary>
    /// Dla danego indeksu pasma (0..N-1) oblicza wartość A = sum(α_i * S_i),
    /// gdzie S_i to powierzchnia danej powierzchni, α_i to współczynnik pochłaniania dla tej powierzchni i pasma.
    /// Wymaga oszacowania powierzchni (metoda powyżej). Jeśli nie można oszacować powierzchni,
    /// zaleca się wypełnić ręcznie powierzchnie w inspectorze lub dostarczyć własne wartości.
    /// </summary>
    public float TotalAbsorptionForBand(int bandIndex)
    {
        float sum = 0f;
        foreach (var s in surfaces)
        {
            if (s == null || s.surfaceCollider == null || s.material == null) continue;

            float area = 0f;
            if (s.surfaceCollider is BoxCollider box)
            {
                Vector3 worldScale = box.transform.lossyScale;
                Vector3 size = Vector3.Scale(box.size, worldScale);
                area = Mathf.Abs(size.x * size.y);
            }
            else if (s.surfaceCollider is MeshCollider meshCol && meshCol.sharedMesh != null)
            {
                var mesh = meshCol.sharedMesh;
                float meshArea = 0f;
                var verts = mesh.vertices;
                var tris = mesh.triangles;
                for (int i = 0; i < tris.Length; i += 3)
                {
                    Vector3 a = verts[tris[i]];
                    Vector3 b = verts[tris[i + 1]];
                    Vector3 c = verts[tris[i + 2]];
                    meshArea += Vector3.Cross(b - a, c - a).magnitude * 0.5f;
                }
                Vector3 sScale = meshCol.transform.lossyScale;
                meshArea *= Mathf.Abs(sScale.x * sScale.y);
                area = meshArea;
            }
            else
            {
                // Jeżeli nie umiemy oszacować powierzchni, pomijamy.
                area = 0f;
            }

            float alpha = s.material.GetAbsorptionByIndex(bandIndex);
            sum += alpha * area;
        }
        return sum;
    }

    /// <summary>
    /// Oblicza czas pogłosu T60 metodą Sabine dla podanego pasma (index band).
    /// T60 = 0.161 * V / A, gdzie A = sum(alpha_i * S_i).
    /// Jeśli A jest bliskie zera, zwraca duże T (np. 999s) jako wskazanie.
    /// </summary>
    public float ComputeSabineT60(int bandIndex)
    {
        float V = Volume;
        float A = TotalAbsorptionForBand(bandIndex);
        if (A <= 1e-6f) return 999f; // praktycznie nieskończony
        return 0.161f * V / A;
    }
}
