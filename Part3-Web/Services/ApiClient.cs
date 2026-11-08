using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDay.Web.Services
{
    public interface IApiClient
    {
        Task<ApiResult<T>> GetAsync<T>(string url);
        Task<ApiResult<T>> PostAsync<T>(string url, object? body = null);
        Task<ApiResult<T>> PutAsync<T>(string url, object? body = null);
        Task<ApiResult<object>> DeleteAsync(string url);
        Task<ApiResult<T>> PostFileAsync<T>(string url, IFormFile file);
    }

    /// <summary>
    /// Talks to the RaceDay API. Forwards the API session cookie stored in the user's MVC session.
    /// </summary>
    public class ApiClient : IApiClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _accessor;

        public ApiClient(HttpClient http, IHttpContextAccessor accessor)
        {
            _http = http;
            _accessor = accessor;
        }

        public Task<ApiResult<T>> GetAsync<T>(string url)
            => SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, url));

        public Task<ApiResult<T>> PostAsync<T>(string url, object? body = null)
            => SendAsync<T>(JsonRequest(HttpMethod.Post, url, body));

        public Task<ApiResult<T>> PutAsync<T>(string url, object? body = null)
            => SendAsync<T>(JsonRequest(HttpMethod.Put, url, body));

        public Task<ApiResult<object>> DeleteAsync(string url)
            => SendAsync<object>(new HttpRequestMessage(HttpMethod.Delete, url));

        public Task<ApiResult<T>> PostFileAsync<T>(string url, IFormFile file)
        {
            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.FileName);

            return SendAsync<T>(new HttpRequestMessage(HttpMethod.Post, url) { Content = content });
        }

        private static HttpRequestMessage JsonRequest(HttpMethod method, string url, object? body)
        {
            var request = new HttpRequestMessage(method, url);
            if (body != null)
            {
                request.Content = new StringContent(
                    JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
            }
            return request;
        }

        private async Task<ApiResult<T>> SendAsync<T>(HttpRequestMessage request)
        {
            using (request)
            {
                var session = _accessor.HttpContext?.Session;

                var cookie = session?.GetString(SessionKeys.ApiCookie);
                if (!string.IsNullOrEmpty(cookie))
                    request.Headers.TryAddWithoutValidation("Cookie", cookie);

                HttpResponseMessage response;
                try
                {
                    response = await _http.SendAsync(request);
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
                {
                    return new ApiResult<T>
                    {
                        Success = false,
                        StatusCode = 503,
                        Message = "The RaceDay service is not reachable right now. Please try again shortly."
                    };
                }

                using (response)
                {
                    // Remember the API's session cookie so later calls stay logged in
                    if (session != null && response.Headers.TryGetValues("Set-Cookie", out var cookies))
                    {
                        var sessionCookie = cookies.FirstOrDefault(c =>
                            c.StartsWith(".AspNetCore.Session", StringComparison.OrdinalIgnoreCase));
                        if (sessionCookie != null)
                            session.SetString(SessionKeys.ApiCookie, sessionCookie.Split(';')[0]);
                    }

                    var body = await response.Content.ReadAsStringAsync();
                    var result = new ApiResult<T>
                    {
                        StatusCode = (int)response.StatusCode,
                        Success = response.IsSuccessStatusCode
                    };

                    if (response.IsSuccessStatusCode)
                    {
                        if (!string.IsNullOrWhiteSpace(body) && typeof(T) != typeof(object))
                        {
                            try
                            {
                                result.Data = JsonSerializer.Deserialize<T>(body, JsonOptions);
                            }
                            catch (JsonException)
                            {
                                result.Success = false;
                                result.StatusCode = 500;
                                result.Message = "We received an unexpected response. Please try again.";
                            }
                        }
                    }
                    else
                    {
                        result.Message = ExtractMessage(body, (int)response.StatusCode);
                    }

                    return result;
                }
            }
        }

        private static string ExtractMessage(string body, int status)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                        return message.GetString()!;

                    if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                    {
                        var texts = errors.EnumerateObject()
                            .Where(p => p.Value.ValueKind == JsonValueKind.Array)
                            .SelectMany(p => p.Value.EnumerateArray().Select(v => v.GetString()))
                            .Where(t => !string.IsNullOrWhiteSpace(t))
                            .ToList();

                        var joined = string.Join(" ", texts);
                        if (joined.Length > 0 && !joined.Contains("JSON") && !joined.Contains("System."))
                            return joined;

                        return "Please check the details you entered and try again.";
                    }
                }
            }
            catch (JsonException)
            {
            }

            return status switch
            {
                401 => "Please log in to continue.",
                403 => "You do not have permission to do that.",
                404 => "We could not find what you were looking for.",
                409 => "That action conflicts with existing information.",
                _ => "Something went wrong. Please try again."
            };
        }
    }
}