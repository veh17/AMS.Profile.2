using System;
using System.Data;

namespace AMS.Profile
{
    public interface IProfile : IReadOnlyProfile, ICloneable
    {
        new string Name { get; set; }

        string DefaultName { get; }

        bool ReadOnly { get; set; }

        void SetValue(string section, string entry, object value);

        void RemoveEntry(string section, string entry);

        void RemoveSection(string section);

        void SetDataSet(DataSet ds);

        IReadOnlyProfile CloneReadOnly();

        event ProfileChangingHandler Changing;

        event ProfileChangedHandler Changed;
    }
}
