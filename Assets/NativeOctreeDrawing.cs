using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace NativeOctree
{
	/// <summary>
	/// Editor drawing of the NativeOctree. Currently only drawing x,y (z should also be supported but then a 3d view is needed)
	/// </summary>
	public unsafe partial struct NativeOctree<T> where T : unmanaged
	{
		public static void Draw(NativeOctree<T> tree, NativeList<OctElement<T>> results, AABB range,
			Color[][] texture)
		{
			var widthMult = texture.Length / tree.bounds.Extents.x * 2 / 2 / 2;
			var heightMult = texture[0].Length / tree.bounds.Extents.y * 2 / 2 / 2;

			var widthAdd = tree.bounds.Center.x + tree.bounds.Extents.x;
			var heightAdd = tree.bounds.Center.y + tree.bounds.Extents.y;

			for (int i = 0; i < tree.nodes->Capacity; i++)
			{
				var node = UnsafeUtility.ReadArrayElement<OctNode>(tree.nodes->Ptr, i);

				if(node.count > 0)
				{
					for (int k = 0; k < node.count; k++)
					{
						var element = UnsafeUtility.ReadArrayElement<OctElement<T>>(tree.elements->Ptr, node.firstChildIndex + k);

						DrawPoint(element, Color.red);

						// Draw 2d to texture
						texture[(int) ((element.pos.x + widthAdd) * widthMult)]
							[(int) ((element.pos.y + heightAdd) * heightMult)] = Color.red;
					}
				}
			}

			foreach (var element in results) {
				DrawPoint(element, Color.green);
				texture[(int) ((element.pos.x + widthAdd) * widthMult)]
					[(int) ((element.pos.y + heightAdd) * heightMult)] = Color.green;
			}

			DrawBounds(texture, range, tree);
		}

		private static void DrawPoint(OctElement<T> element, Color color) {
			Debug.DrawLine(element.pos + (float3) Vector3.left, element.pos + (float3) Vector3.right,color, 15f);
			Debug.DrawLine(element.pos + (float3) Vector3.up, element.pos + (float3) Vector3.down, color, 15f);
			Debug.DrawLine(element.pos + (float3) Vector3.back, element.pos + (float3) Vector3.forward, color, 15f);
		}

		static void DrawBounds(Color[][] texture, AABB bounds, NativeOctree<T> tree)
		{
			var widthMult = texture.Length / tree.bounds.Extents.x * 2 / 2 / 2;
			var heightMult = texture[0].Length / tree.bounds.Extents.y * 2 / 2 / 2;

			var widthAdd = tree.bounds.Center.x + tree.bounds.Extents.x;
			var heightAdd = tree.bounds.Center.y + tree.bounds.Extents.y;

			var top = new float2(bounds.Center.x, bounds.Center.y - bounds.Extents.y);
			var left = new float2(bounds.Center.x - bounds.Extents.x, bounds.Center.y);

			for (int leftToRight = 0; leftToRight < bounds.Extents.x * 2; leftToRight++)
			{
				var poxX = left.x + leftToRight;
				texture[(int) ((poxX + widthAdd) * widthMult)][(int) ((bounds.Center.y + heightAdd + bounds.Extents.y) * heightMult)] = Color.blue;
				texture[(int) ((poxX + widthAdd) * widthMult)][(int) ((bounds.Center.y + heightAdd - bounds.Extents.y) * heightMult)] = Color.blue;
			}

			for (int topToBottom = 0; topToBottom < bounds.Extents.y * 2; topToBottom++)
			{
				var posY = top.y + topToBottom;
				texture[(int) ((bounds.Center.x + widthAdd + bounds.Extents.x) * widthMult)][(int) ((posY + heightAdd) * heightMult)] = Color.blue;
				texture[(int) ((bounds.Center.x + widthAdd - bounds.Extents.x) * widthMult)][(int) ((posY + heightAdd) * heightMult)] = Color.blue;
				
			
			}
		}
	}
}