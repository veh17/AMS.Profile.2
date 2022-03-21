using System.Collections.Generic;

namespace AMS.Profile
{
	internal class Section
	{
		public string Name { get; private set; } = string.Empty;
		public List<Entry> Entries { get; private set; } = null;

		public Section(string Name, List<Entry> Entries)
		{
			this.Name = Name;
			this.Entries = Entries;
		}
	}
}
