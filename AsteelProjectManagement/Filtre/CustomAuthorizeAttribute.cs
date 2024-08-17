using AsteelProjectManagement.Models;
using System.Linq;
using System.Web;
using System.Web.Mvc;


public class CustomAuthorizeAttribute : AuthorizeAttribute
{
    private readonly int _requiredRoleId;

    public CustomAuthorizeAttribute(int requiredRoleId)
    {
        _requiredRoleId = requiredRoleId;
    }

    protected override bool AuthorizeCore(HttpContextBase httpContext)
    {
        var db = new PrjContext(); // Remplacez par votre contexte de données

        var userName = httpContext.User.Identity.Name;
        var user = db.Users.SingleOrDefault(u => u.Username == userName);

        if (user == null)
        {
            return false;
        }

        return db.UserRoleAssignments
            .Any(ra => ra.UserID == user.UserID && ra.RoleID == _requiredRoleId);
    }

    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    {
        if (filterContext.HttpContext.User.Identity.IsAuthenticated)
        {
            filterContext.Result = new RedirectToRouteResult(
                new System.Web.Routing.RouteValueDictionary
                {
                    { "controller", "Tasks" },
                    { "action", "AccessDenied" }
                });
        }
        else
        {
            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}
