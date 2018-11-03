# -*- coding: utf-8 -*-
import clr
from System import *
from System.IO import *
from System.Collections.Generic import *
from System.Diagnostics import *

#Debugger.Launch()
NetHome = "C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\Framework\\.NETFramework\\v4.0\\"
clr.AddReferenceToFileAndPath(NetHome + "System.Drawing.dll")
from System.Drawing import *

def RunOneTime():
  print PM

def init():
  print "init() in init.py"
RunOneTime()
