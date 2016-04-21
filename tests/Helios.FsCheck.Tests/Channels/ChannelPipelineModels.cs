using System;
using System.Linq;
using FsCheck;
using FsCheck.Experimental;
using Helios.Channels;

namespace Helios.FsCheck.Tests.Channels
{
    public class PipelineModelNode
    {
        public string Name;
        public IChannelHandler Handler;
        public PipelineModelNode Previous;
        public PipelineModelNode Next;

        public override string ToString()
        {
            return $"{Handler}";
        }

        public PipelineModelNode Clone()
        {
            return new PipelineModelNode() { Handler = Handler, Next = Next, Name = Name, Previous = Previous};
        }
    }

    public class PipelineModel
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

        public PipelineModel Clone()
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
            return new PipelineModel() {Head = newHead, Length = Length, Tail = current};
        }

        public static PipelineModel Fresh()
        {
            var head = new PipelineModelNode() { Handler = new NamedChannelHandler("HEAD"), Name = "HEAD" };
            var tail = new PipelineModelNode() { Handler = new NamedChannelHandler("TAIL"), Name = "TAIL" };
            head.Next = tail;
            tail.Previous = head;
            return new PipelineModel() { Length = 2, Head = head, Tail = tail };
        }
    }

    public class NamedChannelHandler : ChannelHandlerAdapter
    {
        public NamedChannelHandler(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override string ToString()
        {
            return $"NamedChannelHandler(Name={Name})";
        }
    }

    public class ChannelPipelineStateMachine : Machine<IChannelPipeline, PipelineModel>
    {
        public static PipelineModel AddToHead(PipelineModel obj0, PipelineModelNode newNode)
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

        public static PipelineModel AddToTail(PipelineModel obj0, PipelineModelNode newNode)
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

        public override Gen<Operation<IChannelPipeline, PipelineModel>> Next(PipelineModel obj0)
        {
            return Gen.OneOf(AddFirst.AddFirstGen(), AddLast.AddLastGen());
        }

        public override Arbitrary<Setup<IChannelPipeline, PipelineModel>> Setup
            => Arb.From(Gen.Fresh(() => (Setup<IChannelPipeline, PipelineModel>) new PipelineSetup()));

        #region Commands

        class PipelineSetup : Setup<IChannelPipeline, PipelineModel>
        {
            public override IChannelPipeline Actual()
            {
                return new DefaultChannelPipeline(TestChannel.Instance);
            }

            public override PipelineModel Model()
            {
                return PipelineModel.Fresh();
            }
        }

        abstract class AddHandler : Operation<IChannelPipeline, PipelineModel>
        {
            protected readonly string Name;

            protected AddHandler(string name)
            {
                Name = name;
            }

            private IChannelHandler _handler;

            public IChannelHandler Handler
            {
                get
                {
                    if (_handler == null)
                    {
                        _handler = new NamedChannelHandler(Name);
                    }
                    return _handler;
                }
            }

            protected PipelineModelNode NewHandler()
            {
                
                var node = new PipelineModelNode() {Handler = Handler, Name = Name};
                return node;
            }

            public override bool Pre(PipelineModel _arg1)
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
            public static Gen<Operation<IChannelPipeline, PipelineModel>> AddFirstGen()
            {
                return Arb.Default.Char().Generator.ArrayOf(30).Select(x => (Operation<IChannelPipeline, PipelineModel>)new AddFirst(new string(x)));
            }

            public AddFirst(string name) : base(name)
            {
            }

            public override Property Check(IChannelPipeline obj0, PipelineModel obj1)
            {
                var pipeline = obj0.AddFirst(Name, Handler);
                var pFirst = pipeline.Skip(1).First(); //bypass the head node
                var mFirst = obj1.Head.Next.Handler;
                var pLength = pipeline.Count();
                return (pFirst == mFirst)
                    .Label($"Expected head of pipeline to be {mFirst}, was {pFirst}")
                    .And(() => pLength == obj1.Length).Label($"Expected length of pipeline to be {obj1.Length}, was {pLength}");
            }

            public override PipelineModel Run(PipelineModel obj0)
            {
                var newNode = NewHandler();
                var model = AddToHead(obj0, newNode);
                return model;
            }
        }

        class AddLast : AddHandler
        {
            public static Gen<Operation<IChannelPipeline, PipelineModel>> AddLastGen()
            {
                return Arb.Default.Char().Generator.ArrayOf(30).Select(x => (Operation<IChannelPipeline, PipelineModel>)new AddLast(new string(x)));
            }

            public AddLast(string name) : base(name)
            {
            }

            public override Property Check(IChannelPipeline obj0, PipelineModel obj1)
            {
                var pipeline = obj0.AddLast(Name, Handler);
                var pFirst = pipeline.Reverse().Skip(1).First(); //bypass the head node
                var mFirst = obj1.Tail.Previous.Handler;
                var pLength = pipeline.Count();
                return (pFirst == mFirst)
                    .Label($"Expected tail of pipeline to be {mFirst}, was {pFirst}")
                    .And(() => pLength == obj1.Length).Label($"Expected length of pipeline to be {obj1.Length}, was {pLength}");
            }

            public override PipelineModel Run(PipelineModel obj0)
            {
                var newNode = NewHandler();
                var model = AddToTail(obj0, newNode);
                return model;
            }
        }

        #endregion
    }
}
