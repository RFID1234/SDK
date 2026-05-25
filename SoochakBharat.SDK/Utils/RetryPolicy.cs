using System;
using System.Threading.Tasks;
namespace SoochakBharat.SDK.Utils
{
    public static class RetryPolicy
    {
        public static async Task ExecuteWithBackoffAsync(Func<Task> action, int maxAttempts = 5, int baseDelayMs = 1000)
        {
            int attempt = 0;
            while (true)
            {
                try { await action(); return; }
                catch when (attempt < maxAttempts)
                {
                    attempt++;
                    int delay = Math.Min(baseDelayMs * attempt, 30000);
                    await Task.Delay(delay);
                }
            }
        }
    }
}
