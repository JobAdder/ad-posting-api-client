using System.Text;

namespace SEEK.AdPostingApi.Client
{
  public class AdPostingApiClientLog
  {
    private readonly StringBuilder _Log = new StringBuilder();
    private readonly StringBuilder _BlockLog = new StringBuilder();

    public string Block => _BlockLog.ToString();
    public string Last { get; private set; }

    public void BeginBlock()
    {
      _BlockLog.Clear();
    }

    internal void Log(string request)
    {
      _Log.AppendLine(request).AppendLine();
      _BlockLog.AppendLine(request).AppendLine();
      Last = request;
    }

    public override string ToString()
    {
      return _Log.ToString();
    }
  }
}
