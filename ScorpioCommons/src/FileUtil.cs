using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace Scorpio.Commons {
    public static class FileUtil {
        //public static readonly byte[] BomBuffer = new byte[] { 0xef, 0xbb, 0xbf };
        public static readonly Encoding UTF8WithBom = new UTF8Encoding(true);
        public static readonly Encoding DefaultEncoding = new UTF8Encoding(false);
        /// <summary> 判断文件是否存在 </summary>
        public static bool FileExist(String file) {
            return file != null && file.Trim().Length != 0 && File.Exists(file);
        }
        /// <summary> 判断文件夹是否存在 </summary>
        public static bool PathExist(String path) {
            return path != null && path.Trim().Length != 0 && Directory.Exists(path);
        }
        public static bool CreateDirectoryByFile(string file) {
            return CreateDirectory(Path.GetDirectoryName(file));
        }
        public static bool CreateDirectory(string path) {
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
                return true;
            }
            return false;
        }
        public static string RemoveExtension(string file) {
            var index = file.LastIndexOf(".");
            return file.Substring(0, index);
        }
        public static string ChangeExtension(string file, string extension) {
            return Path.ChangeExtension(file, extension);
        }
        public static string GetFileExtension(string file) {
            var index = file.LastIndexOf(".");
            return file.Substring(index + 1);
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
        public static void DeleteFolder(string folder, string[] extensions, bool recursive) {
            if (!Directory.Exists(folder)) return;
            foreach (string file in GetFiles(folder, extensions, SearchOption.TopDirectoryOnly)) {
                File.Delete(file);
            }
            if (recursive) {
                foreach (string dir in Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly)) {
                    DeleteFolder(dir, extensions, recursive);
                }
            }
            if (Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly).Length > 0 || Directory.GetFiles(folder, "*", SearchOption.TopDirectoryOnly).Length > 0)
                return;
            Directory.Delete(folder);
        }
        /// <summary> 复制文件 </summary>
        public static void CopyFile(string source, string target, bool overwrite) {
            if (File.Exists(source)) {
                CreateDirectoryByFile(target);
                File.Copy(source, target, overwrite);
            }
        }
        /// <summary> 拷贝文件夹 </summary>
        public static void CopyFolder(string source, string target, string[] extensions, bool recursive) {
            source = Path.GetFullPath(source);
            target = Path.GetFullPath(target);
            if (!Directory.Exists(source)) return;
            if (!Directory.Exists(target)) Directory.CreateDirectory(target);
            foreach (var file in GetFiles(source, extensions, SearchOption.TopDirectoryOnly)) {
                File.Copy(file, Path.Combine(target, Path.GetFileName(file)), true);
            }
            if (recursive) {
                foreach (string folder in Directory.GetDirectories(source, "*", SearchOption.TopDirectoryOnly)) {
                    CopyFolder(folder, Path.Combine(target, Path.GetFileName(folder)), extensions, recursive);
                }
            }
        }
        /// <summary> 同步两个文件夹 </summary>
        public static void SyncFolder(string source, string target, string[] extensions, bool recursive) {
            source = Path.GetFullPath(source);
            target = Path.GetFullPath(target);
            if (!Directory.Exists(source)) return;
            if (!Directory.Exists(target)) { Directory.CreateDirectory(target); }
            var files = new HashSet<string>();
            foreach (var file in GetFiles(source, extensions, SearchOption.TopDirectoryOnly)) {
                files.Add(Path.GetFileName(file));
            }
            foreach (var file in GetFiles(target, extensions, SearchOption.TopDirectoryOnly)) {
                if (!files.Contains(Path.GetFileName(file))) {
                    File.Delete(file);
                }
            }
            foreach (var file in files) {
                var sourceFile = Path.Combine(source, file);
                var targetFile = Path.Combine(target, file);
                var sourceFileInfo = new FileInfo(sourceFile);
                var targetFileInfo = new FileInfo(targetFile);
                if (!targetFileInfo.Exists || sourceFileInfo.Length != targetFileInfo.Length || sourceFileInfo.LastWriteTime != targetFileInfo.LastWriteTime) {
                    File.Copy(sourceFile, targetFile, true);
                }
            }
            var dirs = new HashSet<string>();
            foreach (var dir in Directory.GetDirectories(source, "*", SearchOption.TopDirectoryOnly)) {
                dirs.Add(Path.GetFileName(dir));
            }
            foreach (var dir in Directory.GetDirectories(target, "*", SearchOption.TopDirectoryOnly)) {
                if (!dirs.Contains(Path.GetFileName(dir))) {
                    DeleteFolder(dir, null, true);
                }
            }
            foreach (var dir in dirs) {
                SyncFolder(Path.Combine(source, dir), Path.Combine(target, dir), extensions, recursive);
            }
        }
        /// <summary> 获取文件列表 </summary>
        public static IEnumerable<string> GetFiles(string path, string[] extensions, SearchOption searchOption) {
            if (extensions == null || extensions.Length == 0) {
                return Directory.GetFiles(path, "*", searchOption);
            } else {
                var files = new List<string>();
                foreach (var extension in extensions) {
                    files.AddRange(Directory.GetFiles(path, $"*.{extension}", searchOption));
                }
                return files;
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
