using LaserPewer.Utilities;
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

                    if (tokens[0] == "MACHINE")
                    {
                        AppCore.MachineList.CreateProfile(
                            decodeGuid(tokens[1]),
                            decodeString(tokens[2]),
                            new Size(decodeDouble(tokens[3]), decodeDouble(tokens[4])),
                            (Corner)decodeUShort(tokens[5]),
                            decodeDouble(tokens[6]));
                    }

                    if (tokens[0] == "LAST_MACHINE")
                    {
                        Guid uniqueId = decodeGuid(tokens[1]);
                        MachineList.IProfile lastActive = AppCore.MachineList.Profiles.First(profile => profile.UniqueId == uniqueId);
                        AppCore.MachineList.Active = lastActive;
                    }

                    if (tokens[0] == "LAST_VECTOR")
                    {
                        AppCore.Generator.VectorPower = decodeDouble(tokens[1]);
                        AppCore.Generator.VectorSpeed = decodeDouble(tokens[2]);
                    }
                }
            }
        }

        public void Save()
        {
            using (StreamWriter writer = new StreamWriter(getConfigPath()))
            {
                foreach (MachineList.IProfile profile in AppCore.MachineList.Profiles)
                {
                    writer.Write("MACHINE");
                    writer.Write(' ');
                    writer.Write(encode(profile.UniqueId));
                    writer.Write(' ');
                    writer.Write(encode(profile.FriendlyName));
                    writer.Write(' ');
                    writer.Write(encode(profile.TableSize.Width));
                    writer.Write(' ');
                    writer.Write(encode(profile.TableSize.Height));
                    writer.Write(' ');
                    writer.Write(encode((ushort)profile.Origin));
                    writer.Write(' ');
                    writer.Write(encode(profile.MaxFeedRate));
                    writer.WriteLine();
                }

                writer.Write("LAST_MACHINE");
                writer.Write(' ');
                writer.Write(encode(AppCore.MachineList.Active.UniqueId));
                writer.WriteLine();

                writer.Write("LAST_VECTOR");
                writer.Write(' ');
                writer.Write(encode(AppCore.Generator.VectorPower));
                writer.Write(' ');
                writer.Write(encode(AppCore.Generator.VectorSpeed));
                writer.WriteLine();
            }
        }

        private static string getConfigPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".lspewer");
        }

        private static string encode(ushort d)
        {
            return Convert.ToBase64String(BitConverter.GetBytes(d));
        }

        private static ushort decodeUShort(string s)
        {
            return BitConverter.ToUInt16(Convert.FromBase64String(s), 0);
        }

        private static string encode(double d)
        {
            return Convert.ToBase64String(BitConverter.GetBytes(d));
        }

        private static double decodeDouble(string s)
        {
            return BitConverter.ToDouble(Convert.FromBase64String(s), 0);
        }

        private static string encode(Guid g)
        {
            return Convert.ToBase64String(g.ToByteArray());
        }

        private static Guid decodeGuid(string s)
        {
            return new Guid(Convert.FromBase64String(s));
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
