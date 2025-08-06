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
        /// 渲染代码块
        /// </summary>
        public override float Render(SKCanvas canvas, string code, float x, float y, float width)
        {
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
            
            // 绘制代码文本
            using (var paint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = FontSize,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName(FontFamily)
            })
            {
                // 简化的文本换行绘制
                var lines = code.Split('\n');
                float lineY = y + FontSize + _padding;
                
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
                        int charPos = 0;
                        int lastPos = 0;
                        float lineWidth = 0;
                        
                        while (charPos < line.Length)
                        {
                            char c = line[charPos];
                            float charWidth = paint.MeasureText(c.ToString());
                            
                            if (lineWidth + charWidth > width - _padding * 2)
                            {
                                // 需要换行
                                canvas.DrawText(line.Substring(lastPos, charPos - lastPos), x + _padding, lineY, paint);
                                lastPos = charPos;
                                lineY += FontSize * LineHeight;
                                lineWidth = 0;
                            }
                            else
                            {
                                lineWidth += charWidth;
                                charPos++;
                            }
                        }
                        
                        // 绘制最后一行
                        if (lastPos < line.Length)
                        {
                            canvas.DrawText(line.Substring(lastPos), x + _padding, lineY, paint);
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