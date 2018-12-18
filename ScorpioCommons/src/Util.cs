using System;
using System.Collections.Generic;
using System.Text;

namespace Scorpio.Commons {
    public static class Util {
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
    }
}
