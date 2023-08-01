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
        var signature = Convert.ToBase64String(signatureBytes);
        return signature;
    }

    public bool Validate(string message, string signature)
    {
        var unsignedBytes = Encoding.UTF8.GetBytes(message);
        var signatureBytes = Convert.FromBase64String(signature);        
        return _bufferSigner.Validate(unsignedBytes, signatureBytes);
    }
}
