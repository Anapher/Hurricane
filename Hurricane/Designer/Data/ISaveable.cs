namespace Hurricane.Designer.Data
{
    public interface ISaveable
    {
        void Save(string path);
        string Filter { get; }
        string BaseDirectory { get; }
    }
}