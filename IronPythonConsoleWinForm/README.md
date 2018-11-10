![](http://yanzixiang.github.io/2015/11/13/IronPythonConsole/IronPythonConsoleWinForm.png)

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