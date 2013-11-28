using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Helios.Core.Ops
{
    /// <summary>
    /// Interface used for executing commands and actions - represents
    /// the lowest possible unit of work
    /// </summary>
    public interface IExecutor
    {
        bool AcceptingJobs { get; }

        void Execute(Action op);

        void Execute(IList<Action> op);

        void Execute(IList<Action> ops, out IList<Action> remainingOps);

        void Shutdown();
    }
}
