using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Customer.Controllers;

public class iOneCustomerController : AbpControllerBase
{
    public ActionResult Index()
    {
        return Content("Hello from Customer Module!");
    }
}

