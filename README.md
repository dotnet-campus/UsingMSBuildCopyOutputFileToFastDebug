# Roslyn 让 VisualStudio 急速调试底层库方法

在一个大项目里面调试底层库经常需要重新编译整个项目，本项目提供了在底层库编译完成之后将输出文件复制到主项目的输出文件夹，通过外部项目调试的方式提高调试效率

| Build | NuGet |
|--|--|
|![](https://github.com/dotnet-campus/UsingMSBuildCopyOutputFileToFastDebug/workflows/.NET%20Core/badge.svg)|[![](https://img.shields.io/nuget/v/dotnetCampus.UsingMSBuildCopyOutputFileToFastDebug.svg)](https://www.nuget.org/packages/dotnetCampus.UsingMSBuildCopyOutputFileToFastDebug)|

## 使用方法

### 安装

在需要调试的底层库项目安装 NuGet 库 [dotnetCampus.UsingMSBuildCopyOutputFileToFastDebug](https://www.nuget.org/packages/dotnetCampus.UsingMSBuildCopyOutputFileToFastDebug)

此库推荐仅在 Debug 下安装

```xml
  <ItemGroup Condition=" '$(Configuration)' == 'Debug'">
    <PackageReference Include="dotnetCampus.UsingMSBuildCopyOutputFileToFastDebug" Version="1.3.1" />
  </ItemGroup>
```

### 配置

配置有两个方法，高级方法将会覆盖基础方法

**基础方法**：

在底层库项目的 `Properties/launchSettings.json` 文件中配置调试启动参数。可以通过 Visual Studio 右击底层库项目属性，点击调试，打开调试启动配置文件 UI 界面，选择启动方式为可执行文件，路径修改为主项目的启动程序。工具将自动读取 `launchSettings.json` 中首个 `commandName` 为 `Executable` 的配置项，从中获取 `executablePath` 作为主项目的可执行文件路径。配置完成之后就可以在底层库点击调试运行主项目调试，同时支持打断点和进行二进制兼容的更改。

`launchSettings.json` 中可执行文件配置的路径支持相对路径，相对于当前底层库项目的输出文件夹。工具会从 `executablePath` 找到主项目的可执行文件，并将底层库的输出文件拷贝到该可执行文件所在的文件夹。

**高级方法**：

编辑底层库项目的 csproj 文件，添加 `MainProjectExecutablePath` 属性直接指定主项目的可执行文件路径。此方法将覆盖基础方法的 `launchSettings.json` 配置：

```xml
    <PropertyGroup>
        <MainProjectExecutablePath>主项目的输出可执行文件</MainProjectExecutablePath>
    </PropertyGroup>
```

请注意如果路径包含空格，记得加上引号，如下面例子

```xml
    <PropertyGroup>
        <MainProjectExecutablePath>"C:\dotnet campus\Foo\bin\release\net9.0\Foo.exe"</MainProjectExecutablePath>
    </PropertyGroup>
```

以上 `MainProjectExecutablePath` 支持相对路径，相对于当前底层库项目 csproj 的相对路径

### 多框架兼容性检查

当底层库项目使用多框架（`<TargetFrameworks>`）时，工具会自动检查当前编译的目标框架与主项目可执行文件的框架是否兼容。兼容规则如下：

- 主项目为 .NET Core / .NET 5+ 系列 → 仅拷贝 .NET Core / .NET 5+ 系列的输出
- 主项目为 .NET Framework 系列 → 仅拷贝 .NET Framework 系列的输出
- 跨系列框架（如 .NET Core 库拷贝到 .NET Framework 主项目）将自动跳过，不做拷贝

工具通过检测主项目可执行文件所在目录是否存在 `.runtimeconfig.json` / `.deps.json` 文件判断其框架类型。对于 .NET Framework 系列，工具还会进一步读取 `.exe.config` 文件中的 `supportedRuntime` 信息。

## 推荐使用方法

如果是小项目进行调试，推荐修改库的demo或添加单元测试进行测试

如果是需要调试具体状态，而不方便写demo等推荐使用此工具提升调试效率，可以将此工具在各个底层库安装

如果是需要做比较大的更改，如接口修改，推荐使用 [dotnet-campus/DllReferencePathChanger: VS DLL引用替换插件](https://github.com/dotnet-campus/DllReferencePathChanger ) 插件

## 原理

在软件运行的时候依然可以移动 dll 或 exe 的路径，而此工具将底层库项目的输出 dll 和 pdb 文件拷贝到主项目可执行文件所在的文件夹（通过 `MainProjectExecutablePath` 或 `launchSettings.json` 获取），将原本的 dll 和 pdb 重命名为 `.bak` 文件，然后通过调试的可执行文件方式启动主项目

此时的主项目将会加载新的 dll 文件，同时因为存在 pdb 文件也能进去代码调试

通过将原本dll重命名的方式可以解决主项目执行的文件占用问题

此调试方式要求对底层库的更改满足二进制兼容

关于二进制兼容请看 [VisualStudio 通过外部调试方法快速调试库代码](https://blog.lindexi.com/post/visualstudio-%E9%80%9A%E8%BF%87%E5%A4%96%E9%83%A8%E8%B0%83%E8%AF%95%E6%96%B9%E6%B3%95%E5%BF%AB%E9%80%9F%E8%B0%83%E8%AF%95%E5%BA%93%E4%BB%A3%E7%A0%81 )

原有的 dll 和 pdb 文件将被加入清理列表文件，将会在执行清理的时候进行清理

## 细节

### 启用控制

默认仅有在 Debug 下开启此功能，如需在 Release 也开启，请通过设置 `EnableUsingMSBuildCopyOutputFileToFastDebug` 属性为 `true` 开启：

```xml
    <PropertyGroup>
        <EnableUsingMSBuildCopyOutputFileToFastDebug>true</EnableUsingMSBuildCopyOutputFileToFastDebug>
    </PropertyGroup>
```

此项设置之后将会在 Debug 和 Release 下都开启复制。

### 复制文件

工具默认将当前项目的输出 dll 和 pdb 文件（`$(AssemblyName).dll` 和 `$(AssemblyName).pdb`）拷贝到主项目可执行文件所在的文件夹。如需复制更多文件，可在项目文件中自行扩展 `OutputFileToCopy` 项组。示例如下，可实现将追加的文件一并输出到最终输出路径：

```xml
  <ItemGroup>
    <OutputFileToCopy Include="$(OutputPath)Lib1.dll" />
    <OutputFileToCopy Include="$(OutputPath)Lib1.pdb" />
    <OutputFileToCopy Include="$(OutputPath)Lib2.dll" />
    <OutputFileToCopy Include="$(OutputPath)Lib2.pdb" />
  </ItemGroup>
```

通常，这个功能用于将所依赖的基础库一并输出到最终输出路径。当依赖库也有变更时，仅复制当前项目的产物会导致依赖项目未被拷贝，进而出现调试时无法命中断点的问题。

### 文件占用处理

目标文件夹如果已存在同名 dll 或 pdb 文件，工具会先将其重命名为 `{原文件名}{序号}.{扩展名}.bak`，再拷贝新文件。被重命名的 bak 文件会被记录到清理列表文件（位于 `$(IntermediateOutputPath)CleanUsingMSBuildCopyOutputFileToFastDebugFile.txt`），在执行 MSBuild Clean 目标时统一清理。

## 感谢

感谢 https://github.com/kkwpsv/lsjutil 提供 json 解析