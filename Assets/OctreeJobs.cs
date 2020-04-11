using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace NativeOctree
{
	/// <summary>
	/// Examples on jobs for the NativeOctree
	/// </summary>
	public static class OctreeJobs
	{
		/// <summary>
		/// Bulk insert many items into the tree
		/// </summary>
		[BurstCompile]
		public struct AddBulkJob<T> : IJob where T : unmanaged
		{
			[ReadOnly]
			public NativeArray<OctElement<T>> Elements;

			public NativeOctree<T> Octree;

			public void Execute()
			{
				Octree.ClearAndBulkInsert(Elements);
			}
		}

		/// <summary>
		/// Example on how to do a range query, it's better to write your own and do many queries in a batch
		/// </summary>
		[BurstCompile]
		public struct RangeQueryJob<T> : IJob where T : unmanaged
		{
			[ReadOnly]
			public AABB Bounds;

			[ReadOnly]
			public NativeOctree<T> Octree;

			public NativeList<OctElement<T>> Results;

			public void Execute()
			{
				for (int i = 0; i < 1000; i++)
				{
					Octree.RangeQuery(Bounds, Results);
					Results.Clear();
				}
				Octree.RangeQuery(Bounds, Results);
			}
		}
	}
}