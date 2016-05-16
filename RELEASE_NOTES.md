#### 2.1.0 May 16 2016
Placeholder for next major perf upgrade

#### 2.0.2 May 6 2016
* Added DNS support to `ServerBootstrap` so hostnames can be bound.

#### 2.0.1 May 6 2016
* Added `MessageToMessageDecoder<T>` base class to Helios.Codecs

#### 2.0 May 3 2016
Major performance and stability rewrite of Helios, including breaking API changes.

The existing API has been left intact, but marked as `Obsolete`. Going forward please use the `IChannel` APIs provided inside the `Helios.Channels` namespace. They are virtually identical to the equivalent DotNetty APIs, although with some minor differences as a result of existing code and styles within Helios itself.

#### 1.4.2 Dec 12 2015
Bugfixed - fixed an issue with `NoOpDecoder` where it wouldn't properly drain incoming `IByteBuffer` instances.

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