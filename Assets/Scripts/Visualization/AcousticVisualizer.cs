using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wizualizacja wyników symulacji.
///
/// Funkcje:
/// - rysowanie œcie¿ek akustycznych (direct + odbicie),
/// - rysowanie punktów odbicia,
/// - rysowanie IR w czasie,
/// - opcjonalnie wykres FFT.
/// 
/// Uwaga:
/// Ten skrypt s³u¿y tylko do wizualizacji — nie wykonuje obliczeñ.
/// </summary>
public class AcousticVisualizer : MonoBehaviour
{
    [Header("Path visualization settings")]
    public bool showPaths = true;
    public Color directColor = Color.yellow;
    public Color reflectionColor = Color.cyan;
    public float pathWidth = 0.02f;

    [Header("Impulse Response visualization")]
    public bool showIR = true;
    public RectTransform irGraphParent;
    public GameObject irPointPrefab;
    public float irScale = 100f;

    [Header("FFT visualization (optional)")]
    public bool showFFT = false;
    public RectTransform fftGraphParent;
    public GameObject fftPointPrefab;
    public float fftScale = 25f;

    private List<LineRenderer> linePool = new List<LineRenderer>();
    private List<GameObject> irPoints = new List<GameObject>();
    private List<GameObject> fftPoints = new List<GameObject>();

    // --- PATH VISUALIZATION -----------------------------------------------------------------

    /// <summary>
    /// Rysuje wszystkie œcie¿ki akustyczne w 3D na scenie.
    /// </summary>
    public void VisualizeSoundField(List<AcousticPath> paths)
    {
        ClearLines();

        if (!showPaths || paths == null) return;

        foreach (var path in paths)
        {
            LineRenderer lr = CreateLineRenderer();

            if (path.order == 0)
            {
                lr.startColor = directColor;
                lr.endColor = directColor;
                lr.positionCount = 2;
                lr.SetPosition(0, path.startPoint);
                lr.SetPosition(1, path.endPoint);
            }
            else
            {
                lr.startColor = reflectionColor;
                lr.endColor = reflectionColor;
                lr.positionCount = 3;
                lr.SetPosition(0, path.startPoint);
                lr.SetPosition(1, path.reflectionPoint);
                lr.SetPosition(2, path.endPoint);
            }
        }
    }

    private LineRenderer CreateLineRenderer()
    {
        GameObject obj = new GameObject("AcousticPath");
        LineRenderer lr = obj.AddComponent<LineRenderer>();

        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = pathWidth;
        lr.endWidth = pathWidth;
        lr.useWorldSpace = true;

        linePool.Add(lr);
        return lr;
    }

    private void ClearLines()
    {
        foreach (var lr in linePool)
            if (lr != null) Destroy(lr.gameObject);

        linePool.Clear();
    }

    // --- IR VISUALIZATION -------------------------------------------------------------------

    /// <summary>
    /// Rysuje 2D wykres odpowiedzi impulsowej h(t) na Canvasie.
    /// </summary>
    public void VisualizeImpulseResponse(float[] h)
    {
        ClearIR();

        if (!showIR || h == null || !irGraphParent)
            return;

        float width = irGraphParent.rect.width;
        float height = irGraphParent.rect.height;

        for (int i = 0; i < h.Length; i += 8) // nie rysujemy ka¿dego sample, tylko co 8 w celu oszczêdnoœci
        {
            float x = (float)i / h.Length * width;
            float y = (h[i] * irScale) + height / 2f;

            GameObject p = Instantiate(irPointPrefab, irGraphParent);
            p.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);

            irPoints.Add(p);
        }
    }

    private void ClearIR()
    {
        foreach (var p in irPoints)
            Destroy(p);

        irPoints.Clear();
    }

    // --- FFT VISUALIZATION ------------------------------------------------------------------

    /// <summary>
    /// Rysuje widmo sygna³u IR (modu³ FFT).
    /// </summary>
    public void VisualizeFFT(float[] real, float[] imag)
    {
        ClearFFT();

        if (!showFFT || real == null || imag == null || fftGraphParent == null)
            return;

        int n = real.Length;
        float width = fftGraphParent.rect.width;
        float height = fftGraphParent.rect.height;

        for (int k = 0; k < n / 2; k += 4) // czêœæ rzeczywista do Nyquista
        {
            float magnitude = Mathf.Sqrt(real[k] * real[k] + imag[k] * imag[k]);

            float x = (float)k / (n / 2) * width;
            float y = magnitude * fftScale;

            GameObject p = Instantiate(fftPointPrefab, fftGraphParent);
            p.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);

            fftPoints.Add(p);
        }
    }

    private void ClearFFT()
    {
        foreach (var p in fftPoints)
            Destroy(p);

        fftPoints.Clear();
    }
}
