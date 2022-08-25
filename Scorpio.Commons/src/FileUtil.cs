using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Scorpio.Commons {
    public static class FileUtil {
        public enum NameType {
            None,
            Lower,
            Upper,
        }
        //两个文件比较类型
        public enum CompareType {
            Size,                   //比较大小
            SizeAndModifyTime,      //比较大小和文件最后修改时间
            Content,                //比较内容
            MD5,                    //比较MD5
        }
        //public static readonly byte[] BomBuffer = new byte[] { 0xef, 0xbb, 0xbf };
        private const int ContentBlockSize = 2048;
        public static readonly Encoding UTF8WithBom = new UTF8Encoding(true);
        public static readonly Encoding DefaultEncoding = new UTF8Encoding(false);
        private static string GetName(this string name, NameType nameType) {
            switch (nameType) {
                case NameType.None:
                    return name;
                case NameType.Lower:
                    return name.ToLowerInvariant();
                case NameType.Upper:
                    return name.ToUpperInvariant();
                default:
                    return name;
            }
        }
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
        /// <summary> 比较两个内容是否相同 </summary>
        static bool CompareArray(byte[] source, byte[] target, int length) {
            for (var i = 0; i < length; ++i) {
                if (source[i] != target[i]) {
                    return false;
                }
            }
            return true;
        }
        public static bool CompareFile(string sourceFile, string targetFile) {
            return CompareFile(sourceFile, targetFile, CompareType.Content);
        }
        //比较两个文件是否相同
        public static bool CompareFile(string sourceFile, string targetFile, CompareType compareType) {
            if (!File.Exists(sourceFile) || !File.Exists(targetFile)) { 
                return false;
            }
            var sourceFileInfo = new FileInfo(sourceFile);
            var targetFileInfo = new FileInfo(targetFile);
            //首先比较大小,大小不同文件肯定不同
            if (sourceFileInfo.Length != targetFileInfo.Length) {
                return false;
            }
            if (compareType == CompareType.SizeAndModifyTime) {
                return sourceFileInfo.LastWriteTimeUtc == targetFileInfo.LastWriteTimeUtc;
            } else if (compareType == CompareType.Content) {
                int sourceReaded, targetReaded;
                byte[] sourceBuffer = new byte[ContentBlockSize];
                byte[] targetBuffer = new byte[ContentBlockSize];
                using (var sourceStream = new FileStream(sourceFile, FileMode.Open)) {
                    using (var targetStream = new FileStream(targetFile, FileMode.Open)) {
                        while (0 < (sourceReaded = sourceStream.Read(sourceBuffer, 0, ContentBlockSize))) {
                            targetReaded = targetStream.Read(targetBuffer, 0, ContentBlockSize);
                            if (sourceReaded != targetReaded || !CompareArray(sourceBuffer, targetBuffer, sourceReaded)) {
                                return false;
                            }
                        }
                    }
                }
                return true;
            } else if (compareType == CompareType.MD5) {
                return GetMD5FromFile(sourceFile) == GetMD5FromFile(targetFile);
            }
            return true;
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
        /// <summary> 同步一个文件 </summary>
        public static bool SyncFile(string source, string target) {
            return SyncFile(source, target, CompareType.Content);
        }
        /// <summary> 同步一个文件 </summary>
        public static bool SyncFile(string source, string target, CompareType compareType) {
            if (!CompareFile(source, target, compareType)) {
                File.Copy(source, target, true);
                return true;
            }
            return false;
        }
        /// <summary> 删除文件 </summary>
        public static bool DeleteFile(string fileName) {
            if (File.Exists(fileName)) {
                File.Delete(fileName);
                return true;
            }
            return false;
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
        public static bool DeleteEmptyFolder(string folder, bool recursive) {
            if (!Directory.Exists(folder)) { return false; }
            var changed = false;
            var dirs = Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly);
            foreach (var dir in dirs) {
                if (recursive) {
                    changed |= DeleteEmptyFolder(dir, recursive);
                }
                changed |= DeleteFolderIfEmpty(dir);
            }
            changed |= DeleteFolderIfEmpty(folder);
            return changed;
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
            CopyFolder(source, target, searchPatterns, recursive, NameType.None);
        }
        /// <summary> 拷贝文件夹 </summary>
        public static void CopyFolder(string source, string target, string[] searchPatterns, bool recursive, NameType nameType) {
            source = Path.GetFullPath(source);
            target = Path.GetFullPath(target);
            if (!Directory.Exists(source)) return;
            if (!Directory.Exists(target)) Directory.CreateDirectory(target);
            foreach (var file in GetFiles(source, searchPatterns, SearchOption.TopDirectoryOnly)) {
                File.Copy(file, Path.Combine(target, Path.GetFileName(file).GetName(nameType)), true);
            }
            if (recursive) {
                foreach (string folder in Directory.GetDirectories(source, "*", SearchOption.TopDirectoryOnly)) {
                    CopyFolder(folder, Path.Combine(target, Path.GetFileName(folder).GetName(nameType)), searchPatterns, recursive);
                }
            }
        }
        /// <summary> 移动文件夹 </summary>
        public static void MoveFiles(string source, string target, string searchPattern, bool recursive, bool overwrite) {
            MoveFolder(source, target, searchPattern == null ? null : new[] { searchPattern }, recursive, overwrite);
        }
        /// <summary> 移动文件夹 </summary>
        public static void MoveFolder(string source, string target, string[] searchPatterns, bool recursive, bool overwrite) {
            MoveFolder(source, target, searchPatterns, recursive, overwrite, NameType.None);
        }
        /// <summary> 移动文件夹 </summary>
        public static void MoveFolder(string source, string target, string[] searchPatterns, bool recursive, bool overwrite, NameType nameType) {
            if (!Directory.Exists(source)) return;
            if (!Directory.Exists(target)) Directory.CreateDirectory(target);
            foreach (string file in GetFiles(source, searchPatterns, SearchOption.TopDirectoryOnly)) {
                MoveFile(file, Path.Combine(target, Path.GetFileName(file).GetName(nameType)), overwrite);
            }
            if (recursive) {
                foreach (string folder in Directory.GetDirectories(source, "*", SearchOption.TopDirectoryOnly)) {
                    MoveFolder(folder, Path.Combine(target, Path.GetFileName(folder).GetName(nameType)), searchPatterns, recursive, overwrite);
                }
            }
            DeleteFolderIfEmpty(source);
        }
        /// <summary> 同步文件夹 </summary>
        public static bool SyncFiles(string source, string target, string searchPattern, bool recursive) {
            return SyncFolder(source, target, searchPattern == null ? null : new[] { searchPattern }, recursive);
        }
        /// <summary> 同步文件夹 </summary>
        public static bool SyncFolder(string source, string target, string[] searchPatterns, bool recursive) {
            return SyncFolder(source, target, searchPatterns, recursive, NameType.None);
        }
        public static bool SyncFolder(string source, string target, string[] searchPatterns, bool recursive, NameType nameType) {
            return SyncFolder(source, target, searchPatterns, recursive, CompareType.Size, nameType);
        }
        /// <summary> 同步文件夹 </summary>
        public static bool SyncFolder(string source, string target, string[] searchPatterns, bool recursive, CompareType compareType, NameType nameType) {
            source = Path.GetFullPath(source);
            target = Path.GetFullPath(target);
            if (!Directory.Exists(source)) return false;
            if (!Directory.Exists(target)) { Directory.CreateDirectory(target); }
            var files = new HashSet<string>();
            var existFiles = new HashSet<string>();
            var changed = false;
            foreach (var file in GetFiles(source, searchPatterns, SearchOption.TopDirectoryOnly)) {
                var name = Path.GetFileName(file);
                files.Add(name);
                existFiles.Add(name.GetName(nameType));
            }
            foreach (var file in GetFiles(target, searchPatterns, SearchOption.TopDirectoryOnly)) {
                if (!existFiles.Contains(Path.GetFileName(file))) {
                    File.Delete(file);
                    changed = true;
                }
            }
            foreach (var file in files) {
                var sourceFile = Path.Combine(source, file);
                var targetFile = Path.Combine(target, file.GetName(nameType));
                if (!File.Exists(targetFile) || !CompareFile(sourceFile, targetFile, compareType)) {
                    File.Copy(sourceFile, targetFile, true);
                    changed = true;
                }
            }
            var dirs = new HashSet<string>();
            var existDirs = new HashSet<string>();
            foreach (var dir in Directory.GetDirectories(source, "*", SearchOption.TopDirectoryOnly)) {
                var name = Path.GetFileName(dir);
                dirs.Add(name);
                existDirs.Add(name.GetName(nameType));
            }
            foreach (var dir in Directory.GetDirectories(target, "*", SearchOption.TopDirectoryOnly)) {
                if (!existDirs.Contains(Path.GetFileName(dir))) {
                    DeleteFolder(dir, null, true);
                    changed = true;
                }
            }
            if (recursive) {
                foreach (var dir in dirs) {
                    changed |= SyncFolder(Path.Combine(source, dir), Path.Combine(target, dir.GetName(nameType)), searchPatterns, recursive);
                }
            }
            return changed;
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
        public static string GetMD5FromFileStream(string fileName) {
            return GetMD5FromFile(fileName);
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

        public static byte[] GetMD5(string buffer) {
            return GetMD5(DefaultEncoding.GetBytes(buffer));
        }
        public static byte[] GetMD5(byte[] buffer) {
            if (buffer == null) return null;
            using (MD5 md = new MD5()) {
                return md.ComputeHash(buffer);
            }
        }
    }
}
