using System.Drawing;
using System.Globalization;
using System.Security.Principal;
using System.Text;
using CrawData.Models;
using HtmlAgilityPack;
using OfficeOpenXml;

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
        for (int i = 1; i < operatorNames.Count; i++)
        {
            tasks.Add(crawl.Get_Details_Operator_Async($"{url + RemoveAccents(operatorNames[i].ToLower())}"));
        }
        Operator[] operatorsArray = await Task.WhenAll(tasks);
        operators.AddRange(operatorsArray);


        // var op = await crawl.Get_Details_Operator_Async("https://www.ubisoft.com/en-gb/game/rainbow-six/siege/game-info/operators/solis");

        string filePath = @"C:\Users\tuan\Desktop\data\operators.xlsx";
        // Kiểm tra và xóa file nếu đã tồn tại
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage(filePath))
        {
            for (int i = 0; i < operators.Count(); i++)
            {
                var sheet = package.Workbook.Worksheets.Add(operators[i].OperatorName);
                sheet.Cells["A1"].Value = "OperatorId";
                sheet.Cells["B1"].Value = "OperatorName";
                sheet.Cells["C1"].Value = "OperatorIcon";
                sheet.Cells["D1"].Value = "UnknownInformation";
                sheet.Cells["E1"].Value = "Ability";
                sheet.Cells["F1"].Value = "PrimaryWeapon";
                sheet.Cells["G1"].Value = "SecondaryWeapon";
                sheet.Cells["H1"].Value = "Gadgets";
                sheet.Cells["I1"].Value = "Skill";
                sheet.Cells["J1"].Value = "Side";
                sheet.Cells["K1"].Value = "Squad";
                sheet.Cells["L1"].Value = "SquadIcon";
                sheet.Cells["M1"].Value = "Health";
                sheet.Cells["N1"].Value = "Speed";
                sheet.Cells["O1"].Value = "Difficulty";
                sheet.Cells["P1"].Value = "RealName";
                sheet.Cells["Q1"].Value = "DateOfBirth";
                sheet.Cells["R1"].Value = "PlaceOfBirth";
                sheet.Cells["S1"].Value = "Biography";

                sheet.Cells["A2"].Value = operators[i].OperatorId;
                sheet.Cells["B2"].Value = operators[i].OperatorName;
                sheet.Cells["C2"].Value = operators[i].OperatorIcon;
                sheet.Cells["D2"].Value = operators[i].UnknownInformation;
                sheet.Cells["E2"].Value = operators[i].Ability.Description;

                int primaryRow = 2;
                for (int j = 0; j < operators[i].PrimaryWeapon.Count(); j++)
                {

                    sheet.Cells[$"F{primaryRow}"].Value = operators[i].PrimaryWeapon[j].WeaponName;
                    sheet.Cells[$"F{primaryRow + 1}"].Value = operators[i].PrimaryWeapon[j].Img;
                    sheet.Cells[$"F{primaryRow + 2}"].Value = operators[i].PrimaryWeapon[j].Type;

                    primaryRow += 4;

                }

                int secondaryRow = 2;
                for (int j = 0; j < operators[i].SecondaryWeapon.Count(); j++)
                {
                    sheet.Cells[$"G{secondaryRow}"].Value = operators[i].SecondaryWeapon[j].WeaponName;
                    sheet.Cells[$"G{secondaryRow + 1}"].Value = operators[i].SecondaryWeapon[j].Img;
                    sheet.Cells[$"G{secondaryRow + 2}"].Value = operators[i].SecondaryWeapon[j].Type;

                    secondaryRow += 4;
                }

                int gadgetRow = 2;
                for (int j = 0; j < operators[i].Gadgets.Count(); j++)
                {
                    sheet.Cells[$"H{gadgetRow}"].Value = operators[i].Gadgets[j].Name;
                    sheet.Cells[$"H{gadgetRow + 1}"].Value = operators[i].Gadgets[j].Img;

                    gadgetRow += 3;
                }


                sheet.Cells["I2"].Value = operators[i]?.Skill?.SkillName ?? "No Skill";
                sheet.Cells["I3"].Value = operators[i]?.Skill?.Img ?? "No Image";

                sheet.Cells["J2"].Value = operators[i]?.Side ?? "No Side";

                sheet.Cells["K2"].Value = operators[i]?.Squad ?? "No Squad";

                sheet.Cells["L2"].Value = operators[i]?.SquadIcon ?? "No Icon";

                sheet.Cells["M2"].Value = operators[i]?.Health ?? 0; // Nếu giá trị mặc định không phải là số, thay đổi phù hợp
                sheet.Cells["N2"].Value = operators[i]?.Speed ?? 0;
                sheet.Cells["O2"].Value = operators[i]?.Difficulty ?? 0;

                sheet.Cells["P2"].Value = operators[i]?.RealName ?? "No Name";
                sheet.Cells["Q2"].Value = operators[i]?.DateofBirth ?? "No Date"; // Định dạng ngày tháng
                sheet.Cells["R2"].Value = operators[i]?.PlaceofBirth ?? "No Place";
                sheet.Cells["S2"].Value = operators[i]?.Biography ?? "No Biography";

            }


            // Save to file
            package.Save();
        }


        // await crawl.Get_Details_Operator_Async("https://www.ubisoft.com/en-gb/game/rainbow-six/siege/game-info/operators/solis");
        // await crawl.Get_Details_Operator_Async("https://www.ubisoft.com/en-gb/game/rainbow-six/siege/game-info/operators/glaz");

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
                    if (div.InnerText == "NØKK")
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

                                    //tiếp theo là lấy skill(mô tả và video )
                                    var promo_operator__ability__row = div
                                                    .Descendants("div")
                                                    .FirstOrDefault(d => d.Attributes["class"] != null
                                                    && d.Attributes["class"].Value == "promo operator__ability__row ");

                                    if (promo_operator__ability__row == null)
                                    {
                                        return op;
                                    }

                                    //lấy description trước
                                    var skillDesciption = promo_operator__ability__row.Descendants("div")
                                    .FirstOrDefault(d => d.Attributes["class"] != null && d.Attributes["class"].Value == "promo__wrapper__content")
                                    .Descendants("p").FirstOrDefault();

                                    if (skillDesciption != null)
                                    {
                                        op.Ability.Description = skillDesciption.InnerHtml;
                                    }
                                    //video
                                    //Chưa lấy được link video!


                                    //tiếp theo lấy loadout
                                    var loadout = html.Descendants("div")
                                    .FirstOrDefault(d => d.Attributes["class"] != null && d.Attributes["class"].Value == "operator__loadout")
                                    .Descendants("div")
                                    .Where(d => d.Attributes["class"] != null && d.Attributes["class"].Value == "operator__loadout__category")
                                    .ToList();
                                    //thẻ loadout này sẽ có 4 div nhỏ ở trong
                                    //div 1 primary weapon
                                    //div 2 secondary weapon
                                    //div 3 GADGET
                                    //div 4 UNIQUE ABILITY


                                    //vòng lặp để duyệt qua 4 mục lớn của loadout
                                    for (int i = 0; i < loadout.Count; i++)
                                    {
                                        //lấy các vũ khí trong 1 loadout
                                        var loadout_category = loadout[i].Descendants("div")
                                        .FirstOrDefault(d => d.Attributes["class"] != null && d.Attributes["class"].Value == "operator__loadout__category__items")
                                        .Descendants("div")
                                        .Where(d => d.Attributes["class"] != null && d.Attributes["class"].Value == "operator__loadout__weapon")
                                        .ToList();


                                        for (int j = 0; j < loadout_category.Count; j++)
                                        {
                                            var nameAndType = loadout_category[j].Descendants("p").ToList();
                                            var imgLink = loadout_category[j].Descendants("img").FirstOrDefault().Attributes["src"].Value;
                                            if (i == 0)
                                            {
                                                op.PrimaryWeapon.Add(new PrimaryWeapon
                                                {
                                                    WeaponName = nameAndType[0].InnerText,
                                                    Type = nameAndType[1].InnerText,
                                                    Img = imgLink
                                                });
                                            }

                                            else if (i == 1)
                                            {
                                                op.SecondaryWeapon.Add(new SecondaryWeapon
                                                {
                                                    WeaponName = nameAndType[0].InnerHtml,
                                                    Type = nameAndType[1].InnerHtml,
                                                    Img = imgLink
                                                });
                                            }

                                            else if (i == 2)
                                            {
                                                op.Gadgets.Add(new Gadget
                                                {
                                                    Name = nameAndType[0].InnerText,
                                                    Img = imgLink
                                                });
                                            }

                                            else if (i == 3)
                                            {
                                                op.Skill = new Skill
                                                {
                                                    SkillName = nameAndType[0].InnerHtml,
                                                    Img = imgLink
                                                };
                                            }
                                        }

                                    }

                                    //Lấy Real Name - Date of birth - place of birth
                                    var realName_dob_pob_t = html.Descendants("div")
                                    .FirstOrDefault(d => d.Attributes["class"] != null && d.Attributes["class"].Value == "operator__biography");

                                    if (realName_dob_pob_t == null) return op;

                                    var realName_dob_pob = realName_dob_pob_t.Descendants("div")
                                    .FirstOrDefault(d => d.Attributes["class"] != null && d.Attributes["class"].Value == "operator__biography__infos")
                                    .Descendants("div")
                                    .Where(d => d.Attributes["class"] != null && d.Attributes["class"].Value == "operator__biography__info")
                                    .ToList();

                                    if (realName_dob_pob == null && realName_dob_pob.Count() == 0)
                                    {
                                        return op;
                                    }

                                    //realName_dob_pob có 3 phần tử
                                    //[0]-realname
                                    //[1]-date of birth
                                    //[2]-place of birth

                                    for (int i = 0; i < realName_dob_pob.Count; i++)
                                    {
                                        var operator__biography__info = realName_dob_pob[i]
                                        .Descendants("div")
                                        .FirstOrDefault(d => d.Attributes["class"] != null && d.Attributes["class"].Value == "operator__biography__info__value")
                                        .InnerHtml;

                                        if (i == 0)
                                        {
                                            op.RealName = operator__biography__info;
                                        }

                                        else if (i == 1)
                                        {
                                            op.DateofBirth = operator__biography__info;
                                        }

                                        else if (i == 2)
                                        {
                                            op.PlaceofBirth = operator__biography__info;
                                        }
                                    }


                                    //lấy biography
                                    var biography = html.Descendants("div")
                                    .FirstOrDefault(d => d.Attributes["class"] != null && d.Attributes["class"].Value == "operator__biography__description")
                                    .InnerHtml;

                                    op.Biography = biography;


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