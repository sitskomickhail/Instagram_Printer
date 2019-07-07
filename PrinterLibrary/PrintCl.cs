using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Printing;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PrinterLibrary
{
    public class PrintCl
    {
        public string Path { get; set; }

        public PrintCl(string path)
        {
            if (path.Length > 0)
                Path = path;
        }

        public void PrintImage()
        {
            PrintDocument printD = new PrintDocument();
            printD.PrintPage += PrintD_PrintPage;
            printD.Print();
        }

        private void PrintD_PrintPage(object sender, PrintPageEventArgs e)
        {
            using (var bmp = new Bitmap(Path))
            {
                Image img = new Bitmap(bmp);
                Point loc = new Point(0, 0);
                e.Graphics.DrawImage(img, loc);
            }
        }
    }
}