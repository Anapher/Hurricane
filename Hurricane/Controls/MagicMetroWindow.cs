using System.Windows;
using Hurricane.Model.Skin;
using MahApps.Metro.Controls;
using WindowSettings = Hurricane.Model.Skin.WindowSettings;

namespace Hurricane.Controls
{
    internal class MagicMetroWindow : MetroWindow
    {
        public static readonly DependencyProperty AdvancedViewSkinProperty = DependencyProperty.Register(
            "AdvancedViewSkin", typeof (IWindowSkin), typeof (MagicMetroWindow), new PropertyMetadata(default(IWindowSkin)));

        public static readonly DependencyProperty DockViewSkinProperty = DependencyProperty.Register(
            "DockViewSkin", typeof (IWindowSkin), typeof (MagicMetroWindow), new PropertyMetadata(default(IWindowSkin)));

        public static readonly DependencyProperty WindowSettingsProperty = DependencyProperty.Register(
            "WindowSettings", typeof (WindowSettings), typeof (MagicMetroWindow), new PropertyMetadata(default(WindowSettings)));

        public static readonly DependencyProperty ShowMagicArrowBelowCursorProperty = DependencyProperty.Register(
            "ShowMagicArrowBelowCursor", typeof (bool), typeof (MagicMetroWindow), new PropertyMetadata(default(bool)));

        public IWindowSkin AdvancedViewSkin
        {
            get { return (IWindowSkin) GetValue(AdvancedViewSkinProperty); }
            set { SetValue(AdvancedViewSkinProperty, value); }
        }

        public IWindowSkin DockViewSkin
        {
            get { return (IWindowSkin) GetValue(DockViewSkinProperty); }
            set { SetValue(DockViewSkinProperty, value); }
        }

        public WindowSettings WindowSettings
        {
            get { return (WindowSettings)GetValue(WindowSettingsProperty); }
            set { SetValue(WindowSettingsProperty, value); }
        }

        public bool ShowMagicArrowBelowCursor
        {
            get { return (bool)GetValue(ShowMagicArrowBelowCursorProperty); }
            set { SetValue(ShowMagicArrowBelowCursorProperty, value); }
        }

        public IWindowSkin CurrentSkin { get; set; }
        public CurrentWindowState CurrentWindowState { get; set; }

        protected void ApplyWindowSkin()
        {
            var newWindowSkin = CurrentWindowState == CurrentWindowState.Normal ? AdvancedViewSkin : DockViewSkin;
            if (CurrentSkin == newWindowSkin)
                return;

            if (CurrentSkin != null)
            {
                
            }
        }
    }

    public enum CurrentWindowState
    {
        Normal,
        Docked
    }
}