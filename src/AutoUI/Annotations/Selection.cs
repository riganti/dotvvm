namespace DotVVM.AutoUI.Annotations;

/// <summary>
/// Base class for selectable items, please prefer to derive from <see cref="Selection{TKey}" />
/// </summary>
public abstract record Selection
{
    /// <summary> The label to display in the selector component </summary>
    public string DisplayName { get; set; }

    private protected abstract void SorryWeCannotAllowYouToInheritThisClass();
}

/// <summary>
/// Base class for selectable items. See also <see cref="SelectionAttribute" />
/// </summary>
/// <typeparam name="TKey">Type of the value (the identifier). The property labeled with [Selection(typeof(This))] will have to be of type <typeparamref name="TKey"/></typeparam>
/// <example>
/// public record ProductSelection : Selection&lt;Guid&gt;;
/// // and then ...
/// public class ProductSelectionProvider : ISelectionProvider&lt;ProductSelection&gt;
/// {
///     public Task&lt;List&lt;ProductSelection&gt;&gt; GetSelectorItems() =>
///         Task.FromResult(new() {
///             new ProductSelection() { Value = new Guid("00000000-0000-0000-0000-000000000001"), DisplayName = "First product" },
///         });
/// }
/// </example>
public abstract record Selection<TKey> : Selection
{
    /// <summary> The value identifying this selection item </summary>
    public TKey Value { get; set; }

    private protected override void SorryWeCannotAllowYouToInheritThisClass() => throw new System.NotImplementedException("Mischief managed.");
}
