namespace DotVVM.AutoUI.Annotations;

public interface IConditionalFieldAttribute
{
    string ViewNames { get; set; }
    string Roles { get; set; }
    AuthenticationMode IsAuthenticated { get; set; }
}
