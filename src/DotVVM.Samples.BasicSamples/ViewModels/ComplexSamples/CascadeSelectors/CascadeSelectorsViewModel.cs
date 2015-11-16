using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.CascadeSelectors
{
    public class CascadeSelectorsViewModel : DotvvmViewModelBase
    {
        private Sample11DataService _dataService = new Sample11DataService();

        public override Task Init()
        {
            Cities = _dataService.GetCities();
            SelectedHotel = new HotelModel();
            return base.Init();
        }

        public List<CityModel> Cities { get; set; }

        public int SelectedCityId { get; set; }

        public List<HotelModel> HotelsInCity { get; set; }

        public int? SelectedHotelId { get; set; }

        public HotelModel SelectedHotel { get; set; }

        public void SelectedCityChanged()
        {
            HotelsInCity = _dataService.GetHotels(SelectedCityId);
        }

        public void SelectedHotelChanged()
        {
            SelectedHotel = _dataService.GetHotelById(SelectedHotelId.Value);
        }
    }


    public class CityModel
    {
        public string Name { get; set; }

        public int Id { get; set; }
    }

    public class HotelModel
    {
        public string Name { get; set; }

        public int CityId { get; set; }

        public int Id { get; set; }
    }

    public class Sample11DataService
    {
        HotelModel[] _hotels = new HotelModel[]
            {
                new HotelModel() { Id = 1, CityId = 1, Name = "Hotel Prague #1" },
                new HotelModel() { Id = 2, CityId = 1, Name = "Hotel Prague #2" },
                new HotelModel() { Id = 3, CityId = 1, Name = "Hotel Prague #3" },
                new HotelModel() { Id = 4, CityId = 2, Name = "Hotel Seattle #1" },
                new HotelModel() { Id = 5, CityId = 2, Name = "Hotel Seattle #2" },
                new HotelModel() { Id = 6, CityId = 2, Name = "Hotel Seattle #3" },
                new HotelModel() { Id = 7, CityId = 3, Name = "Hotel New York #1" },
                new HotelModel() { Id = 8, CityId = 3, Name = "Hotel New York #2" },
                new HotelModel() { Id = 9, CityId = 3, Name = "Hotel New York #3" }
            };

        public List<CityModel> GetCities()
        {
            return new List<CityModel>()
            {
                new CityModel() { Id = 1, Name = "Prague" },
                new CityModel() { Id = 2, Name = "Seattle" },
                new CityModel() { Id = 3, Name = "New York" }
            };
        }

        public HotelModel GetHotelById(int hotelId)
        {
            return _hotels.FirstOrDefault(h => h.Id == hotelId);
        }

        public List<HotelModel> GetHotels(int cityId)
        {
            return _hotels.Where(h => h.CityId == cityId).ToList();
        }
    }
}
