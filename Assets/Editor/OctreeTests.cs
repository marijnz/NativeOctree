using System.Diagnostics;
using NUnit.Framework;
using NativeOctree;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class OctreeTests
{
    AABB Bounds => new AABB{Center = 0, Extents = 1000};

    float3[] GetValues()
    {
        Random.InitState(0);
        var values = new float3[20000];

        for (int x = 0; x < values.Length; x++) {
            var val = new int3(Random.Range(-900, 900), Random.Range(-900, 900), Random.Range(-900, 900));
            values[x] = val;
        }

        return values;
    }

    [Test]
    public void InsertTriggerDivideBulk()
    {
        var values = GetValues();

        var elements = new NativeArray<OctElement<int>>(values.Length, Allocator.TempJob);

        for (int i = 0; i < values.Length; i++)
        {
            elements[i] = new OctElement<int>
            {
                pos = values[i],
                element = i
            };
        }

        var job = new OctreeJobs.AddBulkJob<int>
        {
            Elements = elements,
            Octree = new NativeOctree<int>(Bounds)
        };

        var s = Stopwatch.StartNew();

        job.Run();

        s.Stop();
        Debug.Log(s.Elapsed.TotalMilliseconds);

        OctreeDrawer.Draw(job.Octree);
        job.Octree.Dispose();
        elements.Dispose();
    }

    [Test]
    public void RangeQueryAfterBulk()
    {
        var values = GetValues();

        NativeArray<OctElement<int>> elements = new NativeArray<OctElement<int>>(values.Length, Allocator.TempJob);

        for (int i = 0; i < values.Length; i++)
        {
            elements[i] = new OctElement<int>
            {
                pos = values[i],
                element = i
            };
        }

        var octree = new NativeOctree<int>(Bounds);
        octree.ClearAndBulkInsert(elements);

        var queryJob = new OctreeJobs.RangeQueryJob<int>
        {
            Octree = octree,
            Bounds = new AABB {Center = 100, Extents = new float3(200, 1000, 200)},
            Results = new NativeList<OctElement<int>>(1000, Allocator.TempJob)
        };

        var s = Stopwatch.StartNew();
        queryJob.Run();
        s.Stop();
        Debug.Log(s.Elapsed.TotalMilliseconds + " result: " + queryJob.Results.Length);

        OctreeDrawer.DrawWithResults(queryJob);
        octree.Dispose();
        elements.Dispose();
        queryJob.Results.Dispose();
    }

    [Test]
    public void InsertTriggerDivideNonBurstBulk()
    {
        var values = GetValues();

        var positions = new NativeArray<float3>(values.Length, Allocator.TempJob);
        var octree = new NativeOctree<int>(Bounds);

        positions.CopyFrom(values);

        NativeArray<OctElement<int>> elements = new NativeArray<OctElement<int>>(positions.Length, Allocator.Temp);

        for (int i = 0; i < positions.Length; i++)
        {
            elements[i] = new OctElement<int>
            {
                pos = positions[i],
                element = i
            };
        }

        var s = Stopwatch.StartNew();

        octree.ClearAndBulkInsert(elements);

        s.Stop();
        Debug.Log(s.Elapsed.TotalMilliseconds);

        OctreeDrawer.Draw(octree);
        octree.Dispose();
        positions.Dispose();
    }
}
