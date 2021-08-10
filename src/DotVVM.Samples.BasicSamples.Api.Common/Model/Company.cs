namespace DotVVM.Samples.BasicSamples.Api.Common.Model
{
    public class Company<T>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Owner { get; set; }

        public T Department { get; set; }
    }
}
