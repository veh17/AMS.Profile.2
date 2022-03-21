namespace AMS.Profile
{
    public class ProfileChangingArgs : ProfileChangedArgs
    {
        private bool m_cancel;

        public ProfileChangingArgs(
          ProfileChangeType changeType,
          string section,
          string entry,
          object value)
          : base(changeType, section, entry, value)
        {
        }

        public bool Cancel
        {
            get => m_cancel;
            set => m_cancel = value;
        }
    }
}
