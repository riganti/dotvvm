using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ComplexSamples.CascadeSelectors
{
	public class TripleComboBoxesViewModel : DotvvmViewModelBase
	{

	    public List<GridItem> GridItems { get; set; } = new List<GridItem>()
	    {
	        new GridItem() { RegionId = 1, Region = "North America", CountryId = 11, Country = "USA", CityId = 111, City = "New York" },
	        new GridItem() { RegionId = 1, Region = "North America", CountryId = 11, Country = "USA", CityId = 112, City = "Seattle" },
	        new GridItem() { RegionId = 1, Region = "North America", CountryId = 12, Country = "Canada", CityId = 121, City = "Toronto" },
	        new GridItem() { RegionId = 1, Region = "North America", CountryId = 12, Country = "Canada", CityId = 112, City = "Vancouver" },
	        new GridItem() { RegionId = 2, Region = "Europe", CountryId = 21, Country = "Germany", CityId = 211, City = "Berlin" },
	        new GridItem() { RegionId = 2, Region = "Europe", CountryId = 21, Country = "Germany", CityId = 212, City = "Munich" },
	        new GridItem() { RegionId = 2, Region = "Europe", CountryId = 22, Country = "France", CityId = 221, City = "Paris" },
	        new GridItem() { RegionId = 2, Region = "Europe", CountryId = 22, Country = "France", CityId = 222, City = "Lyon" },
	        new GridItem() { RegionId = 3, Region = "Asia", CountryId = 31, Country = "China", CityId = 311, City = "Beijing" },
	        new GridItem() { RegionId = 3, Region = "Asia", CountryId = 31, Country = "China", CityId = 312, City = "Shanghai" },
	        new GridItem() { RegionId = 3, Region = "Asia", CountryId = 32, Country = "Japan", CityId = 321, City = "Tokyo" },
	        new GridItem() { RegionId = 3, Region = "Asia", CountryId = 32, Country = "Japan", CityId = 322, City = "Osaka" }
	    };

	    public List<IdNameItem> Regions { get; set; } = new List<IdNameItem>();

	    public List<IdNameItem> Countries { get; set; } = new List<IdNameItem>();

	    public List<IdNameItem> Cities { get; set; } = new List<IdNameItem>();

	    public GridEditItem Selected { get; set; } = new GridEditItem();

	    public override Task Load()
	    {
	        if (!Context.IsPostBack)
	        {
	            FillRegions();
	        }

	        return base.Load();
	    }


	    public void Select(GridItem item)
	    {
	        Selected.RegionId = item.RegionId;
            OnRegionChanged();

	        Selected.CountryId = item.CountryId;
            OnCountryChanged();

	        Selected.CityId = item.CityId;
	    }

	    private void FillRegions()
	    {
            Regions = GridItems
                .Select(i => new IdNameItem() { Id = i.RegionId, Name = i.Region })
                .Distinct()
                .ToList();
        }

	    public void OnRegionChanged()
	    {
	        Countries = GridItems
                .Where(i => i.RegionId == Selected.RegionId)
	            .Select(i => new IdNameItem() { Id = i.CountryId, Name = i.Country })
	            .Distinct()
	            .ToList();
	    }

	    public void OnCountryChanged()
	    {
            Cities = GridItems
                .Where(i => i.CountryId == Selected.CountryId)
                .Select(i => new IdNameItem() { Id = i.CityId, Name = i.City })
                .Distinct()
                .ToList();
        }
	}

    public class IdNameItem : IEquatable<IdNameItem>
    {
        public string Name { get; set; }

        public int Id { get; set; }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(IdNameItem other)
        {
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }

    public class GridItem
    {

        public string Region { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public int RegionId { get; set; }

        public int CountryId { get; set; }

        public int CityId { get; set; }

    }

    public class GridEditItem
    {

        public int? RegionId { get; set; }

        public int? CountryId { get; set; }

        public int? CityId { get; set; }

    }
}

