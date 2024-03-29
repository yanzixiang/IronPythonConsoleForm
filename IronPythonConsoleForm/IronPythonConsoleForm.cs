﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;

namespace PythonConsoleControl
{
  public partial class IronPythonConsoleForm : Form
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
    public IronPythonConsoleForm()
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
  }
}
