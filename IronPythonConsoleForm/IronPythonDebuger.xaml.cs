using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Controls;

using IronPython.Runtime;
using Microsoft.Scripting.Hosting;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Threading;

namespace PythonConsoleControl
{
  public partial class IronPythonDebuger : UserControl, IronPythonMonitor
  {
    TextEditor avalonEdit;
    public string CurrentShowingFile;

    public List<string> AllowedFiles;
    public string CurrentFile { get; private set; }

    public bool Watching { get; private set; } = false;
    public int MonitorDelay { get; private set; } = 50;
    public IronPythonDebuger()
    {
      avalonEdit = new TextEditor();
      avalonEdit.ShowLineNumbers = true;
      avalonEdit.FontFamily = new FontFamily("Consolas");
      avalonEdit.FontSize = 14;
      avalonEdit.WordWrap = true;

      InitializeComponent();
      grid.Children.Add(avalonEdit);

      Loaded += IronPythonDebuger_Loaded;
    }

    protected bool DesignMode
    {
      get
      {
        return DesignerProperties.GetIsInDesignMode(this);
      }
    }

    private void IronPythonDebuger_Loaded(object sender, RoutedEventArgs e)
    {
      if (!DesignMode)
      {
        InitHighLight();
      }
    }

    public void HighlightLine(int linenum, Brush foreground, Brush background)
    {
      //Console.WriteLine(String.Format("HighlightLine : {0}", linenum));

      if (Watching)
      {
        Dispatcher.Invoke(() =>
        {
          if (linenum > avalonEdit.Document.LineCount)
          {
            return;
          }
          try
          {
            avalonEdit.ScrollToLine(linenum);
            DocumentLine line = avalonEdit.Document.GetLineByNumber(linenum);
            avalonEdit.Select(line.Offset, line.TotalLength - 1);
            line = null;
            Thread.Sleep(MonitorDelay);
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex.ToString());
          }
        });
      }
    }

    public void InitHighLight()
    {
      IHighlightingDefinition pythonHighlighting;
      using (Stream s = typeof(IronPythonDebuger).Assembly.GetManifestResourceStream("IronPythonConsoleForm.Resources.Python.xshd"))
      {
        if (s == null)
          throw new InvalidOperationException("Could not find embedded resource");
        using (XmlReader reader = new XmlTextReader(s))
        {
          pythonHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
              HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
      }
      HighlightingManager.Instance.RegisterHighlighting("Python Highlighting",
          new string[] { ".cool" },
          pythonHighlighting);
      avalonEdit.SyntaxHighlighting = pythonHighlighting;
    }

    public void TracebackEvent(object sender, IPYTracebackEventArgs e)
    {
      FunctionCode code = e.frame.f_code;

      string filename = code.co_filename;

      ShowIronPythonFile(filename);

      try
      {
        switch (e.result)
        {
          case "call":
            TracebackCall(e);
            break;

          case "line":
            TracebackLine(e);
            break;

          case "return":
            TracebackReturn(e);
            break;

          default:
            break;
        }
      }
      catch (Exception ex)
      {
      }
    }

    private void TracebackCall(IPYTracebackEventArgs e)
    {
      if (e.frame != null)
      {
        int lineNo = (int)e.frame.f_lineno;
        HighlightLine(lineNo, Brushes.LightGreen, Brushes.Black);
      }
    }

    private void TracebackReturn(IPYTracebackEventArgs e)
    {
      int lineNo = (int)e.frame.f_code.co_firstlineno;
      HighlightLine(lineNo, Brushes.LightPink, Brushes.Black);
    }

    private void TracebackLine(IPYTracebackEventArgs e)
    {
      int lineNo = (int)e.frame.f_lineno;
      HighlightLine(lineNo, Brushes.Yellow, Brushes.Black);
    }

    #region IronPythonMonitor

    public void ShowIronPythonFile(string FileName)
    {
      if (FileName == CurrentFile)
      {
      }
      else
      {
        Dispatcher.Invoke(() =>
        {
          avalonEdit.Load(FileName);
        });
        CurrentFile = FileName;
      }
    }

    public void StartWatching()
    {
      Watching = true;
    }
    public void StopWatching()
    {
      Watching = false;
    }

    public void SetMonitorDelay(int delay)
    {
      MonitorDelay = delay;
    }

    public void CatchException(Exception ex)
    {
      var Engine = IronPythonManager.Instance.Engine;
      ExceptionOperations eo = Engine.GetService<ExceptionOperations>();
      string error = eo.FormatException(ex);
    }
    #endregion
  }
}
