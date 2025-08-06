using System;
using System.Text;

namespace Md2Image.Core
{
    /// <summary>
    /// 文本编码处理器，负责处理文本编码相关的操作
    /// </summary>
    public class TextEncodingHandler
    {
        /// <summary>
        /// 默认编码
        /// </summary>
        private readonly Encoding _defaultEncoding;

        /// <summary>
        /// 创建文本编码处理器实例
        /// </summary>
        /// <param name="encoding">指定编码，默认为UTF-8</param>
        public TextEncodingHandler(Encoding encoding = null)
        {
            // 确保.NET能够处理所有可能的编码
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"注册编码提供程序时出错: {ex.Message}");
            }
            
            // 确保使用UTF-8编码并启用BOM检测
            _defaultEncoding = encoding ?? new UTF8Encoding(false);
        }

        /// <summary>
        /// 检测文本的编码
        /// </summary>
        /// <param name="bytes">文本字节数组</param>
        /// <returns>检测到的编码</returns>
        public Encoding DetectEncoding(byte[] bytes)
        {
            // 检查BOM标记
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return Encoding.UTF8;
            
            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
                return Encoding.BigEndianUnicode;
            
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
                return Encoding.Unicode;
            
            if (bytes.Length >= 4 && bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 0xFE && bytes[3] == 0xFF)
                return Encoding.UTF32;

            // 尝试检测中文编码
            try
            {
                // 尝试GB18030/GB2312/GBK
                Encoding gbk = Encoding.GetEncoding("GB18030");
                string gbkString = gbk.GetString(bytes);
                byte[] gbkBytes = gbk.GetBytes(gbkString);
                
                if (bytes.Length == gbkBytes.Length)
                {
                    bool isSame = true;
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        if (bytes[i] != gbkBytes[i])
                        {
                            isSame = false;
                            break;
                        }
                    }
                    
                    if (isSame)
                        return gbk;
                }
            }
            catch
            {
                // 忽略异常，继续尝试其他编码
            }

            // 默认返回UTF-8
            return _defaultEncoding;
        }

        /// <summary>
        /// 将文本转换为指定编码
        /// </summary>
        /// <param name="text">原始文本</param>
        /// <param name="sourceEncoding">源编码</param>
        /// <param name="targetEncoding">目标编码</param>
        /// <returns>转换后的文本</returns>
        public string ConvertEncoding(string text, Encoding sourceEncoding, Encoding targetEncoding)
        {
            if (string.IsNullOrEmpty(text))
                return text;
                
            byte[] bytes = sourceEncoding.GetBytes(text);
            bytes = Encoding.Convert(sourceEncoding, targetEncoding, bytes);
            return targetEncoding.GetString(bytes);
        }

        /// <summary>
        /// 确保文本使用UTF-8编码
        /// </summary>
        /// <param name="text">原始文本</param>
        /// <returns>UTF-8编码的文本</returns>
        public string EnsureUtf8(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
                
            // 先转换为字节数组，检测编码
            byte[] bytes = _defaultEncoding.GetBytes(text);
            Encoding detectedEncoding = DetectEncoding(bytes);
            
            // 如果不是UTF-8，则转换
            if (detectedEncoding != Encoding.UTF8)
            {
                return ConvertEncoding(text, detectedEncoding, Encoding.UTF8);
            }
            
            return text;
        }
    }
}