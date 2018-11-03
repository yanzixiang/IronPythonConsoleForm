using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;

namespace PythonConsoleControl
{
  public partial class IronPythonConsoleControl : UserControl,IDisposable
  {
    PythonConsolePad pad;

    public IronPythonConsoleControl()
    {
      InitializeComponent();
      pad = new PythonConsolePad();
      grid.Children.Add(pad.Control);
      IHighlightingDefinition pythonHighlighting;
      using (Stream s = typeof(IronPythonConsoleControl).Assembly.GetManifestResourceStream("IronPythonConsoleForm.Resources.Python.xshd"))
      {
        if (s == null)
          throw new InvalidOperationException("Could not find embedded resource");
        using (XmlReader reader = new XmlTextReader(s))
        {
          pythonHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
              HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
      }
      // and register it in the HighlightingManager
      HighlightingManager.Instance.RegisterHighlighting("Python Highlighting", new string[] { ".cool" }, pythonHighlighting);
      pad.Control.SyntaxHighlighting = pythonHighlighting;
      IList<IVisualLineTransformer> transformers = pad.Control.TextArea.TextView.LineTransformers;
      for (int i = 0; i < transformers.Count; ++i)
      {
        if (transformers[i] is HighlightingColorizer)
        {
          transformers[i] = new PythonConsoleHighlightingColorizer(pythonHighlighting, pad.Control.Document);
        }
      }
    }

    private void IronPythonConsoleControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      grid.Width = e.NewSize.Width;
      grid.Height = e.NewSize.Height;
      Pad.Control.Width = e.NewSize.Width - 100;
      Pad.Control.InvalidateArrange();
    }

    /// <summary>
    /// Performs the specified action on the console host but only once the console
    /// has initialized.
    /// </summary>
    public void WithHost(Action<PythonConsoleHost> hostAction)
    {
      this.hostAction = hostAction;
      Host.ConsoleCreated += new ConsoleCreatedEventHandler(Host_ConsoleCreated);
    }

    Action<PythonConsoleHost> hostAction;

    void Host_ConsoleCreated(object sender, EventArgs e)
    {
      Console.ConsoleInitialized += new ConsoleInitializedEventHandler(Console_ConsoleInitialized);
    }

    void Console_ConsoleInitialized(object sender, EventArgs e)
    {
      hostAction.Invoke(Host);
    }

    public PythonConsole Console
    {
      get { return pad.Console; }
    }

    public PythonConsoleHost Host
    {
      get { return pad.Host; }
    }

    public PythonConsolePad Pad
    {
      get { return pad; }
    }
    #region IPY变量
    public Dictionary<string, object> Variables = new Dictionary<string, object>();
    public void UpdateVariables()
    {
      foreach (KeyValuePair<string, object> v in Variables)
      {
        string tag = v.Key;
        object value = v.Value;
        Host.Console.ScriptScope.SetVariable(tag, value);
      }
    }
    public void SetVariable(string tag, object value)
    {
      Variables[tag] = value;
    }
    #endregion IPY变量

    public void Print(string s)
    {
      string state = "print " + s;
      Host.Console.RunStatements(state);
    }

    public void Dispose()
    {
      pad.Dispose();
    }
  }
}
