using System;
namespace Scorpio.Commons {
    /// <summary> 随机工具类 </summary>
    public static class RandomUtil {
        private static Random random = new Random(Environment.TickCount);
        /// <summary> 设置随机种子 </summary>
        public static int Seed { set { random = new Random(value); } }
        /// <summary> 随机一个数（包含最大最小值） </summary>
        public static int RangeRandomInt(int low, int high) {
            if (low == high)
                return high;
            if (low > high) {
                logger.warn("RangeRandomInt low > hight  low:" + low + " hight:" + high);
                return high;
            }
            return random.Next() % (high - low + 1) + low;
        }
        /// <summary> 随机一个数（包含最大最小值） </summary>
        public static long RangeRandomLong(long low, long high) {
            if (low == high)
                return high;
            if (low > high) {
                logger.warn("RangeRandomLong low > hight  low:" + low + " hight:" + high);
                return high;
            }
            return random.Next() % (high - low + 1) + low;
        }
        /// <summary> 随机一个数 </summary>
        public static float RangeRandomFloat(float low, float high) {
            if (low == high)
                return high;
            if (low > high) {
                logger.warn("RangeRandomFloat low > hight  low:" + low + " hight:" + high);
                return high;
            }
            return Convert.ToSingle(random.NextDouble()) * (high - low) + low;
        }
        /// <summary> 随机一个数 </summary>
        public static double RangeRandomDouble(double low, double high) {
            if (low == high)
                return high;
            if (low > high) {
                logger.warn("RangeRandomDouble low > hight  low:" + low + " hight:" + high);
                return high;
            }
            return random.NextDouble() * (high - low) + low;
        }
        /// <summary> 随机一个权重值 返回权重的索引 </summary>
        public static int RandomWeightArray(int[] weights) {
            if (weights.Length == 0) return -1;
            if (weights.Length == 1) return 0;
            int maxWeight = 0;
            for (int i = 0; i < weights.Length; ++i)
                maxWeight += weights[i];
            int value = RangeRandomInt(1, maxWeight);
            int weight = 0;
            for (int i = 0; i < weights.Length; ++i) {
                weight += weights[i];
                if (value <= weight)
                    return i;
            }
            return -1;
        }
        public static int RandomWeight(params int[] weights) {
            return RandomWeightArray(weights);
        }
    }
}