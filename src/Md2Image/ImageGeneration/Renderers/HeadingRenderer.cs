using SkiaSharp;

namespace Md2Image.ImageGeneration.Renderers
{
    /// <summary>
    /// 标题渲染器
    /// </summary>
    public class HeadingRenderer : BaseElementRenderer
    {
        private readonly int _level;
        private readonly float _fontSize;
        private readonly bool _showUnderline;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="level">标题级别</param>
        /// <param name="fontFamily">字体</param>
        public HeadingRenderer(int level, string? fontFamily = null) : base(fontFamily)
        {
            _level = level;
            _fontSize = level == 1 ? 24 : 20;
            _showUnderline = level == 1;
        }
        
        /// <summary>
        /// 渲染标题
        /// </summary>
        public override float Render(SKCanvas canvas, string text, float x, float y, float width)
        {
            using (var paint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = _fontSize,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName(FontFamily, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
            })
            {
                // 绘制文本
                canvas.DrawText(text, x, y + _fontSize, paint);
                
                // 绘制下划线（仅一级标题）
                if (_showUnderline)
                {
                    using (var linePaint = new SKPaint
                    {
                        Color = SKColors.LightGray,
                        StrokeWidth = 1
                    })
                    {
                        canvas.DrawLine(x, y + _fontSize + 8, x + width, y + _fontSize + 8, linePaint);
                    }
                }
                
                return y + _fontSize + (_showUnderline ? 24 : 20);
            }
        }
        
        /// <summary>
        /// 测量标题高度
        /// </summary>
        public override float MeasureHeight(string content, float width)
        {
            float textHeight = MeasureTextHeight(content, width, _fontSize, 1.4f);
            return textHeight + (_showUnderline ? 24 : 20);
        }
    }
}