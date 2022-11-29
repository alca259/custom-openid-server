using Microsoft.AspNetCore.Authorization;

namespace AuthApiClient.Filters;

public sealed class RequireScope : IAuthorizationRequirement
{
	public List<string> Scopes { get; }

	public RequireScope(params string[] scopeNames)
	{
		Scopes = scopeNames?.ToList() ?? new List<string>();
	}
}
