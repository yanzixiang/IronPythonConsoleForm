// Copyright (c) 2010 Joe Moorhouse

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;
using System.Threading;
using System.Diagnostics;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.CodeCompletion;

using Microsoft.Scripting;

namespace PythonConsoleControl
{
  /// <summary>
  /// Interface console to AvalonEdit and handle autocompletion.
  /// </summary>
  public class PythonTextEditor
  {
    internal TextEditor textEditor;
    internal TextArea textArea;
    StringBuilder writeBuffer = new StringBuilder();
    volatile bool writeInProgress = false;
    int completionEventIndex = 0;
    int descriptionEventIndex = 1;
    WaitHandle[] completionWaitHandles;
    AutoResetEvent completionRequestedEvent = new AutoResetEvent(false);
    AutoResetEvent descriptionRequestedEvent = new AutoResetEvent(false);

    public PythonTextEditor(TextEditor textEditor)
    {
      this.textEditor = textEditor;
      this.textArea = textEditor.TextArea;
      completionWaitHandles = new WaitHandle[] { completionRequestedEvent, descriptionRequestedEvent };
      textEditor.IsVisibleChanged += TextEditor_IsVisibleChanged;
    }

    private void StartCompletionThread()
    {
      CompletionThread = new Thread(new ThreadStart(Completion));
      CompletionThread.Priority = ThreadPriority.Lowest;
      CompletionThread.SetApartmentState(ApartmentState.STA);
      CompletionThread.IsBackground = true;
      CompletionThread.Start();
    }

    private void StopCompletionThread()
    {
      if (CompletionThread != null)
      {
        try
        {
          CompletionThread.Abort();
          CompletionThread = null;
        }catch(Exception ex)
        {

        }
      }
    }

    private void TextEditor_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if ((bool)e.NewValue)
      {
        StartCompletionThread();
      }
      else
      {
        StopCompletionThread();
      }
    }

    public bool WriteInProgress
    {
      get { return writeInProgress; }
    }

    public ICollection<CommandBinding> CommandBindings
    {
      get { return (this.textArea.ActiveInputHandler as TextAreaDefaultInputHandler).CommandBindings; }
    }

    public void Write(string text)
    {
      Write(text, false);
    }

    Stopwatch sw;

    public void Write(string text, bool allowSynchronous)
    {
      //text = text.Replace("\r\r\n", "\r\n");
      text = text.Replace("\r\r\n", "\r");
      text = text.Replace("\r\n", "\r");
      if (text == ">>>")
      {
        text = "";
      }
      if (allowSynchronous)
      {
        MoveToEnd();
        PerformTextInput(text);
        return;
      }
      lock (writeBuffer)
      {
        writeBuffer.Append(text);
      }
      if (!writeInProgress)
      {
        writeInProgress = true;
        ThreadPool.QueueUserWorkItem(new WaitCallback(CheckAndOutputWriteBuffer));
        sw = Stopwatch.StartNew();
      }
    }

    private void CheckAndOutputWriteBuffer(object stateInfo)
    {
      AutoResetEvent writeCompletedEvent = new AutoResetEvent(false);
      Action action = new Action(delegate()
      {
        string toWrite;
        lock (writeBuffer)
        {
          toWrite = writeBuffer.ToString();
          writeBuffer.Remove(0, writeBuffer.Length);
          //writeBuffer.Clear();
        }
        MoveToEnd();
        PerformTextInput(toWrite);
        writeCompletedEvent.Set();
      });
      while (true)
      {
        // Clear writeBuffer and write out.
        textArea.Dispatcher.BeginInvoke(action, DispatcherPriority.Normal);
        // Check if writeBuffer has refilled in the meantime; if so clear and write out again.
        writeCompletedEvent.WaitOne();
        lock (writeBuffer)
        {
          if (writeBuffer.Length == 0)
          {
            writeInProgress = false;
            break;
          }
        }
      }
    }

    private void MoveToEnd()
    {
      int lineCount = textArea.Document.LineCount;
      if (textArea.Caret.Line != lineCount) textArea.Caret.Line = textArea.Document.LineCount;
      int column = textArea.Document.Lines[lineCount - 1].Length + 1;
      if (textArea.Caret.Column != column) textArea.Caret.Column = column;
    }

    public void PerformTextInput(string text)
    {
      if (text == "\n" || text == "\r\n")
      {
        string newLine = TextUtilities.GetNewLineFromDocument(textArea.Document, textArea.Caret.Line);
        this.textEditor.AppendText(newLine);
        //using (textArea.Document.RunUpdate())
        //{

        //    textArea.Selection.ReplaceSelectionWithText(textArea, newLine);
        //}
      }
      else
        this.textEditor.AppendText(text);
      textArea.Caret.BringCaretToView();
    }

    public int Column
    {
      get { return textArea.Caret.Column; }
      set { textArea.Caret.Column = value; }
    }

    /// <summary>
    /// Gets the current cursor line.
    /// </summary>
    public int Line
    {
      get { return textArea.Caret.Line; }
      set { textArea.Caret.Line = value; }
    }

