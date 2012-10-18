﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Threading;


namespace ParallelTasks
{
    class Worker
    {
        Thread thread;
        Deque<Task> tasks;
        WorkStealingScheduler scheduler;

        public bool LookingForWork { get; private set; }
        public AutoResetEvent Gate { get; private set; }

#if WINDOWS_PHONE
        // Cannot access Environment.ProcessorCount in phone app. (Security issue).
        static Hashtable<Thread, Worker> workers = new Hashtable<Thread, Worker>(1);
#else
        static Hashtable<Thread, Worker> workers = new Hashtable<Thread, Worker>(Environment.ProcessorCount);
#endif
        public static Worker CurrentWorker
        {
            get
            {
                var currentThread = Thread.CurrentThread;
                Worker worker;
                if (workers.TryGet(currentThread, out worker))
                    return worker;
                return null;
            }
        }

#if XBOX
        static int affinityIndex;
#endif

        public Worker(WorkStealingScheduler scheduler, int index)
        {
            this.thread = new Thread(Work);
            this.thread.Name = "ParallelTasks Worker " + index;
            this.thread.IsBackground = true;
            this.tasks = new Deque<Task>();
            this.scheduler = scheduler;
            this.Gate = new AutoResetEvent(false);

            workers.Add(thread, this);
        }

        public void Start()
        {
            thread.Start();
        }

        public void AddWork(Task task)
        {
            tasks.LocalPush(task);
        }

        private void Work()
        {
#if XBOX
            int i = Interlocked.Increment(ref affinityIndex) - 1;
            int affinity = Parallel.ProcessorAffinity[i % Parallel.ProcessorAffinity.Length];
            Thread.CurrentThread.SetProcessorAffinity((int)affinity);
#endif

            Task task;
            while (true)
            {
                FindWork(out task);
                task.DoWork();

                //if (tasks.LocalPop(ref task))
                //{
                //    task.DoWork();
                //}
                //else
                //    FindWork();
            }
        }

        private void FindWork(out Task task)
        {
            bool foundWork = false;
            task = default(Task);

            do
            {
                // check our local queue for work
                if (tasks.LocalPop(ref task))
                    break;

                // check the global queue for work
                if (scheduler.TryGetTask(out task))
                    break;

                // look for any replicable tasks
                var replicable = WorkItem.Replicable;
                if (replicable.HasValue)
                {
                    replicable.Value.DoWork();
                    WorkItem.SetReplicableNull(replicable);

                    // MartinG@DigitalRune: Continue checking local queue and replicables. 
                    // No need to steal work yet.
                    continue;
                }

                // try to steal work off other workers
                for (int i = 0; i < scheduler.Workers.Count; i++)
                {
                    var worker = scheduler.Workers[i];
                    if (worker == this)
                        continue;

                    if (worker.tasks.TrySteal(ref task))
                    {
                        foundWork = true;
                        break;
                    }
                }

                // Wait until a new task gets scheduled.
                if (!foundWork)
                    Gate.WaitOne();

            } while (!foundWork);
        }
    }
}
