using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using LAB_3___Compressor.Huffman_Coding;
using LAB_3___Compressor.LZW_Commpression;
using System.Text.Json;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;

namespace LAB_3___API
{
    public class FileManage
    {
        LZW lzw = new LZW();
        public string OriginalFileName { get; set; }
        public string CompressedFileName { get; set; }
        public string CompressedFilePath { get; set; }
        public double CompressionRatio { get; set; }
        public double CompressionFactor { get; set; }
        public double ReductionPercentage { get; set; }
        public DateTime DateOfCompression { get; set; }

        public List<FileManage> GetAllCompressions(string path)
        {
            List<FileManage> list = new List<FileManage>();
            using (StreamReader sr = new StreamReader(path))
            {
                string result = sr.ReadToEnd();

                JsonSerializerOptions name_rule = new JsonSerializerOptions {IgnoreNullValues = true };
                list = JsonSerializer.Deserialize<List<FileManage>>(result, name_rule);
            }
            return list;
        }

        public void SaveFile(IFormFile file, string output_path)
        {
            if (File.Exists(output_path))
            {
                File.Delete(output_path);
            }
            using (var fs = new FileStream(output_path, FileMode.OpenOrCreate))
            {
                
                file.CopyTo(fs);
            }
            return;
        }


        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public void CompressFile(string path)
        {
            LZW lZW = new LZW();
            byte[] buffer;
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                buffer = new byte[fs.Length];
                using (var br = new BinaryReader(fs))
                {
                    br.Read(buffer,0,(int)fs.Length);
                }
            }

            byte[] result = lzw.EncodeData(buffer);

            for (int i = 1; File.Exists(CompressedFilePath) ; i++)
            {
                var split = CompressedFileName.Split(".lzw");
                if (split[0].Contains("("))
                {
                    var split2 = split[0].Split("(");
                    CompressedFileName = split2[0] + "(" + i + ")" + ".lzw";
                }
                else
                CompressedFileName = split[0] + "(" + i + ")" + ".lzw";

                split = CompressedFilePath.Split("compressions");
                CompressedFilePath = split[0] + "compressions\\" + CompressedFileName;
            }

            using (var fs = new FileStream(CompressedFilePath, FileMode.OpenOrCreate))
            {
                fs.Write(result, 0, result.Length);
            }
            CompressionRatio = lzw.CompressionRatio();
            CompressionFactor = lzw.CompressionFactor();
            ReductionPercentage = lzw.ReductionPercentage();
        }

        public void DecompressFile(string path, string file_name)
        {
            LZW lzw = new LZW();
            byte[] buffer;


            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                buffer = new byte[fs.Length];
                using (var br = new BinaryReader(fs))
                {
                    br.Read(buffer, 0, buffer.Length);
                }
            }
            lzw = new LZW();
            byte[] result = lzw.DecodeData(buffer);

            string[] path_result = path.Split("Data");
            string file_path = path_result[0] + $"\\Data\\decompressions\\{file_name}";
            using (var fs = new FileStream(file_path, FileMode.OpenOrCreate))
            {
                fs.Write(result, 0, result.Length);
            }
        }

        public void WriteCompression(FileManage file)
        {
            string[] path = CompressedFilePath.Split("Data");
            string file_path = path[0] + "\\Data\\compressions_history.json";

            List<FileManage> list;
            string json_text = "";
            using(FileStream fs = new FileStream(file_path, FileMode.OpenOrCreate))
            {
              using(StreamReader sr = new StreamReader(fs))
                {
                    json_text = sr.ReadToEnd();
                }                                                   
            }
            if (json_text == "")
            {
                using (StreamWriter sw = new StreamWriter(file_path, false, Encoding.UTF8))
                {
                    string file_stats = JsonSerializer.Serialize(file);
                    sw.Write("[" + file_stats + "]");
                    return;
                }               
            }
            else
            {
                JsonSerializerOptions name_rule = new JsonSerializerOptions {IgnoreNullValues = true };
                list = JsonSerializer.Deserialize<List<FileManage>>(json_text, name_rule);
                list.Add(file);
            }
            using (StreamWriter sw = new StreamWriter(file_path, false, Encoding.UTF8))
            {
                string file_stats = JsonSerializer.Serialize(list);
                sw.Write(file_stats);
            }
        }


        public string GetOriginalName(string path_root ,string Compressed_FileName)
        {

            string file_path = path_root + "\\Data\\compressions_history.json";

            string json_text = "";
            using (FileStream fs = new FileStream(file_path, FileMode.OpenOrCreate))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    json_text = sr.ReadToEnd();
                }
            }
            JsonSerializerOptions name_rule = new JsonSerializerOptions { IgnoreNullValues = true };
            List<FileManage> list = JsonSerializer.Deserialize<List<FileManage>>(json_text, name_rule);

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].CompressedFileName == Compressed_FileName) return list[i].OriginalFileName;
            }
            return null;

        }
    }
}
