namespace AuthServer.DTOs;

public sealed class AuthorizeDto
{
    public string ApplicationName { get; set; }
    public string Scope { get; set; }
}
