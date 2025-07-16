using System.Collections.Concurrent;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace ExamTopicsRecursiveScraper
{
    class Program
    {
        private static readonly ConcurrentBag<ExamQuestion> _questions = new();
        private static readonly HtmlWeb _web = new HtmlWeb
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
        };

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Start scraping ServiceNow HR examenvragen...\n");

            try
            {
                int totalPages = await GetTotalPages();
                Console.WriteLine($"Gevonden totaal aantal pagina's: {totalPages}\n");

                await ProcessAllPages(totalPages);

                Console.WriteLine($"\nScraping voltooid! Totaal gevonden vragen: {_questions.Count}\n");

                string outputPath = @"C:\Users\halil\Desktop\HR vragen.docx";
                SaveQuestionsToWord(outputPath, _questions.OrderBy(q => q.QuestionNumber));
                Console.WriteLine($"Alle vragen en antwoorden zijn opgeslagen in: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nCritical error: {ex.Message}");
            }
        }

        static async Task<int> GetTotalPages()
        {
            var doc = await Task.Run(() => _web.Load("https://www.examtopics.com/discussions/servicenow/1/"));
            var paginationNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class, 'page-indicator')]");
            if (paginationNode != null)
            {
                var text = paginationNode.InnerText;
                var parts = text.Split(new[] { " of " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && int.TryParse(parts[1], out int total))
                    return total;
            }
            return 127; // Fallback als paginadetectie mislukt
        }

        static async Task ProcessAllPages(int totalPages)
        {
            for (int currentPage = 1; currentPage <= totalPages; currentPage++)
            {
                await ProcessPage(currentPage);
                await Task.Delay(2000); // Anti-DDOS delay
            }
        }

        static async Task ProcessPage(int pageNumber)
        {
            try
            {
                var url = $"https://www.examtopics.com/discussions/servicenow/{pageNumber}/";
                Console.WriteLine($"Verwerken pagina: {url}");

                var doc = await Task.Run(() => _web.Load(url));
                var links = GetDiscussionLinks(doc);

                foreach (var link in links)
                {
                    ProcessDiscussion(link);
                    await Task.Delay(500); // Extra delay tussen vragen
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout op pagina {pageNumber}: {ex.Message}");
            }
        }

        static List<string> GetDiscussionLinks(HtmlDocument doc)
        {
            return doc.DocumentNode
                .SelectNodes("//a[contains(@href, '/discussions/servicenow/view/') and contains(text(), 'Exam CIS-HR topic')]")
                ?.Select(a =>
                {
                    var href = a.GetAttributeValue("href", "");
                    return href.StartsWith("http") ? href : "https://www.examtopics.com" + href;
                })
                .Distinct()
                .ToList() ?? new List<string>();
        }

        static void ProcessDiscussion(string url)
        {
            try
            {
                var doc = _web.Load(url);

                var question = new ExamQuestion
                {
                    QuestionNumber = ExtractQuestionNumber(doc),
                    QuestionText = ExtractQuestionText(doc),
                    Options = ExtractOptions(doc),
                    DiscussionUrl = url
                };

                _questions.Add(question);
                Console.WriteLine($"Gevonden vraag {question.QuestionNumber}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij vraag {url}: {ex.Message}");
            }
        }

        static int ExtractQuestionNumber(HtmlDocument doc)
        {
            var header = doc.DocumentNode
                .SelectSingleNode("//h1[contains(@class, 'discussion-header')]")
                ?.InnerText;

            if (header != null)
            {
                var match = Regex.Match(header, @"question (\d+)", RegexOptions.IgnoreCase);
                if (match.Success)
                    return int.Parse(match.Groups[1].Value);
            }
            return 0;
        }

        static string ExtractQuestionText(HtmlDocument doc)
        {
            return doc.DocumentNode
                .SelectSingleNode("//div[contains(@class, 'question-body')]//p")
                ?.InnerText
                ?.Trim() ?? "Geen vraagtekst gevonden";
        }

        static Dictionary<string, string> ExtractOptions(HtmlDocument doc)
        {
            var options = new Dictionary<string, string>();
            var nodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'question-choices')]//li")
                        ?? new HtmlNodeCollection(null);

            foreach (var node in nodes)
            {
                try
                {
                    var text = node.InnerText.Trim();
                    if (text.Length < 2) continue;

                    var separatorIndex = text.IndexOf('.');
                    if (separatorIndex <= 0 || separatorIndex >= text.Length - 1) continue;

                    var key = text.Substring(0, separatorIndex).Trim();
                    var value = text.Substring(separatorIndex + 1).Trim();

                    if (!options.ContainsKey(key))
                        options[key] = value;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fout bij verwerken antwoordoptie: {ex.Message}");
                }
            }
            return options;
        }

        static void SaveQuestionsToWord(string filepath, IEnumerable<ExamQuestion> questions)
        {
            if (File.Exists(filepath))
                File.Delete(filepath);

            using (var wordDoc = WordprocessingDocument.Create(filepath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                foreach (var q in questions)
                {
                    if (string.IsNullOrWhiteSpace(q.QuestionText) || q.QuestionText.StartsWith("Geen vraagtekst")) continue;
                    if (q.Options == null || q.Options.Count == 0) continue;

                    // Vraag
                    body.AppendChild(new Paragraph(new Run(new Text($"Vraag: {q.QuestionText}"))));
                    body.AppendChild(new Paragraph(new Run(new Text("")))); // Lege regel

                    // Antwoordopties
                    foreach (var opt in q.Options)
                    {
                        var optText = $"{opt.Key}. {opt.Value}";
                        body.AppendChild(new Paragraph(new Run(new Text(optText))));
                    }

                    // Link naar de discussiepagina
                    if (!string.IsNullOrWhiteSpace(q.DiscussionUrl))
                        body.AppendChild(new Paragraph(new Run(new Text($"Bekijk deze vraag online: {q.DiscussionUrl}"))));

                    // Lege regel tussen vragen
                    body.AppendChild(new Paragraph(new Run(new Text(""))));
                }
            }
        }
    }

    public class ExamQuestion
    {
        public int QuestionNumber { get; set; }
        public string QuestionText { get; set; }
        public Dictionary<string, string> Options { get; set; }
        public string DiscussionUrl { get; set; }
    }
}
