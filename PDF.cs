using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.VisualBasic.Devices;

namespace SO_NAIJI
{
    public class PDF
    {
        public void InsertTargetPages(PdfCopy objPDFCopy, string PDFpath,List<string> targetSoNo)
        {
            PdfReader pdfReader = new PdfReader(PDFpath);
            SimpleTextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
            List<int> pages = new List<int>();

            for (int pageNum = 1; pageNum <= pdfReader.NumberOfPages; pageNum++)
            {
                foreach (string SoNo in targetSoNo)
                {
                    strategy = new SimpleTextExtractionStrategy();
                    if (PdfTextExtractor.GetTextFromPage(pdfReader, pageNum, strategy).ToString().Contains(SoNo))
                    {
                        pages.Add(pageNum);
                        break;
                    }
                }
            }
            objPDFCopy.AddDocument(pdfReader, pages);
        }
    }
}
