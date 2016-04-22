using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FsCheck;
using FsCheck.Experimental;
using FSharpx;
using Helios.Channels;
using Microsoft.FSharp.Core;
using Random = FsCheck.Random;
using Task = System.Threading.Tasks.Task;

// ReSharper disable RedundantOverridenMember

namespace Helios.FsCheck.Tests.Channels
{
    public class PipelineModelNode
    {
        public string Name;
        public NamedChannelHandler Handler;
        public PipelineModelNode Previous;
        public PipelineModelNode Next;

        public override string ToString()
        {
            return $"{Handler}";
        }

        public PipelineModelNode Clone()
        {
            return new PipelineModelNode() { Handler = Handler, Next = Next, Name = Name, Previous = Previous };
        }
    }

    public class PipelineMutationModel
    {
        public PipelineModelNode Head;
        public PipelineModelNode Tail;
        public int Length;

        public bool Contains(string name)
        {
            var current = Head;
            while (current != null)
            {
                if (string.Equals(current.Name, name))
                    return true;
                current = current.Next;
                if (current == null)
                {
                    break;
                }
            }
            return false;
        }

        public Queue<Tuple<string, SupportedEvent>> PredictedOutcome(SupportedEvent e)
        {
            var q = new Queue<Tuple<string, SupportedEvent>>();
            var current = Head;
            while (current != null)
            {
                if (current.Handler.Event.HasFlag(e))
                {
                    q.Enqueue(Tuple.Create(current.Handler.Name, e));
                }
                current = current.Next;
                if (current == null)
                {
                    break;
                }
            }
            return q;
        } 

        public override string ToString()
        {
            var pipeString = string.Empty;
            var current = Head;
            while (current != null)
            {
                pipeString += $"{current}";
                current = current.Next;
                if (current == null)
                {
                    break;
                }
                pipeString += " --> ";
            }

            return $"Model(Length={Length}, Items={pipeString})";
        }

        public PipelineMutationModel Clone()
        {
            var newHead = Head.Clone();
            var current = newHead;
            var next = Head.Next;
            while (next != null)
            {
                var clone = next.Clone();
                current.Next = clone;
                clone.Previous = current;
                next = next.Next;
                current = clone;
            }
            return new PipelineMutationModel() { Head = newHead, Length = Length, Tail = current };
        }

        public static PipelineMutationModel Fresh()
        {
            var head = new PipelineModelNode() { Handler = new NamedChannelHandler("HEAD"), Name = "HEAD" };
            var tail = new PipelineModelNode() { Handler = new NamedChannelHandler("TAIL"), Name = "TAIL" };
            head.Next = tail;
            tail.Previous = head;
            return new PipelineMutationModel() { Length = 2, Head = head, Tail = tail };
        }
    }

    [Flags]
    public enum SupportedEvent : uint
    {
        None,
        ChannelRegistered,
        ChannelUnregistered,
        ChannelActive,
        ChannelInactive,
        ChannelRead,
        ChannelReadComplete,
        ChannelWritabilityChanged,
        HandlerAdded,
        HandlerRemoved,
        UserEventTriggered,
        WriteAsync,
        Flush,
        BindAsync,
        ConnectAsync,
        DisconnectAsync,
        CloseAsync,
        ExceptionCaught,
        DeregisterAsync,
        Read
    }

    #region ChanneldHandlers

    public class NamedChannelHandler : ChannelHandlerAdapter
    {
        public static Gen<string> CreateName()
        {
            return Arb.Default.Char().Generator.ArrayOf(30).Select(x => new string(x));
        }

        public static Gen<NamedChannelHandler> CreateHandler()
        {
            return CreateName().Select(s => new NamedChannelHandler(s));
        }

        public static Gen<NamedChannelHandler> AllHandlers()
        {
            return Gen.OneOf(CreateHandler(), OutboundNamedChannelHandler.CreateOutboundHandler(),
                AllEventsChannelHandler.GenAllEventsHandler());
        }

