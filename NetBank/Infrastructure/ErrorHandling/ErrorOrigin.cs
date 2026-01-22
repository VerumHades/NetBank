namespace NetBank.Infrastructure.ErrorHandling;

/// <summary>
/// Classifies the origin of a module error.
/// </summary>
public enum ErrorOrigin
{
    Client,
    System,
    External
}
