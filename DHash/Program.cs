using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHash
{

    class Options
    {
        [Option('f', "folder", HelpText = "Folder to hash", Required = true)]
        public string Folder { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if(!Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine("Bad args!");
                return;
            }

            if(! (options.Folder.EndsWith("\\") || options.Folder.EndsWith("/")))
            {
                options.Folder += "\\";
            }
            var folderParts = options.Folder.Split(Path.DirectorySeparatorChar);
            folderParts = folderParts.SelectMany(x => x.Split(Path.AltDirectorySeparatorChar)).Where(x=>!string.IsNullOrEmpty(x)).ToArray();

            DirectoryContents folder = new DirectoryContents(options.Folder);
            var exedir = AppDomain.CurrentDomain.BaseDirectory;
            var targetFolderName = folderParts.Last();
            var outFilePath = Path.Combine(exedir, targetFolderName) + ".xml";
            var progress = new Progress<ProgressData>();
            progress.ProgressChanged += (s, e) =>
            {
                var pct = (e.Current / e.Total) * 100;
                Console.Write("                        ");
                Console.Write("{0}%\r", pct);
            };

            folder.LoadContents(progress);

            using (var fs = File.Create(outFilePath))
                folder.ToFile(fs);

        }
    }
}
