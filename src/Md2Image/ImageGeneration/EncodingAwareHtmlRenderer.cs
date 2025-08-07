using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Md2Image.Core;
using SkiaSharp;
using System.Xml;
using System.Text.RegularExpressions;
using System.Linq;
using Md2Image.ImageGeneration.Renderers;

namespace Md2Image.ImageGeneration
{
    /// <summary>
    /// 编码感知的HTML渲染器，确保正确处理中文和特殊字符
    /// </summary>
    public class EncodingAwareHtmlRenderer : HtmlRenderer
    {
        private readonly TextEncodingHandler _encodingHandler;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="encodingHandler">编码处理器</param>
        public EncodingAwareHtmlRenderer(TextEncodingHandler? encodingHandler = null)
        {
            _encodingHandler = encodingHandler ?? new TextEncodingHandler();
        }
        
        /// <summary>
        /// 重写HTML解析方法，确保正确处理编码
        /// </summary>
        protected override List<HtmlElement> ParseHtmlSequentially(string htmlContent)
        {
            // 使用编码处理器确保HTML内容使用正确的编码
            htmlContent = _encodingHandler.EnsureUtf8(htmlContent);
            
            var elements = new List<HtmlElement>();
            
            try
            {
                // 提取body内容
                var bodyMatch = Regex.Match(htmlContent, @"<body[^>]*>(.*?)</body>", RegexOptions.Singleline);
                string bodyContent = bodyMatch.Success ? bodyMatch.Groups[1].Value : htmlContent;
                
                // 提取内容区域
                var contentMatch = Regex.Match(bodyContent, @"<div\s+class=""content"">(.*?)</div>", RegexOptions.Singleline);
                string contentHtml = contentMatch.Success ? contentMatch.Groups[1].Value : bodyContent;
                
                // 处理列表项
                contentHtml = ProcessListItems(contentHtml);
                
                // 使用正则表达式按顺序提取HTML元素
                string pattern = @"<(h1|h2|p|pre|blockquote|table|img|ul|ol|li)[^>]*>(.*?)</\1>|<img[^>]*>";
                var matches = Regex.Matches(contentHtml, pattern, RegexOptions.Singleline);
                
                foreach (Match match in matches)
                {
                    string tagName = match.Groups[1].Value.ToLower();
                    string content = match.Groups[2].Value;
                    
                    // 根据标签类型创建对应的元素
                    switch (tagName)
                    {
                        case "h1":
                            elements.Add(new HtmlElement
                            {
                                Type = ElementType.Heading1,
                                Content = StripHtmlTags(content)
                            });
                            break;
                        case "h2":
                            elements.Add(new HtmlElement
                            {
                                Type = ElementType.Heading2,
                                Content = StripHtmlTags(content)
                            });
                            break;
                        case "p":
                            // 检查是否是列表项
                            if (content.StartsWith("• ") || content.StartsWith("- "))
                            {
                                elements.Add(new HtmlElement
                                {
                                    Type = ElementType.ListItem,
                                    Content = StripHtmlTags(content)
                                });
                            }
                            else
                            {
                                elements.Add(new HtmlElement
                                {
                                    Type = ElementType.Paragraph,
                                    Content = StripHtmlTags(content)
                                });
                            }
                            break;
                        case "pre":
                            // 处理代码块，保留语言标识
                            string codeContent = content;
                            
                            // 检查是否有code标签
                            var codeMatch = Regex.Match(content, @"<code[^>]*>(.*?)</code>", RegexOptions.Singleline);
                            if (codeMatch.Success)
                            {
                                codeContent = codeMatch.Groups[1].Value;
                                
                                // 检查是否有语言类
                                var classMatch = Regex.Match(codeMatch.Value, @"class=""language-([^""]+)""");
                                if (classMatch.Success)
                                {
                                    string language = classMatch.Groups[1].Value;
                                    codeContent = $"```{language}\n{codeContent}";
                                }
                            }
                            
                            // 处理HTML实体，特别是代码中的注释符号
                            codeContent = codeContent
                                .Replace("&lt;", "<")
                                .Replace("&gt;", ">")
                                .Replace("&amp;", "&")
                                .Replace("&quot;", "\"")
                                .Replace("&apos;", "'")
                                .Replace("&nbsp;", " ")
                                .Replace("&#x2F;", "/")
                                .Replace("&#x27;", "'")
                                .Replace("&#x2F;&#x2F;", "//") // 特别处理注释符号
                                .Replace("&sol;&sol;", "//")
                                .Replace("&sol;", "/");
                                
                            // 直接解码所有HTML实体
                            codeContent = System.Net.WebUtility.HtmlDecode(codeContent);
                            
                            elements.Add(new HtmlElement
                            {
                                Type = ElementType.CodeBlock,
                                Content = codeContent
                            });
                            break;
                        case "blockquote":
                            elements.Add(new HtmlElement
                            {
                                Type = ElementType.Blockquote,
                                Content = StripHtmlTags(content)
                            });
                            break;
                        case "table":
                            elements.Add(new HtmlElement
                            {
                                Type = ElementType.Table,
                                Content = content // 保留表格的HTML标签
                            });
                            break;
                        case "img":
                            // 处理img标签，提取src属性
                            var srcMatch = Regex.Match(match.Value, @"src=""([^""]+)""");
                            if (srcMatch.Success)
                            {
                                elements.Add(new HtmlElement
                                {
                                    Type = ElementType.Image,
                                    Content = srcMatch.Groups[1].Value
                                });
                            }
                            break;
                        case "ul":
                        case "ol":
                            // 处理列表
                            var listItems = Regex.Matches(content, @"<li[^>]*>(.*?)</li>", RegexOptions.Singleline);
                            foreach (Match item in listItems)
                            {
                                elements.Add(new HtmlElement
                                {
                                    Type = ElementType.ListItem,
                                    Content = "• " + StripHtmlTags(item.Groups[1].Value)
                                });
                            }
                            break;
                        case "li":
                            // 直接处理列表项
                            elements.Add(new HtmlElement
                            {
                                Type = ElementType.ListItem,
                                Content = "• " + StripHtmlTags(content)
                            });
                            break;
                    }
                }
                
                // 如果没有解析到任何元素，尝试使用更简单的方法
                if (elements.Count == 0)
                {
                    // 尝试提取纯文本并作为段落
                    string plainText = StripHtmlTags(contentHtml);
                    if (!string.IsNullOrWhiteSpace(plainText))
                    {
                        elements.Add(new HtmlElement
                        {
                            Type = ElementType.Paragraph,
                            Content = plainText
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTML解析错误: {ex.Message}");
                
                // 解析失败时，添加一个错误信息元素
                elements.Add(new HtmlElement
                {
                    Type = ElementType.Paragraph,
                    Content = $"HTML解析错误: {ex.Message}"
                });
            }
            
            return elements;
        }
        
        /// <summary>
        /// 处理列表项，将ul/li转换为带有特殊标记的段落
        /// </summary>
        private string ProcessListItems(string html)
        {
            // 处理无序列表
            html = Regex.Replace(html, @"<ul[^>]*>(.*?)</ul>", m => {
                string listContent = m.Groups[1].Value;
                string result = "";
                
                // 提取所有列表项
                var listItems = Regex.Matches(listContent, @"<li[^>]*>(.*?)</li>", RegexOptions.Singleline);
                foreach (Match item in listItems)
                {
                    result += $"<p>• {item.Groups[1].Value}</p>";
                }
                
                return result;
            }, RegexOptions.Singleline);
            
            // 处理有序列表
            html = Regex.Replace(html, @"<ol[^>]*>(.*?)</ol>", m => {
                string listContent = m.Groups[1].Value;
                string result = "";
                
                // 提取所有列表项
                var listItems = Regex.Matches(listContent, @"<li[^>]*>(.*?)</li>", RegexOptions.Singleline);
                for (int i = 0; i < listItems.Count; i++)
                {
                    result += $"<p>{i + 1}. {listItems[i].Groups[1].Value}</p>";
                }
                
                return result;
            }, RegexOptions.Singleline);
            
            return html;
        }
        
        /// <summary>
        /// 去除HTML标签并解码HTML实体，确保正确处理编码
        /// </summary>
        private string StripHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;
                
            // 使用编码处理器确保文本使用正确的编码
            html = _encodingHandler.EnsureUtf8(html);
                
            // 先替换常见HTML实体
            string result = html
                .Replace("&quot;", "\"")
                .Replace("&apos;", "'")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&amp;", "&")
                .Replace("&nbsp;", " ");
                
            // 去除HTML标签
            result = Regex.Replace(result, @"<[^>]+>", string.Empty);
            
            return result;
        }
    }
}