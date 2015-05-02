using System;
using System.Windows;
using System.Windows.Input;

namespace Hurricane.Model.Skin
{
    public interface IWindowSkin
    {
        WindowSkinConfiguration Configuration { get; set; }

        void EnableSkin();
        void DisableSkin();

        event EventHandler DragMoveStart;
        event EventHandler DragMoveStop;
        event EventHandler CloseRequest;
        event EventHandler ToggleWindowState;
        event EventHandler<MouseEventArgs> TitleBarMouseMove;
    }
}