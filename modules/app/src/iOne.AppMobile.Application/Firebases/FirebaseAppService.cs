using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using iOne.AppMobile.Firebases;
using iOne.AppMobile.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;

namespace iOne.AppMobile.Firebases;

public class FirebaseAppService : ApplicationService, IFireBaseAppService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FirebaseAppService> _logger;
    private static bool _isInitialized;
    private static readonly object _lock = new();

    public FirebaseAppService(
        IConfiguration configuration,
        ILogger<FirebaseAppService> logger)
    {
        _configuration = configuration;
        _logger = logger;

    }



    public static void InitializeFirebase(IConfiguration configuration, ILogger logger)
    {
        if (_isInitialized) return;

        lock (_lock)
        {
            if (_isInitialized) return;

            try
            {
                var credentialPath = configuration["Firebase:CredentialPath"];
                GoogleCredential credential = null;

                if (!string.IsNullOrEmpty(credentialPath) && System.IO.File.Exists(credentialPath))
                {
                    credential = GoogleCredential.FromFile(credentialPath);
                }
                else
                {
                    var envPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
                    if (!string.IsNullOrEmpty(envPath) && System.IO.File.Exists(envPath))
                    {
                        credential = GoogleCredential.GetApplicationDefault();
                    }
                }

                if (credential == null)
                {
                    logger.LogWarning("Firebase credential path is not configured or file not found. Firebase will not be initialized.");
                    return;
                }

                // Add scope for Firebase Cloud Messaging
                credential = credential.CreateScoped(new[]
                {
                    "https://www.googleapis.com/auth/firebase.messaging"
                });

                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = credential
                    });
                    logger.LogInformation("Firebase initialized successfully.");
                }

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to initialize Firebase. Firebase will not be available.");
                return;
            }
        }
    }

    public async Task<object> CheckFirebaseAuthAsync()
    {
        try
        {
            var path = _configuration["Firebase:CredentialPath"];

            if (string.IsNullOrEmpty(path))
            {
                return new
                {
                    success = false,
                    message = "Firebase credential path not configured"
                };
            }

            if (!File.Exists(path))
            {
                return new
                {
                    success = false,
                    message = "Credential file not found",
                    path
                };
            }

            var credential = GoogleCredential
                .FromFile(path)
                .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

            var accessToken = await credential
                .UnderlyingCredential
                .GetAccessTokenForRequestAsync();

            return new
            {
                success = true,
                message = "Firebase credential works",
                tokenLength = accessToken.Length
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Firebase auth check failed");

            return new
            {
                success = false,
                message = "Firebase credential failed",
                error = ex.Message
            };
        }
    }

    /// <summary>
    /// Send push notification to a single device
    /// </summary>
    public async Task<string> SendNotificationAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        var message = new Message
        {
            Token = deviceToken,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = data
        };

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation("Successfully sent message: {Response}", response);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to device: {DeviceToken}", deviceToken);
            throw;
        }
    }

    /// <summary>
    /// Send push notification to multiple devices
    /// </summary>
    public async Task<BatchResponse> SendNotificationToMultipleDevicesAsync(
        List<string> deviceTokens,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        var message = new MulticastMessage
        {
            Tokens = deviceTokens,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = data
        };

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
            _logger.LogInformation("Successfully sent {SuccessCount}/{TotalCount} messages",
                response.SuccessCount, deviceTokens.Count);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to multiple devices");
            throw;
        }
    }

    /// <summary>
    /// Send push notification to a topic
    /// </summary>
    public async Task<string> SendNotificationToTopicAsync(
        string topic,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        var message = new Message
        {
            Topic = topic,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = data
        };

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation("Successfully sent message to topic {Topic}: {Response}", topic, response);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to topic: {Topic}", topic);
            throw;
        }
    }

    /// <summary>
    /// Subscribe devices to a topic
    /// </summary>
    public async Task<TopicManagementResponse> SubscribeToTopicAsync(List<string> deviceTokens, string topic)
    {
        // Validate input
        if (deviceTokens == null || deviceTokens.Count == 0)
        {
            _logger.LogWarning("Device tokens list is null or empty");
            throw new ArgumentException("Device tokens cannot be null or empty", nameof(deviceTokens));
        }

        if (string.IsNullOrWhiteSpace(topic))
        {
            _logger.LogWarning("Topic name is null or empty");
            throw new ArgumentException("Topic cannot be null or empty", nameof(topic));
        }

        // Validate topic name: Firebase topics can only contain letters, numbers, underscores, and hyphens
        // Topic names are case-insensitive and must start with a letter
        if (!System.Text.RegularExpressions.Regex.IsMatch(topic, @"^[a-zA-Z][a-zA-Z0-9_-]*$"))
        {
            _logger.LogWarning("Invalid topic name format: {Topic}. Topic must start with a letter and contain only letters, numbers, underscores, and hyphens", topic);
            throw new ArgumentException($"Invalid topic name format. Topic must start with a letter and contain only letters, numbers, underscores, and hyphens. Got: {topic}", nameof(topic));
        }

        // Filter out null/empty tokens and log warnings
        var validTokens = deviceTokens.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
        if (validTokens.Count != deviceTokens.Count)
        {
            _logger.LogWarning("Some device tokens are null or empty. Valid tokens: {ValidCount}/{TotalCount}", validTokens.Count, deviceTokens.Count);
        }

        if (validTokens.Count == 0)
        {
            _logger.LogWarning("No valid device tokens provided");
            throw new ArgumentException("No valid device tokens provided", nameof(deviceTokens));
        }

        // Validate FCM token format (basic check)
        // FCM tokens are typically 140-200 characters long and contain alphanumeric characters, hyphens, underscores, and colons
        var invalidTokens = validTokens.Where(t => t.Length < 100 || t.Length > 250 || t.Contains(" ")).ToList();
        if (invalidTokens.Any())
        {
            _logger.LogWarning("Some device tokens appear to be invalid (too short/long or contain spaces). Invalid tokens: {InvalidTokens}",
                string.Join(", ", invalidTokens.Select((t, i) => $"Token[{i}]: {t.Substring(0, Math.Min(20, t.Length))}...")));
            _logger.LogWarning("FCM tokens should be 140-200 characters long and not contain spaces. Please ensure you're using real FCM tokens from your mobile app.");
        }

        try
        {
            _logger.LogInformation("Subscribing {TokenCount} devices to topic: {Topic}", validTokens.Count, topic);
            var response = await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(validTokens, topic);

            _logger.LogInformation("Successfully subscribed {SuccessCount}/{TotalCount} devices to topic {Topic}",
                response.SuccessCount, validTokens.Count, topic);

            // Log errors if any
            if (response.FailureCount > 0 && response.Errors != null)
            {
                foreach (var error in response.Errors)
                {
                    _logger.LogWarning("Failed to subscribe device at index {Index} to topic {Topic}. Reason: {Reason}. Token: {Token}",
                        error.Index, topic, error.Reason,
                        error.Index < validTokens.Count ? validTokens[error.Index] : "unknown");
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe devices to topic: {Topic}. Token count: {TokenCount}", topic, validTokens.Count);
            throw;
        }
    }

    /// <summary>
    /// Unsubscribe devices from a topic
    /// </summary>
    public async Task<TopicManagementResponse> UnsubscribeFromTopicAsync(List<string> deviceTokens, string topic)
    {
        // Validate input
        if (deviceTokens == null || deviceTokens.Count == 0)
        {
            _logger.LogWarning("Device tokens list is null or empty");
            throw new ArgumentException("Device tokens cannot be null or empty", nameof(deviceTokens));
        }

        if (string.IsNullOrWhiteSpace(topic))
        {
            _logger.LogWarning("Topic name is null or empty");
            throw new ArgumentException("Topic cannot be null or empty", nameof(topic));
        }

        // Filter out null/empty tokens
        var validTokens = deviceTokens.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
        if (validTokens.Count == 0)
        {
            _logger.LogWarning("No valid device tokens provided");
            throw new ArgumentException("No valid device tokens provided", nameof(deviceTokens));
        }

        try
        {
            _logger.LogInformation("Unsubscribing {TokenCount} devices from topic: {Topic}", validTokens.Count, topic);
            var response = await FirebaseMessaging.DefaultInstance.UnsubscribeFromTopicAsync(validTokens, topic);

            _logger.LogInformation("Successfully unsubscribed {SuccessCount}/{TotalCount} devices from topic {Topic}",
                response.SuccessCount, validTokens.Count, topic);

            // Log errors if any
            if (response.FailureCount > 0 && response.Errors != null)
            {
                foreach (var error in response.Errors)
                {
                    _logger.LogWarning("Failed to unsubscribe device at index {Index} from topic {Topic}. Reason: {Reason}. Token: {Token}",
                        error.Index, topic, error.Reason,
                        error.Index < validTokens.Count ? validTokens[error.Index] : "unknown");
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unsubscribe devices from topic: {Topic}. Token count: {TokenCount}", topic, validTokens.Count);
            throw;
        }
    }
}
