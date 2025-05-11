using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Authorization;

public class PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy =  await base.GetPolicyAsync(policyName);
        if(policy is not null)
        {
            return policy;
        }

        if (policyName.StartsWith(PermissionConstants.AnyOfPermissionsPrefix))
        {
            return new AuthorizationPolicyBuilder()
                .AddRequirements(new AnyOfPermissionsRequirement(policyName[PermissionConstants.AnyOfPermissionsPrefix.Length..]))
                .Build();
        } else
        {
            return new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();
        }
    }
}
