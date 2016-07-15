// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FsCheck;
using FsCheck.Experimental;
using Helios.Channels;
using Helios.Channels.Bootstrap;
using Helios.Channels.Sockets;
using Helios.Codecs;
using Helios.FsCheck.Tests.Channels.Sockets.Models;
using Helios.Tests.Channels;
using Helios.Util;
using Helios.Util.Concurrency;

namespace Helios.FsCheck.Tests.Channels.Sockets
{
    public class TcpServerSocketChannelStateMachine : Machine<ITcpServerSocketModel, ITcpServerSocketModel>
    {
        public static IPEndPoint TEST_ADDRESS = new IPEndPoint(IPAddress.IPv6Loopback, 0);

        private ModelData _internalState;
        private readonly IEventLoopGroup ClientWorkerGroup = new MultithreadEventLoopGroup(2);
        public int OperationSanityCheck;

        private readonly IEventLoopGroup ServerGroup = new MultithreadEventLoopGroup(2);
        private readonly IEventLoopGroup ServerWorkerGroup = new MultithreadEventLoopGroup(2);

        protected ModelData InternalState
        {
            get { return _internalState ?? (_internalState = InitializeServer()); }
            set { _internalState = value; }
        }

        public override Arbitrary<Setup<ITcpServerSocketModel, ITcpServerSocketModel>> Setup
        {
            get
            {
                return
                    Arb.From(
                        Gen.Constant(
                            (Setup<ITcpServerSocketModel, ITcpServerSocketModel>)
                                new TcpServerSocketChannelSetup(InternalState)));
            }
        }


        public override TearDown<ITcpServerSocketModel> TearDown => new ShutdownChannels(this);

        public override Gen<Operation<ITcpServerSocketModel, ITcpServerSocketModel>> Next(ITcpServerSocketModel obj0)
        {
            Gen<Operation<ITcpServerSocketModel, ITcpServerSocketModel>> returnGen = null;
            if (obj0.BoundAddress != null && obj0.LocalChannels.Count == 0)
            {
                returnGen = ClientConnect.Generator();
            }
            else
            {
                returnGen = Gen.OneOf(ClientWrite.Generator(), ClientDisconnect.Generator(), ClientConnect.Generator());
            }

            OperationSanityCheck++;
            return returnGen;
        }

        public ModelData InitializeServer()
        {
            var md = new ModelData();
            md.ServerEventLoop = ServerGroup;
            md.ServerWorkerEventLoop = ServerWorkerGroup;
            md.ClientEventLoop = ClientWorkerGroup;
            var actual = md.ActualModel = new ConcurrentTcpServerSocketModel();
            var sb = new ServerBootstrap()
                .Group(md.ServerEventLoop, md.ServerWorkerEventLoop)
                .Channel<TcpServerSocketChannel>()
                .ChildHandler(new ActionChannelInitializer<TcpSocketChannel>(channel =>
                {
                    // actual is implemented using a ConcurrentTcpServerSocketModel, shared mutable state between all children.
                    ConstructServerPipeline(channel, actual);
                }));

            var serverChannel = md.ServerChannel = sb.BindAsync(TEST_ADDRESS).Result;
            md.ActualModel.SetSelf(serverChannel);
            return md;
        }

        public void ShutdownAll()
        {
            try
            {
                Task.WaitAll(ServerGroup.ShutdownGracefullyAsync(), ServerWorkerGroup.ShutdownGracefullyAsync(),
                    ClientWorkerGroup.ShutdownGracefullyAsync());
            }
            catch
            {
            }
        }

        public sealed class ModelData
        {
            public ITcpServerSocketModel ActualModel;
            public IEventLoopGroup ClientEventLoop;
            public IChannel ServerChannel;
            public IEventLoopGroup ServerEventLoop;
            public IEventLoopGroup ServerWorkerEventLoop;
        }

        #region Setup and TearDown

        /// <summary>
        ///     We don't perform any actual work on the channel here - that's performed inside separate commands
        ///     that have to be activated in an order determined by pre-conditions.
        /// </summary>
        public class TcpServerSocketChannelSetup : Setup<ITcpServerSocketModel, ITcpServerSocketModel>
        {
            private readonly ModelData _internalState;

