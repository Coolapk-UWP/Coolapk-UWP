<img alt="Coolapk LOGO" src="./logo3.png" width="200px" />

# Coolapk UWP
一个基于UWP平台的第三方酷安客户端

基于[@一块小板子](http://www.coolapk.com/u/695942)的源码([Github](https://github.com/oboard/CoolApk-UWP))

<a href="https://github.com/Tangent-90/Coolapk-UWP/blob/master/LICENSE"><img alt="GitHub" src="https://img.shields.io/github/license/Tangent-90/Coolapk-UWP.svg?label=License&style=flat-square"></a>
<a href="https://github.com/Tangent-90/Coolapk-UWP/issues"><img alt="GitHub" src="https://img.shields.io/github/issues/Tangent-90/Coolapk-UWP.svg?label=Issues&style=flat-square"></a>
<a href="https://github.com/Tangent-90/Coolapk-UWP/stargazers"><img alt="GitHub" src="https://img.shields.io/github/stars/Tangent-90/Coolapk-UWP.svg?label=Stars&style=flat-square"></a>

<a href="https://github.com/Tangent-90/Coolapk-UWP/releases/latest"><img alt="GitHub All Releases" src="https://img.shields.io/github/downloads/Tangent-90/Coolapk-UWP/total.svg?label=DOWNLOAD&logo=github&style=for-the-badge"></a>
<a href=""><img alt="Baidu Netdisk" src="https://img.shields.io/badge/download-%e5%af%86%e7%a0%81%ef%bc%9alIIl-magenta.svg?label=%e4%b8%8b%e8%bd%bd&logo=baidu&style=for-the-badge"></a>

## 目录
- [声明](#声明)
- [现有功能](#现有功能)
- [如何安装应用](#如何安装应用)
  - [最低需求](#最低需求)
  - [使用应用安装脚本安装应用](#使用应用安装脚本安装应用)
  - [使用应用安装程序安装应用](#使用应用安装程序安装应用)
  - [更新应用](#更新应用)
- [屏幕截图](#屏幕截图)
- [使用到的开源模块](#使用到的开源模块)
- [鸣谢](#鸣谢)

## 声明
1. 本程序是[酷安](https://coolapk.com)的第三方客户端，仅用作学习交流使用，禁止用于商业用途。
2. 本程序是开源软件，因此，在使用时请确保程序是来自[本Github仓库](https://github.com/Tangent-90/Coolapk-UWP)，以确保您的数据安全。
3. 若程序来源无异常，程序运行过程中您的所有数据都仅用于与酷安的服务器交流或储存于本地，开发者不会窃取您的任何数据。但即便如此，也请注意使用环境的安全性。
4. 若您对[酷安](https://coolapk.com)如何处理您的数据存在疑虑，请访问[酷安用户服务协议](https://m.coolapk.com/mp/user/agreement)、[酷安隐私保护政策](https://coolapk.com/about/privacy.html)。

## 现有功能
1. 夜间模式
2. 登录/点赞
3. 浏览动态/图文/问答/通知
4. 更多内容请自行发掘<img alt="流汗滑稽" src="./src/CoolapkUWP/Assets/Emoji/1259.png" width="20px" />

## 如何安装应用

<iframe src="//player.bilibili.com/player.html?aid=844607436&bvid=BV1y54y1a768&cid=309705979&page=1" scrolling="no" border="0" frameborder="no" framespacing="0" allowfullscreen="true"> </iframe>

### 最低需求
- Windows 10 Build ~~15063~~ 17134及以上
- 设备需支持ARM/x86/x64
- Feature2分支支持Windows 10 Build 15035及以上
- 至少100MB的空余储存空间(用于储存安装包与安装应用)

### 使用应用安装脚本安装应用
- 下载并解压最新的[安装包`(UWP_x.x.x.0_Debug_Test.rar)`](https://github.com/Tangent-90/Coolapk-UWP/releases/latest)
- 如果没有应用安装脚本，下载[`Install.ps1`](Install.ps1)到目标目录
![Install.ps1](Images/Guides/Snipaste_2019-10-12_22-49-11.png)
- 右击`Install.ps1`，选择“使用PowerShell运行”
- 应用安装脚本将会引导您完成此过程的剩余部分

### 使用应用安装程序安装应用
- 下载并解压最新的[安装包`(UWP_x.x.x.0_Debug_Test.rar)`](https://github.com/Tangent-90/Coolapk-UWP/releases/latest)
- [开启旁加载模式](https://www.windowscentral.com/how-enable-windows-10-sideload-apps-outside-store)
  - 如果您想开发UWP应用，您可以开启[开发人员模式](https://docs.microsoft.com/zh-cn/windows/uwp/get-started/enable-your-device-for-development)，**对于大多数不需要做UWP开发的用户来说，开发人员模式是没有必要的**
- 安装`Dependencies`文件夹下的适用于您的设备的所有依赖包
![Dependencies](Images/Guides/Snipaste_2019-10-13_15-51-33.png)
- 安装`*.cer`证书到`本地计算机`→`受信任的根证书颁发机构`
  - 这项操作需要用到管理员权限，如果您安装证书时没有用到该权限，则可能是因为您将证书安装到了错误的位置或者您使用的是超级管理员账户
  ![安装证书](Images/Guides/Snipaste_2019-10-12_22-46-37.png)
  ![导入本地计算机](Images/Guides/Snipaste_2019-10-19_15-28-58.png)
  ![储存到受信任的根证书颁发机构](Images/Guides/Snipaste_2019-10-20_23-36-44.png)
- 双击`*.appxbundle`，单击安装，坐和放宽
![安装](Images/Guides/Snipaste_2019-10-13_12-42-40.png)

### 更新应用
- 下载并解压最新的[安装包`(UWP_x.x.x.0_x86_x64_arm_Debug.appxbundle)`](https://github.com/Tangent-90/Coolapk-UWP/releases/latest)
- 双击`*.appxbundle`，单击更新，坐和放宽
![更新](Images/Guides/Snipaste_2019-10-13_16-01-09.png)

### 一键安装
- 在浏览器中打开链接：`ms-appinstaller:?source=https://raw.fastgit.org/Tangent-90/Coolapk-UWP/screenshots/Appxbundle/CoolapkUWP.appxbundle`
- 弹出窗口后单击打开
![弹窗](Images/Guides/Snipaste_2021-03-05_22-26-31.png)
- 单击安装，坐和放宽
![安装](Images/Guides/Snipaste_2019-10-13_12-42-40.png)

## 屏幕截图
- 启动图
![启动图](Images/Screenshots/Snipaste_2020-04-29_14-53-51.png)
- 首页
![头条](Images/Screenshots/Snipaste_2020-06-22_17-11-05.png)
![酷图](Images/Screenshots/Snipaste_2020-06-21_17-13-25.png)
![关注](Images/Screenshots/Snipaste_2020-06-22_17-48-43.png)
- 通知
![通知](Images/Screenshots/Snipaste_2020-06-22_17-14-10.png)
- 搜索
![搜索栏](Images/Screenshots/Snipaste_2020-06-22_17-15-10.png)
![搜索页面](Images/Screenshots/Snipaste_2020-06-21_17-09-49.png)
- 动态
![个人动态](Images/Screenshots/Snipaste_2020-06-22_17-16-03.png)
![动态详情](Images/Screenshots/Snipaste_2020-06-22_17-16-37.png)
- 图文
![图文](Images/Screenshots/Snipaste_2020-06-21_16-52-01.png)
- 问答
![问答](Images/Screenshots/Snipaste_2020-06-21_17-11-33.png)

## 使用到的模块
- [Coolapk-kotlin](https://github.com/bjzhou/Coolapk-kotlin)
- [UWP Community Toolkit](https://github.com/Microsoft/UWPCommunityToolkit/)
- [Win UI](https://github.com/microsoft/microsoft-ui-xaml)
- [Json.NET](https://www.newtonsoft.com/json)
- [QRCoder](https://github.com/codebude/QRCoder)
- [Metro Log](https://github.com/novotnyllc/MetroLog)
- [Color Thief](https://github.com/KSemenenko/ColorThief)

## 鸣谢
- 酷安UWP原作者[@一块小板子](http://www.coolapk.com/u/695942)([Github](https://github.com/oboard))
- OpenCoolapk作者[@roykio](http://www.coolapk.com/u/703542)([Github](https://github.com/roykio))
- CoolapkTokenCrack作者[@ZCKun](http://www.coolapk.com/u/654147)([Github](https://github.com/ZCKun))
- Coolapk-kotlin作者[@bjzhou](http://www.coolapk.com/u/528097)([Github](https://github.com/bjzhou))
- 以及所有为酷安UWP项目做出贡献的同志们
- **铺路尚未成功，同志仍需努力！**
