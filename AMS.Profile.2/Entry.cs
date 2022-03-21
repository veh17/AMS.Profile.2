namespace AMS.Profile
{
	internal class Entry
	{
		public string Name { get; private set; } = string.Empty;
		public string Value { get; private set; } = string.Empty;

		public Entry(string Name, string Value)
		{
			this.Name = Name;
			this.Value = Value;
		}
	}
}
