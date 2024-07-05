using AutoMapper;
using LyricsScraperApi.Models;

namespace LyricsScraperApi.UnitTests.Models
{
    public class MappingProfileTests
    {
        private readonly IMapper _mapper;

        public MappingProfileTests() => _mapper = new MapperConfiguration(cfg => { cfg.AddProfile<MappingProfile>(); }).CreateMapper();

        [Fact]
        public void MappingProfile_AssertConfiguration_ShouldBeValid() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}
