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

using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCoreFileDeleteBenchmark
{
	public class TaskThread : DeleteThread
	{
		private readonly Task _task;

		public TaskThread(List<string> paths) : base(paths)
		{
			_task = new Task(Process);
		}

		public override void Start()
		{
			_task.Start();
		}

		public override void WaitToComplete()
		{
			_task.Wait();
		}	
	}
}