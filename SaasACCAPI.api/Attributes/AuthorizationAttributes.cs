using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace SaasACCAPI.api.Attributes;

public class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _allowedRoles;

    public RequireRoleAttribute(params string[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Usuario no autenticado" });
            return;
        }

        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
        
        if (string.IsNullOrEmpty(userRole) || !_allowedRoles.Contains(userRole))
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}

public class RequireComercioAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Usuario no autenticado" });
            return;
        }

        var comercioIdClaim = user.FindFirst("ComercioId")?.Value;
        
        if (string.IsNullOrEmpty(comercioIdClaim))
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Usuario sin comercio asignado" });
            return;
        }
    }
}
