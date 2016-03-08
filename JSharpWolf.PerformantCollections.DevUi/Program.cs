using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace JSharpWolf.PerformantCollections.DevUi
{
    class Program
    {
        static Random _rnd = new Random();
        static void Main(string[] args)
        {
            ExtendableDictTest();
            Console.ReadLine();
        }

        static void ExtendableDictTest()
        {
            var x = new SkipList<int, int>();
            var cd = new SortedDictionary<int,int>();
            var rnd = new Random();
            var hs = new HashSet<int>();
            while (hs.Count < 1000000)
            {
                var num = rnd.Next(1, 50000000);
                if (hs.Contains(num)) continue;
                hs.Add(num);
            }
            //foreach (var i in hs)
            //{ 
            //    x.TryAdd(i, i + 1);
            //}

            Parallel.ForEach(hs, (i) =>
            {
                var z = x.TryAdd(i, i + 1);
                Debug.Assert(z);
            });
        }
        static void FlBenchmark()
        {

            Run(1000, 1024);
            Console.WriteLine("1000");
            WriteResults(Run(1000, 8192));
            Console.WriteLine("10000");
            WriteResults(Run(10000, 8192));
            Console.WriteLine("100000");
            WriteResults(Run(100000, 8192));
            Console.WriteLine("1000000");
            WriteResults(Run(1000000, 8192));
            Console.WriteLine("10000000");
            WriteResults(Run(10000000, 8192));
            Console.WriteLine("100000000");
            WriteResults(Run(100000000, 8192));
            Console.WriteLine("200000000");
            WriteResults(Run(200000000, 8192));
            Console.WriteLine("1000000000");
            WriteResults(Run(1000000000, 8192, false));
            Console.WriteLine("2000000000");
            WriteResults(Run(2000000000, 8192, false));
            Console.ReadLine();
        }
        static void WriteResults(Tuple<long, long, long> r)
        {
            Console.WriteLine("-----------------------");
            Console.WriteLine($"List      : {r.Item1,0:F1}");
            Console.WriteLine($"Fragmented: {r.Item2,0:F1}");
            Console.WriteLine($"Linked    : {r.Item3,0:F1}");
        }

        static Tuple<long, long, long> Run(int sampleSize, int fragSize, bool run1 = true)
        {
            long m1, m2, m3;
            var sw = new Stopwatch();

            sw.Start();
            var l = new List<int>();
            if (run1)
                for (var i = 0; i < sampleSize; ++i)
                {
                    l.Add(i);
                }
            sw.Stop();
            m1 = sw.ElapsedMilliseconds;
            var fl = new FragmentedList<int>(fragSize);
            sw.Restart();
            for (var i = 0; i < sampleSize; ++i)
            {
                fl.Add(i);
            }
            sw.Stop();
            m2 = sw.ElapsedMilliseconds;
            var ll = new LinkedList<int>();
            sw.Restart();
            for (var i = 0; i < sampleSize; ++i)
            {
                ll.AddLast(i);
            }
            sw.Stop();
            m3 = sw.ElapsedMilliseconds;
            var r = _rnd.Next(0, sampleSize);
            if (run1 && sampleSize > r)
                if (l[r] != fl[r])
                    throw new Exception();
            return Tuple.Create(m1, m2, m3);
        }

    }
}
