# Roslyn 让 VisualStudio 急速调试底层库方法

在一个大项目里面调试底层库经常需要重新编译整个项目，本项目提供了在底层库编译完成之后将输出文件复制到主项目的输出文件夹，通过外部项目调试的方式提高调试效率

| Build | NuGet |
|--|--|
|![](https://github.com/dotnet-campus/UsingMSBuildCopyOutputFileToFastDebug/workflows/.NET%20Core/badge.svg)|[![](https://img.shields.io/nuget/v/dotnetCampus.UsingMSBuildCopyOutputFileToFastDebug.svg)](https://www.nuget.org/packages/dotnetCampus.UsingMSBuildCopyOutputFileToFastDebug)|

## 使用方法

### 安装

在需要调试的底层库项目安装 NuGet 库 [dotnetCampus.UsingMSBuildCopyOutputFileToFastDebug](https://www.nuget.org/packages/dotnetCampus.UsingMSBuildCopyOutputFileToFastDebug)

### 配置

配置有两个方法

基础方法：

通过右击底层库属性，点击调试，设置为可执行文件，路径修改为主项目的启动程序。就可以在底层库点击调试运行主项目调试，同时支持打断点和进行二进制兼容的更改

高级方法：

编辑底层库项目的 csproj 文件，添加下面代码

```csharp
    <PropertyGroup>
        <MainProjectPath>主项目的输出文件夹</MainProjectPath>
    </PropertyGroup>
```

请注意 主项目的输出文件夹 的路径最后使用 `\` 结束，如 `C:\lindexi\doubi\` 如果是将底层库放在其他文件夹，请将 主项目的输出文件夹 修改为实际的文件夹

## 原理

在软件运行的时候依然可以移动 dll 或 exe 的路径，而此工具将底层库项目的输出 dll 和 pdb 文件拷贝到主项目的文件夹或 MainProjectPath 设置的文件夹，将原本的dll和pdb重命名，然后通过调试的可执行文件方式启动主项目

此时的主项目将会加载新的dll文件，同时因为存在 pdb 文件也能进去代码调试

通过将原本dll重命名的方式可以解决主项目执行的文件占用问题

此调试方式要求对底层库的更改满足二进制兼容

关于二进制兼容请看 [VisualStudio 通过外部调试方法快速调试库代码](https://blog.lindexi.com/post/visualstudio-%E9%80%9A%E8%BF%87%E5%A4%96%E9%83%A8%E8%B0%83%E8%AF%95%E6%96%B9%E6%B3%95%E5%BF%AB%E9%80%9F%E8%B0%83%E8%AF%95%E5%BA%93%E4%BB%A3%E7%A0%81 )