        public SupportedEvent Event { get; internal set; } = SupportedEvent.None;

        public NamedChannelHandler(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public void RegisterFiredEvent(SupportedEvent e)
        {
            if (Event.HasFlag(e))
            {
                ChannelPipelineModel.EventQueue.Enqueue(new Tuple<string, SupportedEvent>(Name, e));
            }
        }

        private string _eventsStr;

        protected string EventsStr
        {
            get
            {
                if (_eventsStr == null)
                {
                    var supportedEvents = new List<SupportedEvent>();
                    foreach (var value in Enum.GetValues(typeof(SupportedEvent)).Cast<SupportedEvent>())
                    {
                        if (Event.HasFlag(value))
                            supportedEvents.Add(value);
                    }
                    _eventsStr = string.Join("|", supportedEvents);
                }
                return _eventsStr;
            }
        }

        public override string ToString()
        {

            return $"NamedChannelHandler(Name={Name}, Events={EventsStr})";
        }

        [Skip]
        public override void ChannelRegistered(IChannelHandlerContext context)
        {
            RegisterFiredEvent(SupportedEvent.ChannelRegistered);
            context.FireChannelRegistered();
        }

        [Skip]
        public override void ChannelUnregistered(IChannelHandlerContext context)
        {
            RegisterFiredEvent(SupportedEvent.ChannelUnregistered);
            context.FireChannelUnregistered();
        }

        [Skip]
        public override void ChannelActive(IChannelHandlerContext context)
        {
            RegisterFiredEvent(SupportedEvent.ChannelActive);
            context.FireChannelActive();
        }

        [Skip]
        public override void ChannelInactive(IChannelHandlerContext context)
        {
            RegisterFiredEvent(SupportedEvent.ChannelInactive);
            context.FireChannelInactive();
        }

        [Skip]
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            RegisterFiredEvent(SupportedEvent.ChannelRead);
            context.FireChannelRead(message);
        }

        [Skip]
        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            RegisterFiredEvent(SupportedEvent.ChannelReadComplete);
            context.FireChannelReadComplete();
        }

        [Skip]
        public override void ChannelWritabilityChanged(IChannelHandlerContext context)
        {
            RegisterFiredEvent(SupportedEvent.ChannelWritabilityChanged);
            context.FireChannelWritabilityChanged();
        }

        [Skip]
        public override void HandlerAdded(IChannelHandlerContext context)
        {
            RegisterFiredEvent(SupportedEvent.HandlerAdded);
        }

        [Skip]
        public override void HandlerRemoved(IChannelHandlerContext context)
        {
            RegisterFiredEvent(SupportedEvent.HandlerRemoved);
        }

