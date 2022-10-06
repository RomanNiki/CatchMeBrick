using UnityEngine;

namespace Networking
{
    public static class ClientPrefs
    {
        private const string MasterVolumeKey = "MasterVolume";
        private const string MusicVolumeKey = "MusicVolume";
        private const string ClientGuidKey = "client_guid";
        private const string AvailableProfilesKey = "AvailableProfiles";

        private const float DefaultMasterVolume = 0.5f;
        private const float DefaultMusicVolume = 0.8f;

        public static float GetMasterVolume()
        {
            return PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume);
        }

        public static void SetMasterVolume(float volume)
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, volume);
        }

        public static float GetMusicVolume()
        {
            return PlayerPrefs.GetFloat(MusicVolumeKey, DefaultMusicVolume);
        }

        public static void SetMusicVolume(float volume)
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, volume);
        }

        /// <summary>
        /// Either loads a Guid string from Unity preferences, or creates one and checkpoints it, then returns it.
        /// </summary>
        /// <returns>The Guid that uniquely identifies this client install, in string form. </returns>
        public static string GetGuid()
        {
            if (PlayerPrefs.HasKey(ClientGuidKey))
            {
                return PlayerPrefs.GetString(ClientGuidKey);
            }

            var guid = System.Guid.NewGuid();
            var guidString = guid.ToString();

            PlayerPrefs.SetString(ClientGuidKey, guidString);
            return guidString;
        }

        public static string GetAvailableProfiles()
        {
            return PlayerPrefs.GetString(AvailableProfilesKey, "");
        }

        public static void SetAvailableProfiles(string availableProfiles)
        {
            PlayerPrefs.SetString(AvailableProfilesKey, availableProfiles);
        }

    }
}