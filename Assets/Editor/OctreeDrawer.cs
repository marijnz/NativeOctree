using NativeOctree;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class OctreeDrawer : EditorWindow
{
	[MenuItem("Window/OctreeDrawer")]
	static void Init()
	{
		GetWindow(typeof(OctreeDrawer)).Show();
	}

	public static void Draw<T>(NativeOctree<T> octree) where T : unmanaged
	{
		OctreeDrawer window = (OctreeDrawer)GetWindow(typeof(OctreeDrawer));
		window.DoDraw(octree, default, default);
	}

	public static void DrawWithResults<T>(OctreeJobs.RangeQueryJob<T> queryJob) where T : unmanaged
	{
		OctreeDrawer window = (OctreeDrawer)GetWindow(typeof(OctreeDrawer));
		window.DoDraw(queryJob);
	}

	[SerializeField]
	Color[][] pixels;

	void DoDraw<T>(NativeOctree<T> octree, NativeList<OctElement<T>> results, AABB bounds) where T : unmanaged
	{
		pixels = new Color[256][];
		for (var i = 0; i < pixels.Length; i++)
		{
			pixels[i] = new Color[256];
		}
		NativeOctree<T>.Draw(octree, results, bounds, pixels);
	}

	void DoDraw<T>(OctreeJobs.RangeQueryJob<T> queryJob) where T : unmanaged
	{
		DoDraw(queryJob.Octree, queryJob.Results, queryJob.Bounds);
	}

	void OnGUI()
	{
		if(pixels != null)
		{
			var texture = new Texture2D(256, 256);
			for (var x = 0; x < pixels.Length; x++)
			{
				for (int y = 0; y < pixels[x].Length; y++)
				{
					texture.SetPixel(x, y, pixels[x][y]);
				}
			}
			texture.Apply();

			GUI.DrawTexture(new Rect(0, 0, position.width, position.height), texture);
		}
	}
}