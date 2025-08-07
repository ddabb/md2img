# 引用和代码块测试

## 引用测试

> 这是一个简单的引用。
> 这是引用的第二行。

> 这是另一个引用，包含**加粗**和*斜体*文本。

## 代码块测试

```csharp
// 这是一段C#代码
Console.WriteLine("Hello, World!");
```

```javascript
// 这是一段JavaScript代码
console.log("Hello, World!");
```

```
// 这是一段没有指定语言的代码
print("Hello, World!")
```

## 混合测试

> 引用中包含代码 `Console.WriteLine("Hello");`

```csharp
// 代码中包含注释
/* 这是多行注释
   第二行注释 */
Console.WriteLine("包含//注释的代码");