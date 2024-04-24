namespace Common.Shared.Events;

public record OrderCreatedEvent
{
    // public Dictionary<string, string> Headers { get; set; } // eğer ki kullandığımız kuyruk  tool'unun header'ı yoksa bu şekilde kendim header kullanacağım. 
    public string OrderCode { get; set; } = null!;
}