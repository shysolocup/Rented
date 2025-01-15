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
		int elapsed = 0;
		Node node = self.GetNode(nodeName);
		
		while (!node.IsNodeReady()) {
			await Task.Delay(25);
			elapsed += 25;
			
			if (elapsed >= 5000) {
				throw new TimeoutException();
			}
		}

		return self.GetNode<T>(nodeName);
	}

	public static async Task<Node> GetNodeAsync(this Node self, string nodeName) {
		return await self.GetNodeAsync<Node>(nodeName);
	}

	public static async Task WaitUntilNodeReady(this Node self, string nodeName)
	{
		int elapsed = 0;
		Node node = self.GetNode(nodeName);
		
		while (!node.IsNodeReady()) {
			await Task.Delay(25);
			elapsed += 25;
			
			if (elapsed >= 5000) {
				throw new TimeoutException();
			}
		}
	}

	public async static Task WaitUntilPropertyReady(this Node self, string propertyName)
	{
		int elapsed = 0;
		
		while ((object)self.Get(propertyName) == null) {
			await Task.Delay(25);
			elapsed += 25;
			
			if (elapsed >= 5000) {
				throw new TimeoutException();
			}
		}
	}
}

#endif
