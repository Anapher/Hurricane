using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Hurricane.ViewModelBase;
using updateSystemDotNet;
using updateSystemDotNet.appEventArgs;

namespace Hurricane.Settings
{
    public class UpdateService : PropertyChangedBase, IDisposable
    {
        readonly updateController _updController;
        readonly Language _language;

        public UpdateService(Language currentlanguage)
        {
            _language = currentlanguage;
            _updController = new updateController
            {
                updateUrl = "http://hurrican.16mb.com/update",
                projectId = "fe287b37-6dfb-4e1b-bec9-fd1ce797f148",
                publicKey = "<RSAKeyValue><Modulus>6tY5/Ym5hZGCN4VEu1mno8C9pPWJc7PTpxEPFyAiq532oNwo1npgUiC83IzQcbj5PYkCmqDXBiD1g4SmgfAg/kQ1VXIfPYge++SCzxI85GwNFn+TRRHloo8Bvnwn9ZJJuECAzb6AcLTRfCsP0cEi0ynNyyS/whs5gmYHSE/lyrCphEOWCZgWPRCaK6vIBP2BjknGwZlg6PchT+JdAvKvxnDhQOIF7x0JWOioBVJQQ7+vHHOzcoaaPzmfQtrJ2myVBi4LIpjtSwHGlq2WXHbIsZW3LGZGD28cyiYm+aWwVuO9x/QhXn3prnH7sZqgHxWQvmIDd1/0cbpM1jitUvad35wqSCbGh4/XxZJiK8l3FumP0YF6DRXkjWoLWUARtEOh+A4O153UhFcYV5cwH4R2WAXjEWpFLwA/vukOnYttylFpzoIFldKGXNbUYND4vteGQHqP6U7Hih20cg5OR8fSvKNfIv5znADEqviHVATtAaxVD4rKZLehTLN1UvzZClGV+Q7OAOZmY4qCXeeIZN28nn7iMZHrkpLwDrjrsMIRbAunx5zMfpib07peN5fuK9PoRDmDArkfV/JZ1/eid+MMQm63zKMxjvQCY/pb7O+F7ylRDJiytDi8AKW4HUxrtOAOrsXfSAHPi95CYR3nUXKcYEEoQ+4nY4js2MKTY+e7h/8=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>",
                releaseFilter = {checkForFinal = true, checkForBeta = true, checkForAlpha = false},
                restartApplication = true,
                retrieveHostVersion = true,
                autoCloseHostApplication = true,
                Language = _language == Language.English ? Languages.English : Languages.Deutsch
            };

            _updController.updateFound += updController_updateFound;
            _updController.downloadUpdatesCompleted += updController_downloadUpdatesCompleted;
            _updController.downloadUpdatesProgressChanged += updController_downloadUpdatesProgressChanged;
        }

        void updController_downloadUpdatesProgressChanged(object sender, downloadUpdatesProgressChangedEventArgs e)
        {
            ProgressState = e.ProgressPercentage;
        }

        void updController_downloadUpdatesCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled)
                _updController.applyUpdate();
        }

        #region Public Methods
        public void CheckForUpdates(Window basewindow)
        {
            _updController.checkForUpdatesAsync();
        }

        public void Update()
        {
            _updController.downloadUpdates();
        }

        public void CancelUpdate()
        {
            _updController.cancelUpdateDownload();
            UpdateFound = false;
        }
        #endregion

        public string CurrentVersion
        {
            get
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
                return String.Format("{0}.{1}.{2}", fvi.ProductMajorPart, fvi.ProductMinorPart,fvi.ProductBuildPart);
            }
        }

        public string NewVersion { get; protected set; }
        public string Changelog { get; set; }
        public long UpdateSize { get; set; }
        
        private double _progressstate;
        public double ProgressState
        {
            get { return _progressstate; }
            set
            {
                SetProperty(value, ref _progressstate);
            }
        }

        private bool _updatefound;
        public bool UpdateFound
        {
            get { return _updatefound; }
            set
            {
                SetProperty(value, ref _updatefound);
            }
        }

        void updController_updateFound(object sender, updateFoundEventArgs e)
        {
            var version = e.Result.newUpdatePackages.Last().releaseInfo.Version;
            NewVersion = version.Substring(0, version.Length - 2); //remove two to get 0.0.0 instead of 0.0.0.0
            StringBuilder sb = new StringBuilder();
            foreach (var package in e.Result.newUpdatePackages)
            {
                sb.AppendLine("[i]" + string.Format(Application.Current.Resources["UpdateChangelogText"].ToString(), package.releaseInfo.Version, DateTime.Parse(package.ReleaseDate).ToString(Application.Current.Resources["DateFormat"].ToString())));
                sb.AppendLine();
                sb.AppendLine(_language == Language.English ? _updController.currentUpdateResult.Changelogs[package].englishChanges : _updController.currentUpdateResult.Changelogs[package].germanChanges);
                sb.AppendLine();
                UpdateSize += package.packageSize;
            }
            Changelog = sb.ToString();
            UpdateFound = true;
        }

        public enum Language { English, German }

        public void Dispose()
        {
            _updController.Dispose();
        }
    }
}
