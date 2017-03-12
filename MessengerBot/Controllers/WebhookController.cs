using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using MessengerBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MessengerBot.Controllers
{
	public class WebhookController : ApiController
	{
		private const string pageToken = "EAAC47TZC6nG4BAI8dOuNyA6jafwwxD4abGEZBGa2PLDrY3hGUrZCeD8Yot2YeZBK44PfFhieIOofgOKNZBahDth1J30ZCQ5sY0ZA9VZA3eBvbOZCBAZBy8HZC8xqZAeci7l6r3lBVtYqUcvW9Pmv9Tx2pKZAW4g47P3I1f0KtK1ZCJZAZAlZC3wZDZD";
        private const string appSecret = "7a3277e97737b4e4500e7c902e096e72";

        private readonly Dictionary<string, string> imagenes = new Dictionary<string, string>()
        {
            { "gazelle", "http://193.146.116.151/images2/BB5476_01_standard.jpg" },
            { "ultraboost", "http://193.146.116.151/images2/BA8278_01_standard.jpg" },
            { "superstar", "http://193.146.116.151/images2/C77124_01_standard.jpg" },
            { "supernova", "http://193.146.116.151/images2/BB1612_01_standard.jpg" },
            { "stansmith", "http://193.146.116.151/images2/M20324_01_standard.jpg" }
        };

        private const string ADIDAS_URL_SEARCH = "http://www.adidas.es/search?q=calzado";

        public HttpResponseMessage Get()
		{
			var querystrings = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
			if (querystrings["hub.verify_token"] == "uCode_FJ_Fans")
			{
				return new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(querystrings["hub.challenge"], Encoding.UTF8, "text/plain")
				};
			}
			return new HttpResponseMessage(HttpStatusCode.Unauthorized);
		}

		[HttpPost]
		public async Task<HttpResponseMessage> Post()
		{
			var signature = Request.Headers.GetValues("X-Hub-Signature").FirstOrDefault().Replace("sha1=", "");
			var body = await Request.Content.ReadAsStringAsync();
			if (!VerifySignature(signature, body))
				return new HttpResponseMessage(HttpStatusCode.BadRequest);

			var value = JsonConvert.DeserializeObject<WebhookModel>(body);
			if (value._object != "page")
				return new HttpResponseMessage(HttpStatusCode.OK);

			foreach (var item in value.entry[0].messaging)
			{
                if (item.message.attachments != null)
                {
                    if (item.message.attachments[0].type == "image")
                    {
                        try
                        {
                            var imageData = TestController.ObtenerDatosImagen(item.message.attachments[0].payload.url);
                            if(imageData != null && imageData.Count > 0 && imageData[0].score > 0.5)
                            {
                                string message = String.Format("Su zapatilla corresponde al modelo {0}. ", //Puede encontrar más información en {1}",
                                    imageData[0].zapatilla, ADIDAS_URL_SEARCH + "+" + imageData[0].zapatilla);
                                //await SendMessage(GetMessageTemplate(message, item.sender.id));

                                await SendMessage(GetComplexTemplate(imageData[0].zapatilla, message, item.sender.id, ADIDAS_URL_SEARCH + "+" + imageData[0].zapatilla));
                            }
                            else
                            {
                                string message = String.Format("No se ha podido determinar el modelo de su zapatilla." +
                                    "Puede encontrar más información en {0} {1}", ADIDAS_URL_SEARCH, item.message.attachments[0].payload.url);
                                await SendMessage(GetMessageTemplate(message, item.sender.id));
                            }
                        }
                        catch(Exception ex)
                        {
                            string message = String.Format("No se ha podido determinar el modelo de su zapatilla." +
                                "Puede encontrar más información en {0} {1}", ADIDAS_URL_SEARCH, item.message.attachments[0].payload.url);
                            await SendMessage(GetMessageTemplate(message, item.sender.id));
                        }                        
                    }
                }
                else
                {
                    if (item.message == null && item.postback == null)
                        continue;
                    else
                        await SendMessage(GetMessageTemplate("Hola, le atiende el asistente de imágenes de Adidas. " +
                            "Envíe una imagen para poder analizarla", item.sender.id));
                }
			}

			return new HttpResponseMessage(HttpStatusCode.OK);
		}

		private bool VerifySignature(string signature, string body)
		{
			var hashString = new StringBuilder();
			using (var crypto = new HMACSHA1(Encoding.UTF8.GetBytes(appSecret)))
			{
				var hash = crypto.ComputeHash(Encoding.UTF8.GetBytes(body));
				foreach (var item in hash)
					hashString.Append(item.ToString("X2"));
			}

			return hashString.ToString().ToLower() == signature.ToLower();
		}

		/// <summary>
		/// get text message template
		/// </summary>
		/// <param name="text">text</param>
		/// <param name="sender">sender id</param>
		/// <returns>json</returns>
		private JObject GetMessageTemplate(string text, string sender)
		{
			return JObject.FromObject(new
			{
				recipient = new { id = sender },
				message = new { text = text }
			});
		}

		/// <summary>
		/// send message
		/// </summary>
		/// <param name="json">json</param>
		private async Task SendMessage(JObject json)
		{
			using (HttpClient client = new HttpClient())
			{
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				HttpResponseMessage res = await client.PostAsync($"https://graph.facebook.com/v2.6/me/messages?access_token={pageToken}", 
                    new StringContent(json.ToString(), Encoding.UTF8, "application/json"));
			}
		}


        private JObject GetComplexTemplate(string model, string text, string sender, string url)
        {
            return JObject.FromObject(GetComplexTemplateObject(model, text, sender, url));
        }

        /// <summary>
		/// get text message template
		/// </summary>
		/// <param name="text">text</param>
		/// <param name="sender">sender id</param>
		/// <returns>json</returns>
		private ComplexMessage GetComplexTemplateObject(string model, string text, string sender, string url)
        {
            return new ComplexMessage()
            {
                recipient = new Recipient()
                {
                    id = sender
                },
                message = new ExtraMessage()
                {
                    attachment = new AttachmentMessage()
                    {
                        type = "template",
                        payload = new PayloadMessage()
                        {
                            template_type = "generic",
                            elements = new List<ElementMessage>()
                            {
                                new ElementMessage()
                                {
                                    title = model,
                                    subtitle = text,
                                    item_url = url,
                                    image_url = imagenes[model],
                                    buttons = new List<ButtonMessage>()
                                    {
                                        new ButtonMessage()
                                        {
                                            type = "web_url",
                                            url = url,
                                            title = "Ver catálogo WEB"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
	}
}

