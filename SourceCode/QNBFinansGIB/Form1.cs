using QNBFinansGIB.DTO;
using QNBFinansGIB.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace QNBFinansGIB
{
    public partial class Form1 : Form
    {
        // Burada genel olarak hazır bilgiler üzerine tanımlama yapılıyor
        public List<GidenFaturaDTO> gidenFaturaListesi = new List<GidenFaturaDTO>();
        public List<GidenFaturaDetayDTO> gidenFaturaDetayListesi = new List<GidenFaturaDetayDTO>();

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Burada Form görüntülenirken
        /// Kendi hazırladığımız Giden Fatura ve Detaylarının oluşturup listeye atıyoruz ki
        /// Buradan sonra butonlara tıklandığında
        /// Bu listeler üzerinden kolayca işlem yapılabilsin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Shown(object sender, EventArgs e)
        {
            #region İlk Verilerin Hazırlanması
            #region Giden Faturaları Ekleme
            var gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = Guid.NewGuid().ToString(),
                TuzelKisiAd = "Deneme Şirketi - 1",
                VergiNo = "53602329864",
                VergiDairesi = "ANKARA",
                Adres = "Kızılay Gima'nın Önü",
                FaksNo = "03121111111",
                KodBankaAd = "Türkiye İş Bankası",
                BankaSube = "Kızılay",
                IbanNo = "TR130006297372542652383131",
                DuzenlemeTarihi = new DateTime(2022, 1, 13),
                KodIlAd = "Ankara",
                KodIlceAd = "Çankaya",
                KodSatisTuruKod = 1,
                GibNumarasi = "MLT2022000000008",
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
            };
            gidenFaturaListesi.Add(gidenFatura);

            gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = Guid.NewGuid().ToString(),
                TuzelKisiAd = "Deneme Şirketi - 2",
                VergiNo = "9250936109",
                VergiDairesi = "ANKARA",
                Adres = "Kızılay YKM",
                FaksNo = "03121111112",
                KodBankaAd = "Vakıflar Bankası",
                BankaSube = "Kızılay",
                IbanNo = "TR530006291586775935345942",
                DuzenlemeTarihi = new DateTime(2022, 1, 1),
                KodIlAd = "Ankara",
                KodIlceAd = "Etimesgut",
                KodSatisTuruKod = 2,
                GibNumarasi = "MLT2022000000008",
                BelgeOid = "12kyb38rq712tm",
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
            };
            gidenFaturaListesi.Add(gidenFatura);

            gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = Guid.NewGuid().ToString(),
                TuzelKisiAd = "Deneme Şirketi - 3",
                VergiNo = "5240018140",
                VergiDairesi = "ANKARA",
                Adres = "Zafer Çarşısı",
                FaksNo = "03121111113",
                KodBankaAd = "Vakıf Katılım Bankası",
                BankaSube = "Kızılay",
                IbanNo = "TR830006214925976839299642",
                DuzenlemeTarihi = new DateTime(2022, 1, 11),
                KodIlAd = "Ankara",
                KodIlceAd = "Etimesgut",
                KodSatisTuruKod = 2,
                GibNumarasi = "KST2022000000070",
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
            };
            gidenFaturaListesi.Add(gidenFatura);

            gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = Guid.NewGuid().ToString(),
                TuzelKisiAd = "Deneme Şirketi - 4",
                VergiNo = "6940116151",
                VergiDairesi = "ANKARA",
                Adres = "Göksu Parkı",
                FaksNo = "03121111114",
                KodBankaAd = "Kuveyt Tür",
                BankaSube = "Kızılay",
                IbanNo = "TR350006267645192879857714",
                DuzenlemeTarihi = new DateTime(2022, 1, 31),
                KodIlAd = "Ankara",
                KodIlceAd = "Çubuk",
                KodSatisTuruKod = 10,
                GibNumarasi = "ILG2022000000630",
                BelgeOid = "13kz0tj9og11fd",
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
            };
            gidenFaturaListesi.Add(gidenFatura);

            gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = Guid.NewGuid().ToString(),
                TuzelKisiAd = "Deneme Şirketi - 3",
                VergiNo = "20101516422",
                VergiDairesi = "ANKARA",
                Adres = "Dost'un Önü",
                FaksNo = "03121111113",
                KodBankaAd = "Enpara.Com Bankadan Güzeli",
                BankaSube = "Karakusunlar Şube",
                IbanNo = "TR630006295412435711171488",
                DuzenlemeTarihi = new DateTime(2022, 2, 1),
                KodIlAd = "Ankara",
                KodIlceAd = "Pursaklar",
                KodSatisTuruKod = 2,
                GibNumarasi = "SUS2022000000087",
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
            };
            gidenFaturaListesi.Add(gidenFatura);

            gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = Guid.NewGuid().ToString(),
                GercekKisiTcKimlikNo = "52270709114",
                GercekKisiAd = "Adnan",
                GercekKisiSoyad = "Şenses",
                CepTelefonNo = "05555555555",
                EPostaAdresi = "adnansenses@mynet.com",
                IkametgahAdresi = "Dost'un Önü",
                KodBankaAd = "Halkbank",
                BankaSube = "Etimesgut Şube",
                IbanNo = "TR540006234114125194267726",
                DuzenlemeTarihi = new DateTime(2022, 2, 2),
                KodIlAd = "Konya",
                KodIlceAd = "Beyşehir",
                KodSatisTuruKod = 10,
                GibNumarasi = "YZG2022000000077",
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
            };
            gidenFaturaListesi.Add(gidenFatura);

            gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = Guid.NewGuid().ToString(),
                GercekKisiTcKimlikNo = "20077290692",
                GercekKisiAd = "Mehmet Emin",
                GercekKisiSoyad = "Akdeniz",
                CepTelefonNo = "05555555554",
                EPostaAdresi = "memo@yahoo.com",
                IkametgahAdresi = "Fışkiyenin orası",
                KodBankaAd = "Bank Mellat",
                BankaSube = "Yenimahalle Şube",
                IbanNo = "TR410006276748779926367358",
                DuzenlemeTarihi = new DateTime(2022, 1, 25),
                KodIlAd = "Ağrı",
                KodIlceAd = "Patnos",
                KodSatisTuruKod = 11,
                GibNumarasi = "AGR2022000000039",
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
            };
            gidenFaturaListesi.Add(gidenFatura);
            #endregion

            // burada fatura detayları kabarık olsun diye tüm fatura ana nesnelerine eklenecek bir yapı kuruldu
            // ancak arada bir ilişki istenirse
            // DTO sınıfında GidenFaturaDetayDTO sınıfına bir GidenFaturaId alanı eklenerek
            // Ve üstteki regionda faturalara elle id verildikten sonra
            // fatura detayları eklenirken buradaki ana nesne id alanları girilebilir
            #region Giden Fatura Detayları Ekleme

            var gidenFaturaDetay = new GidenFaturaDetayDTO
            {
                BirimFiyat = (decimal)0.1234,
                Miktar = 1000,
                KodKdvTuruOran = 0,
                KodIskontoTuruOran = 0,
                KdvHaricTutar = 1234,
                KdvTutari = 0,
                GibKisaltma = "KGM",
                IrsaliyeNo = "1",
                MalzemeFaturaAciklamasi = "Ofis demirbaşı için alındı",
                SevkIrsaliyeTarihi = new DateTime(2022, 1, 1),
                SevkIrsaliyesiNo = "34",
                PlakaNo = "06GUC777",
                KodFaturaUrunTuruAd = "Dizüstü Bilgisayar",
                Tonaj = 1,
                YuklemeFormuNo = 1234
            };
            gidenFaturaDetayListesi.Add(gidenFaturaDetay);

            gidenFaturaDetay = new GidenFaturaDetayDTO
            {
                BirimFiyat = (decimal)0.1234,
                Miktar = 1000,
                KodKdvTuruOran = 1,
                KodIskontoTuruOran = 0,
                KdvHaricTutar = 1234,
                KdvTutari = (decimal)12.34,
                GibKisaltma = "MTR",
                PlakaNo = "06GC7447",
                KodFaturaUrunTuruAd = "Çubuk Mikrofon",
            };
            gidenFaturaDetayListesi.Add(gidenFaturaDetay);

            gidenFaturaDetay = new GidenFaturaDetayDTO
            {
                BirimFiyat = (decimal)0.1234,
                Miktar = 1000,
                KodKdvTuruOran = 1,
                KodIskontoTuruOran = 0,
                KdvHaricTutar = 1234,
                KdvTutari = (decimal)12.34,
                GibKisaltma = "KWT",
                PlakaNo = "06GC7447",
                KodFaturaUrunTuruAd = "Çubuk Mikrofon",
            };
            gidenFaturaDetayListesi.Add(gidenFaturaDetay);

            gidenFaturaDetay = new GidenFaturaDetayDTO
            {
                BirimFiyat = (decimal)0.124,
                Miktar = 5000,
                KodKdvTuruOran = 8,
                KodIskontoTuruOran = 0,
                KdvHaricTutar = 6020,
                KdvTutari = (decimal)481.6,
                GibKisaltma = "MON",
                PlakaNo = "06J1234",
                KodFaturaUrunTuruAd = "Mouse",
            };
            gidenFaturaDetayListesi.Add(gidenFaturaDetay);

            gidenFaturaDetay = new GidenFaturaDetayDTO
            {
                BirimFiyat = 500,
                Miktar = 120,
                KodKdvTuruOran = 18,
                KodIskontoTuruOran = 0,
                KdvHaricTutar = 60000,
                KdvTutari = 12800,
                GibKisaltma = "LTR",
                PlakaNo = "06C99876",
                KodFaturaUrunTuruAd = "Ayakkabı",
            };
            gidenFaturaDetayListesi.Add(gidenFaturaDetay);

            #endregion

            #region Giden Fatura Toplamları Hesaplama

            foreach (var item in gidenFaturaListesi)
            {
                item.KdvHaricTutar = gidenFaturaDetayListesi.Sum(j => j.KdvHaricTutar);
                item.KdvTutari = gidenFaturaDetayListesi.Sum(j => j.KdvTutari);
                item.FaturaTutari = item.KdvHaricTutar + item.KdvTutari;
            }

            #endregion
            #endregion
        }

        /// <summary>
        /// Sistemde E-Fatura ve E-Arşiv Servislerine Gönderilecek Formatta
        /// İdeal XML dosyalarının oluşturulması için XML Oluştur Butonuna tıklandığı zaman
        /// Yapılacak işlemlerin hazırlandığı metottur.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnXmlOlustur_Click(object sender, EventArgs e)
        {
            using (var dialogKlasorSecimi = new FolderBrowserDialog())
            {
                dialogKlasorSecimi.SelectedPath = Application.StartupPath;
                DialogResult result = dialogKlasorSecimi.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialogKlasorSecimi.SelectedPath))
                {
                    var klasorAdi = dialogKlasorSecimi.SelectedPath;
                    var index = new Random().Next(gidenFaturaListesi.Count);
                    var gidenFatura = gidenFaturaListesi[index];

                    #region XML Oluşturma
                    var dosyaAdi = "";
                    if (!string.IsNullOrEmpty(gidenFatura.TuzelKisiAd))
                    {
                        var kullaniciMi = DisServisler.EFaturaKullanicisiMi(gidenFatura.VergiNo);
                        if (kullaniciMi)
                        {
                            dosyaAdi = YardimciSiniflar.EFaturaXMLOlustur(gidenFatura, gidenFaturaDetayListesi, klasorAdi);
                        }
                        else
                        {
                            dosyaAdi = YardimciSiniflar.EArsivXMLOlustur(gidenFatura, gidenFaturaDetayListesi, klasorAdi, true);
                        }
                    }
                    else
                    {
                        dosyaAdi = YardimciSiniflar.EArsivXMLOlustur(gidenFatura, gidenFaturaDetayListesi, klasorAdi, false);
                    }

                    MessageBox.Show(dosyaAdi + " adresinde gerekli XML dosyası oluşturulmuştur.");

                    #endregion
                }
            }
        }

        /// <summary>
        /// Sistemde E-Fatura veya E-Arşive yollansın yollanmasın
        /// Hazırlanan XML dosyalarının bu sistemlerde nasıl göründüğüne dair
        /// Önizleme alınmasını sağlayan metottur.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGIBOnizleme_Click(object sender, EventArgs e)
        {
            using (var dialogKlasorSecimi = new FolderBrowserDialog())
            {
                dialogKlasorSecimi.SelectedPath = Application.StartupPath;
                DialogResult result = dialogKlasorSecimi.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialogKlasorSecimi.SelectedPath))
                {
                    var klasorAdi = dialogKlasorSecimi.SelectedPath;
                    var index = new Random().Next(gidenFaturaListesi.Count);
                    var gidenFatura = gidenFaturaListesi[index];

                    #region XML Oluşturma
                    var dosyaAdi = "";
                    var geriDonus = new GeriDonus();
                    geriDonus.Tip = 0;
                    var dosya = new byte[1];
                    var kullaniciMi = false;
                    if (!string.IsNullOrEmpty(gidenFatura.TuzelKisiAd))
                    {
                        kullaniciMi = DisServisler.EFaturaKullanicisiMi(gidenFatura.VergiNo);
                        if (kullaniciMi)
                        {
                            dosyaAdi = YardimciSiniflar.EFaturaXMLOlustur(gidenFatura, gidenFaturaDetayListesi, klasorAdi);
                            geriDonus = DisServisler.EFaturaOnIzleme(gidenFatura, dosyaAdi);
                            if (geriDonus != null)
                                dosya = geriDonus.Dosya;
                        }
                        else
                        {
                            dosyaAdi = YardimciSiniflar.EArsivXMLOlustur(gidenFatura, gidenFaturaDetayListesi, klasorAdi, true);
                            dosya = DisServisler.EArsivOnIzleme(gidenFatura, dosyaAdi);
                        }
                    }
                    else
                    {
                        dosyaAdi = YardimciSiniflar.EArsivXMLOlustur(gidenFatura, gidenFaturaDetayListesi, klasorAdi, false);
                        dosya = DisServisler.EArsivOnIzleme(gidenFatura, dosyaAdi);
                    }

                    if (dosya != null && dosya.Length > 1)
                    {
                        var dosyaAdiTemp = dosyaAdi.Replace("xml", "pdf");
                        if (kullaniciMi && !string.IsNullOrEmpty(gidenFatura.BelgeOid))
                            dosyaAdiTemp = dosyaAdi.Replace("xml", "zip");
                        if (geriDonus != null && geriDonus.Tip == 1)
                            dosyaAdiTemp = dosyaAdi.Replace("xml", "zip");

                        File.WriteAllBytes(dosyaAdiTemp, dosya);

                        MessageBox.Show(dosyaAdiTemp + " adresinde gerekli PDF veya ZIP dosyası oluşturulmuştur.");
                    }
                    else
                    {
                        MessageBox.Show("Söz konusu faturanın önizlemesi oluşturulamamıştır");
                    }

                    #endregion
                }
            }
        }
    }
}