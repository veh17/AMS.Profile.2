using System;
using System.IO;
using System.Xml;

namespace AMS.Profile
{
    public class XmlBuffer : IDisposable
    {
        private XmlBased m_profile;
        private XmlDocument m_doc;
        private FileStream m_file;
        internal bool m_needsFlushing;

        internal XmlBuffer(XmlBased profile, bool lockFile)
        {
            m_profile = profile;
            if (!lockFile)
                return;
            m_profile.VerifyName();
            if (File.Exists(m_profile.Name))
                m_file = new FileStream(m_profile.Name, FileMode.Open, m_profile.ReadOnly ? FileAccess.Read : FileAccess.ReadWrite, FileShare.Read);
        }

        internal void Load(XmlTextWriter writer)
        {
            writer.Flush();
            writer.BaseStream.Position = 0L;
            m_doc.Load(writer.BaseStream);
            m_needsFlushing = true;
        }

        internal XmlDocument XmlDocument
        {
            get
            {
                if (m_doc == null)
                {
                    m_doc = new XmlDocument();
                    if (m_file != null)
                    {
                        m_file.Position = 0L;
                        m_doc.Load(m_file);
                    }
                    else
                    {
                        m_profile.VerifyName();
                        if (File.Exists(m_profile.Name))
                            m_doc.Load(m_profile.Name);
                    }
                }
                return m_doc;
            }
        }

        internal bool IsEmpty => XmlDocument.InnerXml == string.Empty;

        public bool NeedsFlushing => m_needsFlushing;

        public bool Locked => m_file != null;

        public void Flush()
        {
            if (m_profile == null)
                throw new InvalidOperationException("Cannot flush an XmlBuffer object that has been closed.");
            if (m_doc == null)
                return;
            if (m_file == null)
            {
                m_doc.Save(m_profile.Name);
            }
            else
            {
                m_file.SetLength(0L);
                m_doc.Save(m_file);
            }
            m_needsFlushing = false;
        }

        public void Reset()
        {
            if (m_profile == null)
                throw new InvalidOperationException("Cannot reset an XmlBuffer object that has been closed.");
            m_doc = null;
            m_needsFlushing = false;
        }

        public void Close()
        {
            if (m_profile == null)
                return;
            if (m_needsFlushing)
                Flush();
            m_doc = null;
            if (m_file != null)
            {
                m_file.Close();
                m_file = null;
            }
            if (m_profile != null)
                m_profile.m_buffer = null;
            m_profile = null;
        }

        public void Dispose() => Close();
    }
}
