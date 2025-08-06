using System;
using SkiaSharp;

namespace Md2Image.ImageGeneration
{
    /// <summary>
    /// HTML渲染配置，用于配置HTML渲染的各种参数
    /// </summary>
    public class HtmlRenderingConfig
    {
        /// <summary>
        /// 默认字体族
        /// </summary>
        public string DefaultFontFamily { get; set; } = "Arial, 'Microsoft YaHei', sans-serif";
        
        /// <summary>
        /// 默认字体大小
        /// </summary>
        public float DefaultFontSize { get; set; } = 16;
        
        /// <summary>
        /// 默认行高
        /// </summary>
        public float DefaultLineHeight { get; set; } = 1.6f;
        
        /// <summary>
        /// 默认段落间距
        /// </summary>
        public float DefaultParagraphSpacing { get; set; } = 16;
        
        /// <summary>
        /// 默认边距
        /// </summary>
        public float DefaultMargin { get; set; } = 20;
        
        /// <summary>
        /// 代码块字体族
        /// </summary>
        public string CodeFontFamily { get; set; } = "Consolas, 'Source Code Pro', monospace";
        
        /// <summary>
        /// 代码块字体大小
        /// </summary>
        public float CodeFontSize { get; set; } = 14;
        
        /// <summary>
        /// 代码块行高
        /// </summary>
        public float CodeLineHeight { get; set; } = 1.5f;
        
        /// <summary>
        /// 代码块背景色
        /// </summary>
        public SKColor CodeBackgroundColor { get; set; } = new SKColor(246, 248, 250);
        
        /// <summary>
        /// 标题1字体大小
        /// </summary>
        public float H1FontSize { get; set; } = 24;
        
        /// <summary>
        /// 标题2字体大小
        /// </summary>
        public float H2FontSize { get; set; } = 20;
        
        /// <summary>
        /// 标题字体族
        /// </summary>
        public string HeadingFontFamily { get; set; } = "Arial, 'Microsoft YaHei', sans-serif";
        
        /// <summary>
        /// 引用块字体颜色
        /// </summary>
        public SKColor BlockquoteColor { get; set; } = SKColors.Gray;
        
        /// <summary>
        /// 引用块边框颜色
        /// </summary>
        public SKColor BlockquoteBorderColor { get; set; } = SKColors.LightGray;
        
        /// <summary>
        /// 背景色
        /// </summary>
        public SKColor BackgroundColor { get; set; } = SKColors.White;
        
        /// <summary>
        /// 文本颜色
        /// </summary>
        public SKColor TextColor { get; set; } = SKColors.Black;
        
        /// <summary>
        /// 创建默认配置
        /// </summary>
        /// <returns>默认配置实例</returns>
        public static HtmlRenderingConfig CreateDefault()
        {
            return new HtmlRenderingConfig();
        }
        
        /// <summary>
        /// 创建微信风格配置
        /// </summary>
        /// <returns>微信风格配置实例</returns>
        public static HtmlRenderingConfig CreateWechatStyle()
        {
            return new HtmlRenderingConfig
            {
                DefaultFontFamily = "'Microsoft YaHei', 'Hiragino Sans GB', sans-serif",
                DefaultFontSize = 15,
                DefaultLineHeight = 1.7f,
                BackgroundColor = SKColors.White,
                TextColor = new SKColor(51, 51, 51),
                H1FontSize = 22,
                H2FontSize = 18,
                CodeFontSize = 13,
                DefaultMargin = 24
            };
        }
    }
}