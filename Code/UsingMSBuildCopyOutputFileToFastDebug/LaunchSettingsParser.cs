using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace UsingMSBuildCopyOutputFileToFastDebug
{
    public class LaunchSettingsParser
    {
        public bool Execute()
        {
            var file = Path.Combine("Properties", "launchSettings.json");
            Console.WriteLine($"开始从 launchSettings（{file}） 文件读取输出文件夹");
            if (!File.Exists(file))
            {
                Console.WriteLine($"找不到{file}文件，读取结束");
                return false;
            }

            var text = File.ReadAllText(file);

            var launchSettings = JsonConvert.DeserializeObject<LaunchSettings>(text);
            if (launchSettings == null)
            {
                return false;
            }

            foreach (var launchSettingsProfile in launchSettings.Profiles)
            {
                var launchProfile = launchSettingsProfile.Value;
                if (launchProfile.CommandName == "Executable")
                {
                    var executablePath = launchProfile.ExecutablePath;
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

    [DataContract]
    public class LaunchSettings
    {
        [DataMember(Name = "profiles")]
        public IDictionary<string, LaunchProfile> Profiles { get; set; }
    }

    [DataContract]
    public class LaunchProfile
    {
        [DataMember(Name = "commandName")]
        public string CommandName { get; set; }

        [DataMember(Name = "executablePath")]
        public string ExecutablePath { get; set; }

        [DataMember(Name = "commandLineArgs")]
        public string CommandLineArgs { get; set; }

        [DataMember(Name = "nativeDebugging")]
        public bool NativeDebugging { get; set; }
    }
}