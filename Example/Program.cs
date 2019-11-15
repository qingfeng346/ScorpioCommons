using System;
using Scorpio.Commons;

namespace Example {
    class Program {
        static void Main(string[] args) {
            var perform = new Perform();
            perform.Help = "fewafawefaewfawefawe";
            perform.AddExecute("123123", "help", null);
            perform.Start(args);
        }
    }
}
