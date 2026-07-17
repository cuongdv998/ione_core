using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Policy.Controllers;

public class iOnePolicyController : AbpControllerBase
{
    public ActionResult Index()
    {
        return Content("Hello from Policy Module!");
    }
}

