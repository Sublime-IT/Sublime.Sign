using System.Text;
using System.Text.Json;
using Sublime.Sign.Common.Exceptions;
using Sublime.Sign.Common.Interfaces;
using Sublime.Sign.HmacSha256;

namespace Sublime.Sign.Headers;

///<summary>
/// SecureHeaderSerializer ensures that the content is safe to be transmitted in a header of a request.
/// Additionally it ensures that you can't tamper with the content using private and signature validation.
/// Signature validation is done using HMACSHA256
///</summary>
/// <throws>InvalidSignatureException</throws>
public class SecureHeaderSerializer<T> : ISecureSerializer<T>
{
    private readonly ISignatureCreator<string> _signatureCreator;
    private readonly ISignatureValidator<string> _signatureValidator;

    public SecureHeaderSerializer(string privateKey)
    {
        var hmacStringSignature = new HmacStringSignature(privateKey);//hmac signature

        _signatureCreator = hmacStringSignature;
        _signatureValidator = hmacStringSignature;
    }

    public T Deserialize((string content, string signature) serialized)
    {
        if (!_signatureValidator.Validate(serialized.content, serialized.signature)) {
            throw new InvalidSignatureException();
        }
    
        var contentBytes = Convert.FromBase64String(serialized.content);
        var content = Encoding.UTF8.GetString(contentBytes);

        return JsonSerializer.Deserialize<T>( content )!;
    }

    public (string content, string signature) Serialize(T @object)
    {
        var content = JsonSerializer.Serialize(@object);
        var contentBytes = Encoding.UTF8.GetBytes(content);
        var contentBase64 = Convert.ToBase64String(contentBytes);
        var signature = _signatureCreator.Sign(contentBase64);

        return (contentBase64, signature);
    }
}