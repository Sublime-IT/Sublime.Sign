using Microsoft.AspNetCore.Http;

namespace Sublime.Sign.Headers;

public class HeaderAccessor<T> {
    private SecureHeaderSerializer<T> _serializer;

    public HeaderAccessor(string privateKey) {
        _serializer = new SecureHeaderSerializer<T>(privateKey);
    }

    public void SetHeader(HttpRequest request, string headerName, T value) {
        var serialized = _serializer.Serialize(value);
        
        if (request.Headers.ContainsKey(headerName)) {
            request.Headers.Remove(headerName);
        }

        if (request.Headers.ContainsKey($"{headerName}-Signature")) {
            request.Headers.Remove($"{headerName}-Signature");
        }
        
        request.Headers.Add(headerName, serialized.content);
        request.Headers.Add($"{headerName}-Signature", serialized.signature);
    }

    public T GetHeader(HttpRequest request, string headerName) {
        var content = request.Headers[headerName];
        var signature = request.Headers[$"{headerName}-Signature"];
        var serialized = (content, signature);

        return _serializer.Deserialize(serialized);
    }
}