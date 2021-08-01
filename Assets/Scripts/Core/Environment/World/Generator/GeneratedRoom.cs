using System;
using System.Collections.Generic;
using Extensions;
using UnityEngine;
using UnityEngine.Assertions;

public class GeneratedRoom
{
    private List<RectInt> sections;

    public int CellCount => GetUniqueCells().Count;
    public int SectionCount => sections.Count;

    public GeneratedRoom(RectInt initialSection)
    {
        sections = new List<RectInt>(3);
        sections.Add(initialSection);
    }

    private HashSet<Vector2Int> GetUniqueCells()
    {
        HashSet<Vector2Int> allCells = new HashSet<Vector2Int>();
        foreach (var rect in sections)
            allCells.UnionWith(rect.GetAllPositions());

        return allCells;
    }

    public void AddSection(RectInt rect)
    {
        sections.Add(rect);
    }

    public IEnumerable<Vector2Int> GetAllCells()
    {
        return GetUniqueCells();
    }

    public IEnumerable<RectInt> GetSections()
    {
        return sections;
    }

    public RectInt GetSection(int idx)
    {
        return sections[idx];
    }

    public int SizeMerging(RectInt newSection)
    {
        var allCells = GetUniqueCells();
        allCells.UnionWith(newSection.GetAllPositions());
        return allCells.Count;
    }

    public int OpeningWidthMerging(RectInt newSection)
    {
        // For overlapping sctions, the merging width
        // is the width of the gap created by the merge,
        // computed by looking at the width and height of
        // the overlapping rectangle

        int maxWidth = 0;
        foreach (var section in sections)
        {
            if (section.Overlaps(newSection))
            {
                var yTop = Mathf.Min(section.yMax, newSection.yMax);
                var yBottom = Mathf.Max(section.yMin, newSection.yMin);
                var yWidth = yTop - yBottom;

                var xRight = Mathf.Min(section.xMax, newSection.xMax);
                var xLeft = Mathf.Max(section.xMin, newSection.xMin);
                var xWidth = xRight - xLeft;

                maxWidth = Mathf.Max(maxWidth, yWidth, xWidth);
            }
        }

        Assert.IsTrue(maxWidth > 0);

        return maxWidth;
    }
}
