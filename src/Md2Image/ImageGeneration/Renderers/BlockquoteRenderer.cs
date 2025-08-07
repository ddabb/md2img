using SkiaSharp;
using System;
using System.Text;

namespace Md2Image.ImageGeneration.Renderers
{
    /// <summary>
    /// 引用块渲染器
    /// </summary>
    public class BlockquoteRenderer : BaseElementRenderer
    {
        private readonly float _leftIndent;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fontFamily">字体</param>
        public BlockquoteRenderer(string? fontFamily = null) : base(fontFamily)
        {
            _leftIndent = 20;
        }
        
        /// <summary>
        /// 渲染引用块
        /// </summary>
        public override float Render(SKCanvas canvas, string text, float x, float y, float width)
        {
            // 首先计算文本的高度和位置
            float textHeight = MeasureTextHeight(text, width - _leftIndent, FontSize, LineHeight);
            float textStartY = y + FontSize + 2; // 文本起始位置
            
            // 计算实际文本结束位置（考虑到最后一行的高度）
            float actualTextEndY = textStartY + textHeight;
            
            // 计算背景区域，减小背景阴影区域的大小
            float bgPaddingTop = 2; // 减小顶部padding
            float bgPaddingBottom = 5; // 减小底部padding
            var bgRect = new SKRect(x, y - bgPaddingTop, x + width, actualTextEndY + bgPaddingBottom);
            
            // 绘制背景
            using (var bgPaint = new SKPaint
            {
                Color = new SKColor(245, 245, 245), // 浅灰色背景
                IsAntialias = true
            })
            {
                canvas.DrawRect(bgRect, bgPaint);
            }
            
            // 绘制左侧竖线
            using (var linePaint = new SKPaint
            {
                Color = new SKColor(120, 120, 120), // 使用更浅的灰色
                StrokeWidth = 4 // 稍微减小线宽
            })
            {
                // 竖线高度应该与文本的高度更接近
                float lineTop = textStartY - FontSize * 0.5f; // 文本起始位置上方一点
                float lineBottom = textStartY + textHeight - FontSize * 0.5f; // 文本结束位置
                canvas.DrawLine(x + 4, lineTop, x + 4, lineBottom, linePaint);
            }
            
            // 绘制文本
            using (var paint = new SKPaint
            {
                Color = new SKColor(50, 50, 50), // 更深的文本颜色
                TextSize = FontSize,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName(FontFamily)
            })
            {
                // 处理中文和英文混合文本
                var lines = text.Split('\n');
                float lineY = textStartY; // 使用计算好的文本起始位置
            
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    lineY += FontSize * LineHeight * 0.7f; // 减少空行间距
                    continue;
                }
                
                // 处理长行
                float availableWidth = width - _leftIndent;
                if (paint.MeasureText(line) <= availableWidth)
                {
                    canvas.DrawText(line, x + _leftIndent, lineY, paint);
                    lineY += FontSize * LineHeight;
                }
                else
                {
                    // 需要换行的情况
                    int startIndex = 0;
                    int currentIndex = 0;
                    float currentWidth = 0;
                    StringBuilder currentLine = new StringBuilder();
                    
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
                            canvas.DrawText(segment, x + _leftIndent, lineY, paint);
                            lineY += FontSize * LineHeight;
                            
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
                    if (startIndex < line.Length)
                    {
                        string segment = line.Substring(startIndex);
                        canvas.DrawText(segment, x + _leftIndent, lineY, paint);
                        lineY += FontSize * LineHeight;
                    }
                }
            }
                
                return lineY + 10;
            }
        }
        
        /// <summary>
        /// 测量引用块高度
        /// </summary>
        public override float MeasureHeight(string content, float width)
        {
            float textHeight = MeasureTextHeight(content, width - _leftIndent, FontSize, LineHeight);
            return textHeight; 
        }
    }
}