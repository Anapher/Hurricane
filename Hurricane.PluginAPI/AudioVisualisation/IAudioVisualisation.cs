using System.Windows;

namespace Hurricane.PluginAPI.AudioVisualisation
{
    /// <summary>
    /// The audio visualisation interface
    /// </summary>
    public interface IAudioVisualisation
    {
        /// <summary>
        /// Enable the audio visualisation
        /// </summary>
        void Enable();

        /// <summary>
        /// Disable the audio visualisation
        /// </summary>
        void Disable();

        /// <summary>
        /// When the plugin gets disabled
        /// </summary>
        void Dispose();

        /// <summary>
        /// The <see cref="SpectrumProvider"/> for the virtualisation
        /// </summary>
        ISpectrumProvider SpectrumProvider { set; }

        /// <summary>
        /// This function should return the audio visualisation
        /// </summary>
        /// <returns>The audio visualisation, ready for displaying</returns>
        UIElement VisualElement { get; }

        /// <summary>
        /// Some colors from the current theme
        /// </summary>
        ColorInformation ColorInformation { set; }
    }
}
