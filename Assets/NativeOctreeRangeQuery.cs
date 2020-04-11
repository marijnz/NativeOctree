using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace NativeOctree
{
	public unsafe partial struct NativeOctree<T> where T : unmanaged
	{
		struct OctreeRangeQuery
		{
			NativeOctree<T> tree;

			UnsafeList* fastResults;
			int count;

			AABB bounds;

			public void Query(NativeOctree<T> tree, AABB bounds, NativeList<OctElement<T>> results)
			{
				this.tree = tree;
				this.bounds = bounds;
				count = 0;

				// Get pointer to inner list data for faster writing
				fastResults = (UnsafeList*) NativeListUnsafeUtility.GetInternalListDataPtrUnchecked(ref results);

				RecursiveRangeQuery(tree.bounds, false, 1, 1);

				fastResults->Length = count;
			}

			public void RecursiveRangeQuery(AABB parentBounds, bool parentContained, int prevOffset, int depth)
			{
				if(count + 8 * tree.maxLeafElements > fastResults->Capacity)
				{
					fastResults->Resize<OctElement<T>>(math.max(fastResults->Capacity * 2, count + 8 * tree.maxLeafElements));
				}

				var depthSize = LookupTables.DepthSizeLookup[tree.maxDepth - depth+1];
 				for (int l = 0; l < 8; l++)
				{
					var childBounds = GetChildBounds(parentBounds, l);

					var contained = parentContained;
					if(!contained)
					{
						if(bounds.Contains(childBounds))
						{
							contained = true;
						}
						else if(!Intersects(bounds, childBounds))
						{
							continue;
						}
					}

					var at = prevOffset + l * depthSize;

					var elementCount = UnsafeUtility.ReadArrayElement<int>(tree.lookup->Ptr, at);

					if(elementCount > tree.maxLeafElements && depth < tree.maxDepth)
					{
						RecursiveRangeQuery(childBounds, contained, at+1, depth+1);
					}
					else if(elementCount != 0)
					{
						var node = UnsafeUtility.ReadArrayElement<OctNode>(tree.nodes->Ptr, at);

						if(contained)
						{
							var index = (void*) ((IntPtr) tree.elements->Ptr + node.firstChildIndex * UnsafeUtility.SizeOf<OctElement<T>>());

							UnsafeUtility.MemCpy((void*) ((IntPtr) fastResults->Ptr + count * UnsafeUtility.SizeOf<OctElement<T>>()),
								index, node.count * UnsafeUtility.SizeOf<OctElement<T>>());
							count += node.count;
						}
						else
						{
							for (int k = 0; k < node.count; k++)
							{
								var element = UnsafeUtility.ReadArrayElement<OctElement<T>>(tree.elements->Ptr, node.firstChildIndex + k);
								if(bounds.Contains(element.pos))
								{
									UnsafeUtility.WriteArrayElement(fastResults->Ptr, count++, element);
								}
							}
						}
					}
				}
			}
			
			public bool Intersects(AABB a, AABB b)
			{
				return (abs(a.Center[0] - b.Center[0]) < (a.Extents[0] + b.Extents[0])) &&
				       (abs(a.Center[1] - b.Center[1]) < (a.Extents[1] + b.Extents[1])) &&
				       (abs(a.Center[2] - b.Center[2]) < (a.Extents[2] + b.Extents[2]));
			}

			static AABB GetChildBounds(AABB parentBounds, int childZIndex)
			{
				var half = parentBounds.Extents.x * .5f;
				switch (childZIndex)
				{
					case 0: return new AABB { Center = new float3(parentBounds.Center.x - half, parentBounds.Center.y + half,  parentBounds.Center.z - half), Extents = half};
					case 1: return new AABB { Center = new float3(parentBounds.Center.x + half, parentBounds.Center.y + half,  parentBounds.Center.z - half), Extents = half};
					case 2: return new AABB { Center = new float3(parentBounds.Center.x - half, parentBounds.Center.y - half,  parentBounds.Center.z - half), Extents = half};
					case 3: return new AABB { Center = new float3(parentBounds.Center.x + half, parentBounds.Center.y - half,  parentBounds.Center.z - half), Extents = half};
					case 4: return new AABB { Center = new float3(parentBounds.Center.x - half, parentBounds.Center.y + half,  parentBounds.Center.z + half), Extents = half};
					case 5: return new AABB { Center = new float3(parentBounds.Center.x + half, parentBounds.Center.y + half,  parentBounds.Center.z + half), Extents = half};
					case 6: return new AABB { Center = new float3(parentBounds.Center.x - half, parentBounds.Center.y - half,  parentBounds.Center.z + half), Extents = half};
					case 7: return new AABB { Center = new float3(parentBounds.Center.x + half, parentBounds.Center.y - half,  parentBounds.Center.z + half), Extents = half};
					default: throw new Exception();
				}
			}
		}

	}
}