        [Skip]
        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            RegisterFiredEvent(SupportedEvent.UserEventTriggered);
            context.FireUserEventTriggered(evt);
        }

        [Skip]
        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            RegisterFiredEvent(SupportedEvent.WriteAsync);
            return context.WriteAsync(message);
        }

        [Skip]
        public override void Flush(IChannelHandlerContext context)
        {
            RegisterFiredEvent(SupportedEvent.Flush);
            context.Flush();
        }

        [Skip]
        public override Task BindAsync(IChannelHandlerContext context, EndPoint localAddress)
        {
            RegisterFiredEvent(SupportedEvent.BindAsync);
            return context.BindAsync(localAddress);
        }

        [Skip]
        public override Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
        {
            RegisterFiredEvent(SupportedEvent.ConnectAsync);
            return context.ConnectAsync(remoteAddress, localAddress);
        }

        [Skip]
        public override Task DisconnectAsync(IChannelHandlerContext context)
        {
            RegisterFiredEvent(SupportedEvent.DisconnectAsync);
            return context.DisconnectAsync();
        }

        [Skip]
        public override Task CloseAsync(IChannelHandlerContext context)
        {
            RegisterFiredEvent(SupportedEvent.CloseAsync);
            return context.CloseAsync();
        }

        [Skip]
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            RegisterFiredEvent(SupportedEvent.ExceptionCaught);
            context.FireExceptionCaught(exception);
        }

        [Skip]
        public override Task DeregisterAsync(IChannelHandlerContext context)
        {
            RegisterFiredEvent(SupportedEvent.DeregisterAsync);
            return context.DeregisterAsync();
        }

        [Skip]
        public override void Read(IChannelHandlerContext context)
        {
            RegisterFiredEvent(SupportedEvent.Read);
            context.Read();
        }
    }

    public class OutboundNamedChannelHandler : NamedChannelHandler
    {
        public static Gen<NamedChannelHandler> CreateOutboundHandler()
        {
            return CreateName().Select(s => (NamedChannelHandler)new OutboundNamedChannelHandler(s));
        }

        public OutboundNamedChannelHandler(string name) : base(name)
        {
            Event = SupportedEvent.WriteAsync
                     & SupportedEvent.Flush
                     & SupportedEvent.BindAsync
                     & SupportedEvent.ConnectAsync
                     & SupportedEvent.DisconnectAsync
                     & SupportedEvent.CloseAsync
                     & SupportedEvent.ExceptionCaught
                     & SupportedEvent.DeregisterAsync
                     & SupportedEvent.Read;
        }

        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            return base.WriteAsync(context, message);
        }

        public override void Flush(IChannelHandlerContext context)
        {
            base.Flush(context);
        }

        public override Task BindAsync(IChannelHandlerContext context, EndPoint localAddress)
        {
            return base.BindAsync(context, localAddress);
        }

        public override Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
        {
            return base.ConnectAsync(context, remoteAddress, localAddress);
        }

        public override Task DisconnectAsync(IChannelHandlerContext context)
        {
            return base.DisconnectAsync(context);
        }

        public override Task CloseAsync(IChannelHandlerContext context)
        {
            return base.CloseAsync(context);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            base.ExceptionCaught(context, exception);
        }

        public override Task DeregisterAsync(IChannelHandlerContext context)
        {
            return base.DeregisterAsync(context);
        }

        public override void Read(IChannelHandlerContext context)
        {
            base.Read(context);
        }
    }

    public class AllEventsChannelHandler : NamedChannelHandler
    {
        public static readonly SupportedEvent[] AllValidEvent =
            Enum.GetValues(typeof(SupportedEvent)).Cast<SupportedEvent>().Except(new[] { SupportedEvent.None }).ToArray();

        public static Gen<SupportedEvent[]> GenEvents()
        {
            return Gen.ArrayOf(Gen.Elements(AllValidEvent));
        }

        public static FSharpFunc<T2, TResult> Create<T2, TResult>(Func<T2, TResult> func)
        {
            Converter<T2, TResult> conv = input => func(input);
            return FSharpFunc<T2, TResult>.FromConverter(conv);
        }

        public static FSharpFunc<T1, FSharpFunc<T2, TResult>> Create<T1, T2, TResult>(Func<T1, T2, TResult> func)
        {
            Converter<T1, FSharpFunc<T2, TResult>> conv = value1 =>
         {
             return Create<T2, TResult>(value2 => func(value1, value2));
         };
            return FSharpFunc<T1, FSharpFunc<T2, TResult>>.FromConverter(conv);
        }

        public static Gen<NamedChannelHandler> GenAllEventsHandler()
        {
            Func<string, SupportedEvent[], NamedChannelHandler> producer = (s, e) => new AllEventsChannelHandler(s, e) as NamedChannelHandler;
            var fsFunc = Create(producer);
            return Gen.Map2(fsFunc, CreateName(), GenEvents());
        }

        public AllEventsChannelHandler(string name, SupportedEvent[] @event) : base(name)
        {
            foreach (var e in @event.Distinct())
            {
                Event = Event & e;
            }
        }

        public override void ChannelRegistered(IChannelHandlerContext context)
        {
            base.ChannelRegistered(context);
        }

        public override void ChannelUnregistered(IChannelHandlerContext context)
        {
            base.ChannelUnregistered(context);
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            base.ChannelInactive(context);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            base.ChannelRead(context, message);
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            base.ChannelReadComplete(context);
        }

        public override void ChannelWritabilityChanged(IChannelHandlerContext context)
        {
            base.ChannelWritabilityChanged(context);
        }

        public override void HandlerAdded(IChannelHandlerContext context)
        {
            base.HandlerAdded(context);
        }

        public override void HandlerRemoved(IChannelHandlerContext context)
        {
            base.HandlerRemoved(context);
        }

        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            base.UserEventTriggered(context, evt);
        }

        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            return base.WriteAsync(context, message);
        }

        public override void Flush(IChannelHandlerContext context)
        {
            base.Flush(context);
        }

        public override Task BindAsync(IChannelHandlerContext context, EndPoint localAddress)
        {
            return base.BindAsync(context, localAddress);
        }

        public override Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
        {
            return base.ConnectAsync(context, remoteAddress, localAddress);
        }

        public override Task DisconnectAsync(IChannelHandlerContext context)
        {
            return base.DisconnectAsync(context);
        }

        public override Task CloseAsync(IChannelHandlerContext context)
        {
            return base.CloseAsync(context);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            base.ExceptionCaught(context, exception);
        }

        public override Task DeregisterAsync(IChannelHandlerContext context)
        {
            return base.DeregisterAsync(context);
        }

        public override void Read(IChannelHandlerContext context)
        {
            base.Read(context);
        }
    }

    #endregion

    public class ChannelPipelineModel : Machine<IChannelPipeline, PipelineMutationModel>
    {
        /// <summary>
        /// Queue used by <see cref="NamedChannelHandler"/> implementations to register their results
        /// </summary>
        public static readonly Queue<Tuple<string, SupportedEvent>> EventQueue = new Queue<Tuple<string, SupportedEvent>>();

        public static PipelineMutationModel AddToHead(PipelineMutationModel obj0, PipelineModelNode newNode)
        {
            var model = obj0.Clone();
            var next = model.Head.Next;
            newNode.Previous = model.Head;
            newNode.Next = next;
            next.Previous = newNode;
            model.Head.Next = newNode;
            model.Length++;
            return model;
        }

        public static PipelineMutationModel RemoveHead(PipelineMutationModel obj0)
        {
            var model = obj0.Clone();
            var oldNext = model.Head.Next;
            var newNext = oldNext.Next;
            newNext.Previous = model.Head;
            model.Head.Next = newNext;
            model.Length--;
            return model;
        }

        public static PipelineMutationModel AddToTail(PipelineMutationModel obj0, PipelineModelNode newNode)
        {
            var model = obj0.Clone();
            var prev = model.Tail.Previous;
            newNode.Previous = prev;
            newNode.Next = model.Tail;
            prev.Next = newNode;
            model.Tail.Previous = newNode;
            model.Length++;
            return model;
        }

        public static PipelineMutationModel RemoveTail(PipelineMutationModel obj0)
        {
            var model = obj0.Clone();
            var oldPrev = model.Tail.Previous;
            var newPrev = oldPrev.Previous;
            model.Tail.Previous = newPrev;
            newPrev.Next = model.Tail;
            model.Length--;
            return model;
        }

        private readonly bool _supportsEventFiring;

        public ChannelPipelineModel(bool supportsEventFiring = false)
        {
            _supportsEventFiring = supportsEventFiring;
        }


        public static readonly Gen<Operation<IChannelPipeline, PipelineMutationModel>>[] MutationHandlers = {
            AddFirst.AddFirstGen(), AddLast.AddLastGen(),
            RemoveFirst.RemoveFirstGen(), RemoveLast.RemoveLastGen(), ContainsAllModelHandlers.GenContainsAll()
        };

        public static readonly Gen<Operation<IChannelPipeline, PipelineMutationModel>>[] InvocationHandlers =
        {
            InvokeChannelInactive.GenChannelInactive(), InvokeChannelActive.GenChannelActive()
        };

        public static readonly Gen<Operation<IChannelPipeline, PipelineMutationModel>>[] AllHandlers =
            MutationHandlers.Concat(InvocationHandlers).ToArray();

        public override Gen<Operation<IChannelPipeline, PipelineMutationModel>> Next(PipelineMutationModel obj0)
        {
            if(!_supportsEventFiring)
                return Gen.OneOf(MutationHandlers);
            return Gen.OneOf(AllHandlers);
        }

        public override Arbitrary<Setup<IChannelPipeline, PipelineMutationModel>> Setup
            => Arb.From(Gen.Fresh(() => (Setup<IChannelPipeline, PipelineMutationModel>)new PipelineSetup()));

        #region Mutation Commands

        class PipelineSetup : Setup<IChannelPipeline, PipelineMutationModel>
        {
            public override IChannelPipeline Actual()
            {
                return new DefaultChannelPipeline(TestChannel.Instance);
            }

            public override PipelineMutationModel Model()
            {
                return PipelineMutationModel.Fresh();
            }
        }

        abstract class DoAnythingWithHandler : Operation<IChannelPipeline, PipelineMutationModel>
        {
            protected DoAnythingWithHandler()
            {
                ChannelPipelineModel.EventQueue.Clear();
                // always reset the queue
            }
        }

        abstract class AddHandler : DoAnythingWithHandler
        {
            protected readonly string Name;

            protected AddHandler(NamedChannelHandler handler)
            {
                Handler = handler;
                Name = Handler.Name;
            }

            public NamedChannelHandler Handler { get; }

            protected PipelineModelNode NewHandler()
            {

                var node = new PipelineModelNode() { Handler = Handler, Name = Name };
                return node;
            }

            public override bool Pre(PipelineMutationModel _arg1)
            {
                // Can't allow two handlers with the same name to be added
                return !_arg1.Contains(Name);
            }

            public override string ToString()
            {
                return $"{GetType().Name}: {Handler}";
            }


        }

        class AddFirst : AddHandler
        {
            public static Gen<Operation<IChannelPipeline, PipelineMutationModel>> AddFirstGen()
            {
                return NamedChannelHandler.AllHandlers().Select(x => (Operation<IChannelPipeline, PipelineMutationModel>)new AddFirst(x));
            }
            public AddFirst(NamedChannelHandler handler) : base(handler)
            {
            }

            public override Property Check(IChannelPipeline obj0, PipelineMutationModel obj1)
            {
                var pipeline = obj0.AddFirst(Name, Handler);
                var pFirst = pipeline.Skip(1).First(); //bypass the head node
                var mFirst = obj1.Head.Next.Handler;
                var pLength = pipeline.Count();
                return (pFirst == mFirst)
                    .Label($"Expected head of pipeline to be {mFirst}, was {pFirst}")
                    .And(() => pLength == obj1.Length).Label($"Expected length of pipeline to be {obj1.Length}, was {pLength}")
                    .And(() => !mFirst.Event.HasFlag(SupportedEvent.HandlerAdded) || EventQueue.All(x => x.Item1.Equals(mFirst.Name) && x.Item2.HasFlag(SupportedEvent.HandlerAdded)))
                    .Label($"{mFirst} {(mFirst.Event.HasFlag(SupportedEvent.HandlerAdded) ? "does" : "does not")} support {SupportedEvent.HandlerAdded}, but found that queue contained {string.Join(",", EventQueue)}");
            }

            public override PipelineMutationModel Run(PipelineMutationModel obj0)
            {
                var newNode = NewHandler();
                var model = AddToHead(obj0, newNode);
                return model;
            }


        }

        class AddLast : AddHandler
        {
            public static Gen<Operation<IChannelPipeline, PipelineMutationModel>> AddLastGen()
            {
                return NamedChannelHandler.AllHandlers().Select(x => (Operation<IChannelPipeline, PipelineMutationModel>)new AddLast(x));
            }

            public AddLast(NamedChannelHandler handler) : base(handler)
            {
            }

            public override Property Check(IChannelPipeline obj0, PipelineMutationModel obj1)
            {
                var pipeline = obj0.AddLast(Name, Handler);
                var pFirst = pipeline.Reverse().Skip(1).First(); //bypass the head node
                var mFirst = obj1.Tail.Previous.Handler;
                var pLength = pipeline.Count();
                return (pFirst == mFirst)
                    .Label($"Expected tail of pipeline to be {mFirst}, was {pFirst}")
                    .And(() => pLength == obj1.Length).Label($"Expected length of pipeline to be {obj1.Length}, was {pLength}")
                    .And(() => !mFirst.Event.HasFlag(SupportedEvent.HandlerAdded) || EventQueue.All(x => x.Item1.Equals(mFirst.Name) && x.Item2.HasFlag(SupportedEvent.HandlerAdded)))
                    .Label($"{mFirst} {(mFirst.Event.HasFlag(SupportedEvent.HandlerAdded) ? "does" : "does not")} support {SupportedEvent.HandlerAdded}, but found that queue contained {string.Join(",", EventQueue)}"); ;
            }

            public override PipelineMutationModel Run(PipelineMutationModel obj0)
            {
                var newNode = NewHandler();
                var model = AddToTail(obj0, newNode);
                return model;
            }

        }

        class RemoveFirst : DoAnythingWithHandler
        {
            public static Gen<Operation<IChannelPipeline, PipelineMutationModel>> RemoveFirstGen()
            {
                return Gen.Constant((Operation<IChannelPipeline, PipelineMutationModel>)new RemoveFirst());
            }

            public override Property Check(IChannelPipeline obj0, PipelineMutationModel obj1)
            {
                var pipeline = obj0.RemoveFirst();
                var pFirst = obj0.Skip(1).First(); //bypass the head node
                var mFirst = obj1.Head.Next.Handler;
                var pLength = obj0.Count();
                return (pFirst == mFirst)
                    .Label($"Expected tail of pipeline to be {mFirst}, was {pFirst}")
                    .And(() => pLength == obj1.Length).Label($"Expected length of pipeline to be {obj1.Length}, was {pLength}")
                    .And(() => !mFirst.Event.HasFlag(SupportedEvent.HandlerRemoved) || EventQueue.All(x => x.Item1.Equals(mFirst.Name) && x.Item2.HasFlag(SupportedEvent.HandlerRemoved)))
                    .Label($"{mFirst} {(mFirst.Event.HasFlag(SupportedEvent.HandlerRemoved) ? "does" : "does not")} support {SupportedEvent.HandlerRemoved}, but found that queue contained {string.Join(",", EventQueue)}");
            }

            public override bool Pre(PipelineMutationModel _arg1)
            {
                return _arg1.Length > 3; // need to have at least 3
            }

            public override PipelineMutationModel Run(PipelineMutationModel obj0)
            {
                return RemoveHead(obj0);
            }
        }

        class RemoveLast : DoAnythingWithHandler
        {
            public static Gen<Operation<IChannelPipeline, PipelineMutationModel>> RemoveLastGen()
            {
                return Gen.Constant((Operation<IChannelPipeline, PipelineMutationModel>)new RemoveLast());
            }

            public override Property Check(IChannelPipeline obj0, PipelineMutationModel obj1)
            {
                var pipeline = obj0.RemoveLast();
                var pFirst = obj0.Reverse().Skip(1).First(); //bypass the head node
                var mFirst = obj1.Tail.Previous.Handler;
                var pLength = obj0.Count();
                return (pFirst == mFirst)
                    .Label($"Expected tail of pipeline to be {mFirst}, was {pFirst}")
                    .And(() => pLength == obj1.Length).Label($"Expected length of pipeline to be {obj1.Length}, was {pLength}")
                    .And(() => !mFirst.Event.HasFlag(SupportedEvent.HandlerRemoved) || EventQueue.All(x => x.Item1.Equals(mFirst.Name) && x.Item2.HasFlag(SupportedEvent.HandlerRemoved)))
                    .Label($"{mFirst} {(mFirst.Event.HasFlag(SupportedEvent.HandlerRemoved) ? "does" : "does not")} support {SupportedEvent.HandlerRemoved}, but found that queue contained {string.Join(",", EventQueue)}");
            }

            public override bool Pre(PipelineMutationModel _arg1)
            {
                return _arg1.Length > 3; // need to have at least 4
            }

            public override PipelineMutationModel Run(PipelineMutationModel obj0)
            {
                return RemoveTail(obj0);
            }
        }

        class ContainsAllModelHandlers : DoAnythingWithHandler
        {
            public static Gen<Operation<IChannelPipeline, PipelineMutationModel>> GenContainsAll()
            {
                return Gen.Constant((Operation<IChannelPipeline, PipelineMutationModel>)new ContainsAllModelHandlers());
            }

            public override Property Check(IChannelPipeline obj0, PipelineMutationModel obj1)
            {
                var pipeline = obj0 as DefaultChannelPipeline;
                var head = obj1.Head;
                var next = head.Next;
                while (next != null && next != obj1.Tail)
                {
                    var inPipe = pipeline.Get(next.Name);
                    if (inPipe != next.Handler)
                    {
                        return false.Label($"Expected to find handler {inPipe} in pipeline, but did not");
                    }
                    next = next.Next;

                }
                return true.ToProperty();
            }

            public override PipelineMutationModel Run(PipelineMutationModel obj0)
            {
                return obj0;
            }
        }

        #endregion

        #region ChannelPipeline Invocation Events

        private abstract class ChannelInvocationBaseEvent : DoAnythingWithHandler
        {
            public SupportedEvent Event { get; private set; }

            protected ChannelInvocationBaseEvent(SupportedEvent es)
            {
                Event = es;
            }

            public override Property Check(IChannelPipeline obj0, PipelineMutationModel obj1)
            {
                var model = obj1.PredictedOutcome(Event);
                var actual = Execute(obj0);
                return
                    model.SequenceEqual(actual)
                        .Label($"Expected model ({string.Join(",", model)}) to equal actual ({string.Join(",", actual)})");
            }

            protected Queue<Tuple<string, SupportedEvent>> Execute(IChannelPipeline pipeline)
            {
                ExecuteInternal(pipeline);
                return EventQueue;
            }

            protected abstract void ExecuteInternal(IChannelPipeline pipeline);

            public override PipelineMutationModel Run(PipelineMutationModel obj0)
            {
                // state of the model is not affected
                return obj0;
            }
        }

        private class InvokeChannelActive : ChannelInvocationBaseEvent
        {
            public static Gen<Operation<IChannelPipeline, PipelineMutationModel>> GenChannelActive()
            {
                return Gen.Constant((Operation<IChannelPipeline, PipelineMutationModel>) new InvokeChannelActive());
            }

            public InvokeChannelActive() : base(SupportedEvent.ChannelActive)
            {
            }

            protected override void ExecuteInternal(IChannelPipeline pipeline)
            {
                pipeline.FireChannelActive();
            }
        }

        private class InvokeChannelInactive : ChannelInvocationBaseEvent
        {
            public static Gen<Operation<IChannelPipeline, PipelineMutationModel>> GenChannelInactive()
            {
                return Gen.Constant((Operation<IChannelPipeline, PipelineMutationModel>)new InvokeChannelInactive());
            }

            public InvokeChannelInactive() : base(SupportedEvent.ChannelInactive)
            {
            }

            protected override void ExecuteInternal(IChannelPipeline pipeline)
            {
                pipeline.FireChannelInactive();
            }
        }

        #endregion
    }
}
