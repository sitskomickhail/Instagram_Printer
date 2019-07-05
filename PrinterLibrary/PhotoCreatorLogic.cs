using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.Win32;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PrinterLibrary
{
    public class PhotoCreatorLogic
    {
        public static string TemplateName { get; set; }

        private const string _imgTemplatePath = @"\Templates";
        private const string _imgLogoPath = @"\Logos";
        private const string _imgPath = @"\Photos";

        public PhotoCreatorLogic(string photoName)
        {
            if (TemplateName.Length == 0)
            {
                var files = Directory.GetFiles(Environment.CurrentDirectory + _imgTemplatePath);
                if (files.Count() > 0)
                    TemplateName = files[0];
            }

            using (var img0 = (Bitmap)Image.FromFile(Environment.CurrentDirectory + _imgPath + "\\" + photoName))
            using (var img1 = (Bitmap)Image.FromFile(Environment.CurrentDirectory + _imgTemplatePath + "\\" + TemplateName))
            using (var bmp = AlphaBlending(img0, img1, (byte)0))
            {
                bmp.Save("photo.jpg", ImageFormat.Jpeg);
            }
        }

        internal void AddLogo(string photo, string logo)
        {
            Bitmap bmp = null;
            using (var img0 = (Bitmap)Image.FromFile(Environment.CurrentDirectory + _imgLogoPath + "\\" + logo))
            using (var img1 = (Bitmap)Image.FromFile(Environment.CurrentDirectory + "\\" + photo))
            {
                if (img0 == null || img1 == null)
                    throw new NullReferenceException();

                bmp = new Bitmap(
                    Math.Max(img0.Width, img1.Width),
                    Math.Max(img0.Height, img1.Height),
                    PixelFormat.Format24bppRgb
                    ); //Width = 1280, Height = 861
                       //1 size pict: Width = 630, Height = 700

                Color clr0, clr1;
                for (int _x = 0; _x < bmp.Width; _x++)
                    for (int _y = 0; _y < bmp.Height; _y++)
                    {
                        clr0 = img1.GetPixel(_x, _y);
                        bmp.SetPixel(_x, _y,
                            Color.FromArgb(
                                Math.Min(255, clr0.R * 1),
                                Math.Min(255, clr0.G * 1),
                                Math.Min(255, clr0.B * 1)
                            )
                        );
                    }

                int widthFor1 = Math.Min(img0.Width, 630);
                int heightFor1 = Math.Min(img0.Height, 680);

                for (int _x = 0; _x < widthFor1; _x++)
                    for (int _y = 0; _y < heightFor1; _y++)
                    {
                        clr0 = img0.GetPixel(_x, _y);
                        clr1 = img1.GetPixel(_x, _y);
                        bmp.SetPixel(_x + 492, _y + 552,
                            Color.FromArgb(
                                Math.Min(255, clr0.R * 1),
                                Math.Min(255, clr0.G * 1),
                                Math.Min(255, clr0.B * 1)
                            )
                        );
                    }

            }
            bmp.Save("photo.jpg", ImageFormat.Jpeg);
        }

        internal void AddPhoto(string oldPhoto, string secondPhoto)
        {
            Bitmap bmp = null;
            using (var img0 = (Bitmap)Image.FromFile(Environment.CurrentDirectory + _imgPath + "\\" + secondPhoto))
            using (var img1 = (Bitmap)Image.FromFile(Environment.CurrentDirectory + "\\" + oldPhoto))
            {
                if (img0 == null || img1 == null)
                    throw new NullReferenceException();

                bmp = new Bitmap(
                    Math.Max(img0.Width, img1.Width),
                    Math.Max(img0.Height, img1.Height),
                    PixelFormat.Format24bppRgb
                    ); //Width = 1280, Height = 861
                       //1 size pict: Width = 630, Height = 700

                Color clr0, clr1;
                for (int _x = 0; _x < bmp.Width; _x++)
                    for (int _y = 0; _y < bmp.Height; _y++)
                    {
                        clr0 = img1.GetPixel(_x, _y);
                        bmp.SetPixel(_x, _y,
                            Color.FromArgb(
                                Math.Min(255, clr0.R * 1),
                                Math.Min(255, clr0.G * 1),
                                Math.Min(255, clr0.B * 1)
                            )
                        );
                    }

                int widthFor1 = Math.Min(img0.Width, 630);
                int heightFor1 = Math.Min(img0.Height, 680);

                for (int _x = 10; _x < widthFor1; _x++)
                    for (int _y = 20; _y < heightFor1; _y++)
                    {
                        clr0 = img0.GetPixel(_x, _y);
                        clr1 = img1.GetPixel(_x, _y);
                        bmp.SetPixel(_x + 635, _y,
                            Color.FromArgb(
                                Math.Min(255, clr0.R * 1),
                                Math.Min(255, clr0.G * 1),
                                Math.Min(255, clr0.B * 1)
                            )
                        );
                    }

            }
            bmp.Save("photo.jpg", ImageFormat.Jpeg);
        }

        Bitmap AlphaBlending(Bitmap x, Bitmap y, byte s)
        {
            if (x == null || y == null)
                throw new NullReferenceException();

            Bitmap bmp = new Bitmap(
                Math.Max(x.Width, y.Width),
                Math.Max(x.Height, y.Height),
                PixelFormat.Format24bppRgb
                ); //Width = 1280, Height = 861
            //1 size pict: Width = 630, Height = 700

            Color clr0, clr1;
            for (int _x = 0; _x < bmp.Width; _x++)
                for (int _y = 0; _y < bmp.Height; _y++)
                {
                    clr0 = y.GetPixel(_x, _y);
                    bmp.SetPixel(_x, _y,
                        Color.FromArgb(
                            Math.Min(255, clr0.R * (255 - s) / 255),
                            Math.Min(255, clr0.G * (255 - s) / 255),
                            Math.Min(255, clr0.B * (255 - s) / 255)
                        )
                    );
                }

            int widthFor1 = Math.Min(x.Width, 640);
            int heightFor1 = Math.Min(x.Height, 680);

            for (int _x = 10; _x < widthFor1; _x++)
                for (int _y = 20; _y < heightFor1; _y++)
                {
                    clr0 = x.GetPixel(_x, _y);
                    clr1 = y.GetPixel(_x, _y);
                    bmp.SetPixel(_x, _y,
                        Color.FromArgb(
                            Math.Min(255, clr0.R * (255 - s) / 255 + clr1.R * s / 255),
                            Math.Min(255, clr0.G * (255 - s) / 255 + clr1.G * s / 255),
                            Math.Min(255, clr0.B * (255 - s) / 255 + clr1.B * s / 255)
                        )
                    );
                }

            string day = (DateTime.Now.Day < 10) ? $"0{DateTime.Now.Day}" : $"{DateTime.Now.Day}";
            string month = (DateTime.Now.Month < 10) ? $"0{DateTime.Now.Month}" : $"{DateTime.Now.Month}";
            string firstText = $"{day}.{month}.{DateTime.Now.Year}";

            PointF firstLocation = new PointF(645f, 697f);

            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                PrivateFontCollection pfc = new PrivateFontCollection();
                pfc.AddFontFile("SpecialElite-Regular.ttf");

                using (Font arialFont = new Font(pfc.Families[0], 21, FontStyle.Bold))
                {
                    graphics.DrawString(firstText, arialFont, Brushes.Black, firstLocation);
                }
            }
            return bmp;
        }
    }
}
