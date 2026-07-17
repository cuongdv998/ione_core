using System.Text;
using System.Text.Json;
using iOne.PartnerIntegration.Pti.Crypto;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Xunit;

namespace iOne.PartnerIntegration.Tests;

public class PtiRsaSignerPgpDecryptTests
{
    [Fact]
    public void NormalizeBase64JsonEnvelope_QuotedJsonString_Returns_Base64_No_Quotes()
    {
        var b64 = Convert.ToBase64String("hello-world"u8.ToArray());
        var wrapped = JsonSerializer.Serialize(b64);

        Assert.Equal(b64, PtiRsaSigner.NormalizeBase64JsonEnvelope(wrapped));
    }

    [Fact]
    public void NormalizeBase64JsonEnvelope_Raw_Base64_Trims_To_Self()
    {
        var b64 = Convert.ToBase64String("cipher"u8.ToArray());

        Assert.Equal(b64.Trim(), PtiRsaSigner.NormalizeBase64JsonEnvelope("  " + b64 + "\n"));
    }

    [Fact]
    public void NormalizeBase64JsonEnvelope_Json_Object_Throws()
    {
        const string fakeObject = "{\"x\":1}";
        Assert.Throws<InvalidOperationException>(() =>
            PtiRsaSigner.NormalizeBase64JsonEnvelope(fakeObject));
    }

    [Fact]
    public void DecryptEnvelope_RoundTrips_Ephemeral_Key_And_EncryptBody_Envelope_Format()
    {
        var secArmor = GenerateEphemeral1024RsaSecretRingArmored();
        var pubArmor = ArmorExportFirstEncryptionSubkey(secArmor);

        var plainJson = """{"transId":"t1","hello":"world"}""";

        var cipherB64 = PtiRsaSigner.EncryptWithPgpFromArmored(plainJson, pubArmor);
        var envelope = JsonSerializer.Serialize(cipherB64);

        var roundTrip = PtiRsaSigner.DecryptOpenPgpEncryptedBase64EnvelopeToUtf8(
            envelope,
            secArmor,
            pgpPrivateKeyPassphraseUtf8: null);

        Assert.Equal(plainJson, roundTrip);
    }

    [Fact]
    public void DecryptDirect_RoundTrips_Armored_Text()
    {
        var secArmor = GenerateEphemeral1024RsaSecretRingArmored();
        var pubArmor = ArmorExportFirstEncryptionSubkey(secArmor);

        var plainJson = """{"transId":"t1","hello":"world"}""";

        // Encrypt and sign (returns armored string)
        var armoredMessage = PtiRsaSigner.EncryptAndSignWithPgpFromArmored(
            plainJson,
            pubArmor,
            secArmor,
            "");

        var roundTrip = PtiRsaSigner.DecryptOpenPgpDirectToUtf8(
            armoredMessage,
            secArmor,
            pgpPrivateKeyPassphraseUtf8: null);

        Assert.Equal(plainJson, roundTrip);
    }

    [Fact]
    public void EncryptAndSign_RoundTrips_With_Signature_Verification()
    {
        var secArmor = GenerateEphemeral1024RsaSecretRingArmored();
        var pubArmor = ArmorExportFirstEncryptionSubkey(secArmor);

        var plainJson = """{"transId":"test-123","vehicleInfo":{"frameNumber":"ABC123"}}""";
        var passphrase = "";

        // Encrypt and sign
        var armoredMessage = PtiRsaSigner.EncryptAndSignWithPgpFromArmored(
            plainJson,
            pubArmor,
            secArmor,
            passphrase);

        // Verify it starts with PGP MESSAGE header
        Assert.StartsWith("-----BEGIN PGP MESSAGE-----", armoredMessage);
        Assert.Contains("-----END PGP MESSAGE-----", armoredMessage);

        // Decrypt and verify signature
        var decrypted = DecryptAndVerifyPgpMessage(armoredMessage, secArmor, passphrase);

        Assert.Equal(plainJson, decrypted);
    }

    /// <summary>
    /// Decrypts and verifies a PGP message (mirrors Java PGPUtils.decryptAndVerify).
    /// </summary>
    private static string DecryptAndVerifyPgpMessage(string armoredMessage, string armoredSecretKey, string passphrase)
    {
        using var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(armoredMessage));
        using var decoderStream = PgpUtilities.GetDecoderStream(messageStream);

