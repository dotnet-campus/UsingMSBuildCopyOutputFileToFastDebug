using System;
using dotnetCampus.Cli;
using dotnetCampus.MSBuildUtils;

namespace UsingMSBuildCopyOutputFileToFastDebug
{
    public class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            Logger.Message($"UsingMSBuildCopyOutputFileToFastDebug {Environment.CommandLine}");
#endif

            CommandLine.Parse(args).AddHandler<CleanOptions>(c =>
            {
                Logger.Message($"Enter CleanOptions");
            })
                .AddHandler<CopyOutputFileOptions>(c =>
                {
                    Logger.Message($"Enter CopyOutputFileOptions");
                });
        }

        private static IMSBuildLogger Logger { get; } = new MSBuildConsoleLogger();
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