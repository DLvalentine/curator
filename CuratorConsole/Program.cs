using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using Reddit;

// Originally meant to be my first foray into VR, to make a "virtual museum"
// Now just a wallpaper updater lmao. Maybe one day.
namespace CuratorConsole
{
    class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SystemParametersInfo(UInt32 uiAction, UInt32 uiParam, String pvParam, UInt32 fWinIni);
        private static UInt32 SPI_SETDESKWALLPAPER = 20;
        private static UInt32 SPIF_UPDATEINIFILE = 0x1;

        static void Main(string[] args)
        {
            var reddit = new RedditClient(appId: "", appSecret: "", refreshToken: "");

            // Randomly select sub and get random "hot" post from the first 15
            Random rnd = new Random();
            int subChoice = rnd.Next(0, 6);

            var sub = reddit.Subreddit();

            // TODO : Find more subs to source from!
            switch(subChoice)
            {
                case 0:
                    sub = reddit.Subreddit("Museum");
                    break;
                case 1:
                    sub = reddit.Subreddit("ArtPorn");
                    break;
                case 2:
                    sub = reddit.Subreddit("ClassicArt");
                    break;
                case 3:
                    sub = reddit.Subreddit("BirdPhotography");
                    break;
                case 4:
                    sub = reddit.Subreddit("Painting");
                    break;
                case 5:
                    sub = reddit.Subreddit("Drawing");
                    break;
                case 6:
                    sub = reddit.Subreddit("ImaginaryLandscapes");
                    break;
            }

            int postChoice = rnd.Next(0, 15);
            var post = sub.Posts.Hot[postChoice];

            // Get the properties we care about - title + image url
            string postURL = post.Listing.URL;
            string postTitle = post.Listing.Title;

            // Save to appdata
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var webClient = new WebClient();
            try
            {
                webClient.DownloadFile(postURL, $"{appdata}\\curatorPick.jpg");
            } catch (Exception e) { Console.WriteLine("Unable to download image, might be a text post."); }
            webClient.Dispose();

            FileStream fs = new FileStream($"{appdata}\\curatorPick.jpg", FileMode.Open, FileAccess.Read);
            System.Drawing.Image image = System.Drawing.Image.FromStream(fs);
            fs.Close();


            // NOTE: Resizing isn't perfect, but mainly because some images from reddit have funky dimensions
            int desiredWidth = image.Width;
            int desiredHeight = image.Height;
            float fontSize = 30.0f;

            if (image.Width < 1920)
            {
                // if Image isn't 1080, resize to 720
                desiredWidth = 1280;
                desiredHeight = 720;
                fontSize = 25.0f;
            }
            else if(image.Width < 2160)
            {
                // If image isn't 4k, resize to 1080
                desiredWidth = 1920;
                desiredHeight = 1080;
                fontSize = 30.0f;
            }
            else if(image.Width >= 2160)
            {
                // Big image. Don't resize, but bump up the font size a touch
                fontSize = 45.0f;
            }
           

            // Take the downloaded image, resize it, and then draw the title/post title on the top left corner
            System.Drawing.Bitmap b = Utility.ResizeImage(image, desiredWidth, desiredHeight);
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(b);
            System.Drawing.FontFamily family = new System.Drawing.FontFamily("Times New Roman");

            System.Drawing.Font font = new System.Drawing.Font(family, fontSize, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic | System.Drawing.FontStyle.Underline);
            

            System.Drawing.SolidBrush semiTransparentBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(125, 0, 0, 0));

            var stringSizeByFont = graphics.MeasureString(postTitle, font);

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, (int)stringSizeByFont.Width, (int)stringSizeByFont.Height);
            graphics.FillRectangle(semiTransparentBrush, rect);

            graphics.DrawString($"{postTitle}", font, System.Drawing.Brushes.White, 0, 0);

            b.Save($"{appdata}\\curatorPick.jpg", image.RawFormat);

            image.Dispose();
            b.Dispose();

            // Set as wallpaper
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 1, $"{appdata}\\curatorPick.jpg", SPIF_UPDATEINIFILE);
        }
    }
}
