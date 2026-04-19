using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PaymentService.Models;
using PaymentService.Models.Settings;
using PaymentService.Services.Interfaces;

namespace PaymentService.Services.Providers
{
    public class PayPalProvider : IPaymentProvider
    {
        private readonly PayPalSettings _settings;
        private readonly HttpClient _httpClient;

        public PayPalProvider(IOptions<PayPalSettings> options, HttpClient httpClient)
        {
            _settings = options.Value;
            _httpClient = httpClient;
        }

        private async Task<string> GetAccessToken()
        {
            var auth = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.Secret}")
            );
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_settings.BaseUrl}/v1/oauth2/token"
            );

            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
            request.Content = new StringContent(
                "grant_type=client_credentials",
                Encoding.UTF8,
                "application/x-www-form-urlencoded"
            );

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(json);
            
            return data.GetProperty("access_token").GetString();
        }

        public async Task<string> CreatePaymentUrl(Payment payment)
        {
            var token = await GetAccessToken();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_settings.BaseUrl}/v2/checkout/orders"
            );

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        amount = new
                        {
                            currency_code = "USD",
                            value = payment.Amount.ToString("F2"),
                        },
                    },
                },
                application_context = new
                {
                    return_url = $"{_settings.ReturnUrl}?paymentId={payment.Id}",
                    cancel_url = _settings.CancelUrl,
                },
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<JsonElement>(json);

            //  lấy approval link
            if (data.TryGetProperty("links", out var links))
            {
                foreach (var link in links.EnumerateArray())
                {
                    if (link.GetProperty("rel").GetString() == "approve")
                    {
                        return link.GetProperty("href").GetString();
                    }
                }
            }

            throw new Exception("No approval URL found");
        }

        //  3. Capture payment sau khi user thanh toán
        public async Task<string> Capture(string token)
        {
            var accessToken = await GetAccessToken();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_settings.BaseUrl}/v2/checkout/orders/{token}/capture"
            );

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent("", Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<JsonElement>(json);

            if (data.TryGetProperty("status", out var status))
            {
                return status.GetString(); // COMPLETED
            }

            if (data.TryGetProperty("name", out var errorName))
            {
                return "FAILED - " + errorName.GetString();
            }

            return "FAILED";
        }
    }
}
