using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Md2Image.Core
{
    /// <summary>
    /// Markdown转换器接口，定义将Markdown转换为图片的核心功能
    /// </summary>
    public interface IMarkdownConverter
    {
        /// <summary>
        /// 将Markdown文本转换为图片
        /// </summary>
        /// <param name="markdownText">Markdown文本内容</param>
        /// <param name="options">转换选项</param>
        /// <returns>生成的图片数据列表</returns>
        Task<IList<byte[]>> ConvertToImagesAsync(string markdownText, ConversionOptions options = null);
        
        /// <summary>
        /// 将Markdown文件转换为图片
        /// </summary>
        /// <param name="markdownFilePath">Markdown文件路径</param>
        /// <param name="options">转换选项</param>
        /// <returns>生成的图片数据列表</returns>
        Task<IList<byte[]>> ConvertFileToImagesAsync(string markdownFilePath, ConversionOptions options = null);
        
        /// <summary>
        /// 将Markdown文本转换为图片并保存到指定路径
        /// </summary>
        /// <param name="markdownText">Markdown文本内容</param>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="fileNamePrefix">文件名前缀</param>
        /// <param name="options">转换选项</param>
        /// <returns>生成的图片文件路径列表</returns>
        Task<IList<string>> ConvertAndSaveAsync(string markdownText, string outputDirectory, string fileNamePrefix, ConversionOptions options = null);
        
        /// <summary>
        /// 将Markdown文件转换为图片并保存到指定路径
        /// </summary>
        /// <param name="markdownFilePath">Markdown文件路径</param>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="fileNamePrefix">文件名前缀，如果为null则使用原文件名</param>
        /// <param name="options">转换选项</param>
        /// <returns>生成的图片文件路径列表</returns>
        Task<IList<string>> ConvertFileAndSaveAsync(string markdownFilePath, string outputDirectory, string fileNamePrefix = null, ConversionOptions options = null);
    }
}