namespace Adapters.Config;

/// <summary>
/// CORS configuration for cross-origin requests (e.g. frontend en Amplify).
/// </summary>
/// <remarks>
/// Si CORS__AllowedOrigins está vacío, se permite cualquier origen (útil en desarrollo).
/// En producción, define los orígenes permitidos para evitar problemas con CloudFront/caché.
/// </remarks>
public class CorsConfig
{
    /// <summary>
    /// Orígenes permitidos (ej. https://development.d2g4yx5jn4b7oi.amplifyapp.com).
    /// Separados por coma. Si está vacío, se usa AllowAnyOrigin().
    /// </summary>
    public string[] AllowedOrigins { get; private set; } = [];

    /// <summary>
    /// Inicializa desde CORS__AllowedOrigins (lista separada por comas).
    /// </summary>
    public CorsConfig Init()
    {
        var value = EnvHelper.GetEnv("CORS__AllowedOrigins", "");
        AllowedOrigins = string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return this;
    }

    /// <summary>
    /// Indica si se deben permitir todos los orígenes (config vacía).
    /// </summary>
    public bool AllowAnyOrigin => AllowedOrigins.Length == 0;
}
