using Autotrader;
using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Autotader
{
    class Data
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public string Price { get; set; }
        public string Mileage { get; set; }
        public string Phone { get; set; }
    }

    class Program
    {
        const string Path = "http://www.autotrader.co.uk/car-search?sort=make&radius=1500&postcode=bh13na&onesearchad=Used&onesearchad=Nearly%20New&onesearchad=New&make={1}&year-from=2013&maximum-mileage=40000&seller-type=private&page={0}";

        static ConcurrentBag<Data> _result = new ConcurrentBag<Data>();
        static string[] _makes = { "Audi", "Abarth", "Aston Martin", "Porsche", "Bentley", "Jaguar", "BMW", "Lamborghini", "Land Rover", "Lexus", "Maserati", "McLaren", "Volkswagen", "Mercedes-Benz", "Ferrari", "MINI" };

        static void Main(string[] args)
        {
            try
            {
                var start = DateTime.Now;
                Console.WriteLine($"download started at {start}");

                var tasks = new Task[_makes.Length];
                for (int i = 0; i < tasks.Length; i++)
                {
                    var make = _makes[i];
                    tasks[i] = Task.Run(async () => await DoWork(make));
                }

                Task.WaitAll(tasks);

                var fileName = "data.xlsx";
                ExcelExporter.Export(_result, fileName);

                Console.WriteLine($"done at {DateTime.Now} ({DateTime.Now - start:hh\\:mm\\:ss} elapsed), results saved to {fileName}");
            }
            catch (Exception ex)
            {
                File.WriteAllText("error.log", ex.ToString());
                Console.WriteLine($"error occured, message saved to file error.log");
            }
            Console.Read();
        }

        async static Task DoWork(string make)
        {
            var hc = HttpHelper.CreateClient();
            var doc = new HtmlDocument();

            for (int i = 1; i < int.MaxValue; i++)
            {
                using (var resp = await hc.GetAsync(string.Format(Path, i, make)))
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        var page = await resp.Content.ReadAsStringAsync();
                        doc.LoadHtml(page);

                        var items = doc.DocumentNode.SelectNodes("//h2[@class=\"listing-title title-wrap\"]/a[@class=\"listing-fpa-link\"]");
                        foreach (var item in items)
                        {
                            var href = item.Attributes["href"].Value;
                            ProcessData(href, make);
                        }

                        Console.WriteLine($"finished page {i} for {make}");
                    }
                    else if (resp.StatusCode == HttpStatusCode.Redirect)
                    {
                        Console.WriteLine($"{make} completed on page {i - 1}");
                        return;
                    }
                    else
                    {
                        Console.WriteLine($"error on page {i} for {make} with code {resp.StatusCode}");
                        i--;
                    }
                }
            }
        }

        static void ProcessData(string href, string make)
        {
            var web = new HtmlWeb();
            var doc = web.Load("http://www.autotrader.co.uk" + href).DocumentNode;

            var model = doc.SelectSingleNode("//span[@class=\"pricetitle__advertTitle\"]");
            var price = doc.SelectSingleNode("//section[@class=\"priceTitle__price gui-advert-price\"]");
            var mileage = doc.SelectSingleNode("//li[@class=\"keyFacts__item\"][3]");
            var phone = doc.SelectSingleNode("//div[@itemprop=\"telephone\"]");

            _result.Add(new Data
            {
                Make = make,
                Model = model.InnerText,
                Price = price.InnerText,
                Mileage = mileage.InnerText,
                Phone = phone.InnerText
            });
        }
    }
}
