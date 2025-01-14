#if TOOLS
using Godot;
using System;
using System.Threading.Tasks;

[Tool]
[GlobalClass]
public partial class Lib : EditorPlugin {}

public static class Extensions
{
	public static Vector2 Snapped(this Vector2 vector, Vector2 gridSize)
	{
		return new Vector2(
			Mathf.Floor(vector.X / gridSize.X) * gridSize.X,
			Mathf.Floor(vector.Y / gridSize.Y) * gridSize.Y
		);
	}
	
	public static async Task<T> GetNodeAsync<T>(this Node self, string nodeName) where T : class
	{
		var waitTask = Task.Run(async () => {
			while (!self.GetNode(nodeName).IsNodeReady()) await Task.Delay(25);
		});

		if (waitTask != await Task.WhenAny(waitTask, Task.Delay(5000))) {
			throw new TimeoutException();
		}

		return self.GetNode<T>(nodeName);
	}

	public static async Task<Node> GetNodeAsync(this Node self, string nodeName) {
		return await self.GetNodeAsync<Node>(nodeName);
	}
}

#endif
