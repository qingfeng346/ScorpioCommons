using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Linq;

namespace Scorpio.Commons {
    public static partial class ScorpioUtil {
        public class ProcessResult {
            public int exitCode;
            public string output;
            public string error;
            public ProcessResult(Process process) {
                exitCode = process.ExitCode;
                if (process.StartInfo.RedirectStandardOutput) {
                    output = process.StandardOutput.ReadToEnd();
                }
                if (process.StartInfo.RedirectStandardError) {
                    error = process.StandardError.ReadToEnd();
                }
            }
        }
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
        static void StartProcess(string fileName, string workingDirectory, string arguments, Action<Process> preStart, Action<Process> postStart) {
            using (var process = new Process()) {
                process.StartInfo.FileName = fileName;
                if (!string.IsNullOrEmpty(workingDirectory)) {
                    process.StartInfo.WorkingDirectory = workingDirectory;
                }
                if (!string.IsNullOrEmpty(arguments)) {
                    process.StartInfo.Arguments = arguments;
                }
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                preStart?.Invoke(process);
                process.Start();
                postStart?.Invoke(process);
            }
        }
        public static void Start(string fileName, string workingDirectory = null, string arguments = null) {
            StartProcess(fileName, workingDirectory, arguments, null, null);
        }
        public static void StartPowershell(string fileName, string workingDirectory = null, string arguments = null) {
            Start("pwsh", workingDirectory, $"-ExecutionPolicy Unrestricted {fileName} {arguments}");
        }
        public static void StartShell(string fileName, string workingDirectory = null, string arguments = null) {
            Start("sh", workingDirectory, $"{fileName} {arguments}");
        }
        public static ProcessResult Execute(string fileName, string workingDirectory = null, IEnumerable<string> arguments = null, bool showWindow = false) {
            var builder = new StringBuilder();
            if (arguments != null) {
                foreach (var arg in arguments) {
                    builder.Append("\"" + arg + "\" ");
                }
            }
            return Execute(fileName, workingDirectory, builder.ToString(), showWindow);
        }
        public static ProcessResult Execute(string fileName, string workingDirectory = null, string arguments = null, bool showWindow = false) {
            ProcessResult processResult = null;
            StartProcess(fileName, workingDirectory, arguments, (process) => {
                if (showWindow) {
                    process.StartInfo.CreateNoWindow = false;
                    process.StartInfo.UseShellExecute = true;
                } else {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
    #if UNITY_EDITOR_WIN
                    process.StartInfo.StandardOutputEncoding = Encoding.GetEncoding("GBK");
                    process.StartInfo.StandardErrorEncoding = Encoding.GetEncoding("GBK");
    #endif
                }
                process.EnableRaisingEvents = true;
            }, (process) => {
                process.WaitForExit();
                processResult = new ProcessResult(process);
            });
            return processResult;
        }
        public static ProcessResult ExecutePowershell(string fileName, string workingDirectory = null, string arguments = null, bool showWindow = false) {
            return Execute("pwsh", workingDirectory, $"-ExecutionPolicy Unrestricted {fileName} {arguments}", showWindow);
        }
        public static ProcessResult ExecuteShell(string fileName, string workingDirectory = null, string arguments = null) {
            return Execute("sh", workingDirectory, $"{fileName} {arguments}", false);
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
        public static string GetAlign(this string str, int length) {
            var builder = new StringBuilder();
            builder.Append(str);
            for (var i = str.Length; i < length; ++i) {
                builder.Append(' ');
            }
            return builder.ToString();
        }
        //多线程运行列表
        public static void StartQueue<T>(IEnumerable<T> datas, Func<T, int, Task> func, int queue = 16) {
            var sync = new object();
            var tasks = new List<Task<string>>();
            var list = datas.ToArray();
            var length = list.Length;
            var current = 0;
            var isError = false;
            for (var i = 0; i < queue; i++) {
                var task = Task.Run(async () => {
                    try {
                    Start:
                        if (isError) { return ""; }
                        int index;
                        T data = default;
                        lock (sync) {
                            if (current < length) {
                                index = current++;
                                data = list[index];
                            } else {
                                return "";
                            }
                        }
                        await func(data, index);
                        goto Start;
                    } catch (Exception e) {
                        return e.ToString();
                    }
                });
                tasks.Add(task);
            }
            while (tasks.Count > 0) {
                var taskIndex = Task.WaitAny(tasks.ToArray());
                var taskResult = tasks[taskIndex].Result;
                tasks.RemoveAt(taskIndex);
                if (!string.IsNullOrEmpty(taskResult)) {
                    isError = true;
                    Task.WaitAll(tasks.ToArray());
                    throw new Exception(taskResult);
                }
            }
        }
    }
}
