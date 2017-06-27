using DotVVM.Framework.ViewModel;
using System.Collections.Generic;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ViewModelProtection
{
    public class ComplexViewModelProtectionViewModel : DotvvmViewModelBase
    {
        [Protect(ProtectMode.SignData)]
        public List<string> Seasons { get; set; } = new List<string> { "Spring", "Summer", "Autumn", "Winter" };

        [Protect(ProtectMode.SignData)]
        public string SelectedColor { get; set; } = "red";

        [Bind(Direction.ServerToClient)]
        public Message TestMessage { get; set; } = new Message();

        public Song TestSong { get; set; } = new Song();

        public class Message
        {
            [Protect(ProtectMode.SignData)]
            public string Text { get; set; } = "Sample text";

            public Song AnotherSong { get; set; }
        }

        public class Song
        {
            [Protect(ProtectMode.SignData)]
            public string Author { get; set; } = "John Smith";

            public string Title { get; set; } = "A Song";
        }
    }
}