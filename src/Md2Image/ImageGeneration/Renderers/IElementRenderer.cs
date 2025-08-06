using SkiaSharp;

namespace Md2Image.ImageGeneration.Renderers
{
    /// <summary>
    /// HTML元素渲染器接口
    /// </summary>
    public interface IElementRenderer
    {
        /// <summary>
        /// 渲染元素
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="content">内容</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="width">可用宽度</param>
        /// <returns>渲染后的Y坐标位置</returns>
        float Render(SKCanvas canvas, string content, float x, float y, float width);
        
        /// <summary>
        /// 测量元素高度
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="width">可用宽度</param>
        /// <returns>元素高度</returns>
        float MeasureHeight(string content, float width);
    }
}