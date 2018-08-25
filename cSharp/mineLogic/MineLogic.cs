using System;
using ETModel;
namespace Minesweeper {
    public class MineLogic {

        private static MineLogic instance;

        private readonly Int32 mine = Int32.MaxValue;

        public static MineLogic Instance {
            get {
                if (instance == null) {
                    return new MineLogic();
                }
                return instance;
            }
        }
        /// <summary>
        /// 生成扫雷表
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="sweeperCount">雷的数量</param>
        /// <returns></returns>
        public Int32[,] GenerateMineMap(Int32 width, Int32 height, Int32 sweeperCount) {
            Int32 widthRandom;
            Int32 heightRandom;
            if(width*height < sweeperCount) {
                return null;
            }
            Int32[,] mineMap = new Int32[width, height];
            while (0 < sweeperCount) {
                widthRandom = RandomHelper.RandomNumber(0, width);
                heightRandom = RandomHelper.RandomNumber(0, height);
                if(mineMap[widthRandom,heightRandom] != this.mine) {
                    --sweeperCount;
                    for (Int32 i = -1; i < 2; ++i) {
                        for (Int32 r = -1; r < 2; ++r) {
                            Int32 w = widthRandom + i;
                            Int32 h = heightRandom + r;
                            if (w < 0 || width <= w ||
                                h < 0 || height <= h ||
                                (i == 0 && r == 0) || 
                                mineMap[w,h] == this.mine) {
                                continue;
                            }
                            ++mineMap[w, h];
                        }
                    }
                }
            }
            return mineMap;
        }
    }
}