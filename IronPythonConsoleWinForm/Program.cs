using System;
using System.Windows.Forms;

using PythonConsoleControl;

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
