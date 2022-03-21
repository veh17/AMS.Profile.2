using System;
using System.Data;

namespace AMS.Profile
{
    public interface IReadOnlyProfile : ICloneable
    {
        string Name { get; }

        object GetValue(string section, string entry);

        string GetValue(string section, string entry, string defaultValue);

        int GetValue(string section, string entry, int defaultValue);

        double GetValue(string section, string entry, double defaultValue);

        bool GetValue(string section, string entry, bool defaultValue);

        bool HasEntry(string section, string entry);

        bool HasSection(string section);

        string[] GetEntryNames(string section);

        string[] GetSectionNames();

        DataSet GetDataSet();
    }
}
