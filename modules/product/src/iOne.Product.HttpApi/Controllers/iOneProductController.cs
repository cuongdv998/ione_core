using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Product.Controllers;

public class iOneProductController : AbpControllerBase
{
    public ActionResult Index()
    {
        return Content("Hello from Product Module!");
    }
}

