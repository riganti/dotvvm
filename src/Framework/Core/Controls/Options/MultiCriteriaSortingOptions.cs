using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls;

public class MultiCriteriaSortingOptions : SortingOptions, ISortingMultipleCriteriaCapability
{
    public IList<SortCriterion> Criteria { get; set; } = new List<SortCriterion>();

    public int MaxSortCriteriaCount { get; set; } = 3;

    public override IQueryable<T> ApplyToQueryable<T>(IQueryable<T> queryable)
    {
        foreach (var criterion in Criteria.Reverse())
        {
            queryable = SortingImplementation.ApplySortingToQueryable(queryable, criterion.SortExpression, criterion.SortDescending);
        }

        return base.ApplyToQueryable(queryable);
    }

    public override void SetSortExpression(string? sortExpression)
    {
        if (SortExpression == null)
        {
            SortExpression = sortExpression;
            SortDescending = false;
        }
        else if (sortExpression == SortExpression)
        {
            if (!SortDescending)
            {
                SortDescending = true;
            }
            else if (Criteria.Any())
            {
                SortExpression = Criteria[0].SortExpression;
                SortDescending = Criteria[0].SortDescending;
                Criteria.RemoveAt(0);
            }
            else
            {
                SortExpression = null;
                SortDescending = false;
            }
        }
        else
        {
            var index = Criteria.ToList().FindIndex(c => c.SortExpression == sortExpression);
            if (index >= 0)
            {
                if (!Criteria[index].SortDescending)
                {
                    Criteria[index].SortDescending = true;
                }
                else
                {
                    Criteria.RemoveAt(index);
                }
            }
            else
            {
                if (Criteria.Count < MaxSortCriteriaCount - 1)
                {
                    Criteria.Add(new SortCriterion() { SortExpression = sortExpression });
                }
                else
                {
                    SortExpression = sortExpression;
                    SortDescending = false;
                    Criteria.Clear();
                }
            }
        }
    }
}
