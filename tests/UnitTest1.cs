using Microsoft.AspNetCore.Http;
using Sublime.Sign.Common.Exceptions;
using Sublime.Sign.Headers;
using Sublime.Sign.HmacSha256;

namespace tests;

public class Tests
{
    private HeaderAccessor<string> headerAccessor;
    private HeaderAccessor<string> headerAccessor2;

    private HmacBufferSignature hmacBufferSignature;
    private HmacStringSignature hmacStringSignature;

    [SetUp]
    public void Setup()
    {
        string pk = "privateKey";

      headerAccessor = new HeaderAccessor<string>(pk);
      headerAccessor2 = new HeaderAccessor<string>(pk);

        hmacBufferSignature = new HmacBufferSignature(pk);
        hmacStringSignature = new HmacStringSignature(pk);
    }

    [Test]
    public void TestStringSignature() {
        var signature = hmacStringSignature.Sign("test");
        Assert.IsTrue(hmacStringSignature.Validate("test", signature));
    }

    [Test]
    public void TestHeaderAccessor()
    {
        // Create HttpRequest
        var request = new DefaultHttpContext().Request;

        // Set header
        headerAccessor.SetHeader(request, "headerName", "value");

        // Get header
        try {
            var value = headerAccessor.GetHeader(request, "headerName");
        } catch (InvalidSignatureException) {
            Assert.Fail();
        }
    }
}