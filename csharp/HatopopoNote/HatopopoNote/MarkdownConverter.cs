﻿using System.Net.NetworkInformation;

namespace HatopopoNote
{
	internal class MarkdownConverter
	{
		private static readonly string MarkdownDirectoryName = "md";

		public void Execute()
		{
			if (!TryGetTargetDirectory(out var targetDirectory)) return;
			Console.WriteLine($"target : {targetDirectory!.FullName}");

			// .mdのファイルを取得
			var files = new List<FileInfo>();
			foreach ( var file in targetDirectory.GetFiles("*.md",SearchOption.AllDirectories))
			{
				Console.WriteLine($"{file.FullName}");
			}

			// publicフォルダにmdを格納
			var publicDirectory = Path.Combine(targetDirectory.Parent.FullName, "public");
			foreach ( var file in files)
			{
				var mdFilePah = file.FullName.Replace(targetDirectory.Parent.FullName, "");
				File.Copy(file.FullName, Path.Combine(publicDirectory, mdFilePah));


			}
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
	}
}
