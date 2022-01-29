﻿using FlaUI.Core.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UIElement
{
    public string Name { get; set; }
//    public string Path { get; set; }
    public string ControlType { get; set; }
    public int ChildrenCount { get; set; }
    public List<UIElement> Children { get; set; }
}
