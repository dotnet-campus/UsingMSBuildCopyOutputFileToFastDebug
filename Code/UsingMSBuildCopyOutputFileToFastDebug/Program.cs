﻿using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UsingMSBuildCopyOutputFileToFastDebug
{
    class Program
    {
        static void Main(string[] args)
        {

        }
    }

    public class SafeOutputFileCopyTask : Microsoft.Build.Utilities.Task
    {
        public string[] SourceFiles { set; get; }
        public string DestinationFolder { set; get; }

        public override bool Execute()
        {
            if (SourceFiles == null || !SourceFiles.Any())
            {
                Console.WriteLine("warning: 用户没有传入需要复制的文件");
                return true;
            }

            var str = new System.Text.StringBuilder();

            str.Append("用户传入需要复制的文件\r\n");
            foreach (var sourceFile in SourceFiles)
            {
                str.Append(sourceFile);
                str.Append("\r\n"); // AppendLine 干什么去了
            }

            str.Append("用户将要复制的文件夹");
            str.Append(DestinationFolder);
            str.Append("\r\n");

            Console.WriteLine(str.ToString());

            foreach (var sourceFile in SourceFiles.Select(sourceFile => new FileInfo(sourceFile)))
            {
                var destinationFile = Path.Combine(DestinationFolder, sourceFile.Name);

                if (File.Exists(destinationFile))
                {
                    Console.WriteLine("发现需要复制的文件已经存在");

                    var sourceFileName = sourceFile.Name.Replace(sourceFile.Extension, "");

                    Console.WriteLine("开始移动文件");

                    for (int i = 0; i < 65535; i++)
                    {
                        var newFileName = Path.Combine(DestinationFolder,
                            $"{sourceFileName}{i + 1}{sourceFile.Extension}.bak");
                        if (!File.Exists(newFileName))
                        {
                            File.Move(destinationFile, newFileName);
                            break;
                        }
                    }

                    Console.WriteLine("移动文件完成");
                }

                Console.WriteLine("复制文件");

                File.Copy(sourceFile.FullName, destinationFile);

                Console.WriteLine("复制完成" + destinationFile);
            }

            //Tracer = str.ToString();
            return true;
        }
    }
}