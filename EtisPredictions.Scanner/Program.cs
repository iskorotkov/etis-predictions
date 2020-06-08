using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace EtisPredictions.Scanner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var web = new HtmlWeb();
            var loginUrl = "https://student.psu.ru/pls/stu_cus_et/stu.login";
            var loginDoc = await web.LoadFromWebAsync(loginUrl);
            // Console.WriteLine(loginDoc.ParsedText);

            var redirect = "stu.signs";
            var username = "Коротков";
            var password = "hpDln71pPmwf";
            var recaptchaResponse =
                "03AGdBq26zvMFjDsm84hYc2hHfnMhzui9hzOiDjrvvaFBWeR3JFuYv-0kH_grB984ydv6BdWOlFIPSiHsadn_2P8VBA-N7w0k4LIxLHeH95iHu5wW5mUEay9YdM6z1-02L782xef-ZrKbIzUih0qnFFkZKvsM7P9EIDsePy819gk_0TSYU5TyEISNqExzmKm8K07o0x3kSuVo9SS6fh7y5EWSuDH2XJIgKsY-KFq_N9Qe7q0sBWhrLdXAf2lGFO8JJi2c9wGihVck_yN1leid7OdilT-avSJpkSO2eql_Z1jin6RDxDf0xaPE0KgPFNAN43KQ-JMaqyJaSYyCYC4yNfXZ_Xh361ULUNT7JhbgBFEemT8GNx4aAMLs";
            var formEntries = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("p_redirect", redirect),
                new KeyValuePair<string, string>("p_username", username),
                new KeyValuePair<string, string>("p_password", password),
                new KeyValuePair<string, string>("p_recaptcha_response", recaptchaResponse)
            };
            var formContent = new FormUrlEncodedContent(formEntries);

            var cookies = new CookieContainer();
            var clientHandler = new HttpClientHandler
            {
                CookieContainer = cookies,
                UseCookies = true
            };
            var client = new HttpClient(clientHandler);
            var loginResult = await client.PostAsync(loginUrl, formContent);
            loginResult.EnsureSuccessStatusCode();

            var stream = await loginResult.Content.ReadAsStreamAsync();
            var streamReader = new StreamReader(stream, Encoding.GetEncoding(1251));
            var content = await streamReader.ReadToEndAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(content);

            // var signsUrl = "https://student.psu.ru/pls/stu_cus_et/stu.signs";
        }
    }
}
