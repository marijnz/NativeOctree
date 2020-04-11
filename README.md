# Native Octree
An Octree Native Collection for Unity DOTS. It's ported from https://github.com/marijnz/NativeQuadtree with very few changes.

## Implementation
- It's a DOTS native container, meaning it's handling its own unmanaged memory and can be passed into jobs!
- It currently only supports the storing of points
- The bulk insertion is using morton codes. This allows very fast bulk insertion but causes an increasing (minor) overhead with an increased depth

## Performance
There's some very rudimentary performance tests included. With 20k elements on a 2000x2000x2000m map, a max depth of 6 and 16 max elements per leaf. Burst enabled, ran on main thread on my i7-7700K CPU @ 4.20GHz:</br>

- Job: Bulk insertion of all elements - Takes ~1ms
- Job: 1k queries on a 200x200x200m range - Takes ~2.7ms

With Burst disabled the tests are about 10x slower.

## Stability
The only tests test for performance so there's no real test coverage. I'm sure there's edge cases that are not caught. I would highly recommend writing more tests if you're planning to use the code in production.

## Query debug view
There's a very simple debug drawer that visualizes nodes and query hits. Example below is of a 200x1000x200m AABB.
<p align="center">
<img src="media/verticalquery.gif" width="500"/></br
</p>

## Potential future work / missing features
- Unit tests
- Support for basic shapes
- Other types of queries, such as raycasts
- Support individual adding and removing of elements
