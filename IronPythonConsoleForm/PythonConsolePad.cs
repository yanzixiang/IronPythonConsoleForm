// Copyright (c) 2010 Joe Moorhouse

using ICSharpCode.AvalonEdit;
using System.Windows.Media;
using System.Windows.Controls;

namespace PythonConsoleControl
{
  public class PythonConsolePad
  {
    PythonTextEditor pythonTextEditor;
    TextEditor textEditor;
    PythonConsoleHost host;

    public PythonConsolePad()
    {
      textEditor = new TextEditor();
      pythonTextEditor = new PythonTextEditor(textEditor);
      host = new PythonConsoleHost(pythonTextEditor);
      host.Run();
      textEditor.FontFamily = new FontFamily("Consolas");
      textEditor.FontSize = 14;
      textEditor.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
      textEditor.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
      textEditor.WordWrap = true;
    }

    public TextEditor Control
    {
      get { return textEditor; }
    }

    public PythonConsoleHost Host
    {
      get { return host; }
    }

    public PythonConsole Console
    {
      get { return host.Console; }
    }

    public void Dispose()
    {
      host.Dispose();
    }
  }
}
