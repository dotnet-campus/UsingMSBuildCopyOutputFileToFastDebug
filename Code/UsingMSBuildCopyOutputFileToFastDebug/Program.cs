using System;
using System.Collections.Generic;
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
                return CommandLine.Parse(args).AddHandler<CleanOptions>(c => { Logger.Message($"Enter CleanOptions"); })
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
            var destinationFolder = GetDestinationFolder(copyOutputFileOptions);
            if (TargetFrameworkChecker.CheckCanCopy(destinationFolder, copyOutputFileOptions) is false)
            {
                // 如果当前的框架是兼容的，那就进行拷贝，否则不做任何拷贝逻辑
                return;
            }

            var outputFileList = GetOutputFileList(copyOutputFileOptions.OutputFileToCopyList);
            var safeOutputFileCopyTask = new SafeOutputFileCopyTask()
            {
                DestinationFolder = destinationFolder.FullName,
                CleanFile = copyOutputFileOptions.CleanFilePath,
                SourceFiles = outputFileList.Select(t => t.FullName).ToArray()
            };
            safeOutputFileCopyTask.Execute();
        }

        private static List<FileInfo> GetOutputFileList(string outputFileToCopyList)
        {
            var fileList = new List<FileInfo>();
            foreach (var file in outputFileToCopyList.Split(System.IO.Path.PathSeparator))
            {
                // 不要优化 Linq 我需要调试这些文件，在这里加断点
                fileList.Add(new FileInfo(file));
            }

            return fileList;
        }

        /// <summary>
        /// 获取准备输出的文件夹
        /// </summary>
        /// <param name="copyOutputFileOptions"></param>
        /// <returns></returns>
        private static DirectoryInfo GetDestinationFolder(CopyOutputFileOptions copyOutputFileOptions)
        {
            var mainProjectPath = copyOutputFileOptions.MainProjectPath;
            // 如果用户有设置此文件夹，那就期望是输出到此文件夹
            if (!string.IsNullOrEmpty(mainProjectPath))
            {
                if (Directory.Exists(mainProjectPath) is false)
                {
                    throw new DirectoryNotFoundException(
                        $"Can not find '{mainProjectPath}' FullPath={Path.GetFullPath(mainProjectPath)}");
                }

                return new DirectoryInfo(mainProjectPath);
            }

            // 尝试去读取 LaunchSettings 文件
            var launchSettingsParser = new LaunchSettingsParser();
            if (launchSettingsParser.Execute())
            {
                var launchMainProjectPath = launchSettingsParser.LaunchMainProjectPath;
                if (Directory.Exists(launchMainProjectPath) is false)
                {
                    throw new DirectoryNotFoundException($"Can not find '{launchMainProjectPath}'");
                }

                return new DirectoryInfo(launchMainProjectPath);
            }

            throw new ArgumentException($"没有从 MainProjectPath 和 LaunchSettings 获取到输出的文件夹");
        }

        private static IMSBuildLogger Logger { get; } = new MSBuildConsoleLogger();
    }


    public static class TargetFrameworkChecker
    {
        public static bool CheckCanCopy(DirectoryInfo targetFolder, CopyOutputFileOptions copyOutputFileOptions)
        {
            return true;
        }
    }

    [Verb("CopyOutputFile")]
    public class CopyOutputFileOptions
    {
        [Option("MainProjectPath")] public string MainProjectPath { set; get; } = null!;

        [Option("CleanFilePath")] public string CleanFilePath { set; get; }

        [Option("OutputFileToCopyList")] public string OutputFileToCopyList { set; get; }

        [Option("TargetFramework")] public string TargetFramework { set; get; }
    }

    [Verb("Clean")]
    public class CleanOptions
    {
        [Option("CleanFilePath")] public string CleanFilePath { set; get; }
    }
}