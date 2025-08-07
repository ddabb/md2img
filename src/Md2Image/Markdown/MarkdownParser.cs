using System;
using System.IO;
using System.Threading.Tasks;
using Markdig;
using Markdig.Syntax;

namespace Md2Image.Markdown
{
    /// <summary>
    /// Markdown解析器，负责将Markdown文本解析为HTML
    /// </summary>
    public class MarkdownParser
    {
        private readonly MarkdownPipeline _pipeline;
        
        /// <summary>
        /// 创建Markdown解析器实例
        /// </summary>
        public MarkdownParser()
        {
            // 配置Markdig管道，启用常用扩展
            _pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseEmojiAndSmiley()
                .UseSoftlineBreakAsHardlineBreak()
                .UsePipeTables()
                .UseGridTables()
                .UseListExtras()
                .UseTaskLists()
                .UseAutoLinks()
                .UseGenericAttributes() // 允许使用通用属性
                .UseAutoIdentifiers() // 自动为标题生成ID
                .UseCustomContainers() // 支持自定义容器
                .UseDefinitionLists() // 支持定义列表
                .UseFootnotes() // 支持脚注
                .UseCitations() // 支持引用
                .UseBootstrap() // 使用Bootstrap样式
                .UseMediaLinks() // 支持媒体链接
                .Build();
        }
        
    /// <summary>
    /// 将Markdown文本解析为HTML
    /// </summary>
    /// <param name="markdown">Markdown文本</param>
    /// <returns>解析后的HTML</returns>
    public virtual string ParseToHtml(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return string.Empty;
            }
            
            return Markdig.Markdown.ToHtml(markdown, _pipeline);
        }
        
    /// <summary>
    /// 从文件读取并解析Markdown
    /// </summary>
    /// <param name="filePath">Markdown文件路径</param>
    /// <returns>解析后的HTML</returns>
    public virtual async Task<string> ParseFileToHtmlAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Markdown文件不存在: {filePath}");
            }
            
            string markdown = await File.ReadAllTextAsync(filePath);
            return ParseToHtml(markdown);
        }
        
        /// <summary>
        /// 获取Markdown文档的元数据（如标题、作者等）
        /// </summary>
        /// <param name="markdown">Markdown文本</param>
        /// <returns>元数据字典</returns>
        public System.Collections.Generic.Dictionary<string, string> ExtractMetadata(string markdown)
        {
            var document = Markdig.Markdown.Parse(markdown, _pipeline);
            var result = new System.Collections.Generic.Dictionary<string, string>();
            
            // 尝试提取标题
            var headings = document.Descendants<HeadingBlock>();
            foreach (var heading in headings)
            {
                if (heading.Level == 1)
                {
                    var title = markdown.Substring(heading.Span.Start, heading.Span.Length).Trim('#', ' ', '\t');
                    result["title"] = title;
                    break;
                }
            }
            
            return result;
        }
    }
}