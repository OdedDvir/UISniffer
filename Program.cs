using FlaUI.Core;
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
                e.LoadChildren(true);
                var ui = new UIElement
                {
                    Name = e.Name,
                    ControlType = e.ControlType.ToString(),
                    ChildrenCount = e.Children.Count
                };
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
            string switchType = String.Empty;
            string outFileName;
            string appIdentifier = String.Empty;
            // Get command line arguments
            switch (argsCount)
            {
                case 1:
                    switchType = "-all";
                    outFileName = $"{Directory.GetCurrentDirectory()}\\AllApps.json";
                    break;
                case 3:
                    switchType = Environment.GetCommandLineArgs()[1];
                    outFileName = Environment.GetCommandLineArgs()[2];
                    break;
                case 4:
                    switchType = Environment.GetCommandLineArgs()[1];
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
                        Console.Write($"Searching for {appIdentifier} ... ");
                        appRoot = Elements[0].Children.Where(e =>  e.Name != null && e.Name.Contains(appIdentifier)).First();
                        break;
                    case "-pid":
                        //Search by process Id
                        Console.Write($"Searching for {appIdentifier} ... ");
                        appRoot = Elements[0].Children.Where(e => e.AutomationElement.Properties.ProcessId.ToString() == appIdentifier).First();
                        break;
                    case "-all":
                        Console.Write($"Inspecting all apps... ");
                        appRoot = Elements[0];
                        break;
                    default:
                        ShowSyntax();
                        return;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Could not find app.");
                return;
            }
            Console.WriteLine("Ok!");
            try
            {
                Console.WriteLine("Scanning. Please be patient...");
                StringBuilder jsonUI = new StringBuilder();
                int i = 1;
                appRoot.LoadChildren(false);
                foreach (ElementViewModel item in appRoot.Children)
                {
                    Console.WriteLine($"Scanning item {i++} : {item.Name} ...");
                    try
                    {
                        jsonUI.Append(JsonConvert.SerializeObject(ScanElements(item), Formatting.Indented));
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                Console.WriteLine($"Writing JSON to file {outFileName}.");
                File.WriteAllText(outFileName, jsonUI.ToString(), Encoding.UTF8);
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

