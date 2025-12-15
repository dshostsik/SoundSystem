namespace Visualization
{
    public static class WaveVisualizerFactory
    {
        private static WaveVisualizer waveVisualizer;

        public static WaveVisualizer Visualizer
        {
            get
            {
                waveVisualizer ??= new WaveVisualizer();
                return waveVisualizer;
            }
        }
    }
}