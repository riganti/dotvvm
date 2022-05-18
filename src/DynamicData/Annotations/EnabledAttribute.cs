using System;

namespace DotVVM.AutoUI.Annotations;

/// <summary>
/// Indicates under which conditions the auto-generated field should be visible to the user.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class EnabledAttribute : Attribute, IConditionalFieldAttribute
{

    /// <summary>
    /// Gets or sets a name of the view or an expression that specifies for which views the field should be editable.
    /// You can use ! (NOT), &amp; (AND) and | (OR) operators.
    /// Examples:
    /// - Insert            // editable only in the Insert view
    /// - Insert | Edit     // editable in Insert or Edit views
    /// - !Insert &amp; !Edit   // editable in all views except for Insert and Edit
    /// </summary>
    public string ViewNames { get; set; }

    /// <summary>
    /// Gets or sets a name of the role or an expression that specifies for which roles the field should be editable.
    /// You can use ! (NOT), &amp; (AND) and | (OR) operators.
    /// Examples:
    /// - Admin                   // editable only for the Admin role
    /// - Admin | Contributor     // editable for Admin or Contributor roles
    /// - !Admin &amp; !Contributor   // editable for all roles except for Admin or Contributor roles
    /// </summary>
    public string Roles { get; set; }

    /// <summary>
    /// Gets or sets whether the field should be editable for authenticated or non-authenticated users, or null for both kinds (default behavior).
    /// </summary>
    public AuthenticationMode IsAuthenticated { get; set; }

}
