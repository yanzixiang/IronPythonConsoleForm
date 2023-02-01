// Copyright (c) 2010 Joe Moorhouse

using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

using ICSharpCode.AvalonEdit;

namespace PythonConsoleControl
{
  public class PythonConsolePad
  {
    PythonTextEditor pythonTextEditor;

    public PythonConsolePad()
    {
      Control = new TextEditor();
      pythonTextEditor = new PythonTextEditor(Control);
      
      Control.FontFamily = new FontFamily("Consolas");
      Control.FontSize = 14;
      Control.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
      Control.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
      Control.WordWrap = true;

      Control.IsVisibleChanged += Control_IsVisibleChanged;
    }

    private void Control_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (Control.IsVisible)
      {
        StartHost();
      }
      else
      {
        StopHost();
      }
    }

    public bool HostStarted = false;
    public void StartHost()
    {
      if (!HostStarted)
      {
        Host = new PythonConsoleHost(pythonTextEditor);
        Host.Run();
        HostStarted = true;
      }
    }

    public void StopHost()
    {
      if (HostStarted)
      {
        Host.Stop();
        Host = null;
        HostStarted = false;
      }
    }

    public TextEditor Control { get; }

    public PythonConsoleHost Host { get; private set; }

    public PythonConsole Console
    {
      get { return Host.Console; }
    }

    public void Dispose()
    {
      if (Host != null)
      {
        Host.Dispose();
      }
    }
  }
}
