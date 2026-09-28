using System.Collections.Generic;

namespace DeckToolBox.App.Framework.Models;

public class ModuleDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Badge { get; set; } = string.Empty;
    public string Overview { get; set; } = string.Empty;

    public InfoCardItem WhyCard { get; set; } = new();
    public InfoCardItem ConsequenceCard { get; set; } = new();
    public List<StepCommandItem> Steps { get; set; } = new();
    public InfoCardItem VerificationCard { get; set; } = new();
}
