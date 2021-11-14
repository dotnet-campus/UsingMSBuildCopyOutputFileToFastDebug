using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UsingMSBuildCopyOutputFileToFastDebug
{
    public class LaunchSettingsParser
    {
        public bool Execute()
        {
            var file = Path.Combine("Properties", "launchSettings.json");
            Console.WriteLine("开始从 launchSettings 文件读取输出文件夹");
            Console.WriteLine($"开始读取{file}文件");
            if (!File.Exists(file))
            {
                Console.WriteLine($"找不到{file}文件，读取结束");
                return false;
            }

            var text = File.ReadAllText(file);

            var root = (JObject) JsonConvert.DeserializeObject(text);
            if (root == null)
            {
                return false;
            }

            var profilesObject = (JObject)root["profiles"];

            if (profilesObject == null)
            {
                return false;
            }

            foreach (var keyValuePair in profilesObject)
            {
                var commandName = keyValuePair.Value?["commandName"];
                if (commandName?.ToString() == "Executable")
                {
                    var executablePath = keyValuePair.Value["executablePath"];
                    if (executablePath != null)
                    {
                        // executablePath = C:\lindexi\foo\foo.exe
                        LaunchMainProjectExecutablePath = executablePath.ToString();
                        return true;
                    }
                }
            }

            return false;
        }

        public string LaunchMainProjectExecutablePath { set; get; }
    }
}