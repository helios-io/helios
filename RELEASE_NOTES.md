#### 1.4.1 Jul 07 2015
Bugfix - we no longer throw exceptions upon shutting down TCP reactors.

#### 1.4.0 Apr 01 2015
Major update to Helios designed to help support [Akka.NET v1](http://getakka.net/).

* Added Mono support to all outbound clients. All benchmarks have been run on Mono as well as the full build suite.
* Fixed data loss issued caused during periods of high-speed writes. This has been resolved.
* Added stubs for tracing and monitoring.
* Integrated [Helios.DedicatedThreadPool](https://github.com/helios-io/DedicatedThreadPool) for a massive performance boost on the `DedicatedThreadFiber`, used by default on most socket clients.


#### 1.3.6 Feb 09 2015
* Minor bug fixes for concurrent exceptions