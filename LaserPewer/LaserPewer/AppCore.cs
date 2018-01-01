using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LaserPewer
{
    public class AppCore
    {
        private static AppCore singleton;
        public static AppCore Instance
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new AppCore();
                }
                return singleton;
            }
        }

        public static IReadOnlyList<MachineProfile> Profiles
        {
            get
            {
                return Instance.profiles.AsReadOnly();
            }
        }

        public static GrblMachine Machine
        {
            get
            {
                return Instance.machine;
            }
        }

        private readonly List<MachineProfile> profiles;
        private readonly GrblMachine machine;

        private AppCore()
        {
            profiles = new List<MachineProfile>();
            machine = new GrblMachine();
        }

        public void Initialize()
        {
            LoadSettings();

            if (profiles.Count == 0)
            {
                profiles.Add(new MachineProfile() { FriendlyName = "Default Machine", TableWidth = 300, TableHeight = 200, MaxFeedRate = 10000 });
            }
        }

        public void LoadSettings()
        {
            string configPath = getConfigPath();

            if (!File.Exists(configPath)) return;

            using (StreamReader reader = new StreamReader(getConfigPath()))
            {
                for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    string[] tokens = line.Split(' ');
                    if (tokens.Length == 0) continue;
                    if (tokens[0] == "PROFILE")
                    {
                        MachineProfile profile = new MachineProfile();
                        profile.FriendlyName = decodeString(tokens[1]);
                        profile.TableWidth = decodeDouble(tokens[2]);
                        profile.TableHeight = decodeDouble(tokens[3]);
                        profile.MaxFeedRate = decodeDouble(tokens[4]);
                        profiles.Add(profile);
                    }
                }
            }
        }

        public void SaveSettings()
        {
            using (StreamWriter writer = new StreamWriter(getConfigPath()))
            {
                foreach (MachineProfile profile in profiles)
                {
                    writer.Write("PROFILE");
                    writer.Write(' ');
                    writer.Write(encode(profile.FriendlyName));
                    writer.Write(' ');
                    writer.Write(encode(profile.TableWidth));
                    writer.Write(' ');
                    writer.Write(encode(profile.TableHeight));
                    writer.Write(' ');
                    writer.Write(encode(profile.MaxFeedRate));
                    writer.WriteLine();
                }
            }
        }

        public int ProfileIndex(MachineProfile profile)
        {
            return profiles.IndexOf(profile);
        }

        public void AddProfile(MachineProfile profile)
        {
            profiles.Add(profile);
        }

        public bool TryDeleteProfile(MachineProfile profile)
        {
            if (profiles.Count == 1) return false;
            return profiles.Remove(profile);
        }

        private static string getConfigPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".lspewer");
        }

        private static string encode(double d)
        {
            return Convert.ToBase64String(BitConverter.GetBytes(d));
        }

        private static double decodeDouble(string s)
        {
            return BitConverter.ToDouble(Convert.FromBase64String(s), 0);
        }

        private static string encode(string s)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
        }

        private static string decodeString(string s)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(s));
        }
    }
}
