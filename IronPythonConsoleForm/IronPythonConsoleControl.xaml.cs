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
    public IronPythonConsoleControl()
    {
      InitializeComponent();
      InitHighLight();
      IsVisibleChanged += IronPythonConsoleControl_IsVisibleChanged;
    }

    private void IronPythonConsoleControl_IsVisibleChanged(object sender, 
      DependencyPropertyChangedEventArgs e)
    {
      if (!PadInited)
      {
        InitPad();
      }
    }

    public static void InitHighLight()
    {
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
    }

    public void InitPad(Action<PythonConsoleHost> hostAction = null)
    {
      Pad = new PythonConsolePad();
      Pad.StartHost();

      if (hostAction != null)
      {
        WithHost(hostAction);
      }

      grid.Children.Add(Pad.Control);
      

      try
      {
        Pad.Control.SyntaxHighlighting = pythonHighlighting;
        IList<IVisualLineTransformer> transformers = Pad.Control.TextArea.TextView.LineTransformers;
        for (int i = 0; i < transformers.Count; ++i)
        {
          if (transformers[i] is HighlightingColorizer)
          {
            transformers[i] = new PythonConsoleHighlightingColorizer(pythonHighlighting, Pad.Control.Document);
          }
        }
      }
      catch (Exception ex)
      {

      }
      PadInited = true;
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
      this.ConsoleInitializedAction = hostAction;
      if (Host != null)
      {
        Host.ConsoleCreated += new ConsoleCreatedEventHandler(Host_ConsoleCreated);
      }
    }

    Action<PythonConsoleHost> ConsoleInitializedAction;

    void Host_ConsoleCreated(object sender, EventArgs e)
    {
      if (Console != null)
      {
        Console.ConsoleInitialized += new ConsoleInitializedEventHandler(Console_ConsoleInitialized);
      }
    }

    void Console_ConsoleInitialized(object sender, EventArgs e)
    {
      ConsoleInitializedAction.Invoke(Host);
    }

    public PythonConsole Console
    {
      get
      {
        if (Pad != null)
        {
          return Pad.Console;
        }
        else
        {
          return null;
        }
      }
    }

    public PythonConsoleHost Host
    {
      get {
        if (Pad != null)
        {
          return Pad.Host;
        }
        else
        {
          return null;
        }
      }
    }

    private bool PadInited;
    public PythonConsolePad Pad { get; private set; }
    public static IHighlightingDefinition pythonHighlighting;
    #region IPY变量
    public Dictionary<string, object> Variables = new Dictionary<string, object>();
    public void UpdateVariables()
    {
      foreach (KeyValuePair<string, object> v in Variables)
      {
        string tag = v.Key;
        object value = v.Value;
        if(Host != null)
        {
          if(Host.Console != null)
          {
            if (Host.Console.ScriptScope != null)
            {
              Host.Console.ScriptScope.SetVariable(tag, value);
            }
          }
        }
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
      try
      {
        if (Pad != null)
        {
          Pad.Dispose();
        }
      }catch(Exception ex)
      {

      }
    }
  }
}
