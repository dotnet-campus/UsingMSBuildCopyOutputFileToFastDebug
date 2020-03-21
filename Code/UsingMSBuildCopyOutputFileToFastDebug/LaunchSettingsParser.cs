using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Xml;
using Microsoft.Build.Framework;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

namespace UsingMSBuildCopyOutputFileToFastDebug
{
    public class LaunchSettingsParser : Microsoft.Build.Utilities.Task
    {
        /// <inheritdoc />
        public override bool Execute()
        {
            Debugger.Launch();

            var file = Path.Combine("Properties", "launchSettings.json");
            Console.WriteLine("开始从 launchSettings 文件读取输出文件夹");
            Console.WriteLine($"开始读取{file}文件");
            if (!File.Exists(file))
            {
                Console.WriteLine($"找不到{file}文件，读取结束");
            }

            var text = File.ReadAllText(file);


            //var jObject = JObject.Parse(File.ReadAllText(file));

            return true;
        }

        [Output]
        public string LaunchMainProjectPath { set; get; }
    }
}