namespace LeadshineCard.Core.Helpers;

/// <summary>
/// 异步辅助工具
/// </summary>
internal static class AsyncHelper
{
    /// <summary>
    /// 快速 API 调用（不使用 Task.Run）
    /// </summary>
    public static T FastCall<T>(Func<T> func)
    {
        return func();
    }

    /// <summary>
    /// 异步 API 调用（使用 Task.Run）
    /// </summary>
    public static Task<T> AsyncCall<T>(Func<T> func)
    {
        return Task.Run(func);
    }

    /// <summary>
    /// 带指数退避的轮询
    /// </summary>
    public static async Task<T> PollWithBackoffAsync<T>(
        Func<Task<T>> pollFunc,
        Func<T, bool> completionCheck,
        int timeoutMs = 30000,
        int initialDelayMs = 50,
        int maxDelayMs = 500,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.Now;
        var timeout = timeoutMs > 0 ? TimeSpan.FromMilliseconds(timeoutMs) : TimeSpan.MaxValue;
        var delay = initialDelayMs;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (DateTime.Now - startTime > timeout)
            {
                throw new TimeoutException($"操作超时，超时时间: {timeoutMs}ms");
            }

            var result = await pollFunc();
            if (completionCheck(result))
            {
                return result;
            }

            await Task.Delay(delay, cancellationToken);

            // 指数退避，但不超过最大延迟
            delay = Math.Min(delay * 2, maxDelayMs);
        }
    }
}
