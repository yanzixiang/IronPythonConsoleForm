using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;

namespace YZXLogicEngine
{
  public partial class IronPythonConsoleForm : Form
  {
    public static void OpenInThread()
    {
      Thread newThread = new Thread(Open);
      newThread.SetApartmentState(ApartmentState.STA);
      newThread.Start();
    }

    Dispatcher Dispatcher;

    public static void Open()
    {
      IronPythonConsoleForm c = new IronPythonConsoleForm();
      c.Show();
    }
    public IronPythonConsoleForm()
    {
      InitializeComponent();
      Load += PythonConsoleForm_Load;
      Resize += PythonConsoleForm_Resize;
    }

    private void PythonConsoleForm_Load(object sender, EventArgs e)
    {
      UpdatePCVSize();
      PCV.Pad.Host.ConsoleCreated += Host_ConsoleCreated;
    }


    private void Host_ConsoleCreated(object sender, EventArgs e)
    {
      PCV.Pad.Host.Console.ConsoleInitialized += Console_ConsoleInitialized;
    }
    private void Console_ConsoleInitialized(object sender, EventArgs e)
    {
      if(Dispatcher != null)
      {
        PCV.Console.SetDispatcher(Dispatcher);
      }

      

      PCV.SetVariable("self", this);

      PCV.UpdateVariables();

      var console = PCV.Pad.Console;
      BeginInvoke((Action)(() =>
      {
        PCV.Pad.Control.WordWrap = true;
        console.ExecuteFile(initFile);
        this.Activate();
      }));
    }

    private void PythonConsoleForm_Resize(object sender, EventArgs e)
    {
      UpdatePCVSize();
    }

    public void UpdatePCVSize()
    {
      PCVHost.Size = ClientSize;
      PCV.Width = ClientSize.Width;
      PCV.Height = ClientSize.Height;
    }

    private string initFile = "IronPython\\init.py";
    public string InitFile
    {
      get
      {
        return initFile;
      }
      set
      {
        initFile = value;
      }
    }
  }
}
