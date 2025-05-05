using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Godot;
using Godot.Collections;

namespace Appox
{

	#region ShyUtil
	public static class ShyUtil {
		public static Variant BlankVariant = new Variant();

		public static (TValue, int)[] Enumerate<[MustBeVariant] TValue>(Array<TValue> arr)
		{
			return [.. arr.ToArray().Select((value, i) => ( value, i ))];
		}
	}
	#endregion


	#region Jsonc
	public static partial class Jsonc {

		public static string Minify(string jsonc)
		{
			return LineCommentGuh().Replace(BlockCommentGuh().Replace(jsonc, ""), "");
		}


		[GeneratedRegex(@"//.*?$", RegexOptions.Multiline)]
		private static partial Regex LineCommentGuh();


		[GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
		private static partial Regex BlockCommentGuh();
	}
	#endregion


	#region MiniCallableCache
	public static class MiniCallableCache {
		static public Dictionary<string, Callable> Cache = new Dictionary<string, Callable>() {
			["test"] = Callable.From( () => {
				
			})
		};
	}
	#endregion

}
