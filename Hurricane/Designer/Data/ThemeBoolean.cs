namespace Hurricane.Designer.Data
{
    public class ThemeBoolean : IThemeSetting
    {
        public string DisplayName { get; set; }
        public string ID { get; set; }
        public string RegexPattern { get; set; }

        public bool BooleanValue { get; set; }

        public string Value
        {
            get { return BooleanValue.ToString().ToLower(); }
        }

        public void SetValue(string content)
        {
            BooleanValue = bool.Parse(content);
        }
    }
}
