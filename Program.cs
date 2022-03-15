using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AnimCopy
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() < 4)
                Console.WriteLine("Usage: animcopy source_zobj zobj_to_append output_zobj [0xanimation_header_offset...]");

            byte[] inf = File.ReadAllBytes(args[0]);
            byte[] outf = File.ReadAllBytes(args[1]);
            List<byte> outff = outf.ToList();

            for (int i = 3; i < args.Count(); i++)
            {
                int oAnimOffs = Convert.ToInt32(args[i], 16);

                byte[] Header = inf.Skip(oAnimOffs).Take(16).ToArray();

                Int16 FrameCount = BitConverter.ToInt16(Header.Take(2).Reverse().ToArray());
                Int32 RotOffs = BitConverter.ToInt32(Header.Skip(4).Take(4).Reverse().ToArray());
                Int32 Index = BitConverter.ToInt32(Header.Skip(8).Take(4).Reverse().ToArray());
                Int16 Limit = BitConverter.ToInt16(Header.Skip(12).Take(2).Reverse().ToArray());

                RotOffs &= 0x00FFFFFF;
                Index &= 0x00FFFFFF;

                byte[] Rots = inf.Skip(RotOffs).Take(Index - RotOffs).ToArray();
                byte[] Indexes = inf.Skip(Index).Take(oAnimOffs - Index).ToArray();

                Int32 OffsetRotsNew = outff.Count();
                outff.AddRange(Rots);
                Int32 OffsetIndexesNew = outff.Count();
                outff.AddRange(Indexes);
                Int32 NewAnimOffset = outff.Count();

                outff.AddRange(BitConverter.GetBytes(FrameCount).ToArray().Reverse());
                outff.Add(0);
                outff.Add(0);
                outff.AddRange(BitConverter.GetBytes(0x06000000 + OffsetRotsNew).ToArray().Reverse());
                outff.AddRange(BitConverter.GetBytes(0x06000000 + OffsetIndexesNew).ToArray().Reverse());
                outff.AddRange(BitConverter.GetBytes(Limit).ToArray().Reverse());
                outff.Add(0);
                outff.Add(0);
                outff.Add(0);
                outff.Add(0);
                outff.Add(0);
                outff.Add(0);

                Console.WriteLine($"Wrote animation that used to be at {args[i]} to: {NewAnimOffset.ToString("X")}");
            }

            File.WriteAllBytes(args[2], outff.ToArray());


        }
    }
}
