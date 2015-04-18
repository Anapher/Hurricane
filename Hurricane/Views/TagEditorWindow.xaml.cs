using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Hurricane.Music.Track;
using Hurricane.ViewModels;
using Microsoft.Win32;
using TagLib;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for TagEditorWindow.xaml
    /// </summary>
    public partial class TagEditorWindow
    {
        public TagEditorWindow(LocalTrack track)
        {
            DataContext = new TagEditorViewModel(track, this);
            InitializeComponent();
        }
    }
}
