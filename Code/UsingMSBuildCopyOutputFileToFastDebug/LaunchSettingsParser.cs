using System;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Xml;
using Lsj.Util.Collections;
using Lsj.Util.JSON;
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
            var file = Path.Combine("Properties", "launchSettings.json");
            Console.WriteLine("开始从 launchSettings 文件读取输出文件夹");
            Console.WriteLine($"开始读取{file}文件");
            if (!File.Exists(file))
            {
                Console.WriteLine($"找不到{file}文件，读取结束");
            }

            var text = File.ReadAllText(file);

            var o = JSONParser.Parse(text);
            var profiles = o.profiles;
            var data = profiles.data;
            if (data is SafeDictionary<string, object> d)
            {
                foreach (var keyValuePair in d)
                {
                    if (keyValuePair.Value is JSONObject jsonObject)
                    {
                        if (jsonObject.TryGetMember(new CustomGetMemberBinder("ExecutablePath", true),
                            out var filePath))
                        {
                            var path = filePath?.ToString();

                            if (!string.IsNullOrEmpty(path))
                            {
                                LaunchMainProjectPath = Path.GetDirectoryName(path);
                                Console.WriteLine($"读取到 {LaunchMainProjectPath} 文件夹");
                                return true;
                            }
                        }
                    }
                }
            }

            return true;
        }

        [Output]
        public string LaunchMainProjectPath { set; get; }
    }

    public class CustomGetMemberBinder : GetMemberBinder
    {
        /// <inheritdoc />
        public CustomGetMemberBinder(string name, bool ignoreCase) : base(name, ignoreCase)
        {
        }

        /// <inheritdoc />
        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            return null;
        }
    }
}