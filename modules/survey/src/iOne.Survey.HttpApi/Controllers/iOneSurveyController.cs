using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Survey.Controllers;

public class iOneSurveyController : AbpControllerBase
{
    public ActionResult Index()
    {
        return Content("Hello from Survey Module!");
    }
}

