using SkiaSharp;
using System;
using System.IO;

namespace Md2Image.ImageGeneration.Renderers
{
    /// <summary>
    /// 图片渲染器
    /// </summary>
    public class ImageRenderer : BaseElementRenderer
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fontFamily">字体</param>
        public ImageRenderer(string? fontFamily = null) : base(fontFamily)
        {
        }
        
        /// <summary>
        /// 渲染图片
        /// </summary>
        public override float Render(SKCanvas canvas, string src, float x, float y, float width)
        {
            try
            {
                // 尝试加载图片
                SKBitmap? imageBitmap = null;
                
                if (src.StartsWith("http://") || src.StartsWith("https://"))
                {
                    // 从URL加载图片（实际应用中应使用HTTP客户端）
                    // 这里简化处理，使用占位图
                    imageBitmap = new SKBitmap(400, 200);
                    using (var imageCanvas = new SKCanvas(imageBitmap))
                    {
                        imageCanvas.Clear(SKColors.LightGray);
                        using (var paint = new SKPaint
                        {
                            Color = SKColors.Gray,
                            TextSize = 16,
                            IsAntialias = true,
                            TextAlign = SKTextAlign.Center
                        })
                        {
                            imageCanvas.DrawText("图片: " + src, 200, 100, paint);
                        }
                    }
                }
                else if (File.Exists(src))
                {
                    // 从本地文件加载图片
                    using (var stream = File.OpenRead(src))
                    {
                        imageBitmap = SKBitmap.Decode(stream);
                    }
                }
                else
                {
                    // 创建占位图
                    imageBitmap = new SKBitmap(400, 200);
                    using (var imageCanvas = new SKCanvas(imageBitmap))
                    {
                        imageCanvas.Clear(SKColors.LightGray);
                        using (var paint = new SKPaint
                        {
                            Color = SKColors.Gray,
                            TextSize = 16,
                            IsAntialias = true,
                            TextAlign = SKTextAlign.Center
                        })
                        {
                            imageCanvas.DrawText("找不到图片: " + src, 200, 100, paint);
                        }
                    }
                }
                
                if (imageBitmap != null)
                {
                    // 计算图片尺寸，保持宽高比
                    float imageWidth = width;
                    float imageHeight = imageBitmap.Height * (imageWidth / imageBitmap.Width);
                    
                    // 绘制图片
                    var rect = new SKRect(x, y, x + imageWidth, y + imageHeight);
                    canvas.DrawBitmap(imageBitmap, rect);
                    
                    return y + imageHeight + 20;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"图片渲染错误: {ex.Message}");
                
                // 绘制错误信息
                using (var paint = new SKPaint
                {
                    Color = SKColors.Red,
                    TextSize = 14,
                    IsAntialias = true
                })
                {
                    canvas.DrawText($"图片渲染错误: {ex.Message}", x, y + 14, paint);
                }
                
                return y + 30;
            }
            
            return y + 20;
        }
        
        /// <summary>
        /// 测量图片高度
        /// </summary>
        public override float MeasureHeight(string src, float width)
        {
            try
            {
                if (File.Exists(src))
                {
                    using (var stream = File.OpenRead(src))
                    {
                        var imageBitmap = SKBitmap.Decode(stream);
                        float imageWidth = width;
                        float imageHeight = imageBitmap.Height * (imageWidth / imageBitmap.Width);
                        return imageHeight + 20;
                    }
                }
            }
            catch
            {
                // 忽略错误
            }
            
            // 默认高度
            return width / 2 + 20;
        }
    }
}