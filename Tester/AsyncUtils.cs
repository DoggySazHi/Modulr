using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Modulr.Tester;

public static class AsyncUtils
{
    // Just robbed these out of UNObot.
    public static void ContinueWithoutAwait(this Task task, Action<Task> exceptionCallback)
    {
        _ = task.ContinueWith(exceptionCallback, TaskContinuationOptions.OnlyOnFaulted);
    }
        
    public static void ContinueWithoutAwait(this Task task, ILogger logger)
    {
        _ = task.ContinueWith(t =>
        {
            var fieldInfo = typeof(Task).GetField("m_action", BindingFlags.Instance | BindingFlags.NonPublic);
            var action = fieldInfo?.GetValue(t) as Delegate;
            var method = $"{action?.Method.Name ?? "<unknown method>"}.{action?.Method.DeclaringType?.FullName ?? "<unknown>"}";
            logger.LogError(t.Exception, "Exception raised in async method {Method}.\n{Exception}", method, t.Exception);
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
}