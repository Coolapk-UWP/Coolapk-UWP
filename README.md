![酷安](https://coolapk.com/static/images/header-logo.png)

# Coolapk UWP
一个基于UWP的酷安客户端

基于[@一块小板子](http://www.coolapk.com/u/695942)的源码([Github链接](https://github.com/oboard/CoolApk-UWP))

## 现有功能
1. 浏览和下载应用
2. 访问开发人员中心
3. 浏览头条贴
4. 登录帐号
5. 夜间模式

## 屏幕截图
- PC/Hub
  - 启动图
  ![启动图](Screenshots/2019-10-13-123321.png)
  - 头条
  ![头条](Screenshots/2019-10-13-130542.jpg)
  - 应用与游戏
  ![应用与游戏](Screenshots/2019-10-13-125244.jpg)
  - 动态
  ![动态](Screenshots/2019-10-13-124904.jpg)
  - 动态详情
  ![动态详情](Screenshots/2019-10-13-124940.jpg)
  - 搜索
  ![搜索](Screenshots/2019-10-13-132516.jpg)
  - 应用详情
  ![应用详情](Screenshots/批注-2019-10-13-132558.jpg)
- Mobile
  - 启动图
  ![启动图](http://image.coolapk.com/feed/2019/1011/21/1049834_df0515be_0447_7124@480x800.jpeg.m.jpg)
  - 头条
  ![头条](http://image.coolapk.com/feed/2019/1011/21/1049834_51aa286c_0447_7126@480x800.jpeg.m.jpg)
  - 应用与游戏
  ![应用与游戏](http://image.coolapk.com/feed/2019/1011/21/1049834_1157a574_0447_7131@480x800.jpeg.m.jpg)
  - 动态
  ![动态](http://m.qpic.cn/psb?/V11c3we24eJ6Sb/3oTZ12V7J7TBVWAEv7yQLxbfGYdInRsiTPSPbHJ6Eis!/b/dLYAAAAAAAAA&bo=4AEgAwAAAAABB.I!&rf=viewer_4)
  - 搜索
  ![搜索](http://image.coolapk.com/feed/2019/1011/21/1049834_bd0a2a3d_0447_7133@480x800.jpeg.m.jpg)
  - 应用详情
  ![应用详情](http://image.coolapk.com/feed/2019/1011/21/1049834_0e5928bf_0447_7134@480x800.jpeg.m.jpg)

## 支持的语言
中文

## 如何安装应用
### 最低需求
- Windows 10 Build 15063及以上
- 设备需支持ARM/ARM64/x86/x64
- 至少50MB的空余储存空间(用于储存安装包与安装应用)

### 使用应用安装脚本安装应用
- 下载并解压最新的[安装包](https://github.com/Tangent-90/Coolapk-UWP/releases)
- 如果没有应用安装脚本，下载[`Install.ps1`](Install.ps1)到目标目录
![Install.ps1](Screenshots/Snipaste_2019-10-12_22-49-11.png)
- 右击`Install.ps1`，选择“使用PowerShell运行”
- 应用安装脚本将会引导您完成此过程的剩余部分

### 使用应用安装程序安装应用
- 下载并解压最新的[安装包](https://github.com/Tangent-90/Coolapk-UWP/releases)
- [开启旁加载模式](https://www.windowscentral.com/how-enable-windows-10-sideload-apps-outside-store)
  - 如果您想开发UWP应用，您可以开启[开发人员模式](https://docs.microsoft.com/zh-cn/windows/uwp/get-started/enable-your-device-for-development)，**对于大多数不需要做UWP开发的用户来说，开发人员模式是没有必要的**
- 安装*.cer证书到`本地计算机`→`受信任的根证书颁发机构`
  - 这项操作需要用到管理员权限，如果您安装证书时没有用到该权限，则可能是因为您将证书安装到了错误的位置或者您使用的是超级管理员账户
  ![安装证书](Screenshots/Snipaste_2019-10-12_22-46-37.png)
  ![导入本地计算机](Screenshots/Snipaste_2019-10-12_22-47-51.png)
  ![储存到受信任的根证书颁发机构](Screenshots/Snipaste_2019-10-12_22-48-11.png)
- 双击*.appxbundle，单击安装，坐和放宽
## 鸣谢
