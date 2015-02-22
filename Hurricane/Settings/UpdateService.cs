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
                updateUrl = "http://hurricaneproject.bplaced.net/updater/updates/",
                projectId = "337f8837-7d1c-4659-8cc4-5e22f6eaf4e3",
                publicKey = "<RSAKeyValue><Modulus>0WxHvbAz4V0URRjsNxCRjYA3HMx3L40Woa+mriLOTDF5YkjKUPoKlZxWIrBFSbBTAogkcNAryphaGzI8sAAsxeWE+SlVjKI2vGPFZA1EedXSbmPXQdWY1jD15F6Ks6/TmL0Aacvk6OHc3d6RzGyGQ52GSInGEmeQq1iIgvhT/HXZzftpRthTEx9YtfKRMtyd/Vsq99B25gTQ+kl1OzlzmogN9apwQLavKxYFSCkZZRDlzND6hxMKLBpeWiZOqlw0lYyDAcE72EkxjQqPMmPD06t+UDBQE+0++uXmLWMfgMxKDi1G8U1An7OfsNC1iZVcR2dytNMW+5FE84bN8ZJTSDXHGuUwn+z6Se+kglaQEmpHLSt6SunH33BxouQa5bliB3LHa6+84p4VjJR2EoYRPRjjN46cTZbq8w8AHLBqLJ+2nirXBbDI50pGs+ek0YKMj5qb4x0pDl4P0MhHvdvHQL4Sia6ZzssI5bOwTSI/hl1HezNznVKlGzx5i0H5fRaVXLoHBNWXMKvdgqhRTJJr6zcPc+iKUBNTLoqbInRy8oX7dmJIZQbq3PvV5CJygIkbiFvLPw+DKSn9ffE2SRF1ZWnzQ33lCQc3c0BLU585BjAkAzX4TQLgU+PbZGX/V3JNqQBcvmdDurRwmU1td1Fx7gGlBtvcWS8BdP6oUWa6+8M=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>",
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
            var sb = new StringBuilder();
            foreach (var package in e.Result.newUpdatePackages)
            {
                DateTime releaseDateTime;
                var releaseDateTimeString = DateTime.TryParse(package.ReleaseDate, out releaseDateTime) ? releaseDateTime.ToString(Application.Current.Resources["DateFormat"].ToString()) : package.ReleaseDate;

                sb.AppendLine("[i]" + string.Format(Application.Current.Resources["UpdateChangelogText"].ToString(), package.releaseInfo.Version, releaseDateTimeString));
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
