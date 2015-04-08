using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Hurricane.AppMainWindow.Messages;

namespace Hurricane.Settings
{
    static class SaveLocationManager
    {
        static DirectoryInfo AppDataDirectory
        {
            get
            {
                return
                    new DirectoryInfo(Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Hurricane"));
            }
        }

        private static FileInfo InstalledInfoFile
        {
            get
            {
                return new FileInfo(".IsInstalled");
            }
        }

        private static bool? _isInstalled;
        public static bool IsInstalled()
        {
            return (_isInstalled ?? (_isInstalled = InstalledInfoFile.Exists)).Value;
        }

        public static async void MoveToAppData(WindowDialogService messageService)
        {
            var appDataDir = AppDataDirectory;

            var configFile = new FileInfo(Path.Combine(appDataDir.FullName, "config.xml"));
            var playlistFile = new FileInfo(Path.Combine(appDataDir.FullName, "playlists.xml"));
            var currentFile = new FileInfo(Path.Combine(appDataDir.FullName, "current.xml"));

            bool replaceFiles = true;
            if (appDataDir.Exists && (configFile.Exists || playlistFile.Exists))
            {
                replaceFiles = await
                    messageService.ShowMessage(
                        Application.Current.Resources["ReplaceFilesAtNewSaveLocation"].ToString(),
                        Application.Current.Resources["MoveSaveLocation"].ToString(), true, DialogMode.Single,
                        Application.Current.Resources["Yes"].ToString(), Application.Current.Resources["No"].ToString());
            }

            if (!appDataDir.Exists) appDataDir.Create();

            var localConfig = new FileInfo("config.xml");
            var localPlaylists = new FileInfo("playlists.xml");
            var localCurrent = new FileInfo("current.xml");

            if (!configFile.Exists || replaceFiles)
                localConfig.CopyTo(configFile.FullName, true);

            if (!playlistFile.Exists || replaceFiles)
                localPlaylists.CopyTo(playlistFile.FullName, true);

            if (!currentFile.Exists || replaceFiles)
                localCurrent.CopyTo(currentFile.FullName, true);

            File.Move("youtube-dl.exe", Path.Combine(appDataDir.FullName, "youtube-dl.exe"));

            // ReSharper disable once LocalizableElement
            File.WriteAllText(InstalledInfoFile.FullName, "garcon?");

            await
                messageService.ShowMessage(Application.Current.Resources["MoveSaveLocationSuccessful"].ToString(),
                    Application.Current.Resources["MoveSaveLocation"].ToString(), false, DialogMode.Single);
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        public static async void MoveToLocalFoler(WindowDialogService messageService)
        {
            var appDataDir = AppDataDirectory;
            var localConfig = new FileInfo("config.xml");
            var localPlaylists = new FileInfo("playlists.xml");
            var localCurrent = new FileInfo("current.xml");

            bool replaceFiles = false;
            if (localConfig.Exists || localPlaylists.Exists)
            {
                replaceFiles = await
                    messageService.ShowMessage(
                        Application.Current.Resources["ReplaceFilesAtNewSaveLocation"].ToString(),
                        Application.Current.Resources["MoveSaveLocation"].ToString(), true, DialogMode.Single,
                        Application.Current.Resources["Yes"].ToString(), Application.Current.Resources["No"].ToString());
            }

            var configFile = new FileInfo(Path.Combine(appDataDir.FullName, "config.xml"));
            var playlistFile = new FileInfo(Path.Combine(appDataDir.FullName, "playlists.xml"));
            var currentFile = new FileInfo(Path.Combine(appDataDir.FullName, "current.xml"));

            if (!localConfig.Exists || replaceFiles)
                configFile.CopyTo(localConfig.FullName, true);

            if (!localPlaylists.Exists ||replaceFiles)
                playlistFile.CopyTo(localPlaylists.FullName, true);

            if (!localCurrent.Exists || replaceFiles)
                currentFile.CopyTo(localCurrent.FullName, true);

            try
            {
                File.Move(Path.Combine(appDataDir.FullName, "youtube-dl.exe"), "youtube-dl.exe");
            }
            catch (IOException)
            {
                //file already exists
            }


            InstalledInfoFile.Delete();

            await
    messageService.ShowMessage(Application.Current.Resources["MoveSaveLocationSuccessful"].ToString(),
        Application.Current.Resources["MoveSaveLocation"].ToString(), false, DialogMode.Single);
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }
    }
}