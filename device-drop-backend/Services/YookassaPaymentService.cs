using System.Text;
using device_drop_backend.Models;
using Newtonsoft.Json;
using System.Globalization;

namespace device_drop_backend.Services;

public interface IPaymentService
{
    Task<PaymentResponse> CreatePayment(PaymentRequest request);
}

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public int OrderId { get; set; }
    public string Description { get; set; }
}

public class YooKassaPaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly string _storeId;
    private readonly string _apiKey;
    private readonly string _callbackUrl;

    public YooKassaPaymentService(IConfiguration configuration, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _storeId = configuration["YooKassa:StoreId"] ?? throw new ArgumentNullException("YooKassa:StoreId");
        _apiKey = configuration["YooKassa:ApiKey"] ?? throw new ArgumentNullException("YooKassa:ApiKey");
        _callbackUrl = configuration["YooKassa:CallbackUrl"] ?? "http://localhost:3000/return";
    }

    public async Task<PaymentResponse> CreatePayment(PaymentRequest request)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero");
        }

        var payload = new
        {
            amount = new
            {
                value = request.Amount.ToString("F2", CultureInfo.InvariantCulture),
                currency = "RUB"
            },
            capture = true,
            description = request.Description,
            metadata = new
            {
                order_id = request.OrderId
            },
            confirmation = new
            {
                type = "redirect",
                return_url = _callbackUrl
            }
        };

        var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
        var idempotenceKey = Guid.NewGuid().ToString();
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Idempotence-Key", idempotenceKey);
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_storeId}:{_apiKey}"))
        );

        var response = await _httpClient.PostAsync("https://api.yookassa.ru/v3/payments", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"YooKassa API returned {response.StatusCode}: {errorContent}");
        }

        var responseData = await response.Content.ReadAsStringAsync();
        var payment = JsonConvert.DeserializeObject<PaymentResponse>(responseData);

        if (payment == null || string.IsNullOrEmpty(payment.Id) || payment.Confirmation == null ||
            string.IsNullOrEmpty(payment.Confirmation.ConfirmationUrl))
        {
            throw new InvalidOperationException("Invalid YooKassa response");
        }

        return payment;
    }
}