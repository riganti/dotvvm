using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ViewModelProtection
{
    public class ComplexViewModelProtectionViewModel : DotvvmViewModelBase
    {
        public const string OriginalText = "Lorem Ipsum Dolor Sit Amet";

        public string ChangedText => "The quick brown fox jumps over the lazy dog";

        [Bind(Direction.Both)]
        public Message BothMessage { get; set; } = new Message(OriginalText);

        [Bind(Direction.ClientToServer)]
        public Message ClientToServerMessage { get; set; } = new Message(OriginalText);

        [Bind(Direction.IfInPostbackPath)]
        public Message IfInPostbackPathMessage { get; set; } = new Message(OriginalText);

        [Bind(Direction.None)]
        public Message NoneMessage { get; set; } = new Message(OriginalText);

        [Bind(Direction.ServerToClientFirstRequest)]
        public Message ServerToClientFirstRequestMessage { get; set; } = new Message(OriginalText);

        [Bind(Direction.ServerToClient)]
        public Message ServerToClientMessage { get; set; } = new Message(OriginalText);

        [Bind(Direction.ServerToClientPostback)]
        public Message ServerToClientPostbackMessage { get; set; } = new Message(OriginalText);

        [Protect(ProtectMode.SignData)]
        public string SignedColor { get; set; } = "red";

        public class Message
        {
            public Message(string text)
            {
                Text = text;
            }

            public string Text { get; set; }
        }
    }
}