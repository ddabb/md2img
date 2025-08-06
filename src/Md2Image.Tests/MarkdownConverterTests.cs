using System;
using System.IO;
using System.Threading.Tasks;
using Md2Image.Core;
using Xunit;

namespace Md2Image.Tests
{
    public class MarkdownConverterTests
    {
        [Fact]
        public async Task ConvertToImagesAsync_WithSimpleMarkdown_ShouldReturnImageData()
        {
            // 准备
            var converter = new MarkdownConverter();
            var markdown = "# 测试标题\n\n这是一个测试。";
            
            // 执行
            var result = await converter.ConvertToImagesAsync(markdown);
            
            // 验证
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.True(result.Count > 0);
            Assert.True(result[0].Length > 0);
        }
        
        [Fact]
        public async Task ConvertAndSaveAsync_WithSimpleMarkdown_ShouldCreateImageFile()
        {
            // 准备
            var converter = new MarkdownConverter();
            var markdown = "# 测试标题\n\n这是一个测试。";
            var outputDir = Path.Combine(Path.GetTempPath(), "Md2ImageTests");
            var fileNamePrefix = "test";
            
            // 确保输出目录存在
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            
            // 执行
            var result = await converter.ConvertAndSaveAsync(markdown, outputDir, fileNamePrefix);
            
            try
            {
                // 验证
                Assert.NotNull(result);
                Assert.NotEmpty(result);
                Assert.True(result.Count > 0);
                
                foreach (var filePath in result)
                {
                    Assert.True(File.Exists(filePath));
                    Assert.True(new FileInfo(filePath).Length > 0);
                }
            }
            finally
            {
                // 清理
                foreach (var filePath in result)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                
                if (Directory.Exists(outputDir))
                {
                    Directory.Delete(outputDir, true);
                }
            }
        }
        
        [Fact]
        public void GetAvailableTemplates_ShouldReturnTemplateList()
        {
            // 准备
            var converter = new MarkdownConverter();
            
            // 执行
            var templates = converter.GetAvailableTemplates();
            
            // 验证
            Assert.NotNull(templates);
            Assert.NotEmpty(templates);
            Assert.Contains("default", templates);
            Assert.Contains("wechat", templates);
        }
    }
}