using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Master.Controllers;

public class iOneMasterController : AbpControllerBase
{
    public ActionResult Index()
    {
        return Content("Hello from Master Module!");
    }
}

