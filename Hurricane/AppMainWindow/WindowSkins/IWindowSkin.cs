using System;
using Hurricane.Music;

namespace Hurricane.AppMainWindow.WindowSkins
{
    public interface IWindowSkin
    {
        event EventHandler DragMoveStart;
        event EventHandler DragMoveStop;
        event EventHandler CloseRequest;
        event EventHandler ToggleWindowState;

        void EnableWindow();
        void DisableWindow();
        void RegisterSoundPlayer(CSCoreEngine engine);
        void MusicManagerEnabled(object manager);

        WindowSkinConfiguration Configuration { get; set; }
    }
}
