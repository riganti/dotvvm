namespace DotVVM.Framework.Testing.Generator.Modifications
{
    public class UITestNameMarkupFileInsertText : MarkupFileInsertText
    {
        private string _uniqueName;

        public string UniqueName
        {
            get
            {
                return _uniqueName;
            }
            set
            {
                Text = " UITests.Name=\"" + value + "\"";
                _uniqueName = value;
            }
        }
    }
}