            public TcpServerSocketChannelSetup(ModelData internalState)
            {
                _internalState = internalState;
            }

            public override ITcpServerSocketModel Actual()
            {
                return _internalState.ActualModel;
            }

            public override ITcpServerSocketModel Model()
            {
                return new ImmutableTcpServerSocketModel(_internalState.ServerChannel,
                    (IPEndPoint) _internalState.ServerChannel.LocalAddress);
            }
        }

        public class ShutdownChannels : TearDown<ITcpServerSocketModel>
        {
            private readonly TcpServerSocketChannelStateMachine _sm;
            private readonly ModelData Model;

            public ShutdownChannels(TcpServerSocketChannelStateMachine sm)
            {
                _sm = sm;
                Model = _sm.InternalState;
            }

            public override void Actual(ITcpServerSocketModel _arg1)
            {
                if (Model != null)
                {
                    var server = Model.ServerChannel as IServerChannel;
                    var serverEventLoop = Model.ServerEventLoop;
                    var workerEventLoop = Model.ServerWorkerEventLoop;
                    var clientEventLoop = Model.ClientEventLoop;
                    var clientCloses = new List<Task>();
                    foreach (var client in _arg1.LocalChannels)
                        clientCloses.Add(client.CloseAsync());
                    var closeChannels = Task.WhenAll(clientCloses).ContinueWith(tr =>
                    {
                        if (server != null)
                        {
                            return server.CloseAsync();
                        }
                        return TaskEx.Completed;
                    });


                    try
                    {
                        closeChannels.Wait();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception occurred while tearing down MBT: {0}", ex);
                    }


                    // clear the state, which will force the server to rebuild again
                    _sm.InternalState = null;
                }
            }
        }

        #endregion

        #region Helpers

        public static IChannelHandler FreshLengthFramePrepender()
        {
            return new LengthFieldPrepender(4, false);
        }

        public static IChannelHandler FreshLengthFrameDecoder()
        {
            return new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4, true);
        }

        public static IChannelHandler NewIntCodec()
        {
            // releasing messages on this iteration, since these tests may run long
            return new IntCodec(true);
        }

        public static IChannelHandler NewServerHandler(ITcpServerSocketModel model)
        {
            return new TcpServerSocketStateHandler(model);
        }

        public static IChannelHandler NewClientHandler(IChannel self)
        {
            return new TcpClientSocketStateHandler(new TcpClientSocketModel(self));
        }

        public static void ConstructServerPipeline(IChannel channelToModify, ITcpServerSocketModel model)
        {
            channelToModify.Pipeline.AddLast(FreshLengthFramePrepender())
                .AddLast(FreshLengthFrameDecoder())
                .AddLast(NewIntCodec())
                .AddLast(NewServerHandler(model));
        }

        public static void ConstructClientPipeline(IChannel channelToModify)
        {
            channelToModify.Pipeline.AddLast(FreshLengthFramePrepender())
                .AddLast(FreshLengthFrameDecoder())
                .AddLast(NewIntCodec())
                .AddLast(NewClientHandler(channelToModify));
        }

        #endregion

        #region Generators

        public class ServerEventLoops
        {
            public ServerEventLoops(IEventLoopGroup serverEventLoopGroup, IEventLoopGroup workerEventLoopGroup)
            {
                ServerEventLoopGroup = serverEventLoopGroup;
                WorkerEventLoopGroup = workerEventLoopGroup;
            }

            public IEventLoopGroup ServerEventLoopGroup { get; private set; }
            public IEventLoopGroup WorkerEventLoopGroup { get; private set; }
        }

        public static Arbitrary<ServerEventLoops> GenServerEventLoops()
        {
            return
                Arb.From(
                    Gen.Constant(new ServerEventLoops(new MultithreadEventLoopGroup(1), new MultithreadEventLoopGroup(2))));
        }

        public static Arbitrary<IEventLoopGroup> GenClientEventLoops()
        {
            return Arb.From(Gen.Constant((IEventLoopGroup) new MultithreadEventLoopGroup(2)));
        }

        #endregion

        #region Commands

