using System;
using AIEmployeeSupport.Application.Common.Settings;
using AIEmployeeSupport.Infrastructure.Security;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace AIEmployeeSupport.Tests;

public class EncryptionServiceTests
{
    [Fact]
    public void Encrypt_Decrypt_RoundTrip_ReturnsOriginalString()
    {
        var options = Options.Create(new EncryptionSettings { Key = "my-secret-key-that-is-long-enough" });
        var service = new EncryptionService(options);
        string original = "Sensitive Data 123";

        var encrypted = service.Encrypt(original);
        var decrypted = service.Decrypt(encrypted);

        decrypted.Should().Be(original);
        encrypted.Should().NotBe(original);
    }

    [Fact]
    public void Constructor_NullOrEmptyKey_ThrowsInvalidOperationException()
    {
        var options = Options.Create(new EncryptionSettings { Key = "" });
        
        Action act = () => new EncryptionService(options);

        act.Should().Throw<InvalidOperationException>();
    }
}
