using System.IO;
using System.Threading.Tasks;

namespace M1nforum.TemplateCompiler
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var cs = new Compiler().ParseFile("default", args[0]);
			File.WriteAllText(args[1], cs);
		}
	}
}




