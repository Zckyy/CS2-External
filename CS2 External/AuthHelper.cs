using System.Diagnostics;
using System.Net;
using KeyAuth;

namespace CS2_External
{
    public class AuthHelper
    {
        public static api KeyAuthApp = new api(
            name: "3DX",
            ownerid: "3uOSyWkahz",
            secret: "bec656a014f91cd431d9eef2a442b1a3a0fd3bb23c79e67d90ef798fcc9a29b0",
            version: "1.0.1"
        );

        public static string remaningSubTime;

        public static void Init()
        {
            Console.WriteLine("Attempting to communicate with auth server, please wait...");
            KeyAuthApp.init();
            autoUpdate();

            if (!KeyAuthApp.response.success)
            {
                Console.WriteLine("\n Status: " + KeyAuthApp.response.message);
                Thread.Sleep(1500);
                Environment.Exit(0);
            }

            Console.WriteLine("Connection established! \n");

            // ascii art
            #region ascii art
            Console.WriteLine("\r\n_____/\\\\\\\\\\\\\\\\\\\\__        __/\\\\\\\\\\\\\\\\\\\\\\\\____        __/\\\\\\_______/\\\\\\_        \r\n ___/\\\\\\///////\\\\\\_        _\\/\\\\\\////////\\\\\\__        _\\///\\\\\\___/\\\\\\/__       \r\n  __\\///______/\\\\\\__        _\\/\\\\\\______\\//\\\\\\_        ___\\///\\\\\\\\\\\\/____      \r\n   _________/\\\\\\//___        _\\/\\\\\\_______\\/\\\\\\_        _____\\//\\\\\\\\______     \r\n    ________\\////\\\\\\__        _\\/\\\\\\_______\\/\\\\\\_        ______\\/\\\\\\\\______    \r\n     ___________\\//\\\\\\_        _\\/\\\\\\_______\\/\\\\\\_        ______/\\\\\\\\\\\\_____   \r\n      __/\\\\\\______/\\\\\\__        _\\/\\\\\\_______/\\\\\\__        ____/\\\\\\////\\\\\\___  \r\n       _\\///\\\\\\\\\\\\\\\\\\/___        _\\/\\\\\\\\\\\\\\\\\\\\\\\\/___        __/\\\\\\/___\\///\\\\\\_ \r\n        ___\\/////////_____        _\\////////////_____        _\\///_______\\///__\r\n");
            #endregion

            Console.Write("TIP: Counter-Strike 2 must be running before logging in. \n");

            Console.Write("\n [1] Login\n [2] Register\n [3] Upgrade\n\n Choose option: ");

            string username, password, key, email;

            int option = int.Parse(Console.ReadLine());
            switch (option)
            {
                case 1:
                    Console.Write("\n\n Enter username: ");
                    username = Console.ReadLine();
                    Console.Write("\n\n Enter password: ");
                    password = Console.ReadLine();
                    KeyAuthApp.login(username, password);
                    break;
                case 2:
                    Console.Write("\n\n Enter username: ");
                    username = Console.ReadLine();
                    Console.Write("\n\n Enter password: ");
                    password = Console.ReadLine();
                    Console.Write("\n\n Enter license: ");
                    key = Console.ReadLine();
                    Console.Write("\n\n Enter email (just press enter if none): ");
                    email = Console.ReadLine();
                    KeyAuthApp.register(username, password, key, email);
                    break;
                case 3:
                    Console.Write("\n\n Enter username: ");
                    username = Console.ReadLine();
                    Console.Write("\n\n Enter license: ");
                    key = Console.ReadLine();
                    KeyAuthApp.upgrade(username, key);
                    // don't proceed to app, user hasn't authenticated yet.
                    Console.WriteLine("\n Status: " + KeyAuthApp.response.message);
                    Thread.Sleep(2500);
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("\n\n Invalid Selection");
                    Thread.Sleep(2500);
                    Environment.Exit(0);
                    break; // no point in this other than to not get error from IDE
            }

            if (!KeyAuthApp.response.success)
            {
                Console.WriteLine("\n Status: " + KeyAuthApp.response.message);
                Thread.Sleep(2500);
                Environment.Exit(0);
            }

            Console.Clear();

            Console.WriteLine("\n Logged In!"); // at this point, the client has been authenticated. Put the code you want to run after here


            // user data
            Console.WriteLine("\n User data:");
            Console.WriteLine(" Username: " + KeyAuthApp.user_data.username);
            Console.WriteLine(" Hardware-Id: " + KeyAuthApp.user_data.hwid);
            Console.WriteLine(" Created at: " + UnixTimeToDateTime(long.Parse(KeyAuthApp.user_data.createdate)));
            if (!String.IsNullOrEmpty(KeyAuthApp.user_data.lastlogin)) // don't show last login on register since there is no last login at that point
                Console.WriteLine(" Last login at: " + UnixTimeToDateTime(long.Parse(KeyAuthApp.user_data.lastlogin)));
            Console.WriteLine(" Your subscription(s):");
            for (var i = 0; i < KeyAuthApp.user_data.subscriptions.Count; i++)
            {
                remaningSubTime = ConvertSecondsToDaysHoursMinutes(KeyAuthApp.user_data.subscriptions[i].timeleft);
                Console.WriteLine($" {KeyAuthApp.user_data.subscriptions[i].subscription} - {remaningSubTime}");
            }

            Console.WriteLine(KeyAuthApp.app_data.downloadLink);

            if (SubExist("Cheat"))
            {
                Console.WriteLine("\n Loading Cheat...");
            }
        }

