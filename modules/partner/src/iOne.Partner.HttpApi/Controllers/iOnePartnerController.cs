using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Partner.Controllers;

public class iOnePartnerController : AbpControllerBase
{
    public ActionResult Index()
    {
        return Content("Hello from Partner Module!");
    }
}

