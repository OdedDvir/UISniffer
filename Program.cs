using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA2;
using FlaUI.UIA3;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Text;

ObservableCollection<ElementViewModel> Elements = new ObservableCollection<ElementViewModel>();
AutomationBase _automation;
AutomationElement _rootElement;

UIElement ScanElements(ElementViewModel e)
{
    var ui = new UIElement();
    ui.Name = e.Name;
    ui.ControlType = e.ControlType;
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
    desktopViewModel.LoadChildren(true);
    Elements.Add(desktopViewModel);
}

Initialize(AutomationType.UIA3);


if (Environment.GetCommandLineArgs().Length < 4)
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
    string jsonUI = JsonConvert.SerializeObject(ScanElements(appRoot));
    File.WriteAllText(outFileName, jsonUI, Encoding.UTF8);

}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}
Console.WriteLine("All Done!");