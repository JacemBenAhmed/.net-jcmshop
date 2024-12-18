using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using projTP.Models;

namespace projTP.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            string userName = "Anonyme";
            bool isAuthenticated = false;

            var token = Request.Cookies["authToken"];
            if (!string.IsNullOrEmpty(token))
            {
                isAuthenticated = true;

                var jwtHandler = new JwtSecurityTokenHandler();
                try
                {
                    var jwtToken = jwtHandler.ReadJwtToken(token);
                    var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                    if (nameClaim != null)
                    {
                        userName = nameClaim.Value;
                    }
                }
                catch (Exception)
                {
                    isAuthenticated = false;
                }
            }

            


            ViewData["UserName"] = userName;
            ViewData["IsAuthenticated"] = isAuthenticated;
        }

    }
}