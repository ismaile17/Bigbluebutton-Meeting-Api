using MediatR;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    public class BaseController : ControllerBase
    {
        private IMediator _mediator;

        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();

        [ApiExplorerSettings(IgnoreApi = true)]
        [DisplayName("Login olan App User Id Verir")]
        public int GetUserId()
        {
            var claims = from c in User.Claims.Where(s => s.Type == "UserId") select new { c.Type, c.Value };
            return !claims.Any() ? 0 : Convert.ToInt32(claims?.FirstOrDefault().Value);
        }

   
        [ApiExplorerSettings(IgnoreApi = true)]
        [DisplayName("Login olan EmployeeId Verir")]
        public int GetEmployeeId()
        {
            var claims = from c in User.Claims.Where(s => s.Type == "employeeId") select new { c.Type, c.Value };
            return !claims.Any() ? 0 : Convert.ToInt32(claims?.FirstOrDefault().Value);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [DisplayName("Login olan CompanyId Verir")]
        public int GetCompanyId()
        {
            var claims = from c in User.Claims.Where(s => s.Type == "companyId") select new { c.Type, c.Value };
            return !claims.Any() ? 0 : Convert.ToInt32(claims?.FirstOrDefault().Value);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public string[] GetRoles()
        {
            var claims = from c in User.Claims.Where(s => s.Type.Contains("role")) select new { c.Type, c.Value };
            if (claims == null)
                return null;
            return claims.Select(f => f.Value).ToArray();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public int GetRoleId()
        {
            var claims = from c in User.Claims.Where(s => s.Type.Contains("role")) select new { c.Type, c.Value };
            if (claims == null)
                return 0;
            return claims.Select(f => Convert.ToInt32(f.Value)).FirstOrDefault();
        }
    }
}
