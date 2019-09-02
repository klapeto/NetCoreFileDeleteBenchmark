// Copyright (C) 2019 klapeto
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
using McMaster.Extensions.CommandLineUtils;

namespace NetCoreFileDeleteBenchmark
{
	class Program
	{
		[Option(Template = "-n <number>", Description = "The number of files to use (Default = 100)")]
		private int _nFiles { get; set; } = 100;

		[Option(Template = "-a", Description = "Use async operations (Default = false)")]
		private bool _async { get; set; }

		[Option(Template = "-s <size>", Description = "The file size to use in bytes (Default = 1Kib)")]
		private int _fileSize { get; set; } = 1024;


		[Option(Template = "-t", Description = "Do not use Task for async operations (Default = false)")]
		private bool _noUseTask { get; set; }

		private List<DeleteThread> _threads = new List<DeleteThread>();
		private List<string> _files = new List<string>();
		private string _tempDirectory;
		private Stopwatch _stopwatch = new Stopwatch();

		private void OnExecute()
		{
			if (_nFiles < 1)
			{
				Console.Error.WriteLine("The number of files should be at least 1");
				return;
			}

			if (_fileSize < 1)
			{
				Console.Error.WriteLine("The file size should be at least 1 byte");
				return;
			}

			CreateFiles();

			if (_async)
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
				SingleThreadExecution();
				_stopwatch.Stop();
			}

			Console.WriteLine($"Deleted {_files.Count} files in {_stopwatch.ElapsedMilliseconds:F2} ms");
			Cleanup();
		}

		private void PrepareThreads()
		{
			var threadFactory = _noUseTask ?
							(Func<List<string>, DeleteThread>)((paths) => new NativeThread(paths)) :
							(Func<List<string>, DeleteThread>)((paths) => new TaskThread(paths));


			var cpus = Environment.ProcessorCount;

			var filesPerCpu = _files.Count / cpus;
			for (int i = 0; i < cpus; i++)
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
			catch (System.Exception e)
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
			var bytes = new byte[_fileSize];
			Console.WriteLine($"Creating {_nFiles} temporary files of size: {_fileSize} bytes...");
			_stopwatch.Start();
			for (int i = 0; i < _nFiles; i++)
			{
				var filePath = Path.Join(_tempDirectory, $"{i}");

				try
				{
					using (var fs = new FileStream(filePath, FileMode.Create))
					{
						fs.Write(bytes, 0, bytes.Length);
						fs.Flush();
					}
					//System.IO.File.WriteAllBytes(filePath, bytes);
					_files.Add(filePath);
				}
				catch (System.Exception e)
				{
					Console.Error.WriteLine($"Failed to write file: {Environment.NewLine}{e}");
				}
			}
			_stopwatch.Stop();
			Console.WriteLine($"Writen {_files.Count} files in {_stopwatch.ElapsedMilliseconds:F2} ms");
			_stopwatch.Reset();
		}

		private void SingleThreadExecution()
		{
			foreach (var item in _files)
			{
				try
				{
					System.IO.File.Delete(item);
				}
				catch (System.Exception e)
				{
					Console.Error.WriteLine($"Failed to delete file: {Environment.NewLine}{e}");
				}
			}
		}

		static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);
	}
}
