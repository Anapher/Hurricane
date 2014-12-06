using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hurricane.Settings
{
    public class UpdateService : ViewModelBase.PropertyChangedBase
    {
        updateSystemDotNet.updateController updController;
        Language language;
        const string changelogseperator = "------------------------------------";

        public UpdateService(Language currentlanguage)
        {
            this.language = currentlanguage;
            updController = new updateSystemDotNet.updateController();
            updController.updateFound += updController_updateFound;
            updController.downloadUpdatesCompleted += updController_downloadUpdatesCompleted;
            updController.downloadUpdatesProgressChanged += updController_downloadUpdatesProgressChanged;

            updController.updateUrl = "http://hurrican.16mb.com/update";
            updController.projectId = "fe287b37-6dfb-4e1b-bec9-fd1ce797f148";
            updController.publicKey = "<RSAKeyValue><Modulus>6tY5/Ym5hZGCN4VEu1mno8C9pPWJc7PTpxEPFyAiq532oNwo1npgUiC83IzQcbj5PYkCmqDXBiD1g4SmgfAg/kQ1VXIfPYge++SCzxI85GwNFn+TRRHloo8Bvnwn9ZJJuECAzb6AcLTRfCsP0cEi0ynNyyS/whs5gmYHSE/lyrCphEOWCZgWPRCaK6vIBP2BjknGwZlg6PchT+JdAvKvxnDhQOIF7x0JWOioBVJQQ7+vHHOzcoaaPzmfQtrJ2myVBi4LIpjtSwHGlq2WXHbIsZW3LGZGD28cyiYm+aWwVuO9x/QhXn3prnH7sZqgHxWQvmIDd1/0cbpM1jitUvad35wqSCbGh4/XxZJiK8l3FumP0YF6DRXkjWoLWUARtEOh+A4O153UhFcYV5cwH4R2WAXjEWpFLwA/vukOnYttylFpzoIFldKGXNbUYND4vteGQHqP6U7Hih20cg5OR8fSvKNfIv5znADEqviHVATtAaxVD4rKZLehTLN1UvzZClGV+Q7OAOZmY4qCXeeIZN28nn7iMZHrkpLwDrjrsMIRbAunx5zMfpib07peN5fuK9PoRDmDArkfV/JZ1/eid+MMQm63zKMxjvQCY/pb7O+F7ylRDJiytDi8AKW4HUxrtOAOrsXfSAHPi95CYR3nUXKcYEEoQ+4nY4js2MKTY+e7h/8=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

            updController.releaseFilter.checkForFinal = true;
            updController.releaseFilter.checkForBeta = true;
            updController.releaseFilter.checkForAlpha = false;

            updController.restartApplication = true;
            updController.retrieveHostVersion = true;
            updController.autoCloseHostApplication = true;
            updController.Language = this.language == Language.English ? updateSystemDotNet.Languages.English : updateSystemDotNet.Languages.Deutsch;
        }

        void updController_downloadUpdatesProgressChanged(object sender, updateSystemDotNet.appEventArgs.downloadUpdatesProgressChangedEventArgs e)
        {
            ProgressState = e.ProgressPercentage;
        }

        void updController_downloadUpdatesCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled)
                updController.applyUpdate();
        }

        #region Public Methods
        public void CheckForUpdates(Window basewindow)
        {
            updController.checkForUpdatesAsync();
        }

        public void Update()
        {
            updController.downloadUpdates();
        }

        public void CancelUpdate()
        {
            updController.cancelUpdateDownload();
            this.UpdateFound = false;
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
        
        private double progressstate;
        public double ProgressState
        {
            get { return progressstate; }
            set
            {
                SetProperty(value, ref progressstate);
            }
        }

        
        private bool updatefound;
        public bool UpdateFound
        {
            get { return updatefound; }
            set
            {
                SetProperty(value, ref updatefound);
            }
        }

        void updController_updateFound(object sender, updateSystemDotNet.appEventArgs.updateFoundEventArgs e)
        {
            var version = e.Result.newUpdatePackages.Last().releaseInfo.Version;
            NewVersion = version.Substring(0, version.Length - 2); //remove two to get 0.0.0 instead of 0.0.0.0
            System.Text.StringBuilder sb = new StringBuilder();
            foreach (var package in e.Result.newUpdatePackages)
            {
                sb.AppendLine(string.Format(Application.Current.FindResource("updatechangelogtext").ToString(), package.releaseInfo.Version, DateTime.Parse(package.ReleaseDate).ToString(Application.Current.FindResource("DateFormat").ToString())));
                sb.AppendLine(changelogseperator);
                sb.AppendLine(this.language == Language.English ? updController.currentUpdateResult.Changelogs[package].englishChanges : updController.currentUpdateResult.Changelogs[package].germanChanges);
                sb.AppendLine();
                UpdateSize += package.packageSize;
            }
            this.Changelog = sb.ToString();
            UpdateFound = true;
        }

        public enum Language { English, German }
    }
}