        using var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(armoredSecretKey));
        using var keyDecoder = PgpUtilities.GetDecoderStream(keyStream);
        var secretKeyRings = new PgpSecretKeyRingBundle(keyDecoder);

        var objectFactory = new PgpObjectFactory(decoderStream);
        var firstObject = objectFactory.NextPgpObject();
        var encryptedDataList = (firstObject is PgpEncryptedDataList)
            ? (PgpEncryptedDataList)firstObject
            : (PgpEncryptedDataList)objectFactory.NextPgpObject();

        PgpPrivateKey? privateKey = null;
        PgpPublicKeyEncryptedData? encryptedData = null;

        foreach (PgpEncryptedData encData in encryptedDataList.GetEncryptedDataObjects())
        {
            if (encData is not PgpPublicKeyEncryptedData pkEncData)
                continue;

            var secretKey = FindSecretKey(secretKeyRings, pkEncData.KeyId);
            if (secretKey == null)
                continue;

            var passphraseChars = string.IsNullOrEmpty(passphrase) ? Array.Empty<char>() : passphrase.ToCharArray();
            privateKey = secretKey.ExtractPrivateKey(passphraseChars);
            encryptedData = pkEncData;
            break;
        }

        if (encryptedData == null || privateKey == null)
            throw new InvalidOperationException("Cannot decrypt: no matching secret key found.");

        using var clearStream = encryptedData.GetDataStream(privateKey);
        objectFactory = new PgpObjectFactory(clearStream);
        var message = objectFactory.NextPgpObject();

        // Uncompress if compressed
        if (message is PgpCompressedData compressedData)
        {
            var compStream = compressedData.GetDataStream();
            objectFactory = new PgpObjectFactory(compStream);
            message = objectFactory.NextPgpObject();
        }

        PgpOnePassSignature? onePassSig = null;
        PgpPublicKey? signingPublicKey = null;

        if (message is PgpOnePassSignatureList onePassList)
        {
            onePassSig = onePassList[0];
            signingPublicKey = FindPublicKey(secretKeyRings, onePassSig.KeyId);
            if (signingPublicKey != null)
                onePassSig.InitVerify(signingPublicKey);
            message = objectFactory.NextPgpObject();
        }

        if (message is not PgpLiteralData literalData)
            throw new InvalidOperationException("Expected PGPLiteralData after signature list.");

        using var literalStream = literalData.GetInputStream();
        using var resultStream = new MemoryStream();
        int ch;
        while ((ch = literalStream.ReadByte()) >= 0)
        {
            resultStream.WriteByte((byte)ch);
            onePassSig?.Update((byte)ch);
        }

        var plaintext = Encoding.UTF8.GetString(resultStream.ToArray());

        // Verify signature if present
        if (onePassSig != null)
        {
            var signatureList = (PgpSignatureList)objectFactory.NextPgpObject();
            var signature = signatureList[0];
            if (!onePassSig.Verify(signature))
                throw new InvalidOperationException("Signature verification failed.");
        }

        // Verify integrity
        if (encryptedData.IsIntegrityProtected())
        {
            if (!encryptedData.Verify())
                throw new InvalidOperationException("Message integrity check failed.");
        }

        return plaintext;
    }

    private static PgpSecretKey? FindSecretKey(PgpSecretKeyRingBundle bundle, long keyId)
    {
        foreach (PgpSecretKeyRing ring in bundle.GetKeyRings())
        {
            var key = ring.GetSecretKey(keyId);
            if (key != null)
                return key;
        }
        return null;
    }

    private static PgpPublicKey? FindPublicKey(PgpSecretKeyRingBundle bundle, long keyId)
    {
        foreach (PgpSecretKeyRing ring in bundle.GetKeyRings())
        {
            var secretKey = ring.GetSecretKey(keyId);
            if (secretKey != null)
                return secretKey.PublicKey;
        }
        return null;
    }

    /// <summary>
    /// Single-use RSA key ring (1024-bit, fast) with unencrypted private key material
    /// (empty passphrase) — matches how we call production decrypt with <c>null</c> passphrase bytes.
    /// </summary>
    private static string GenerateEphemeral1024RsaSecretRingArmored()
    {
        var rnd = new SecureRandom();
        var kpg = GeneratorUtilities.GetKeyPairGenerator("RSA");
        kpg.Init(new RsaKeyGenerationParameters(BigInteger.ValueOf(0x10001), rnd, 1024, 12));

        var masterPair = new PgpKeyPair(PublicKeyAlgorithmTag.RsaSign, kpg.GenerateKeyPair(), DateTime.UtcNow);
        var masterSub = new PgpSignatureSubpacketGenerator();
        masterSub.SetKeyFlags(false, PgpKeyFlags.CanCertify | PgpKeyFlags.CanSign);

        var encPair = new PgpKeyPair(PublicKeyAlgorithmTag.RsaGeneral, kpg.GenerateKeyPair(), DateTime.UtcNow);
        var encSub = new PgpSignatureSubpacketGenerator();
        encSub.SetKeyFlags(false, PgpKeyFlags.CanEncryptCommunications | PgpKeyFlags.CanEncryptStorage);

        var keyRingGen = new PgpKeyRingGenerator(
            PgpSignature.DefaultCertification,
            masterPair,
            "pti-rsa-test@example.invalid",
            SymmetricKeyAlgorithmTag.Aes128,
            "".ToCharArray(),
            true,
            masterSub.Generate(),
            null,
            rnd);
        keyRingGen.AddSubKey(encPair, encSub.Generate(), null);

        var secRing = keyRingGen.GenerateSecretKeyRing();
        using var sout = new MemoryStream();
        using (var arm = new ArmoredOutputStream(sout))
            secRing.Encode(arm);

        return Encoding.UTF8.GetString(sout.ToArray());
    }

    private static string ArmorExportFirstEncryptionSubkey(string armoredSecretKey)
    {
        using var keyIn = new MemoryStream(Encoding.UTF8.GetBytes(armoredSecretKey));
        using var decoded = PgpUtilities.GetDecoderStream(keyIn);

        foreach (PgpSecretKeyRing ring in new PgpSecretKeyRingBundle(decoded).GetKeyRings())
        {
            foreach (PgpSecretKey sk in ring.GetSecretKeys())
            {
                if (!sk.PublicKey.IsEncryptionKey)
                    continue;

                using var outMs = new MemoryStream();
                using (var aos = new ArmoredOutputStream(outMs))
                    sk.PublicKey.Encode(aos);

                return Encoding.UTF8.GetString(outMs.ToArray());
            }
        }

        throw new InvalidOperationException("No encryption subkey in generated ring.");
    }
}
