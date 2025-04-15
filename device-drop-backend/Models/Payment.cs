using Newtonsoft.Json;

namespace device_drop_backend.Models;

public class PaymentResponse
{
    public string Id { get; set; }
    public Confirmation Confirmation { get; set; }
}

public class Confirmation
{
    public string Type { get; set; }
    [JsonProperty("confirmation_url")]
    public string ConfirmationUrl { get; set; }
}