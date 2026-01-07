using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tworzy funkcjê przejœcia H(f) z listy AcousticPath, a nastêpnie generuje odpowiedŸ impulsow¹ h(t) metod¹ IFFT.
/// </summary>
public class ImpulseResponseGenerator
{
    private FourierAnalyzer fft = new FourierAnalyzer();

    /// <summary>
    /// Buduje dyskretn¹ funkcjê przejœcia H(f) (cz. zespolona).
    /// </summary>
    public (float[] real, float[] imag) ComputeTransferFunction(List<AcousticPath> paths, int fftSize, float sampleRate)
    {
        float[] H_re = new float[fftSize];
        float[] H_im = new float[fftSize];

        for (int k = 0; k < fftSize; k++)
        {
            float f = k * sampleRate / fftSize;

            float re = 0f;
            float im = 0f;

            foreach (var p in paths)
            {
                float A = p.amplitude;

                // kierunkowoœæ g³oœnika
                float D = p.speaker.GetDirectivityGain(p.endPoint);
                A *= D;

                // odbicie (jeœli jest)
                if (p.reflectionPerBand != null && p.reflectionPerBand.Length > 3)
                    A *= p.reflectionPerBand[3]; // przyk³adowo pasmo 1kHz

                float w = 2f * Mathf.PI * f;
                float phase = -w * p.delay;

                re += A * Mathf.Cos(phase);
                im += A * Mathf.Sin(phase);
            }

            H_re[k] = re;
            H_im[k] = im;
        }

        return (H_re, H_im);
    }

    /// <summary>
    /// IFFT ? h(t)
    /// </summary>
    public float[] GenerateImpulseResponse((float[] real, float[] imag) H)
    {
        float[] re = (float[])H.real.Clone();
        float[] im = (float[])H.imag.Clone();

        fft.IFFT(re, im);
        return re; // czêœæ rzeczywista to IR
    }
}
