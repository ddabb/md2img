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
                IsAntialias = true
            })
            {
                // 简化的文本换行绘制
                var words = text.Split(' ');
                var currentLine = new StringBuilder();
                float lineY = y + FontSize + 5;
                
                foreach (var word in words)
                {
                    var testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
                    var lineWidth = paint.MeasureText(testLine);
                    
                    if (lineWidth <= width - _leftIndent)
                    {
                        currentLine.Append(currentLine.Length == 0 ? word : " " + word);
                    }
                    else
                    {
                        canvas.DrawText(currentLine.ToString(), x + _leftIndent, lineY, paint);
                        lineY += FontSize * LineHeight;
                        currentLine.Clear();
                        currentLine.Append(word);
                    }
                }
                
                if (currentLine.Length > 0)
                {
                    canvas.DrawText(currentLine.ToString(), x + _leftIndent, lineY, paint);
                    lineY += FontSize * LineHeight;
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