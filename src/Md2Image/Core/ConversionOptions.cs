using System;
using System.Drawing;

namespace Md2Image.Core
{
    /// <summary>
    /// Markdown转图片的转换选项
    /// </summary>
    public class ConversionOptions
    {
        /// <summary>
        /// 图片宽度（像素）
        /// </summary>
        public int Width { get; set; } = 800;
        
        /// <summary>
        /// 图片最大高度（像素），超过此高度将自动分页
        /// </summary>
        public int MaxHeight { get; set; } = 1200;
        
        /// <summary>
        /// 图片格式
        /// </summary>
        public ImageFormat Format { get; set; } = ImageFormat.Png;
        
        /// <summary>
        /// 图片质量（1-100，仅对JPEG格式有效）
        /// </summary>
        public int Quality { get; set; } = 90;
        
        /// <summary>
        /// 模板名称
        /// </summary>
        public string TemplateName { get; set; } = "default";
        
        /// <summary>
        /// 自定义模板路径
        /// </summary>
        public string CustomTemplatePath { get; set; }
        
        /// <summary>
        /// 模板变量
        /// </summary>
        public TemplateVariables Variables { get; set; } = new TemplateVariables();
        
        /// <summary>
        /// 是否添加水印
        /// </summary>
        public bool AddWatermark { get; set; } = false;
        
        /// <summary>
        /// 水印文本
        /// </summary>
        public string WatermarkText { get; set; }
        
        /// <summary>
        /// 水印图片路径
        /// </summary>
        public string WatermarkImagePath { get; set; }
        
        /// <summary>
        /// 水印透明度（0-1）
        /// </summary>
        public float WatermarkOpacity { get; set; } = 0.3f;
    }
    
    /// <summary>
    /// 图片格式枚举
    /// </summary>
    public enum ImageFormat
    {
        Png,
        Jpeg,
        Webp
    }
    
    /// <summary>
    /// 模板变量
    /// </summary>
    public class TemplateVariables
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; }
        
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime? Date { get; set; }
        
        /// <summary>
        /// 自定义变量字典
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> CustomVariables { get; set; } = new System.Collections.Generic.Dictionary<string, object>();
    }
}