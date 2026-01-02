using System;
using UnityEngine;

namespace Visualization
{
    public class WaveVisualizer
    {
        private int amplitudeUniformIndex;
        private int frequencyUniformIndex;
        private int speedUniformIndex;
        private Renderer renderer;

        private float amplitude;

        public float Amplitude
        {
            get => amplitude;
            set
            {
                /*if (value < 0) throw new System.ArgumentOutOfRangeException("Amplitude cannot be negative");
                if (!renderer.Equals(null))
                {
                    amplitude = value;
                    renderer.material.SetFloat(Amplitude1, amplitude);
                } else throw new System.NullReferenceException("Renderer is not set. Failed to sign amplitude");*/
                amplitude = this.SetUniformValue(amplitudeUniformIndex, value);
            }
        }

        private float frequency;

        public float Frequency
        {
            get => frequency;
            set
            {
                /*if (value < 0) throw new System.ArgumentOutOfRangeException("Frequency cannot be negative");
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
                }
                else throw new System.NullReferenceException("Renderer is not set. Failed to sign frequency");*/
                frequency = this.SetUniformValue(frequencyUniformIndex, value);
            }
        }

        private float speed;

        public float Speed
        {
            get => speed;
            set
            {
                /*if (value < 0) throw new System.ArgumentOutOfRangeException("Speed cannot be negative");

                if (!renderer.Equals(null))
                {
                    speed = value;
                    renderer.material.SetFloat(Speed1, speed);
                } else throw new System.NullReferenceException("Renderer is not set. Failed to sign speed");*/
                speed = this.SetUniformValue(speedUniformIndex, value);
            }
        }


        /// <summary>
        /// Applies value to shader uniform. Furthermore, is used to control state
        /// </summary>
        /// <param name="uniformID">ID number of the uniform in shader</param>
        /// <param name="value">Passed value</param>
        /// <param name="min">Minimal value that can be set (default: 0)</param>
        /// <param name="max">Maximal value that can be set (default: <see cref="int.MaxValue"/>>)</param>
        /// <returns>Value that has been set. Apply it to the field to control state</returns>
        /// <exception cref="ArgumentOutOfRangeException">if argument exceeds range</exception>
        private float SetUniformValue(int uniformID, float value, float min = 0, float max = int.MaxValue)
        {
            if (renderer.Equals(null)) throw new NullReferenceException("Renderer is not set. Failed to sign value");
            
            if (value < min || value > max) throw new ArgumentOutOfRangeException("Argument must be between " + min + " and " + max);
            
            renderer.material.SetFloat(uniformID, value);
            return value;
        }

        public Renderer Renderer
        {
            get => renderer;
            set
            {
                if (!value.Equals(null)) renderer = value;
                else throw new System.NullReferenceException("Renderer cannot be null");
                amplitudeUniformIndex = renderer.material.shader.FindPropertyIndex("_Amplitude");
                if (amplitudeUniformIndex == -1) throw new Exception("Amplitude property not found");
                frequencyUniformIndex = renderer.material.shader.FindPropertyIndex("_Frequency");
                if (frequencyUniformIndex == -1) throw new Exception("Frequency property not found");
                speedUniformIndex = renderer.material.shader.FindPropertyIndex("_Speed");
                if (speedUniformIndex == -1) throw new Exception("Speed property not found");
            }
        }
    }
}