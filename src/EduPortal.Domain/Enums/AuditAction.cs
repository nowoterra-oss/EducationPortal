namespace EduPortal.Domain.Enums;

public enum AuditAction
{
    Create = 1,
    Read = 2,
    Update = 3,
    Delete = 4,
    Login = 5,
    Logout = 6,
    LoginFailed = 7,
    PasswordChanged = 8,
    Export = 9,
    Import = 10,
    Other = 99
}
