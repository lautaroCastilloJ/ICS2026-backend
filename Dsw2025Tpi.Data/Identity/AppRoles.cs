
namespace Dsw2025Tpi.Data.Identity;

public static class AppRoles
{
    public const string Administrador = "Administrador";
    public const string Cliente = "Cliente";

    public static readonly string[] AllowedRoles = { Administrador, Cliente };
}
