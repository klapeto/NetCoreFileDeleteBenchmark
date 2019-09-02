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

namespace NetCoreFileDeleteBenchmark
{
	public abstract class DeleteThread
	{
		private List<string> _filePaths;
		protected DeleteThread(List<string> filePaths) => _filePaths = filePaths;

		protected void Process()
		{
			foreach (var file in _filePaths)
			{
				try
				{
					System.IO.File.Delete(file);
				}
				catch (System.Exception e)
				{
					Console.WriteLine($"Failed to delete file: '{file}'{Environment.NewLine}{e}");
				}
			}
		}

		public abstract void Start();
		public abstract void WaitToComplete();
	}
}