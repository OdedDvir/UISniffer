﻿using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA2;
using FlaUI.UIA3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ObservableCollection<ElementViewModel> Elements = new ObservableCollection<ElementViewModel>();
            AutomationBase _automation;
            AutomationElement _rootElement;

            UIElement ScanElements(ElementViewModel e)
            {
                var ui = new UIElement();
                ui.Name = e.Name;
                ui.ControlType = e.ControlType;
                //ui.Path = $"{currentPath}/{ui.Name}";
                // ui.Value = e.ItemDetails.ToJson();
                e.LoadChildren(true);
                ui.ChildrenCount = e.Children.Count;
                if (ui.ChildrenCount > 0)
                {
                    ui.Children = new List<UIElement>();
                    foreach (var item in e.Children)
                    {
                        ui.Children.Add(ScanElements(item));
                    }
                }
                return ui;
            }

            void Initialize(AutomationType selectedAutomationType)
            {
                _automation = selectedAutomationType == AutomationType.UIA2 ? (AutomationBase)new UIA2Automation() : new UIA3Automation();
                _rootElement = _automation.GetDesktop();

                var desktopViewModel = new ElementViewModel(_rootElement);
                desktopViewModel.LoadChildren(false);
                Elements.Add(desktopViewModel);
            }

            Initialize(AutomationType.UIA2);


            if (args.Length < 3)
            {
                Console.WriteLine("Syntax:");
                Console.WriteLine("   InspectUI -title <Application_Window_Title> <Output_File_Path>");
                Console.WriteLine("   InspectUI -pid <Process_Id> <Output_File_Path>");
                Console.WriteLine("Examples:");
                Console.WriteLine("   InspectUI -title \"Word\" output.json");
                Console.WriteLine("   InspectUI -pid 20020 output.json");
                return;
            }

            // Get command line arguments
            string switchType = Environment.GetCommandLineArgs()[1];
            string appIdentifier = Environment.GetCommandLineArgs()[2];
            string outFileName = Environment.GetCommandLineArgs()[3];

            Console.WriteLine($"Searching for {appIdentifier}...");
            ElementViewModel appRoot;
            try
            {
                switch (switchType.ToLower())
                {
                    case "-title":
                        //Search by window title
                        appRoot = Elements[0].Children.Where(e => e.Name.Contains(appIdentifier)).First();
                        break;
                    case "-pid":
                        //Search by process Id
                        appRoot = Elements[0].Children.Where(e => e.AutomationElement.Properties.ProcessId.ToString() == appIdentifier).First();
                        break;
                    default:
                        return;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Could not find app.");
                return;
            }
            Console.WriteLine("App found!");
            Console.WriteLine($"Writing JSON to file {outFileName}...");
            try
            {
                appRoot.LoadChildren(true);
                string jsonUI = JsonConvert.SerializeObject(ScanElements(appRoot), Formatting.Indented);
                //string jsonUI = JsonConvert.SerializeObject(appRoot, Formatting.Indented, new JsonSerializerSettings
                //    {
                //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                //        NullValueHandling = NullValueHandling.Ignore,          
                //    }
                //);
                File.WriteAllText(outFileName, jsonUI, Encoding.UTF8);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            Console.WriteLine("All Done!");
        }
    }
}

