namespace Adapters.Config;

/// <summary>
/// CORS configuration for cross-origin requests (e.g. frontend en Amplify).
/// </summary>
/// <remarks>
/// Variable de entorno: CORS_ORIGINS (orígenes separados por coma).
/// Si no está definida, se usa solo https://development.d2g4yx5jn4b7oi.amplifyapp.com.
/// </remarks>
public class CorsConfig
{
    private const string DefaultOrigins = "https://development.d2g4yx5jn4b7oi.amplifyapp.com";

    /// <summary>
    /// Orígenes permitidos. Por defecto: https://development.d2g4yx5jn4b7oi.amplifyapp.com
    /// </summary>
    public string[] AllowedOrigins { get; private set; } = [];

    /// <summary>
    /// Inicializa desde CORS_ORIGINS (lista separada por comas). Si no está definida, usa orígenes por defecto.
    /// </summary>
    public CorsConfig Init()
    {
        var value = EnvHelper.GetEnv("CORS_ORIGINS", DefaultOrigins);
        AllowedOrigins = string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return this;
    }
}
