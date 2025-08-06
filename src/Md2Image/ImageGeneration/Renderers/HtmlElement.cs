namespace Md2Image.ImageGeneration.Renderers
{
    /// <summary>
    /// HTML元素
    /// </summary>
    public class HtmlElement
    {
        /// <summary>
        /// 元素类型
        /// </summary>
        public ElementType Type { get; set; }
        
        /// <summary>
        /// 元素内容
        /// </summary>
        public string Content { get; set; }
    }
}