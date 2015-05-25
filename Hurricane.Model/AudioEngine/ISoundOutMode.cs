using System.Collections.ObjectModel;

namespace Hurricane.Model.AudioEngine
{
    public interface ISoundOutMode
    {
        string Name { get; }
        ObservableCollection<ISoundOutDevice> Devices { get; }
    }
}