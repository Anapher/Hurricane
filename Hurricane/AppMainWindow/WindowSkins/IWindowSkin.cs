using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        void RegisterSoundPlayer(Hurricane.Music.CSCoreEngine engine);
        void MusicManagerEnabled(object manager);

        WindowSkinConfiguration Configuration { get; set; }
    }
}
