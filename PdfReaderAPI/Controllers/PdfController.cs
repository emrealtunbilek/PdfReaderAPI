using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Reflection.PortableExecutable;
using System.Text;

namespace PdfReaderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        [HttpGet]
        public IActionResult TestAPI()
        {
            return Ok("Test");
        }

        [HttpPost]
        public IActionResult ReadPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            Dictionary<string, string> mappings = new Dictionary<string, string>()
                {
                    { "Dönen Varlıklar", "" },
                    { "Kanunen Kabul Edilmeyen Giderler", "" },
                    { "Dönem Net Karı veya Zararı", "" },
                    { "Brüt Satışlar", "" },
                    { "Kısa Vadeli Yabancı Kaynaklar", "" },
                };




            using var memoryStream = new MemoryStream();
                file.CopyTo(memoryStream);
                byte[] pdfBytes = memoryStream.ToArray();

                PdfReader reader = new PdfReader(pdfBytes);

                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    string pageText = PdfTextExtractor.GetTextFromPage(reader, i);


                foreach (var mapping in mappings)
                {
                    if (pageText.Contains(mapping.Key))
                    {
                        mappings[mapping.Key] = ExtractValue(pageText, mapping.Key);
                    }
                }
            }

                reader.Close();

                return Ok(mappings);


        }


        private string ExtractValue(string text, string key)
        {
            int startIndex = text.IndexOf(key) + key.Length;
            int endIndex = text.IndexOf('\n', startIndex);

            return text.Substring(startIndex, endIndex - startIndex).Trim();
        }



        private void ExtractKeysAndValues(string text, Dictionary<string, List<string>> mappings)
        {

            string[] lines = text.Split('\n');
            foreach (string line in lines)
            {
                int colonIndex = line.IndexOf(':');
                if (colonIndex != -1)
                {
                    string key = line.Substring(0, colonIndex).Trim();
                    string value = line.Substring(colonIndex + 1).Trim();

                    if (!mappings.ContainsKey(key))
                    {
                        mappings[key] = new List<string>();
                    }
                    else
                    {
                        key += Guid.NewGuid();
                        mappings[key] = new List<string>();
                    }

                    mappings[key].Add(value);
                }
            }
        }


    }
}
