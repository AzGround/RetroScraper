using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RetroScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            GetHtmlAsync();
            Console.ReadLine();
        }

        private static async void GetHtmlAsync()
        {
            var url = "https://retrofm.ru/index.php?go=Playlist&date=31.07.2019&time_start=00%3A00&time_stop=23%3A59";

            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var playlist = htmlDocument.DocumentNode.Descendants("ul")
                .Where(node => node.HasClass("jplay-list")).ToList();
            var musicItems = playlist[0].Descendants("li")
                .Where(node => node.HasClass("player-in-playlist-holder")).ToList();
            musicItems.Reverse();

            foreach (var item in musicItems)
            {
                string time = item.Descendants("span")
                    .Where(node => node.HasClass("time")).FirstOrDefault().InnerText;
                var musicInfo = item.Descendants("div")
                    .Where(node => node.HasClass("jp-title")).FirstOrDefault();
                string artist = musicInfo.Descendants("span").FirstOrDefault().InnerText;
                string title = musicInfo.Descendants("em").FirstOrDefault().InnerText;

                Console.WriteLine($"{time}|{artist}|{title}");
                Console.WriteLine();
            }

            //saveToFile(html.Result, "html.txt", "");
        }

        static private void saveToFile(string text, string name, string path)
        {
            File.WriteAllText(path + name, text, Encoding.UTF8);
            //using (StreamWriter sw = File.CreateText(path + name))
            //{
            //    sw.WriteLine(text);
            //}
        }
    }
}
