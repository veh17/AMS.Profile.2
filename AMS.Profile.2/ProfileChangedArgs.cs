using System;

namespace AMS.Profile
{
    public class ProfileChangedArgs : EventArgs
    {
        private readonly ProfileChangeType m_changeType;
        private readonly string m_section;
        private readonly string m_entry;
        private readonly object m_value;

        public ProfileChangedArgs(
          ProfileChangeType changeType,
          string section,
          string entry,
          object value)
        {
            m_changeType = changeType;
            m_section = section;
            m_entry = entry;
            m_value = value;
        }

        public ProfileChangeType ChangeType => m_changeType;

        public string Section => m_section;

        public string Entry => m_entry;

        public object Value => m_value;
    }
}
