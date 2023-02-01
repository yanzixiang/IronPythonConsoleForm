using System;

namespace PythonConsoleControl
{
  public interface IronPythonMonitor
  {
    void ShowIronPythonFile(string FileName);
    void TracebackEvent(object sender, IPYTracebackEventArgs e);
    void StartWatching();
    void StopWatching();

    void SetMonitorDelay(int delay);

    void CatchException(Exception ex);
  }
}
