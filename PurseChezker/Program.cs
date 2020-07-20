using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using YamlDotNet.RepresentationModel;

namespace PurseChezker
{
    static class Program {
        private static Dictionary<string, string> customFiles = new Dictionary<string, string>();

        [STAThread]
        static void Main() {
            run();
        }

        private static void run() {
            configLoad();
            isLogin();

            Console.ReadLine();
        }

        private static bool isLogin() {
            bool obf = false;

            var config = new YamlStream();
            config.Load(File.OpenText(customFiles["configFile"]));

            var mapping = (YamlMappingNode)config.Documents[0].RootNode;
            
            foreach (var entry in mapping.Children) {
                Console.WriteLine(((YamlScalarNode)entry.Key).Value);
            }

            var license = (YamlMappingNode)mapping.Children["license"];

            foreach (var item in license) {
                Console.WriteLine(
                    "{0}\t{1}",
                    item.Children[new YamlScalarNode("user")],
                    item.Children[new YamlScalarNode("key")]
                );
            }

            return obf;
        }

        private static void configLoad() {
            string strPath = AppDomain.CurrentDomain.BaseDirectory + "/config";

            if (!Directory.Exists(strPath)) {
                Directory.CreateDirectory(strPath);
            }

            string configPath = strPath + "/config.yml";

            customFiles.Add("configFile", configPath);

            if (!File.Exists(configPath)) {
                File.Create(configPath).Close();
                File.WriteAllLines(configPath, new string[] {
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
                });
            }
        }

        private static bool comboLoad(bool openDialog, string cbPath) {
            bool obf = false;

            if (openDialog) {
                using (OpenFileDialog openFileDialog = new OpenFileDialog()) {
                    openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    openFileDialog.Filter = "Combo File (*.txt, *.yml, *.yaml)|*.txt|*.yml|*.yaml";
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

        private static bool proxiesLoad(bool openDialog, string pxPath) {
            bool obf = false;

            if (openDialog) {
                using (OpenFileDialog openFileDialog = new OpenFileDialog()) {
                    openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    openFileDialog.Filter = "Proxies File (*.txt, *.yml, *.yaml)|*.txt|*.yml|*.yaml";
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
    }
}
