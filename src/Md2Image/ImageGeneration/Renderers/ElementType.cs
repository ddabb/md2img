namespace Md2Image.ImageGeneration.Renderers
{
    /// <summary>
    /// HTML元素类型
    /// </summary>
    public enum ElementType
    {
        /// <summary>
        /// 一级标题
        /// </summary>
        Heading1,
        
        /// <summary>
        /// 二级标题
        /// </summary>
        Heading2,
        
        /// <summary>
        /// 段落
        /// </summary>
        Paragraph,
        
        /// <summary>
        /// 图片
        /// </summary>
        Image,
        
        /// <summary>
        /// 代码块
        /// </summary>
        CodeBlock,
        
        /// <summary>
        /// 引用块
        /// </summary>
        Blockquote,
        
        /// <summary>
        /// 表格
        /// </summary>
        Table,
        
        /// <summary>
        /// 列表项
        /// </summary>
        ListItem
    }
}