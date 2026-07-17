using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Finance.Controllers;

public class iOneFinanceController : AbpControllerBase
{
    public ActionResult Index()
    {
        return Content("Hello from Finance Module!");
    }
}

