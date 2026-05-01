namespace csharp_fastapi_template.errors;

public class ParamNotValidatedException : Exception 
{
    public ParamNotValidatedException(string param, string message)
        : base($"Param '{param}' is not valid: {message}")
    { }
}