using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace AMS.Profile
{
    public class Registry : AMS.Profile.Profile
    {
        private RegistryKey m_rootKey = Microsoft.Win32.Registry.CurrentUser;

        public Registry()
        {
        }

        public Registry(RegistryKey rootKey, string subKeyName)
          : base("")
        {
            if (rootKey != null)
                m_rootKey = rootKey;
            if (subKeyName == null)
                return;
            Name = subKeyName;
        }

        public Registry(Registry reg)
          : base((AMS.Profile.Profile)reg)
          => m_rootKey = reg.m_rootKey;

        public override string DefaultName
        {
            get
            {
                if (Application.CompanyName == "" || Application.ProductName == "")
                    throw new InvalidOperationException("Application.CompanyName and/or Application.ProductName are empty and they're needed for the DefaultName.");
                return "Software\\" + Application.CompanyName + "\\" + Application.ProductName;
            }
        }

        public override object Clone() => new Registry(this);

        public RegistryKey RootKey
        {
            get => m_rootKey;
            set
            {
                VerifyNotReadOnly();
                if (m_rootKey == value || !RaiseChangeEvent(true, ProfileChangeType.Other, null, nameof(RootKey), value))
                    return;
                m_rootKey = value;
                RaiseChangeEvent(false, ProfileChangeType.Other, null, nameof(RootKey), value);
            }
        }

        protected RegistryKey GetSubKey(string section, bool create, bool writable)
        {
            VerifyName();
            string str = Name + "\\" + section;
            return create ? m_rootKey.CreateSubKey(str) : m_rootKey.OpenSubKey(str, writable);
        }

        public override void SetValue(string section, string entry, object value)
        {
            if (value == null)
            {
                RemoveEntry(section, entry);
            }
            else
            {
                VerifyNotReadOnly();
                VerifyAndAdjustSection(ref section);
                VerifyAndAdjustEntry(ref entry);
                if (!RaiseChangeEvent(true, ProfileChangeType.SetValue, section, entry, value))
                    return;
                using (RegistryKey subKey = GetSubKey(section, true, true))
                    subKey.SetValue(entry, value);
                RaiseChangeEvent(false, ProfileChangeType.SetValue, section, entry, value);
            }
        }

        public override object GetValue(string section, string entry)
        {
            VerifyAndAdjustSection(ref section);
            VerifyAndAdjustEntry(ref entry);
            using (RegistryKey subKey = GetSubKey(section, false, false))
                return subKey == null ? null : subKey.GetValue(entry);
        }

        public override void RemoveEntry(string section, string entry)
        {
            VerifyNotReadOnly();
            VerifyAndAdjustSection(ref section);
            VerifyAndAdjustEntry(ref entry);
            using (RegistryKey subKey = GetSubKey(section, false, true))
            {
                if (subKey == null || subKey.GetValue(entry) == null || !RaiseChangeEvent(true, ProfileChangeType.RemoveEntry, section, entry, null))
                    return;
                subKey.DeleteValue(entry, false);
                RaiseChangeEvent(false, ProfileChangeType.RemoveEntry, section, entry, null);
            }
        }

        public override void RemoveSection(string section)
        {
            VerifyNotReadOnly();
            VerifyName();
            VerifyAndAdjustSection(ref section);
            using (RegistryKey registryKey = m_rootKey.OpenSubKey(Name, true))
            {
                if (registryKey == null || !HasSection(section) || !RaiseChangeEvent(true, ProfileChangeType.RemoveSection, section, null, null))
                    return;
                registryKey.DeleteSubKeyTree(section);
                RaiseChangeEvent(false, ProfileChangeType.RemoveSection, section, null, null);
            }
        }

        public override string[] GetEntryNames(string section)
        {
            VerifyAndAdjustSection(ref section);
            using (RegistryKey subKey = GetSubKey(section, false, false))
                return subKey?.GetValueNames();
        }

        public override string[] GetSectionNames()
        {
            VerifyName();
            using (RegistryKey registryKey = m_rootKey.OpenSubKey(Name))
                return registryKey?.GetSubKeyNames();
        }
    }
}
