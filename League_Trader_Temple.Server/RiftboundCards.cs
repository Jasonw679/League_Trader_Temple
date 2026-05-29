namespace League_Trader_Temple.Server
{
    public class RiftboundCard
    {
        public string Id { get; set; } = "";

        public string Name { get; set; } = "";

        public string SetId { get; set; } = "";

        public int CollectorNumber { get; set; }

        public string Rarity { get; set; } = "";

        public string Faction { get; set; } = "";

        public string Type { get; set; } = "";

        public string Orientation { get; set; } = "";

        public RiftboundCardStats Stats { get; set; } = new();

        public string Image { get; set; } = "";

        public RiftboundCardImageThumb ImageThumb { get; set; } = new();

        public string ImageBlurDataUrl { get; set; } = "";

        public bool IsBanned { get; set; }

        public int VisitCount { get; set; }
    }

    public class RiftboundCardPage
    {
        public RiftboundCard[] Items { get; set; } = [];

        public int Total { get; set; }

        public int Page { get; set; }

        public int Size { get; set; }

        public int Pages { get; set; }
    }

    public class RiftboundCardStats
    {
        public int? Energy { get; set; }

        public int? Might { get; set; }

        public int? Power { get; set; }
    }

    public class RiftboundCardImageThumb
    {
        public string Small { get; set; } = "";

        public string Medium { get; set; } = "";

        public string Large { get; set; } = "";
    }
}
