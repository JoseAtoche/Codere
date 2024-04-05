namespace Models.ShowModels.ExternalAPI
{
    public class ExternalAPIShowModel
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Language { get; set; }
        public List<string> Genres { get; set; }
        public string Status { get; set; }
        public int? Runtime { get; set; }
        public int? AverageRuntime { get; set; }
        public DateTime Premiered { get; set; }
        public string Ended { get; set; }
        public string OfficialSite { get; set; }
        public Scheduleclass? Schedule { get; set; }
        public Ratingclass Rating { get; set; }
        public int Weight { get; set; }
        public Networkclass Network { get; set; }
        public object DvdCountry { get; set; }
        public Externalsclass? Externals { get; set; }
        public Imageclass Image { get; set; }
        public string Summary { get; set; }
        public int Updated { get; set; }
        public Linkclass _links { get; set; }

        public DateTime? EndedDate
        {
            get
            {
                // Intentar convertir la cadena a DateTime, devuelve null si no se puede
                if (DateTime.TryParse(Ended, out DateTime endedDate))
                {
                    return endedDate;
                }
                return null;
            }
        }
        public object WebChannel { get; set; }

        public class Countryclass
        {
            public string Name { get; set; }
            public string Code { get; set; }
            public string Timezone { get; set; }
        }
        public class Selfclass
        {
            public string Href { get; set; }
        }

        public class Scheduleclass
        {
            public string Time { get; set; }
            public List<string> Days { get; set; }
        }

        public class Ratingclass
        {
            public double? Average { get; set; }
        }

        public class Previousepisodeclass
        {
            public string Href { get; set; }
        }

        public class Networkclass
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Countryclass Country { get; set; }
            public string OfficialSite { get; set; }
        }
        public class Linkclass
        {
            public Selfclass Self { get; set; }
            public Previousepisodeclass Previousepisode { get; set; }
        }
        public class Imageclass
        {
            public string Medium { get; set; }
            public string Original { get; set; }
        }

        public class Externalsclass
        {
            public int? Tvrage { get; set; }
            public int? Thetvdb { get; set; }
            public string? Imdb { get; set; }
        }

    }



}
