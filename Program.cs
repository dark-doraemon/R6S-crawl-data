using System.Globalization;
using System.Text;
using CrawData.Models;
using HtmlAgilityPack;

namespace CrawData;

class Program
{

    static public async Task Main(string[] args)
    {
        string url = "https://www.ubisoft.com/en-gb/game/rainbow-six/siege/game-info/operators/";
        Crawl crawl = new Crawl();
        List<string> operatorNames = await crawl.Craw_Operator_Name_Async(url); //lưu name of operator
        List<Operator> operators = new List<Operator>();

        // for (int i = 1; i < operatorNames.Count; i++)
        // {
        //     Console.WriteLine($"{i + " " + operatorNames[i].ToLower()}");
        //     operators.Add(await crawl.Get_Details_Operator_Async($"{url + operatorNames[i].ToLower()}"));
        // }

        List<Task<Operator>> tasks = new List<Task<Operator>>();

        // for (int i = 1; i < operatorNames.Count; i++)
        // {
        //     tasks.Add(crawl.Get_Details_Operator_Async($"{url + RemoveAccents(operatorNames[i].ToLower())}"));
        // }
        // Operator[] operatorsArray = await Task.WhenAll(tasks);

        // operators.AddRange(operatorsArray);

        // for (int i = 0; i < operators.Count; i++)
        // {
        //     Console.WriteLine($"{i + " " + operators[i].OperatorId} - {operators[i].OperatorName} - {operators[i].Side} - {operators[i].Squad} - {operators[i].Health} - {operators[i].Speed} - {operators[i].Difficulty}");
        // }

        await crawl.Get_Details_Operator_Async("https://www.ubisoft.com/en-gb/game/rainbow-six/siege/game-info/operators/solis");        

    }

