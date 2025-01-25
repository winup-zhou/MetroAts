using System;



namespace TobuSignal {
    public class SpeedPattern {
        public double TargetSpeed;
        public double Location;
        public double MaxSpeed;

        public static SpeedPattern inf = new SpeedPattern(Config.LessInf, Config.LessInf, Config.LessInf);

        public SpeedPattern() { Copy(inf); }

        public SpeedPattern(double targetSpeed, double locationStart, double MaxSpeed = Config.LessInf) {
            this.TargetSpeed = targetSpeed;
            this.Location = locationStart;
            this.MaxSpeed = MaxSpeed;
        }

        public virtual double AtLocation(double location, double idealDecel, double voffset = 0) {
            var offsetLimit = Math.Max(0, TargetSpeed + voffset);
            if (location >= Location) {
                return offsetLimit;
            } else {
                double dat = (offsetLimit * 1000 / 3600) * (offsetLimit * 1000 / 3600)
                    - 2 * idealDecel * 1000 / 3600 * (Location - location);
                if (dat > 0) {
                    return Math.Min(Math.Sqrt(dat) * 3600 / 1000, MaxSpeed);
                } else {
                    return Math.Min(offsetLimit, MaxSpeed);
                }
            }
        }

        public override bool Equals(object obj) {
            return (obj is SpeedPattern) && (this.TargetSpeed == ((SpeedPattern)obj).TargetSpeed) &&
                (this.Location == ((SpeedPattern)obj).Location);
        }

        public override int GetHashCode() {
            return Convert.ToInt32(this.Location * 100 + this.TargetSpeed);
        }

        public override string ToString() {
            return this.TargetSpeed + "-" + this.Location;
        }

        public void Copy(SpeedPattern pattern) {
            this.TargetSpeed = pattern.TargetSpeed;
            this.Location = pattern.Location;
            this.MaxSpeed = pattern.MaxSpeed;
        }

    }
}
