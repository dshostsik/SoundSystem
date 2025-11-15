using UnityEngine;

/// <summary>
/// Prosty splot liniowy: y[n] = ? x[k] * h[n-k]
/// </summary>
public class AudioConvolver
{
    public float[] Convolve(float[] signal, float[] ir)
    {
        int N = signal.Length;
        int M = ir.Length;
        int L = N + M - 1;

        float[] output = new float[L];

        for (int n = 0; n < L; n++)
        {
            float sum = 0f;
            for (int k = 0; k < M; k++)
            {
                int i = n - k;
                if (i >= 0 && i < N)
                    sum += signal[i] * ir[k];
            }
            output[n] = sum;
        }

        return output;
    }
}
