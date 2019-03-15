using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace Scorpio.Commons {
    public static class Util {
        public static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string CurrentDirectory = Environment.CurrentDirectory;
        private const double KB_LENGTH = 1024;              //1KB 的字节数
        private const double MB_LENGTH = 1048576;           //1MB 的字节数
        private const double GB_LENGTH = 1073741824;		//1GB 的字节数
        public static string GetMemory(long by) {
            if (by < MB_LENGTH)
                return string.Format("{0:f2} KB", Convert.ToDouble(by) / KB_LENGTH);
            else if (by < GB_LENGTH)
                return string.Format("{0:f2} MB", Convert.ToDouble(by) / MB_LENGTH);
            else
                return string.Format("{0:f2} GB", Convert.ToDouble(by) / GB_LENGTH);
        }
        public static string GetExcelColumn(int column) {
            StringBuilder stringBuilder = new StringBuilder();
            if (column < 26) {
                stringBuilder.Append((char)('A' + column));
            } else if (column < 27 * 26) {
                stringBuilder.Append((char)('A' + column / 26 - 1));
                stringBuilder.Append((char)('A' + column % 26));
            } else {
                throw new Exception("column is out of max column : " + column);
            }
            return stringBuilder.ToString();
        }
        public static string ToOneUpper(string str) {
            if (string.IsNullOrWhiteSpace(str)) return str;
            if (str.Length <= 1) return str.ToUpper();
            return char.ToUpper(str[0]) + str.Substring(1);
        }
        /// <summary> 获得一个文件的MD5码 </summary>
        public static string GetMD5FromFile(string fileName) {
            return GetMD5FromBuffer(FileUtil.GetFileBuffer(fileName));
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
        public static void RegisterApplication(string app) {
            var path = Path.GetDirectoryName(app);
            if (Environment.OSVersion.ToString().ToLower().Contains("windows")) {
                var p = new List<string>(Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User).Split(';'));
                if (!p.Contains(path)) {
                    p.Add(path);
                    Environment.SetEnvironmentVariable("Path", string.Join(";", p.ToArray()), EnvironmentVariableTarget.User);
                }
            } else {
                var info = new ProcessStartInfo("ln");
                info.Arguments = $"-s {app} /usr/bin/";
                info.CreateNoWindow = false;
                info.ErrorDialog = true;
                info.UseShellExecute = true;
                info.RedirectStandardOutput = false;
                info.RedirectStandardError = false;
                info.RedirectStandardInput = false;
                var process = Process.Start(info);
                process.WaitForExit();
                process.Close();
            }
        }
        public static void PrintSystemInfo() {
            Logger.info($"os version : {Environment.OSVersion}");
            Logger.info($"is 64bit process : {Environment.Is64BitProcess}");
            Logger.info($"user name : {Environment.UserName}");
            Logger.info($"app path is : {BaseDirectory}");
            Logger.info($"environment path is : {CurrentDirectory}");
        }
    }
}
