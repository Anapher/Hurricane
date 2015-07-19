using System.Collections.Generic;
using System.Diagnostics;

namespace Hurricane.ViewModel
{
    public class AboutViewModel
    {
        private RelayCommand _openHyperlinkCommand;

        public AboutViewModel()
        {
            Libraries = new List<LibraryInfo>
            {
                new LibraryInfo
                {
                    Name = "CSCore – .NET Sound Library",
                    Description = "CSCore is a free .NET audio library which is completely written in C#.",
                    Website = "https://github.com/filoe/cscore",
                    LicenceUrl = "https://github.com/filoe/cscore/blob/master/license.md"
                },
                new LibraryInfo
                {
                    Name = "GongSolutions.WPF.DragDrop",
                    Description = "The GongSolutions.WPF.DragDrop library is a drag'n'drop framework for WPF",
                    Website = "https://punker76.github.io/gong-wpf-dragdrop/",
                    LicenceUrl = "https://github.com/punker76/gong-wpf-dragdrop/blob/master/LICENSE"
                },
                new LibraryInfo
                {
                    Name = "Hardcodet.Wpf.TaskbarNotification",
                    Description =
                        "It's an implementation of a NotifyIcon (aka system tray icon or taskbar icon) for the WPF platform.",
                    Website = "http://www.hardcodet.net/wpf-notifyicon",
                    LicenceUrl = "http://www.codeproject.com/info/cpol10.aspx"
                },
                new LibraryInfo
                {
                    Name = "Json.NET",
                    Description = "Popular high-performance JSON framework for .NET",
                    Website = "http://www.newtonsoft.com/json",
                    LicenceUrl = "https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md"
                },
                new LibraryInfo
                {
                    Name = "MahApps.Metro",
                    Description = "A toolkit for creating metro-style WPF applications.",
                    Website = "http://mahapps.com/",
                    LicenceUrl = "https://github.com/MahApps/MahApps.Metro/blob/master/LICENSE"
                },
                new LibraryInfo
                {
                    Name = "Ookii.Dialogs",
                    Description = "Ookii.Dialogs is a class library for .Net applications providing several common dialogs.",
                    Website = "http://www.ookii.org/software/dialogs/",
                    LicenceUrl = "https://github.com/marklagendijk/WinLess/blob/master/packages/Ookii.Dialogs.1.0/license.txt"
                },
                new LibraryInfo
                {
                    Name = "TagLib#",
                    Description =
                        "TagLib# (aka taglib-sharp) is a library for reading and writing metadata in media files, including video, audio, and photo formats.",
                    Website = "https://github.com/mono/taglib-sharp",
                    LicenceUrl = "http://opensource.org/licenses/lgpl-2.1.php"
                }
            };

            Images = new List<ImageInfo>
            {
                new ImageInfo
                {
                    Name = "Ionicons",
                    Licence = "http://opensource.org/licenses/MIT",
                    Website = "http://ionicons.com/"
                },
                new ImageInfo
                {
                    Name = "Linh Pham Thi Dieu",
                    Licence = null,
                    Website = "http://linhpham.me/"
                },
                new ImageInfo
                {
                    Name = "SimpleIcon",
                    Licence = "https://creativecommons.org/licenses/by/3.0/",
                    Website = "http://www.flaticon.com/authors/simpleicon"
                },
                new ImageInfo
                {
                    Name = "Spinking",
                    Licence = "https://creativecommons.org/licenses/by/3.0/",
                    Website = "http://vk.com/bashkatovsm"
                },
                new ImageInfo
                {
                    Name = "Designmodo",
                    Licence = "https://creativecommons.org/licenses/by/3.0/",
                    Website = "http://designmodo.com/"
                }
            };
        }

        public List<LibraryInfo> Libraries { get; set; }
        public List<ImageInfo> Images { get; set; }

        public RelayCommand OpenHyperlinkCommand
        {
            get
            {
                return _openHyperlinkCommand ?? (_openHyperlinkCommand = new RelayCommand(parameter =>
                {
                    Process.Start(parameter.ToString());
                }));
            }
        }
    }

    public class LibraryInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Website { get; set; }
        public string LicenceUrl { get; set; }
    }

    public class ImageInfo
    {
        public string Name { get; set; }
        public string Website { get; set; }
        public string Licence { get; set; }
    }
}