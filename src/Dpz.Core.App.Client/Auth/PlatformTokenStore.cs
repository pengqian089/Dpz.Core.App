using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Dpz.Core.App.Client.Auth;

public sealed class PlatformTokenStore(ILogger<PlatformTokenStore> logger) : ITokenStore
{
    private const string ServiceName = "Dpz.Core.App";
    private const string AccountName = "oidc-token-cache";
    private static readonly string WindowsStorePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Dpz.Core.App",
        "token.cache"
    );

    public async Task<TokenCache?> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var raw = await ReadSecretAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            return JsonSerializer.Deserialize<TokenCache>(raw);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "从安全存储读取 token 缓存失败");
            return null;
        }
    }

    public async Task SaveAsync(
        TokenCache tokenCache,
        CancellationToken cancellationToken = default
    )
    {
        var payload = JsonSerializer.Serialize(tokenCache);
        await WriteSecretAsync(payload, cancellationToken).ConfigureAwait(false);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await DeleteSecretAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<string?> ReadSecretAsync(CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows())
        {
            return await Task.Run(
                    () =>
                    {
                        if (!File.Exists(WindowsStorePath))
                        {
                            return null;
                        }

                        var encrypted = File.ReadAllBytes(WindowsStorePath);
                        if (encrypted == null || encrypted.Length == 0)
                        {
                            logger.LogWarning("Windows 安全存储中未找到 token 缓存");
                            return null;
                        }
                        var decrypted = ProtectedData.Unprotect(
                            encrypted,
                            optionalEntropy: null,
                            DataProtectionScope.CurrentUser
                        );

                        return System.Text.Encoding.UTF8.GetString(decrypted);
                    },
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        if (OperatingSystem.IsMacOS())
        {
            return await RunProcessAsync(
                    "security",
                    $"find-generic-password -a \"{AccountName}\" -s \"{ServiceName}\" -w",
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        if (OperatingSystem.IsLinux())
        {
            return await RunProcessAsync(
                    "secret-tool",
                    $"lookup service \"{ServiceName}\" account \"{AccountName}\"",
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        throw new PlatformNotSupportedException("当前平台不支持安全存储。");
    }

    private async Task WriteSecretAsync(string value, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows())
        {
            await Task.Run(
                    () =>
                    {
                        var dir = Path.GetDirectoryName(WindowsStorePath);
                        if (!string.IsNullOrWhiteSpace(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        var raw = System.Text.Encoding.UTF8.GetBytes(value);
                        if (raw == null || raw.Length == 0)
                        {
                            logger.LogWarning("尝试写入空 token 缓存到 Windows 安全存储");
                            return;
                        }
                        var encrypted = ProtectedData.Protect(
                            raw,
                            optionalEntropy: null,
                            DataProtectionScope.CurrentUser
                        );

                        File.WriteAllBytes(WindowsStorePath, encrypted);
                    },
                    cancellationToken
                )
                .ConfigureAwait(false);

            return;
        }

        if (OperatingSystem.IsMacOS())
        {
            await RunProcessAsync(
                    "security",
                    $"add-generic-password -U -a \"{AccountName}\" -s \"{ServiceName}\" -w \"{EscapeShellArg(value)}\"",
                    cancellationToken
                )
                .ConfigureAwait(false);

            return;
        }

        if (OperatingSystem.IsLinux())
        {
            await RunProcessAsyncWithInputAsync(
                    "secret-tool",
                    $"store --label=\"{ServiceName}\" service \"{ServiceName}\" account \"{AccountName}\"",
                    value,
                    cancellationToken
                )
                .ConfigureAwait(false);

            return;
        }

        throw new PlatformNotSupportedException("当前平台不支持安全存储。");
    }

    private async Task DeleteSecretAsync(CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows())
        {
            await Task.Run(
                    () =>
                    {
                        if (File.Exists(WindowsStorePath))
                        {
                            File.Delete(WindowsStorePath);
                        }
                    },
                    cancellationToken
                )
                .ConfigureAwait(false);

            return;
        }

        if (OperatingSystem.IsMacOS())
        {
            await RunProcessAsync(
                    "security",
                    $"delete-generic-password -a \"{AccountName}\" -s \"{ServiceName}\"",
                    cancellationToken
                )
                .ConfigureAwait(false);

            return;
        }

        if (OperatingSystem.IsLinux())
        {
            await RunProcessAsync(
                    "secret-tool",
                    $"clear service \"{ServiceName}\" account \"{AccountName}\"",
                    cancellationToken
                )
                .ConfigureAwait(false);

            return;
        }

        throw new PlatformNotSupportedException("当前平台不支持安全存储。");
    }

    private static string EscapeShellArg(string value)
    {
        return value.Replace("\"", "\\\"", StringComparison.Ordinal);
    }

    private async Task<string?> RunProcessAsync(
        string fileName,
        string arguments,
        CancellationToken cancellationToken
    )
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        process.Start();

        var output = await process
            .StandardOutput.ReadToEndAsync(cancellationToken)
            .ConfigureAwait(false);
        var error = await process
            .StandardError.ReadToEndAsync(cancellationToken)
            .ConfigureAwait(false);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                logger.LogWarning("安全存储命令返回非零: {Error}", error.Trim());
            }

            return null;
        }

        return output.Trim();
    }

    private async Task RunProcessAsyncWithInputAsync(
        string fileName,
        string arguments,
        string input,
        CancellationToken cancellationToken
    )
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        process.Start();

        await process
            .StandardInput.WriteAsync(input.AsMemory(), cancellationToken)
            .ConfigureAwait(false);
        await process.StandardInput.FlushAsync(cancellationToken).ConfigureAwait(false);
        process.StandardInput.Close();

        var error = await process
            .StandardError.ReadToEndAsync(cancellationToken)
            .ConfigureAwait(false);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"写入安全存储失败，退出码: {process.ExitCode}，错误: {error}"
            );
        }
    }
}
