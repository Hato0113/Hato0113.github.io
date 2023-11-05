namespace HatopopoNote
{
	internal class MarkdownConverter
	{
		private static readonly string MarkdownDirectoryPath = "../../md";

		public void Execute()
		{
			Console.WriteLine(System.Environment.CurrentDirectory);

			return;

			// ファイル収集
			var directoryInfo = new DirectoryInfo(MarkdownDirectoryPath);

			foreach (var file in directoryInfo.GetFiles())
			{
				Console.WriteLine(file.FullName);
			}
		}
	}
}
