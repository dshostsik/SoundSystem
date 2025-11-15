using System;
using UnityEngine;

/// <summary>
/// Prosta implementacja FFT / IFFT.
/// U¿ywana w IR Generatorze oraz do debugowania wykresów.
/// </summary>
public class FourierAnalyzer
{
    /// <summary>
    /// FFT — Cooley–Tukey, operuje na tablicach float (re, im).
    /// </summary>
    public void FFT(float[] real, float[] imag)
    {
        int n = real.Length;
        if (n <= 1) return;

        int half = n / 2;

        float[] evenReal = new float[half];
        float[] evenImag = new float[half];
        float[] oddReal = new float[half];
        float[] oddImag = new float[half];

        for (int i = 0; i < half; i++)
        {
            evenReal[i] = real[2 * i];
            evenImag[i] = imag[2 * i];
            oddReal[i] = real[2 * i + 1];
            oddImag[i] = imag[2 * i + 1];
        }

        FFT(evenReal, evenImag);
        FFT(oddReal, oddImag);

        for (int k = 0; k < half; k++)
        {
            float t = -2f * Mathf.PI * k / n;
            float cos = Mathf.Cos(t);
            float sin = Mathf.Sin(t);

            float tre = cos * oddReal[k] - sin * oddImag[k];
            float tim = sin * oddReal[k] + cos * oddImag[k];

            real[k] = evenReal[k] + tre;
            imag[k] = evenImag[k] + tim;

            real[k + half] = evenReal[k] - tre;
            imag[k + half] = evenImag[k] - tim;
        }
    }

    /// <summary>
    /// IFFT — FFT z odwróconym znakiem i skalowaniem 1/N.
    ///</summary>
    public void IFFT(float[] real, float[] imag)
    {
        int n = real.Length;

        for (int i = 0; i < n; i++)
            imag[i] = -imag[i];

        FFT(real, imag);

        for (int i = 0; i < n; i++)
        {
            real[i] /= n;
            imag[i] = -imag[i] / n;
        }
    }
}
