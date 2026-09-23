namespace TaskManagement.API.Models;

public static class UserRole
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";

    public static readonly string[] All = [Admin, Manager, User];
}

public static class TaskStatusConstants
{
    public const string ToDo = "To Do";
    public const string InProgress = "In Progress";
    public const string Done = "Done";

    public static readonly string[] All = [ToDo, InProgress, Done];
}

public static class TaskPriorityConstants
{
    public const string Low = "Low";
    public const string Medium = "Medium";
    public const string High = "High";
    public const string Urgent = "Urgent";

    public static readonly string[] All = [Low, Medium, High, Urgent];
}
