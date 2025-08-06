# Md2Image

Md2Image是一个.NET工具库，用于将Markdown文件渲染为图片，支持自定义模板，方便在各种场景下使用，特别适合微信公众号等不支持直接粘贴富文本的平台。

## 功能特点

- 支持标准Markdown语法
- 支持代码高亮
- 支持表格、列表、引用等常用Markdown元素
- 支持图片嵌入
- 提供默认模板和自定义模板功能
- 支持自动分页（根据内容长度自动拆分为多张图片）
- 支持添加水印
- 提供命令行工具和编程API

## 安装

### 通过NuGet安装

```
dotnet add package Md2Image
```

### 命令行工具

```
dotnet tool install --global Md2Image.Cli
```

## 使用方法

### 命令行使用

```
md2image -i input.md -o output_directory --template wechat
```

#### 命令行选项

- `-i, --input`：输入的Markdown文件路径（必需）
- `-o, --output`：输出目录路径（默认为当前目录）
- `-t, --template`：使用的模板名称（默认为"default"）
- `--custom-template`：自定义模板文件路径
- `--width`：图片宽度，像素（默认为800）
- `--max-height`：图片最大高度，像素（默认为1200）
- `--format`：图片格式（png、jpg、webp，默认为png）
- `--quality`：图片质量，1-100（默认为90，仅对jpg和webp格式有效）
- `--title`：文档标题
- `--author`：文档作者
- `--watermark`：水印文本
- `--list-templates`：列出可用的模板

### 编程API使用

```csharp
using Md2Image.Core;

// 创建转换器实例
var converter = new MarkdownConverter();

// 设置转换选项
var options = new ConversionOptions
{
    TemplateName = "wechat",
    Width = 800,
    MaxHeight = 1200,
    Format = ImageFormat.Png,
    Variables = new TemplateVariables
    {
        Title = "我的文档",
        Author = "作者名称",
        Date = DateTime.Now
    },
    AddWatermark = true,
    WatermarkText = "Md2Image"
};

// 转换并保存图片
var outputFiles = await converter.ConvertFileAndSaveAsync(
    "input.md",
    "output_directory",
    "output_prefix",
    options);

// 输出生成的图片文件路径
foreach (var file in outputFiles)
{
    Console.WriteLine(file);
}
```

## 自定义模板

Md2Image支持自定义HTML模板。模板使用Scriban模板引擎，可以访问以下变量：

- `title`：文档标题
- `author`：文档作者
- `date`：文档日期
- `content`：Markdown渲染后的HTML内容
- `watermark_text`：水印文本（如果启用）

您可以创建自己的HTML模板文件，并通过`--custom-template`选项或`CustomTemplatePath`属性指定。

## 许可证

MIT