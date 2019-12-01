<img alt="fluentterminal logo" src="./logo.png" width="200px" />

# Coolapk UWP
一个基于UWP的酷安客户端

基于[@一块小板子](http://www.coolapk.com/u/695942)的源码([Github](https://github.com/oboard/CoolApk-UWP))

<a href="https://github.com/Tangent-90/Coolapk-UWP/blob/master/LICENSE"><img alt="GitHub" src="https://img.shields.io/github/license/Tangent-90/Coolapk-UWP.svg?label=License&style=flat-square"></a>
<a href="https://github.com/Tangent-90/Coolapk-UWP/issues"><img alt="GitHub" src="https://img.shields.io/github/issues/Tangent-90/Coolapk-UWP.svg?label=Issues&style=flat-square"></a>
<a href="https://github.com/Tangent-90/Coolapk-UWP/stargazers"><img alt="GitHub" src="https://img.shields.io/github/stars/Tangent-90/Coolapk-UWP.svg?label=Stars&style=flat-square"></a>

<a href="https://github.com/Tangent-90/Coolapk-UWP/releases/latest"><img alt="GitHub All Releases" src="https://img.shields.io/github/downloads/Tangent-90/Coolapk-UWP/total.svg?label=DOWNLOAD&logo=github&style=for-the-badge"></a>
<a href="https://pan.baidu.com/s/1Wjy-CUfjm0sOHCKLwQEALQ"><img alt="Baidu Netdisk" src="https://img.shields.io/badge/download-%e5%af%86%e7%a0%81%ef%bc%9alIIl-magenta.svg?label=%e4%b8%8b%e8%bd%bd&logo=baidu&style=for-the-badge"></a>

## 目录
- [现有功能](#现有功能)
- [支持的语言](#支持的语言)
- [如何安装应用](#如何安装应用)
  - [最低需求](#最低需求)
  - [使用应用安装脚本安装应用](#使用应用安装脚本安装应用)
  - [使用应用安装程序安装应用](#使用应用安装程序安装应用)
  - [更新应用](#更新应用)
- [屏幕截图](#屏幕截图)
- [鸣谢](#鸣谢)

## 现有功能
1. 动态磁贴
2. 夜间模式
3. 主题跟随系统
4. 应用内检查更新
5. 登录/发帖/回复/点赞
6. 浏览应用/动态/图文/问答/通知
7. 更多内容请自行发掘🌸🐔

## 支持的语言
中文

## 如何安装应用
### 最低需求
- Windows 10 Build 15063及以上
- 设备需支持ARM/ARM64/x86/x64
- 至少100MB的空余储存空间(用于储存安装包与安装应用)

### 使用应用安装脚本安装应用
- 下载并解压最新的[安装包`(UWP_x.x.x.0_Debug_Test.rar)`](https://github.com/Tangent-90/Coolapk-UWP/releases/latest)
- 如果没有应用安装脚本，下载[`Install.ps1`](Install.ps1)到目标目录
![Install.ps1](Screenshots/Snipaste_2019-10-12_22-49-11.png)
- 右击`Install.ps1`，选择“使用PowerShell运行”
- 应用安装脚本将会引导您完成此过程的剩余部分

### 使用应用安装程序安装应用
- 下载并解压最新的[安装包`(UWP_x.x.x.0_Debug_Test.rar)`](https://github.com/Tangent-90/Coolapk-UWP/releases/latest)
- [开启旁加载模式](https://www.windowscentral.com/how-enable-windows-10-sideload-apps-outside-store)
  - 如果您想开发UWP应用，您可以开启[开发人员模式](https://docs.microsoft.com/zh-cn/windows/uwp/get-started/enable-your-device-for-development)，**对于大多数不需要做UWP开发的用户来说，开发人员模式是没有必要的**
- 安装`Dependencies`文件夹下的适用于您的设备的所有依赖包
![Dependencies](Screenshots/Snipaste_2019-10-13_15-51-33.png)
- 安装`*.cer`证书到`本地计算机`→`受信任的根证书颁发机构`
  - 这项操作需要用到管理员权限，如果您安装证书时没有用到该权限，则可能是因为您将证书安装到了错误的位置或者您使用的是超级管理员账户
  ![安装证书](Screenshots/Snipaste_2019-10-12_22-46-37.png)
  ![导入本地计算机](Screenshots/Snipaste_2019-10-19_15-28-58.png)
  ![储存到受信任的根证书颁发机构](Screenshots/Snipaste_2019-10-20_23-36-44.png)
- 双击`*.appxbundle`，单击安装，坐和放宽
![安装](Screenshots/Snipaste_2019-10-13_12-42-40.png)

### 更新应用
- 下载并解压最新的[安装包`(UWP_x.x.x.0_x86_x64_arm_Debug.appxbundle)`](https://github.com/Tangent-90/Coolapk-UWP/releases/latest)
- 双击`*.appxbundle`，单击安装，坐和放宽
![安装](Screenshots/Snipaste_2019-10-13_16-01-09.png)

## 屏幕截图
- 启动图
![启动图](Screenshots/2019-10-23-127642.png)
- 首页
![头条](Screenshots/批注-2019-11-30-184431.jpg)
![关注](Screenshots/批注-2019-11-30-183949.jpg)
- 通知
![通知](Screenshots/批注-2019-11-30-184511.jpg)
- 搜索
![搜索栏](Screenshots/批注-2019-11-30-182039.jpg)
![搜索页面](Screenshots/批注-2019-11-30-182138.jpg)
- 应用游戏
![应用游戏](Screenshots/批注-2019-11-30-182900.jpg)
- 应用详情
![应用详情](Screenshots/批注-2019-11-30-182933.jpg)
- 动态
![个人动态](Screenshots/批注-2019-11-30-183116.jpg)
![动态详情](Screenshots/批注-2019-11-30-191108.jpg)
- 图文
![图文](Screenshots/批注-2019-11-30-183241.jpg)
- 问答
![问答](Screenshots/批注-2019-11-30-184130.jpg)


## 鸣谢
- 酷安UWP原作者[@一块小板子](http://www.coolapk.com/u/695942)([Github](https://github.com/oboard))
- OpenCoolapk作者[@roykio](http://www.coolapk.com/u/703542)([Github](https://github.com/roykio))
- CoolapkTokenCrack作者[@ZCKun](http://www.coolapk.com/u/654147)([Github](https://github.com/ZCKun))
- Coolapk-kotlin作者[@bjzhou](http://www.coolapk.com/u/528097)([Github](https://github.com/bjzhou))
- 以及所有为酷安UWP项目做出贡献的同志们
- **铺路尚未成功，同志仍需努力！**
