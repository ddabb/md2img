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
        public BlockquoteRenderer(string fontFamily = null) : base(fontFamily)
        {
            _leftIndent = 20;
        }
        
        /// <summary>
        /// 渲染引用块
        /// </summary>
        public override float Render(SKCanvas canvas, string text, float x, float y, float width)
        {
            // 绘制左侧竖线
            using (var linePaint = new SKPaint
            {
                Color = SKColors.LightGray,
                StrokeWidth = 4
            })
            {
                float textHeight = MeasureTextHeight(text, width - _leftIndent, FontSize, LineHeight);
                canvas.DrawLine(x + 4, y, x + 4, y + textHeight + 10, linePaint);
            }
            
            // 绘制文本
            using (var paint = new SKPaint
            {
                Color = SKColors.Gray,
                TextSize = FontSize,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName(FontFamily)
            })
            {
                // 处理中文和英文混合文本
                var lines = text.Split('\n');
                float lineY = y + FontSize + 5;
                
                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        lineY += FontSize * LineHeight;
                        continue;
                    }
                    
                    // 处理长行
                    if (paint.MeasureText(line) <= width - _leftIndent)
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
                        
                        while (currentIndex < line.Length)
                        {
                            char c = line[currentIndex];
                            float charWidth = paint.MeasureText(c.ToString());
                            
                            if (currentWidth + charWidth > width - _leftIndent)
                            {
                                // 需要换行
                                string segment = line.Substring(startIndex, currentIndex - startIndex);
                                canvas.DrawText(segment, x + _leftIndent, lineY, paint);
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
            return textHeight + 30;
        }
    }
}