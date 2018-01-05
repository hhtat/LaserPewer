using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LaserPewer.Model
{
    public class PersistentSettings
    {
        public void Load()
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
                        AppCore.MachineList.AddProfile(new MachineList.Profile(
                            decodeString(tokens[1]),
                            new Size(decodeDouble(tokens[2]), decodeDouble(tokens[3])),
                            decodeDouble(tokens[4])));
                    }
                }
            }
        }

        public void Save()
        {
            using (StreamWriter writer = new StreamWriter(getConfigPath()))
            {
                foreach (MachineList.Profile profile in AppCore.MachineList.Profiles)
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
