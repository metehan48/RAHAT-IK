using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RAHATIK.Web.Models;
using RAHATIK.Web.Models.Entities;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace RAHATIK.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RepositoryContext _context;
        private string _url = "https://efatura.etrsoft.com";
        private string _token = "";
        public HomeController(ILogger<HomeController> logger, RepositoryContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            await Login();
            var getList = await GetData();
            foreach (var item in getList)
            {
                if (_context.TotalPrices.FirstOrDefault(x => x.AccountNumber == item.AccountNumber) == null)
                {
                    _context.TotalPrices.Add(new TotalPrice
                    {
                        Price = item.Price,
                        AccountNumber = item.AccountNumber
                    });
                    _context.SaveChanges();
                }
                else
                {
                    var edit = _context.TotalPrices.FirstOrDefault(x => x.AccountNumber == item.AccountNumber);
                    edit.Price = item.Price;
                    _context.SaveChanges();
                }
            }

            return View(getList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        async Task Login()
        {
            var username = "apitest";
            var password = "test123";
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            // Pass the handler to httpclient(from you are calling api)
            HttpClient client = new HttpClient(clientHandler);
            client.BaseAddress = new Uri(_url);
            client.DefaultRequestHeaders.Authorization
                = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));


            // Set the Content-Type header
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                var content = new StringContent("{}", Encoding.UTF8, "application/json");
                // Make the POST request
                HttpResponseMessage response = await client.PostAsync("/fmi/data/v1/databases/testdb/sessions", content);
                response.EnsureSuccessStatusCode();

                // Read the response content
                string responseBody = await response.Content.ReadAsStringAsync();
                // Deserialize the JSON data into the C# class
                GetResponse data = JsonConvert.DeserializeObject<GetResponse>(responseBody);

                // Access the deserialized data
                _token = data.response.token;
                List<Message> messages = data.messages;
            }
            catch (HttpRequestException exception)
            {
                // Handle any exceptions
                var message = exception.Message;
            }
        }

        async Task<List<TotalPrice>> GetData()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            HttpClient client = new HttpClient(clientHandler);
            client.BaseAddress = new Uri(_url);
            string jsonData = "{\"fieldData\": {}, \"script\" : \"getData\"}";
            // Set up the request headers
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            // Prepare the request body
            var content2 = new StringContent(jsonData, Encoding.UTF8, "application/json");

            // Send the PATCH request
            HttpResponseMessage response = await client.PatchAsync("/fmi/data/v1/databases/testdb/layouts/testdb/records/1", content2);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                GetResponse data = JsonConvert.DeserializeObject<GetResponse>(responseBody);
                List<DataResponse> scriptResult = JsonConvert.DeserializeObject<List<DataResponse>>(data.response.scriptResult);

                List<TotalPrice> list = scriptResult.Select(x => new TotalPrice
                {
                    AccountNumber = x.hesap_kodu,
                    Price = Convert.ToDecimal(string.IsNullOrEmpty(x.borc.ToString()) ? "0" : x.borc)
                }).ToList();

                List<TotalPrice> newList = list
                    .GroupBy(x => x.AccountNumber.Split('.')[0])
                    .Select(group => new TotalPrice
                    {
                        AccountNumber = group.Key,
                        Price = group.Sum(x => x.Price)
                    })
                    .ToList();

                newList.AddRange(list
                    .GroupBy(x => x.AccountNumber.Split('.')[0] + "." + x.AccountNumber.Split('.')[1])
                    .Select(group => new TotalPrice
                    {
                        AccountNumber = group.Key,
                        Price = group.Sum(x => x.Price)
                    }));

                newList.AddRange(list
                    .Where(x => x.AccountNumber.Split(".").Count() == 3)
                    .GroupBy(x => x.AccountNumber.Split('.')[0] + "." + x.AccountNumber.Split('.')[1] + "." + x.AccountNumber.Split('.')[2])
                    .Select(group => new TotalPrice
                    {
                        AccountNumber = group.Key,
                        Price = group.Sum(x => x.Price)
                    }));
                return newList = newList.OrderBy(x => x.Id).ToList();
            }
            else
            {
                //$"Error: {response.StatusCode}";
                return new List<TotalPrice>();
            }
        }

        public class Response
        {
            public string? token { get; set; }
            public string? scriptResult { get; set; }

        }

        public class Message
        {
            public string code { get; set; }
            public string message { get; set; }
        }

        public class GetResponse
        {
            public Response response { get; set; }
            public List<Message> messages { get; set; }
        }

        public class DataResponse
        {
            public int id { get; set; }
            public string hesap_kodu { get; set; }
            public string hesap_adi { get; set; }
            public string tipi { get; set; }
            public int ust_hesap_id { get; set; }
            public object borc { get; set; }
            public object alacak { get; set; }
            public object borc_sistem { get; set; }
            public object alacak_sistem { get; set; }
            public object borc_doviz { get; set; }
            public object alacak_doviz { get; set; }
            public object borc_islem_doviz { get; set; }
            public object alacak_islem_doviz { get; set; }
            public string birim_adi { get; set; }
            public int bakiye_sekli { get; set; }
            public int aktif { get; set; }
            public int dovizkod { get; set; }
        }
    }
}