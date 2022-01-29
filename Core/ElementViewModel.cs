using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;
using FlaUI.Core.Patterns;
using FlaUI.Core.Tools;
using FlaUI.UIA3.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ElementViewModel : ObservableObject
{
    public event Action<ElementViewModel> SelectionChanged;

    public ElementViewModel(AutomationElement automationElement)
    {
        AutomationElement = automationElement;
        Children = new ExtendedObservableCollection<ElementViewModel>();
        ItemDetails = new ExtendedObservableCollection<ObservableObject>();
    }

    public AutomationElement AutomationElement { get; }

    public string Name => NormalizeString(AutomationElement.Properties.Name.ValueOrDefault);

    public ControlType ControlType => AutomationElement.Properties.ControlType.TryGetValue(out ControlType value) ? value : ControlType.Custom;

    public ExtendedObservableCollection<ElementViewModel> Children { get; set; }

    public ExtendedObservableCollection<ObservableObject> ItemDetails { get; set; }

    public void LoadChildren(bool loadInnerChildren)
    {
        foreach (var child in Children)
        {
            child.SelectionChanged -= SelectionChanged;
        }

        var childrenViewModels = new List<ElementViewModel>();
        try
        {
            foreach (var child in AutomationElement.FindAllChildren())
            {
                var childViewModel = new ElementViewModel(child);
                childViewModel.SelectionChanged += SelectionChanged;
                childrenViewModels.Add(childViewModel);

                if (loadInnerChildren)
                {
                    childViewModel.LoadChildren(true);
                    //                        childViewModel.LoadChildren(false);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }

        Children.Reset(childrenViewModels);
    }

    private string NormalizeString(string value)
    {
        if (String.IsNullOrEmpty(value))
        {
            return value;
        }
        return value.Replace(Environment.NewLine, " ").Replace('\r', ' ').Replace('\n', ' ');
    }
}
