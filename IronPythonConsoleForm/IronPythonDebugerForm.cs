using System;
using System.Threading;
using System.Windows.Forms;

namespace PythonConsoleControl
{
  public partial class IronPythonDebugerForm : Form,IronPythonMonitor
  {
    public static void OpenInThread()
    {
      Thread newThread = new Thread(Open);
      newThread.SetApartmentState(ApartmentState.STA);
      newThread.Start();
    }

    public static void Open()
    {
      IronPythonConsoleForm c = new IronPythonConsoleForm();
      c.Show();
    }
    public IronPythonDebugerForm()
    {
      InitializeComponent();
      Load += IronPythonConsoleForm_Load;
      Resize += PythonConsoleForm_Resize;
    }

    private void IronPythonConsoleForm_Load(object sender, EventArgs e)
    {
      AfterLoad();
    }

    public void AfterLoad()
    {
      UpdatePCVSize();
    }

    private void PythonConsoleForm_Resize(object sender, EventArgs e)
    {
      UpdatePCVSize();
    }

    public void UpdatePCVSize()
    {
      if (PCVHost != null)
      {
        PCVHost.Size = ClientSize;
      }
      if(PCV != null)
      {
        PCV.Width = ClientSize.Width;
        PCV.Height = ClientSize.Height;
      }
    }

    #region IronPythonMonitor
    public void ShowIronPythonFile(string FileName)
    {
      PCV.ShowIronPythonFile(FileName);
    }

    public void TracebackEvent(object sender,IPYTracebackEventArgs e)
    {
      PCV.TracebackEvent(this,e);
    }

    public void StartWatching()
    {
      PCV.StartWatching();
    }

    public void StopWatching()
    {
      PCV.StopWatching();
    }

    public void SetMonitorDelay(int delay=20)
    {
      PCV.SetMonitorDelay(delay);
    }
    public void CatchException(Exception ex)
    {

    }
    #endregion
  }
}
