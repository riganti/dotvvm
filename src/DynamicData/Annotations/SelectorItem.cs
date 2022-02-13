namespace DotVVM.Framework.Controls.DynamicData.Annotations;

public abstract record SelectorItem
{
    public string DisplayName { get; set; }

    private protected abstract void SorryWeCannotAllowYouToInheritThisClass();
}

public abstract record SelectorItem<TKey> : SelectorItem
{
    public TKey Id { get; set; }

    private protected override void SorryWeCannotAllowYouToInheritThisClass() => throw new System.NotImplementedException("Mischief managed.");
}