    /// <summary>
    /// Gets the total number of lines in the text editor.
    /// </summary>
    public int TotalLines
    {
      get { return textArea.Document.LineCount; }
    }

    public delegate string StringAction();
    /// <summary>
    /// Gets the text for the specified line.
    /// </summary>
    public string GetLine(int index)
    {
      return (string)textArea.Dispatcher.Invoke(new StringAction(delegate()
      {
        DocumentLine line = textArea.Document.Lines[index];
        return textArea.Document.GetText(line);
      }));
    }

    /// <summary>
    /// Replaces the text at the specified index on the current line with the specified text.
    /// </summary>
    public void Replace(int index, int length, string text)
    {
      //int currentLine = textArea.Caret.Line - 1;
      int currentLine = textArea.Document.LineCount - 1;
      int startOffset = textArea.Document.Lines[currentLine].Offset;
      textArea.Document.Replace(startOffset + index, length, text);
    }

    public event TextCompositionEventHandler TextEntering
    {
      add { textArea.TextEntering += value; }
      remove { textArea.TextEntering -= value; }
    }

    public event TextCompositionEventHandler TextEntered
    {
      add { textArea.TextEntered += value; }
      remove { textArea.TextEntered -= value; }
    }

    public event KeyEventHandler PreviewKeyDown
    {
      add { textArea.PreviewKeyDown += value; }
      remove { textArea.PreviewKeyDown -= value; }
    }

    public int SelectionStart
    {
      get
      {
        return textArea.Selection.SurroundingSegment.Offset;
      }
    }

    public int SelectionLength
    {
      get
      {
        return textArea.Selection.Length;
      }
    }

    public bool SelectionIsMultiline
    {
      get
      {
        //return textArea.Selection.IsMultiline(textArea.Document);
        return textArea.Selection.IsMultiline;
      }
    }

    public int SelectionStartColumn
    {
      get
      {
        int startOffset = textArea.Selection.SurroundingSegment.Offset;
        return startOffset - textArea.Document.GetLineByOffset(startOffset).Offset + 1;
      }
    }

    public int SelectionEndColumn
    {
      get
      {
        int endOffset = textArea.Selection.SurroundingSegment.EndOffset;
        return endOffset - textArea.Document.GetLineByOffset(endOffset).Offset + 1;
      }
    }

    public PythonConsoleCompletionDataProvider CompletionProvider { get; set; } = null;

    public Thread CompletionThread { get; private set; }

    public bool StopCompletion()
    {
      if (CompletionProvider.AutocompletionInProgress)
      {
        // send Ctrl-C abort
        CompletionThread.Abort(new KeyboardInterruptException(""));
        return true;
      }
      return false;
    }

    public PythonConsoleCompletionWindow CompletionWindow { get; private set; } = null;

    public void ShowCompletionWindow()
    {
      completionRequestedEvent.Set();
    }

    public void UpdateCompletionDescription()
    {
      descriptionRequestedEvent.Set();
    }

    /// <summary>
    /// Perform completion actions on the background completion thread.
    /// </summary>
    [DebuggerStepThrough]
    void Completion()
    {
      while (true)
      {
        try
        {
          int action = WaitHandle.WaitAny(completionWaitHandles);
          if (action == completionEventIndex && CompletionProvider != null) BackgroundShowCompletionWindow();
          if (action == descriptionEventIndex && CompletionProvider != null && CompletionWindow != null) BackgroundUpdateCompletionDescription();
        }catch(Exception ex)
        {

        }
      }
    }

    /// <summary>
    /// Obtain completions (this runs in its own thread)
    /// </summary>
    internal void BackgroundShowCompletionWindow() //ICompletionItemProvider
    {
      // provide AvalonEdit with the data:
      string itemForCompletion = "";
      textArea.Dispatcher.Invoke(new Action(delegate()
      {
        DocumentLine line = textArea.Document.Lines[textArea.Caret.Line - 1];
        itemForCompletion = textArea.Document.GetText(line);
      }));

      ICompletionData[] completions = CompletionProvider.GenerateCompletionData(itemForCompletion);

      if (completions != null && completions.Length > 0) textArea.Dispatcher.BeginInvoke(new Action(delegate()
      {
        CompletionWindow = new PythonConsoleCompletionWindow(textArea, this);
        IList<ICompletionData> data = CompletionWindow.CompletionList.CompletionData;
        foreach (ICompletionData completion in completions)
        {
          data.Add(completion);
        }
        CompletionWindow.Show();
        CompletionWindow.Closed += delegate
        {
          CompletionWindow = null;
        };
      }));

    }

    internal void BackgroundUpdateCompletionDescription()
    {
      CompletionWindow.UpdateCurrentItemDescription();
    }

    public void RequestCompletioninsertion(TextCompositionEventArgs e)
    {
      if (CompletionWindow != null) CompletionWindow.CompletionList.RequestInsertion(e);
      // if autocompletion still in progress, terminate
      StopCompletion();
    }

  }
}