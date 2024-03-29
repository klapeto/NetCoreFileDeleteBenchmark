﻿// Copyright (C) 2019 klapeto
// 
// This file is part of NetCoreFileDeleteBenchmark.
// 
// NetCoreFileDeleteBenchmark is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// NetCoreFileDeleteBenchmark is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NetCoreFileDeleteBenchmark.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace NetCoreFileDeleteBenchmark
{
	internal class Program
	{
		[Option(Template = "-n <number>", Description = "The number of files to use (Default = 100)")]
		private int NFiles { get; set; } = 100;

		[Option(Template = "-a", Description = "Use multiple threads for concurrent deletes (Default = false)")]
		private bool MultipleThreads { get; set; }

		[Option(Template = "-s <size>", Description = "The file size to use in bytes (Default = 1Kib)")]
		private int FileSize { get; set; } = 1024;


		[Option(Template = "-t", Description = "Do not use Task for async operations (Default = false)")]
		private bool NoUseTask { get; set; }

		[Option(Template = "-w", Description = "Use async/await")]
		private bool AsyncMethods { get; set; }

		private readonly List<DeleteThread> _threads = new List<DeleteThread>();
		private readonly List<string> _files = new List<string>();
		private string _tempDirectory;
		private readonly Stopwatch _stopwatch = new Stopwatch();

		private void OnExecute()
		{
			if (NFiles < 1)
			{
				Console.Error.WriteLine("The number of files should be at least 1");
				return;
			}

			if (FileSize < 1)
			{
				Console.Error.WriteLine("The file size should be at least 1 byte");
				return;
			}

			CreateFiles();

			if (MultipleThreads)
			{
				Console.WriteLine("Using multiple threads for deleting the files");
				Console.WriteLine("Deleting files...");
				PrepareThreads();
				_stopwatch.Start();
				AsyncExecution();
				_stopwatch.Stop();
			}
			else
			{
				Console.WriteLine("Using single thread for deleting the files");
				Console.WriteLine("Deleting files...");
				_stopwatch.Start();
				if (AsyncMethods)
					SingleThreadExecutionAsync().GetAwaiter().GetResult();
				else
					SingleThreadExecution();
				_stopwatch.Stop();
			}

			Console.WriteLine($"Deleted {_files.Count} files in {_stopwatch.ElapsedMilliseconds:F2} ms");
			Cleanup();
		}

		private void PrepareThreads()
		{
			var threadFactory = NoUseTask ?
							(Func<List<string>, DeleteThread>)(paths => new NativeThread(paths)) :
							paths => new TaskThread(paths);


			var cpus = Environment.ProcessorCount;

			var filesPerCpu = _files.Count / cpus;
			for (var i = 0; i < cpus; i++)
			{
				_threads.Add(threadFactory(_files.Skip(i * filesPerCpu).Take(filesPerCpu).ToList()));
			}
		}

		private void Cleanup()
		{
			try
			{
				Directory.Delete(_tempDirectory, recursive: true);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Failed to delete directory: '{_tempDirectory}' {Environment.NewLine}{e}");
			}
		}

		private void AsyncExecution()
		{
			foreach (var thread in _threads)
			{
				thread.Start();
			}

			foreach (var thread in _threads)
			{
				thread.WaitToComplete();
			}
		}

		private void CreateFiles()
		{
			_tempDirectory = Path.Join(Path.GetTempPath(), "NetCoreDeleteBenchmark");
			if (!Directory.Exists(_tempDirectory)) Directory.CreateDirectory(_tempDirectory);
			var bytes = new byte[FileSize];
			Console.WriteLine($"Creating {NFiles} temporary files of size: {FileSize} bytes to : '{_tempDirectory}'");

			var cpus = Environment.ProcessorCount;
			var threads = new List<Task>();

			for (var i = 0; i < NFiles; i++)
			{
				var filePath = Path.Join(_tempDirectory, $"{i}");
				_files.Add(filePath);
			}

			var filesPerCpu = _files.Count / cpus;
			_stopwatch.Start();

			for (var i = 0; i < cpus; i++)
			{
				threads.Add(CreateFilesAsync(_files.Skip(i * filesPerCpu).Take(filesPerCpu).ToList(), bytes));
			}

			Task.WaitAll(threads.ToArray());
			_stopwatch.Stop();
			Console.WriteLine($"Written {_files.Count} files in {_stopwatch.ElapsedMilliseconds:F2} ms");
			_stopwatch.Reset();
		}

		private static Task CreateFilesAsync(IList<string> files, byte[] data)
		{
			return Task.Factory.StartNew(() =>
			{
				foreach (var file in files)
				{
					try
					{
						using (var fs = new FileStream(file, FileMode.Create))
						{
							fs.Write(data, 0, data.Length);
							fs.Flush();
						}
					}
					catch (Exception e)
					{
						Console.Error.WriteLine($"Failed to write file: {Environment.NewLine}{e}");
					}
				}
			}, TaskCreationOptions.LongRunning);
		}

		private void SingleThreadExecution()
		{
			foreach (var item in _files)
			{
				try
				{
					File.Delete(item);
				}
				catch (Exception e)
				{
					Console.Error.WriteLine($"Failed to delete file: {Environment.NewLine}{e}");
				}
			}
		}

		private async Task SingleThreadExecutionAsync()
		{
			foreach (var item in _files)
			{
				try
				{
					await Task.Run(() => File.Delete(item));
				}
				catch (Exception e)
				{
					Console.Error.WriteLine($"Failed to delete file: {Environment.NewLine}{e}");
				}
			}
		}

		private static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);
	}
}
