namespace CargoLink.ModernApi.Models;

/// <summary>
/// Standard error response returned when an API request fails.
/// </summary>
public class ErrorResponse
{
    /// <summary>Machine-readable error code.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Human-readable error message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Optional additional details about the error.</summary>
    public string? Details { get; set; }
}
