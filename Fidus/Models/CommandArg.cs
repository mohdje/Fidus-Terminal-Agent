using Fidus.Enums;

public class CommandArg(string[] names, CommandArgId id, string description, string invalidMessage)
{
    public string[] Names = names;
    public CommandArgId Id = id;
    public string Description = description;
    public string InvalidMessage = invalidMessage;
}