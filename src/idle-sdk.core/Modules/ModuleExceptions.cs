namespace IdleSdk.Core.Modules;

public class ModuleRegistrationException : InvalidOperationException
{
    public ModuleRegistrationException(string message) : base(message)
    {
    }
}

public class ModuleDependencyException : InvalidOperationException
{
    public ModuleDependencyException(string message) : base(message)
    {
    }
}

public class ModuleVersionException : InvalidOperationException
{
    public ModuleVersionException(string message) : base(message)
    {
    }
}
