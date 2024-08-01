using EF.AuditLog.Samples.Models;
using EF.AuditLog;
using EF.AuditLog.Abstractions;

namespace EF.AuditLog.Samples.Persistence.Configurations;

internal sealed class UserAuditConfiguration : IAuditConfiguration<User>
{
    public void Configure(AuditTypeConfigurationBuilder<User> configurationBuilder)
    {
        configurationBuilder.IgnoreProperty(x => x.Password);
    }
}