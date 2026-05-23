namespace League_Trader_Temple.Server
{
    public class RiftboundCard
    {
        public string Id { get; set; } = "";

        public string Name { get; set; } = "";

        public string RiftboundId { get; set; } = "";

        public string PublicCode { get; set; } = "";

        public int CollectorNumber { get; set; }

        public RiftboundCardAttributes Attributes { get; set; } = new();

        public RiftboundCardClassification Classification { get; set; } = new();

        public RiftboundCardText Text { get; set; } = new();

        public RiftboundCardSet Set { get; set; } = new();

        public RiftboundCardMedia Media { get; set; } = new();

        public string[] Tags { get; set; } = [];

        public string Orientation { get; set; } = "";

        public RiftboundCardMetadata Metadata { get; set; } = new();
    }

    public class RiftboundCardPage
    {
        public RiftboundCard[] Items { get; set; } = [];

        public int Total { get; set; }

        public int Page { get; set; }

        public int Size { get; set; }

        public int Pages { get; set; }
    }

    public class RiftboundCardAttributes
    {
        public int? Energy { get; set; }

        public int? Might { get; set; }

        public int? Power { get; set; }
    }

    public class RiftboundCardClassification
    {
        public string Type { get; set; } = "";

        public string? Supertype { get; set; }

        public string Rarity { get; set; } = "";

        public string[] Domain { get; set; } = [];
    }

    public class RiftboundCardText
    {
        public string Rich { get; set; } = "";

        public string Plain { get; set; } = "";

        public string? Flavour { get; set; }
    }

    public class RiftboundCardSet
    {
        public string Id { get; set; } = "";

        public string SetId { get; set; } = "";

        public string Label { get; set; } = "";
    }

    public class RiftboundCardMedia
    {
        public string ImageUrl { get; set; } = "";

        public string Artist { get; set; } = "";

        public string AccessibilityText { get; set; } = "";
    }

    public class RiftboundCardMetadata
    {
        public string CleanName { get; set; } = "";

        public DateTimeOffset? UpdatedOn { get; set; }

        public bool AlternateArt { get; set; }

        public bool Overnumbered { get; set; }

        public bool Signature { get; set; }
    }
}
