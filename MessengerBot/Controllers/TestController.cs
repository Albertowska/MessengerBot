using MessengerBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Http;

namespace MessengerBot.Controllers
{
    public class TestController : ApiController
    {
        public const string SERVICIO = "http://193.146.116.151/post_image2.php";

        public const string TEST = "https://scontent.xx.fbcdn.net/v/t35.0-12/17273640_10208376781352370_2047908941_o.jpg?_nc_ad=z-m&oh=d7380e1a67354bd082d724880a38a792&oe=58C5EF66";

        public HttpResponseMessage Get(string url)
        {
            try
            {
                var data = ObtenerDatosImagen(url);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(data.ToString(), Encoding.UTF8, "text/plain")
                };
            }
            catch(Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message)
                };
            }            
        }

        public static List<ResultadoImagen> ObtenerDatosImagen(string url)
        {
            try
            {
                string resultadoImagenList = null;

                using (HttpClient client = new HttpClient())
                {
                    var formContent = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("url", url)
                    });

                    HttpResponseMessage httpResponse = client.PostAsync(SERVICIO, formContent).Result;
                    resultadoImagenList = httpResponse.Content.ReadAsStringAsync().Result;
                }

                return JsonConvert.DeserializeObject<List<ResultadoImagen>>(resultadoImagenList);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
