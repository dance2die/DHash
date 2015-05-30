using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml;

namespace DHash
{
    class DirectoryContents
    {
        public string Folder { get; private set; }

        public DirectoryContents(string folder)
        {
            Folder = folder;
        }

        public void ToFile(FileStream fs)
        {
            using (var writer = XmlWriter.Create(fs))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<string, string>));
                serializer.WriteObject(writer, PathToHash);
            }
        }

        Dictionary<string, string> PathToHash = new Dictionary<string, string>();
        HashSet<string> AllHashes = new HashSet<string>();


        public void LoadContents(IProgress<ProgressData> progress)
        {
            PathToHash.Clear();
            AllHashes.Clear();
                        
            var files = GetAllFiles(Folder);
            var progdata = new ProgressData();
            progdata.Total = files.Count();
            foreach (var file in files)
            {
                progdata.Current++;
                progdata.Message = "Hashing: " + file;
                progress.Report(progdata);
                try {
                    var hash = HashFile(file);
                    PathToHash.Add(file, hash);
                    AllHashes.Add(hash);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Failed to hash {0}: {1}", file, ex.Message);
                }
            }
        }

        IEnumerable<string> GetAllFiles(string folder)
        {
            return new FileSystemEnumerable(new DirectoryInfo(folder), "*", SearchOption.AllDirectories).Select(x=>x.FullName);
        }

        string HashFile(string file)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-","").ToLower();
                }
            }
        }
    }
}
