// Copyright (c) 2010 Joe Moorhouse

using System;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Reflection;

using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Hosting.Shell;

using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;

namespace PythonConsoleControl
{
  public delegate void ConsoleCreatedEventHandler(object sender, EventArgs e);

  /// <summary>
  /// Hosts the python console.
  /// </summary>
  public class PythonConsoleHost : ConsoleHost, IDisposable
  {
    Thread thread;
    PythonTextEditor textEditor;

    public event ConsoleCreatedEventHandler ConsoleCreated;

    public PythonConsoleHost(PythonTextEditor textEditor)
    {
      this.textEditor = textEditor;

    }

    /// <summary>
    /// yzx
    /// </summary>
    /// <param name="frame"></param>
    /// <param name="result"></param>
    /// <param name="payload"></param>
    /// <returns></returns>
    private TracebackDelegate OnTracebackReceived(TraceBackFrame frame, string result, object payload)
    {
      System.Console.WriteLine(frame.f_lineno);
      return OnTracebackReceived;
    }
    public PythonConsole Console { get; private set; }

    protected override Type Provider
    {
      get { return typeof(PythonContext); }
    }

    /// <summary>
    /// Runs the console host in its own thread.
    /// </summary>
    public void Run()
    {
      thread = new Thread(RunConsole);
      thread.IsBackground = true;
      thread.Start();
    }

    public void Stop()
    {
      try
      {
        thread.Abort();
        thread = null;
      }catch(Exception ex)
      {

      }
    }

    protected virtual void Dispose(bool gc)
    {
      if (Console != null)
      {
        Console.Dispose();
        if (gc)
        {
          GC.SuppressFinalize(this);
        }
      }

      if (thread != null)
      {
        thread.Join();
      }
    }

    protected override CommandLine CreateCommandLine()
    {
      return new PythonCommandLine();
    }

    protected override OptionsParser CreateOptionsParser()
    {
      return new PythonOptionsParser();
    }

    /// <remarks>
    /// After the engine is created the standard output is replaced with our custom Stream class so we
    /// can redirect the stdout to the text editor window.
    /// This can be done in this method since the Runtime object will have been created before this method
    /// is called.
    /// </remarks>
    protected override IConsole CreateConsole(
      ScriptEngine engine, 
      CommandLine commandLine,
      ConsoleOptions options)
    {
      SetOutput(new PythonOutputStream(textEditor));
      Console = new PythonConsole(textEditor, commandLine);
      ConsoleCreated?.Invoke(this, EventArgs.Empty);
      Runtime.SetTrace(OnTracebackReceived);//yzx
      return Console;
    }

    protected virtual void SetOutput(PythonOutputStream stream)
    {
      Runtime.IO.SetOutput(stream, Encoding.UTF8);
    }

    /// <summary>
    /// Runs the console.
    /// </summary>
    [DebuggerStepThrough]
    public void RunConsole()
    {
      try
      {
        this.Run(new string[] { "-X:FullFrames" });
      }catch(Exception ex)
      {

      }
    }

    protected override ScriptRuntimeSetup CreateRuntimeSetup()
    {
      ScriptRuntimeSetup srs = ScriptRuntimeSetup.ReadConfiguration();
      foreach (var langSetup in srs.LanguageSetups)
      {
        if (langSetup.FileExtensions.Contains(".py"))
        {
          langSetup.Options["SearchPaths"] = new string[0];
        }
      }
      return srs;
    }

    protected override void ParseHostOptions(string[] args)
    {
      // Python doesn't want any of the DLR base options.
      foreach (string s in args)
      {
        Options.IgnoredArgs.Add(s);
      }
    }

    [DebuggerStepThrough]
    protected override void ExecuteInternal()
    {
      var pc = HostingHelpers.GetLanguageContext(Engine) as PythonContext;
      pc.SetModuleState(typeof(ScriptEngine), Engine);
      Type t = pc.GetType();
      Assembly ass = t.Assembly;
      try {
        base.ExecuteInternal();
      }
      catch (ImportException iex)
      {
        Console.WriteLine(iex.ToString(), Style.Warning);
      }
    }

    public void Dispose()
    {
      Dispose(true);
    }
  }
}
