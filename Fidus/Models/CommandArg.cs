public class CommandArg(string[] names, Type valueType, string invalidMessage)
{
    public string[] Names = names;
    public Type ValueType = valueType;
    public string InvalidMessage = invalidMessage;
}