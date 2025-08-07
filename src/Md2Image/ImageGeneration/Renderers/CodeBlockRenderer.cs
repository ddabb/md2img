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
        public CodeBlockRenderer(string fontFamily = "Consolas, monospace") : base(fontFamily)
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
            
            return language;
        }
        
        /// <summary>
        /// 渲染代码块
        /// </summary>
        public override float Render(SKCanvas canvas, string code, float x, float y, float width)
        {
            // 提取代码语言（如果有）
            string language = GetCodeLanguage(ref code);
            
            // 绘制背景
            using (var bgPaint = new SKPaint
            {
                Color = new SKColor(246, 248, 250), // 浅灰色背景
                IsAntialias = true
            })
            {
                var bgRect = new SKRect(x, y, x + width, y + MeasureTextHeight(code, width - _padding * 2, FontSize, LineHeight) + _padding * 2);
                canvas.DrawRect(bgRect, bgPaint);
            }
            
            // 如果有语言标识，绘制语言标签
            float currentY = y + FontSize + _padding;
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
                Color = SKColors.Black,
                TextSize = FontSize,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName(FontFamily)
            })
            {
                // 处理代码行
                var lines = code.Split('\n');
                float lineY = currentY;
                
                foreach (var line in lines)
                {
                    // 处理长行
                    if (paint.MeasureText(line) <= width - _padding * 2)
                    {
                        canvas.DrawText(line, x + _padding, lineY, paint);
                    }
                    else
                    {
                        // 需要换行的情况
                        int startIndex = 0;
                        int currentIndex = 0;
                        float currentWidth = 0;
                        
                        while (currentIndex < line.Length)
                        {
                            char c = line[currentIndex];
                            float charWidth = paint.MeasureText(c.ToString());
                            
                            if (currentWidth + charWidth > width - _padding * 2)
                            {
                                // 需要换行
                                string segment = line.Substring(startIndex, currentIndex - startIndex);
                                canvas.DrawText(segment, x + _padding, lineY, paint);
                                lineY += FontSize * LineHeight;
                                startIndex = currentIndex;
                                currentWidth = 0;
                            }
                            
                            currentWidth += charWidth;
                            currentIndex++;
                        }
                        
                        // 绘制最后一行
                        if (startIndex < line.Length)
                        {
                            string segment = line.Substring(startIndex);
                            canvas.DrawText(segment, x + _padding, lineY, paint);
                        }
                    }
                    
                    lineY += FontSize * LineHeight;
                }
                
                return lineY + _padding;
            }
        }
        
        /// <summary>
        /// 测量代码块高度
        /// </summary>
        public override float MeasureHeight(string content, float width)
        {
            float textHeight = MeasureTextHeight(content, width - _padding * 2, FontSize, LineHeight);
            return textHeight + _padding * 2;
        }
    }
}