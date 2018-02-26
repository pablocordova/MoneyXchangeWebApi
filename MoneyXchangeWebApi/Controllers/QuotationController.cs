using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MoneyXchangeWebApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ModelBinding;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MoneyXchangeWebApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class QuotationController : Controller
    {
        private readonly AppSettings _mySettings;
        private readonly ApplicationDbContext context;

        static HttpClient client = new HttpClient();

        public QuotationController(IOptions<AppSettings> settings, ApplicationDbContext context)
        {
            _mySettings = settings.Value;
            this.context = context;
        }

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> GetAsync(GetAsyncParams values)
        {
            // Object for error response
            dynamic errorResponse = new ExpandoObject();

            string keyAddress = "&api_key=" + _mySettings.apiKey;

            // Get from database Memory
            var dataRefences = context.References;
            string[] references = new string[dataRefences.Count()];
            for (int i = 0; i < dataRefences.Count(); i++)
            {
                references[i] = dataRefences.ToArray()[i].Symbol;
            }

            var dataExchanges = context.Exchanges;
            string[] exchanges = new string[dataExchanges.Count()];
            for (int i = 0; i < dataExchanges.Count(); i++)
            {
                exchanges[i] = dataExchanges.ToArray()[i].Symbol;
            }

            // Validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool isReferenceValid = references.Contains(values.reference);
            if (!isReferenceValid)
            {
                errorResponse.Error = "Symbol not permitted";
                errorResponse.message = values.reference;
                return BadRequest(errorResponse);
            }

            string[] symbols = values.exchanges.Split(',');

            for (int i = 0; i < symbols.Length; i++)
            {
                bool isExchangeValid = exchanges.Contains(values.exchanges);
                if (!isExchangeValid)
                {
                    errorResponse.Error = "Symbol not permitted";
                    errorResponse.message = values.exchanges;
                    return BadRequest(errorResponse);
                }
            }

            // Generate path to request
            string path = _mySettings.pathBase;

            for (int i = 0; i < symbols.Length; i++)
            {
                path += values.reference + symbols[i];
                if ((i + 1) != symbols.Length)
                {
                    path += ",";
                }
            }

            path += keyAddress;

            // Request to api serve
            HttpResponseMessage response = await client.GetAsync(path);
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();
            List<RequiredParams> model = null;
            model = JsonConvert.DeserializeObject<List<RequiredParams>>(content);

            // Generate response
            dynamic res = new ExpandoObject();
            res.reference = values.reference;
            res.date = model[0].timestamp;

            // Generate object with all symbols required
            var ratesObject = new ExpandoObject() as IDictionary<string, Object>;

            for (int i = 0; i < model.Count; i++)
            {
                ratesObject.Add(symbols[i], model[i].price);
            }

            res.rates = ratesObject;

            return Ok(res);
        }

    }

    public class GetAsyncParams
    {
        [BindRequired]
        [StringLength(3)]
        public string reference { get; set; }
        [MinLength(3)]
        public string exchanges { get; set; }
    }

    public class RequiredParams
    {
        [JsonProperty("price")]
        public double price { get; set; }
        [JsonProperty("timestamp")]
        public int timestamp { get; set; }
    }
}
