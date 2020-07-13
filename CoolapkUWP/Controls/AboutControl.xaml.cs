using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Controls
{
    public sealed partial class AboutControl : UserControl
    {
        public AboutControl()
        {
            this.InitializeComponent();

            main.Text =
@"
#### 欢迎使用酷安 UWP！

##### 声明

1. 本程序是[酷安](https://coolapk.com)的第三方客户端，仅用作学习交流使用，禁止用于商业用途。
2. 本程序是开源软件，因此，在使用时请确保程序是来自[本Github仓库](https://github.com/Tangent-90/Coolapk-UWP)，以确保您的数据安全。
3. 若程序来源无异常，程序运行过程中您的所有数据都仅用于与酷安的服务器交流或储存于本地，开发者不会窃取您的任何数据。但即便如此，也请注意使用环境的安全性。
4. 若您对[酷安](https://coolapk.com)如何处理您的数据存在疑虑，请访问[酷安用户服务协议](https://m.coolapk.com/mp/user/agreement)、[酷安隐私保护政策](https://coolapk.com/about/privacy.html)。

##### 鸣谢

|                                       贡献                                     |                作者               |
| ----------------------------------------------------------------------------- | -------------------------------- |
| [原作(CoolApk-UWP)](https://github.com/oboard/CoolApk-UWP)                     | [oboard](https://github.com/oboard)|
| [Token获取方法(CoolapkTokenCrack)](https://github.com/ZCKun/CoolapkTokenCrack) | [ZCKun](https://github.com/ZCKun/)|

##### 引用及参考

- [Coolapk-kotlin](https://github.com/bjzhou/Coolapk-kotlin)
- [UWP Community Toolkit](https://github.com/Microsoft/UWPCommunityToolkit/)
- [Win UI](https://github.com/microsoft/microsoft-ui-xaml)
- [Json.NET](https://www.newtonsoft.com/json)
- [QRCoder](https://github.com/codebude/QRCoder)
- [Metro Log](https://github.com/novotnyllc/MetroLog)
";
        }
    }
}