using System.Text;
using Sublime.Sign.Common.Interfaces;

namespace Sublime.Sign.HmacSha256;

public class HmacStringSignature : ISignatureCreator<string>, ISignatureValidator<string>
{
    private readonly HmacBufferSignature _bufferSigner;
    public HmacStringSignature(string privateKey)
    {
        _bufferSigner = new HmacBufferSignature(privateKey);
    }

    public string Sign(string message)
    {
        var unsignedBytes = Encoding.UTF8.GetBytes(message);
        var signatureBytes = _bufferSigner.Sign(unsignedBytes);
        return BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();
    }

    public bool Validate(string message, string signature)
    {
        var unsignedBytes = Encoding.UTF8.GetBytes(message);
        var signatureBytes = Encoding.UTF8.GetBytes(signature);
        return _bufferSigner.Validate(unsignedBytes, signatureBytes);
    }
}
