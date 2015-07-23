using System.Windows;
using System.Windows.Controls;
using Hurricane.Model.Music.Playable;

namespace Hurricane.Extensions
{
    class PlayingTrackTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PlayableDataTemplate { get; set; }
        public DataTemplate StreamableDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is IStreamable)
                return StreamableDataTemplate;

            return PlayableDataTemplate;
        }
    }
}