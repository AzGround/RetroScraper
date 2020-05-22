using HtmlAgilityPack;
using RetroScraper.RetroParser;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RetroScraper
{
    class Program
    {
        private static string nameOut = "musiclist.txt";
        private static string nameConf = "config.ini";
        private static string fileOut = AppDomain.CurrentDomain.BaseDirectory + nameOut;
        private static string fileConf = AppDomain.CurrentDomain.BaseDirectory + nameConf;
        private static INIManager manager = new INIManager(fileConf);
        private static DateTime dateFrom;
        private static DateTime dateTo;

        static void Main(string[] args)
        {
            if (InitConfig() == 0)
                return;
            File.WriteAllText(fileOut, "", Encoding.UTF8);

            try
            {
                for (DateTime dateCur = dateFrom; dateCur <= dateTo; dateCur = dateCur.AddDays(1))
                {
                    GetHtmlAsync(dateCur.ToString("dd.MM.yyyy")).Wait();
                }

                Console.WriteLine($"\nShow \"{nameOut}\" to show music list\n\nPrint Enter to exit.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.ReadLine();
        }

        private static int InitConfig()
        {
            if (!File.Exists(fileConf))
            {
                File.WriteAllText(fileConf, ";Date format dd.mm.yyyy", Encoding.UTF8);
                manager.WritePrivateString("General", "url", "https://retrofm.ru/index.php?go=Playlist");
                manager.WritePrivateString("General", "dateFrom", "");
                manager.WritePrivateString("General", "dateTo", "");

                Console.WriteLine($"Configuration file don't exist!\n\nFile \"{nameConf}\" initialized.");
                Console.ReadLine();
                return 0;
            }

            string strDateFrom = manager.GetPrivateString("General", "dateFrom");
            string strDateTo = manager.GetPrivateString("General", "dateTo");

            if (strDateFrom.Equals("") || strDateTo.Equals(""))
            {
                Console.WriteLine("Enter the dates in the ini file!");
                Console.ReadLine();
                return 0;
            }

            dateFrom = DateTime.Parse(strDateFrom);
            dateTo = DateTime.Parse(strDateTo);

            if (dateTo < dateFrom)
            {
                Console.WriteLine("Date To cannot be less Date From!");
                Console.ReadLine();
                return 0;
            }

            return 1;
        }

        private static async Task GetHtmlAsync(string date)
        {
            var url = manager.GetPrivateString("General", "url") + "&date=" + date + "&time_start=00%3A00&time_stop=23%3A59";

            var httpClient = new HttpClient();
            try
            {
                var html = await httpClient.GetStringAsync(url);

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var playlist = htmlDocument.DocumentNode.Descendants("ul")
                    .Where(node => node.HasClass("jplay-list")).ToList();
                var musicItems = playlist[0].Descendants("li")
                    .Where(node => node.HasClass("player-in-playlist-holder")).ToList();
                musicItems.Reverse();

                int i = 0;
                foreach (var item in musicItems)
                {
                    string time = item.Descendants("span")
                        .Where(node => node.HasClass("time")).FirstOrDefault().InnerText;
                    var musicInfo = item.Descendants("div")
                        .Where(node => node.HasClass("jp-title")).FirstOrDefault();
                    string artist = musicInfo.Descendants("span").FirstOrDefault().InnerText;
                    string title = musicInfo.Descendants("em").FirstOrDefault().InnerText;

                    File.AppendAllText(fileOut, $"{date}|{time}|{artist}|{title}\n", Encoding.UTF8);
                    ++i;
                }

                if (i == musicItems.Count)
                    Console.WriteLine($"{date} - Done!");
                else
                    Console.WriteLine($"{date} - Have error! | {i} of {musicItems.Count} elements complete.");
            }
            catch (HttpRequestException)
            {
                throw new Exception("Page doesn't exist.");
            }
            catch (Exception)
            {
                throw new Exception("Error with write file.");
            }
        }
    }
}
