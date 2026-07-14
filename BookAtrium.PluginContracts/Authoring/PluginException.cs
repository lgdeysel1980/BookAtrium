namespace BookAtrium.PluginContracts;

/// <summary>Simple plugin exception. The host translates these into user-facing messages.</summary>
public class PluginException : Exception
{
    public PluginException(string message) : base(message) { }
    public PluginException(string message, Exception inner) : base(message, inner) { }
}

public sealed class PluginNetworkException : PluginException
{
    public PluginNetworkException(string message) : base(message) { }
    public PluginNetworkException(string message, Exception inner) : base(message, inner) { }
}

public sealed class PluginRateLimitException : PluginException
{
    public PluginRateLimitException(string message) : base(message) { }
}

public sealed class PluginAuthenticationException : PluginException
{
    public PluginAuthenticationException(string message) : base(message) { }
}

public sealed class PluginFormatException : PluginException
{
    public PluginFormatException(string message) : base(message) { }
    public PluginFormatException(string message, Exception inner) : base(message, inner) { }
}
