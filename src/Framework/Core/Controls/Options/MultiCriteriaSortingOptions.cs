using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls.Options;

namespace DotVVM.Framework.Controls;

public class MultiCriteriaSortingOptions : ISortingOptions, ISortingStateCapability, ISortingSetSortExpressionCapability, IApplyToQueryable
{
    public IList<SortCriterion> Criteria { get; set; } = new List<SortCriterion>();

    public int MaxSortCriteriaCount { get; set; } = 3;

    public virtual IQueryable<T> ApplyToQueryable<T>(IQueryable<T> queryable)
    {
        foreach (var criterion in Criteria.Reverse())
        {
            queryable = SortingImplementation.ApplySortingToQueryable(queryable, criterion.SortExpression, criterion.SortDescending);
        }
        return queryable;
    }

    public virtual bool IsSortingAllowed(string sortExpression) => true;

    public virtual void SetSortExpression(string? sortExpression)
    {
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

    public bool IsColumnSortedAscending(string? sortExpression) => Criteria.Any(c => c.SortExpression == sortExpression && !c.SortDescending);

    public bool IsColumnSortedDescending(string? sortExpression) => Criteria.Any(c => c.SortExpression == sortExpression && c.SortDescending);
}
