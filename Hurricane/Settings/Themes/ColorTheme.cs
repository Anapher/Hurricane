using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
namespace Hurricane.Settings.Themes
{
    [Serializable]
    public class ColorTheme
    {
        public string FileName { get; set; }

        [XmlIgnore]
        public string Name
        {
            get
            {
                return System.Windows.Application.Current.FindResource(FileName).ToString();
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as ColorTheme;
            if (other == null) return false;
            return other.FileName == this.FileName;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
