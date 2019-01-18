namespace SkypeImTranslator.WebRole.Controllers
{
    using System.Collections.Generic;
    using System.Web.Http;

    [RoutePrefix("api/v1")]
    public class TranslationController : ApiController
    {
        [Route("list")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
    }
}
