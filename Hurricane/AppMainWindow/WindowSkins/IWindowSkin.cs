using System;
using System.Windows.Input;
using Hurricane.Music;

namespace Hurricane.AppMainWindow.WindowSkins
{
    public interface IWindowSkin
    {
        event EventHandler DragMoveStart;
        event EventHandler DragMoveStop;
        event EventHandler CloseRequest;
        event EventHandler ToggleWindowState;
        event EventHandler<MouseEventArgs> TitleBarMouseMove;

        void EnableWindow();
        void DisableWindow();

        WindowSkinConfiguration Configuration { get; set; }
    }
}
