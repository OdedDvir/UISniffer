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
        static void ShowSyntax()
        {
            Console.WriteLine("Syntax:");
            Console.WriteLine("   InspectUI -all <Output_File_Path>");
            Console.WriteLine("   InspectUI -title <Application_Window_Title> <Output_File_Path>");
            Console.WriteLine("   InspectUI -pid <Process_Id> <Output_File_Path>");
            Console.WriteLine("Examples:");
            Console.WriteLine("   InspectUI -all output.json");
            Console.WriteLine("   InspectUI -title \"Word\" output.json");
            Console.WriteLine("   InspectUI -pid 20020 output.json");
        }

        static void Main(string[] args)
        {
            ObservableCollection<ElementViewModel> Elements = new ObservableCollection<ElementViewModel>();
            AutomationBase _automation;
            AutomationElement _rootElement;

            UIElement ScanElements(ElementViewModel e)
            {
                var ui = new UIElement();
                ui.Name = e.Name;
                ui.ControlType = e.ControlType.ToString();
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

            int argsCount = Environment.GetCommandLineArgs().Count();
            // Get command line arguments
            if (argsCount < 3)
            {
                ShowSyntax();
                return;
            }
            string switchType = Environment.GetCommandLineArgs()[1];
            string outFileName;
            string appIdentifier = "Testing";

            switch (argsCount)
            {
                case 3:
                    outFileName = Environment.GetCommandLineArgs()[2];
                    break;
                case 4:
                    appIdentifier = Environment.GetCommandLineArgs()[2];
                    outFileName = Environment.GetCommandLineArgs()[3];
                    break;
                default:
                    ShowSyntax();
                    return;
            }

            Initialize(AutomationType.UIA2);
            ElementViewModel appRoot;
            try
            {
                switch (switchType.ToLower())
                {
                    case "-title":
                        //Search by window title
                        Console.Write($"Searching for {appIdentifier}... ");
                        appRoot = Elements[0].Children.Where(e => e.Name.Contains(appIdentifier)).First();
                        break;
                    case "-pid":
                        //Search by process Id
                        Console.Write($"Searching for {appIdentifier}... ");
                        appRoot = Elements[0].Children.Where(e => e.AutomationElement.Properties.ProcessId.ToString() == appIdentifier).First();
                        break;
                    case "-all":
                        Console.Write($"Inspecting all apps... ");
                        appRoot = Elements[0];
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
            Console.WriteLine("Ok!");
            Console.WriteLine($"Writing JSON to file {outFileName}. Please be patient...");
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

