using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Hurricane.ViewModelBase;
using Microsoft.Win32;

namespace Hurricane.Views.Tools
{
    /// <summary>
    /// Interaction logic for QueueManagerWindow.xaml
    /// </summary>
    public partial class LanguageCreatorWindow : INotifyPropertyChanged
    {
        public LanguageCreatorWindow()
        {
            InitializeComponent();

        }

        #region Properties

        private LanguageDocument currentLanguageDocument;
        public LanguageDocument CurrentLanguageDocument
        {
            get { return currentLanguageDocument; }
            set { currentLanguageDocument = value; OnPropertyChanged("CurrentLanguageDocument"); }
        }

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; OnPropertyChanged("FilePath"); }
        }
        
        #endregion

        #region Commands
        private RelayCommand _openDocument;
        public RelayCommand OpenDocument
        {
            get
            {
                return _openDocument ?? (_openDocument = new RelayCommand(parameter =>
                {
                    var ofd = new OpenFileDialog() { Filter = "XAML files|*.xaml|All files|*.*" };
                    if (ofd.ShowDialog() != true) return;
                    ofd.Multiselect = false;
                    ofd.CheckFileExists = true;
                    CurrentLanguageDocument = LanguageDocument.FromFile(ofd.FileName);
                    FilePath = ofd.FileName;
                }));
            }
        }

        private RelayCommand _newDocument;
        public RelayCommand NewDocument
        {
            get
            {
                return _newDocument ?? (_newDocument = new RelayCommand(parameter =>
                {
                    CurrentLanguageDocument = LanguageDocument.CreateNew();
                    FilePath = null;
                }));
            }
        }   

        private RelayCommand _saveDocument;
        public RelayCommand SaveDocument
        {
            get { return _saveDocument ?? (_saveDocument = new RelayCommand(parameter =>
            {
                if (string.IsNullOrEmpty(FilePath)) { SaveAs(); } else { CurrentLanguageDocument.SaveDocument(FilePath);}
            })); }
        }

        private RelayCommand _saveDocumentAs;
        public RelayCommand SaveDocumentAs
        {
            get { return _saveDocumentAs ?? (_saveDocumentAs = new RelayCommand(parameter => { SaveAs(); })); }
        }

        private void SaveAs()
        {
            var sfd = new SaveFileDialog() { Filter = "XAML files|*.xaml|All files|*.*" };
            if (sfd.ShowDialog(this) == true)
            {
                CurrentLanguageDocument.SaveDocument(sfd.FileName);
                FilePath = sfd.FileName;
            }   
        }

        private RelayCommand _closeCommand;
        public RelayCommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(parameter => { this.Close(); })); }
        }

        private RelayCommand _resetValues;
        public RelayCommand ResetValues
        {
            get
            {
                return _resetValues ?? (_resetValues = new RelayCommand(parameter =>
                {
                    this.CurrentLanguageDocument.LanguageEntries.ForEach(x => x.Value = string.Empty);
                }));
            }
        }
        #endregion

        #region INotifyPropertyChanged
        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public class LanguageDocument
    {
        public List<LanguageEntry> LanguageEntries { get; set; }

        private LanguageDocument()
        {
            
        }

        public void SaveDocument(string path)
        {
            using (var sw = new StreamWriter(path))
            {
                sw.WriteLine("<ResourceDictionary xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
                sw.WriteLine("                    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
                sw.WriteLine("                    xmlns:system=\"clr-namespace:System;assembly=mscorlib\">");
                foreach (var languageEntry in LanguageEntries)
                {
                    sw.WriteLine("    <system:String x:Key=\"{0}\">{1}</system:String>", languageEntry.Key, languageEntry.Value);
                }
                sw.WriteLine("</ResourceDictionary>");
            }
        }

        public static LanguageDocument FromFile(string path)
        {
            return FromString(File.ReadAllText(path));
        }

        public static LanguageDocument FromString(string text)
        {
            var document = new LanguageDocument() { LanguageEntries = new List<LanguageEntry>() };
            foreach (Match match in Regex.Matches(text, "<system:String x:Key=\"(?<key>(.*?))\">(?<value>(.*?))</system:String>"))
                document.LanguageEntries.Add(new LanguageEntry() { Key = match.Groups["key"].Value, Value = match.Groups["value"].Value });
            SetEnglishWords(document);
            return document;
        }

        public static LanguageDocument CreateNew()
        {
            var document = FromString(EnglishDocument());
            document.LanguageEntries.ForEach(x => x.Value = string.Empty);
            return document;
        }

        private static void SetEnglishWords(LanguageDocument document)
        {
            foreach (Match match in Regex.Matches(EnglishDocument(), "<system:String x:Key=\"(?<key>(.*?))\">(?<value>(.*?))</system:String>"))
            {
                document.LanguageEntries.First(x => x.Key == match.Groups["key"].Value).EnglishWord =
                    match.Groups["value"].Value;
            }
        }

        public static string EnglishDocument()
        {
            return Properties.Resources.Hurricane_en_us;
            /*
            if (string.IsNullOrEmpty(_englishDocument))
            {
                var info = Application.GetResourceStream(new Uri("pack://application:,,,/Hurricane;component/Resources/Languages/Hurricane.en-us.xaml"));
                if (info == null) throw new Exception("Couldn't read english file");
                var tempFile = Path.GetTempFileName();
                using (var sw = new StreamWriter(tempFile, false, UTF8Encoding.Unicode))
                {
                    info.Stream.Seek(0, SeekOrigin.Begin);
                    info.Stream.CopyTo(sw.BaseStream);
                }
                using (var sr = new StreamReader(info.Stream, UTF8Encoding.UTF8))
                {
                    
                    _englishDocument = File.ReadAllText(tempFile); // sr.ReadToEnd();
                    File.Delete(tempFile);
                }
            }
            return _englishDocument;*/
        }
    }

    public class LanguageEntry
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string EnglishWord { get; set; }
    }
}
