using System;
using System.Linq;
using System.Net;

namespace AIEmployeeSupport.Application.Validators;

public static class UrlSecurityValidator
{
    public static bool IsSafeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true; // Empty is fine, it means use default.

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;

        // Force HTTPS for external URLs
        if (uri.Scheme != Uri.UriSchemeHttps) return false;

        var host = uri.Host;
        
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || host.Equals("127.0.0.1") || host.Equals("::1"))
            return false;

        // Prevent AWS/Azure metadata IPs
        if (host.Equals("169.254.169.254")) return false;

        // Try parse IP to block private ranges
        if (IPAddress.TryParse(host, out var ip))
        {
            var bytes = ip.GetAddressBytes();
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                if (bytes[0] == 10) return false; // 10.x.x.x
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return false; // 172.16.x.x - 172.31.x.x
                if (bytes[0] == 192 && bytes[1] == 168) return false; // 192.168.x.x
                if (bytes[0] == 127) return false; // loopback
            }
        }

        return true;
    }
}
