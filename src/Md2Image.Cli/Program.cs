using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Md2Image.Core;

namespace Md2Image.Cli
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args.Length == 0 || (args.Length == 1 && (args[0] == "-h" || args[0] == "--help")))
            {
                ShowHelp();
                return 0;
            }

            if (args.Length == 1 && args[0] == "--list-templates")
            {
                ListTemplates();
                return 0;
            }

            string inputFile = null;
            string outputDir = Directory.GetCurrentDirectory();
            string template = "default";
            string customTemplatePath = null;
            int width = 800;
            int maxHeight = 5000; // 增加最大高度，确保长内容能够完整显示
            string format = "png";
            int quality = 90;
            string title = null;
            string author = null;
            string watermark = null;

            // 解析命令行参数
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-i":
                    case "--input":
                        if (i + 1 < args.Length) inputFile = args[++i];
                        break;
                    case "-o":
                    case "--output":
                        if (i + 1 < args.Length) outputDir = args[++i];
                        break;
                    case "-t":
                    case "--template":
                        if (i + 1 < args.Length) template = args[++i];
                        break;
                    case "--custom-template":
                        if (i + 1 < args.Length) customTemplatePath = args[++i];
                        break;
                    case "--width":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out int w)) width = w;
                        break;
                    case "--max-height":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out int h)) maxHeight = h;
                        break;
                    case "--format":
                        if (i + 1 < args.Length) format = args[++i];
                        break;
                    case "--quality":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out int q)) quality = q;
                        break;
                    case "--title":
                        if (i + 1 < args.Length) title = args[++i];
                        break;
                    case "--author":
                        if (i + 1 < args.Length) author = args[++i];
                        break;
                    case "--watermark":
                        if (i + 1 < args.Length) watermark = args[++i];
                        break;
                }
            }

            // 检查必需参数
            if (string.IsNullOrEmpty(inputFile))
            {
                Console.WriteLine("错误：必须指定输入文件路径。使用 -i 或 --input 选项。");
                ShowHelp();
                return 1;
            }

            try
            {
                // 创建转换器实例
                var converter = new MarkdownConverter();
                
                // 检查输入文件是否存在
                if (!File.Exists(inputFile))
                {
                    Console.WriteLine($"错误：输入文件不存在：{inputFile}");
                    return 1;
                }
                
                // 确保输出目录存在
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }
                
                // 解析图片格式
                if (!Enum.TryParse<ImageFormat>(format, true, out var imageFormat))
                {
                    Console.WriteLine($"错误：不支持的图片格式：{format}，支持的格式有：png、jpeg、webp");
                    return 1;
                }
                
                // 创建转换选项
                var options = new ConversionOptions
                {
                    TemplateName = template,
                    Width = width,
                    MaxHeight = maxHeight,
                    Format = imageFormat,
                    Quality = quality,
                    Variables = new TemplateVariables
                    {
                        Title = title ?? Path.GetFileNameWithoutExtension(inputFile),
                        Author = author,
                        Date = DateTime.Now
                    }
                };
                
                // 设置自定义模板
                if (!string.IsNullOrEmpty(customTemplatePath) && File.Exists(customTemplatePath))
                {
                    options.CustomTemplatePath = customTemplatePath;
                }
                
                // 设置水印
                if (!string.IsNullOrEmpty(watermark))
                {
                    options.AddWatermark = true;
                    options.WatermarkText = watermark;
                }
                
                Console.WriteLine($"正在处理Markdown文件：{inputFile}");
                
                // 转换并保存图片
                var outputFiles = await converter.ConvertFileAndSaveAsync(
                    inputFile,
                    outputDir,
                    Path.GetFileNameWithoutExtension(inputFile),
                    options);
                
                Console.WriteLine($"转换完成，生成了 {outputFiles.Count} 个图片文件：");
                foreach (var file in outputFiles)
                {
                    Console.WriteLine($"- {file}");
                }
                
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误：{ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"内部错误：{ex.InnerException.Message}");
                }
                return 1;
            }
        }
        
        static void ShowHelp()
        {
            Console.WriteLine("Md2Image - 将Markdown文件转换为图片的工具");
            Console.WriteLine();
            Console.WriteLine("用法：");
            Console.WriteLine("  md2image -i <输入文件> [选项]");
            Console.WriteLine();
            Console.WriteLine("选项：");
            Console.WriteLine("  -i, --input <文件>       输入的Markdown文件路径（必需）");
            Console.WriteLine("  -o, --output <目录>      输出目录路径（默认为当前目录）");
            Console.WriteLine("  -t, --template <名称>    使用的模板名称（默认为\"default\"）");
            Console.WriteLine("  --custom-template <文件> 自定义模板文件路径");
            Console.WriteLine("  --width <数值>           图片宽度，像素（默认为800）");
            Console.WriteLine("  --max-height <数值>      图片最大高度，像素（默认为1200）");
            Console.WriteLine("  --format <格式>          图片格式（png、jpg、webp，默认为png）");
            Console.WriteLine("  --quality <数值>         图片质量，1-100（默认为90，仅对jpg和webp格式有效）");
            Console.WriteLine("  --title <文本>           文档标题");
            Console.WriteLine("  --author <文本>          文档作者");
            Console.WriteLine("  --watermark <文本>       水印文本");
            Console.WriteLine("  --list-templates         列出可用的模板");
            Console.WriteLine("  -h, --help               显示帮助信息");
        }
        
        static void ListTemplates()
        {
            var converter = new MarkdownConverter();
            var templates = converter.GetAvailableTemplates();
            
            Console.WriteLine("可用的模板：");
            foreach (var templateName in templates)
            {
                Console.WriteLine($"- {templateName}");
            }
        }
    }
}