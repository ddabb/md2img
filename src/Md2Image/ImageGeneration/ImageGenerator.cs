using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Md2Image.Core;
using SkiaSharp;

namespace Md2Image.ImageGeneration
{
    /// <summary>
    /// 图像生成器，负责将HTML渲染为图片
    /// </summary>
    public class ImageGenerator
    {
        private readonly HtmlRenderer _renderer;
        
        /// <summary>
        /// 创建图像生成器实例
        /// </summary>
        public ImageGenerator()
        {
            _renderer = new HtmlRenderer();
        }
        
    /// <summary>
    /// 将HTML内容生成为图片
    /// </summary>
    /// <param name="htmlContent">HTML内容</param>
    /// <param name="options">转换选项</param>
    /// <returns>生成的图片数据列表</returns>
    public virtual async Task<IList<byte[]>> GenerateImagesAsync(string htmlContent, ConversionOptions options)
        {
            // 渲染HTML为位图
            var renderedPages = await _renderer.RenderHtmlAsync(htmlContent, options);
            
            // 将位图转换为指定格式的图片数据
            var result = new List<byte[]>();
            foreach (var bitmap in renderedPages)
            {
                using (var ms = new MemoryStream())
                {
                    switch (options.Format)
                    {
                        case ImageFormat.Png:
                            bitmap.Encode(ms, SKEncodedImageFormat.Png, 100);
                            break;
                        case ImageFormat.Jpeg:
                            bitmap.Encode(ms, SKEncodedImageFormat.Jpeg, options.Quality);
                            break;
                        case ImageFormat.Webp:
                            bitmap.Encode(ms, SKEncodedImageFormat.Webp, options.Quality);
                            break;
                        default:
                            bitmap.Encode(ms, SKEncodedImageFormat.Png, 100);
                            break;
                    }
                    
                    result.Add(ms.ToArray());
                }
            }
            
            return result;
        }
        
    /// <summary>
    /// 将HTML内容生成为图片并保存到指定路径
    /// </summary>
    /// <param name="htmlContent">HTML内容</param>
    /// <param name="outputDirectory">输出目录</param>
    /// <param name="fileNamePrefix">文件名前缀</param>
    /// <param name="options">转换选项</param>
    /// <returns>生成的图片文件路径列表</returns>
    public virtual async Task<IList<string>> GenerateAndSaveImagesAsync(string htmlContent, string outputDirectory, string fileNamePrefix, ConversionOptions options)
        {
            // 确保输出目录存在
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            
            // 生成图片数据
            var imageDataList = await GenerateImagesAsync(htmlContent, options);
            
            // 确定文件扩展名
            string extension = options.Format switch
            {
                ImageFormat.Png => ".png",
                ImageFormat.Jpeg => ".jpg",
                ImageFormat.Webp => ".webp",
                _ => ".png"
            };
            
            // 保存图片文件
            var result = new List<string>();
            for (int i = 0; i < imageDataList.Count; i++)
            {
                string fileName = imageDataList.Count > 1
                    ? $"{fileNamePrefix}_{i + 1}{extension}"
                    : $"{fileNamePrefix}{extension}";
                    
                string filePath = Path.Combine(outputDirectory, fileName);
                await File.WriteAllBytesAsync(filePath, imageDataList[i]);
                result.Add(filePath);
            }
            
            return result;
        }
        
        /// <summary>
        /// 添加水印
        /// </summary>
        /// <param name="bitmap">原始图片</param>
        /// <param name="options">转换选项</param>
        /// <returns>添加水印后的图片</returns>
        private SKBitmap AddWatermark(SKBitmap bitmap, ConversionOptions options)
        {
            if (!options.AddWatermark || (string.IsNullOrEmpty(options.WatermarkText) && string.IsNullOrEmpty(options.WatermarkImagePath)))
            {
                return bitmap;
            }
            
            using (var canvas = new SKCanvas(bitmap))
            {
                if (!string.IsNullOrEmpty(options.WatermarkText))
                {
                    // 添加文字水印
                    using (var paint = new SKPaint
                    {
                        Color = new SKColor(0, 0, 0, (byte)(options.WatermarkOpacity * 255)),
                        TextSize = 24,
                        IsAntialias = true,
                        TextAlign = SKTextAlign.Right
                    })
                    {
                        canvas.DrawText(options.WatermarkText, bitmap.Width - 20, bitmap.Height - 20, paint);
                    }
                }
                else if (!string.IsNullOrEmpty(options.WatermarkImagePath) && File.Exists(options.WatermarkImagePath))
                {
                    // 添加图片水印
                    try
                    {
                        using (var watermarkBitmap = SKBitmap.Decode(options.WatermarkImagePath))
                        {
                            // 计算水印位置和大小
                            int maxWatermarkWidth = bitmap.Width / 4;
                            int maxWatermarkHeight = bitmap.Height / 4;
                            
                            float scale = Math.Min(
                                (float)maxWatermarkWidth / watermarkBitmap.Width,
                                (float)maxWatermarkHeight / watermarkBitmap.Height);
                            
                            int scaledWidth = (int)(watermarkBitmap.Width * scale);
                            int scaledHeight = (int)(watermarkBitmap.Height * scale);
                            
                            // 绘制水印
                            var rect = new SKRect(
                                bitmap.Width - scaledWidth - 20,
                                bitmap.Height - scaledHeight - 20,
                                bitmap.Width - 20,
                                bitmap.Height - 20);
                                
                            using (var paint = new SKPaint
                            {
                                FilterQuality = SKFilterQuality.High,
                                ColorF = new SKColorF(1, 1, 1, options.WatermarkOpacity)
                            })
                            {
                                canvas.DrawBitmap(watermarkBitmap, rect, paint);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"添加图片水印失败: {ex.Message}");
                    }
                }
            }
            
            return bitmap;
        }
    }
}