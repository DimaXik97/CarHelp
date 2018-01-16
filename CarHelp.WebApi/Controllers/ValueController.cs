using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarHelp.WebApi.Controllers
{
    [Route("api/value")]
    public class ValueController : Controller
    {
        [Authorize]
        [HttpGet]
        public string Get()
        {
            Debug.WriteLine("OK");
            return "OK";
        }
    }
}