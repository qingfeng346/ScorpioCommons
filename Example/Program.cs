using System;
using Scorpio.Commons;

namespace Example {
    class Program {
        static void Main(string[] args) {
            //Console.WriteLine(ScorpioUtil.RequestString("https://qingfeng346.gitee.io/spm/sco/package.json"));
            //FileUtil.SyncFolder(@"C:\Users\qingf\Desktop\test\aaa\", @"C:\Users\qingf\Desktop\test\bbb", null, true);
            //var perform = new Perform();
            //perform.Help = "fewafawefaewfawefawe";
            //perform.AddExecute("123123", "help", null);
            //perform.Start(args);
            //Console.WriteLine(ScorpioUtil.CurrentDirectory);
            //Console.WriteLine(ScorpioUtil.BaseDirectory);
            //Console.WriteLine(ScorpioUtil.StartProcess("cmd", null, new[] { "/c", "test.bat", "11 11", "22 22" }));
            long a = 123123123;
            Console.WriteLine(a.GetMemory());
        }
    }
}
