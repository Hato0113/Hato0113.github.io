using System.Net.NetworkInformation;

namespace HatopopoNote
{
	internal class MarkdownConverter
	{
		private static readonly string MarkdownDirectoryName = "md";

		public async Task Execute()
		{
			if (!TryGetTargetDirectory(out var targetDirectory)) return;
			Console.WriteLine($"target : {targetDirectory!.FullName}");

			// .mdのファイルを取得
			var files = new List<FileInfo>();
			foreach (var file in targetDirectory.GetFiles("*.md", SearchOption.AllDirectories))
			{
				Console.WriteLine($"{file.FullName}");
				files.Add(file);
			}

			// publicフォルダにhtml形式に変換したmdを格納
			await Contert2HtmlAsync(targetDirectory, files);
		}

		private bool TryGetTargetDirectory(out DirectoryInfo? targetDirectory)
		{
			// mdが見つかるまでさかのぼる
			var currentDirectory = new DirectoryInfo(System.Environment.CurrentDirectory);
			targetDirectory = null;
			while (targetDirectory == null && currentDirectory != null)
			{
				var directory = currentDirectory.GetDirectories(MarkdownDirectoryName);
				if (directory.Length != 0)
				{
					targetDirectory = directory[0];
					return true;
				}

				currentDirectory = currentDirectory.Parent;
			}

			return false;
		}

		public async Task Contert2HtmlAsync(DirectoryInfo targetDirectory, List<FileInfo> files)
		{
			var publicDirectoryPath = Path.Combine(targetDirectory!.Parent!.FullName, "public");

			foreach (var file in files)
			{
				await CreateAndWriteToFileAsync(publicDirectoryPath, targetDirectory, file);
			}
		}

		private async Task CreateAndWriteToFileAsync(string publicDirectoryPath, DirectoryInfo targetDirectory, FileInfo file)
		{
			var mdFilePath = GetRelativePath(targetDirectory, file);
			var mdFile = new FileInfo(Path.Combine(publicDirectoryPath, mdFilePath));

			await CreateDirectoryIfNotExistsAsync(mdFile.Directory!);

			var lines = await ReadFileLinesAsync(file);
			var parsedLines = ParseMarkdown(lines);

			await WriteToFileAsync(mdFile, parsedLines);
		}

		private string GetRelativePath(DirectoryInfo targetDirectory, FileInfo file)
		{
			return file.FullName.Replace(targetDirectory.Parent.FullName + Path.DirectorySeparatorChar, "");
		}

		private async Task CreateDirectoryIfNotExistsAsync(DirectoryInfo directory)
		{
			if (!directory.Exists)
			{
				await Task.Run(() => Directory.CreateDirectory(directory.FullName));
			}
		}

		private async Task<List<string>> ReadFileLinesAsync(FileInfo file)
		{
			var lines = new List<string>();

			using (var reader = new StreamReader(file.FullName))
			{
				string line;
				while ((line = await reader.ReadLineAsync()) != null)
				{
					lines.Add(line);
				}
			}

			return lines;
		}

		private List<string> ParseMarkdown(List<string> lines)
		{
			var parser = new Parser();
			return parser.ParseMarkdown(lines.ToArray()).ToList();
		}

		private async Task WriteToFileAsync(FileInfo file, List<string> lines)
		{
			using (var writer = new StreamWriter(file.FullName))
			{
				foreach (var line in lines)
				{
					await writer.WriteLineAsync(line);
				}
			}
		}
	}
}
