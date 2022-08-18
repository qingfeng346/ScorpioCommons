using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;

namespace Scorpio.Commons {
    public static partial class ScorpioUtil {
        const string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        const int READ_LENGTH = 8192;
        public static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string CurrentDirectory = Environment.CurrentDirectory;
        private const double KB_LENGTH = 1024;              //1KB 的字节数
        private const double MB_LENGTH = 1048576;           //1MB 的字节数
        private const double GB_LENGTH = 1073741824;		//1GB 的字节数
        public static string GetMemory(this long by) {
            if (by < MB_LENGTH)
                return string.Format("{0:0.##} KB", Convert.ToDouble(by) / KB_LENGTH);
            else if (by < GB_LENGTH)
                return string.Format("{0:0.##} MB", Convert.ToDouble(by) / MB_LENGTH);
            else
                return string.Format("{0:0.##} GB", Convert.ToDouble(by) / GB_LENGTH);
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

        public static string ToBase64(byte[] bytes) {
            return Convert.ToBase64String(bytes);
        }
        public static byte[] FromBase64(string base64) {
            return Convert.FromBase64String(base64);
        }
        public static int StartProcess(string fileName, string workingDirectory = null, IEnumerable<string> arguments = null, Action<Process> preStart = null, Action<Process> waitExit = null) {
            try {
                using (var process = new Process()) {
                    process.StartInfo.FileName = fileName;
                    if (!string.IsNullOrEmpty(workingDirectory)) {
                        process.StartInfo.WorkingDirectory = workingDirectory;
                    }
                    if (arguments != null) {
                        var builder = new StringBuilder();
                        foreach (var argument in arguments) {
                            builder.Append($@" ""{argument}"" ");
                        }
                        process.StartInfo.Arguments = builder.ToString();
                    }
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.EnableRaisingEvents = true;
                    preStart?.Invoke(process);
                    process.Start();
                    process.OutputDataReceived += (sender, args) => {
                        Console.WriteLine(args.Data);
                    };
                    process.ErrorDataReceived += (sender, args) => {
                        Console.Error.WriteLine(args.Data);
                    };
                    waitExit?.Invoke(process);
                    process.WaitForExit();
                    return process.ExitCode;
                }
            } catch (Exception e) {
                logger.error("StartProcess Error : " + e.ToString());
            }
            return -1;
        }
        public static int StartCwd(string fileName, string workingDirectory = null, IEnumerable<string> arguments = null, Action<Process> preStart = null, Action<Process> waitExit = null) {
            try {
                using (var process = new Process()) {
                    process.StartInfo.FileName = "cmd";
                    if (!string.IsNullOrEmpty(workingDirectory)) {
                        process.StartInfo.WorkingDirectory = workingDirectory;
                    }
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    process.StartInfo.CreateNoWindow = false;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.EnableRaisingEvents = true;
                    process.Start();
                    process.OutputDataReceived += (sender, args) => {
                        Console.WriteLine(args.Data);
                    };
                    process.ErrorDataReceived += (sender, args) => {
                        Console.Error.WriteLine(args.Data);
                    };
                    var builder = new StringBuilder();
                    builder.Append(fileName);
                    if (arguments != null) {
                        foreach (var argument in arguments) {
                            builder.Append($@" ""{argument}"" ");
                        }
                    }
                    preStart?.Invoke(process);
                    process.StandardInput.WriteLine(builder.ToString());
                    process.StandardInput.WriteLine();  //防止某些cmd有pause
                    process.StandardInput.WriteLine("exit");
                    waitExit?.Invoke(process);
                    process.WaitForExit();
                    return process.ExitCode;
                }
            } catch (Exception e) {
                logger.error("StartProcess Error : " + e.ToString());
            }
            return -1;
        }

        public static bool IsWindows() {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }
        public static bool IsLinux() {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }
        public static bool IsMacOS() {
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
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
        public static void UnregisterApplication(string app) {
            if (IsWindows()) {
                var path = Path.GetFullPath(Path.GetDirectoryName(app));
                var environmentVariables = new List<string>(Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User).Split(';'));
                if (environmentVariables.Contains(path)) {
                    environmentVariables.Remove(path);
                    Environment.SetEnvironmentVariable("Path", string.Join(";", environmentVariables.ToArray()), EnvironmentVariableTarget.User);
                }
            } else {
                var fileName = Path.GetFileName(app);
                FileUtil.DeleteFile($"/usr/local/bin/{fileName}");
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
            logger.info($"OperatingSystem : {osPlatform}");
            logger.info($"OSArchitecture : {RuntimeInformation.OSArchitecture}");
            logger.info($"UserName : {Environment.UserName}");
            logger.info($"Application Directory : {BaseDirectory}");
            logger.info($"Environment Directory : {CurrentDirectory}");
        }
        public static void Download(string url, string fileName, Action<long, long> progress = null) {
            FileUtil.DeleteFile(fileName);
            using (var fileStream = new FileStream(fileName, FileMode.CreateNew)) {
                Download(url, fileStream, progress);
            }
        }
        public static void Download(string url, Stream stream, Action<long, long> progress = null) {
            logger.info($"开始下载文件... : {url}");
            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            using (var response = request.GetResponse()) {
                var length = response.ContentLength;
                using (var responseStream = response.GetResponseStream()) {
                    var bytes = new byte[READ_LENGTH];
                    var readed = 0;
                    while (true) {
                        var readSize = responseStream.Read(bytes, 0, READ_LENGTH);
                        if (readSize <= 0) { break; }
                        readed += readSize;
                        stream.Write(bytes, 0, readSize);
                        progress?.Invoke(readed, length);
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
        public static object ChangeType(this string value, Type type) {
            if (type == typeof(string)) {
                return value;
            } else if (type == typeof(bool)) {
                value = value.ToLowerInvariant();
                return value == "true" || value == "yes" || value == "1";
            } else if (type == typeof(sbyte) ||
                       type == typeof(byte) ||
                       type == typeof(short) ||
                       type == typeof(ushort) ||
                       type == typeof(int) ||
                       type == typeof(uint) ||
                       type == typeof(long) ||
                       type == typeof(ulong) ||
                       type == typeof(float) ||
                       type == typeof(double) ||
                       type == typeof(decimal)) {
                return Convert.ChangeType(value, type);
            } else if (type.IsEnum) {
                if (int.TryParse(value, out var result)) {
                    return Enum.ToObject(type, result);
                } else {
                    return Enum.Parse(type, value, true);
                }
            } else if (type == typeof(DateTime)) {
                return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(double.Parse(value));
            } else {
                return null;
            }
        }
    }
}
