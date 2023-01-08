using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TobuAts {
    public class CalculatedLimit {

        public SpeedLimit NextLimit = new SpeedLimit();
        public double CurrentTarget;

        public CalculatedLimit(SpeedLimit limit, double target) {
            this.NextLimit = limit;
            this.CurrentTarget = target;
        }

        public static CalculatedLimit Calculate(double location, double idealdecel, double voffset, params SpeedLimit[] limits) {
            if (limits.Length == 0) return new CalculatedLimit(SpeedLimit.inf, Config.LessInf);
            if (limits.Length == 1) return new CalculatedLimit(limits[0], limits[0].AtLocation(location, idealdecel, voffset));
            Array.Sort(limits, (a, b) => a.Location.CompareTo(b.Location));
            int pointer = 0;
            double currentTarget = Config.LessInf, nextTarget = Config.LessInf;
            while (limits[pointer].Location < location - 10) pointer++;
            for (int i = 0; i < limits.Length; i++) {
                currentTarget = Math.Min(currentTarget, limits[i].AtLocation(location, idealdecel, voffset));
            }
            SpeedLimit nextLimit;
            if (pointer < limits.Length) {
                for (int i = 0; i < limits.Length; i++) {
                    nextTarget = Math.Min(nextTarget, limits[i].AtLocation(limits[pointer].Location, idealdecel, voffset));
                }
                nextLimit = new SpeedLimit(nextTarget, limits[pointer].Location);
            } else {
                nextLimit = SpeedLimit.inf;
            }
            return new CalculatedLimit(nextLimit, currentTarget);
        }
    }
}
