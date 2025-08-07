using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Md2Image.ImageGeneration.Renderers
{
    /// <summary>
    /// 元素渲染器基类
    /// </summary>
    public abstract class BaseElementRenderer : IElementRenderer
    {
        /// <summary>
        /// 默认字体
        /// </summary>
        protected string FontFamily { get; set; } = "Microsoft YaHei, Arial, sans-serif";
        
        /// <summary>
        /// 默认字体大小
        /// </summary>
        protected float FontSize { get; set; } = 16;
        
        /// <summary>
        /// 默认行高
        /// </summary>
        protected float LineHeight { get; set; } = 1.6f;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fontFamily">字体</param>
        public BaseElementRenderer(string? fontFamily = null)
        {
            if (!string.IsNullOrEmpty(fontFamily))
            {
                FontFamily = fontFamily;
            }
        }
        
        /// <summary>
        /// 渲染元素
        /// </summary>
        public abstract float Render(SKCanvas canvas, string content, float x, float y, float width);
        
        /// <summary>
        /// 测量元素高度
        /// </summary>
        public abstract float MeasureHeight(string content, float width);
        
        /// <summary>
        /// 测量文本高度
        /// </summary>
        protected float MeasureTextHeight(string text, float maxWidth, float fontSize, float lineHeight)
        {
            if (string.IsNullOrEmpty(text))
                return 0;
                
            using (var paint = new SKPaint
            {
                TextSize = fontSize,
                IsAntialias = true
            })
            {
                // 简化的文本换行计算
                var words = text.Split(' ');
                var lines = new List<string>();
                var currentLine = new StringBuilder();
                
                foreach (var word in words)
                {
                    var testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
                    var width = paint.MeasureText(testLine);
                    
                    if (width <= maxWidth)
                    {
                        currentLine.Append(currentLine.Length == 0 ? word : " " + word);
                    }
                    else
                    {
                        lines.Add(currentLine.ToString());
                        currentLine.Clear();
                        currentLine.Append(word);
                    }
                }
                
                if (currentLine.Length > 0)
                {
                    lines.Add(currentLine.ToString());
                }
                
                return lines.Count * fontSize * lineHeight;
            }
        }
    }
}