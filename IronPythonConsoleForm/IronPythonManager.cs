using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;

using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Hosting;

namespace PythonConsoleControl
{
  public class IronPythonManager
  {
    private static IronPythonManager instance;
    public static IronPythonManager Instance
    {
      get
      {
        if (instance == null)
        {
          instance = new IronPythonManager();
          instance.InitIronPython();

        }
        return instance;
      }
    }

    [XmlIgnore]
    public ScriptEngine Engine;

    [XmlIgnore]

    CodeContext codeContext = DefaultContext.Default;

    public object GetVariable(string key, string name)
    {
      try
      {
        ScriptScope Scope = ScriptScopes[key];
        return Scope.GetVariable(name);
      }
      catch (Exception ex)
      {
        return null;
      }
    }

    public void InitIronPython()
    {
      try
      {
        Engine = Python.CreateEngine();
        ICollection<string> sps = Engine.GetSearchPaths();
        sps.Add(@"Lib");
        sps.Add(@"C:\Program Files (x86)\IronPython 2.7\Lib");
        Engine.SetSearchPaths(sps);

        if (codeContext == null)
        {
          PythonDictionary keyValues = new PythonDictionary();
          ModuleContext context = new ModuleContext(keyValues, DefaultContext.DefaultPythonContext);
          codeContext = new CodeContext(keyValues, context);
        }

        Engine.Runtime.SetTrace(OnTracebackReceived);
      }
      catch (ImportException e)
      {
        throw e;
      }
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
      IPYTracebackEventArgs args = new IPYTracebackEventArgs();
      args.frame = frame;
      args.result = result;
      args.payload = payload;
      foreach (var monitorList in Monitors.Values)
      {
        foreach (var monitor in monitorList)
        {
          monitor.TracebackEvent(monitor,args);
        }
      }
      return OnTracebackReceived;
    }

    public Dictionary<string, ScriptScope> ScriptScopes = new Dictionary<string, ScriptScope>();
    Dictionary<string, ScriptSource> ScriptSources = new Dictionary<string, ScriptSource>();
    Dictionary<string, CompiledCode> CompiledCodes = new Dictionary<string, CompiledCode>();
    public Dictionary<string, List<IronPythonMonitor>> Monitors = new Dictionary<string, List<IronPythonMonitor>>();

    public void RunFile(string path,bool runInit = false,string Key="")
    {
      try
      {
        ScriptScope Scope = Engine.CreateScope();
        if (Key == "")
        {
          Key = path;
        }
        ScriptScopes[Key] = Scope;

        ScriptSource scriptSource = Engine.CreateScriptSourceFromFile(path);
        CompiledCode compiledCode = scriptSource.Compile();
        compiledCode.Execute(Scope);

        ScriptSources[path] = scriptSource;
        CompiledCodes[path] = compiledCode;

        if (runInit)
        {
          PythonFunction InitAction = (PythonFunction)GetVariable(path, "Init");
          if (InitAction != null)
          {
            PythonCalls.Call(codeContext, InitAction);
          }
        }
      }catch(Exception ex)
      {
        ExceptionOperations eo = Engine.GetService<ExceptionOperations>();
        string error = eo.FormatException(ex);
        throw ex;
      }
    }

    public void RunInit(string path)
    {
      RunFunction(path, "Init");
    }

    public void RunAction(string key,string actionName, params object[] args)
    {
      try
      {
        PythonFunction functionAction = (PythonFunction)GetVariable(key, actionName);
        if (functionAction != null)
        {
          PythonCalls.Call(codeContext, functionAction,args);
        }
      }
      catch (Exception ex)
      {
        if (CheckIfHasMonitor(key))
        {
          MonitorCatchException(key, ex);
        }
        ExceptionOperations eo = Engine.GetService<ExceptionOperations>();
        string error = eo.FormatException(ex);
        throw ex;
      }
    }

    public Task RunActionAsync(string key, string actionName)
    {
      var action = (Action)(() => {
        RunAction(key, actionName);
      });

      Task task = Task.Factory.StartNew(action, TaskCreationOptions.None);
      return task;
    }

    public object RunFunction(string key,string functionName, params object[] args)
    {
      try
      {
        PythonFunction functionAction = (PythonFunction)GetVariable(key,functionName);
        if (functionAction != null)
        {
          return PythonCalls.Call(codeContext, functionAction,args);
        }
        else
        {
          return null;
        }
      }
      catch (Exception ex)
      {
        if (CheckIfHasMonitor(key))
        {
          MonitorCatchException(key, ex);
        }
        ExceptionOperations eo = Engine.GetService<ExceptionOperations>();
        string error = eo.FormatException(ex);
        throw ex;
      }
    }

    public Task<object> RunFunctionAsync(string key, string functionName)
    {
      var func = (Action)(() => {
        RunAction(key, functionName);
      });

      Task<object> task = Task.Factory.StartNew(func, TaskCreationOptions.None) as Task<object>;
      return task;
    }

    #region Monitor
    public void AddMonitor(string key, IronPythonMonitor monitor)
    {
      if (!Monitors.ContainsKey(key))
      {
        Monitors[key] = new List<IronPythonMonitor>();
      }

      var list = Monitors[key];
      if (list.Contains(monitor))
      {
        list.Add(monitor);
      }
    }

    public void RemoveMonitor(string key, IronPythonMonitor monitor)
    {
      if (Monitors.ContainsKey(key))
      {
        var list = Monitors[key];
        if (list.Contains(monitor))
        {
          list.Remove(monitor);
        }
      }
    }

    public bool CheckIfHasMonitor(string key)
    {
      if (Monitors.ContainsKey(key))
      {
        var list = Monitors[key];
        if(list.Count > 0)
        {
          return true;
        }
      }
      return false;
    }

    public bool MonitorCatchException(string key,Exception exception)
    {
      if (Monitors.ContainsKey(key))
      {
        var list = Monitors[key];
        foreach(var monitor in list)
        {
          monitor.CatchException(exception);
        }
      }
      return false;
    }
    #endregion
  }
}
