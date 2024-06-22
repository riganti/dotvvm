using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls.Options;

namespace DotVVM.Framework.Controls;

/// <summary>
/// Sorting options which supports sorting by multiple columns.
/// In the default implementation, the sorting is applied in the reverse order of clicks on the columns - i.e. the last clicked column is the primary sort criterion, the previous one is the secondary criterion, etc.
/// It behaves similarly to the standard <see cref="SortingOptions"/>, except that sorting is "stable".
/// Maximum number of sort criteria can be set by <see cref="MaxSortCriteriaCount"/>, and is 3 by default.
/// </summary>
public class MultiCriteriaSortingOptions : ISortingOptions, ISortingStateCapability, ISortingSetSortExpressionCapability, IApplyToQueryable
{
    public IList<SortCriterion> Criteria { get; set; } = new List<SortCriterion>();

    /// <summary> Maximum length of the <see cref="Criteria" /> list. When exceeded, the overhanging tail is discarded. </summary>
    public int MaxSortCriteriaCount { get; set; } = 3;

    public virtual IQueryable<T> ApplyToQueryable<T>(IQueryable<T> queryable)
    {
        foreach (var criterion in Criteria.Reverse())
        {
            queryable = SortingImplementation.ApplySortingToQueryable(queryable, criterion.SortExpression, criterion.SortDescending);
        }
        return queryable;
    }

    /// <summary> When overridden in derived class, it can disallow sorting by certain columns. </summary>
    public virtual bool IsSortingAllowed(string sortExpression) => true;

    public virtual void SetSortExpression(string? sortExpression)
    {
        if (sortExpression is {} && !IsSortingAllowed(sortExpression))
            throw new ArgumentException($"Sorting by column '{sortExpression}' is not allowed.");

        if (sortExpression == null)
        {
            Criteria.Clear();
            return;
        }

        var index = Criteria.ToList().FindIndex(c => c.SortExpression == sortExpression);
        if (index == 0)
        {
            // toggle the sort direction if we clicked on the column on the front
            Criteria[index].SortDescending = !Criteria[index].SortDescending;
        }
        else if (index > 0)
        {
            // if the column is already sorted, move it to the front
            Criteria.RemoveAt(index);
            Criteria.Insert(0, new SortCriterion() { SortExpression = sortExpression });
        }
        else
        {
            // add the column to the front
            Criteria.Insert(0, new SortCriterion() { SortExpression = sortExpression });
        }

        while (Criteria.Count > MaxSortCriteriaCount)
        {
            Criteria.RemoveAt(Criteria.Count - 1);
        }
    }

    /// <summary> Returns if the specified column is sorted in ascending order in any of the <see cref="Criteria"/> </summary>
    public bool IsColumnSortedAscending(string? sortExpression) => Criteria.Any(c => c.SortExpression == sortExpression && !c.SortDescending);

    /// <summary> Returns if the specified column is sorted in descending order in any of the <see cref="Criteria"/> </summary>
    public bool IsColumnSortedDescending(string? sortExpression) => Criteria.Any(c => c.SortExpression == sortExpression && c.SortDescending);
}

/// <summary>
/// Represents a sort criterion.
/// </summary>
public sealed record SortCriterion
{
    /// <summary>
    /// Gets or sets whether the sort order should be descending.
    /// </summary>
    public bool SortDescending { get; set; }

    /// <summary>
    /// Gets or sets the name of the property that is used for sorting. Null means the grid should not be sorted. May contain chained properties separated by dots.
    /// </summary>
    public string? SortExpression { get; set; }
}
