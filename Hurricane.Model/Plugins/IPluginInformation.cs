namespace Hurricane.Model.Plugins
{
    public interface IPluginInformation
    {
        bool IsEnabled { get; set; }
        string Path { get; set; }
    }
}
