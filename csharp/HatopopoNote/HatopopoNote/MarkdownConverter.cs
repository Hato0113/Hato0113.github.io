using System.Net.NetworkInformation;
using System.Text;

namespace HatopopoNote
{
	internal class MarkdownConverter
	{
		
		private static readonly string MarkdownDirectoryName = "md";
		private static readonly string TargetMarkdownExtension = "*.md";

		/// <summary>
		/// 実行エントリポイント
		/// </summary>
		public async Task<bool> Execute()
		{
			if (!TryGetTargetDirectory(out var targetDirectory)) return false;
			Console.WriteLine($"target : {targetDirectory!.FullName}");
			if (targetDirectory.Parent == null) return false;

			// .mdのファイルを取得
			var files = new List<FileInfo>();
			foreach (var file in targetDirectory.GetFiles(TargetMarkdownExtension, SearchOption.AllDirectories))
			{
				Console.WriteLine($"{file.FullName}");
				files.Add(file);
			}

			// publicフォルダにhtml形式に変換したmdを格納
			await Convert2HtmlAsync(targetDirectory, files);
			return true;
		}

		/// <summary>
		/// マークダウンが格納されているディレクトリの探索
		/// </summary>
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

		/// <summary>
		/// HTMLに変換する機能の呼び出し
		/// </summary>
		private async Task Convert2HtmlAsync(DirectoryInfo targetDirectory, List<FileInfo> files)
		{
			var publicDirectoryPath = Path.Combine(targetDirectory.Parent!.FullName, "public");

			foreach (var file in files)
			{
				await CreateAndWriteToFileAsync(publicDirectoryPath, targetDirectory, file);
			}
		}

		/// <summary>
		/// htmlファイルの作成と書き込み
		/// </summary>
		private async Task CreateAndWriteToFileAsync(string publicDirectoryPath, DirectoryInfo targetDirectory, FileInfo file)
		{
			// .htmlに変換
			var mdFilePath = GetRelativePath(targetDirectory, file);
			var htmlFileName = mdFilePath.Replace(".md",".html");
			var htmlFile = new FileInfo(Path.Combine(publicDirectoryPath, htmlFileName));

			await CreateDirectoryIfNotExistsAsync(htmlFile.Directory!);

			// 一行ずつ読み込んで変換
			var lines = await ReadFileLinesAsync(file);
			var parsedLines = ParseMarkdown(lines);

			await WriteToFileAsync(htmlFile, parsedLines);
		}

		private string GetRelativePath(DirectoryInfo targetDirectory, FileInfo file)
		{
			return file.FullName.Replace(targetDirectory.Parent!.FullName + Path.DirectorySeparatorChar, "");
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

			using var reader = new StreamReader(file.FullName);
			while (await reader.ReadLineAsync() is { } line)
			{
				lines.Add(line);
			}

			return lines;
		}

		private List<string> ParseMarkdown(List<string> lines)
		{
			var parser = new Parser();
			var builder = new StringBuilder();
			foreach (var line in lines)
			{
				builder.AppendLine(line);
			}

			var htmlBody = Markdig.Markdown.ToHtml(builder.ToString());
			var list = new List<string>();
			list.Add(htmlBody);
			return list;
		}

		// ファイルへの書き込み
		private async Task WriteToFileAsync(FileInfo file, List<string> lines)
		{
			await using var writer = new StreamWriter(file.FullName);
			foreach (var line in lines)
			{
				await writer.WriteLineAsync(line);
			}
		}
	}
}
