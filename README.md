NetCoreFileDeleteBenchmark
==========================

This project was created to test a theory that Async File.Deletes cannot introduce some throughput gains. This was made simple using the standard File.Delete method. When used with async, as many threads as possible are created. Results are below.

Results
==========
Test System
-----------
* **OS:** Kubuntu 19.04 with GNU/Linux 5.0.0-25-generic
* **CPU:** AMD Ryzen Threadripper 1920X (12c/24t)
* **RAM:** 32 GB (2x16GB NUMA Configuration)
* **Test Storage:** Samsung 970 EVO 500GB

Table of Results
----------------
* On async runs, `Environment.ProcessorCount` threads are created and files are equally distributed to them before starting. 
* Relative percentages are related to perspective Sync/Async.
* ASync(T): Many threads deleting files in succession. Threads are actually `Task.Run()` tasks, so no guarantees there.
* ASync(N): Same as above but the execution is guaranteed to be run in separated Native threads created by `Thread` class.


| Mode    | N of Files	| File Size	|   Time    | Relative  |
|---------|-------------|-----------|-----------|-----------|
| Sync    |	1000		|	1B		|  ~10ms	|	100%	|
| ASync(T)|	1000		|	1B		|  ~11ms   	|	90%		|
| ASync(N)|	1000		|	1B		|  ~10ms   	|	100%	|
| Sync    |	10000		|	1B		|  99ms		|	100%	|
| ASync(T)|	10000		|	1B		|  92ms   	|	108%	|
| ASync(N)|	10000		|	1B		|  92ms   	|	108%	|
| Sync    |	100000		|	1B		|  1063ms	|	100%	|
| ASync(T)|	100000		|	1B		|  865ms   	|	122%	|
| ASync(N)|	100000		|	1B		|  870ms   	|	122%	|
| Sync    |	100000		|	1B		|  11645ms	|	100%	|
| ASync(T)|	100000		|	1B		|  9387ms   |	124%	|
| ASync(N)|	100000		|	1B		|  9138ms   |	127%	|


More results will be posted on the above table.