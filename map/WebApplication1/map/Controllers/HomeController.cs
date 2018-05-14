using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;

namespace WebApplication1.Controllers
{
    public class Parser
    {
        public string ParseTitle(string content)
        {
            Regex regex = new Regex("<title>(.*)</title>");
            Match result = regex.Match(content);
            return result.Groups[1].ToString();
        }

        public string ParsePreview(string content)
        {
            Regex regex = new Regex("<text>(.*)<more>", RegexOptions.Multiline);
            Match result = regex.Match(content);
            return result.Groups[1].ToString();
        }

        public string ParseContent(string content)
        {
            Regex regex = new Regex("<text>(.*)</text>", RegexOptions.Multiline);
            Match result = regex.Match(content);
            var str = result.Groups[1].ToString();
            return str.Replace("<more>", "");
        }

        public string[] ParseBooks(string content)
        {
            Match result = new Regex("<books>(.*)</books>").Match(content);
            string[] arr = result.Groups[1].ToString().Split("<book>");
            return arr;

        }

        public string ParseQuestions(ref string content)
        {
            Match result = new Regex("<questions>(.*)</questions>").Match(content);
            return result.Groups[1].ToString();
        }

        public string ParseArticles(ref string content)
        {
            Match result = new Regex("<articles>(.*)</articles>").Match(content);
            return result.Groups[1].ToString();
        }

        public string ParseResources(ref string content)
        {
            Match result = new Regex("<resources>(.*)</resources>").Match(content);
            return result.Groups[1].ToString();
        }
    }

    public class PreviewBuilder
    {
        Parser reader = new Parser();

        public IEnumerable<PreviewItem> Build()
        {
            foreach (var path in Directory.EnumerateFiles("posts"))
            {
                string content = File.ReadAllText(path).Replace("\r\n", " ");
                string title = reader.ParseTitle(content);
                string preview = reader.ParsePreview(content);
                string fileName = path;
                yield return new PreviewItem(title, preview, fileName);
            }
        }
        
        public class PreviewItem
        {
            public PreviewItem(string title, string text, string fileName)
            {
                Title = title;
                Text = text;
                FileName = fileName;
            }

            public string Title { get; set; }
            public string Text { get; set; }
            public string FileName { get; set; }
        }

    }

    public class InfoBuilder
    {
        Parser parser = new Parser();

        public InfoItem Build(string path)
        {
            string content = File.ReadAllText(path).Replace("\r\n", " ");
            string title = parser.ParseTitle(content);
            string text = parser.ParseContent(content);
            var books = parser.ParseBooks(content);
            var questions = parser.ParseQuestions(ref content);
            var articles = parser.ParseArticles(ref content);
            var resources = parser.ParseResources(ref content);
            return new InfoItem(title, text, books, questions, articles, resources);
        }

        public class InfoItem
        {
            public InfoItem(string title, string text, string[] books, string questions, string articles, string resources)
            {
                Title = title;
                Text = text;
                Books = books;
                Questions = questions;
                Articles = articles;
                Resources = resources;
            }

            public string Title { get; set; }
            public string Text { get; set; }
            public string[] Books { get; set; }
            public string Questions { get; set; }
            public string Articles { get; set; }
            public string Resources { get; set; }


        }
    }

    public class HomeController : Controller
    {
        private PreviewBuilder _previewBuilder = new PreviewBuilder();
        private InfoBuilder _infoBuilder = new InfoBuilder();

        public IActionResult Index()
        {
            var items = _previewBuilder.Build().ToList();
            return View(items);
        }

        public IActionResult GetInfo(string path)
        {
            var item = _infoBuilder.Build(path);
            return View(item);
        }
    }
}
