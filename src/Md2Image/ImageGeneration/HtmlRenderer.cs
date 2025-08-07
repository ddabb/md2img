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
    /// HTML渲染器，负责将HTML内容渲染为位图
    /// </summary>
    public class HtmlRenderer
    {
        // 默认字体，确保支持中文
        private string _defaultFontFamily = "Microsoft YaHei, Arial, sans-serif";
        
        // 默认边距
        private readonly float _defaultMargin = 20;
        
        // 元素渲染器字典
        private readonly Dictionary<ElementType, IElementRenderer> _renderers;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public HtmlRenderer()
        {
            // 确保已注册字体
            EnsureFontsRegistered();
            
            // 初始化渲染器
            _renderers = new Dictionary<ElementType, IElementRenderer>
            {
                { ElementType.Heading1, new HeadingRenderer(1, _defaultFontFamily) },
                { ElementType.Heading2, new HeadingRenderer(2, _defaultFontFamily) },
                { ElementType.Paragraph, new ParagraphRenderer(_defaultFontFamily) },
                { ElementType.Image, new ImageRenderer() },
                { ElementType.CodeBlock, new CodeBlockRenderer(_defaultFontFamily) },
                { ElementType.Blockquote, new BlockquoteRenderer(_defaultFontFamily) },
                { ElementType.Table, new TableRenderer(_defaultFontFamily) },
                { ElementType.ListItem, new ParagraphRenderer(_defaultFontFamily) }
            };
        }
        
        /// <summary>
        /// 确保已注册所需的字体
        /// </summary>
        private void EnsureFontsRegistered()
        {
            try
            {
                // 尝试使用系统字体
                var fontManager = SKFontManager.Default;
                var availableFontFamilies = fontManager.GetFontFamilies();
                
                // 检查是否有中文字体可用
                bool hasChineseFont = false;
                foreach (var family in availableFontFamilies)
                {
                    if (family.Contains("SimSun") || family.Contains("Microsoft YaHei") || 
                        family.Contains("SimHei") || family.Contains("NSimSun") ||
                        family.Contains("FangSong") || family.Contains("KaiTi") ||
                        family.Contains("STHeiti") || family.Contains("STKaiti") ||
                        family.Contains("STSong") || family.Contains("STFangsong"))
                    {
                        hasChineseFont = true;
                        _defaultFontFamily = family;
                        break;
                    }
                }
                
                // 如果没有找到中文字体，使用默认字体
                if (!hasChineseFont)
                {
                    Console.WriteLine("警告：未找到中文字体，将使用默认字体。可能导致中文显示为方框。");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"注册字体时出错: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 将HTML内容渲染为位图
        /// </summary>
        /// <param name="htmlContent">HTML内容</param>
        /// <param name="options">转换选项</param>
        /// <returns>渲染后的位图列表</returns>
        public virtual Task<IList<SKBitmap>> RenderHtmlAsync(string htmlContent, ConversionOptions options)
        {
            // 解析HTML内容
            var htmlElements = ParseHtmlSequentially(htmlContent);
            
            // 计算内容总高度
            float totalHeight = CalculateTotalHeight(htmlElements, options.Width);
            
            // 创建位图
            var bitmap = new SKBitmap(options.Width, (int)Math.Ceiling(totalHeight));
            using (var canvas = new SKCanvas(bitmap))
            {
                // 填充背景色
                canvas.Clear(SKColors.White);
                
                // 渲染HTML元素
                RenderElements(canvas, htmlElements, options.Width);
            }
            
            // 分页处理
            return Task.FromResult<IList<SKBitmap>>(SplitIntoPages(bitmap, options.MaxHeight));
        }
        
        /// <summary>
        /// 按顺序解析HTML内容为元素列表
        /// </summary>
        protected virtual List<HtmlElement> ParseHtmlSequentially(string htmlContent)
        {
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
                string pattern = @"<(h1|h2|p|pre|blockquote|table|img)[^>]*>(.*?)</\1>|<img[^>]*>";
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
        /// 去除HTML标签并解码HTML实体
        /// </summary>
        private string StripHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;
                
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
        
        /// <summary>
        /// 计算内容总高度
        /// </summary>
        private float CalculateTotalHeight(List<HtmlElement> elements, int width)
        {
            float totalHeight = _defaultMargin * 2; // 上下边距
            
            foreach (var element in elements)
            {
                if (_renderers.TryGetValue(element.Type, out var renderer))
                {
                    totalHeight += renderer.MeasureHeight(element.Content ?? string.Empty, width - _defaultMargin * 2);
                }
            }
            
            return totalHeight;
        }
        
        /// <summary>
        /// 渲染HTML元素
        /// </summary>
        private void RenderElements(SKCanvas canvas, List<HtmlElement> elements, int width)
        {
            float y = _defaultMargin;
            
            foreach (var element in elements)
            {
                if (_renderers.TryGetValue(element.Type, out var renderer))
                {
                    y = renderer.Render(canvas, element.Content ?? string.Empty, _defaultMargin, y, width - _defaultMargin * 2);
                }
            }
        }
        
        /// <summary>
        /// 将位图分割为多页
        /// </summary>
        private IList<SKBitmap> SplitIntoPages(SKBitmap bitmap, int maxHeight)
        {
            var result = new List<SKBitmap>();
            
            // 如果图片高度小于最大高度，直接返回原图
            if (bitmap.Height <= maxHeight)
            {
                result.Add(bitmap);
                return result;
            }
            
            int pageCount = Math.Max(1, bitmap.Height / maxHeight + (bitmap.Height % maxHeight > 0 ? 1 : 0));
            for (int i = 0; i < pageCount; i++)
            {
                int pageHeight = Math.Min(maxHeight, bitmap.Height - i * maxHeight);
                if (pageHeight <= 0) continue;
                
                var pageBitmap = new SKBitmap(bitmap.Width, pageHeight);
                using (var canvas = new SKCanvas(pageBitmap))
                {
                    var srcRect = new SKRect(0, i * maxHeight, bitmap.Width, i * maxHeight + pageHeight);
                    var destRect = new SKRect(0, 0, bitmap.Width, pageHeight);
                    canvas.DrawBitmap(bitmap, srcRect, destRect);
                }
                
                result.Add(pageBitmap);
            }
            
            return result;
        }
    }
}