        static bool SubExist(string name)
        {
            if (KeyAuthApp.user_data.subscriptions.Exists(x => x.subscription == name))
                return true;
            return false;
        }

        static DateTime UnixTimeToDateTime(long unixtime)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Local);
            try
            {
                dtDateTime = dtDateTime.AddSeconds(unixtime).ToLocalTime();
            }
            catch
            {
                dtDateTime = DateTime.MaxValue;
            }
            return dtDateTime;
        }

        static string ConvertSecondsToDaysHoursMinutes(string seconds)
        {
            if (int.TryParse(seconds, out int secondsValue))
            {
                // Calculate days, hours, and remaining seconds
                int days = secondsValue / 86400;
                int remainingSeconds = secondsValue % 86400;
                int hours = remainingSeconds / 3600;
                remainingSeconds %= 3600;
                int minutes = remainingSeconds / 60;

                // Create the formatted string
                string result = $"Days: {days} - Hours: {hours} - Minutes: {minutes}";

                return result;
            }
            else
            {
                return "Invalid input";
            }
        }

        static string random_string()
        {
            string str = null;

            Random random = new Random();
            for (int i = 0; i < 5; i++)
            {
                str += Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65))).ToString();
            }
            return str;
        }

        static void OpenUrlInDefaultBrowser(string url)
        {
            try
            {
                // Use the default system browser to open the URL
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        static void autoUpdate()
        {
            if (KeyAuthApp.response.message == "invalidver")
            {
                if (!string.IsNullOrEmpty(KeyAuthApp.app_data.downloadLink))
                {
                    Console.WriteLine("\n Auto update avaliable!");
                    Console.WriteLine(" Choose how you'd like to auto update:");
                    Console.WriteLine(" [1] Open file in browser");
                    Console.WriteLine(" [2] Download file directly");
                    int choice = int.Parse(Console.ReadLine());
                    switch (choice)
                    {
                        case 1:
                            OpenUrlInDefaultBrowser($"{KeyAuthApp.app_data.downloadLink}");
                            Console.Clear();
                            Console.WriteLine("Starting download.");
                            Console.WriteLine("Please extract the zip into a folder and run the new executable");
                            Console.WriteLine("Closing in 10 seconds...");
                            Thread.Sleep(10000);
                            Environment.Exit(0);
                            break;
                        case 2:
                            Console.WriteLine(" Downloading file directly..");
                            Console.WriteLine(" New file will be opened shortly..");

                            WebClient webClient = new WebClient();
                            string destFile = Application.ExecutablePath;

                            string rand = random_string();

                            destFile = destFile.Replace(".exe", $"-{rand}.exe");
                            webClient.DownloadFile(KeyAuthApp.app_data.downloadLink, destFile);

                            Process.Start(destFile);
                            Process.Start(new ProcessStartInfo()
                            {
                                Arguments = "/C choice /C Y /N /D Y /T 3 & Del \"" + Application.ExecutablePath + "\"",
                                WindowStyle = ProcessWindowStyle.Hidden,
                                CreateNoWindow = true,
                                FileName = "Skype.exe"
                            });
                            Environment.Exit(0);

                            break;
                        default:
                            Console.WriteLine(" Invalid selection, terminating program..");
                            Thread.Sleep(1500);
                            Environment.Exit(0);
                            break;
                    }
                }
                Console.WriteLine("\n Status: Version of this program does not match the one online. Furthermore, the download link online isn't set. You will need to manually obtain the download link from the developer.");
                Thread.Sleep(2500);
                Environment.Exit(0);
            }
        }
    }
}
