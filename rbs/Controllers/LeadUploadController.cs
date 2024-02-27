namespace plweb.Controllers
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class LeadUploadController : ControllerBase
    {
        [HttpPost]
        public ActionResult UploadFile(IFormFile file)
        {
            var output = string.Empty;
            if (file != null)
            {
                var fagent = new FileUploadAgent();
                output = fagent.ImportLeadsViaFile(file);
            }
            return Ok(output);
        }
    }
}
