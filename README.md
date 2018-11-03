IronPython 控制台控件

# 原因和目的

最早的项目为 https://code.google.com/p/ironlab/ ，后来没有人维护了。希望在 WPF 和 Winform 程序里面嵌入 IronPython 控制台,在控件打开之前设置好给 IronPython 所使用的变量，比较方便调试程序，在程序启动后想要进行 API 调用可以直接在 IronPython 控制台里输入即可。

# 用法
## WPF
```
<Window x:Class="IronPythonConsoleWPF.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:IronPythonConsoleWPF"
        xmlns:IPCC="clr-namespace:PythonConsoleControl;assembly=IronPythonConsoleForm"
        mc:Ignorable="d"
        Title="MainWindow" Height="450" Width="800">
    <Grid>
    <IPCC:IronPythonConsoleControl x:Name="Console"></IPCC:IronPythonConsoleControl>
    </Grid>
</Window>
```

## Winform
```
using System;
using System.Windows.Forms;

using YZXLogicEngine;

namespace IronPythonConsoleWinForm
{
  static class Program
  {
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      var form = new IronPythonConsoleForm();
      form.PCV.SetVariable("PM", "1234");

      Application.Run(form);
    }
  }
}
```

<a href="http://www.wtfpl.net/"><img
       src="http://www.wtfpl.net/wp-content/uploads/2012/12/logo-220x1601.png"
       width="220px" height="160px" alt="WTFPL" /></a>