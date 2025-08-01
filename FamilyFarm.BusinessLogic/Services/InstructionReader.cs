using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace FamilyFarm.BusinessLogic.Services
{
    public class InstructionReader
    {
        public static string ReadInstructions(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Instruction file not found.", filePath);

            var instructions = new StringBuilder();

            using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, false))
            {
                var body = doc.MainDocumentPart.Document.Body;
                foreach (var paragraph in body.Elements<Paragraph>())
                {
                    var text = paragraph.InnerText.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        instructions.AppendLine(text);
                    }
                }
            }

            return instructions.ToString();
        }
    }
}
