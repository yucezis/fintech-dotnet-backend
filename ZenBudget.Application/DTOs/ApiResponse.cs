namespace ZenBudget.Application.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public int StatusCode { get; set; }

    // Başarılı işlemler için hızlı üretim metodu
    public static ApiResponse<T> SuccessResponse(T data, string message = "İşlem başarılı", int statusCode = 200)
    {
        return new ApiResponse<T> { Success = true, Data = data, Message = message, StatusCode = statusCode };
    }

    // Hatalı işlemler için hızlı üretim metodu
    public static ApiResponse<T> ErrorResponse(string error, int statusCode = 400)
    {
        return new ApiResponse<T> { Success = false, Error = error, StatusCode = statusCode };
    }
}