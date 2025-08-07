using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Md2Image.ImageGeneration;
using Md2Image.Markdown;
using Md2Image.Templates;

namespace Md2Image.Core
{
    /// <summary>
    /// Markdown转换器实现，将Markdown转换为图片
    /// </summary>
    public class MarkdownConverter : IMarkdownConverter
    {
        private readonly MarkdownParser _markdownParser;
        private readonly TemplateManager _templateManager;
        private readonly ImageGenerator _imageGenerator;
        
        /// <summary>
        /// 创建Markdown转换器实例
        /// </summary>
        /// <param name="templatesDirectory">自定义模板目录</param>
        public MarkdownConverter(string? templatesDirectory = null)
        {
            _markdownParser = new MarkdownParser();
            _templateManager = new TemplateManager(templatesDirectory);
            _imageGenerator = new ImageGenerator();
        }
        
        /// <summary>
        /// 将Markdown文本转换为图片
        /// </summary>
        /// <param name="markdownText">Markdown文本内容</param>
        /// <param name="options">转换选项</param>
        /// <returns>生成的图片数据列表</returns>
        public async Task<IList<byte[]>> ConvertToImagesAsync(string markdownText, ConversionOptions? options = null)
        {
            options ??= new ConversionOptions();
            
            // 提取元数据
            var metadata = _markdownParser.ExtractMetadata(markdownText);
            if (string.IsNullOrEmpty(options.Variables.Title) && metadata.TryGetValue("title", out string? title))
            {
                options.Variables.Title = title;
            }
            
            // 解析Markdown为HTML
            string html = _markdownParser.ParseToHtml(markdownText);
            
            // 应用模板
            string templatedHtml = await _templateManager.ApplyTemplateAsync(options.TemplateName, html, options);
            
            // 生成图片
            return await _imageGenerator.GenerateImagesAsync(templatedHtml, options);
        }
        
        /// <summary>
        /// 将Markdown文件转换为图片
        /// </summary>
        /// <param name="markdownFilePath">Markdown文件路径</param>
        /// <param name="options">转换选项</param>
        /// <returns>生成的图片数据列表</returns>
        public async Task<IList<byte[]>> ConvertFileToImagesAsync(string markdownFilePath, ConversionOptions? options = null)
        {
            if (!File.Exists(markdownFilePath))
            {
                throw new FileNotFoundException($"Markdown文件不存在: {markdownFilePath}");
            }
            
            string markdownText = await File.ReadAllTextAsync(markdownFilePath);
            
            options ??= new ConversionOptions();
            
            // 如果没有指定标题，使用文件名作为标题
            if (string.IsNullOrEmpty(options.Variables.Title))
            {
                options.Variables.Title = Path.GetFileNameWithoutExtension(markdownFilePath);
            }
            
            return await ConvertToImagesAsync(markdownText, options);
        }
        
        /// <summary>
        /// 将Markdown文本转换为图片并保存到指定路径
        /// </summary>
        /// <param name="markdownText">Markdown文本内容</param>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="fileNamePrefix">文件名前缀</param>
        /// <param name="options">转换选项</param>
        /// <returns>生成的图片文件路径列表</returns>
        public async Task<IList<string>> ConvertAndSaveAsync(string markdownText, string outputDirectory, string fileNamePrefix, ConversionOptions? options = null)
        {
            options ??= new ConversionOptions();
            
            // 提取元数据
            var metadata = _markdownParser.ExtractMetadata(markdownText);
            if (string.IsNullOrEmpty(options.Variables.Title) && metadata.TryGetValue("title", out string? title))
            {
                options.Variables.Title = title;
            }
            
            // 解析Markdown为HTML
            string html = _markdownParser.ParseToHtml(markdownText);
            
            // 应用模板
            string templatedHtml = await _templateManager.ApplyTemplateAsync(options.TemplateName, html, options);
            
            // 生成并保存图片
            return await _imageGenerator.GenerateAndSaveImagesAsync(templatedHtml, outputDirectory, fileNamePrefix, options);
        }
        
        /// <summary>
        /// 将Markdown文件转换为图片并保存到指定路径
        /// </summary>
        /// <param name="markdownFilePath">Markdown文件路径</param>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="fileNamePrefix">文件名前缀，如果为null则使用原文件名</param>
        /// <param name="options">转换选项</param>
        /// <returns>生成的图片文件路径列表</returns>
        public async Task<IList<string>> ConvertFileAndSaveAsync(string markdownFilePath, string outputDirectory, string? fileNamePrefix = null, ConversionOptions? options = null)
        {
            if (!File.Exists(markdownFilePath))
            {
                throw new FileNotFoundException($"Markdown文件不存在: {markdownFilePath}");
            }
            
            string markdownText = await File.ReadAllTextAsync(markdownFilePath);
            
            options ??= new ConversionOptions();
            
            // 如果没有指定标题，使用文件名作为标题
            if (string.IsNullOrEmpty(options.Variables.Title))
            {
                options.Variables.Title = Path.GetFileNameWithoutExtension(markdownFilePath);
            }
            
            // 如果没有指定文件名前缀，使用原文件名
            if (string.IsNullOrEmpty(fileNamePrefix))
            {
                fileNamePrefix = Path.GetFileNameWithoutExtension(markdownFilePath);
            }
            
            return await ConvertAndSaveAsync(markdownText, outputDirectory, fileNamePrefix, options);
        }
        
        /// <summary>
        /// 获取可用模板列表
        /// </summary>
        /// <returns>模板名称列表</returns>
        public List<string> GetAvailableTemplates()
        {
            return new List<string>(_templateManager.GetAvailableTemplates());
        }
    }
}