namespace EF.AuditLog.Options;

internal sealed record EntityAuditConfiguration(IReadOnlyList<string> IgnoredProperties, bool ExcludeEntity);