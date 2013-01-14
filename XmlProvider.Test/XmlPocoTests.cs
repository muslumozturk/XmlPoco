namespace XmlProvider.Test
{
    using System;

    using NUnit.Framework;

    using XmlProvider.Test.Model;

    [TestFixture]
    public class XmlPocoTests
    {
        [Test]
        public void RehberTest1()
        {
            var rehber = new Rehber
                                {
                                    AdSoyad = "Müslüm ÖZTÜRK",
                                    DogumTarihi = new DateTime(2012, 1, 2),
                                    Telefon = "34567875"
                                };

            var isOk = XmlPoco.Insert(rehber);
            Assert.AreEqual(isOk,true);
        }

        [Test]
        public void UrunTest1()
        {
            var urun = new Urun
                           {
                               EklenmeTarihi = new DateTime(2012, 3, 4),
                               IsDeleted = false,
                               Sira = 1,
                               UrunAdi = "Deneme Ürünü",
                               UrunKodu = "DU"
                           };
            var isOk = XmlPoco.Insert(urun);
            Assert.AreEqual(isOk, true);
        }

        [Test]
        public void UrunUpdateText()
        {
            long id = 2;
            var item = XmlPoco.Select<Urun>(x => x.Id == id);
            if (item!=null)
            {
                item.UrunAdi = "Güncel Ürün";
                item.UrunKodu = "GU";
                item.IsDeleted = true;
                var isOk= XmlPoco.Update(item);
                Assert.AreEqual(isOk, true);
            }
            else
            {
                throw new Exception("Ürün Bulunamadı");
            }
        }

        [Test]
        public void UrunDeleteText()
        {
            long id = 3;
            var isOk= XmlPoco.Delete<Urun>(id);
            Assert.AreEqual(isOk, true);
        }


    }
}
