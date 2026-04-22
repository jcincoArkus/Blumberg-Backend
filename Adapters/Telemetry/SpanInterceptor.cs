using Adapters.Config;
using Castle.DynamicProxy;
using System.Diagnostics;
using System.Reflection;

namespace Adapters.Telemetry;

/// <summary>
/// Interceptor that automatically creates OpenTelemetry spans for methods decorated with [Span] attribute
/// </summary>
public class SpanInterceptor : IInterceptor
{
    private readonly TelemetryConfig _config;

    public SpanInterceptor(TelemetryConfig config)
    {
        _config = config;
    }

    public void Intercept(IInvocation invocation)
    {
        // If telemetry is disabled, just proceed without instrumentation
        if (!_config.Enabled)
        {
            invocation.Proceed();
            return;
        }

        var method = invocation.Method;
        var spanAttribute = method.GetCustomAttribute<SpanAttribute>();

        // If no [Span] attribute, just proceed
        if (spanAttribute == null)
        {
            invocation.Proceed();
            return;
        }

        // Create span name
        var spanName = string.IsNullOrWhiteSpace(spanAttribute.Name)
            ? $"{invocation.TargetType?.Name}.{method.Name}"
            : spanAttribute.Name;

        // Start the activity/span
        using var activity = ActivitySourceProvider.StartActivity(spanName, spanAttribute.Kind);

        if (activity == null)
        {
            // If activity is not created (e.g., sampling), just proceed
            invocation.Proceed();
            return;
        }

        try
        {
            // Add custom tags from attribute
            var customTags = spanAttribute.GetTags();
            foreach (var tag in customTags)
            {
                activity.SetTag(tag.Key, tag.Value);
            }

            // Add method information
            activity.SetTag("code.function", method.Name);
            activity.SetTag("code.namespace", method.DeclaringType?.Namespace);
            activity.SetTag("code.class", method.DeclaringType?.Name);

            // Add arguments if requested
            if (spanAttribute.IncludeArguments && invocation.Arguments.Length > 0)
            {
                var parameters = method.GetParameters();
                for (int i = 0; i < invocation.Arguments.Length && i < parameters.Length; i++)
                {
                    var paramName = parameters[i].Name ?? $"arg{i}";
                    var paramValue = invocation.Arguments[i];
                    
                    // Safely convert to string, handle nulls
                    var valueStr = paramValue?.ToString() ?? "null";
                    
                    // Truncate long values to avoid huge spans
                    if (valueStr.Length > 200)
                        valueStr = valueStr.Substring(0, 200) + "...";
                    
                    activity.SetTag($"method.argument.{paramName}", valueStr);
                }
            }

            // Execute the method
            invocation.Proceed();

            // Handle async methods
            if (method.ReturnType.IsGenericType && 
                (method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>) ||
                 method.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>)))
            {
                HandleAsyncMethodWithResult(invocation, activity, spanAttribute);
            }
            else if (method.ReturnType == typeof(Task) || method.ReturnType == typeof(ValueTask))
            {
                HandleAsyncMethod(invocation, activity, spanAttribute);
            }
            else
            {
                // Synchronous method
                HandleSyncMethod(invocation, activity, spanAttribute);
            }
        }
        catch (Exception ex)
        {
            if (spanAttribute.RecordException)
            {
                activity.RecordException(ex);
                activity.SetStatus(ActivityStatusCode.Error, ex.Message);
            }
            throw;
        }
    }

    private void HandleSyncMethod(IInvocation invocation, Activity activity, SpanAttribute spanAttribute)
    {
        if (spanAttribute.IncludeReturnValue && invocation.ReturnValue != null)
        {
            var returnStr = invocation.ReturnValue.ToString() ?? "null";
            if (returnStr.Length > 200)
                returnStr = returnStr.Substring(0, 200) + "...";
            
            activity.SetTag("method.return_value", returnStr);
        }

        activity.SetStatus(ActivityStatusCode.Ok);
    }

    private void HandleAsyncMethod(IInvocation invocation, Activity activity, SpanAttribute spanAttribute)
    {
        if (invocation.ReturnValue is Task task)
        {
            invocation.ReturnValue = HandleTaskAsync(task, activity, spanAttribute);
        }
        else if (invocation.ReturnValue is ValueTask valueTask)
        {
            invocation.ReturnValue = HandleValueTaskAsync(valueTask, activity, spanAttribute);
        }
    }

    private void HandleAsyncMethodWithResult(IInvocation invocation, Activity activity, SpanAttribute spanAttribute)
    {
        var returnType = invocation.Method.ReturnType;
        var resultType = returnType.GetGenericArguments()[0];
        
        if (returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var method = typeof(SpanInterceptor)
                .GetMethod(nameof(HandleTaskWithResultAsync), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(resultType);
            
            invocation.ReturnValue = method.Invoke(this, new[] { invocation.ReturnValue, activity, spanAttribute });
        }
    }

    private async Task HandleTaskAsync(Task task, Activity activity, SpanAttribute spanAttribute)
    {
        try
        {
            await task.ConfigureAwait(false);
            activity.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            if (spanAttribute.RecordException)
            {
                activity.RecordException(ex);
                activity.SetStatus(ActivityStatusCode.Error, ex.Message);
            }
            throw;
        }
    }

    private async ValueTask HandleValueTaskAsync(ValueTask valueTask, Activity activity, SpanAttribute spanAttribute)
    {
        try
        {
            await valueTask.ConfigureAwait(false);
            activity.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            if (spanAttribute.RecordException)
            {
                activity.RecordException(ex);
                activity.SetStatus(ActivityStatusCode.Error, ex.Message);
            }
            throw;
        }
    }

    private async Task<T> HandleTaskWithResultAsync<T>(Task<T> task, Activity activity, SpanAttribute spanAttribute)
    {
        try
        {
            var result = await task.ConfigureAwait(false);
            
            if (spanAttribute.IncludeReturnValue && result != null)
            {
                var returnStr = result.ToString() ?? "null";
                if (returnStr.Length > 200)
                    returnStr = returnStr.Substring(0, 200) + "...";
                
                activity.SetTag("method.return_value", returnStr);
            }
            
            activity.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            if (spanAttribute.RecordException)
            {
                activity.RecordException(ex);
                activity.SetStatus(ActivityStatusCode.Error, ex.Message);
            }
            throw;
        }
    }
}
