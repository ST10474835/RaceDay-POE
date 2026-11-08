namespace RaceDay.Web.Services
{
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public bool IsUnauthorized => StatusCode == 401;
    }
}