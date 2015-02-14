using System.Windows;
using WPFSoundVisualizationLib;

namespace AudioVisualisation
{
    public interface IAudioVisualisationPlugin
    {
        UIElement GetUIElement(ISpectrumPlayer spectrumPlayer);
        string Creator { get; }
    }
}