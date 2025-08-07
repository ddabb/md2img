using SkiaSharp;
using System;
using System.Text;

namespace Md2Image.ImageGeneration.Renderers
{
    /// <summary>
    /// 段落渲染器
    /// </summary>
    public class ParagraphRenderer : BaseElementRenderer
    {
        private readonly float _paragraphSpacing;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fontFamily">字体</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="lineHeight">行高</param>
        /// <param name="paragraphSpacing">段落间距</param>
        public ParagraphRenderer(string? fontFamily = null, float fontSize = 16, float lineHeight = 1.6f, float paragraphSpacing = 16) : base(fontFamily)
        {
            FontSize = fontSize;
            LineHeight = lineHeight;
            _paragraphSpacing = paragraphSpacing;
        }
        
        /// <summary>
        /// 渲染段落
        /// </summary>
        public override float Render(SKCanvas canvas, string text, float x, float y, float width)
        {
            if (string.IsNullOrEmpty(text))
            {
                return y + _paragraphSpacing;
            }
            
            using (var paint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = FontSize,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName(FontFamily)
            })
            {
                // 简单的按行渲染方法
                float lineY = y + FontSize;
                
                // 按行分割文本
                string[] lines = text.Split('\n');
                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        lineY += FontSize * LineHeight;
                        continue;
                    }
                    
                    // 如果行宽度小于可用宽度，直接绘制
                    if (paint.MeasureText(line) <= width)
                    {
                        canvas.DrawText(line, x, lineY, paint);
                        lineY += FontSize * LineHeight;
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
                            
                            if (lineWidth + charWidth > width)
                            {
                                // 需要换行
                                if (lastPos == charPos)
                                {
                                    // 至少绘制一个字符
                                    canvas.DrawText(line.Substring(lastPos, 1), x, lineY, paint);
                                    lastPos = charPos + 1;
                                }
                                else
                                {
                                    canvas.DrawText(line.Substring(lastPos, charPos - lastPos), x, lineY, paint);
                                    lastPos = charPos;
                                }
                                
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
                            canvas.DrawText(line.Substring(lastPos), x, lineY, paint);
                            lineY += FontSize * LineHeight;
                        }
                    }
                }
                
                return lineY + _paragraphSpacing - FontSize * LineHeight;
            }
        }
        
        /// <summary>
        /// 测量段落高度
        /// </summary>
        public override float MeasureHeight(string content, float width)
        {
            if (string.IsNullOrEmpty(content))
                return _paragraphSpacing;
                
            float textHeight = MeasureTextHeight(content, width, FontSize, LineHeight);
            return textHeight + _paragraphSpacing;
        }
    }
}