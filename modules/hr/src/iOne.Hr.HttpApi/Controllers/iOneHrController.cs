using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Hr.Controllers;

public class iOneHrController : AbpControllerBase
{
    public ActionResult Index()
    {
        return Content("Hello from HR Module!");
    }
}

