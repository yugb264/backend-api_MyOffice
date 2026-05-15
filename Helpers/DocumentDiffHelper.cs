using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;
namespace backend_api.Helpers   
{
    public class DocumentDiffHelper
    {
        public byte[] GenerateDiff(byte[] file1Bytes, byte[] file2Bytes)
        {
            var text1 = ExtractText(file1Bytes);
            var text2 = ExtractText(file2Bytes);
            Console.WriteLine("TEXT 1:");
            Console.WriteLine(text1);

            Console.WriteLine("TEXT 2:");
            Console.WriteLine(text2);
            var diffParts = BuildWordDiff(text1, text2);

            return CreateDiffDocument(diffParts);
        }

        private string ExtractText(byte[] fileBytes)
        {
            using var stream = new MemoryStream(fileBytes);
            using var doc = WordprocessingDocument.Open(stream, false);

            var body = doc.MainDocumentPart.Document.Body;
            return body.InnerText;
        }

        private List<(string text, string type)> BuildWordDiff(string oldText, string newText)
        {
            var oldWords = oldText.Split(' ');
            var newWords = newText.Split(' ');

            var result = new List<(string, string)>();

            int i = 0, j = 0;

            while (i < oldWords.Length || j < newWords.Length)
            {
                var oldWord = i < oldWords.Length ? oldWords[i] : null;
                var newWord = j < newWords.Length ? newWords[j] : null;

                if (oldWord == newWord)
                {
                    result.Add((oldWord + " ", "same"));
                    i++; j++;
                }
                else
                {
                    if (oldWord != null)
                    {
                        result.Add((oldWord + " ", "removed"));
                        i++;
                    }

                    if (newWord != null)
                    {
                        result.Add((newWord + " ", "added"));
                        j++;
                    }
                }
            }

            return result;
        }
        private byte[] CreateDiffDocument(List<(string text, string type)> parts)
        {
            using var ms = new MemoryStream();

            using (var wordDoc = WordprocessingDocument.Create(ms, DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = new Body();

                var para = new Paragraph();

                foreach (var part in parts)
                {
                    var run = new Run();

                    if (part.type == "added")
                    {
                        run.RunProperties = new RunProperties(
                            new Color() { Val = "00AA00" }
                        );
                    }
                    else if (part.type == "removed")
                    {
                        run.RunProperties = new RunProperties(
                            new Color() { Val = "FF0000" },
                            new Strike() // 👈 THIS MAKES IT CLEAR
                        );
                    }

                    run.Append(new Text(part.text)
                    {
                        Space = SpaceProcessingModeValues.Preserve
                    });

                    para.Append(run);
                }

                body.Append(para);
                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }

            return ms.ToArray();
        }
    }
}