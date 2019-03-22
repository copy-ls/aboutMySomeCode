using System;
using System.Net;

namespace swicthTest
{
    class Program
    {
        class Base
        {
            public string name;
            public Base(string name)
            {
                this.name = name;
            }
        }

        class A: Base
        {
            public A(string name) : base(name) { }
        }

        class B: Base
        {
            public B(string name): base(name) { }
        }

        static void SwitchTest(object o)
        {
            switch (o)
            {
                case int a:
                    Console.WriteLine(a);
                    break;
                case A a:
                    Console.WriteLine(a.name);
                    break;
                case B b:
                    Console.WriteLine(b.name);
                    break;
                default:
                    return;
            }
        }

        static void Main(string[] args)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry("t1.yeahsen.com");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            SwitchTest(1);
            SwitchTest(new A("a"));
            SwitchTest(new B("b"));
        }
    }
}
