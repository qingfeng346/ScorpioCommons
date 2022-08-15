using System.Text;
using System.IO;
using System.Collections.Generic;

namespace Scorpio.Commons {
    public static class FileUtil {
        //public static readonly byte[] BomBuffer = new byte[] { 0xef, 0xbb, 0xbf };
        public static readonly Encoding UTF8WithBom = new UTF8Encoding(true);
        public static readonly Encoding DefaultEncoding = new UTF8Encoding(false);
        /// <summary> 首字母大写 </summary>
        public static string ToOneUpper(string str) {
            if (string.IsNullOrWhiteSpace(str)) return str;
            if (str.Length == 1) return str.ToUpper();
            return char.ToUpper(str[0]) + str.Substring(1);
        }
        /// <summary> 首字母小写 </summary>
        public static string ToOneLower(string str) {
            if (string.IsNullOrWhiteSpace(str)) return str;
            if (str.Length == 1) return str.ToLower();
            return char.ToLower(str[0]) + str.Substring(1);
        }
        /// <summary> 判断文件是否存在 </summary>
        public static bool FileExist(string file) {
            return !string.IsNullOrWhiteSpace(file) && File.Exists(file);
        }
        /// <summary> 判断文件夹是否存在 </summary>
        public static bool PathExist(string path) {
            return !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);
        }
        /// <summary> 根据文件创建路径 </summary>
        public static bool CreateDirectoryByFile(string file) {
            return CreateDirectory(Path.GetDirectoryName(file));
        }
        /// <summary> 创建路径 </summary>
        public static bool CreateDirectory(string path) {
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
                return true;
            }
            return false;
        }
        /// <summary> 删除后缀名 </summary>
        public static string RemoveExtension(string file) {
            var index = file.LastIndexOf(".");
            return file.Substring(0, index);
        }
        /// <summary> 修改后缀名 </summary>
        public static string ChangeExtension(string file, string extension) {
            return Path.ChangeExtension(file, extension);
        }
        /// <summary> 获取后缀名 </summary>
        public static string GetExtension(string file) {
            return Path.GetExtension(file);
        }
        /// <summary> 获取文件名 </summary>
        public static string GetFileName(string path) {
            return Path.GetFileName(path);
        }
        /// <summary> 获取不带后缀文件名 </summary>
        public static string GetFileNameWithoutExtension(string path) {
            return Path.GetFileNameWithoutExtension(path);
        }
        /// <summary> 获取文件相对路径 </summary>
        public static string GetRelativePath(string file, string path) {
            file = Path.GetFullPath(file);
            path = Path.GetFullPath(path);
            return file.Substring(path.Length + 1).Replace("\\", "/");
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
        /// <summary> 如果是空文件夹,则删除 </summary>
        public static bool DeleteFolderIfEmpty(string folder) {
            if (!Directory.Exists(folder)) {
                return false;
            }
            if (Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly).Length > 0 || Directory.GetFiles(folder, "*", SearchOption.TopDirectoryOnly).Length > 0) {
                return false;
            }
            Directory.Delete(folder);
            return true;
        }
        /// <summary> 删除空文件夹 </summary>
        public static void DeleteEmptyFolder(string folder, bool recursive) {
            if (!Directory.Exists(folder)) { return; }
            var dirs = Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly);
            foreach (var dir in dirs) {
                if (recursive) {
                    DeleteEmptyFolder(dir, recursive);
                }
                DeleteFolderIfEmpty(dir);
            }
            DeleteFolderIfEmpty(folder);
        }
        /// <summary> 复制文件 </summary>
        public static void CopyFile(string source, string target, bool overwrite) {
            if (File.Exists(source)) {
                CreateDirectoryByFile(target);
                File.Copy(source, target, overwrite);
            }
        }
        /// <summary> 移动文件 </summary>
        public static void MoveFile(string source, string target, bool overwrite) {
            if (FileExist(source)) {
                CreateDirectoryByFile(target);
                if (overwrite) DeleteFile(target);
                File.Move(source, target);
            }
        }
        /// <summary> 删除文件夹 </summary>
        public static void DeleteFiles(string folder, string searchPattern, bool recursive) {
            DeleteFolder(folder, searchPattern == null ? null : new[] { searchPattern }, recursive);
        }
        /// <summary> 删除文件夹 </summary>
        public static void DeleteFolder(string folder, string[] searchPatterns, bool recursive) {
            if (!Directory.Exists(folder)) return;
            foreach (string file in GetFiles(folder, searchPatterns, SearchOption.TopDirectoryOnly)) {
                File.Delete(file);
            }
            if (recursive) {
                foreach (string dir in Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly)) {
                    DeleteFolder(dir, searchPatterns, recursive);
                }
            }
            DeleteFolderIfEmpty(folder);
        }
        /// <summary> 拷贝文件夹 </summary>
        public static void CopyFiles(string source, string target, string searchPattern, bool recursive) {
            CopyFolder(source, target, searchPattern == null ? null : new[] { searchPattern }, recursive);
        }
        /// <summary> 拷贝文件夹 </summary>
        public static void CopyFolder(string source, string target, string[] searchPatterns, bool recursive) {
            source = Path.GetFullPath(source);
            target = Path.GetFullPath(target);
            if (!Directory.Exists(source)) return;
            if (!Directory.Exists(target)) Directory.CreateDirectory(target);
            foreach (var file in GetFiles(source, searchPatterns, SearchOption.TopDirectoryOnly)) {
                File.Copy(file, Path.Combine(target, Path.GetFileName(file)), true);
            }
            if (recursive) {
                foreach (string folder in Directory.GetDirectories(source, "*", SearchOption.TopDirectoryOnly)) {
                    CopyFolder(folder, Path.Combine(target, Path.GetFileName(folder)), searchPatterns, recursive);
                }
            }
        }
        /// <summary> 移动文件夹 </summary>
        public static void MoveFiles(string source, string target, string searchPattern, bool recursive, bool overwrite) {
            MoveFolder(source, target, searchPattern == null ? null : new[] { searchPattern }, recursive, overwrite);
        }
        /// <summary> 移动文件夹 </summary>
        public static void MoveFolder(string source, string target, string[] searchPatterns, bool recursive, bool overwrite) {
            if (!Directory.Exists(source)) return;
            if (!Directory.Exists(target)) Directory.CreateDirectory(target);
            foreach (string file in GetFiles(source, searchPatterns, SearchOption.TopDirectoryOnly)) {
                MoveFile(file, Path.Combine(target, Path.GetFileName(file)), overwrite);
            }
            if (recursive) {
                foreach (string folder in Directory.GetDirectories(source, "*", SearchOption.TopDirectoryOnly)) {
                    MoveFolder(folder, Path.Combine(target, Path.GetFileName(folder)), searchPatterns, recursive, overwrite);
                }
            }
            DeleteFolderIfEmpty(source);
        }
        /// <summary> 同步文件夹 </summary>
        public static void SyncFiles(string source, string target, string searchPattern, bool recursive) {
            SyncFolder(source, target, searchPattern == null ? null : new[] { searchPattern }, recursive);
        }
        /// <summary> 同步文件夹 </summary>
        public static void SyncFolder(string source, string target, string[] searchPatterns, bool recursive) {
            source = Path.GetFullPath(source);
            target = Path.GetFullPath(target);
            if (!Directory.Exists(source)) return;
            if (!Directory.Exists(target)) { Directory.CreateDirectory(target); }
            var files = new HashSet<string>();
            foreach (var file in GetFiles(source, searchPatterns, SearchOption.TopDirectoryOnly)) {
                files.Add(Path.GetFileName(file));
            }
            foreach (var file in GetFiles(target, searchPatterns, SearchOption.TopDirectoryOnly)) {
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
            if (recursive) {
                foreach (var dir in dirs) {
                    SyncFolder(Path.Combine(source, dir), Path.Combine(target, dir), searchPatterns, recursive);
                }
            }
        }
        /// <summary> 获取文件列表 </summary>
        public static List<string> GetFiles(string path, string searchPattern, SearchOption searchOption) {
            return GetFiles(path, searchPattern == null ? null : new[] { searchPattern }, searchOption);
        }
        /// <summary> 获取文件列表 </summary>
        public static List<string> GetFiles(string path, string[] searchPatterns, SearchOption searchOption) {
            if (searchPatterns == null || searchPatterns.Length == 0) {
                return new List<string>(Directory.GetFiles(path, "*", searchOption));
            } else {
                var files = new List<string>();
                foreach (var searchPattern in searchPatterns) {
                    files.AddRange(Directory.GetFiles(path, searchPattern, searchOption));
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
        /// <summary> 获得一个文件的MD5码 </summary>
        public static string GetMD5FromFile(string fileName) {
            using (var stream = new FileStream(fileName, FileMode.Open)) {
                return MD5.GetMd5String(stream);
            }
        }
        /// <summary> 获得一段字符串的MD5 </summary>
        public static string GetMD5FromString(string buffer) {
            return GetMD5FromBuffer(Encoding.UTF8.GetBytes(buffer));
        }
        /// <summary> 根据一段内存获得MD5码 </summary>
        public static string GetMD5FromBuffer(byte[] buffer) {
            if (buffer == null) return null;
            return MD5.GetMd5String(buffer);
        }
        /// <summary> 获得一个文件的MD5码 </summary>
        public static string GetMD5FromStream(Stream stream) {
            return MD5.GetMd5String(stream);
        }
    }
}
