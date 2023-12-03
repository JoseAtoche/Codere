using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace APICodere.Models.Dtos
{
    [Table("Show")]
    public class ShowDto
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Language { get; set; }
        public List<string>? Genres { get; set; }
        public string Status { get; set; }
        public int Runtime { get; set; }
        public int AverageRuntime { get; set; }
        public DateTime Premiered { get; set; }
        public DateTime Ended { get; set; }
        public string OfficialSite { get; set; }
        [NotMapped]

        public ScheduleDto Schedule { get; set; }
        public int? IdSchedule { get; set; }

        public double Rating { get; set; }
        public int Weight { get; set; }

        [NotMapped] 
        public NetworkDto Network { get; set; }

        public int? idNetwork { get; set; }
        public string WebChannel { get; set; }
        public string DvdCountry { get; set; }
        [NotMapped]

        public ExternalsDto Externals { get; set; }
        public int? idExternals { get; set; }

        [NotMapped]
        public ImageDto Image { get; set; }
        public int? idImage { get; set; }

        public string Summary { get; set; }
        public int Updated { get; set; }

        [NotMapped]
        public LinkDto Link { get; set; }
        public int? idLink { get; set; }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            ShowDto otherShow = (ShowDto)obj;

            // Comparar todas las propiedades, incluyendo las de clases internas
            return GetType().GetProperties().All(property =>
            {
                object thisValue = property.GetValue(this);
                object otherValue = property.GetValue(otherShow);

                // Si la propiedad es una clase, comparar recursivamente
                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    return thisValue != null && otherValue != null
                        ? thisValue.Equals(otherValue)
                        : thisValue == otherValue;
                }

                return Equals(thisValue, otherValue);
            });
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                GetType().GetProperties().Select(property => property.GetValue(this))
            );
        }
    }
    [Table("Schedule")]
    public class ScheduleDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Time { get; set; }
        public List<string> Days { get; set; }
    }

    [Table("Country")]
    public class CountryDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Code { get; set; }
        public string Timezone { get; set; }
    }
    [Table("Network")]
    public class NetworkDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CountryDto Country { get; set; }
        public int idCountry { get; set; }

        public string OfficialSite { get; set; }

    }
    [Table("Externals")]
    public class ExternalsDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int Tvrage { get; set; }
        public int Thetvdb { get; set; }
        public string Imdb { get; set; }
    }
    [Table("Image")]
    public class ImageDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Medium { get; set; }
        public string Original { get; set; }
    }
    [Table("Link")]
    public class LinkDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string SelfHref { get; set; }
        public string PreviousepisodeHref { get; set; }
    }


    
}
