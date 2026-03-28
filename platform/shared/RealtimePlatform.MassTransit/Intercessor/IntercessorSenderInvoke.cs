using System.Reflection;

using Intercessor.Abstractions;

namespace RealtimePlatform.MassTransit.Intercessor;

internal static class IntercessorSenderInvoke
{
    public static Task DispatchAsync(ISender sender, object request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(request);
        Type requestRuntimeType = request.GetType();
        Type? genericIRequest = requestRuntimeType.GetInterfaces()
            .FirstOrDefault(static i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
        if (genericIRequest is null)
            return sender.SendAsync((IRequest)request, cancellationToken);

        Type responseType = genericIRequest.GetGenericArguments()[0];
        MethodInfo? methodDefinition = FindGenericSendAsyncMethodDefinition();
        if (methodDefinition is null)
            throw new InvalidOperationException("Could not resolve generic SendAsync on ISender.");
        MethodInfo closed = methodDefinition.MakeGenericMethod(responseType);
        object? invokeResult = closed.Invoke(sender, new object[] { request, cancellationToken });
        if (invokeResult is not Task task)
            throw new InvalidOperationException("ISender.SendAsync did not return a Task.");
        return task;
    }

    private static MethodInfo? FindGenericSendAsyncMethodDefinition()
    {
        foreach (MethodInfo m in typeof(ISender).GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            if (m.Name != nameof(ISender.SendAsync) || !m.IsGenericMethodDefinition || m.GetParameters().Length != 2)
                continue;
            Type p0 = m.GetParameters()[0].ParameterType;
            if (!p0.IsGenericType || p0.GetGenericTypeDefinition() != typeof(IRequest<>))
                continue;
            return m;
        }

        return null;
    }
}
