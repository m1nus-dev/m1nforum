using System;
using System.IO;
using System.Threading.Tasks;

namespace M1nforum.TemplateCompiler
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("html to cs compiler");
			Console.WriteLine("input:  " + args[0]);
			Console.WriteLine("output:  " + args[1]);

			var cs = new Compiler().ParseFile("default", args[0]);
			File.WriteAllText(args[1], cs);
		}
	}
}




