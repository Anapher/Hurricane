using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace Hurricane.Extensions
{
    /// <summary>
    /// Classe utilitaire permettant d'utiliser un texte à la place d'une ImageSource
    /// 
    /// ImageSource="{Tools:ImageFromFont Text=&#xf01a;, FontFamily=/GestionDesComptes;component/Resources/#FontAwesome, Brush=HotPink, Weight=ExtraBold}"
    /// </summary>
    public class ImageFromFont : MarkupExtension
    {
        /// <summary>
        /// Obtient/défini le texte affiché
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Obtient/défini la police de caractères à utiliser
        /// </summary>
        public FontFamily FontFamily { get; set; }

        /// <summary>
        /// Obtient/défini le style de la police
        /// </summary>
        public FontStyle Style { get; set; }

        /// <summary>
        /// Obtient/défini l'épaisseur de la police
        /// </summary>
        public FontWeight Weight { get; set; }

        /// <summary>
        /// Obtient/défini l'étirement des caractères de la police
        /// </summary>
        public FontStretch Stretch { get; set; }

        /// <summary>
        /// Obtient/défini le pinceau utilisé pour dessiner le texte
        /// </summary>
        public Brush Brush { get; set; }

        /// <summary>
        /// Constructeur 
        /// </summary>
        public ImageFromFont()
        {
            Text = "G";
            FontFamily = new FontFamily("Segoe UI Symbol");
            Style = FontStyles.Normal;
            Weight = FontWeights.Normal;
            Stretch = FontStretches.Normal;
            Brush = new SolidColorBrush(Colors.Black);
        }

        /// <summary>
        /// Méthode créé une image source à partir d'un texte (avec ses aramètres : font, style,...)
        /// </summary>
        /// <param name="text">le texte</param>
        /// <param name="fontFamily">la police</param>
        /// <param name="fontStyle">le style de la police</param>
        /// <param name="fontWeight">l'épaisseur de la police</param>
        /// <param name="fontStretch">l'étirement de la police</param>
        /// <param name="foreBrush">la pinceau utilisé pour déssiner le texte</param>
        /// <returns>une image source</returns>
        private static ImageSource CreateGlyph(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, Brush foreBrush)
        {
            if (fontFamily != null && !String.IsNullOrEmpty(text))
            {
                //premier essai, on charge la police directement
                Typeface typeface = new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);

                GlyphTypeface glyphTypeface;
                if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
                {
                    //si ça ne fonctionne pas (et pour le mode design dans certains cas) on ajoute l'uri pack://application
                    typeface = new Typeface(new FontFamily(new Uri("pack://application:,,,"), fontFamily.Source), fontStyle, fontWeight, fontStretch);
                    if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
                        throw new InvalidOperationException("No glyphtypeface found");
                }

                //détermination des indices/tailles des caractères dans la police
                ushort[] glyphIndexes = new ushort[text.Length];
                double[] advanceWidths = new double[text.Length];

                for (int n = 0; n < text.Length; n++)
                {
                    ushort glyphIndex;
                    try
                    {
                        glyphIndex = glyphTypeface.CharacterToGlyphMap[text[n]];

                    }
                    catch (Exception)
                    {
                        glyphIndex = 42;
                    }
                    glyphIndexes[n] = glyphIndex;

                    double width = glyphTypeface.AdvanceWidths[glyphIndex] * 1.0;
                    advanceWidths[n] = width;
                }

                try
                {

                    //création de l'objet DrawingImage (compatible avec Imagesource) à partir d'un glyphrun
                    GlyphRun gr = new GlyphRun(glyphTypeface, 0, false, 1.0, glyphIndexes,
                                               new Point(0, 0), advanceWidths, null, null, null, null, null, null);

                    GlyphRunDrawing glyphRunDrawing = new GlyphRunDrawing(foreBrush, gr);
                    return new DrawingImage(glyphRunDrawing);
                }
                catch (Exception ex)
                {
                    // ReSharper disable LocalizableElement
                    Console.WriteLine("Error in generating Glyphrun : " + ex.Message);
                    // ReSharper restore LocalizableElement
                }
            }
            return null;
        }

        /// <summary>
        /// Génère l'image source à partir des paramètres
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return CreateGlyph(Text, FontFamily, Style, Weight, Stretch, Brush);
        }
    }
}