        public abstract class NonBindCommand : Operation<ITcpServerSocketModel, ITcpServerSocketModel>
        {
            public Exception CaughtException { get; protected set; }

            public override bool Pre(ITcpServerSocketModel _arg1)
            {
                // must be bound
                return _arg1.BoundAddress != null;
            }

            public sealed override Property Check(ITcpServerSocketModel obj0, ITcpServerSocketModel obj1)
            {
                if (CaughtException != null)
                {
                    return false.Label($"Exception occurred on run: {CaughtException}");
                }

                return CheckInternal(obj0, obj1);
            }

            protected abstract Property CheckInternal(ITcpServerSocketModel obj0, ITcpServerSocketModel obj1);

            public sealed override ITcpServerSocketModel Run(ITcpServerSocketModel obj0)
            {
                try
                {
                    return RunInternal(obj0);
                }
                catch (AggregateException ex)
                {
                    CaughtException = ex.Flatten();
                    return obj0;
                }
            }

            protected abstract ITcpServerSocketModel RunInternal(ITcpServerSocketModel obj0);
        }

        public class ClientConnect : NonBindCommand
        {
            public ClientConnect(int clientCount, IEventLoopGroup clientEventLoopGroup)
            {
                ClientCount = clientCount;
                ClientEventLoopGroup = clientEventLoopGroup;
            }

            /// <summary>
            ///     The number of clients we're going to attempt to connect simultaneously
            /// </summary>
            public int ClientCount { get; }

            public IEventLoopGroup ClientEventLoopGroup { get; }

            public static Gen<Operation<ITcpServerSocketModel, ITcpServerSocketModel>> Generator()
            {
                Func<int, IEventLoopGroup, Operation<ITcpServerSocketModel, ITcpServerSocketModel>> producer =
                    (s, e) => new ClientConnect(s, e) as Operation<ITcpServerSocketModel, ITcpServerSocketModel>;
                var fsFunc = FsharpDelegateHelper.Create(producer);
                return Gen.Map2(fsFunc, Gen.Choose(1, 10), GenClientEventLoops().Generator);
            }

            protected override Property CheckInternal(ITcpServerSocketModel obj0, ITcpServerSocketModel obj1)
            {
                return TcpServerSocketModelComparer.Instance.Equals(obj0, obj1)
                    .Label(
                        $"Expected {obj1} (order doesn't matter) but was {obj0}");
            }

            protected override ITcpServerSocketModel RunInternal(ITcpServerSocketModel obj0)
            {
                var cb = new ClientBootstrap()
                    .Group(ClientEventLoopGroup)
                    .Channel<TcpSocketChannel>()
                    .Handler(new ActionChannelInitializer<TcpSocketChannel>(ConstructClientPipeline));

                var connectTasks = new List<Task<IChannel>>();
                for (var i = 0; i < ClientCount; i++)
                {
                    connectTasks.Add(cb.ConnectAsync(obj0.BoundAddress));
                }

                if (!Task.WaitAll(connectTasks.ToArray(), TimeSpan.FromSeconds(ClientCount*2)))
                {
                    throw new TimeoutException(
                        $"Waited {ClientCount} seconds to connect {ClientCount} clients to {obj0.BoundAddress}, but the operation timed out.");
                }

                foreach (var task in connectTasks)
                {
                    // storing our local address for comparison purposes
                    obj0 = obj0.AddLocalChannel(task.Result).AddClient((IPEndPoint) task.Result.LocalAddress);
                }
                return obj0;
            }

            public override bool Pre(ITcpServerSocketModel _arg1)
            {
                // need at least 1 client
                return base.Pre(_arg1) && ClientCount > 0;
            }

            public override string ToString()
            {
                return $"ClientConnect(Count={ClientCount})";
            }
        }

        public class ClientDisconnect : NonBindCommand
        {
            public ClientDisconnect(int clientCount)
            {
                ClientCount = clientCount;
            }

            public int ClientCount { get; }

            public static Gen<Operation<ITcpServerSocketModel, ITcpServerSocketModel>> Generator()
            {
                return
                    Gen.Choose(1, 10)
                        .Select(x => (Operation<ITcpServerSocketModel, ITcpServerSocketModel>) new ClientDisconnect(x));
            }

