#### 2.1.3 Aug 18 2016
* Full Mono support now available for Helios 2.x.
* Improved exception logging inside `TcpSocketChannel` and others.

You can see the [full set of changes for Helios 2.1.3 here](https://github.com/helios-io/helios/milestone/3).

#### 2.1.2 Jul 14 2016
* Made DNS resolution configurable - can target IPV4 / IPV6 or any other `AddressFamily`.
* Caught `ObjectDisposedException`s that are thrown on shutdown so they are no longer logged.

#### 2.1.1 May 27 2016
* Fixed byte buffers - there were reporting that they were encoding as `LittleEndian`. Turns out they were using `BigEndian`. This has been fixed.
* Fixed issue with `AbstractDerivedByteBuf` where calling `Retain` would return the original underlying buffer and not the derived buffer.
* Made configuration warnings in `ServerBootstrap` less cryptic.


#### 2.1.0 May 16 2016
* Added support for batch writes
* Made write objects reusable
* IPv6 support for legacy API

Net performance impact of above changes as reported by build server:

**Before**

          Metric |           Units |             Max |         Average |             Min |          StdDev |
---------------- |---------------- |---------------- |---------------- |---------------- |---------------- |
TotalCollections [Gen0] |     collections / s |           40.14 |           40.14 |           40.14 |            0.00 |
TotalCollections [Gen1] |     collections / s |            2.86 |            2.86 |            2.86 |            0.00 |
TotalCollections [Gen2] |     collections / s |            0.53 |            0.53 |            0.53 |            0.00 |
TotalBytesAllocated |           bytes / s |      293,950.63 |      293,950.63 |      293,950.63 |            0.00 |
[Counter] inbound ops |      operations / s |       55,782.90 |       55,782.90 |       55,782.90 |            0.00 |
[Counter] outbound ops |      operations / |       55,783.07 |       55,783.07 |       55,783.07 |            0.00 |
Max concurrent connections |      operations |          440.00 |          440.00 |          440.00 |            0.00 |

**After**

          Metric |           Units |             Max |         Average |             Min |          StdDev |
---------------- |---------------- |---------------- |---------------- |---------------- |---------------- |
TotalCollections [Gen0] |     collections |           26.41 |           26.41 |           26.41 |            0.00 |
TotalCollections [Gen1] |     collections |            8.33 |            8.33 |            8.33 |            0.00 |
TotalCollections [Gen2] |     collections |            0.08 |            0.08 |            0.08 |            0.00 |
TotalBytesAllocated |           bytes |       38,170.63 |       38,170.63 |       38,170.63 |            0.00 |
[Counter] inbound ops |      operations |       99,728.63 |       99,728.63 |       99,728.63 |            0.00 |
[Counter] outbound ops |      operations |       99,728.67 |       99,728.67 |       99,728.67 |            0.00 |
Max concurrent connections |      operations |          945.00 |          945.00 |          945.00 |            0.00 | 

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