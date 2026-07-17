using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Report.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class iOneReportController : AbpControllerBase
{
    [NonAction]
    public ActionResult Index()
    {
        return Content("Hello from Report Module!");
    }
}

