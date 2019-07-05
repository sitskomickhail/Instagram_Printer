using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Printing;
using System.Drawing;

namespace PrinterLibrary
{
    public class PrintCl
    {
        public static void PrintImage()
        {
            PrintDocument printD = new PrintDocument();
            printD.PrintPage += PrintD_PrintPage;
            printD.Print();
        }

        private static void PrintD_PrintPage(object sender, PrintPageEventArgs e)
        {
            Image img = Image.FromFile("D:\\Foto.jpg");
            Point loc = new Point(100, 100);
            e.Graphics.DrawImage(img, loc);
        }
    }
}