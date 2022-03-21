using System;
using System.Data;

namespace AMS.Profile
{
    public abstract class Profile : IProfile, IReadOnlyProfile, ICloneable
    {
        private string m_name;
        private bool m_readOnly;

        public event ProfileChangingHandler Changing;

        public event ProfileChangedHandler Changed;

        protected Profile() => m_name = DefaultName;

        protected Profile(string name) => m_name = name;

        protected Profile(Profile profile)
        {
            m_name = profile.m_name;
            m_readOnly = profile.m_readOnly;
            Changing = profile.Changing;
            Changed = profile.Changed;
        }

        public string Name
        {
            get => m_name;
            set
            {
                VerifyNotReadOnly();
                if (m_name == value.Trim() || !RaiseChangeEvent(true, ProfileChangeType.Name, null, null, value))
                    return;
                m_name = value.Trim();
                RaiseChangeEvent(false, ProfileChangeType.Name, null, null, value);
            }
        }

        public bool ReadOnly
        {
            get => m_readOnly;
            set
            {
                VerifyNotReadOnly();
                if (m_readOnly == value || !RaiseChangeEvent(true, ProfileChangeType.ReadOnly, null, null, value))
                    return;
                m_readOnly = value;
                RaiseChangeEvent(false, ProfileChangeType.ReadOnly, null, null, value);
            }
        }

        public abstract string DefaultName { get; }

        public abstract object Clone();

        public abstract void SetValue(string section, string entry, object value);

        public abstract object GetValue(string section, string entry);

        public virtual string GetValue(string section, string entry, string defaultValue)
        {
            object obj = GetValue(section, entry);
            return obj == null ? defaultValue : obj.ToString();
        }

        public virtual int GetValue(string section, string entry, int defaultValue)
        {
            object obj = GetValue(section, entry);
            if (obj == null)
                return defaultValue;
            try
            {
                return Convert.ToInt32(obj);
            }
            catch
            {
                return 0;
            }
        }

        public virtual double GetValue(string section, string entry, double defaultValue)
        {
            object obj = GetValue(section, entry);
            if (obj == null)
                return defaultValue;
            try
            {
                return Convert.ToDouble(obj);
            }
            catch
            {
                return 0.0;
            }
        }

        public virtual bool GetValue(string section, string entry, bool defaultValue)
        {
            object obj = GetValue(section, entry);
            if (obj == null)
                return defaultValue;
            try
            {
                return Convert.ToBoolean(obj);
            }
            catch
            {
                return false;
            }
        }

        public virtual bool HasEntry(string section, string entry)
        {
            string[] entryNames = GetEntryNames(section);
            if (entryNames == null)
                return false;
            VerifyAndAdjustEntry(ref entry);
            return Array.IndexOf<string>(entryNames, entry) >= 0;
        }

        public virtual bool HasSection(string section)
        {
            string[] sectionNames = GetSectionNames();
            if (sectionNames == null)
                return false;
            VerifyAndAdjustSection(ref section);
            return Array.IndexOf<string>(sectionNames, section) >= 0;
        }

        public abstract void RemoveEntry(string section, string entry);

        public abstract void RemoveSection(string section);

        public abstract string[] GetEntryNames(string section);

        public abstract string[] GetSectionNames();

        public virtual IReadOnlyProfile CloneReadOnly()
        {
            Profile profile = (Profile)Clone();
            profile.m_readOnly = true;
            return (IReadOnlyProfile)profile;
        }

        public virtual DataSet GetDataSet()
        {
            VerifyName();
            string[] sectionNames = GetSectionNames();
            if (sectionNames == null)
                return (DataSet)null;
            DataSet dataSet = new DataSet(Name);
            foreach (string str1 in sectionNames)
            {
                DataTable dataTable = dataSet.Tables.Add(str1);
                string[] entryNames = GetEntryNames(str1);
                DataColumn[] columns = new DataColumn[entryNames.Length];
                object[] objArray = new object[entryNames.Length];
                int index = 0;
                foreach (string str2 in entryNames)
                {
                    object obj = GetValue(str1, str2);
                    columns[index] = new DataColumn(str2, obj.GetType());
                    objArray[index++] = obj;
                }
                dataTable.Columns.AddRange(columns);
                dataTable.Rows.Add(objArray);
            }
            return dataSet;
        }

        public virtual void SetDataSet(DataSet ds)
        {
            if (ds == null)
                throw new ArgumentNullException(nameof(ds));
            foreach (DataTable table in (InternalDataCollectionBase)ds.Tables)
            {
                string tableName = table.TableName;
                DataRowCollection rows = table.Rows;
                if (rows.Count != 0)
                {
                    foreach (DataColumn column in (InternalDataCollectionBase)table.Columns)
                    {
                        string columnName = column.ColumnName;
                        object obj = rows[0][column];
                        SetValue(tableName, columnName, obj);
                    }
                }
            }
        }

