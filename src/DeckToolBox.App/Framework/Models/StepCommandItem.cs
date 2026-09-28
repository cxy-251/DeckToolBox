namespace DeckToolBox.App.Framework.Models;

public class StepCommandItem
{
    public int StepNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public StepCommandItem() { }

    public StepCommandItem(int stepNumber, string title, string command, string description = "")
    {
        StepNumber = stepNumber;
        Title = title;
        Command = command;
        Description = description;
    }
}
