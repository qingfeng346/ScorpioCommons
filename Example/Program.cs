using System;
using Scorpio.Commons;

namespace Example {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(FileUtil.RemoveExtension("aaaa/bc.cbb\\ccc"));
            //Console.WriteLine(ScorpioUtil.RequestString("https://qingfeng346.gitee.io/spm/sco/package.json"));
            //FileUtil.SyncFolder(@"C:\Users\qingf\Desktop\test\aaa\", @"C:\Users\qingf\Desktop\test\bbb", null, true);
            var perform = new Perform();
            //perform.AddExecute("test", "测试函数", test);
            // perform.AddExecute("Test2", "测试函数", update);
            // perform.AddExecute("Test3", "测试函数", update);
            // perform.AddExecute("Test4", "测试函数", update);
            //perform.Start(args);
            //perform.Help = "fewafawefaewfawefawe";
            //perform.AddExecute("123123", "help", null);
            //perform.Start(args);
            //Console.WriteLine(ScorpioUtil.CurrentDirectory);
            //Console.WriteLine(ScorpioUtil.BaseDirectory);
            //Console.WriteLine(ScorpioUtil.StartProcess("cmd", null, new[] { "/c", "test.bat", "11 11", "22 22" }));
            //long a = 123123123;
            //Console.WriteLine(a.GetMemory());
        }
        static void update([ParamterInfo(label = "label", required = false, def = "123")] string gsergserg,
                           [ParamterInfo(label = "label", def = "123")] string fawefawefwef, int c = 200) {
            //Console.WriteLine(a);
            //Console.WriteLine(b);
            //Console.WriteLine(c);
        }
        //static void test([ParamterInfo(label = "label", param = new[]{"-aa", "-bb", "--aa"},  required = false)] string[] strs,
        //                   [ParamterInfo(label = "label")] int[] ints, int c = 200) {
        //    //Console.WriteLine(a);
        //    //Console.WriteLine(b);
        //    //Console.WriteLine(c);
        //    int a = 0;
        //}
    }
}
