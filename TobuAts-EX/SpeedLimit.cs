using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TobuAts_EX {
    public class SpeedLimit {
        public double Limit;
        public double Location;

        public static SpeedLimit inf = new SpeedLimit(Config.LessInf, Config.LessInf);

        public SpeedLimit() { Copy(inf); }

        public SpeedLimit(double limit, double locationStart) {
            this.Limit = limit;
            this.Location = locationStart;
        }

        public virtual double AtLocation(double location, double idealDecel, double voffset = 0) {
            var offsetLimit = Math.Max(0, Limit + voffset);
            if (location >= Location) {
                return offsetLimit;
            } else {
                double dat = (offsetLimit * 1000 / 3600) * (offsetLimit * 1000 / 3600) 
                    - 2 * idealDecel * 1000 / 3600 * (Location - location);
                if (dat > 0) {
                    return Math.Sqrt(dat) * 3600 / 1000;
                } else {
                    return offsetLimit;
                }
            }
        }

        public override bool Equals(object obj) {
            return (obj is SpeedLimit) && (this.Limit == ((SpeedLimit)obj).Limit) && (this.Location == ((SpeedLimit)obj).Location);
        }

        public override int GetHashCode() {
            return Convert.ToInt32(this.Location * 100 + this.Limit);
        }

        public override string ToString() {
            return this.Limit + ">" + this.Location;
        }

        public void Copy(SpeedLimit limit) {
            this.Limit = limit.Limit;
            this.Location = limit.Location;
        }

    }
}
