using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;

namespace Scorpio.Commons {
    public static class Util {
        const string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        const int READ_LENGTH = 8192;
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
            if (str.isNullOrWhiteSpace()) return str;
            if (str.Length == 1) return str.ToUpper();
            return char.ToUpper(str[0]) + str.Substring(1);
        }
        public static string ToOneLower(string str) {
            if (str.isNullOrWhiteSpace()) return str;
            if (str.Length == 1) return str.ToLower();
            return char.ToLower(str[0]) + str.Substring(1);
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
        /// <summary> 获得一个文件的MD5码 </summary>
        public static string GetMD5FromStream(Stream stream) {
            return MD5.GetMd5String(stream);
        }
        public static string ToBase64(byte[] bytes) {
            return Convert.ToBase64String(bytes);
        }
        public static byte[] FromBase64(string base64) {
            return Convert.FromBase64String(base64);
        }
        public static string StartProcess(string fileName, string arguments = null) {
            string output = "";
            try {
                using (var process = new Process()) {
                    process.StartInfo.FileName = fileName;
                    if (!arguments.isNullOrWhiteSpace())
                        process.StartInfo.Arguments = arguments;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.EnableRaisingEvents = true;
                    process.Start();
                    output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                }
            } catch (Exception e) {
                Logger.error("StartProcess Error : " + e.ToString());
                return null;
            }
            return output;
        }

        public static bool IsWindows() {
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
        }
        public static bool IsLinux() {
            if (Environment.OSVersion.Platform == PlatformID.Unix) {
                return StartProcess("uname").ToLower().Contains("linux");
            }
            return false;
        }
        public static bool IsMacOS() {
            if (Environment.OSVersion.Platform == PlatformID.Unix) {
                return StartProcess("uname").ToLower().Contains("darwin");
            }
            return false;
        }

        public static void RegisterApplication(string app) {
            if (IsWindows()) {
                var path = Path.GetDirectoryName(app);
                var environmentVariables = new List<string>(Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User).Split(';'));
                if (!environmentVariables.Contains(path)) {
                    environmentVariables.Add(path);
                    Environment.SetEnvironmentVariable("Path", string.Join(";", environmentVariables.ToArray()), EnvironmentVariableTarget.User);
                }
            } else {
                StartProcess("ln", $"-s {app} /usr/local/bin/");
            }
        }
        public static void PrintSystemInfo() {
            string osPlatform = "Other";
            if (IsWindows()) {
                osPlatform = "Windows";
            } else if (IsLinux()) {
                osPlatform = "Linux";
            } else if (IsMacOS()) {
                osPlatform = "MacOS";
            }
            Logger.info($"OperatingSystem : {osPlatform}");
            Logger.info($"UserName : {Environment.UserName}");
            Logger.info($"Application Directory : {BaseDirectory}");
            Logger.info($"Environment Directory : {CurrentDirectory}");
        }
        public static void Download(string url, string fileName, Action<long, long> progress = null) {
            Logger.info($"开始下载文件... : {fileName}");
            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            using (var response = request.GetResponse()) {
                var length = response.ContentLength;
                using (var stream = response.GetResponseStream()) {
                    var bytes = new byte[READ_LENGTH];
                    var readed = 0;
                    using (var fileStream = new FileStream(fileName, FileMode.CreateNew)) {
                        while (true) {
                            var readSize = stream.Read(bytes, 0, READ_LENGTH);
                            if (readSize <= 0) { break; }
                            readed += readSize;
                            fileStream.Write(bytes, 0, readSize);
                            progress?.Invoke(readed, length);
                        }
                    }
                }
            }
        }
        public static byte[] Request(string url, Action<HttpWebRequest> postRequest = null) {
            //创建 SL/TLS 安全通道
            try {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                | (SecurityProtocolType)0x300 //Tls11
                                | (SecurityProtocolType)0xC00; //Tls12
            } catch (Exception) { }
            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";
            request.ProtocolVersion = HttpVersion.Version10;
            request.UserAgent = DefaultUserAgent;
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Timeout = 30000;                    //设定超时时间30秒
            if (postRequest != null) postRequest(request);
            using (var response = request.GetResponse()) {
                using (var stream = response.GetResponseStream()) {
                    var bytes = new byte[READ_LENGTH];
                    using (var memoryStream = new MemoryStream()) {
                        while (true) {
                            var readSize = stream.Read(bytes, 0, READ_LENGTH);
                            if (readSize <= 0) { break; }
                            memoryStream.Write(bytes, 0, readSize);
                        }
                        return memoryStream.ToArray();
                    }
                }
            }
        }
        public static string RequestString(string url, Action<HttpWebRequest> postRequest = null) {
            var bytes = Request(url, postRequest);
            return bytes != null ? Encoding.UTF8.GetString(bytes) : "";
        }
        public static bool isNullOrWhiteSpace(this string str) {
            return str == null || str.Trim().Length == 0;
        }
    }
}
