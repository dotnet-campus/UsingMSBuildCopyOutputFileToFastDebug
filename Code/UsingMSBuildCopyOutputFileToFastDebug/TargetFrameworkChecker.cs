﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace UsingMSBuildCopyOutputFileToFastDebug
{
    public static class TargetFrameworkChecker
    {
        public static bool CheckCanCopy(FileInfo targetExecutableFile, CopyOutputFileOptions copyOutputFileOptions)
        {
            DotNetType targetFramework = GetTargetFrameworkDotNetType(copyOutputFileOptions.TargetFramework);

            var exeDotNetType = GetExecutableFileDotNetType(targetExecutableFile);

            return IsCompatible(exeDotNetType, targetFramework);
        }

        /// <summary>
        /// 判断 a 是否兼容 b 版本
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static bool IsCompatible(DotNetType a, DotNetType b)
        {
            if (a.HasFlag(DotNetType.NetCore) && b.HasFlag(DotNetType.NetCore))
            {
                return true;
            }

            if (a.HasFlag(DotNetType.NetFramework) && b.HasFlag(DotNetType.NetFramework))
            {
                return true;
            }

            return false;
        }

        private static DotNetType GetExecutableFileDotNetType(FileInfo targetExecutableFile)
        {
            // 先兼容判断，当前业务逻辑没有那么复杂，毕竟写了周六日了怕两周都写不完
            var name = Path.GetFileNameWithoutExtension(targetExecutableFile.FullName);
            // 如果是存在 runtimeconfig.json 和 deps.json 文件，那就是 .NET Core 的
            var runtimeConfigFile = Path.Combine(targetExecutableFile.DirectoryName!, $"{name}.runtimeconfig.json");
            var depsFile = Path.Combine(targetExecutableFile.DirectoryName!, $"{name}.deps.json");

            if (File.Exists(runtimeConfigFile) || File.Exists(depsFile))
            {
                return DotNetType.NetCore;
            }

            // 如果存在 config 文件，那就是 .NET Framework 的版本了
            return TryGetDotNetFrameworkVersion(targetExecutableFile)??DotNetType.NetFramework;
        }

        private static DotNetType? TryGetDotNetFrameworkVersion(FileInfo targetExecutableFile)
        {
            var configFile = targetExecutableFile.FullName + ".config";
            if (!File.Exists(configFile))
            {
                return null;
            }

            var xDocument = XDocument.Load(configFile);
            var element = xDocument.XPathSelectElement("/configuration/startup/supportedRuntime");
            if (element == null)
            {
                return null;
            }

            var sku = element.Attribute("sku");
            var version = sku?.Value;
            var match = Regex.Match(version ?? string.Empty, @"\.NETFramework,Version=v(\S+)");
            if (!match.Success)
            {
                return null;
            }

            var dotnet = match.Groups[1].Value;
            if (dotnet.Contains("4.0"))
            {
                return DotNetType.NetFramework40;
            }
            if (dotnet.Contains("4.5"))
            {
                return DotNetType.NetFramework45;
            }
            if (dotnet.Contains("4.6"))
            {
                return DotNetType.NetFramework46;
            }
            if (dotnet.Contains("4.7"))
            {
                return DotNetType.NetFramework47;
            }
            if (dotnet.Contains("4.8"))
            {
                return DotNetType.NetFramework48;
            }

            return null;
        }

        private static DotNetType GetTargetFrameworkDotNetType(string targetFramework)
        {
            if (targetFramework.Contains("net40"))
            {
                return DotNetType.NetFramework40;
            }

            if (targetFramework.Contains("net45"))
            {
                return DotNetType.NetFramework45;
            }

            if (targetFramework.Contains("net46"))
            {
                return DotNetType.NetFramework46;
            }

            if (targetFramework.Contains("net47"))
            {
                return DotNetType.NetFramework47;
            }

            if (targetFramework.Contains("net48"))
            {
                return DotNetType.NetFramework48;
            }

            if (targetFramework.Contains("netcoreapp1"))
            {
                return DotNetType.NetCore1;
            }

            if (targetFramework.Contains("netcoreapp2"))
            {
                return DotNetType.NetCore2;
            }

            if (targetFramework.Contains("netcoreapp3"))
            {
                return DotNetType.NetCore3;
            }

            if (targetFramework.Contains("net5."))
            {
                return DotNetType.Net5;
            }

            if (targetFramework.Contains("net6."))
            {
                return DotNetType.Net6;
            }

            if (targetFramework.Contains("net7."))
            {
                return DotNetType.Net7;
            }

            if (targetFramework.Contains("net8."))
            {
                return DotNetType.Net8;
            }

            if (targetFramework.Contains("net9."))
            {
                return DotNetType.Net9;
            }

            if (Regex.IsMatch(targetFramework, @"net\d"))
            {
                return DotNetType.NetCore;
            }

            throw new ArgumentException($"Unknown TargetFramework {targetFramework}");
        }
    }

    [Flags]
    public enum DotNetType
    {
        NetFramework = 1 << 25,
        NetCore = 1 << 26,
        // 还没有需求，就不写了
        NetStandard = 1 << 27,

        NetFramework40 = 1 << 0 | NetFramework,
        NetFramework45 = 1 << 1 | NetFramework,
        // 对于 4.5.1 等，都归为 45 好了
        //NetFramework451 = 1 << 2 | NetFramework,
        //NetFramework452 = 1 << 3 | NetFramework,
        NetFramework46 = 1 << 4 | NetFramework,
        NetFramework47 = 1 << 5 | NetFramework,
        NetFramework48 = 1 << 6 | NetFramework,

        NetCore1 = 1 << 11 | NetCore,
        NetCore2 = 1 << 12 | NetCore,
        NetCore3 = 1 << 13 | NetCore,

        Net5 = 1 << 15 | NetCore,
        Net6 = 1 << 16 | NetCore,

        Net7 = 1 << 17 | NetCore,
        Net8 = 1 << 18 | NetCore,
        Net9 = 1 << 19 | NetCore,
    }
}