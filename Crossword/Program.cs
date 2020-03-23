using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Crossword
{
    class Program
    {
        Grid m_grid;
        string m_xml;

        private void Start(string content)
        {
            XmlNode root;
            XmlNamespaceManager manager;

            try
            {
                m_xml = content;

                // trim the javascript
                m_xml = m_xml.Substring(0, m_xml.Length - 2);
                m_xml = m_xml.Remove(0, "var CrosswordPuzzleData = \"".Length);
                m_xml = m_xml.Replace("\\\"", "\"");

                // load xml
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(m_xml);
                root = doc.DocumentElement;

                // add the namespace
                manager = new XmlNamespaceManager(doc.NameTable);
                manager.AddNamespace("ns", "http://crossword.info/xml/rectangular-puzzle");
            }
            catch (Exception e)
            {
                Console.WriteLine("Gebasz van!");
                Console.WriteLine(e.Message);
                return;
            }

            // get the grid and the cells
            var xGrid = root.SelectSingleNode("//ns:grid", manager);
            var xCells = root.SelectNodes("//ns:cell", manager);

            // crossword size
            int width = int.Parse(xGrid.Attributes["width"].Value);
            int height = int.Parse(xGrid.Attributes["height"].Value);

            // fill the grid
            m_grid = new Grid(width, height);
            foreach (XmlNode xCell in xCells)
            {
                int x, y;
                char solution = '?';
                ConsoleColor color = ConsoleColor.White;

                if (!int.TryParse(xCell.Attributes["x"].Value, out x))
                    continue;

                if (!int.TryParse(xCell.Attributes["y"].Value, out y))
                    continue;

                if (xCell.Attributes["solution"]?.Value != null)
                {
                    solution = char.Parse(xCell.Attributes["solution"].Value);
                    if (xCell.Attributes["background-color"]?.Value != null)
                        color = ConsoleColor.Yellow;
                    else
                        color = ConsoleColor.Red;
                }

                m_grid[x - 1, y - 1] = new Cell(solution, color);
            }

            // print solution
            Console.WriteLine("Itten van a megoldás:");
            Print();
        }

        private void Print()
        {
            Console.WriteLine(new StringBuilder().Insert(0, "+-", m_grid.Width) + "+");
            for (int y = 0; y < m_grid.Height; y++)
            {
                for (int x = 0; x < m_grid.Width; x++)
                {
                    Console.Write("|");
                    if (m_grid[x, y] == null)
                        Console.Write(' ');
                    else
                        PrintChar(m_grid[x, y].Solution, m_grid[x, y].Color);
                    if (x == m_grid.Width - 1)
                        Console.Write("|\n");
                }
                Console.WriteLine(new StringBuilder().Insert(0, "+-", m_grid.Width) + "+");
            }
        }

        private void PrintChar(char c, ConsoleColor color)
        {
            var oldcolor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(c);
            Console.ForegroundColor = oldcolor;
        }

        private string GetDailyCrossword()
        {
            string cw = GetLink("https://www.kisalfold.hu/", "//a[@href]", "href", "napi-keresztrejtveny");
            string cwLink = GetLink(cw, "//script[@src]", "src", "crosswordgame(.)*.txt");

            return CrosswordContentFromHtml(cwLink);
        }

        private string GetLink(string url, string find, string attr, string regexPattern)
        {
            if (string.IsNullOrEmpty(url))
                return "";

            var web = new HtmlWeb();
            var doc = web.Load(url);
            var r = new Regex(regexPattern);

            foreach (HtmlNode link in doc.DocumentNode.SelectNodes(find))
            {
                if (link.Attributes[attr]?.Value == null)
                    continue;

                if (r.IsMatch(link.Attributes[attr].Value))
                    return link.Attributes[attr].Value;
            }
            return "";
        }

        private string CrosswordContentFromHtml(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "";

            using (WebClient client = new WebClient())
            {
                var htmlData = client.DownloadData(url);
                return Encoding.UTF8.GetString(htmlData);
            }
        }

        static void Main(string[] args)
        {
            string crosswordRaw;
            var p = new Program();

            if (args.Length == 1)
            {
                try
                {
                    // read file
                    crosswordRaw = File.ReadAllText(args[0]);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Gebasz van!");
                    Console.WriteLine(e.Message);
                    return;
                }
            }
            else
            {
                Console.WriteLine("Napi keresztrejtvény letöltése...");
                crosswordRaw = p.GetDailyCrossword();
            }

            p.Start(crosswordRaw);

            Console.ReadLine();
        }
    }
}
