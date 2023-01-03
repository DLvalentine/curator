using System;
using System.Net;
using System.Runtime.InteropServices;
using Reddit;

// NOTE -> Currently just proof of concept. Ideally this would be a bit more OO with methods, etc. Procedural is fine for now.
//         Just getting my feet wet with the Reddit API for use later.
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
            // TODO -> token should be permanent, but easy to regen if needed. Might want to document we used Reddit.NET's built in retriever to get it
            var reddit = new RedditClient(appId: "appId", appSecret: "appSecret", refreshToken: "refreshToken");

            // Set sub, get first "hot" post
            var museumSub = reddit.Subreddit("Museum");
            var hotMuseumFirstPost = museumSub.Posts.Hot[0];

            // Get the properties we care about - title + image url
            string postURL = hotMuseumFirstPost.Listing.URL;

            // Save to appdata
            // NOTE/TODO -> Probably requires admin privileges...might need to document this or add to manifest so users get prompted
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var webClient = new WebClient();
            webClient.DownloadFile(postURL, $"{appdata}\\curatorPick.jpg");
            webClient.Dispose();

            // Set as wallpaper
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 1, $"{appdata}\\curatorPick.jpg", SPIF_UPDATEINIFILE);
        }
    }
}
