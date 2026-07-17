using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using iOne.File.Localization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spire.Doc;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace iOne.File.Conversion;

/// <summary>
/// Word → PDF using FreeSpire.Doc. Free edition has usage limits; for production consider a commercial license or LibreOffice-based conversion.
/// </summary>
public class FileConversionAppService : ApplicationService, IFileConversionAppService
{
    private const int MaxPrivateFontPathEntries = 400;

    private readonly FileWordToPdfOptions _wordToPdfOptions;
    private readonly IHostEnvironment _hostEnvironment;

    public FileConversionAppService(
        IOptions<FileWordToPdfOptions> wordToPdfOptions,
        IHostEnvironment hostEnvironment)
    {
        LocalizationResource = typeof(FileResource);
        _wordToPdfOptions = wordToPdfOptions.Value;
        _hostEnvironment = hostEnvironment;
    }

    public virtual Task<byte[]> ConvertWordToPdfAsync(byte[] wordContent, string fileName, CancellationToken cancellationToken = default)
    {
        return ConvertWordToPdfInternalAsync(wordContent, fileName, cancellationToken);
    }

    private async Task<byte[]> ConvertWordToPdfInternalAsync(byte[] wordContent, string fileName, CancellationToken cancellationToken)
    {
        if (wordContent == null || wordContent.Length == 0)
            throw new BusinessException("File:WordToPdf:EmptyFile");

        var ext = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
        if (ext != ".doc" && ext != ".docx")
            throw new BusinessException("File:WordToPdf:UnsupportedExtension");

        var loadFormat = ext == ".doc" ? FileFormat.Doc : FileFormat.Docx;

        try
        {
            if (_wordToPdfOptions.UseLibreOffice)
            {
                try
                {
                    return await ConvertWithLibreOfficeAsync(wordContent, ext, cancellationToken);
                }
                catch (Exception ex) when (_wordToPdfOptions.FallbackToSpireOnLibreOfficeFailure)
                {
                    Logger.LogWarning(ex, "LibreOffice conversion failed for {FileName}, fallback to Spire.Doc.", fileName);
                }
            }

            return ConvertWithSpire(wordContent, loadFormat);
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Word to PDF conversion failed for {FileName}: {Message}", fileName, ex.Message);
            var detail = TruncateForClient(ex);
            throw new BusinessException("File:WordToPdf:ConversionFailed").WithData("detail", detail);
        }
    }

    private byte[] ConvertWithSpire(byte[] wordContent, FileFormat loadFormat)
    {
        using var input = new MemoryStream(wordContent, writable: false);
        using var document = new Document();
        document.LoadFromStream(input, loadFormat);

        var fontDirectories = ResolveFontDirectories();
        if (fontDirectories.Count > 0)
        {
            document.SetCustomFontsFolders(string.Join(";", fontDirectories));
        }

        using var output = new MemoryStream();
        var pdfParams = BuildToPdfParameterList(fontDirectories);
        document.SaveToStream(output, pdfParams);
        return output.ToArray();
    }

    private async Task<byte[]> ConvertWithLibreOfficeAsync(byte[] wordContent, string extension, CancellationToken cancellationToken)
    {
        var workDir = Path.Combine(Path.GetTempPath(), "ione-word2pdf", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDir);

        var inputPath = Path.Combine(workDir, $"source{extension}");
        var outputPath = Path.Combine(workDir, "source.pdf");
        await System.IO.File.WriteAllBytesAsync(inputPath, wordContent, cancellationToken);

        try
        {
            var timeout = _wordToPdfOptions.LibreOfficeTimeoutSeconds <= 0
                ? 120
                : _wordToPdfOptions.LibreOfficeTimeoutSeconds;

            var psi = new ProcessStartInfo
            {
                FileName = string.IsNullOrWhiteSpace(_wordToPdfOptions.LibreOfficeBinaryPath)
                    ? "soffice"
                    : _wordToPdfOptions.LibreOfficeBinaryPath,
                ArgumentList =
                {
                    "--headless",
                    "--nologo",
                    "--nolockcheck",
                    "--nodefault",
                    "--norestore",
                    "--convert-to",
                    "pdf:writer_pdf_Export",
                    "--outdir",
                    workDir,
                    inputPath
                },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            if (!process.Start())
            {
                throw new InvalidOperationException("Unable to start LibreOffice process.");
            }

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeout));
            await process.WaitForExitAsync(timeoutCts.Token);

            var stdOut = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stdErr = await process.StandardError.ReadToEndAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"LibreOffice exited with code {process.ExitCode}. stdout: {stdOut}. stderr: {stdErr}");
            }

            if (!System.IO.File.Exists(outputPath))
            {
                throw new FileNotFoundException(
                    $"LibreOffice did not generate output PDF. stdout: {stdOut}. stderr: {stdErr}", outputPath);
            }

            return await System.IO.File.ReadAllBytesAsync(outputPath, cancellationToken);
        }
        finally
        {
            TryDeleteDirectory(workDir);
        }
    }

    private ToPdfParameterList BuildToPdfParameterList(IReadOnlyList<string> fontDirectories)
    {
        var pdfParams = new ToPdfParameterList();
        if (_wordToPdfOptions.EmbedAllFontsInPdf)
        {
            pdfParams.IsEmbeddedAllFonts = true;
        }

        var fontFiles = CollectFontFiles(fontDirectories);
        foreach (var path in fontFiles)
        {
            pdfParams.PrivateFontPaths.Add(new PrivateFontPath(path, string.Empty));
        }

        return pdfParams;
    }

    private IReadOnlyList<string> ResolveFontDirectories()
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddIfExists(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var full = Path.IsPathRooted(path)
                ? path.Trim()
                : Path.Combine(_hostEnvironment.ContentRootPath, path.Trim());
            if (Directory.Exists(full))
            {
                set.Add(full);
            }
        }

        AddIfExists(_wordToPdfOptions.CustomFontsDirectory);
        AddIfExists(Path.Combine(_hostEnvironment.ContentRootPath, "Fonts"));

        if (_wordToPdfOptions.UseWellKnownSystemFontFolders)
        {
            foreach (var root in GetWellKnownFontRoots())
            {
                AddIfExists(root);
            }
        }

        return set.ToList();
    }

    private static IReadOnlyList<string> GetWellKnownFontRoots()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new[] { @"C:\Windows\Fonts" };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return new[]
            {
                "/System/Library/Fonts",
                "/Library/Fonts",
                string.IsNullOrWhiteSpace(userProfile) ? string.Empty : Path.Combine(userProfile, "Library", "Fonts")
            };
        }

        // Linux default
        return new[]
        {
            "/usr/share/fonts",
            "/usr/local/share/fonts"
        };
    }

    private static IReadOnlyList<string> CollectFontFiles(IReadOnlyList<string> directories)
    {
        var result = new List<string>();
        var patterns = new[] { "*.ttf", "*.otf", "*.ttc" };

        foreach (var dir in directories)
        {
            if (!Directory.Exists(dir))
            {
                continue;
            }

            foreach (var pattern in patterns)
            {
                foreach (var file in Directory.EnumerateFiles(dir, pattern, SearchOption.AllDirectories))
                {
                    result.Add(file);
                    if (result.Count >= MaxPrivateFontPathEntries)
                    {
                        return result;
                    }
                }
            }
        }

        return result;
    }

    private static string TruncateForClient(Exception ex)
    {
        var s = string.IsNullOrWhiteSpace(ex.Message) ? ex.GetType().Name : ex.Message.Trim();
        const int max = 500;
        return s.Length <= max ? s : s[..max] + "…";
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // ignore cleanup errors
        }
    }
}
