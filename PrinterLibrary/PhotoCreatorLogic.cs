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
using System.Diagnostics;

namespace PrinterLibrary
{
    public class PhotoCreatorLogic
    {
        public string TemplateName { get; set; }
        public string LogoName { get; set; }
        private const string _imgTemplatePath = @"\Templates";
        private const string _imgLogoPath = @"\Logos";
        private const string _imgPath = @"\Photos";

        public PhotoCreatorLogic()
        {
            var files = Directory.GetFiles(Environment.CurrentDirectory + _imgTemplatePath + "\\");
            if (files.Count() > 0)
            {
                var file = files[0].Split('\\');
                TemplateName = file[file.Length - 1];
            }

            var logos = Directory.GetFiles(Environment.CurrentDirectory + _imgLogoPath + "\\");
            if (logos.Count() > 0)
            {
                var logo = logos[0].Split('\\');
                LogoName = logo[logo.Length - 1];
            }

        }

        public string CreatePhoto(string photoName)
        {
            int length = 9;
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            string path = res.ToString() + ".jpg";
            path = "Photos" + "\\" + photoName.Split('\\')[0] + "\\" + path;
            using (var img0 = (Bitmap)Image.FromFile(Environment.CurrentDirectory + _imgPath + "\\" + photoName))
            using (var img1 = (Bitmap)Image.FromFile(Environment.CurrentDirectory + _imgTemplatePath + "\\" + TemplateName))
            using (var bmp = AlphaBlending(img0, img1, (byte)0))
            {
                bmp.Save(path, ImageFormat.Jpeg);
            }
            return path;
        }

        public void AddLogo(string photo)
        {
            Bitmap bmp = null;
            using (var img0 = (Bitmap)Image.FromFile(Environment.CurrentDirectory + _imgLogoPath + "\\" + LogoName))
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
            bmp.Save(photo, ImageFormat.Jpeg);
        }

        public void AddPhoto(string oldPhoto, string secondPhoto)
        {
            Bitmap bmp = null;
            using (var img0 = (Bitmap)Image.FromFile(Environment.CurrentDirectory + _imgPath + "\\" + secondPhoto))
            using (var img1 = (Bitmap)Image.FromFile(Environment.CurrentDirectory + "\\" + oldPhoto))
            {
                if (img0 == null || img1 == null)
                    throw new NullReferenceException();

                bmp = new Bitmap(img1.Width, img1.Height, PixelFormat.Format24bppRgb);
                //Width = 1280, Height = 861
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
            bmp.Save(oldPhoto, ImageFormat.Jpeg);
        }

        public Image ResizeImage(int newSize, string path)
        {
            Image originalImage;
            int newHeight = 0;
            using (var bmpTemp = new Bitmap(path))
            {
                originalImage = new Bitmap(bmpTemp);
                if (originalImage.Width <= newSize)
                    newSize = originalImage.Width;

                newHeight = originalImage.Height * newSize / originalImage.Width;

                if (newHeight > newSize)
                {
                    newSize = originalImage.Width * newSize / originalImage.Height;
                    newHeight = newSize;
                }
            }
            return originalImage.GetThumbnailImage(newSize, newHeight, null, IntPtr.Zero);
        }

        private Bitmap AlphaBlending(Bitmap x, Bitmap y, byte s)
        {
            if (x == null || y == null)
                throw new NullReferenceException();

            Bitmap bmp = new Bitmap(
                y.Width, y.Height, PixelFormat.Format24bppRgb
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
