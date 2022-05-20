namespace DotVVM.AutoUI.Annotations;

public abstract record Selection
{
    public string DisplayName { get; set; }

    private protected abstract void SorryWeCannotAllowYouToInheritThisClass();
}

public abstract record Selection<TKey> : Selection
{
    public TKey Value { get; set; }

    private protected override void SorryWeCannotAllowYouToInheritThisClass() => throw new System.NotImplementedException("Mischief managed.");
}
