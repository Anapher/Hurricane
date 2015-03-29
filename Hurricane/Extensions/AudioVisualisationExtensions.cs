using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Hurricane.PluginAPI.AudioVisualisation;
using Hurricane.ViewModels;

namespace Hurricane.Extensions
{
    public class AudioVisualisationExtensions
    {
        private static void AudioVisualisationForAdvancedWindowChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            AudioVisualisationChanged(dependencyObject, dependencyPropertyChangedEventArgs, true);
        }

        public static readonly DependencyProperty AudioVisualisationForAdvancedWindowProperty = DependencyProperty.RegisterAttached(
            "AudioVisualisationForAdvancedWindow", typeof (IAudioVisualisationPlugin), typeof (AudioVisualisationExtensions), new PropertyMetadata(default(IAudioVisualisationPlugin), AudioVisualisationForAdvancedWindowChanged));

        public static void SetAudioVisualisationForAdvancedWindow(DependencyObject element, IAudioVisualisationPlugin value)
        {
            element.SetValue(AudioVisualisationForAdvancedWindowProperty, value);
        }

        public static IAudioVisualisationPlugin GetAudioVisualisationForAdvancedWindow(DependencyObject element)
        {
            return (IAudioVisualisationPlugin) element.GetValue(AudioVisualisationForAdvancedWindowProperty);
        }


        public static readonly DependencyProperty AudioVisualisationForSmartWindowProperty = DependencyProperty.RegisterAttached(
            "AudioVisualisationForSmartWindow", typeof (IAudioVisualisationPlugin), typeof (AudioVisualisationExtensions), new PropertyMetadata(default(IAudioVisualisationPlugin), AudioVisualisationForSmartWindowChanged));

        private static void AudioVisualisationForSmartWindowChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            AudioVisualisationChanged(dependencyObject, dependencyPropertyChangedEventArgs, false);
        }

        private static void AudioVisualisationChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs, bool advancedVisualisation)
        {
            var control = dependencyObject as ContentControl;
            if (control == null) throw new ArgumentException();
            var audioVisualisation = dependencyPropertyChangedEventArgs.NewValue as IAudioVisualisationPlugin;
            if (audioVisualisation == null) throw new ArgumentException();

            var visualisation = advancedVisualisation ? audioVisualisation.AdvancedWindowVisualisation : audioVisualisation.SmartWindowVisualisation;
            visualisation.ColorInformation = GetColorInformation();
            visualisation.SpectrumProvider = MainViewModel.Instance.MusicManager.CSCoreEngine;

            control.Content = visualisation.VisualElement;
            control.Tag = visualisation;
        }

        private static ColorInformation GetColorInformation()
        {
            return new ColorInformation
            {
                AccentColor = (Color)Application.Current.Resources["AccentColor"],
                WhiteColor = (Color)Application.Current.Resources["WhiteColor"],
                BlackColor = (Color)Application.Current.Resources["BlackColor"],
                GrayColor = (Color)Application.Current.Resources["Gray7"]
            };
        }

        public static void SetAudioVisualisationForSmartWindow(DependencyObject element, IAudioVisualisationPlugin value)
        {
            element.SetValue(AudioVisualisationForSmartWindowProperty, value);
        }

        public static IAudioVisualisationPlugin GetAudioVisualisationForSmartWindow(DependencyObject element)
        {
            return (IAudioVisualisationPlugin) element.GetValue(AudioVisualisationForSmartWindowProperty);
        }
    }
}