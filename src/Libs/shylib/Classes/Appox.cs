using System.Linq;
using Godot;
using Godot.Collections;

namespace Appox
{

    public static class ShyUtil {
        public static Variant BlankVariant = new Variant();

        public static (TValue, int)[] Enumerate<[MustBeVariant] TValue>(Array<TValue> arr)
        {
            return [.. arr.ToArray().Select((value, i) => ( value, i ))];
        }
    }

    public static class MiniCallableCache {
        static public Dictionary<string, Callable> Cache = new Dictionary<string, Callable>() {
            ["test"] = Callable.From( () => {
                
            })
        };
    }
}