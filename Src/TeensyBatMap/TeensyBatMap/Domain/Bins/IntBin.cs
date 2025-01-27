using System;

namespace TeensyBatMap.Domain.Bins
{
    public class IntBin : Bin<BatCall>
    {
        public IntBin(string label, Func<BatCall, bool> filter) : base(filter)
        {
            Label = label;
        }
    }

	public class UintBin : Bin<BatCall>
    {
        public UintBin(string label, Func<BatCall, bool> filter) : base(filter)
        {
            Label = label;
        }
    }
}