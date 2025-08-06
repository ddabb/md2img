using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Md2Image.ImageGeneration.Renderers
{
    /// <summary>
    /// 表格渲染器
    /// </summary>
    public class TableRenderer : BaseElementRenderer
    {
        private readonly float _cellPadding;
        private readonly float _cellHeight;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fontFamily">字体</param>
        public TableRenderer(string fontFamily = null) : base(fontFamily)
        {
            FontSize = 14;
            LineHeight = 1.2f;
            _cellPadding = 10;
            _cellHeight = 30;
        }
        
        /// <summary>
        /// 渲染表格
        /// </summary>
        public override float Render(SKCanvas canvas, string tableHtml, float x, float y, float width)
        {
            try
            {
                // 解析表格结构
                var rows = ParseTableHtml(tableHtml);
                
                // 如果没有解析到表格内容，使用简化渲染
                if (rows.Count == 0)
                {
                    return RenderSimpleTable(canvas, x, y, width);
                }
                
                // 计算列宽
                int columnCount = rows.Max(r => r.Count);
                float[] columnWidths = new float[columnCount];
                
                // 平均分配列宽
                for (int i = 0; i < columnCount; i++)
                {
                    columnWidths[i] = width / columnCount;
                }
                
                // 绘制表格
                float currentY = y;
                
                using (var borderPaint = new SKPaint
                {
                    Color = SKColors.Gray,
                    StrokeWidth = 1,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                })
                using (var headerFillPaint = new SKPaint
                {
                    Color = new SKColor(240, 240, 240),
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                })
                using (var textPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    TextSize = FontSize,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName(FontFamily)
                })
                {
                    // 绘制表格行
                    for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                    {
                        var row = rows[rowIndex];
                        float rowHeight = _cellHeight;
                        
                        // 计算行高
                        for (int colIndex = 0; colIndex < row.Count; colIndex++)
                        {
                            string cellText = row[colIndex];
                            float textHeight = MeasureTextHeight(cellText, columnWidths[colIndex] - _cellPadding * 2, FontSize, LineHeight);
                            rowHeight = Math.Max(rowHeight, textHeight + _cellPadding * 2);
                        }
                        
                        // 绘制行背景
                        if (rowIndex == 0) // 表头
                        {
                            canvas.DrawRect(new SKRect(x, currentY, x + width, currentY + rowHeight), headerFillPaint);
                        }
                        else if (rowIndex % 2 == 1) // 奇数行
                        {
                            canvas.DrawRect(new SKRect(x, currentY, x + width, currentY + rowHeight), new SKPaint
                            {
                                Color = new SKColor(250, 250, 250),
                                Style = SKPaintStyle.Fill
                            });
                        }
                        
                        // 绘制单元格
                        float currentX = x;
                        for (int colIndex = 0; colIndex < columnCount; colIndex++)
                        {
                            // 绘制单元格边框
                            canvas.DrawRect(new SKRect(currentX, currentY, currentX + columnWidths[colIndex], currentY + rowHeight), borderPaint);
                            
                            // 绘制单元格内容
                            if (colIndex < row.Count)
                            {
                                string cellText = row[colIndex];
                                
                                // 绘制文本
                                float textX = currentX + _cellPadding;
                                float textY = currentY + _cellPadding + FontSize; // 基线位置
                                
                                // 处理多行文本
                                var words = cellText.Split(' ');
                                var currentLine = new StringBuilder();
                                
                                foreach (var word in words)
                                {
                                    var testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
                                    var lineWidth = textPaint.MeasureText(testLine);
                                    
                                    if (lineWidth <= columnWidths[colIndex] - _cellPadding * 2)
                                    {
                                        currentLine.Append(currentLine.Length == 0 ? word : " " + word);
                                    }
                                    else
                                    {
                                        canvas.DrawText(currentLine.ToString(), textX, textY, textPaint);
                                        textY += FontSize * LineHeight;
                                        currentLine.Clear();
                                        currentLine.Append(word);
                                    }
                                }
                                
                                if (currentLine.Length > 0)
                                {
                                    canvas.DrawText(currentLine.ToString(), textX, textY, textPaint);
                                }
                            }
                            
                            currentX += columnWidths[colIndex];
                        }
                        
                        currentY += rowHeight;
                    }
                }
                
                return currentY + 20;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"表格渲染错误: {ex.Message}");
                return RenderSimpleTable(canvas, x, y, width);
            }
        }
        
        /// <summary>
        /// 测量表格高度
        /// </summary>
        public override float MeasureHeight(string content, float width)
        {
            try
            {
                var rows = ParseTableHtml(content);
                if (rows.Count == 0)
                {
                    return width / 2 + 30; // 简化表格高度
                }
                
                int columnCount = rows.Max(r => r.Count);
                float[] columnWidths = new float[columnCount];
                for (int i = 0; i < columnCount; i++)
                {
                    columnWidths[i] = width / columnCount;
                }
                
                float totalHeight = 0;
                
                for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                {
                    var row = rows[rowIndex];
                    float rowHeight = _cellHeight;
                    
                    for (int colIndex = 0; colIndex < row.Count; colIndex++)
                    {
                        string cellText = row[colIndex];
                        float textHeight = MeasureTextHeight(cellText, columnWidths[colIndex] - _cellPadding * 2, FontSize, LineHeight);
                        rowHeight = Math.Max(rowHeight, textHeight + _cellPadding * 2);
                    }
                    
                    totalHeight += rowHeight;
                }
                
                return totalHeight + 20;
            }
            catch
            {
                return width / 2 + 30; // 简化表格高度
            }
        }
        
        /// <summary>
        /// 解析表格HTML
        /// </summary>
        private List<List<string>> ParseTableHtml(string tableHtml)
        {
            var rows = new List<List<string>>();
            var headerRow = new List<string>();
            
            // 提取表头
            var thMatches = Regex.Matches(tableHtml, @"<th[^>]*>(.*?)</th>", RegexOptions.Singleline);
            foreach (Match match in thMatches)
            {
                headerRow.Add(StripHtmlTags(match.Groups[1].Value));
            }
            
            if (headerRow.Count > 0)
            {
                rows.Add(headerRow);
            }
            
            // 提取表格行
            var trMatches = Regex.Matches(tableHtml, @"<tr[^>]*>(.*?)</tr>", RegexOptions.Singleline);
            foreach (Match trMatch in trMatches)
            {
                var row = new List<string>();
                var tdMatches = Regex.Matches(trMatch.Groups[1].Value, @"<td[^>]*>(.*?)</td>", RegexOptions.Singleline);
                
                if (tdMatches.Count == 0)
                {
                    continue; // 跳过只包含th的行
                }
                
                foreach (Match tdMatch in tdMatches)
                {
                    row.Add(StripHtmlTags(tdMatch.Groups[1].Value));
                }
                
                if (row.Count > 0)
                {
                    rows.Add(row);
                }
            }
            
            return rows;
        }
        
        /// <summary>
        /// 去除HTML标签并解码HTML实体
        /// </summary>
        private string StripHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;
                
            // 先替换常见HTML实体
            string result = html
                .Replace("&quot;", "\"")
                .Replace("&apos;", "'")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&amp;", "&")
                .Replace("&nbsp;", " ");
                
            // 去除HTML标签
            result = Regex.Replace(result, @"<[^>]+>", string.Empty);
            
            return result;
        }
        
        /// <summary>
        /// 渲染简化表格
        /// </summary>
        private float RenderSimpleTable(SKCanvas canvas, float x, float y, float width)
        {
            // 绘制表格边框
            using (var paint = new SKPaint
            {
                Color = SKColors.Gray,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            {
                float tableHeight = width / 2; // 假设表格高度为宽度的一半
                var rect = new SKRect(x, y, x + width, y + tableHeight);
                canvas.DrawRect(rect, paint);
                
                // 绘制表格提示文本
                using (var textPaint = new SKPaint
                {
                    Color = SKColors.Gray,
                    TextSize = 16,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center
                })
                {
                    canvas.DrawText("表格内容 (简化渲染)", x + width / 2, y + tableHeight / 2, textPaint);
                }
                
                return y + tableHeight + 20;
            }
        }
    }
}