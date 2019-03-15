using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Scorpio.Commons {
    public static class FileUtil {
        //public static readonly byte[] BomBuffer = new byte[] { 0xef, 0xbb, 0xbf };
        public static readonly Encoding UTF8WithBom = new UTF8Encoding(true);
        public static readonly Encoding DefaultEncoding = new UTF8Encoding(false);
        /// <summary> 判断文件是否存在 </summary>
        public static bool FileExist(String file) {
            return !string.IsNullOrWhiteSpace(file) && File.Exists(file);
        }
        /// <summary> 判断文件夹是否存在 </summary>
        public static bool PathExist(String path) {
            return !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);
        }
        public static void CreateDirectoryByFile(string file) {
            CreateDirectory(Path.GetDirectoryName(file));
        }
        public static void CreateDirectory(string path) {
             if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
        public static string RemoveExtension(string file) {
            var index = file.LastIndexOf(".");
            return file.Substring(0, index);
        }
        public static string GetFileName(string path) {
            return Path.GetFileName(path);
        }
        public static string GetFileNameWithoutExtension(string path) {
            return Path.GetFileNameWithoutExtension(path);
        }

        /// <summary> 根据字符串创建文件 </summary>
        public static void CreateFile(string fileName, string buffer, string[] filePath) {
            CreateFile(fileName, buffer, filePath, DefaultEncoding);
        }
        /// <summary> 根据字符串创建文件 </summary>
        public static void CreateFile(string fileName, string buffer, string[] filePath, Encoding encoding) {
            if (filePath == null || filePath.Length < 0) return;
            foreach (var path in filePath) {
                CreateFile(path + "/" + fileName, buffer, encoding);
            }
        }
        /// <summary> 根据字符串创建一个文件 </summary>
        public static void CreateFile(string fileName, string buffer) {
            CreateFile(fileName, buffer, DefaultEncoding);
        }
        /// <summary> 根据字符串创建一个文件 </summary>
        public static void CreateFile(string fileName, string buffer, Encoding encoding) {
            CreateFile(fileName, encoding.GetBytes(buffer));
        }
        /// <summary> 根据byte[]创建文件 </summary>
        public static void CreateFile(string fileName, byte[] buffer, string[] filePath) {
            if (filePath == null || filePath.Length < 0) return;
            foreach (var path in filePath) {
                CreateFile(path + "/" + fileName, buffer);
            }
        }
        /// <summary> 根据byte[]创建一个文件 </summary>
        public static void CreateFile(string fileName, byte[] buffer) {
            if (string.IsNullOrEmpty(fileName)) return;
            CreateDirectoryByFile(fileName);
            DeleteFile(fileName);
            using (var fs = new FileStream(fileName, FileMode.Create)) {
                fs.Write(buffer, 0, buffer.Length);
                fs.Flush();
            }
        }
        /// <summary> 删除文件 </summary>
        public static void DeleteFile(string fileName) {
            if (File.Exists(fileName)) File.Delete(fileName);
        }
        /// <summary> 删除文件夹 </summary>
        public static void DeleteFiles(string sourceFolder, string strFilePattern, bool recursive) {
            if (!PathExist(sourceFolder)) return;
            var files = Directory.GetFiles(sourceFolder, strFilePattern);
            foreach (string file in files) {
                File.Delete(file);
            }
            if (recursive) {
                var folders = Directory.GetDirectories(sourceFolder);
                foreach (string folder in folders)
                    DeleteFiles(folder, strFilePattern, recursive);
            }
            if (Directory.GetDirectories(sourceFolder, "*", SearchOption.AllDirectories).Length > 0 || Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories).Length > 0)
                return;
            Directory.Delete(sourceFolder);
        }
        /// <summary> 复制文件 </summary>
        public static void CopyFile(string sourceFile, string destFile, bool overwrite) {
            if (FileExist(sourceFile)) {
                CreateDirectoryByFile(destFile);
                File.Copy(sourceFile, destFile, overwrite);
            }
        }
        /// <summary> 拷贝文件夹 </summary>
        public static void CopyFolder(string sourceFolder, string destFolder, string strFilePattern) {
            if (!Directory.Exists(sourceFolder)) return;
            if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);
            var files = Directory.GetFiles(sourceFolder, strFilePattern, SearchOption.TopDirectoryOnly);
            foreach (var file in files) {
                var name = Path.GetFileName(file);
                var dest = Path.Combine(destFolder, name);
                File.Copy(file, dest, true);
            }
            var folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders) {
                var name = Path.GetFileName(folder);
                var dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest, strFilePattern);
            }
        }
        /// <summary> 获得文件字符串 </summary>
        public static string GetFileString(string fileName) {
            return GetFileString(fileName, DefaultEncoding);
        }
        /// <summary> 获得文件字符串 </summary>
        public static string GetFileString(string fileName, Encoding encoding) {
            var buffer = GetFileBuffer(fileName);
            if (buffer == null) return "";
            return encoding.GetString(buffer);
        }
        /// <summary> 获得文件byte[] </summary>
        public static byte[] GetFileBuffer(string fileName) {
            if (!FileExist(fileName)) return null;
            using (var fs = new FileStream(fileName, FileMode.Open)) {
                long length = fs.Length;
                byte[] buffer = new byte[length];
                fs.Read(buffer, 0, (int)length);
                return buffer;
            }
        }
    }
}
