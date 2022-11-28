namespace AuthServer.ViewModel;

public sealed class LoginViewModel
{
    public string Userlogin { get; set; }
    public string Password { get; set; }
    public bool RememberLogin { get; set; } = true;
    public string ReturnUrl { get; set; }
}
