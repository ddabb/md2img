using SkiaSharp;
using System;

namespace Md2Image.ImageGeneration.Renderers
{
    /// <summary>
    /// 代码块渲染器
    /// </summary>
    public class CodeBlockRenderer : BaseElementRenderer
    {
        private readonly float _padding;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fontFamily">字体</param>
        public CodeBlockRenderer(string fontFamily = "Microsoft YaHei, Consolas, monospace") : base(fontFamily)
        {
            FontSize = 14;
            LineHeight = 1.5f;
            _padding = 10;
        }
        
        /// <summary>
        /// 获取代码语言
        /// </summary>
        private string GetCodeLanguage(ref string code)
        {
            string language = "";
            
            // 检查第一行是否包含语言标识
            int firstLineEnd = code.IndexOf('\n');
            if (firstLineEnd > 0)
            {
                string firstLine = code.Substring(0, firstLineEnd).Trim();
                if (firstLine.StartsWith("```"))
                {
                    language = firstLine.Substring(3).Trim();
                    code = code.Substring(firstLineEnd + 1).Trim();
                }
            }
            
            // 处理HTML实体编码的代码
            code = System.Net.WebUtility.HtmlDecode(code);
            
            return language;
        }
        
        /// <summary>
        /// 渲染代码块
        /// </summary>
        public override float Render(SKCanvas canvas, string code, float x, float y, float width)
        {
            // 提取代码语言（如果有）
            string language = GetCodeLanguage(ref code);
            
            // 确保代码中的注释符号正确显示
            code = code.Replace("&lt;", "<")
                       .Replace("&gt;", ">")
                       .Replace("&amp;", "&")
                       .Replace("&quot;", "\"")
                       .Replace("&apos;", "'")
                       .Replace("&nbsp;", " ")
                       .Replace("&#x2F;", "/")
                       .Replace("&#x27;", "'")
                       .Replace("&#x2F;&#x2F;", "//")
                       .Replace("&sol;&sol;", "//")
                       .Replace("&sol;", "/");
            
            // 直接解码所有HTML实体
            code = System.Net.WebUtility.HtmlDecode(code);
            
            // 确保注释符号正确显示
            code = code.Replace("/ /", "//");
            
            // 确保正确处理中文字符
            try
            {
                // 确保已注册编码提供程序
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                
                // 使用UTF-8编码处理代码内容
                code = System.Text.Encoding.UTF8.GetString(
                    System.Text.Encoding.UTF8.GetBytes(code)
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理代码块编码时出错: {ex.Message}");
            }
            
            // 分割代码行
            var codeLines = code.Split('\n');
            
            // 创建画笔用于测量文本
            using var measurePaint = new SKPaint
            {
                TextSize = FontSize,
                Typeface = SKTypeface.FromFamilyName(FontFamily)
            };
            
            // 计算语言标签高度
            float languageHeight = !string.IsNullOrEmpty(language) ? FontSize * LineHeight : 0;
            
            // 计算每行代码的实际高度（考虑换行）
            float totalCodeHeight = 0;
            foreach (var line in codeLines)
            {
                if (measurePaint.MeasureText(line) <= width - _padding * 2)
                {
                    // 单行
                    totalCodeHeight += FontSize * LineHeight;
                }
                else
                {
                    // 需要换行的情况，计算实际行数
                    float lineWidth = 0;
                    int wrappedLines = 1;
                    
                    foreach (char c in line)
                    {
                        float charWidth = measurePaint.MeasureText(c.ToString());
                        if (lineWidth + charWidth > width - _padding * 2)
                        {
                            wrappedLines++;
                            lineWidth = charWidth;
                        }
                        else
                        {
                            lineWidth += charWidth;
                        }
                    }
                    
                    totalCodeHeight += wrappedLines * FontSize * LineHeight;
                }
            }
            
            // 设置内边距
            float bgPaddingTop = 10;
            float bgPaddingBottom = 20; // 增加底部内边距，确保内容完整显示
            
            // 计算总高度
            float totalHeight = languageHeight + totalCodeHeight + bgPaddingTop + bgPaddingBottom;
            
            // 计算背景矩形
            var bgRect = new SKRect(x, y, x + width, y + totalHeight);
            
            // 绘制背景
            using (var bgPaint = new SKPaint
            {
                Color = new SKColor(246, 248, 250), // 浅灰色背景
                IsAntialias = true
            })
            {
                canvas.DrawRect(bgRect, bgPaint);
            }
            
            // 绘制边框
            using (var borderPaint = new SKPaint
            {
                Color = new SKColor(225, 228, 232),
                IsAntialias = true,
                IsStroke = true,
                StrokeWidth = 1
            })
            {
                canvas.DrawRect(bgRect, borderPaint);
            }
            
            // 计算文本起始位置
            float currentY = y + bgPaddingTop + FontSize; // 从顶部内边距开始，加上字体大小（基线位置）
            
            // 如果有语言标识，绘制语言标签
            if (!string.IsNullOrEmpty(language))
            {
                using (var langPaint = new SKPaint
                {
                    Color = new SKColor(100, 100, 100),
                    TextSize = FontSize * 0.8f,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName(FontFamily)
                })
                {
                    canvas.DrawText(language, x + _padding, currentY, langPaint);
                    currentY += FontSize * LineHeight;
                }
            }
            
            // 绘制代码文本
            using (var paint = new SKPaint
            {
                Color = new SKColor(36, 41, 46), // GitHub代码颜色
                TextSize = FontSize,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName(FontFamily),
                SubpixelText = true, // 启用亚像素渲染，提高文本清晰度
                FilterQuality = SKFilterQuality.High // 使用高质量过滤
            })
            {
                foreach (var line in codeLines)
                {
                    // 处理长行
                    float availableWidth = width - _padding * 2;
                    if (paint.MeasureText(line) <= availableWidth)
                    {
                        canvas.DrawText(line, x + _padding, currentY, paint);
                        currentY += FontSize * LineHeight;
                    }
                    else
                    {
                        // 需要换行的情况
                        int currentIndex = 0;
                        float currentWidth = 0;
                        System.Text.StringBuilder currentLine = new System.Text.StringBuilder();
                        
                        while (currentIndex < line.Length)
                        {
                            char c = line[currentIndex];
                            float charWidth = paint.MeasureText(c.ToString());
                            
                            // 检查添加这个字符是否会超出可用宽度
                            if (currentWidth + charWidth > availableWidth)
                            {
                                // 如果当前行为空（说明单个字符就超出了宽度），至少添加一个字符
                                if (currentLine.Length == 0)
                                {
                                    currentLine.Append(c);
                                    currentIndex++;
                                }
                                
                                // 需要换行
                                string segment = currentLine.ToString();
                                canvas.DrawText(segment, x + _padding, currentY, paint);
                                currentY += FontSize * LineHeight;
                                
                                // 重置当前行
                                currentLine.Clear();
                                currentWidth = 0;
                                continue;
                            }
                            
                            // 添加字符到当前行
                            currentLine.Append(c);
                            currentWidth += charWidth;
                            currentIndex++;
                        }
                        
                        // 绘制最后一行
                        if (currentLine.Length > 0)
                        {
                            string segment = currentLine.ToString();
                            canvas.DrawText(segment, x + _padding, currentY, paint);
                            currentY += FontSize * LineHeight;
                        }
                    }
                }
            }
            
            // 返回实际高度
            return y + totalHeight;
        }
        
        /// <summary>
        /// 测量代码块高度
        /// </summary>
        public override float MeasureHeight(string content, float width)
        {
            // 提取代码语言（如果有）
            string tempContent = content;
            string language = GetCodeLanguage(ref tempContent);
            
            // 计算语言标签高度
            float languageHeight = !string.IsNullOrEmpty(language) ? FontSize * LineHeight : 0;
            
            // 分割代码行
            var codeLines = tempContent.Split('\n');
            
            // 创建画笔用于测量文本
            using var measurePaint = new SKPaint
            {
                TextSize = FontSize,
                Typeface = SKTypeface.FromFamilyName(FontFamily)
            };
            
            // 计算每行代码的实际高度（考虑换行）
            float totalCodeHeight = 0;
            foreach (var line in codeLines)
            {
                if (measurePaint.MeasureText(line) <= width - _padding * 2)
                {
                    // 单行
                    totalCodeHeight += FontSize * LineHeight;
                }
                else
                {
                    // 需要换行的情况，计算实际行数
                    float lineWidth = 0;
                    int wrappedLines = 1;
                    
                    foreach (char c in line)
                    {
                        float charWidth = measurePaint.MeasureText(c.ToString());
                        if (lineWidth + charWidth > width - _padding * 2)
                        {
                            wrappedLines++;
                            lineWidth = charWidth;
                        }
                        else
                        {
                            lineWidth += charWidth;
                        }
                    }
                    
                    totalCodeHeight += wrappedLines * FontSize * LineHeight;
                }
            }
            
            // 设置内边距
            float bgPaddingTop = 10;
            float bgPaddingBottom = 20;
            
            // 返回总高度
            return languageHeight + totalCodeHeight + bgPaddingTop + bgPaddingBottom;
        }
    }
}