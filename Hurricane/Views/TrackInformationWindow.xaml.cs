using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Hurricane.Music;
using Hurricane.Music.Track;
using Hurricane.ViewModelBase;
using Hurricane.Views.UserControls;
using MahApps.Metro.Controls;

namespace Hurricane.Views
{
    /// <summary>
    /// Interaction logic for TrackInformationWindow.xaml
    /// </summary>
    public partial class TrackInformationWindow : MetroWindow
    {
        private BitmapImage image;
        public TrackInformationWindow(PlayableBase track)
        {
            this.CurrentTrack = track;
            InitializeComponent();

            if (!CurrentTrack.IsOpened)
            {
                CurrentTrack.Load();
                if (CurrentTrack.Image == null)
                {
                    CurrentTrack.ImageLoadComplete +=
                        (s, e) => { if (CurrentTrack.Image != null) Dispatcher.Invoke(() => image = CurrentTrack.Image.Clone()); };
                    return;
                }
            }

            if (CurrentTrack.Image != null) image = CurrentTrack.Image.Clone();
        }

        public PlayableBase CurrentTrack { get; set; }

        private RelayCommand _saveImage;
        public RelayCommand SaveImage
        {
            get
            {
                return _saveImage ?? (_saveImage = new RelayCommand(parameter =>
                {
                    if (image != null)
                    {
                        var sfd = new Microsoft.Win32.SaveFileDialog()
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

                        encoder.Frames.Add(BitmapFrame.Create(image));

                        using (var filestream = new FileStream(sfd.FileName, FileMode.Create))
                            encoder.Save(filestream);
                    }
                }));
            }
        }
    }
}
