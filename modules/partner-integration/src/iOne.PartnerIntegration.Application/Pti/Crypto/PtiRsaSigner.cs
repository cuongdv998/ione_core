using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;
namespace iOne.PartnerIntegration.Pti.Crypto;

/// <summary>
/// RSA / PGP utility for PTI API integration.
/// Supports both standard PEM RSA keys (RSA/ECB/PKCS1Padding) and
/// OpenPGP public keys (the format PTI provides for UAT).
/// </summary>
public static class PtiRsaSigner
{
    // ─── PGP path ────────────────────────────────────────────────────────────

    /// <summary>
    /// Signs, compresses, and encrypts <paramref name="plainText"/> using OpenPGP,
    /// mirroring Java PGPUtils.encryptAndSign.
    /// Returns the ASCII-armored PGP message (BEGIN/END PGP MESSAGE block) as a string.
    /// </summary>
    /// <param name="plainText">The plaintext JSON payload to sign and encrypt.</param>
    /// <param name="armoredPublicKey">Partner's PGP public key (for encryption).</param>
    /// <param name="armoredSecretKey">Our PGP secret key (for signing).</param>
    /// <param name="secretKeyPassphrase">Passphrase for our secret key (UTF-8); empty string if unprotected.</param>
    public static string EncryptAndSignWithPgpFromArmored(
        string plainText,
        string armoredPublicKey,
        string armoredSecretKey,
        string secretKeyPassphrase)
    {
        if (string.IsNullOrWhiteSpace(armoredPublicKey))
            throw new ArgumentException("Armored PGP public key is required.", nameof(armoredPublicKey));
        if (string.IsNullOrWhiteSpace(armoredSecretKey))
            throw new ArgumentException("Armored PGP secret key is required.", nameof(armoredSecretKey));

        using var pubKeyStream = new MemoryStream(Encoding.UTF8.GetBytes(armoredPublicKey));
        var pgpPubKey = ReadFirstPgpPublicKey(pubKeyStream);

        using var secKeyStream = new MemoryStream(Encoding.UTF8.GetBytes(armoredSecretKey));
        using var secKeyDecoder = PgpUtilities.GetDecoderStream(secKeyStream);
        var pgpSecretKeyRingBundle = new PgpSecretKeyRingBundle(secKeyDecoder);

        PgpSecretKey? pgpSecretKey = null;
        foreach (PgpSecretKeyRing ring in pgpSecretKeyRingBundle.GetKeyRings())
        {
            foreach (PgpSecretKey key in ring.GetSecretKeys())
            {
                if (key.IsSigningKey)
                {
                    pgpSecretKey = key;
                    break;
                }
            }
            if (pgpSecretKey != null)
                break;
        }

        if (pgpSecretKey == null)
            throw new InvalidOperationException("No signing-capable PGP secret key found in the provided key ring.");

        var passphraseChars = string.IsNullOrEmpty(secretKeyPassphrase)
            ? Array.Empty<char>()
            : secretKeyPassphrase.ToCharArray();

        var pgpPrivateKey = pgpSecretKey.ExtractPrivateKey(passphraseChars);
        var dataBytes = Encoding.UTF8.GetBytes(plainText);

        // Step 1: Sign (armored=false) — one-pass signature + literal data + signature
        byte[] signedBytes;
        using (var signedOut = new MemoryStream())
        {
            var signatureGenerator = new PgpSignatureGenerator(
                pgpSecretKey.PublicKey.Algorithm,
                Org.BouncyCastle.Bcpg.HashAlgorithmTag.Sha1);
            signatureGenerator.InitSign(PgpSignature.BinaryDocument, pgpPrivateKey);

            // Write one-pass signature header
            signatureGenerator.GenerateOnePassVersion(false).Encode(signedOut);

            // Update signature generator with the data
            signatureGenerator.Update(dataBytes);

            // Add signer user ID subpacket
            var signSubPkgGen = new PgpSignatureSubpacketGenerator();
            var userIds = pgpSecretKey.PublicKey.GetUserIds();
            if (userIds != null)
            {
                foreach (object userId in userIds)
                {
                    signSubPkgGen.SetSignerUserId(false, userId.ToString());
                    break;
                }
            }
            signatureGenerator.SetHashedSubpackets(signSubPkgGen.Generate());

            // Write literal data
            var literalGen = new PgpLiteralDataGenerator();
            using (var literalOut = literalGen.Open(
                signedOut,
                PgpLiteralData.Binary,
                PgpLiteralData.Console,
                dataBytes.Length,
                DateTime.UtcNow))
            {
                literalOut.Write(dataBytes, 0, dataBytes.Length);
            }

            // Write signature packet
            signatureGenerator.Generate().Encode(signedOut);

            signedBytes = signedOut.ToArray();
        }

        // Step 2: Compress (ZIP, armored=false)
        byte[] compressedBytes;
        using (var compressedOut = new MemoryStream())
        {
            var compressGen = new PgpCompressedDataGenerator(Org.BouncyCastle.Bcpg.CompressionAlgorithmTag.Zip);
            using (var compStream = compressGen.Open(compressedOut))
            {
                compStream.Write(signedBytes, 0, signedBytes.Length);
            }
            compressGen.Close();
            compressedBytes = compressedOut.ToArray();
        }

        // Step 3: Encrypt (CAST5, armored=true)
        using var encryptedOut = new MemoryStream();
        using (var armoredOut = new Org.BouncyCastle.Bcpg.ArmoredOutputStream(encryptedOut))
        {
            var encGen = new PgpEncryptedDataGenerator(
                Org.BouncyCastle.Bcpg.SymmetricKeyAlgorithmTag.Cast5,
                true,
                new SecureRandom());
            encGen.AddMethod(pgpPubKey);

            using var encStream = encGen.Open(armoredOut, compressedBytes.Length);
            encStream.Write(compressedBytes, 0, compressedBytes.Length);
        }

        // Return the ASCII-armored string
        return Encoding.ASCII.GetString(encryptedOut.ToArray());
    }

