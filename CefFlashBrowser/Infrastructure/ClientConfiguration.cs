using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace CefFlashBrowser.Infrastructure
{
    [DataContract]
    public sealed class ClientConfiguration
    {
        public const string DefaultGameUrl = "http://43.240.221.176:32400/";

        [DataMember(Name = "gameUrl")]
        public string GameUrl { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }

        public static ClientConfiguration Load()
        {
            try
            {
                using (var stream = File.OpenRead(ClientPaths.ClientConfigurationPath))
                {
                    var serializer = new DataContractJsonSerializer(typeof(ClientConfiguration));
                    var configuration = serializer.ReadObject(stream) as ClientConfiguration;

                    if (configuration != null && IsValidGameUrl(configuration.GameUrl))
                    {
                        return configuration;
                    }
                }

                ClientLogger.Error($"Client configuration contains an invalid gameUrl: {ClientPaths.ClientConfigurationPath}");
            }
            catch (Exception exception)
            {
                ClientLogger.Error($"Failed to load client configuration: {ClientPaths.ClientConfigurationPath}", exception);
            }

            return new ClientConfiguration
            {
                GameUrl = DefaultGameUrl,
                Version = "1.0.0"
            };
        }

        private static bool IsValidGameUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                return false;
            }

            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }
    }
}
