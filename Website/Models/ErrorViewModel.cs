namespace Website.Models;

/// <summary>
/// The error view model.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// The request identifier.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// The show request identifier.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
