using DeepSeek.Core.Models;

namespace MyToolBar.Plugin.TabletUtils.Configs;

public class DeepSeekConfig
{
    public string APIKey { get; set; } = string.Empty;
    public string DefaultRole { get; set; } = string.Empty;
}
public enum Model
{
    Chat,Reasoner
}
public class DeepSeekRole
{
    public string Name { get; set; } = string.Empty;
    public Model Model { get; set; } = Model.Chat;
    public string Hint { get; set; } = string.Empty;
}

public class ClientHistory
{
    public string Id { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<Message> Messages { get; set; } = [];
}