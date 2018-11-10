![](http://yanzixiang.github.io/2015/11/13/IronPythonConsole/IronPythonConsoleWPF.png)

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

<a href="http://www.wtfpl.net/"><img
       src="http://www.wtfpl.net/wp-content/uploads/2012/12/logo-220x1601.png"
       width="220px" height="160px" alt="WTFPL" /></a>