        protected string DefaultNameWithoutExtension
        {
            get
            {
                try
                {
                    string configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                    return configurationFile.Substring(0, configurationFile.LastIndexOf('.'));
                }
                catch
                {
                    return "profile";
                }
            }
        }

        protected virtual void VerifyAndAdjustSection(ref string section)
        {
            if (section == null)
                throw new ArgumentNullException(nameof(section));
            section = section.Trim();
        }

        protected virtual void VerifyAndAdjustEntry(ref string entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));
            entry = entry.Trim();
        }

        protected internal virtual void VerifyName()
        {
            if (m_name == null || m_name == "")
                throw new InvalidOperationException("Operation not allowed because Name property is null or empty.");
        }

        protected internal virtual void VerifyNotReadOnly()
        {
            if (m_readOnly)
                throw new InvalidOperationException("Operation not allowed because ReadOnly property is true.");
        }

        protected bool RaiseChangeEvent(
          bool changing,
          ProfileChangeType changeType,
          string section,
          string entry,
          object value)
        {
            if (changing)
            {
                if (Changing == null)
                    return true;
                ProfileChangingArgs e = new ProfileChangingArgs(changeType, section, entry, value);
                OnChanging(e);
                return !e.Cancel;
            }
            if (Changed != null)
                OnChanged(new ProfileChangedArgs(changeType, section, entry, value));
            return true;
        }

        protected virtual void OnChanging(ProfileChangingArgs e)
        {
            if (Changing == null)
                return;
            foreach (ProfileChangingHandler invocation in Changing.GetInvocationList())
            {
                invocation(this, e);
                if (e.Cancel)
                    break;
            }
        }

        protected virtual void OnChanged(ProfileChangedArgs e)
        {
            if (Changed == null)
                return;
            Changed(this, e);
        }

        public virtual void Test(bool cleanup)
        {
            string str1 = "";
            try
            {
                string section = "Profile Test";
                str1 = "initializing the profile -- cleaning up the '" + section + "' section";
                RemoveSection(section);
                str1 = "getting the sections and their count";
                string[] sectionNames = GetSectionNames();
                int num1 = sectionNames == null ? 0 : sectionNames.Length;
                bool flag1 = num1 > 1;
                str1 = "adding some valid entries to the '" + section + "' section";
                SetValue(section, "Text entry", "123 abc");
                SetValue(section, "Blank entry", "");
                SetValue(section, "Null entry", null);
                SetValue(section, "  Entry with leading and trailing spaces  ", "The spaces should be trimmed from the entry");
                SetValue(section, "Integer entry", 17);
                SetValue(section, "Long entry", 1234567890123456789L);
                SetValue(section, "Double entry", 17.95);
                SetValue(section, "DateTime entry", DateTime.Today);
                SetValue(section, "Boolean entry", flag1);
                str1 = "adding a null entry to the '" + section + "' section";
                try
                {
                    SetValue(section, null, "123 abc");
                    throw new Exception("Passing a null entry was allowed for SetValue");
                }
                catch (ArgumentNullException ex)
                {
                }
                str1 = "retrieving a null section";
                try
                {
                    GetValue(null, nameof(Test));
                    throw new Exception("Passing a null section was allowed for GetValue");
                }
                catch (ArgumentNullException ex)
                {
                }
                str1 = "getting the number of entries and their count";
                int num2 = 8;
                string[] entryNames1 = GetEntryNames(section);
                str1 = "verifying the number of entries is " + num2.ToString();
                if (entryNames1.Length != num2)
                    throw new Exception("Incorrect number of entries found: " + entryNames1.Length.ToString());
                str1 = "checking the values for the entries added";
                string str2 = GetValue(section, "Text entry", "");
                if (str2 != "123 abc")
                    throw new Exception("Incorrect string value found for the Text entry: '" + str2 + "'");
                int num3 = GetValue(section, "Text entry", 321);
                if ((uint)num3 > 0U)
                    throw new Exception("Incorrect integer value found for the Text entry: " + num3.ToString());
                string str3 = GetValue(section, "Blank entry", "invalid");
                if (str3 != "")
                    throw new Exception("Incorrect string value found for the Blank entry: '" + str3 + "'");
                int num4 = GetValue(section, "Blank entry") != null ? GetValue(section, "Blank entry", 321) : throw new Exception("Incorrect null value found for the Blank entry");
                if ((uint)num4 > 0U)
                    throw new Exception("Incorrect integer value found for the Blank entry: " + num4.ToString());
                bool flag2 = GetValue(section, "Blank entry", true);
                if (flag2)
                    throw new Exception("Incorrect bool value found for the Blank entry: " + flag2.ToString());
                string str4 = GetValue(section, "Null entry", "");
                if (str4 != "")
                    throw new Exception("Incorrect string value found for the Null entry: '" + str4 + "'");
                object obj1 = GetValue(section, "Null entry");
                if (obj1 != null)
                    throw new Exception("Incorrect object value found for the Blank entry: '" + obj1?.ToString() + "'");
                string str5 = GetValue(section, "  Entry with leading and trailing spaces  ", "");
                if (str5 != "The spaces should be trimmed from the entry")
                    throw new Exception("Incorrect string value found for the Entry with leading and trailing spaces: '" + str5 + "'");
                int num5 = HasEntry(section, "Entry with leading and trailing spaces") ? GetValue(section, "Integer entry", 0) : throw new Exception("The Entry with leading and trailing spaces (trimmed) was not found");
                if (num5 != 17)
                    throw new Exception("Incorrect integer value found for the Integer entry: " + num5.ToString());
                double num6 = GetValue(section, "Integer entry", 0.0);
                if (num6 != 17.0)
                    throw new Exception("Incorrect double value found for the Integer entry: " + num6.ToString());
                long int64 = Convert.ToInt64(GetValue(section, "Long entry"));
                if (int64 != 1234567890123456789L)
                    throw new Exception("Incorrect long value found for the Long entry: " + int64.ToString());
                string str6 = GetValue(section, "Long entry", "");
                if (str6 != "1234567890123456789")
                    throw new Exception("Incorrect string value found for the Long entry: '" + str6 + "'");
                double num7 = GetValue(section, "Double entry", 0.0);
                if (num7 != 17.95)
                    throw new Exception("Incorrect double value found for the Double entry: " + num7.ToString());
                int num8 = GetValue(section, "Double entry", 321);
                if ((uint)num8 > 0U)
                    throw new Exception("Incorrect integer value found for the Double entry: " + num8.ToString());
                string s = GetValue(section, "DateTime entry", "");
                if (s != DateTime.Today.ToString())
                    throw new Exception("Incorrect string value found for the DateTime entry: '" + s + "'");
                if (DateTime.Parse(s) != DateTime.Today)
                    throw new Exception("The DateTime value is not today's date: '" + s + "'");
                bool flag3 = GetValue(section, "Boolean entry", !flag1);
                if (flag3 != flag1)
                    throw new Exception("Incorrect bool value found for the Boolean entry: " + flag3.ToString());
                string str7 = GetValue(section, "Boolean entry", "");
                if (str7 != flag1.ToString())
                    throw new Exception("Incorrect string value found for the Boolean entry: '" + str7 + "'");
                object obj2 = GetValue(section, "Nonexistent entry");
                if (obj2 != null)
                    throw new Exception("Incorrect value found for the Nonexistent entry: '" + obj2?.ToString() + "'");
                string str8 = GetValue(section, "Nonexistent entry", "Some Default");
                if (str8 != "Some Default")
                    throw new Exception("Incorrect default value found for the Nonexistent entry: '" + str8 + "'");
                str1 = "creating a ReadOnly clone of the object";
                IReadOnlyProfile readOnlyProfile = CloneReadOnly();
                double num9 = readOnlyProfile.HasSection(section) ? readOnlyProfile.GetValue(section, "Double entry", 0.0) : throw new Exception("The section is missing from the cloned read-only profile");
                if (num9 != 17.95)
                    throw new Exception("Incorrect double value in the cloned object: " + num9.ToString());
                str1 = "checking if ReadOnly clone can be hacked to allow writing";
                try
                {
                    ((IProfile)readOnlyProfile).ReadOnly = false;
                    throw new Exception("Changing of the ReadOnly flag was allowed on the cloned read-only profile");
                }
                catch (InvalidOperationException ex)
                {
                }
                try
                {
                    ((IProfile)readOnlyProfile).SetValue(section, "Entry which should not be written", "This should not happen");
                    throw new Exception("SetValue did not throw an InvalidOperationException when writing to the cloned read-only profile");
                }
                catch (InvalidOperationException ex)
                {
                }
                if (!cleanup)
                    return;
                str1 = "deleting the entries just added";
                RemoveEntry(section, "Text entry");
                RemoveEntry(section, "Blank entry");
                RemoveEntry(section, "  Entry with leading and trailing spaces  ");
                RemoveEntry(section, "Integer entry");
                RemoveEntry(section, "Long entry");
                RemoveEntry(section, "Double entry");
                RemoveEntry(section, "DateTime entry");
                RemoveEntry(section, "Boolean entry");
                str1 = "deleting a nonexistent entry";
                RemoveEntry(section, "Null entry");
                str1 = "verifying all entries were deleted";
                string[] entryNames2 = GetEntryNames(section);
                if ((uint)entryNames2.Length > 0U)
                    throw new Exception("Incorrect number of entries still found: " + entryNames2.Length.ToString());
                str1 = "deleting the section";
                RemoveSection(section);
                str1 = "verifying the section was deleted";
                int length = GetSectionNames().Length;
                if (num1 != length)
                    throw new Exception("Incorrect number of sections found after deleting: " + length.ToString());
                if (GetEntryNames(section) != null)
                    throw new Exception("The section was apparently not deleted since GetEntryNames did not return null");
            }
            catch (Exception ex)
            {
                throw new Exception("Test Failed while " + str1, ex);
            }
        }
    }
}