    /// <summary>
    /// Encrypts <paramref name="plainText"/> using an OpenPGP armored public key string.
    /// </summary>
    public static string EncryptWithPgpFromArmored(string plainText, string armoredPublicKey)
    {
        if (string.IsNullOrWhiteSpace(armoredPublicKey))
            throw new ArgumentException("Armored PGP public key is required.", nameof(armoredPublicKey));

        using var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(armoredPublicKey));
        var pgpPubKey = ReadFirstPgpPublicKey(keyStream);
        return EncryptPlainWithPgpPublicKey(plainText, pgpPubKey);
    }

    /// <summary>
    /// Encrypts <paramref name="plainText"/> using the OpenPGP public key at
    /// <paramref name="publicKeyPath"/> and returns a Base64-encoded result.
    /// Use this when the key file begins with "-----BEGIN PGP PUBLIC KEY BLOCK-----".
    /// </summary>
    public static string EncryptWithPgp(string plainText, string publicKeyPath)
    {
        if (!File.Exists(publicKeyPath))
            throw new FileNotFoundException($"PTI PGP public key file not found: {publicKeyPath}");

        return EncryptWithPgpFromArmored(plainText, File.ReadAllText(publicKeyPath));
    }

    private static string EncryptPlainWithPgpPublicKey(string plainText, PgpPublicKey pgpPubKey)
    {
        var inputBytes = Encoding.UTF8.GetBytes(plainText);

        using var outputStream = new MemoryStream();
        // Armored (ASCII) output so it travels as a JSON string
        using (var armoredOut = new Org.BouncyCastle.Bcpg.ArmoredOutputStream(outputStream))
        {
            var encGen = new PgpEncryptedDataGenerator(
                Org.BouncyCastle.Bcpg.SymmetricKeyAlgorithmTag.Aes256,
                true,
                new SecureRandom());
            encGen.AddMethod(pgpPubKey);

            using var encStream = encGen.Open(armoredOut, inputBytes.Length);
            encStream.Write(inputBytes, 0, inputBytes.Length);
        }

        return Convert.ToBase64String(outputStream.ToArray());
    }

    /// <summary>
    /// Unwraps EncryptBody-format HTTP body (JSON-encoded string literal or raw text),
    /// then decrypts the OpenPGP message directly (no intermediate Base64 conversion)
    /// using <paramref name="armoredPrivateKey"/>.
    /// </summary>
    /// <param name="responseBodyWire">Wire body — JSON quoted ciphertext or raw text (armored PGP).</param>
    /// <param name="armoredPrivateKey">Armored secret key bundle.</param>
    /// <param name="pgpPrivateKeyPassphraseUtf8">
    /// Passphrase as UTF-8 bytes; <c>null</c> or empty means an unencrypted key material.
    /// </param>
    public static string DecryptOpenPgpDirectToUtf8(
        string responseBodyWire,
        string armoredPrivateKey,
        byte[]? pgpPrivateKeyPassphraseUtf8 = null)
    {
        if (string.IsNullOrWhiteSpace(responseBodyWire))
            throw new ArgumentException("Response body is required.", nameof(responseBodyWire));
        if (string.IsNullOrWhiteSpace(armoredPrivateKey))
            throw new ArgumentException("Armored PGP private key is required.", nameof(armoredPrivateKey));

        var rawText = NormalizeBase64JsonEnvelope(responseBodyWire);
        var bytes = Encoding.UTF8.GetBytes(rawText);

        return DecryptOpenPgpMessageFromBytesToUtf8(
            bytes,
            armoredPrivateKey,
            pgpPrivateKeyPassphraseUtf8 ?? Array.Empty<byte>());
    }

    /// <summary>
    /// Unwraps EncryptBody-format HTTP body (JSON-encoded string literal or raw Base64),
    /// then decrypts the OpenPGP message using <paramref name="armoredPrivateKey"/>.
    /// </summary>
    /// <param name="responseBodyWire">Wire body — JSON quoted base64 ciphertext or plaintext Base64.</param>
    /// <param name="armoredPrivateKey">Armored secret key bundle.</param>
    /// <param name="pgpPrivateKeyPassphraseUtf8">
    /// Passphrase as UTF-8 bytes; <c>null</c> or empty means an unencrypted key material (try empty passphrase).
    /// </param>
    public static string DecryptOpenPgpEncryptedBase64EnvelopeToUtf8(
        string responseBodyWire,
        string armoredPrivateKey,
        byte[]? pgpPrivateKeyPassphraseUtf8 = null)
    {
        if (string.IsNullOrWhiteSpace(responseBodyWire))
            throw new ArgumentException("Response body is required.", nameof(responseBodyWire));
        if (string.IsNullOrWhiteSpace(armoredPrivateKey))
            throw new ArgumentException("Armored PGP private key is required.", nameof(armoredPrivateKey));

        var base64Cipher = NormalizeBase64JsonEnvelope(responseBodyWire);
        byte[] armorOrCompressedBytes;
        try
        {
            armorOrCompressedBytes = Convert.FromBase64String(base64Cipher);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                "PTI response ciphertext is not valid Base64 after JSON envelope unwrap.", ex);
        }

        return DecryptOpenPgpMessageFromBytesToUtf8(
            armorOrCompressedBytes,
            armoredPrivateKey,
            pgpPrivateKeyPassphraseUtf8 ?? Array.Empty<byte>());
    }

    /// <summary>Unwrap EncryptBody ciphertext: accepts <c>"\\"base64\\""</c> JSON document or trimmed Base64.</summary>
    public static string NormalizeBase64JsonEnvelope(string raw)
    {
        var trimmed = raw.Trim();
        if (trimmed.StartsWith("{", StringComparison.Ordinal)
            || trimmed.StartsWith("[", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Unexpected PTI response envelope: JSON object/array not supported for ciphertext unwrap.");
        }

        try
        {
            var asString = JsonSerializer.Deserialize<string>(trimmed,
                JsonOptionsCaseSensitiveString);
            if (!string.IsNullOrEmpty(asString))
                return asString.Trim();
        }
        catch (JsonException)
        {
            // fall through — body may already be naked Base64
        }

        if (trimmed.Length >= 2
            && trimmed[0] == '"'
            && trimmed[^1] == '"')
        {
            return JsonSerializer.Deserialize<string>(trimmed, JsonOptionsCaseSensitiveString)?.Trim()
                   ?? trimmed.Trim('"');
        }

        return trimmed;
    }

    private static readonly JsonSerializerOptions JsonOptionsCaseSensitiveString = new()
    {
        PropertyNameCaseInsensitive = false,
        AllowTrailingCommas = true
    };

    private static string DecryptOpenPgpMessageFromBytesToUtf8(byte[] ciphertextBytes,
        string armoredPrivateKey, byte[] passphraseUtf8Bytes)
    {
        using var inputStream = new MemoryStream(ciphertextBytes, writable: false);
        using var decoderStream = PgpUtilities.GetDecoderStream(inputStream);

        using var secretIn = new MemoryStream(Encoding.UTF8.GetBytes(armoredPrivateKey), writable: false);
        using var secretDecoder = PgpUtilities.GetDecoderStream(secretIn);

        var pgpSecretKeyRingBundle = new PgpSecretKeyRingBundle(secretDecoder);

        var pgpFact = new PgpObjectFactory(decoderStream);
        object? outer = TryNextDecryptableEnvelope(pgpFact);
        while (outer is not PgpEncryptedDataList && outer != null)
            outer = TryNextDecryptableEnvelope(pgpFact);

        if (outer is not PgpEncryptedDataList encList)
            throw new InvalidOperationException(
                "Could not locate PgpEncryptedDataList in decrypted PTI response message.");

        PgpPrivateKey? privateKeyResolved = null;
        PgpPublicKeyEncryptedData? matchingEnc = null;

        foreach (PgpEncryptedData encItem in encList.GetEncryptedDataObjects())
        {
            if (encItem is not PgpPublicKeyEncryptedData encData)
                continue;

            var candidateSecret = FindSecretKey(pgpSecretKeyRingBundle, encData.KeyId);
            if (candidateSecret == null)
                continue;

            try
            {
                char[] passphraseChars = passphraseUtf8Bytes.Length == 0
                    ? []
                    : Encoding.UTF8.GetString(passphraseUtf8Bytes).ToCharArray();

                var extracted = passphraseChars.Length == 0
                    ? candidateSecret.ExtractPrivateKey(Array.Empty<char>())
                    : candidateSecret.ExtractPrivateKey(passphraseChars);

                matchingEnc = encData;
                privateKeyResolved = extracted;
                break;
            }
            catch (Org.BouncyCastle.Bcpg.OpenPgp.PgpException)
            {
                continue;
            }
        }

        if (matchingEnc == null || privateKeyResolved == null)
            throw new InvalidOperationException(
                "Cannot decrypt PTI response: no secret key accepts the ciphertext session.");

        Stream clearCompressed;
        try
        {
            clearCompressed = matchingEnc.GetDataStream(privateKeyResolved);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("OpenPGP session decrypt failed.", ex);
        }

        using (clearCompressed)
        {
            using var buffer = new MemoryStream();
            clearCompressed.CopyTo(buffer);
            var decryptedBytes = buffer.ToArray();

            return TryDecryptOpenPgpWrappedPayloadUtf8(decryptedBytes);
        }
    }

    /// <summary>
    /// EncryptBody uses AES; inner payload may be raw UTF-8 (our encrypt helper) or
    /// <see cref="PgpLiteralData"/> / <see cref="PgpCompressedData"/> wrappers.
    /// </summary>
    private static string TryDecryptOpenPgpWrappedPayloadUtf8(ReadOnlySpan<byte> decryptedBytes)
    {
        var copy = decryptedBytes.ToArray();
        try
        {
            using var stm = new MemoryStream(copy, writable: false);
            var factory = new PgpObjectFactory(stm);
            var plainObject = factory.NextPgpObject();

            while (plainObject is PgpOnePassSignatureList)
                plainObject = factory.NextPgpObject();

            switch (plainObject)
            {
                case PgpCompressedData compressed:
                    return ReadCompressedLiteralUtf8(compressed);
                case PgpLiteralData literalDirect:
                    return ReadLiteralUtf8(literalDirect);
                default:
                    // Not a recognizable inner OpenPGP wrapper — treat as plaintext (JSON/UTF-8).
                    break;
            }
        }
        catch (IOException)
        {
            // Not nested OpenPGP packets (plaintext written directly inside the encrypted envelope).
        }

        return Encoding.UTF8.GetString(copy);
    }

    private static object? TryNextDecryptableEnvelope(PgpObjectFactory pgpFact)
        => pgpFact.NextPgpObject();

    private static PgpSecretKey? FindSecretKey(PgpSecretKeyRingBundle rings, long keyId)
    {
        foreach (PgpSecretKeyRing ring in rings.GetKeyRings())
        {
            var sk = ring.GetSecretKey(keyId);
            if (sk != null)
                return sk;
        }

        return null;
    }

    private static string ReadCompressedLiteralUtf8(PgpCompressedData compressed)
    {
        using var compIn = compressed.GetDataStream();
        var factCompressed = new PgpObjectFactory(compIn);
        var next = factCompressed.NextPgpObject();
        while (next is PgpEncryptedDataList || next is PgpOnePassSignatureList)
            next = factCompressed.NextPgpObject();

        if (next is PgpLiteralData literal)
            return ReadLiteralUtf8(literal);
        throw new InvalidOperationException(
            $"Compressed PGP wrapper did not yield literal data ({next?.GetType().Name}).");
    }

    private static string ReadLiteralUtf8(PgpLiteralData literal)
    {
        using var unc = literal.GetInputStream();
        using var ms = new MemoryStream();
        unc.CopyTo(ms);
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    // ─── Standard PEM RSA path ───────────────────────────────────────────────

    /// <summary>
    /// Encrypts <paramref name="plainText"/> using PEM-encoded RSA subject public key (SPKI) text.
    /// Matches Java RSAUtil.encrypt (RSA/ECB/PKCS1Padding).
    /// </summary>
    public static string EncryptFromPemText(string plainText, string pemPublicKey)
    {
        if (string.IsNullOrWhiteSpace(pemPublicKey))
            throw new ArgumentException("PEM public key text is required.", nameof(pemPublicKey));

        var keyBytes = LoadKeyBytesFromPemText(pemPublicKey);
        using var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);

        var inputBytes = Encoding.UTF8.GetBytes(plainText);
        var encrypted = rsa.Encrypt(inputBytes, RSAEncryptionPadding.Pkcs1);
        return Convert.ToBase64String(encrypted);
    }

    /// <summary>
    /// Encrypts <paramref name="plainText"/> using a standard PEM RSA public key file.
    /// Matches Java RSAUtil.encrypt (RSA/ECB/PKCS1Padding).
    /// </summary>
    public static string Encrypt(string plainText, string publicKeyPath)
    {
        if (!File.Exists(publicKeyPath))
            throw new FileNotFoundException($"PTI key file not found: {publicKeyPath}");

        return EncryptFromPemText(plainText, File.ReadAllText(publicKeyPath));
    }

    /// <summary>
    /// Decrypts a Base64-encoded cipher using a standard PEM RSA private key file.
    /// </summary>
    public static string Decrypt(string encryptedBase64, string privateKeyPath)
    {
        var keyBytes = LoadKeyBytes(privateKeyPath);
        using var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(keyBytes, out _);

        var cipherBytes = Convert.FromBase64String(encryptedBase64);
        var decrypted = rsa.Decrypt(cipherBytes, RSAEncryptionPadding.Pkcs1);
        return Encoding.UTF8.GetString(decrypted);
    }

    /// <summary>
    /// Signs <paramref name="plainText"/> with a standard PEM RSA private key using SHA-256 + PKCS1.
    /// Used when SignatureMode = "Header".
    /// </summary>
    public static string Sign(string plainText, string privateKeyPath)
    {
        var keyBytes = LoadKeyBytes(privateKeyPath);
        using var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(keyBytes, out _);

        var inputBytes = Encoding.UTF8.GetBytes(plainText);
        var signature = rsa.SignData(inputBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return Convert.ToBase64String(signature);
    }

    // ─── Auto-detect helper ──────────────────────────────────────────────────

    /// <summary>
    /// Encrypts using PGP or PEM RSA based on the first non-empty line of <paramref name="keyArmoredOrPem"/>.
    /// </summary>
    public static string EncryptAutoFromKeyText(string plainText, string keyArmoredOrPem)
    {
        if (string.IsNullOrWhiteSpace(keyArmoredOrPem))
            throw new ArgumentException("Public key material is required.", nameof(keyArmoredOrPem));

        var firstNonEmpty = ReadFirstNonEmptyLine(keyArmoredOrPem);
        if (firstNonEmpty.Contains("PGP", StringComparison.OrdinalIgnoreCase))
            return EncryptWithPgpFromArmored(plainText, keyArmoredOrPem);

        return EncryptFromPemText(plainText, keyArmoredOrPem);
    }

    /// <summary>
    /// Encrypts using PGP or PEM RSA automatically based on the key file header.
    /// </summary>
    public static string EncryptAuto(string plainText, string publicKeyPath)
    {
        if (!File.Exists(publicKeyPath))
            throw new FileNotFoundException($"PTI public key file not found: {publicKeyPath}");

        return EncryptAutoFromKeyText(plainText, File.ReadAllText(publicKeyPath));
    }

    // ─── Internals ───────────────────────────────────────────────────────────

    private static byte[] LoadKeyBytes(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"PTI key file not found: {filePath}");

        return LoadKeyBytesFromPemText(File.ReadAllText(filePath));
    }

    private static byte[] LoadKeyBytesFromPemText(string pem)
    {
        var sb = new StringBuilder();
        using var reader = new StringReader(pem);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.StartsWith("-----"))
                continue;

            foreach (var c in line)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') ||
                    (c >= '0' && c <= '9') || c == '+' || c == '/' || c == '=')
                {
                    sb.Append(c);
                }
            }
        }

        return Convert.FromBase64String(sb.ToString());
    }

    private static string ReadFirstNonEmptyLine(string text)
    {
        using var reader = new StringReader(text);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (!string.IsNullOrWhiteSpace(line))
                return line;
        }

        return string.Empty;
    }

    private static PgpPublicKey ReadFirstPgpPublicKey(Stream keyStream)
    {
        using var copyAll = new MemoryStream();
        keyStream.CopyTo(copyAll);
        var data = copyAll.ToArray();

        try
        {
            using var bundleMs = new MemoryStream(data, writable: false);
            using var bundleDecoder = PgpUtilities.GetDecoderStream(bundleMs);
            var bundle = new PgpPublicKeyRingBundle(bundleDecoder);
            foreach (PgpPublicKeyRing ring in bundle.GetKeyRings())
            {
                foreach (PgpPublicKey key in ring.GetPublicKeys())
                {
                    if (key.IsEncryptionKey)
                        return key;
                }
            }
        }
        catch (PgpException)
        {
            using var fallbackMs = new MemoryStream(data, writable: false);
            using var decoderStream = PgpUtilities.GetDecoderStream(fallbackMs);
            var factory = new PgpObjectFactory(decoderStream);
            while (factory.NextPgpObject() is { } chunk)
            {
                if (chunk is PgpPublicKeyRing ringChunk)
                {
                    foreach (PgpPublicKey key in ringChunk.GetPublicKeys())
                    {
                        if (key.IsEncryptionKey)
                            return key;
                    }
                }
                else if (chunk is PgpPublicKey single && single.IsEncryptionKey)
                    return single;
            }

            throw;
        }

        throw new InvalidOperationException("No encryption-capable PGP public key found in the key file.");
    }
}
