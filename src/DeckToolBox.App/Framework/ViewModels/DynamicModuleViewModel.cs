using System;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Framework.ViewModels;

public class DynamicModuleViewModel : ViewModelBase
{
    private readonly Action<string>? _copyAction;

    public ModuleDefinition Model { get; }

    public string Title => Model.Title;
    public string CategoryName => Model.CategoryName;
    public string Overview => Model.Overview;
    public InfoCardItem WhyCard => Model.WhyCard;
    public InfoCardItem ConsequenceCard => Model.ConsequenceCard;
    public System.Collections.Generic.List<StepCommandItem> Steps => Model.Steps;
    public InfoCardItem VerificationCard => Model.VerificationCard;

    public DynamicModuleViewModel(ModuleDefinition model, Action<string>? copyAction = null)
    {
        Model = model;
        _copyAction = copyAction;
    }

    public void CopyCommand(string command)
    {
        _copyAction?.Invoke(command);
    }
}
