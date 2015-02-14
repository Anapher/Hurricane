namespace Hurricane.Designer.Data
{
    public interface IThemeSetting
    {
        string DisplayName { get; set; }
        string ID { get; set; }
        string Value { get; }
        string RegexPattern { get; }
        void SetValue(string content);
    }
}
