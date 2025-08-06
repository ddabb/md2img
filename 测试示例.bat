@echo off
echo Md2Image 测试示例
echo ================

rem 确保输出目录存在
if not exist output mkdir output
if not exist test_results mkdir test_results

rem 编译项目
echo 正在编译项目...
dotnet build src\Md2Image\Md2Image.csproj -c Release
dotnet build src\Md2Image.Cli\Md2Image.Cli.csproj -c Release

rem 运行示例 - 默认模板
echo 正在使用默认模板转换...
dotnet run --project src\Md2Image.Cli\Md2Image.Cli.csproj -- -i samples\example.md -o output -t default --watermark "由Md2Image生成" --title "默认模板示例"
dotnet run --project src\Md2Image.Cli\Md2Image.Cli.csproj -- -i samples\example.md -o test_results -t default --watermark "由Md2Image生成" --title "默认模板示例"

rem 运行示例 - 微信模板
echo 正在使用微信模板转换...
dotnet run --project src\Md2Image.Cli\Md2Image.Cli.csproj -- -i samples\example.md -o output -t wechat --watermark "由Md2Image生成" --title "微信模板示例"
dotnet run --project src\Md2Image.Cli\Md2Image.Cli.csproj -- -i samples\example.md -o test_results -t wechat --watermark "由Md2Image生成" --title "微信模板示例"

rem 运行示例 - 抖音模板
echo 正在使用抖音模板转换...
dotnet run --project src\Md2Image.Cli\Md2Image.Cli.csproj -- -i samples\example.md -o output -t douyin --watermark "由Md2Image生成" --title "抖音模板示例"
dotnet run --project src\Md2Image.Cli\Md2Image.Cli.csproj -- -i samples\example.md -o test_results -t douyin --watermark "由Md2Image生成" --title "抖音模板示例"

rem 运行示例 - 小红书模板
echo 正在使用小红书模板转换...
dotnet run --project src\Md2Image.Cli\Md2Image.Cli.csproj -- -i samples\example.md -o output -t xiaohongshu --watermark "由Md2Image生成" --title "小红书模板示例"
dotnet run --project src\Md2Image.Cli\Md2Image.Cli.csproj -- -i samples\example.md -o test_results -t xiaohongshu --watermark "由Md2Image生成" --title "小红书模板示例"

echo.
echo 转换完成！
echo 请查看output目录中的图片文件用于临时查看。
echo 请查看test_results目录中的图片文件和测试结果说明文档。
echo.

pause