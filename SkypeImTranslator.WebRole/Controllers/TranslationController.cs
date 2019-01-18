namespace SkypeImTranslator.WebRole.Controllers
{
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    [RoutePrefix("api/v1")]
    public class TranslationController : ApiController
    {
        private readonly HttpClient _httpClient;

        public TranslationController(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

#pragma warning disable SG0016 // Controller method is vulnerable to CSRF
        [HttpPost, Route("translate")]
        public async Task<HttpResponseMessage> Translate(CancellationToken cancellationToken)
        {
            using (JsonReader reader = new JsonTextReader(new StreamReader(await Request.Content.ReadAsStreamAsync(), Encoding.UTF8)))
            {
                return await Translate(JToken.Load(reader).ToObject<Data.TextTranslationRequestData>(), cancellationToken);
            }
        }
#pragma warning restore SG0016 // Controller method is vulnerable to CSRF

        private async Task<HttpResponseMessage> Translate(Data.TextTranslationRequestData request, CancellationToken cancellationToken)
        {
            string translationKey = RoleEnvironment.GetConfigurationSettingValue("SkypeIm.Translator.WebRole.TranslationKey");

            StringBuilder query = new StringBuilder("api-version=3.0");
            foreach (string locale in request.TargetLocales)
            {
                query.AppendFormat("&to={0}", locale);
            }
            UriBuilder ub = new UriBuilder("https", "api.cognitive.microsofttranslator.com")
            {
                Path = "translate",
                Query = query.ToString()
            };

            using (HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, ub.Uri))
            {
                JArray payload = new JArray();
                payload.Add(new JObject(new JProperty("Text", request.Text)));
                httpRequest.Content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");
                httpRequest.Headers.Add("Ocp-Apim-Subscription-Key", translationKey);

                HttpResponseMessage translationResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (!translationResponse.IsSuccessStatusCode)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal error.");
                }
                else
                {
                    HttpResponseMessage response = Request.CreateResponse();

                    response.Content = new StreamContent(await translationResponse.Content.ReadAsStreamAsync());
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    return response;
                }
            }
        }
    }
}
 