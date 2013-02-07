#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Threading;

namespace Lokad.Cqrs.Partition
{
    /// <summary>
    /// Polling file-based implementation of <see cref="IQueueReader"/>
    /// </summary>
    public sealed class FileQueueReader : IQueueReader
    {
        readonly StatelessFileQueueReader[] _readers;
        readonly Func<uint, TimeSpan> _waiter;
        uint _emptyCycles;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileQueueReader"/> class.
        /// </summary>
        /// <param name="readers">Multiple file queue readers.</param>
        /// <param name="waiter">The waiter function (to slow down polling if there are no messages).</param>
        public FileQueueReader(StatelessFileQueueReader[] readers, Func<uint, TimeSpan> waiter)
        {
            _readers = readers;
            _waiter = waiter;
        }


        public void Init()
        {
         
        }

        public void InitIfNeeded()
        {
            foreach (var info in _readers)
            {
                info.InitIfNeeded();
            }
        }

        public void AckMessage(MessageTransportContext message)
        {
            if (message == null) throw new ArgumentNullException("message");
            foreach (var queue in _readers)
            {
                if (queue.Name == message.QueueName)
                {
                    queue.AckMessage(message);
                }
            }
        }

        public bool TakeMessage(CancellationToken token, out MessageTransportContext context)
        {
            while (!token.IsCancellationRequested)
            {
                for (var i = 0; i < _readers.Length; i++)
                {
                    var queue = _readers[i];

                    var message = queue.TryGetMessage();
                    switch (message.State)
                    {
                        case GetEnvelopeResultState.Success:

                            _emptyCycles = 0;
                            context = message.Message;
                            return true;
                        case GetEnvelopeResultState.Empty:
                            _emptyCycles += 1;
                            break;
                        case GetEnvelopeResultState.Exception:
                            // access problem, fall back a bit
                            break;
                        case GetEnvelopeResultState.Retry:
                            // this could be the poison
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    var waiting = _waiter(_emptyCycles);
                    token.WaitHandle.WaitOne(waiting);
                }
            }
            context = null;
            return false;
        }

        public void TryNotifyNack(MessageTransportContext context) {}
    }
}