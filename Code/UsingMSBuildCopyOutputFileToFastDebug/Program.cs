#nullable enable
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
                return CommandLine.Parse(args).AddHandler<CleanOptions>(c =>
                    {
                        Logger.Message($"Enter CleanOptions");
                        CleanFile(c);
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
#if DEBUG
                Logger.Error(e.ToString());
#else
                var msBuildException = new MSBuildException(e.Message, e);
                msBuildException.ReportBuildError();
#endif
                return -1;
            }
        }

        private static void CleanFile(CleanOptions cleanOptions)
        {
            try
            {
                var cleanFileList = File.ReadAllLines(cleanOptions.CleanFilePath);
                foreach (var file in cleanFileList)
                {
                    try
                    {
                        if (File.Exists(file))
                        {
                            File.Delete(file);
                            Logger.Message($"Deleted files: {file}");
                        }
                    }
                    catch 
                    {
                        // 删除失败忽略
                    }
                }

                File.Delete(cleanOptions.CleanFilePath);
            }
            catch
            {
                // 删除失败忽略
            }
        }

        private static void CopyOutputFile(CopyOutputFileOptions copyOutputFileOptions)
        {
            var launchMainProjectExecutableFile = GetLaunchMainProjectExecutablePath(copyOutputFileOptions);
            if (launchMainProjectExecutableFile == null)
            {
                Logger.Message($"[UsingMSBuildCopyOutputFileToFastDebug] LaunchMainProjectExecutableFile is null. 没有啥需要做的");
                return;
            }

            Logger.Message($"LaunchMainProjectExecutablePath={launchMainProjectExecutableFile}");
            var destinationFolder = launchMainProjectExecutableFile.Directory;
            if (TargetFrameworkChecker.CheckCanCopy(launchMainProjectExecutableFile, copyOutputFileOptions) is false)
            {
#if DEBUG
                Logger.Message($"当前框架{copyOutputFileOptions.TargetFramework}与{launchMainProjectExecutableFile.FullName}不兼容");
#endif
                // 如果当前的框架是兼容的，那就进行拷贝，否则不做任何拷贝逻辑
                return;
            }

            var outputFileList = copyOutputFileOptions.GetOutputFileList();
            var safeOutputFileCopyTask = new SafeOutputFileCopyTask()
            {
                DestinationFolder = destinationFolder!.FullName,
                CleanFile = copyOutputFileOptions.CleanFilePath,
                SourceFiles = outputFileList.Select(t => t.FullName).ToArray()
            };
            safeOutputFileCopyTask.Execute();
        }

        /// <summary>
        /// 获取准备运行的 Exe 的路径
        /// </summary>
        /// <param name="copyOutputFileOptions"></param>
        /// <returns></returns>
        private static FileInfo? GetLaunchMainProjectExecutablePath(CopyOutputFileOptions copyOutputFileOptions)
        {
            var mainProjectPath = copyOutputFileOptions.MainProjectExecutablePath;
            // 如果用户有设置此文件夹，那就期望是输出到此文件夹
            if (!string.IsNullOrEmpty(mainProjectPath))
            {
                if (File.Exists(mainProjectPath) is false)
                {
                    // 这里不能扔出异常，考虑这个项目还是第一次构建，此时啥输出都应该是不存在的
                    // 更好的做法是在找不到目标文件的时候，给一个警告而已，毕竟此工具也只推荐在调试下使用而已。加个警告没啥锅
                    //throw new FileNotFoundException(
                    //    $"Can not find '{mainProjectPath}' FullPath={Path.GetFullPath(mainProjectPath)}");
                    Logger.Warning($"[UsingMSBuildCopyOutputFileToFastDebug] 找不到 MainProjectExecutablePath 的定义 '{mainProjectPath}' FullPath={Path.GetFullPath(mainProjectPath)} 文件。如项目首次构建，可忽略");
                    return null;
                }

                return new FileInfo(mainProjectPath);
            }

            // 尝试去读取 LaunchSettings 文件
            var launchSettingsParser = new LaunchSettingsParser();
            if (launchSettingsParser.Execute())
            {
                string launchMainProjectExecutablePath = launchSettingsParser.LaunchMainProjectExecutablePath!;
                // 获取到的 launchMainProjectPath 如果是相对路径，那么相对的是当前的输出文件的路径，而不是 csproj 的路径
                if (Path.IsPathRooted(launchMainProjectExecutablePath) is false)
                {
                    launchMainProjectExecutablePath =
                        Path.Combine(copyOutputFileOptions.GetOutputFileList().First().Directory!.FullName,
                            launchMainProjectExecutablePath!);

                    launchMainProjectExecutablePath = Path.GetFullPath(launchMainProjectExecutablePath);
                }

                if (File.Exists(launchMainProjectExecutablePath) is false)
                {
                    // 这里不能扔出异常，考虑这个项目还是第一次构建，此时啥输出都应该是不存在的       //throw new FileNotFoundException($"Can not find '{launchMainProjectExecutablePath}'");
                    Logger.Warning($"[UsingMSBuildCopyOutputFileToFastDebug] 找不到 LaunchSettings 的定义 '{launchMainProjectExecutablePath}' 文件。如项目首次构建，可忽略");

                    return null;
                }

                return new FileInfo(launchMainProjectExecutablePath!);
            }

            throw new ArgumentException($"没有从 MainProjectExecutablePath 和 LaunchSettings 获取到输出的文件夹");
        }

        private static IMSBuildLogger Logger { get; } = new MSBuildConsoleLogger();
    }


    [Verb("CopyOutputFile")]
    public class CopyOutputFileOptions
    {
        [Option("MainProjectExecutablePath")] 
        public string MainProjectExecutablePath { set; get; } = null!;

        [Option("CleanFilePath")] 
        public string CleanFilePath { set; get; } = null!;

        [Option("OutputFileToCopyList")] 
        public string OutputFileToCopyList { set; get; } = null!;

        [Option("TargetFramework")] 
        public string TargetFramework { set; get; } = null!;

        public List<FileInfo> GetOutputFileList()
        {
            var fileList = new List<FileInfo>();
            foreach (var file in OutputFileToCopyList.Split(System.IO.Path.PathSeparator))
            {
                // 不要优化 Linq 我需要调试这些文件，在这里加断点
                fileList.Add(new FileInfo(file));
            }

            return fileList;
        }
    }

    [Verb("Clean")]
    public class CleanOptions
    {
        [Option("CleanFilePath")] 
        public string CleanFilePath { set; get; } = null!;
    }
}