using System;
using UnityEngine;

namespace Visualization
{
    public class WaveVisualizer
    {
        private static int Amplitude1;
        private static int Frequency1;
        private static int Speed1;
        private Renderer renderer;

        private float amplitude = 0.0025f;

        public float Amplitude
        {
            get => amplitude;
            set
            {
                if (value < 0) throw new System.ArgumentOutOfRangeException("Amplitude cannot be negative");
                if (!renderer.Equals(null))
                {
                    amplitude = value;
                    renderer.material.SetFloat(Amplitude1, amplitude);
                    return;
                }

                throw new System.NullReferenceException("Renderer is not set. Failed to sign amplitude");
            }
        }

        private float frequency = 2000.0f;

        public float Frequency
        {
            get => frequency;
            set
            {
                if (value < 0) throw new System.ArgumentOutOfRangeException("Frequency cannot be negative");
                if (!renderer.Equals(null))
                {
                    frequency = value;
                    try
                    { 
                        renderer.material.SetFloat(Frequency1, frequency / 40);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                        throw;
                    }
                    return;
                }

                throw new System.NullReferenceException("Renderer is not set. Failed to sign frequency");
            }
        }

        private float speed = 10.0f;

        public float Speed
        {
            get => speed;
            set
            {
                if (value < 0) throw new System.ArgumentOutOfRangeException("Speed cannot be negative");

                if (!renderer.Equals(null))
                {
                    speed = value;
                    renderer.material.SetFloat(Speed1, speed);
                    return;
                }

                throw new System.NullReferenceException("Renderer is not set. Failed to sign speed");
            }
        }

        public Renderer Renderer
        {
            get => renderer;
            set
            {
                if (!value.Equals(null)) renderer = value;
                else throw new System.NullReferenceException("Renderer cannot be null");
                Amplitude1 = renderer.material.shader.FindPropertyIndex("_Amplitude");
                if (Amplitude1 == -1) throw new Exception("Amplitude property not found");
                Frequency1 = renderer.material.shader.FindPropertyIndex("_Frequency");
                if (Frequency1 == -1) throw new Exception("Frequency property not found");
                Speed1 = renderer.material.shader.FindPropertyIndex("_Speed");
                if (Speed1 == -1) throw new Exception("Speed property not found");
            }
        }
    }
}