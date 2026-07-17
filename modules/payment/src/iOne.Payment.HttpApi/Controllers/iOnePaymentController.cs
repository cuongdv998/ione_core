using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Payment.Controllers;

public class iOnePaymentController : AbpControllerBase
{
    public ActionResult Index()
    {
        return Content("Hello from Payment Module!");
    }
}

