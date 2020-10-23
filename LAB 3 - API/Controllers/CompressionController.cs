using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LAB_3___API.Controllers
{
    [Route("api")]
    [ApiController]
    public class CompressionController : ControllerBase
    {
        private IWebHostEnvironment environment;
        public CompressionController(IWebHostEnvironment env)
        {
            environment = env;
        }

        [HttpGet]
        public string Index()
        {
            string text = "\t\t\t- LAB 4 -\n\nKevin Romero 1047519\nJosé De León 1072619";
            return text;
        }

        [HttpPost("compress/{name}")]
        public ActionResult CompressFile([FromForm] IFormFile file, string name)
        {
            try
            {
                string file_path = environment.ContentRootPath + $"\\Data\\temporal\\{name}.txt";
                string fileName = file.FileName;
                string file_compressedpath = environment.ContentRootPath + $"\\Data\\compressions\\{name}.lzw";
                FileManage _file = new FileManage() { OriginalFileName = file.FileName, CompressedFileName = name + ".lzw", CompressedFilePath = file_compressedpath, DateOfCompression = Convert.ToDateTime(DateTime.Now.ToShortTimeString()) };

                //Save the file in the server
                _file.SaveFile(file, file_path);

                //Compress the file previously saved
                _file.CompressFile(file_path);

                //Write on log file the compression result
                _file.WriteCompression(_file);

                //Delete the original file 
                _file.DeleteFile(file_path);

                FileStream result = new FileStream(_file.CompressedFilePath, FileMode.Open);
                return File(result, "text/plain", _file.CompressedFileName);
                
            }
            catch (Exception)
            {

                return StatusCode(500);
            }           
        }

        [HttpPost("decompress")]
        public ActionResult DecompressFile([FromForm] IFormFile file)
        {
            try
            {
                string file_path = environment.ContentRootPath + $"\\Data\\temporal\\{file.FileName}";
                string output_file_path = environment.ContentRootPath + $"\\Data\\compressions\\{file.FileName}";

                FileManage _file = new FileManage();

                //Save the file in the server
                _file.SaveFile(file, file_path);

                //Get the original file name
                string file_name = _file.GetOriginalName(environment.ContentRootPath, file.FileName);

                //Decompress the file previosly saved
                _file.DecompressFile(file_path, file_name);


                //Delete the original file 
                _file.DeleteFile(file_path);

                string path = environment.ContentRootPath + $"\\Data\\decompressions\\{file_name}";
                FileStream result = new FileStream(path, FileMode.Open);
                return File(result, "text/plain", file_name);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }        
        }

        [HttpGet("compressions")]
        public IEnumerable<FileManage> GetCompressions()
        {
            try
            {
                string path = environment.ContentRootPath+ "\\Data\\compressions_history.json";
                FileManage fm = new FileManage();
                return fm.GetAllCompressions(path);           
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
