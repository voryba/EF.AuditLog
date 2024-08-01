# AuditLog

A plugin for Microsoft.EntityFrameworkCore that enables automatic recording of data change history.

# How to use

To enable automatic recording of data changes in your application, configure your **_DbContext_** to utilize the audit logging functionality.
\
\
Follow these steps:

1. **Configure Audit Settings** \
	In your `DbContext` class, configure the audit settings by calling the **`ApplyAuditSettings`** method. This allows you to define whether all entities should be audited by default or only those explicitly configured.

2. **Apply Audit Entry Configuration** \
   Specify how audit entries should be stored by calling the **_`ApplyAuditEntryConfiguration`_** method. You can pass a custom audit entity type, such as _`CustomAuditEntity`_, to define the structure of your audit logs.

3. **Load Audit Configurations**
   If you have custom audit configurations defined in your assembly, use the **_`ApplyAuditConfigurationsFromAssembly`_** method. This will automatically load and apply these configurations based on the types defined in your application.


```csharp
internal sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
	    .ApplyAuditSettings(options => options.OnlyConfiguredAudited())
            .ApplyAuditEntryConfiguration<CustomAuditEntity>()
            .ApplyAuditConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

## Registering the Audit Save Changes Interceptor

To enable automatic audit logging for your Entity Framework Core context,
you need to register an interceptor that will monitor and log changes during _**SaveChanges**_ operations.
This can be done by adding an interceptor to the **_DbContext_** when configuring it in your application's service container.

**Here’s an example of how to register the AuditSaveChangesInterceptor:**

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
	options.AddInterceptors(new AuditSaveChangesInterceptor());
});
```

## Using a Custom Interceptor with a Custom Entity
If your application requires a custom audit entity, you can extend the functionality by creating a custom interceptor.\
This custom interceptor can override the behavior of the default interceptor to accommodate the specific fields or properties your audit entity might have.

For example, consider a custom audit entity _**CustomAuditEntity**_ that includes an additional _**CreatedBy**_ property:

```csharp
public class CustomAuditEntity : AuditEntry
{
    /// <summary>
    /// Gets or sets the entity identifier.
    /// </summary>
    public string CreatedBy { get; set; }
}
```
To support this custom entity, you would create a custom interceptor by extending **_`AuditSaveChangesInterceptorBase`_**:

```csharp
internal sealed class AuditSaveChangesInterceptor : AuditSaveChangesInterceptorBase<CustomAuditEntity>
{
    protected override Func<CustomAuditEntity> AuditEntryFactory()
    {
        return () => new CustomAuditEntity()
        {
            CreatedBy = "some logic to get the current user",
        };
    }
}
```


## Ignoring Specific Properties in Audit Logs

The **_`UserAuditConfiguration`_** class is used to define how the User entity is handled during auditing. By implementing the **_`IAuditConfiguration`<User>_** interface, you can control which properties are included or excluded in the audit records.

```csharp
internal sealed class UserAuditConfiguration : IAuditConfiguration<User>
{
    public void Configure(AuditTypeConfigurationBuilder<User> configurationBuilder)
    {
        configurationBuilder.IgnoreProperty(x => x.Password);
    }
}
```

## Excluding an Entire Entity from Audit Logs

Alternatively, the **_`UserAuditConfiguration`_** class can be configured to completely exclude the **`User`** entity from being audited. This means that any changes to User records will not be logged.

```csharp
internal sealed class UserAuditConfiguration : IAuditConfiguration<User>
{
    public void Configure(AuditTypeConfigurationBuilder<User> configurationBuilder)
    {
        configurationBuilder.ExcludeEntity();
    }
}
```


