using System;
using System.IO;
using System.Linq;
using dotnetCampus.Cli;
using dotnetCampus.MSBuildUtils;

namespace UsingMSBuildCopyOutputFileToFastDebug
{
    public class Program
    {
        static int Main(string[] args)
        {
#if DEBUG
            Logger.Message($"UsingMSBuildCopyOutputFileToFastDebug {Environment.CommandLine}");

            for (var i = 0; i < args.Length; i++)
            {
                Logger.Message($"Args[{i}]={args[i]}");
            }
#endif

            if (args[0] == "--")
            {
                // 为了兼容旧版本，依然使用如下格式
                // dotnet foo.dll -- --a 1 --b ab
                // 在 .NET 6 将会让 -- 作为第一个参数，因此需要去掉
                var list = args.ToList();
                list.RemoveAt(0);
                args = list.ToArray();
                // 还好上面代码不在乎性能，让我试试这个逗比方法
            }

            try
            {
                return CommandLine.Parse(args).AddHandler<CleanOptions>(c =>
                     {
                         Logger.Message($"Enter CleanOptions");
                     })
                     .AddHandler<CopyOutputFileOptions>(c =>
                     {
                         Logger.Message($"Enter CopyOutputFileOptions");
                         CopyOutputFile(c);
                     })
                     .Run();
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                return -1;
            }
        }

        private static void CopyOutputFile(CopyOutputFileOptions copyOutputFileOptions)
        {

        }

        /// <summary>
        /// 获取准备输出的文件夹
        /// </summary>
        /// <param name="copyOutputFileOptions"></param>
        /// <returns></returns>
        private static DirectoryInfo GetTargetFolder(CopyOutputFileOptions copyOutputFileOptions)
        {
            var mainProjectPath = copyOutputFileOptions.MainProjectPath;
            // 如果用户有设置此文件夹，那就期望是输出到此文件夹
            if (!string.IsNullOrEmpty(mainProjectPath))
            {
                return new DirectoryInfo(mainProjectPath);
            }

            return null;
        }

        private static IMSBuildLogger Logger { get; } = new MSBuildConsoleLogger();
    }


    public static class TargetFrameworkChecker
    {

    }

    [Verb("CopyOutputFile")]
    public class CopyOutputFileOptions
    {
        [Option("MainProjectPath")]
        public string MainProjectPath { set; get; } = null!;

        [Option("CleanFilePath")]
        public string CleanFilePath { set; get; }

        [Option("OutputFileToCopyList")]
        public string OutputFileToCopyList { set; get; }

        [Option("TargetFramework")]
        public string TargetFramework { set; get; }
    }

    [Verb("Clean")]
    public class CleanOptions
    {
        [Option("CleanFilePath")]
        public string CleanFilePath { set; get; }
    }
}