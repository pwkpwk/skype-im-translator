namespace SkypeImTranslator.WebRole.Controllers
{
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
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
            UriBuilder ub = new UriBuilder(RoleEnvironment.GetConfigurationSettingValue("SkypeIm.Translator.WebRole.TranslatorUri"))
            {
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

                    using (JsonReader reader = new JsonTextReader(new StreamReader(await translationResponse.Content.ReadAsStreamAsync(), Encoding.UTF8)))
                    {
                        JToken jt = JToken.ReadFrom(reader);
                        JArray translations = jt.Value<JArray>("translations");
                        JArray responsePayload = new JArray();

                        foreach (JToken translation in translations)
                        {
                            responsePayload.Add(
                                new JObject(
                                    new JProperty("locale", translation.Value<string>("to"),
                                    new JProperty("text", translation.Value<string>("text")))));
                        }

                        response.Content = new StringContent(responsePayload.ToString(), Encoding.UTF8, "application/json");
                    }

                    return response;
                }
            }
        }
    }
}
 