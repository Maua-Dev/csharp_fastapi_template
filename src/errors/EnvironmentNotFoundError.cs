namespace csharp_fastapi_template.errors;

public class EnvironmentNotFoundException : Exception
{
    public EnvironmentNotFoundException(string environmentVariable)
        : base($"Environment variable '{environmentVariable}' is missing or invalid.")
    {
    }
}
