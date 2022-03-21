using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AMS.Profile
{
	public class Ini : Profile
    {
        public Ini()
        {
        }

        public Ini(string fileName)
          : base(fileName)
        {
        }

        public Ini(Ini ini)
          : base(ini)
        {
        }

        public override string DefaultName => DefaultNameWithoutExtension + ".ini";

        public override object Clone() => new Ini(this);

        [DllImport("kernel32", SetLastError = true)]
        private static extern int WritePrivateProfileString(
          string section,
          string key,
          string value,
          string fileName);

        [DllImport("kernel32", SetLastError = true)]
        private static extern int WritePrivateProfileString(
          string section,
          string key,
          int value,
          string fileName);

        [DllImport("kernel32", SetLastError = true)]
        private static extern int WritePrivateProfileString(
          string section,
          int key,
          string value,
          string fileName);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(
          string section,
          string key,
          string defaultValue,
          StringBuilder result,
          int size,
          string fileName);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(
          string section,
          int key,
          string defaultValue,
          [MarshalAs(UnmanagedType.LPArray)] byte[] result,
          int size,
          string fileName);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(
          int section,
          string key,
          string defaultValue,
          [MarshalAs(UnmanagedType.LPArray)] byte[] result,
          int size,
          string fileName);

        public override void SetValue(string section, string entry, object value)
        {
            if (value == null)
            {
                RemoveEntry(section, entry);
            }
            else
            {
                VerifyNotReadOnly();
                VerifyName();
                VerifyAndAdjustSection(ref section);
                VerifyAndAdjustEntry(ref entry);
                if (!RaiseChangeEvent(true, ProfileChangeType.SetValue, section, entry, value))
                    return;
                if (WritePrivateProfileString(section, entry, value.ToString(), Name) == 0)
                    throw new Win32Exception();
                RaiseChangeEvent(false, ProfileChangeType.SetValue, section, entry, value);
            }
        }

        public override object GetValue(string section, string entry)
        {
            VerifyName();
            VerifyAndAdjustSection(ref section);
            VerifyAndAdjustEntry(ref entry);
            int num = 250;
            StringBuilder result;
            int privateProfileString;
            while (true)
            {
                result = new StringBuilder(num);
                privateProfileString = GetPrivateProfileString(section, entry, "", result, num, Name);
                if (privateProfileString >= num - 1)
                    num *= 2;
                else
                    break;
            }
            return privateProfileString == 0 && !HasEntry(section, entry) ? null : result.ToString();
        }

        public override void RemoveEntry(string section, string entry)
        {
            if (!HasEntry(section, entry))
                return;
            VerifyNotReadOnly();
            VerifyName();
            VerifyAndAdjustSection(ref section);
            VerifyAndAdjustEntry(ref entry);
            if (!RaiseChangeEvent(true, ProfileChangeType.RemoveEntry, section, entry, null))
                return;
            if (WritePrivateProfileString(section, entry, 0, Name) == 0)
                throw new Win32Exception();
            RaiseChangeEvent(false, ProfileChangeType.RemoveEntry, section, entry, null);
        }

        public override void RemoveSection(string section)
        {
            if (!HasSection(section))
                return;
            VerifyNotReadOnly();
            VerifyName();
            VerifyAndAdjustSection(ref section);
            if (!RaiseChangeEvent(true, ProfileChangeType.RemoveSection, section, null, null))
                return;
            if (WritePrivateProfileString(section, 0, "", Name) == 0)
                throw new Win32Exception();
            RaiseChangeEvent(false, ProfileChangeType.RemoveSection, section, null, null);
        }

        public override string[] GetEntryNames(string section)
        {
            if (!HasSection(section))
                return null;
            VerifyAndAdjustSection(ref section);
            int size = 500;
            byte[] numArray;
            int privateProfileString;
            while (true)
            {
                numArray = new byte[size];
                privateProfileString = GetPrivateProfileString(section, 0, "", numArray, size, Name);
                if (privateProfileString >= size - 2)
                    size *= 2;
                else
                    break;
            }
            string str = Encoding.ASCII.GetString(numArray, 0, privateProfileString - (privateProfileString > 0 ? 1 : 0));
            return str == "" ? new string[0] : str.Split(new char[1]);
        }

        public override string[] GetSectionNames()
        {
            if (!File.Exists(Name))
                return null;
            int size = 500;
            byte[] numArray;
            int privateProfileString;
            while (true)
            {
                numArray = new byte[size];
                privateProfileString = GetPrivateProfileString(0, "", "", numArray, size, Name);
                if (privateProfileString >= size - 2)
                    size *= 2;
                else
                    break;
            }
            string str = Encoding.ASCII.GetString(numArray, 0, privateProfileString - (privateProfileString > 0 ? 1 : 0));
            return str == "" ? new string[0] : str.Split(new char[1]);
        }

        public void ChangeSectionName(string Section, string NewSection)
		{
            string iniConfig = string.Empty;

            using (StreamReader reader = new StreamReader(Name))
			{
                iniConfig = reader.ReadToEnd();
                iniConfig = iniConfig.Replace($"[{Section}]", $"[{NewSection}]");

                reader.Close();
			}

            File.Delete(Name);
            File.Create(Name).Close();

            using (StreamWriter writer = new StreamWriter(Name))
			{
                writer.Write(iniConfig);
			}
		}

        public void ChangeEntryName(string Section, string Entry, string NewEntry)
		{
            foreach (string entry in GetEntryNames(Section))
			{
                bool newEntry = entry == Entry;
                string value = GetValue(Section, entry).ToString();

                RemoveEntry(Section, entry);
                SetValue(Section, newEntry ? NewEntry : entry, value);
			}
        }
    }
}
