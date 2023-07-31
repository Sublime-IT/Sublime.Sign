using System.Security.Cryptography;
using System.Text;
using Sublime.Sign.Common.Interfaces;

namespace Sublime.Sign.HmacSha256;

public class HmacBufferSignature : ISignatureCreator<byte[]>, ISignatureValidator<byte[]>
{
    private readonly byte[] _privateKey;
    public HmacBufferSignature(string privateKey)
    {
        _privateKey = Encoding.UTF8.GetBytes(privateKey);
    }

    public byte[] Sign(byte[] buffer)
    {
        using var hmac = new HMACSHA256(_privateKey);
        var signature = hmac.ComputeHash(buffer);
        return signature;
    }

    public bool Validate(byte[] buffer, byte[] signature)
    {
        var computedSignature = Sign(buffer);
        return computedSignature.SequenceEqual(signature);
    }
}
