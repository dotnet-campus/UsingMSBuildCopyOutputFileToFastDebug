using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UsingMSBuildCopyOutputFileToFastDebug
{
    public class SafeOutputFileCopyTask : Microsoft.Build.Utilities.Task
    {
        public string[] SourceFiles { set; get; }
        public string DestinationFolder { set; get; }

        /// <summary>
        /// 用于清理文件
        /// </summary>
        public string CleanFile { set; get; }

        public override bool Execute()
        {
            if (SourceFiles == null || !SourceFiles.Any())
            {
                Console.WriteLine("warning: 没有传入需要复制的文件，请给 SourceFiles 赋值需要赋值的文件列表");
                return true;
            }

            var str = new System.Text.StringBuilder();

            str.Append("当前传入需要复制的文件\r\n");
            foreach (var sourceFile in SourceFiles)
            {
                str.Append(sourceFile);
                str.Append("\r\n"); // AppendLine 干什么去了
            }

            str.Append("将要复制到的文件夹为");
            str.Append(DestinationFolder);
            str.Append("\r\n");

            Console.WriteLine(str.ToString());

            foreach (var sourceFile in SourceFiles.Select(sourceFile => new FileInfo(sourceFile)))
            {
                var destinationFile = Path.Combine(DestinationFolder, sourceFile.Name);

                if (File.Exists(destinationFile))
                {
                    Console.WriteLine("发现需要复制的文件已经存在");

                    var sourceFileName = Path.GetFileNameWithoutExtension(sourceFile.FullName);

                    Console.WriteLine("开始移动文件");

                    for (int i = 0; i < 65535; i++)
                    {
                        var newFileName = Path.Combine(DestinationFolder,
                            $"{sourceFileName}{i + 1}{sourceFile.Extension}.bak");
                        if (!File.Exists(newFileName))
                        {
                            File.Move(destinationFile, newFileName);
                            Console.WriteLine($"移动文件完成，将{destinationFile}移动到{newFileName}");
                            AddToClean(newFileName);
                            break;
                        }
                    }

                }

                Console.WriteLine("开始复制文件 " + destinationFile);

                File.Copy(sourceFile.FullName, destinationFile);

                Console.WriteLine("完成复制文件 " + destinationFile);
            }

            Console.WriteLine("全部复制完成");

            //Tracer = str.ToString();
            return true;
        }

        private async void AddToClean(string newFileName)
        {
            // 加入到清理文件

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    newFileName = Path.GetFullPath(newFileName);
                    File.AppendAllLines(CleanFile, new[] { newFileName });

                    return;
                }
                catch (Exception)
                {
                    // 忽略
                }

                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }
        }
    }
}