    static string RemoveAccents(string text)
    {
        // Chuyển đổi chuỗi thành dạng không có dấu
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            // Loại bỏ các ký tự dấu (NonSpacingMark)
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        // Chuyển đổi lại thành dạng chuẩn
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}

public class Crawl
{
    public async Task<List<string>> Craw_Operator_Name_Async(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36 Edg/127.0.0.0");
            HttpResponseMessage responseMessage = await client.GetAsync(url);
            string htmlContent = await responseMessage.Content.ReadAsStringAsync();
            if (responseMessage.IsSuccessStatusCode)
            {
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlContent);
                var stringOperators = htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[1]/div[5]/div[4]/div[4]").OuterHtml;

                htmlDocument.LoadHtml(stringOperators);

                HtmlNodeCollection divs = htmlDocument.DocumentNode.SelectNodes("//div");
                List<string> operators = new List<string>();
                foreach (var div in divs)
                {
                    if(div.InnerText == "NØKK")
                    {
                        operators.Add("nokk");
                    }
                    else operators.Add(div.InnerText);
                }
                return operators;
            }
            else
            {
                Console.WriteLine("Failed");
                return null;
            }
        }
    }


    public async Task<Operator> Get_Details_Operator_Async(string url)
    {
        Operator op = new Operator();
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36 Edg/127.0.0.0");
            HttpResponseMessage responseMessage = await client.GetAsync(url);
            string htmlContent = await responseMessage.Content.ReadAsStringAsync();
            if (responseMessage.IsSuccessStatusCode)
            {
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlContent);

                var html = htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[1]/div[5]"); //class="r6s-op-details"

                if (html != null)
                {
                    //lấy data-ccid có value
                    var data_ccid = htmlDocument.DocumentNode.Descendants("div").Where(d => d.Attributes["data-ccid"] != null);
                    var data_ccid2 = htmlDocument.DocumentNode.Descendants("div").First(d => d.Attributes["data-ccid"] != null);

                    if (data_ccid == null)
                    {
                        Console.WriteLine("Không tìm thấy ccid");
                        return null;
                    }
                    else
                    {
                        foreach (var div in data_ccid)
                        {
                            var dataCcidValue = div.Attributes["data-ccid"].Value;
                            if (!String.IsNullOrEmpty(dataCcidValue) && dataCcidValue != "ccid")
                            {

                                op.OperatorId = div.Attributes["data-ccid"].Value;

                                //lấy class="operator__header__icons__names" chứa icon, name,squad
                                var operator__header__icons__names = div.Descendants("div").Where(d => d.Attributes["class"].Value == "operator__header__icons__names").FirstOrDefault();
                                if (operator__header__icons__names != null)
                                {
                                    //lấy thẻ img -> icon
                                    var icon = operator__header__icons__names.Descendants("img").FirstOrDefault();
                                    op.OperatorIcon = icon.Attributes["src"].Value ?? "";

                                    //lấy thẻ h1 -> Name
                                    var name = operator__header__icons__names.Descendants("h1").FirstOrDefault();
                                    op.OperatorName = name.InnerHtml ?? "";

                                    //lấy thẻ h2 -> unknowinformation   
                                    var unknow = operator__header__icons__names.Descendants("h2").FirstOrDefault();
                                    op.UnknownInformation = unknow.InnerHtml ?? "";

                                    //tiếp theo lấy class = operator__header__squad__detail nằm trong div operator__header__infos nằm trong div

                                    var operator__header__side__wrapper = div.Descendants("div")
                                    .FirstOrDefault(d => d.Attributes["class"].Value == "operator__header__infos")
                                    .Descendants("div")
                                    .FirstOrDefault(d => d.Attributes["class"].Value == "operator__header__side__wrapper")
                                    .Descendants("div").ToList();


                                    //code này dùng để lấy side (attacker or defender)
                                    var operator__header__side__detail =
                                    operator__header__side__wrapper
                                    .Where(d => d.Attributes["class"] != null &&
                                                d.Attributes["class"].Value.Contains("operator__header__side__detail"))
                                    .FirstOrDefault();

                                    if (operator__header__side__detail != null)
                                    {
                                        op.Side = operator__header__side__detail.InnerText;
                                    }
                                    else
                                    {
                                        op.Side = "";
                                    }


                                    //code này dùng để lấy "squad name" operator__header__squad__detail
                                    var operator__header__squad__detail =
                                    operator__header__side__wrapper
                                    .Where(d => d.Attributes["class"] != null &&
                                                d.Attributes["class"].Value.Contains("operator__header__squad__detail"))
                                    .FirstOrDefault();

                                    op.Squad = operator__header__squad__detail != null ? operator__header__squad__detail.InnerText : "";


                                    //tiếp theo là lấy Health - Speed - Difficulty
                                    var operator__header__stats = div
                                                    .Descendants("div")
                                                    .FirstOrDefault(d => d.Attributes["class"].Value == "operator__header__infos")
                                                    .Descendants("div")
                                                    .First(d => d.Attributes["class"] != null &&
                                                    d.Attributes["class"].Value == "operator__header__stats")
                                                    .Descendants("div")
                                                    .Where(d => d.Attributes["class"] != null &&
                                                    d.Attributes["class"].Value == "operator__header__stat").ToList();
                                    // .Descendants("div").ToList();

                                    //sao khi lấy được operator__header__stats ta sẽ có 3 phần tử
                                    //[0] - HEALTH
                                    //[1] - SPEED
                                    //[2] - DIFFICULTY

                                    //có bao nhiêu div có class react-rater-star is-disabled is-active là có bấy nhiêu máu
                                    //ta check xem có bao nhiêu div có class react-rater-star is-disabled is-active
                                    // var health = operator__header__stats[0]
                                    // .Descendants("div")
                                    // .First(d => d.Attributes["class"] != null && d.Attributes["class"].Value == "react-rater")
                                    // .Descendants("div")
                                    // .Where(d => d.Attributes["class"] != null && d.Attributes["class"].Value == "react-rater-star is-disabled is-active").Count();

                                    // op.Health = health;

                                    //3 phần từ này giống như nhau hết -> dùng vòng for
                                    for (int i = 0; i < operator__header__stats.Count(); i++)
                                    {
                                        var data = operator__header__stats[i]
                                        .Descendants("div")
                                        .First(d => d.Attributes["class"] != null && d.Attributes["class"].Value == "react-rater")
                                        .Descendants("div")
                                        .Where(d => d.Attributes["class"] != null && d.Attributes["class"].Value == "react-rater-star is-disabled is-active").Count();

                                        if (i == 0)
                                        {
                                            op.Health = data;
                                        }
                                        else if (i == 1)
                                        {
                                            op.Speed = data;
                                        }

                                        else if (i == 2) op.Difficulty = data;
                                    }

                                    //tiếp theo là lấy skill
                                    var prom_operator__ability__row  = div.Descendants("div").Where(d => d.Attributes["class"] != null 
                                    && d.Attributes["class"].Value == "promo operator__ability__row ");

                                    //video
                                    //Chưa lấy được link video!


                                    //tiếp theo lấy loadout
                                    var loadout = div.Descendants("div")
                                    .FirstOrDefault(d => d.Attributes["class"] != null && d.Attributes["class"].Value == "operator__loadout");

                                    // Console.WriteLine($"{j++} - {op.OperatorId} - {op.OperatorName} - {op.Side} - {op.Squad} - {op.Health} - {op.Speed} - {op.Difficulty}");

                                }

                                else
                                {
                                    Console.WriteLine("Không tìm thấy operator__header__icons__names");
                                    return null; ;
                                }
                                break;
                            }
                        }

                    }

                }
                else
                {
                    Console.WriteLine("Không tìm thấy nút với đường dẫn XPath đã cho.");
                }

            }
        }
        return op;
    }
}