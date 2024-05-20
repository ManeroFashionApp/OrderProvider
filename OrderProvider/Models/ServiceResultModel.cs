using System.Net;

namespace OrderProvider.Models;

public class ServiceResultModel<T>
{
    public HttpStatusCode StatusCode { get; set; }
    public T? Data { get; set; }
}
