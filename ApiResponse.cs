namespace JiraWebhookPublisherApi.Application.Common;
public class ApiResponse<T>
{
    public T Data { get; set; }
    public string Error { get; set; }
    public bool IsSuccess => string.IsNullOrEmpty(Error);

    public ApiResponse(T data)
    {
        Data = data;
    }

    public ApiResponse(string error)
    {
        Error = error;
    }
}
