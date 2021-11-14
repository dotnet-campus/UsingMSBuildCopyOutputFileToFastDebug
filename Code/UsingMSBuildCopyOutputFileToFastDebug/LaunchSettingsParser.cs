using System;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using Lsj.Util.Collections;
using Lsj.Util.JSON;
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
            var profilesObject = (JObject)root["profiles"];

            foreach (var keyValuePair in profilesObject)
            {
                var commandName = keyValuePair.Value["commandName"];
                if (commandName?.ToString() == "Executable")
                {
                    var executablePath = keyValuePair.Value["executablePath"];
                    if (executablePath != null)
                    {
                        LaunchMainProjectPath = executablePath.ToString();
                        return true;
                    }
                }
            }

            return false;
        }

        public string LaunchMainProjectPath { set; get; }
    }
}