using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using Hurricane.Music.Track;
using Hurricane.ViewModelBase;
using MahApps.Metro.Controls;
using Microsoft.Win32;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for TrackInformationWindow.xaml
    /// </summary>
    public partial class TrackInformationWindow : MetroWindow
    {
        private BitmapImage _image;
        public TrackInformationWindow(PlayableBase track)
        {
            CurrentTrack = track;
            if (CurrentTrack.StartTime == 0 && CurrentTrack.EndTime == 0)
            {
                StartTime = 0;
                EndTime = Math.Round(CurrentTrack.DurationTimespan.TotalMilliseconds, 0);
            }
            else
            {
                StartTime = CurrentTrack.StartTime;
                EndTime = CurrentTrack.EndTime;
            }

            MaximumTime = Math.Round(CurrentTrack.DurationTimespan.TotalMilliseconds, 0);

            InitializeComponent();

            if (!CurrentTrack.IsOpened)
            {
                CurrentTrack.Load();
                if (CurrentTrack.Image == null)
                {
                    CurrentTrack.ImageLoadedComplete +=
                        (s, e) => { if (CurrentTrack.Image != null) Dispatcher.Invoke(() => _image = CurrentTrack.Image.Clone()); };
                    return;
                }
            }

            if (CurrentTrack.Image != null) _image = CurrentTrack.Image.Clone();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (StartTime > 0 || EndTime < MaximumTime)
            {
                CurrentTrack.EndTime = EndTime;
                CurrentTrack.StartTime = StartTime;
            }
            else
            {
                CurrentTrack.EndTime = 0;
                CurrentTrack.StartTime = 0;
            }
        }

        public double EndTime { get; set; }
        public double StartTime { get; set; }
        public double MaximumTime { get; set; }

        public PlayableBase CurrentTrack { get; set; }

        private RelayCommand _saveImage;
        public RelayCommand SaveImage
        {
            get
            {
                return _saveImage ?? (_saveImage = new RelayCommand(parameter =>
                {
                    if (_image != null)
                    {
                        var sfd = new SaveFileDialog()
                        {
                            Filter = "PNG|*.png|JPG|*.jpg|GIF|*.gif|BMP|*.bmp",
                            FileName = CurrentTrack.DisplayText
                        };

                        if (sfd.ShowDialog() != true) return;
                        BitmapEncoder encoder;

                        switch (sfd.FilterIndex)
                        {
                            case 0:
                                encoder = new PngBitmapEncoder();
                                break;
                            case 1:
                                encoder = new JpegBitmapEncoder();
                                break;
                            case 2:
                                encoder = new GifBitmapEncoder();
                                break;
                            default:
                                encoder = new BmpBitmapEncoder();
                                break;
                        }

                        encoder.Frames.Add(BitmapFrame.Create(_image));

                        using (var filestream = new FileStream(sfd.FileName, FileMode.Create))
                            encoder.Save(filestream);
                    }
                }));
            }
        }
    }
}