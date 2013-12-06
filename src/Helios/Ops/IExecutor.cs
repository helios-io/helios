using System;
using System.Collections.Generic;

namespace Helios.Ops
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

        /// <summary>
        /// Process a queue of tasks - if the IExecutor is shut down before 
        /// it has a chance to complete its queue, all of the remaining jobs
        /// will be passed to an optional callback <see cref="remainingOps"/>
        /// </summary>
        /// <param name="ops">The queue of actions to execute</param>
        /// <param name="remainingOps">OPTIONAL. Can be null. Callback function for placing any jobs that couldn't be run
        /// due to an exception or shutdown.</param>
        void Execute(IList<Action> ops, Action<IEnumerable<Action>> remainingOps);

        /// <summary>
        /// Immediate shutdown
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Shut down tasks within the allotted time
        /// </summary>
        /// <param name="gracePeriod">The amount of time left to process tasks before forcibly killing the executor</param>
        void Shutdown(TimeSpan gracePeriod);
    }
}
