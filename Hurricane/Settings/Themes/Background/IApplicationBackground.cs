using System;

namespace Hurricane.Settings.Themes.Background
{
    public interface IApplicationBackground
    {
        Uri GetBackground();
        bool IsAnimated { get; }
        bool IsAvailable { get; }
        string DisplayText { get; }
    }
}