namespace XmlProvider.Test.Model
{
    using System;

    public  class Urun
    {
        public long Id { get; set; }
        public string UrunAdi { get; set; }
        public string UrunKodu { get; set; }
        public DateTime EklenmeTarihi { get; set; }
        public short Sira { get; set; }
        public bool IsDeleted { get; set; }
    }
}
