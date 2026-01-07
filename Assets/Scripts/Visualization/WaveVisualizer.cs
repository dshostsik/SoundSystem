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
                amplitude = this.SetUniformValue(amplitudeUniformIndex, value);
            }
        }

        private float frequency;

        public float Frequency
        {
            get => frequency;
            set
            {
                frequency = this.SetUniformValue(frequencyUniformIndex, value);
            }
        }

        private float speed;

        public float Speed
        {
            get => speed;
            set
            {
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
            if (!renderer) throw new NullReferenceException("Renderer is not set. Failed to sign value");
            
            if (value < min || value > max) throw new ArgumentOutOfRangeException("Argument must be between " + min + " and " + max);
            
            renderer.material.SetFloat(uniformID, value);
            return value;
        }

        public Renderer Renderer
        {
            get => renderer;
            set
            {
                if (value != null) renderer = value;
                else throw new System.NullReferenceException("Renderer cannot be null");
                
                amplitudeUniformIndex = Shader.PropertyToID("_Amplitude");
                if (!renderer.material.HasProperty(amplitudeUniformIndex)) throw new Exception("Amplitude property not found");
                frequencyUniformIndex = Shader.PropertyToID("_Frequency");
                if (!renderer.material.HasProperty(frequencyUniformIndex)) throw new Exception("Frequency property not found");
                speedUniformIndex = Shader.PropertyToID("_Speed");
                if (!renderer.material.HasProperty(speedUniformIndex)) throw new Exception("Speed property not found");
            }
        }
    }
}