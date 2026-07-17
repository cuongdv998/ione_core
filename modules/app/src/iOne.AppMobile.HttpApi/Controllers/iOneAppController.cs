using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.AppMobile.Controllers;

public class iOneAppMobileController : AbpControllerBase
{
    public ActionResult Index()
    {
        return Content("Hello from AppMobile Module!");
    }
}
