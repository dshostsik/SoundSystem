namespace Visualization
{
    public static class WaveVisualizerFactory
    {
        private static WaveVisualizer _waveVisualizer;

        public static WaveVisualizer Visualizer
        {
            get
            {
                _waveVisualizer ??= new WaveVisualizer();
                return _waveVisualizer;
            }
        }
    }
}