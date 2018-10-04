namespace OIDCPlay.Models
{
    public class OptionModel
    {
        public string Key { get; set; }
        public OptionType OptionType { get; set; }
        public string Name { get; set; }
        public bool Checked { get; set; }
        public bool HasArgument { get; set; }
        public string Argument { get; set; }
    }
}