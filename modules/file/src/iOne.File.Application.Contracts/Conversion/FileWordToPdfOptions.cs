namespace iOne.File.Conversion;

/// <summary>
/// Optional settings for Word → PDF (Spire.Doc). Use when the server has no system fonts (e.g. Linux/Docker/macOS without Office fonts).
/// </summary>
public class FileWordToPdfOptions
{
    public const string SectionName = "File:WordToPdf";

    /// <summary>
    /// Prefer LibreOffice headless for Word → PDF conversion on all platforms.
    /// </summary>
    public bool UseLibreOffice { get; set; } = true;

    /// <summary>
    /// Binary name/path used to invoke LibreOffice in headless mode (e.g. <c>soffice</c>).
    /// </summary>
    public string LibreOfficeBinaryPath { get; set; } = "soffice";

    /// <summary>
    /// Maximum waiting time (seconds) for one LibreOffice conversion process.
    /// </summary>
    public int LibreOfficeTimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// If LibreOffice conversion fails, allow fallback to Spire.Doc conversion.
    /// </summary>
    public bool FallbackToSpireOnLibreOfficeFailure { get; set; } = true;

    /// <summary>
    /// Embed font subsets in the PDF so viewers render Vietnamese and other scripts correctly (recommended on Linux).
    /// </summary>
    public bool EmbedAllFontsInPdf { get; set; } = true;

    /// <summary>
    /// Register common OS font directories (e.g. <c>/usr/share/fonts</c> in Docker) with Spire so Word’s font names map to real files.
    /// </summary>
    public bool UseWellKnownSystemFontFolders { get; set; } = true;

    /// <summary>
    /// Folder containing .ttf/.otf files for Spire (e.g. Liberation, DejaVu, Noto). Absolute path, or relative to <see cref="Microsoft.Extensions.Hosting.IHostEnvironment.ContentRootPath"/>.
    /// If unset, <c>ContentRoot/Fonts</c> is used when that directory exists.
    /// </summary>
    public string? CustomFontsDirectory { get; set; }
}
