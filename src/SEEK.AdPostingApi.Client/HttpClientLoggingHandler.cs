using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SEEK.AdPostingApi.Client
{
  public class HttpClientLoggingHandler : DelegatingHandler
  {
    public HttpClientLoggingHandler(HttpMessageHandler innerHandler, HttpClientLog logger,
      bool includeRequestContent = true,
      bool includeResponseContent = false)
      : base(innerHandler)
    {
      _Logger = logger;
      _IncludeRequestContent = includeRequestContent;
      _IncludeResponseContent = includeResponseContent;
    }

    private readonly HttpClientLog _Logger;
    private readonly bool _IncludeRequestContent;
    private readonly bool _IncludeResponseContent;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      var output = new StringBuilder();
      try
      {
        output.AppendLine("Request:");
        output.AppendLine(request.ToString());

        if (_IncludeRequestContent && request.Content != null)
        {
          var requestContent = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
          output.AppendLine(requestContent);
        }

        output.AppendLine();

        var response = await base.SendAsync(request, cancellationToken)
                                 .ConfigureAwait(false);

        output.AppendLine("Response:");
        output.AppendLine(response.ToString());

        if (_IncludeResponseContent && response.Content != null)
        {
          var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
          output.AppendLine(responseContent);
        }

        return response;
      }
      finally
      {
        _Logger.Log(output.ToString());
      }
    }
  }
}
