using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Md2Image.Core;

namespace Md2Image.Templates
{
    /// <summary>
    /// 模板管理器，负责加载和应用模板
    /// </summary>
    public class TemplateManager
    {
        private readonly string _templatesDirectory;
        private readonly Dictionary<string, string> _templateCache = new Dictionary<string, string>();
        
        /// <summary>
        /// 创建模板管理器实例
        /// </summary>
        /// <param name="templatesDirectory">模板目录路径</param>
        public TemplateManager(string templatesDirectory = null)
        {
            _templatesDirectory = templatesDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates");
        }
        
        /// <summary>
        /// 获取可用模板列表
        /// </summary>
        /// <returns>模板名称列表</returns>
        public IEnumerable<string> GetAvailableTemplates()
        {
            if (!Directory.Exists(_templatesDirectory))
            {
                yield break;
            }
            
            foreach (var file in Directory.GetFiles(_templatesDirectory, "*.html"))
            {
                yield return Path.GetFileNameWithoutExtension(file);
            }
        }
        
        /// <summary>
        /// 应用模板
        /// </summary>
        /// <param name="templateName">模板名称</param>
        /// <param name="htmlContent">HTML内容</param>
        /// <param name="options">转换选项</param>
        /// <returns>应用模板后的HTML</returns>
        public async Task<string> ApplyTemplateAsync(string templateName, string htmlContent, ConversionOptions options)
        {
            string templateContent;
            
            if (!string.IsNullOrEmpty(options.CustomTemplatePath) && File.Exists(options.CustomTemplatePath))
            {
                // 使用自定义模板
                templateContent = await File.ReadAllTextAsync(options.CustomTemplatePath);
            }
            else
            {
                // 使用内置模板
                string templatePath = Path.Combine(_templatesDirectory, $"{templateName}.html");
                
                if (!File.Exists(templatePath))
                {
                    // 如果指定的模板不存在，使用默认模板
                    templatePath = Path.Combine(_templatesDirectory, "default.html");
                    
                    if (!File.Exists(templatePath))
                    {
                        // 如果默认模板也不存在，使用内置的简单模板
                        return GetSimpleTemplate(htmlContent, options);
                    }
                }
                
                // 从缓存中获取模板，如果不存在则加载
                if (!_templateCache.TryGetValue(templatePath, out templateContent))
                {
                    templateContent = await File.ReadAllTextAsync(templatePath);
                    _templateCache[templatePath] = templateContent;
                }
            }
            
            // 简单的变量替换
            var result = templateContent
                .Replace("{{ content }}", htmlContent)
                .Replace("{{ title }}", options.Variables.Title ?? "")
                .Replace("{{ author }}", options.Variables.Author ?? "")
                .Replace("{{ date }}", options.Variables.Date?.ToString("yyyy-MM-dd") ?? "")
                .Replace("{{ watermark_text }}", options.WatermarkText ?? "");
            
            // 处理简单的条件语句
            result = Regex.Replace(result, @"\{\{\s*if\s+author\s+!=\s+null\s*\}\}(.*?)\{\{\s*end\s*\}\}", 
                !string.IsNullOrEmpty(options.Variables.Author) ? "$1" : "", RegexOptions.Singleline);
            
            result = Regex.Replace(result, @"\{\{\s*if\s+date\s+!=\s+null\s*\}\}(.*?)\{\{\s*end\s*\}\}", 
                options.Variables.Date.HasValue ? "$1" : "", RegexOptions.Singleline);
            
            result = Regex.Replace(result, @"\{\{\s*if\s+watermark_text\s*\}\}(.*?)\{\{\s*end\s*\}\}", 
                !string.IsNullOrEmpty(options.WatermarkText) ? "$1" : "", RegexOptions.Singleline);
            
            return result;
        }
        
        /// <summary>
        /// 获取简单的内置模板
        /// </summary>
        private string GetSimpleTemplate(string htmlContent, ConversionOptions options)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset=\"UTF-8\">");
            sb.AppendLine($"    <title>{options.Variables.Title ?? ""}</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; }");
            sb.AppendLine("        h1 { color: #333; border-bottom: 1px solid #eee; padding-bottom: 10px; }");
            sb.AppendLine("        .watermark { position: fixed; bottom: 10px; right: 10px; opacity: 0.5; font-size: 12px; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            
            if (!string.IsNullOrEmpty(options.Variables.Title))
            {
                sb.AppendLine($"    <h1>{options.Variables.Title}</h1>");
            }
            
            if (!string.IsNullOrEmpty(options.Variables.Author))
            {
                sb.AppendLine($"    <p>作者: {options.Variables.Author}</p>");
            }
            
            if (options.Variables.Date.HasValue)
            {
                sb.AppendLine($"    <p>日期: {options.Variables.Date.Value:yyyy-MM-dd}</p>");
            }
            
            sb.AppendLine("    <div class=\"content\">");
            sb.AppendLine(htmlContent);
            sb.AppendLine("    </div>");
            
            if (!string.IsNullOrEmpty(options.WatermarkText))
            {
                sb.AppendLine($"    <div class=\"watermark\">{options.WatermarkText}</div>");
            }
            
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            
            return sb.ToString();
        }
    }
}