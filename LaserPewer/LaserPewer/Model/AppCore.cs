using System;
using System.IO;
using System.Text;
using System.Windows;

namespace LaserPewer.Model
{
    public class AppCore
    {
        private static AppCore singleton;
        public static AppCore Instance
        {
            get
            {
                if (singleton == null) singleton = new AppCore();
                return singleton;
            }
        }

        public static MachineProfileManager MachineProfiles { get { return Instance.machineProfileManager; } }
        public static GrblMachine Machine { get { return Instance.machine; } }

        private readonly MachineProfileManager machineProfileManager;
        private readonly GrblMachine machine;

        private AppCore()
        {
            machineProfileManager = new MachineProfileManager();
            machine = new GrblMachine();
        }

        public void Initialize()
        {
            LoadSettings();

            if (machineProfileManager.Profiles.Count == 0)
            {
                machineProfileManager.AddProfile(
                    new MachineProfileManager.Profile("Default Machine", new Size(300.0, 200.0), 10000.0));
            }

            machineProfileManager.Active = machineProfileManager.Profiles[0];
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
                        machineProfileManager.AddProfile(new MachineProfileManager.Profile(
                            decodeString(tokens[1]),
                            new Size(decodeDouble(tokens[2]), decodeDouble(tokens[3])),
                            decodeDouble(tokens[4])));
                    }
                }
            }
        }

        public void SaveSettings()
        {
            using (StreamWriter writer = new StreamWriter(getConfigPath()))
            {
                foreach (MachineProfileManager.Profile profile in machineProfileManager.Profiles)
                {
                    writer.Write("PROFILE");
                    writer.Write(' ');
                    writer.Write(encode(profile.FriendlyName));
                    writer.Write(' ');
                    writer.Write(encode(profile.TableSize.Width));
                    writer.Write(' ');
                    writer.Write(encode(profile.TableSize.Height));
                    writer.Write(' ');
                    writer.Write(encode(profile.MaxFeedRate));
                    writer.WriteLine();
                }
            }
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