            protected override Property CheckInternal(ITcpServerSocketModel obj0, ITcpServerSocketModel obj1)
            {
                return TcpServerSocketModelComparer.Instance.Equals(obj0, obj1)
                    .Label(
                        $"Expected {obj1} (order doesn't matter) but was {obj0}");
            }

            protected override ITcpServerSocketModel RunInternal(ITcpServerSocketModel obj0)
            {
                var clientsToBeDisconnected = obj0.LocalChannels.Take(ClientCount).ToList();
                var disconnectTasks = new List<Task>();
                foreach (var client in clientsToBeDisconnected)
                {
                    disconnectTasks.Add(client.DisconnectAsync());
                }

                if (Task.WaitAll(disconnectTasks.ToArray(), TimeSpan.FromSeconds(ClientCount)))
                {
                    throw new TimeoutException(
                        $"Waited {ClientCount} seconds to disconnect {ClientCount} clients from {obj0.BoundAddress}, but the operation timed out.");
                }

                foreach (var client in clientsToBeDisconnected)
                {
                    obj0 = obj0.RemoveClient((IPEndPoint) client.LocalAddress).RemoveLocalChannel(client);
                }
                return obj0;
            }

            public override bool Pre(ITcpServerSocketModel _arg1)
            {
                return base.Pre(_arg1) && ClientCount <= _arg1.LocalChannels.Count;
            }

            public override string ToString()
            {
                return $"ClientDisconnect(Count={ClientCount})";
            }
        }

        public class ClientWrite : NonBindCommand
        {
            private readonly int[] _writes;

            public ClientWrite(int[] writes)
            {
                _writes = writes;
            }

            public static Gen<Operation<ITcpServerSocketModel, ITcpServerSocketModel>> Generator()
            {
                return
                    Gen.ArrayOf(Arb.Default.Int32().Generator)
                        .Select(x => (Operation<ITcpServerSocketModel, ITcpServerSocketModel>) new ClientWrite(x));
            }

            protected override Property CheckInternal(ITcpServerSocketModel obj0, ITcpServerSocketModel obj1)
            {
                // need to give the inbound side a chance to catch up if the load was large.
                Task.Delay(100).Wait();
                Debugger.Break();
                var result = TcpServerSocketModelComparer.Instance.Equals(obj0, obj1)
                    .Label(
                        $"Expected [{string.Join(",", obj1.LastReceivedMessages.OrderBy(x => x))}] received by clients, but found only [{string.Join(",", obj0.LastReceivedMessages.OrderBy(x => x))}]");

                obj0 = obj0.ClearMessages();
                obj1 = obj1.ClearMessages();

                return result;
            }

            protected override ITcpServerSocketModel RunInternal(ITcpServerSocketModel obj0)
            {
                var channels = obj0.LocalChannels;
                var maxIndex = channels.Count - 1;

                // write and read order will be different, but our equality checker knows how to deal with that.
                var rValue = obj0.ClearMessages().WriteMessages(_writes).ReceiveMessages(_writes);

                var tasks = new ConcurrentBag<Task>();
                // generate a distribution of writes concurrently across all channels
                var loopResult = Parallel.ForEach(_writes, i =>
                {
                    var nextChannel = channels[ThreadLocalRandom.Current.Next(0, maxIndex)];

                    // do write and flush for all writes
                    tasks.Add(nextChannel.WriteAndFlushAsync(i));
                });

                // wait for tasks to finish queuing
                SpinWait.SpinUntil(() => loopResult.IsCompleted, TimeSpan.FromMilliseconds(100));

                // picking big timeouts here just in case the input list is large
                var timeout = TimeSpan.FromSeconds(5);
                if (!Task.WaitAll(tasks.ToArray(), timeout))
                {
                    throw new TimeoutException(
                        $"Expected to be able to complete {_writes.Length} operations in under {timeout.TotalSeconds} seconds, but the operation timed out.");
                }

                return rValue;
            }

            public override bool Pre(ITcpServerSocketModel _arg1)
            {
                return base.Pre(_arg1)
                       && _arg1.LocalChannels.Count > 0 // need at least 1 local channel
                       && _writes.Length > 0; // need at least 1 write
            }
        }

        #endregion
    }
}