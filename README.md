helios
======

Helios is concurrency and networking middleware for .NET - think of it as a loose C# port of Java's wildly influential [Netty](http://netty.io/) library. Helios offers .NET developers the ability to develop high-performance networked applications on top of TCP and UDP sockets combined with powerful stream-management, event brokering, and concurrency capabilities.

Helios is currently used to power all of the network operations inside [Akka.NET](https://github.com/akkadotnet/akka.net).

## Features
Helios has a combination of features that were all chosen for their practical value inside networked and event-driven applications:

1. TCP and UDP reactor servers, designed for high-performance socket-servers in .NET
1. TCP and UDP clients.
1. Network message encoders and decoders.
1. Fibers and Event Loops, for easily pushing events and delegates onto dedicated thread-pools.
1. ByteBuffers and tools for simplifying buffer management.
1. Event Brokers and subscription management.
1. Custom `IEnumerable` implementations, such as the `ICircularBuffer`.
1. And lots of other utilities, such as `NullGuard` and `AtomicReference`.

## License
See [LICENSE](https://github.com/Aaronontheweb/helios/blob/master/LICENSE) for details. 

## Contributing
Helios happily accepts pull requests - please use concise, clear commit messages and reference Issue numbers in your pull requests if appropriate.
