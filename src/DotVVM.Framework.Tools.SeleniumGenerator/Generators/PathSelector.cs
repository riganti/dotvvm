namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators
{
    public class PathSelector
    {
        public string UiName { get; set; }

        public int? Index { get; set; }

        public PathSelector Parent { get; set; }

        public override string ToString()
        {
            if (Parent != null)
            {
                return Parent.ToString() + "_" + UiName;
            }

            return UiName;
        }
    }
}
