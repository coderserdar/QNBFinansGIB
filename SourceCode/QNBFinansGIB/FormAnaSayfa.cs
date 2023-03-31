using QNBFinansGIB.DTO;
using QNBFinansGIB.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static QNBFinansGIB.Utils.Enums;

namespace QNBFinansGIB
{
    /// <summary>
    /// QNB Finans GİB Servisi için veri hazırlanan ve servisin test edildiği metotlar içeren
    /// Form sınıfıdır
    /// </summary>
    public partial class frmAnaSayfa : Form
    {
        #region Sabit Elemanlar

        /// <summary>
        /// Giden Fatura Listesi
        /// </summary>
        private readonly List<GidenFaturaDTO> gidenFaturaListesi = new List<GidenFaturaDTO>();

        /// <summary>
        /// Giden Fatura Detay Listesi
        /// </summary>
        private readonly List<GidenFaturaDetayDTO> gidenFaturaDetayListesi = new List<GidenFaturaDetayDTO>();

        /// <summary>
        /// Müstahsil Makbuzu Listesi
        /// </summary>
        private readonly List<MustahsilMakbuzuDTO> mustahsilMakbuzuListesi = new List<MustahsilMakbuzuDTO>();

        /// <summary>
        /// Müstahsil Makbuzu Detay Listesi
        /// </summary>
        private readonly List<MustahsilMakbuzuDetayDTO> mustahsilMakbuzuDetayListesi = new List<MustahsilMakbuzuDetayDTO>();

        #endregion

        public frmAnaSayfa()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Burada Form görüntülenirken
        /// Kendi hazırladığımız Giden Fatura ve Detaylarının oluşturup listeye atıyoruz ki
        /// Buradan sonra butonlara tıklandığında
        /// Bu listeler üzerinden kolayca işlem yapılabilsin
        /// </summary>
        /// <param name="sender">The sender info (For example Main Form)</param>
        /// <param name="e">Event Arguments</param>
        private void FormAnaSayfa_Shown(object sender, EventArgs e)
        {
            #region Ekrandaki Alanların Düzenlenmesi

            dtpFaturaTarihi.Format = DateTimePickerFormat.Short;
            lbEFaturaKullaniciListesi.DrawMode = DrawMode.OwnerDrawVariable;
            lbEFaturaKullaniciListesi.MeasureItem += listBox_MeasureItem;
            lbEFaturaKullaniciListesi.DrawItem += listBox_DrawItem;

            #endregion

            #region İlk Verilerin Hazırlanması

            #region Giden Faturalar

            #region Giden Faturaları Ekleme

            var gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = "11",
                AltBirimAd = "  A Birimi    ",
                TuzelKisiAd = "Deneme Şirketi - 1",
                VergiNo = "53602329864",
                VergiDairesi = "ANKARA",
                Adres = "Kızılay Gima'nın Önü",
                FaksNo = "03121111111",
                BankaAd = "Türkiye İş Bankası",
                BankaSube = "Kızılay",
                IbanNo = "TR130006297372542652383131",
                IslemTarihi = new DateTime(2022, 1, 13),
                IlAd = "Ankara",
                IlceAd = "Çankaya",
                SatisTuruKod = SatisTur.Hesaben.GetHashCode(),
                FaturaGrupTuruKod = FaturaMakbuzGrupTur.Diger.GetHashCode(),
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
                KonaklamaVergisi = 0,
                Aciklama = "Deneme amaçlı eklenmiştir"
            };
            gidenFatura.GibNumarasi = GibNumarasi.RastgeleGibNumarasiOlustur(gidenFatura.IslemTarihi);
            gidenFaturaListesi.Add(gidenFatura);

            gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = "21",
                AltBirimAd = "B Birimi    ",
                TuzelKisiAd = "  Deneme Şirketi - 2",
                VergiNo = "9250936109",
                VergiDairesi = "ANKARA",
                Adres = "Kızılay YKM",
                FaksNo = "03121111112",
                BankaAd = "Vakıflar Bankası",
                BankaSube = "Kızılay",
                IbanNo = "TR530006291586775935345942",
                IslemTarihi = new DateTime(2022, 1, 1),
                IlAd = "Ankara",
                IlceAd = "Etimesgut",
                SatisTuruKod = SatisTur.Pesin.GetHashCode(),
                FaturaGrupTuruKod = FaturaMakbuzGrupTur.Malzeme.GetHashCode(),
                FaturaTuruKod = FaturaTur.Iade.GetHashCode(),
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
                KonaklamaVergisi = 0
            };
            gidenFatura.GibNumarasi = GibNumarasi.RastgeleGibNumarasiOlustur(gidenFatura.IslemTarihi);
            gidenFaturaListesi.Add(gidenFatura);

            gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = "31",
                AltBirimAd = "B Birimi    ",
                TuzelKisiAd = "  Deneme Şirketi - 3",
                VergiNo = "5240018140",
                VergiDairesi = "ANKARA",
                Adres = "Zafer Çarşısı",
                FaksNo = "03121111113",
                BankaAd = "Vakıf Katılım Bankası",
                BankaSube = "Kızılay",
                IbanNo = "TR830006214925976839299642",
                IslemTarihi = new DateTime(2023, 1, 11),
                IlAd = "Ankara",
                IlceAd = "Etimesgut",
                SatisTuruKod = SatisTur.ImalatciIhracatci.GetHashCode(),
                FaturaTuruKod = FaturaTur.Iade.GetHashCode(),
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
                KonaklamaVergisi = 0
            };
            gidenFatura.GibNumarasi = GibNumarasi.RastgeleGibNumarasiOlustur(gidenFatura.IslemTarihi);
            gidenFaturaListesi.Add(gidenFatura);

            gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = "41",
                AltBirimAd = "B Birimi    ",
                TuzelKisiAd = "Deneme Şirketi - 4",
                VergiNo = "6940116151",
                VergiDairesi = "ANKARA",
                Adres = "Göksu Parkı",
                FaksNo = "03121111114",
                BankaAd = "Kuveyt Tür",
                BankaSube = "Kızılay",
                IbanNo = "TR350006267645192879857714",
                IslemTarihi = new DateTime(2022, 1, 31),
                IlAd = "Ankara",
                IlceAd = "Çubuk",
                SatisTuruKod = SatisTur.IhracKayitli.GetHashCode(),
                FaturaGrupTuruKod = FaturaMakbuzGrupTur.Personel.GetHashCode(),
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
                KonaklamaVergisi = 0
            };
            gidenFatura.GibNumarasi = GibNumarasi.RastgeleGibNumarasiOlustur(gidenFatura.IslemTarihi);
            gidenFaturaListesi.Add(gidenFatura);

            gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = "51",
                AltBirimAd = "B Birimi    ",
                TuzelKisiAd = "Deneme Şirketi - 3",
                VergiNo = "20101516422",
                VergiDairesi = "ANKARA",
                Adres = "Dost'un Önü",
                FaksNo = "03121111113",
                BankaAd = "Enpara.Com Bankadan Güzeli",
                BankaSube = "Karakusunlar Şube",
                IbanNo = "TR630006295412435711171488",
                IslemTarihi = new DateTime(2022, 2, 1),
                IlAd = "Ankara",
                IlceAd = "Pursaklar",
                SatisTuruKod = SatisTur.Vadeli.GetHashCode(),
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
                KonaklamaVergisi = 0
            };
            gidenFatura.GibNumarasi = GibNumarasi.RastgeleGibNumarasiOlustur(gidenFatura.IslemTarihi);
            gidenFaturaListesi.Add(gidenFatura);

            gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = "61",
                AltBirimAd = "B Birimi    ",
                GercekKisiTcKimlikNo = "11738150028",
                GercekKisiAd = "Adnan",
                GercekKisiSoyad = "Şenses",
                CepTelefonNo = "05555555555",
                EPostaAdresi = "adnansenses@mynet.com",
                IkametgahAdresi = "Dost'un Önü",
                BankaAd = "Halkbank",
                BankaSube = "Etimesgut Şube",
                IbanNo = "TR540006234114125194267726",
                IslemTarihi = new DateTime(2022, 2, 2),
                IlAd = "Konya",
                IlceAd = "Beyşehir",
                SatisTuruKod = SatisTur.RafFiyatGarantili.GetHashCode(),
                GibNumarasi = "YZG2022000999999",
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
                KonaklamaVergisi = 0
            };
            gidenFatura.GibNumarasi = GibNumarasi.RastgeleGibNumarasiOlustur(gidenFatura.IslemTarihi);
            gidenFaturaListesi.Add(gidenFatura);

            gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = "71",
                AltBirimAd = "B Birimi    ",
                GercekKisiTcKimlikNo = "20077290692",
                GercekKisiAd = "Mehmet Emin",
                GercekKisiSoyad = "Akdeniz",
                CepTelefonNo = "05555555554",
                EPostaAdresi = "memo@yahoo.com",
                IkametgahAdresi = "Fışkiyenin orası",
                BankaAd = "Bank Mellat",
                BankaSube = "Yenimahalle Şube",
                IbanNo = "TR410006276748779926367358",
                IslemTarihi = new DateTime(2022, 1, 25),
                IlAd = "Ağrı",
                IlceAd = "Patnos",
                SatisTuruKod = SatisTur.IstirakeSatilan.GetHashCode(),
                GibNumarasi = "AGR2022000999999",
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
                KonaklamaVergisi = 0
            };
            gidenFatura.GibNumarasi = GibNumarasi.RastgeleGibNumarasiOlustur(gidenFatura.IslemTarihi);
            gidenFaturaListesi.Add(gidenFatura);

            #endregion

            #region Giden Fatura Detayları Ekleme

            var gidenFaturaDetay = new GidenFaturaDetayDTO
            {
                GidenFaturaId = "71",
                BirimFiyat = (decimal) 0.1234,
                Miktar = 1000,
                KdvOran = 0,
                IskontoOran = 0,
                KdvHaricTutar = 1234,
                KdvTutari = 0,
                KonaklamaVergisi = 0,
                GibKisaltma = "KGM",
                IrsaliyeNo = "1",
                MalzemeFaturaAciklamasi = "Ofis demirbaşı için alındı",
                SevkIrsaliyeTarihi = new DateTime(2022, 1, 1),
                SevkIrsaliyesiNo = "34",
                PlakaNo = "06GUC777",
                FaturaUrunTuru = "Dizüstü Bilgisayar",
                Tonaj = 1,
                YuklemeFormuNo = 1234
            };
            gidenFaturaDetayListesi.Add(gidenFaturaDetay);

            gidenFaturaDetay = new GidenFaturaDetayDTO
            {
                GidenFaturaId = "71",
                BirimFiyat = (decimal) 0.1,
                Miktar = 10000,
                KdvOran = 18,
                IskontoOran = 0,
                KdvHaricTutar = 1000,
                KdvTutari = 180,
                KonaklamaVergisi = 0,
                GibKisaltma = "KWT",
                IrsaliyeNo = "1",
                MalzemeFaturaAciklamasi = "Ofis demirbaşı 2 için alındı",
                SevkIrsaliyeTarihi = new DateTime(2022, 1, 1),
                SevkIrsaliyesiNo = "34",
                PlakaNo = "06GUC778",
                FaturaUrunTuru = "Priz",
                Tonaj = 1,
                YuklemeFormuNo = 1236
            };
            gidenFaturaDetayListesi.Add(gidenFaturaDetay);

            gidenFaturaDetay = new GidenFaturaDetayDTO
            {
                GidenFaturaId = "11",
                BirimFiyat = (decimal) 0.1,
                Miktar = 10000,
                KdvOran = 18,
                IskontoOran = 0,
                KdvHaricTutar = 1000,
                KdvTutari = 180,
                KonaklamaVergisi = 20,
                GibKisaltma = "KWT",
                IrsaliyeNo = "1",
                MalzemeFaturaAciklamasi = "Konaklama için alındı",
                SevkIrsaliyeTarihi = new DateTime(2022, 1, 1),
                SevkIrsaliyesiNo = "34",
                PlakaNo = "06GUC778",
                FaturaUrunTuru = "Misafirhane",
                Tonaj = 1,
                YuklemeFormuNo = 1236
            };
            gidenFaturaDetayListesi.Add(gidenFaturaDetay);

            gidenFaturaDetay = new GidenFaturaDetayDTO
            {
                GidenFaturaId = "71",
                BirimFiyat = (decimal) 0.1234,
                Miktar = 1000,
                KdvOran = 1,
                IskontoOran = 0,
                KdvHaricTutar = 1234,
                KdvTutari = (decimal) 12.34,
                KonaklamaVergisi = 0,
                GibKisaltma = "MTR",
                PlakaNo = "06GC7447",
                FaturaUrunTuru = "Çubuk Mikrofon"
            };
            gidenFaturaDetayListesi.Add(gidenFaturaDetay);

            gidenFaturaDetay = new GidenFaturaDetayDTO
            {
                GidenFaturaId = "21",
                BirimFiyat = (decimal) 0.1234,
                Miktar = 1000,
                KdvOran = 1,
                IskontoOran = 0,
                KdvHaricTutar = 1234,
                KdvTutari = (decimal) 12.34,
                KonaklamaVergisi = 0,
                GibKisaltma = "KWT",
                PlakaNo = "06GC7447",
                FaturaUrunTuru = "Çubuk Mikrofon"
            };
            gidenFaturaDetayListesi.Add(gidenFaturaDetay);

            gidenFaturaDetay = new GidenFaturaDetayDTO
            {
                GidenFaturaId = "31",
                BirimFiyat = (decimal) 0.124,
                Miktar = 5000,
                KdvOran = 8,
                IskontoOran = 0,
                KdvHaricTutar = 6020,
                KdvTutari = (decimal) 481.6,
                KonaklamaVergisi = 0,
                GibKisaltma = "MON",
                PlakaNo = "06J1234",
                FaturaUrunTuru = "Mouse"
            };
            gidenFaturaDetayListesi.Add(gidenFaturaDetay);

            gidenFaturaDetay = new GidenFaturaDetayDTO
            {
                GidenFaturaId = "11",
                BirimFiyat = 500,
                Miktar = 120,
                KdvOran = 18,
                IskontoOran = 0,
                KdvHaricTutar = 60000,
                KdvTutari = 12800,
                KonaklamaVergisi = 1200,
                GibKisaltma = "LTR",
                PlakaNo = "06C99876",
                FaturaUrunTuru = "Misafirhane"
            };
            gidenFaturaDetayListesi.Add(gidenFaturaDetay);

            #endregion

            #region Giden Fatura Toplamları Hesaplama ve Olmayanlara Ekleme

            foreach (var item in gidenFaturaListesi)
            {
                if (gidenFaturaDetayListesi.Any(j => j.GidenFaturaId == item.GidenFaturaId))
                {
                    var gidenFaturaDetayListesiTemp = gidenFaturaDetayListesi
                        .Where(j => j.GidenFaturaId == item.GidenFaturaId).ToList();
                    item.KdvHaricTutar = gidenFaturaDetayListesiTemp.Sum(j => j.KdvHaricTutar);
                    item.KdvTutari = gidenFaturaDetayListesiTemp.Sum(j => j.KdvTutari);
                    item.KonaklamaVergisi = gidenFaturaDetayListesiTemp.Sum(j => j.KonaklamaVergisi);
                    item.FaturaTutari = item.KdvHaricTutar + item.KdvTutari + item.KonaklamaVergisi;
                }
                else
                {
                    gidenFaturaDetay = new GidenFaturaDetayDTO
                    {
                        GidenFaturaId = item.GidenFaturaId,
                        BirimFiyat = (decimal) 1.5,
                        Miktar = 1000,
                        KdvOran = 0,
                        IskontoOran = 0,
                        KdvHaricTutar = 1500,
                        KdvTutari = 0,
                        KonaklamaVergisi = 30,
                        GibKisaltma = "MTR",
                        FaturaUrunTuru = "Misafirhane " + item.GidenFaturaId
                    };
                    item.KdvHaricTutar = gidenFaturaDetay.KdvHaricTutar;
                    item.KdvTutari = gidenFaturaDetay.KdvTutari;
                    item.KonaklamaVergisi = gidenFaturaDetay.KonaklamaVergisi;
                    item.FaturaTutari = item.KdvHaricTutar + item.KdvTutari + item.KonaklamaVergisi;
                    gidenFaturaDetayListesi.Add(gidenFaturaDetay);
                }
            }

            #endregion

            #endregion

            #region Müstahsil Makbuzlar

            #region Müstahsil Makbuzları Ekleme

            var mustahsilMakbuzu = new MustahsilMakbuzuDTO
            {
                MustahsilMakbuzuId = "1",
                AltBirimAd = "A Birimi",
                TuzelKisiAd = "Deneme Şirketi - 1",
                VergiNo = "53602329864",
                VergiDairesi = "ANKARA",
                Adres = "Kızılay Gima'nın Önü",
                FaksNo = "03121111111",
                BankaAd = "Türkiye İş Bankası",
                BankaSube = "Kızılay",
                IbanNo = "TR130006297372542652383131",
                IslemTarihi = new DateTime(2022, 4, 13),
                IlAd = "Ankara",
                IlceAd = "Çankaya",
                MustahsilMakbuzuNo = "MLT2022000010998",
                MakbuzGrupTuruKod = FaturaMakbuzGrupTur.Kuspe.GetHashCode(),
                NetTutar = 0,
                GelirVergisi = 0,
                MustahsilMakbuzuTutari = 0
            };
            mustahsilMakbuzuListesi.Add(mustahsilMakbuzu);

            mustahsilMakbuzu = new MustahsilMakbuzuDTO
            {
                MustahsilMakbuzuId = "2",
                AltBirimAd = "B Birimi",
                TuzelKisiAd = "Deneme Şirketi - 2",
                VergiNo = "9250936109",
                VergiDairesi = "ANKARA",
                Adres = "Kızılay YKM",
                FaksNo = "03121111112",
                BankaAd = "Vakıflar Bankası",
                BankaSube = "Kızılay",
                IbanNo = "TR530006291586775935345942",
                IslemTarihi = new DateTime(2022, 4, 1),
                IlAd = "Ankara",
                IlceAd = "Etimesgut",
                MakbuzGrupTuruKod = FaturaMakbuzGrupTur.Personel.GetHashCode(),
                NetTutar = 0,
                GelirVergisi = 0,
                MustahsilMakbuzuTutari = 0
            };
            mustahsilMakbuzu.MustahsilMakbuzuNo = GibNumarasi.RastgeleGibNumarasiOlustur(mustahsilMakbuzu.IslemTarihi);
            mustahsilMakbuzuListesi.Add(mustahsilMakbuzu);

            mustahsilMakbuzu = new MustahsilMakbuzuDTO
            {
                MustahsilMakbuzuId = "c3da32c1-c0f1-4061-8234-ec09f83804fa",
                TuzelKisiAd = "Deneme Şirketi - 3",
                VergiNo = "5240018140",
                VergiDairesi = "ANKARA",
                Adres = "Zafer Çarşısı",
                FaksNo = "03121111113",
                BankaAd = "Vakıf Katılım Bankası",
                BankaSube = "Kızılay",
                IbanNo = "TR830006214925976839299642",
                IslemTarihi = new DateTime(2022, 4, 11),
                IlAd = "Ankara",
                IlceAd = "Etimesgut",
                NetTutar = 0,
                GelirVergisi = 0,
                MustahsilMakbuzuTutari = 0
            };
            mustahsilMakbuzu.MustahsilMakbuzuNo = GibNumarasi.RastgeleGibNumarasiOlustur(mustahsilMakbuzu.IslemTarihi);
            mustahsilMakbuzuListesi.Add(mustahsilMakbuzu);

            mustahsilMakbuzu = new MustahsilMakbuzuDTO
            {
                MustahsilMakbuzuId = "4",
                TuzelKisiAd = "Deneme Şirketi - 4",
                VergiNo = "6940116151",
                VergiDairesi = "ANKARA",
                Adres = "Göksu Parkı",
                FaksNo = "03121111114",
                BankaAd = "Kuveyt Tür",
                BankaSube = "Kızılay",
                IbanNo = "TR350006267645192879857714",
                IslemTarihi = new DateTime(2022, 4, 3),
                IlAd = "Ankara",
                IlceAd = "Çubuk",
                NetTutar = 0,
                GelirVergisi = 0,
                MustahsilMakbuzuTutari = 0
            };
            mustahsilMakbuzu.MustahsilMakbuzuNo = GibNumarasi.RastgeleGibNumarasiOlustur(mustahsilMakbuzu.IslemTarihi);
            mustahsilMakbuzuListesi.Add(mustahsilMakbuzu);

            mustahsilMakbuzu = new MustahsilMakbuzuDTO
            {
                MustahsilMakbuzuId = "5",
                TuzelKisiAd = "Deneme Şirketi - 3",
                VergiNo = "20101516422",
                VergiDairesi = "ANKARA",
                Adres = "Dost'un Önü",
                FaksNo = "03121111113",
                BankaAd = "Enpara.Com Bankadan Güzeli",
                BankaSube = "Karakusunlar Şube",
                IbanNo = "TR630006295412435711171488",
                IslemTarihi = new DateTime(2022, 4, 1),
                IlAd = "Ankara",
                IlceAd = "Pursaklar",
                NetTutar = 0,
                GelirVergisi = 0,
                MustahsilMakbuzuTutari = 0
            };
            mustahsilMakbuzu.MustahsilMakbuzuNo = GibNumarasi.RastgeleGibNumarasiOlustur(mustahsilMakbuzu.IslemTarihi);
            mustahsilMakbuzuListesi.Add(mustahsilMakbuzu);

            mustahsilMakbuzu = new MustahsilMakbuzuDTO
            {
                MustahsilMakbuzuId = "6",
                VergiNo = "52270709114",
                TuzelKisiAd = "Adnan Şenses",
                EPostaAdresi = "adnansenses@mynet.com",
                Adres = "Dost'un Önü",
                BankaAd = "Halkbank",
                BankaSube = "Etimesgut Şube",
                IbanNo = "TR540006234114125194267726",
                IslemTarihi = new DateTime(2022, 4, 2),
                IlAd = "Konya",
                IlceAd = "Beyşehir",
                NetTutar = 0,
                GelirVergisi = 0,
                MustahsilMakbuzuTutari = 0
            };
            mustahsilMakbuzu.MustahsilMakbuzuNo = GibNumarasi.RastgeleGibNumarasiOlustur(mustahsilMakbuzu.IslemTarihi);
            mustahsilMakbuzuListesi.Add(mustahsilMakbuzu);

            mustahsilMakbuzu = new MustahsilMakbuzuDTO
            {
                MustahsilMakbuzuId = "7",
                VergiNo = "20077290692",
                TuzelKisiAd = "Mehmet Emin",
                EPostaAdresi = "memo@yahoo.com",
                BankaAd = "Bank Mellat",
                BankaSube = "Yenimahalle Şube",
                IbanNo = "TR410006276748779926367358",
                IslemTarihi = new DateTime(2022, 4, 25),
                IlAd = "Ağrı",
                IlceAd = "Patnos",
                NetTutar = 0,
                GelirVergisi = 0,
                MustahsilMakbuzuTutari = 0
            };
            mustahsilMakbuzu.MustahsilMakbuzuNo = GibNumarasi.RastgeleGibNumarasiOlustur(mustahsilMakbuzu.IslemTarihi);
            mustahsilMakbuzuListesi.Add(mustahsilMakbuzu);

            #endregion

            #region Müstahsil Makbuz Detayları Ekleme

            var mustahsilMakbuzuDetay = new MustahsilMakbuzuDetayDTO
            {
                MustahsilMakbuzuId = "7",
                BirimFiyat = (decimal) 0.1234,
                Miktar = 1000,
                NetTutar = 1234,
                GelirVergisi = 0,
                GibKisaltma = "KGM",
                IsinMahiyeti = "Dizüstü Bilgisayar"
            };
            mustahsilMakbuzuDetayListesi.Add(mustahsilMakbuzuDetay);

            mustahsilMakbuzuDetay = new MustahsilMakbuzuDetayDTO
            {
                MustahsilMakbuzuId = "7",
                BirimFiyat = (decimal) 0.1234,
                Miktar = 1000,
                NetTutar = 1234,
                GelirVergisi = (decimal) 12.34,
                ToplamTutar = (decimal) 1246.34,
                GibKisaltma = "MTR",
                IsinMahiyeti = "Çubuk Mikrofon"
            };
            mustahsilMakbuzuDetayListesi.Add(mustahsilMakbuzuDetay);

            mustahsilMakbuzuDetay = new MustahsilMakbuzuDetayDTO
            {
                MustahsilMakbuzuId = "2",
                BirimFiyat = (decimal) 0.1234,
                Miktar = 1000,
                NetTutar = 1234,
                GelirVergisi = (decimal) 12.34,
                ToplamTutar = (decimal) 1246.34,
                GibKisaltma = "KWT",
                IsinMahiyeti = "Çubuk Mikrofon"
            };
            mustahsilMakbuzuDetayListesi.Add(mustahsilMakbuzuDetay);

            mustahsilMakbuzuDetay = new MustahsilMakbuzuDetayDTO
            {
                MustahsilMakbuzuId = "3",
                BirimFiyat = (decimal) 0.124,
                Miktar = 5000,
                NetTutar = 6020,
                GelirVergisi = (decimal) 481.6,
                ToplamTutar = (decimal) 6500.6,
                GibKisaltma = "MON",
                IsinMahiyeti = "Mouse"
            };
            mustahsilMakbuzuDetayListesi.Add(mustahsilMakbuzuDetay);

            mustahsilMakbuzuDetay = new MustahsilMakbuzuDetayDTO
            {
                MustahsilMakbuzuId = "1",
                BirimFiyat = 500,
                Miktar = 120,
                NetTutar = 60000,
                GelirVergisi = 12800,
                ToplamTutar = 72800,
                GibKisaltma = "LTR",
                IsinMahiyeti = "Ayakkabı"
            };
            mustahsilMakbuzuDetayListesi.Add(mustahsilMakbuzuDetay);

            #endregion

            #region Müstahsil Makbuz Toplamları Hesaplama ve Olmayanlara Ekleme

            foreach (var item in mustahsilMakbuzuListesi)
            {
                if (mustahsilMakbuzuDetayListesi.Any(j => j.MustahsilMakbuzuId == item.MustahsilMakbuzuId))
                {
                    var mustahsilMakbuzuDetayListesiTemp = mustahsilMakbuzuDetayListesi
                        .Where(j => j.MustahsilMakbuzuId == item.MustahsilMakbuzuId).ToList();
                    item.NetTutar = mustahsilMakbuzuDetayListesiTemp.Sum(j => j.NetTutar);
                    item.GelirVergisi = mustahsilMakbuzuDetayListesiTemp.Sum(j => j.GelirVergisi);
                    item.MustahsilMakbuzuTutari = item.NetTutar + item.GelirVergisi;
                }
                else
                {
                    mustahsilMakbuzuDetay = new MustahsilMakbuzuDetayDTO
                    {
                        MustahsilMakbuzuId = item.MustahsilMakbuzuId,
                        BirimFiyat = (decimal) 1.5,
                        Miktar = 1000,
                        NetTutar = 1500,
                        GelirVergisi = 0,
                        ToplamTutar = 1500,
                        GibKisaltma = "MTR",
                        IsinMahiyeti = "Deneme " + item.MustahsilMakbuzuId
                    };
                    item.NetTutar = mustahsilMakbuzuDetay.NetTutar;
                    item.GelirVergisi = mustahsilMakbuzuDetay.GelirVergisi;
                    item.MustahsilMakbuzuTutari = item.NetTutar + item.GelirVergisi;
                    mustahsilMakbuzuDetayListesi.Add(mustahsilMakbuzuDetay);
                }
            }

            #endregion

            #endregion

            #endregion
        }

        /// <summary>
        /// This method is used to meaure the length of the item in listbox and make it multi line
        /// without using scrollbar and let the user see more efficiently
        /// </summary>
        /// <param name="sender">The sender info (For example Main Form)</param>
        /// <param name="e">Event Arguments</param>
        private static void listBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (!(sender is ListBox listBox)) return;
            if (listBox.Items.Count > 0)
                e.ItemHeight = (int) e.Graphics.MeasureString(listBox.Items[e.Index].ToString(), listBox.Font, listBox.Width).Height;
        }

        /// <summary>
        /// This method is used to meaure the length of the item in listbox and make it multi line
        /// without using scrollbar and let the user see more efficiently
        /// </summary>
        /// <param name="sender">The sender info (For example Main Form)</param>
        /// <param name="e">Event Arguments</param>
        private static void listBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (!(sender is ListBox listBox)) return;
            e.DrawBackground();
            e.DrawFocusRectangle();
            if (listBox.Items.Count > 0)
                e.Graphics.DrawString(listBox.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds);
        }

        /// <summary>
        /// Form kapandığı zaman ne yapılacağını belirten bir metottur
        /// </summary>
        /// <param name="sender">The sender info (For example Main Form)</param>
        /// <param name="e">Event Arguments</param>
        private void frmAnaSayfa_FormClosed(object sender, FormClosedEventArgs e)
        {
            MessageBox.Show("Programı kullandığınız için teşekkürler", MesajSabitler.MesajBasligi, MessageBoxButtons.OK, MessageBoxIcon.Information);
            Application.Exit();
        }

        #region E-Fatura ve E-Arşiv Metotları

        /// <summary>
        /// Sistemde E-Fatura ve E-Arşiv Servislerine Gönderilecek Formatta
        /// İdeal XML dosyalarının oluşturulması için XML Oluştur Butonuna tıklandığı zaman
        /// Yapılacak işlemlerin hazırlandığı metottur.
        /// </summary>
        /// <param name="sender">The sender info (For example Main Form)</param>
        /// <param name="e">Event Arguments</param>
        private void btnXmlOlustur_Click(object sender, EventArgs e)
        {
            using (var dialogKlasorSecimi = new FolderBrowserDialog())
            {
                if (!IslemIcinKlasorSecildiMi(dialogKlasorSecimi, out var klasorAdi)) return;
                IslemYapilacakFaturaBilgisiDuzenle(out var gidenFatura, out var gidenFaturaDetayListesiTemp);

                #region XML Oluşturma

                var eFaturaKullanicisiMi = EFaturaKullanicisiMi(gidenFatura);
                var dosyaAdi = eFaturaKullanicisiMi 
                    ? XMLIslemleri.EFaturaXMLOlustur(gidenFatura, gidenFaturaDetayListesiTemp, klasorAdi) 
                    : XMLIslemleri.EArsivXMLOlustur(gidenFatura, gidenFaturaDetayListesiTemp, klasorAdi,
                        true);

                MessageBox.Show(dosyaAdi + " adresinde gerekli XML dosyası oluşturulmuştur.",
                    MesajSabitler.MesajBasligi, MessageBoxButtons.OK, MessageBoxIcon.Information);

                #endregion
            }
        }

        /// <summary>
        /// Sistemde E-Fatura veya E-Arşive yollansın yollanmasın
        /// Hazırlanan XML dosyalarının bu sistemlerde nasıl göründüğüne dair
        /// Önizleme alınmasını sağlayan metottur. Bu önizleme PDF veya ZIP olarak alınacak
        /// Sorun olsa da olmasa da bir mesaj ile kullanıcı bilgilendirilecektir.
        /// </summary>
        /// <param name="sender">The sender info (For example Main Form)</param>
        /// <param name="e">Event Arguments</param>
        private void btnGIBOnizleme_Click(object sender, EventArgs e)
        {
            using (var dialogKlasorSecimi = new FolderBrowserDialog())
            {
                if (!IslemIcinKlasorSecildiMi(dialogKlasorSecimi, out var klasorAdi)) return;
                IslemYapilacakFaturaBilgisiDuzenle(out var gidenFatura, out var gidenFaturaDetayListesiTemp);

                #region XML Oluşturma ve Servis Önizlemesi

                var geriDonus = new GeriDonus {Tip = 0};
                var dosya = new byte[1];
                var eFaturaKullanicisiMi = EFaturaKullanicisiMi(gidenFatura);
                dosya = eFaturaKullanicisiMi 
                    ? EFaturaXMLOlusturVeOnIzlemeYap(gidenFatura, gidenFaturaDetayListesiTemp, klasorAdi, out var dosyaAdi, out geriDonus, dosya) 
                    : EArsivXMLOlusturVeOnIzlemeYap(gidenFatura, gidenFaturaDetayListesiTemp, klasorAdi, out dosyaAdi);

                var belgeTur = eFaturaKullanicisiMi ? BelgeTur.EFatura : BelgeTur.EArsiv;
                
                FaturaDosyaOnizlemeYap(dosya, dosyaAdi, belgeTur, gidenFatura, geriDonus);

                #endregion
            }
        }

        /// <summary>
        /// Sistem tarafından hazırlanan XML dosyasının
        /// E-Fatura veya E-Arşiv servisine gönderilmesi için hazırlanan metottur.
        /// Burada işlemin başarılı olup olmamasına göre kullanıcı bilgilendirilecektir.
        /// </summary>
        /// <param name="sender">The sender info (For example Main Form)</param>
        /// <param name="e">Event Arguments</param>
        private void btnServiseGonder_Click(object sender, EventArgs e)
        {
            using (var dialogKlasorSecimi = new FolderBrowserDialog())
            {
                if (!IslemIcinKlasorSecildiMi(dialogKlasorSecimi, out var klasorAdi)) return;
                IslemYapilacakFaturaBilgisiDuzenle(out var gidenFatura, out var gidenFaturaDetayListesiTemp);

                #region XML Oluşturma ve Servise Gönderme

                var eFaturaKullanicisiMi = EFaturaKullanicisiMi(gidenFatura);
                var sonuc = eFaturaKullanicisiMi 
                    ? EFaturaXMLOlusturVeServiseGonder(gidenFatura, gidenFaturaDetayListesiTemp, klasorAdi, out var dosyaAdi) 
                    : EArsivXMLOlusturVeServiseGonder(gidenFatura, gidenFaturaDetayListesiTemp, klasorAdi, out dosyaAdi);

                IslemSonucuMesajiHazirla(sonuc, dosyaAdi);

                #endregion
            }
        }

        /// <summary>
        /// Eğer Fatura E-Fatura Mükellefine kesiliyorsa
        /// Ve Belge Oid değeri yoksa,
        /// Daha önce gönderilme ihtimaline karşılık olarak
        /// Bu bilgilerin servis üzerinden temin edilebilmesi için gerekli işlemler gerçekleştirildi
        /// </summary>
        /// <param name="sender">The sender info (For example Main Form)</param>
        /// <param name="e">Event Arguments</param>
        private void btnBelgeOidKontrol_Click(object sender, EventArgs e)
        {
            #region E-Fatura Mükelleflerinin Fatura Id Bilgilerinin Temini

            var gidenFaturaIdListesi = (from item in gidenFaturaListesi where !string.IsNullOrEmpty(item.VergiNo) let eFaturaKullanicisiMi = DisServisler.EFaturaKullanicisiMi(item.VergiNo) where eFaturaKullanicisiMi select item.GidenFaturaId).ToList();

            #endregion

            var islemSonucu = string.Empty;
            if (gidenFaturaIdListesi.Count <= 0) return;
            {
                var builder = new StringBuilder();
                builder.Append(islemSonucu);
                foreach (var item in gidenFaturaIdListesi)
                {
                    var sonuc = DisServisler.BelgeOidKontrol(item, DateTime.Now.AddMonths(-3), DateTime.Now);
                    if (sonuc != MesajSabitler.IslemBasarili && sonuc != MesajSabitler.IslemBasarisiz)
                        builder.Append(item + " belge numaralı faturanın Belge Oid değeri: " + sonuc + " ,");
                }

                islemSonucu = builder.ToString();
                if (islemSonucu.Length <= 2) return;
                islemSonucu = islemSonucu.Substring(0, islemSonucu.Length - 2);
                MessageBox.Show(islemSonucu, MesajSabitler.MesajBasligi, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Faturanın sisteme gönderilip gönderilmediği kontrol edilerek
        /// Buna göre silinip silinmeyeceğina karar verilmesini sağlayan metottur.
        /// </summary>
        /// <param name="sender">The sender info (For example Main Form)</param>
        /// <param name="e">Event Arguments</param>
        private void btnFaturaSil_Click(object sender, EventArgs e)
        {
            var index = new Random().Next(gidenFaturaListesi.Count);
            var gidenFatura = gidenFaturaListesi[index];

            if (!string.IsNullOrEmpty(gidenFatura.VergiNo))
            {
                var eFaturaKullanicisiMi = DisServisler.EFaturaKullanicisiMi(gidenFatura.VergiNo);
                var sonuc = eFaturaKullanicisiMi
                    ? DisServisler.EFaturaSilmeyeUygunMu(gidenFatura.GidenFaturaId)
                    : DisServisler.EArsivSilmeyeUygunMu(gidenFatura.GidenFaturaId);

                if (sonuc)
                    MessageBox.Show(gidenFatura.GidenFaturaId + " yerel belge numaralı fatura silinebilir",
                        MesajSabitler.MesajBasligi, MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(
                        gidenFatura.GidenFaturaId +
                        " yerel belge numaralı fatura GİB servislerine gönderildiği, onaylandığı veya onay beklediği için silinemez",
                        MesajSabitler.MesajBasligi, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("Vergi Numarası olmayan bir fatura silinemez.", MesajSabitler.MesajBasligi,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Ekrandan girilne tarihten öncesine ait
        /// Kayıtlı E-Fatura Mükellef Vergi Numara listesini getiren
        /// Ve Listbox'ı dolduran metottur.
        /// </summary>
        /// <param name="sender">The sender info (For example Main Form)</param>
        /// <param name="e">Event Arguments</param>
        private void btnEFaturaKullaniciListesi_Click(object sender, EventArgs e)
        {
            lbEFaturaKullaniciListesi.Items.Clear();
            var vergiKimlikNoListesi = DisServisler.EFaturaKullaniciListesi(dtpFaturaTarihi.Value);
            foreach (var item in vergiKimlikNoListesi.Where(item => !string.IsNullOrEmpty(item)))
                lbEFaturaKullaniciListesi.Items.Add(item);

            MessageBox.Show(vergiKimlikNoListesi.Count + " adet e-fatura mükellef kaydı bulundu",
                MesajSabitler.MesajBasligi, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Ekrandan girilne tarihten öncesine ait
        /// Kayıtlı E-Fatura Mükellef Vergi Numara listesini dışarıya TXT formatında aktaran metottur
        /// </summary>
        /// <param name="sender">The sender info (For example Main Form)</param>
        /// <param name="e">Event Arguments</param>
        private void btnEFaturaKullaniciListeAktar_Click(object sender, EventArgs e)
        {
            if (lbEFaturaKullaniciListesi.Items.Count < 1)
                MessageBox.Show("Aktarılacak bir E-Fatura Kullanıcı listesi bulunamadı", MesajSabitler.MesajBasligi,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                using (var dialogKlasorSecimi = new FolderBrowserDialog())
                {
                    if (!IslemIcinKlasorSecildiMi(dialogKlasorSecimi, out var klasorAdi)) return;

                    var dosyaAdi = EFaturaKullaniciListesiniTXTFormatindaKaydet(klasorAdi);
                    MessageBox.Show(dosyaAdi + " dosyası üzerinde kayıtlı E-Fatura Mükellefleri listelenmiştir.",
                        MesajSabitler.MesajBasligi, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// Ekrandan girilne tarihten öncesine ait
        /// Kayıtlı E-Fatura Mükellef Vergi Numara listesini temizlemek için hazırlanan metottur
        /// </summary>
        /// <param name="sender">The sender info (For example Main Form)</param>
        /// <param name="e">Event Arguments</param>
        private void btnEFaturaKullaniciListesiTemizle_Click(object sender, EventArgs e)
        {
            var kayitSayisi = lbEFaturaKullaniciListesi.Items.Count;
            lbEFaturaKullaniciListesi.Items.Clear();
            MessageBox.Show(kayitSayisi + " adet E-Fatura Mükellefi sağdaki listeden temizlenmiştir.",
                MesajSabitler.MesajBasligi, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region E-Müstahsil Metotları

        /// <summary>
        /// Sistemde E-Müstahsil Servislerine Gönderilecek Formatta
        /// İdeal XML dosyalarının oluşturulması için XML Oluştur Butonuna tıklandığı zaman
        /// Yapılacak işlemlerin hazırlandığı metottur.
        /// </summary>
        /// <param name="sender">The sender info (For example Main Form)</param>
        /// <param name="e">Event Arguments</param>
        private void btnXmlOlusturMustahsil_Click(object sender, EventArgs e)
        {
            using (var dialogKlasorSecimi = new FolderBrowserDialog())
            {
                if (!IslemIcinKlasorSecildiMi(dialogKlasorSecimi, out var klasorAdi)) return;
                IslemYapilacakMakbuzBilgisiDuzenle(out var mustahsilMakbuzu, out var mustahsilMakbuzuDetayListesiTemp);

                #region XML Oluşturma

                if (!string.IsNullOrEmpty(mustahsilMakbuzu.VergiNo))
                {
                    var dosyaAdi = XMLIslemleri.EMustahsilXMLOlustur(mustahsilMakbuzu,
                        mustahsilMakbuzuDetayListesiTemp, klasorAdi);
                    MessageBox.Show(dosyaAdi + " adresinde gerekli XML dosyası oluşturulmuştur.",
                        MesajSabitler.MesajBasligi, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Müstahsil Makbuzu oluşturmak için vergi numarası zorunludur",
                        MesajSabitler.MesajBasligi, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                #endregion
            }
        }

        /// <summary>
        /// Sistemde E-Müstahsil Servisine yollansın yollanmasın
        /// Hazırlanan XML dosyalarının bu sistemlerde nasıl göründüğüne dair
        /// Önizleme alınmasını sağlayan metottur. Bu önizleme PDF veya ZIP olarak alınacak
        /// Sorun olsa da olmasa da bir mesaj ile kullanıcı bilgilendirilecektir.
        /// </summary>
        /// <param name="sender">The sender info (For example Main Form)</param>
        /// <param name="e">Event Arguments</param>
        private void btnGIBOnizlemeMustahsil_Click(object sender, EventArgs e)
        {
            using (var dialogKlasorSecimi = new FolderBrowserDialog())
            {
                if (!IslemIcinKlasorSecildiMi(dialogKlasorSecimi, out var klasorAdi)) return;
                IslemYapilacakMakbuzBilgisiDuzenle(out var mustahsilMakbuzu, out var mustahsilMakbuzuDetayListesiTemp);

                #region XML Oluşturma ve Servis Önizlemesi

                var dosyaAdi = "";
                var dosya = new byte[1];
                if (!string.IsNullOrEmpty(mustahsilMakbuzu.VergiNo))
                {
                    dosyaAdi = XMLIslemleri.EMustahsilXMLOlustur(mustahsilMakbuzu, mustahsilMakbuzuDetayListesiTemp,
                        klasorAdi);
                    dosya = DisServisler.EMustahsilOnIzleme(mustahsilMakbuzu, dosyaAdi);
                }
                else
                {
                    MessageBox.Show("Müstahsil Makbuzunu servise göndermek için vergi no zorunludur",
                        MesajSabitler.MesajBasligi, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                MustahsilMakbuzuDosyaOnizlemeYap(dosya, dosyaAdi);

                #endregion
            }
        }

        /// <summary>
        /// Sistem tarafından hazırlanan XML dosyasının
        /// E-Müstahsil servisine gönderilmesi için hazırlanan metottur.
        /// Burada işlemin başarılı olup olmamasına göre kullanıcı bilgilendirilecektir.
        /// </summary>
        /// <param name="sender">The sender info (For example Main Form)</param>
        /// <param name="e">Event Arguments</param>
        private void btnServiseGonderMustahsil_Click(object sender, EventArgs e)
        {
            using (var dialogKlasorSecimi = new FolderBrowserDialog())
            {
                if (!IslemIcinKlasorSecildiMi(dialogKlasorSecimi, out var klasorAdi)) return;
                IslemYapilacakMakbuzBilgisiDuzenle(out var mustahsilMakbuzu, out var mustahsilMakbuzuDetayListesiTemp);

                #region XML Oluşturma ve Servise Gönderme

                var dosyaAdi = "";
                var sonuc = "";
                if (!string.IsNullOrEmpty(mustahsilMakbuzu.VergiNo))
                {
                    dosyaAdi = XMLIslemleri.EMustahsilXMLOlustur(mustahsilMakbuzu, mustahsilMakbuzuDetayListesiTemp,
                        klasorAdi);
                    sonuc = DisServisler.EMustahsilGonder(mustahsilMakbuzu, dosyaAdi);
                }
                else
                {
                    MessageBox.Show("Müstahsil Makbuzunu servise göndermek için vergi no zorunludur",
                        MesajSabitler.MesajBasligi, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                IslemSonucuMesajiHazirla(sonuc, dosyaAdi);

                #endregion
            }
        }

        /// <summary>
        /// Makbuzun sisteme gönderilip gönderilmediği kontrol edilerek
        /// Buna göre silinip silinmeyeceğina karar verilmesini sağlayan metottur.
        /// </summary>
        /// <param name="sender">The sender info (For example Main Form)</param>
        /// <param name="e">Event Arguments</param>
        private void btnMakbuzSil_Click(object sender, EventArgs e)
        {
            var index = new Random().Next(mustahsilMakbuzuListesi.Count);
            var mustahsilMakbuzu = mustahsilMakbuzuListesi[index];

            if (!string.IsNullOrEmpty(mustahsilMakbuzu.VergiNo))
            {
                var sonuc = DisServisler.EMustahsilSilmeyeUygunMu(mustahsilMakbuzu.MustahsilMakbuzuId);

                if (sonuc)
                    MessageBox.Show(mustahsilMakbuzu.MustahsilMakbuzuId + " yerel belge numaralı makbuz silinebilir",
                        MesajSabitler.MesajBasligi, MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(
                        mustahsilMakbuzu.MustahsilMakbuzuId +
                        " yerel belge numaralı makbuz GİB servislerine gönderildiği, onaylandığı veya onay beklediği için silinemez",
                        MesajSabitler.MesajBasligi, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("Vergi Numarası olmayan bir makbuz silinemez.", MesajSabitler.MesajBasligi,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Diğer Yardımcı Metotlar
        
        /// <summary>
        /// Faturadaki bilgiler kontrol edilerek
        /// Vergi No veya TC Kimlik No üzerinden
        /// Söz konusu mükellefin E-Fatura
        /// Mükellefi olup olmadığının kontrol edildiği metottur.
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <returns>Şahsın E-Fatura Mükellefi Olup Olmadığı Bilgisi</returns>
        private static bool EFaturaKullanicisiMi(GidenFaturaDTO gidenFatura)
        {
            var eFaturaKullanicisiMi = false;
            if (!string.IsNullOrEmpty(gidenFatura.VergiNo))
                eFaturaKullanicisiMi = DisServisler.EFaturaKullanicisiMi(gidenFatura.VergiNo);
            else
            {
                if (!string.IsNullOrEmpty(gidenFatura.GercekKisiTcKimlikNo))
                    eFaturaKullanicisiMi = DisServisler.EFaturaKullanicisiMi(gidenFatura.GercekKisiTcKimlikNo);
            }

            return eFaturaKullanicisiMi;
        }
        
        /// <summary>
        /// Burada oluşan E-Fatura Kullanıcı Listesini
        /// TXT Dosyasına kaydetmek için
        /// Gerekli işlemlerin gerçekleştirildiği kod blokları bulunmaktadır
        /// </summary>
        /// <param name="klasorAdi">Dosyanın kaydedileceği klasör adı</param>
        /// <returns>Dosyanın adı</returns>
        private string EFaturaKullaniciListesiniTXTFormatindaKaydet(string klasorAdi)
        {
            var ad = Guid.NewGuid().ToString();
            var dosyaAdi = klasorAdi + "/" + ad + ".txt";
            var dosyaKaydi = new StreamWriter(dosyaAdi);
            foreach (var item in lbEFaturaKullaniciListesi.Items)
                dosyaKaydi.WriteLine(item.ToString());
            dosyaKaydi.Close();
            return dosyaAdi;
        }
        
        /// <summary>
        /// E-Fatura veya E-Arşiv Faturası önizlemesi yapılırken
        /// Servisten gelen değere göre
        /// Dosyanın önizlemesinin yapılıp yapılmaması ile ilgili işlemlerin gerçekleştirildiği metottur.
        /// </summary>
        /// <param name="dosya">Servisten Dönen Byte Dizisi</param>
        /// <param name="dosyaAdi">Hedef Dosya Adı</param>
        /// <param name="belgeTur">Belge Türü Bilgisi</param>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="geriDonus">Servisten Gelen geri Dönüş Bilgisi</param>
        private static void FaturaDosyaOnizlemeYap(byte[] dosya, string dosyaAdi, BelgeTur belgeTur, GidenFaturaDTO gidenFatura, GeriDonus geriDonus)
        {
            if (dosya != null && dosya.Length > 1)
            {
                var dosyaAdiTemp = dosyaAdi.Replace("xml", "pdf");
                
                if (belgeTur == BelgeTur.EFatura && !string.IsNullOrEmpty(gidenFatura.BelgeOid))
                    dosya = ZipDosyasindanPdfCikar(dosya);
                if (geriDonus != null && geriDonus.Tip == 1)
                    dosya = ZipDosyasindanPdfCikar(dosya);

                File.WriteAllBytes(dosyaAdiTemp, dosya);

                MessageBox.Show(dosyaAdiTemp + " adresinde gerekli PDF veya ZIP dosyası oluşturulmuştur.",
                    MesajSabitler.MesajBasligi, MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (File.Exists(dosyaAdiTemp))
                    System.Diagnostics.Process.Start(dosyaAdiTemp);
            }
            else
            {
                MessageBox.Show("Söz konusu faturanın önizlemesi oluşturulamamıştır", MesajSabitler.MesajBasligi,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Önizleme için servise gönderme işlemi gerçekleştikten sonra
        /// Servisten gelen byte dizisinin durumuna göre
        /// Makbuzun servis önizlemesinin yapılıp yapılmaması ile ilgili işlemlerin
        /// Gerçekleştirildiği metottur
        /// </summary>
        /// <param name="dosya">Servisten Dönen Byte Dizisi</param>
        /// <param name="dosyaAdi">Hedef Dosya Adı</param>
        private static void MustahsilMakbuzuDosyaOnizlemeYap(byte[] dosya, string dosyaAdi)
        {
            if (dosya != null && dosya.Length > 1)
            {
                var dosyaAdiTemp = dosyaAdi.Replace("xml", "pdf");

                File.WriteAllBytes(dosyaAdiTemp, dosya);

                MessageBox.Show(dosyaAdiTemp + " adresinde gerekli PDF veya ZIP dosyası oluşturulmuştur.",
                    MesajSabitler.MesajBasligi, MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (File.Exists(dosyaAdiTemp))
                    System.Diagnostics.Process.Start(dosyaAdiTemp);
            }
            else
            {
                MessageBox.Show("Söz konusu faturanın önizlemesi oluşturulamamıştır", MesajSabitler.MesajBasligi,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Klasör Seçimi Yapılması ve klasör seçilirse
        /// Bu klasör adının alınması için gerekli işlemlerin gerçekleştirildiği metottur.
        /// </summary>
        /// <param name="dialogKlasorSecimi">Klasör Seçim Diyaloğu</param>
        /// <param name="klasorAdi">Boş Klasör Adı</param>
        /// <returns>Klasör Seçiminin Yapılıp Yapılmadığı Bilgisi</returns>
        private static bool IslemIcinKlasorSecildiMi(FolderBrowserDialog dialogKlasorSecimi, out string klasorAdi)
        {
            dialogKlasorSecimi.SelectedPath = Application.StartupPath;
            var klasorSecimDiyalogu = dialogKlasorSecimi.ShowDialog();

            if (klasorSecimDiyalogu != DialogResult.OK ||
                string.IsNullOrWhiteSpace(dialogKlasorSecimi.SelectedPath))
            {
                klasorAdi = null;
                return false;
            }

            klasorAdi = dialogKlasorSecimi.SelectedPath;
            return true;
        }

        /// <summary>
        /// İşlem yapılacak Giden Fatura ve detay bilgilerinin düzenlenmesi için
        /// Gerekli işlemlerin gerçekleştirildiği metottur.
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="gidenFaturaDetayListesiTemp">Giden Fatura Detay Listesi Bilgisi</param>
        private void IslemYapilacakFaturaBilgisiDuzenle(out GidenFaturaDTO gidenFatura,
            out List<GidenFaturaDetayDTO> gidenFaturaDetayListesiTemp)
        {
            // Burada form yüklendiği zaman hazırlanan giden faturalardan rastgele birinin
            // ve ona ait detay kayıtlarının seçilmesi sağlanıyor
            var index = new Random().Next(gidenFaturaListesi.Count);
            gidenFatura = gidenFaturaListesi[index];
            gidenFaturaDetayListesiTemp = new List<GidenFaturaDetayDTO>();
            var dto = gidenFatura;
            if (gidenFaturaDetayListesi.Any(j => j.GidenFaturaId == dto.GidenFaturaId))
                gidenFaturaDetayListesiTemp =
                    gidenFaturaDetayListesi.Where(j => j.GidenFaturaId == dto.GidenFaturaId).ToList();
        }

        /// <summary>
        /// İşlem yapılacak Müstahsil Makbuzu ve detay bilgilerinin düzenlenmesi için
        /// Gerekli işlemlerin gerçekleştirildiği metottur.
        /// </summary>
        /// <param name="mustahsilMakbuzu">Müstahsil Makbuzu Bilgisi</param>
        /// <param name="mustahsilMakbuzuDetayListesiTemp">Müstahsil Makbuzu Detay Listesi Bilgisi</param>
        private void IslemYapilacakMakbuzBilgisiDuzenle(out MustahsilMakbuzuDTO mustahsilMakbuzu,
            out List<MustahsilMakbuzuDetayDTO> mustahsilMakbuzuDetayListesiTemp)
        {
            // Burada form yüklendiği zaman hazırlanan müstahsil makbuzlarından rastgele birinin
            // ve ona ait detay kayıtlarının seçilmesi sağlanıyor
            var index = new Random().Next(mustahsilMakbuzuListesi.Count);
            mustahsilMakbuzu = mustahsilMakbuzuListesi[index];
            mustahsilMakbuzuDetayListesiTemp = new List<MustahsilMakbuzuDetayDTO>();
            var dto = mustahsilMakbuzu;
            if (mustahsilMakbuzuDetayListesi.Any(j => j.MustahsilMakbuzuId == dto.MustahsilMakbuzuId))
                mustahsilMakbuzuDetayListesiTemp = mustahsilMakbuzuDetayListesi
                    .Where(j => j.MustahsilMakbuzuId == dto.MustahsilMakbuzuId).ToList();
        }

        /// <summary>
        /// Servise gönderme konusunda sonuca göre bilgi mesajı verilmesini sağlayan metottur
        /// </summary>
        /// <param name="sonuc">İşlem Sonucu Bilgisi</param>
        /// <param name="dosyaAdi">Dosya Adı Bilgisi</param>
        private static void IslemSonucuMesajiHazirla(string sonuc, string dosyaAdi)
        {
            if (sonuc == MesajSabitler.IslemBasarisiz)
            {
                MessageBox.Show(dosyaAdi + " dosyasının servise gönderilmesinde bir sorun yaşanmıştır.",
                    MesajSabitler.MesajBasligi, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show(dosyaAdi + " dosyası başarıyla GİB servislerine gönderilmiştir.",
                    MesajSabitler.MesajBasligi,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// E-Fatura Sisteminden Onaylı Fatura Çıktısı PDF yerine ZIP Formatında
        /// Geldiği için bu ZIP formatındaki byte array üzerinden
        /// İçindeki PDF dosyasının alınıp yazdırılması için
        /// Gerkli metottur
        /// </summary>
        /// <param name="dosya">ZIP Byte Array</param>
        /// <returns>PDF Byte Array</returns>
        private static byte[] ZipDosyasindanPdfCikar(byte[] dosya)
        {
            var streamZip = new MemoryStream(dosya);
            using (var zip = new ZipArchive(streamZip, ZipArchiveMode.Update))
            {
                foreach (var entry in zip.Entries)
                {
                    var streamTemp = entry.Open();
                    byte[] bytes;
                    using (var ms = new MemoryStream())
                    {
                        streamTemp.CopyTo(ms);
                        bytes = ms.ToArray();
                    }

                    dosya = bytes;
                }
            }

            return dosya;
        }

        /// <summary>
        /// E-Fatura Servisinden Faturanın Önizlemesini Yapmak İçin
        /// XML Oluşturan ve bunu ilgili servislere gönderen metottur.
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="gidenFaturaDetayListesiTemp">Giden Fatura Detay Listesi Bilgisi</param>
        /// <param name="klasorAdi">Dosyanın Çıkarılacağı Klasör Bilgisi</param>
        /// <param name="dosyaAdi">Oluşacak Dosya Adı Bilgisi</param>
        /// <param name="geriDonus">Servis Geri Dönüş Bilgisi</param>
        /// <param name="dosya">Oluşan Dosya Bilgisi</param>
        /// <returns>Dosya Bilgisi</returns>
        private static byte[] EFaturaXMLOlusturVeOnIzlemeYap(GidenFaturaDTO gidenFatura, List<GidenFaturaDetayDTO> gidenFaturaDetayListesiTemp, string klasorAdi, out string dosyaAdi, out GeriDonus geriDonus, byte[] dosya)
        {
            dosyaAdi = XMLIslemleri.EFaturaXMLOlustur(gidenFatura, gidenFaturaDetayListesiTemp,
                klasorAdi);
            geriDonus = DisServisler.EFaturaOnIzleme(gidenFatura, dosyaAdi);
            if (geriDonus != null)
                dosya = geriDonus.Dosya;
            return dosya;
        }

        /// <summary>
        /// E-Arşiv Servisinden Faturanın Önizlemesini Yapmak İçin
        /// XML Oluşturan ve bunu ilgili servislere gönderen metottur.
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="gidenFaturaDetayListesiTemp">Giden Fatura Detay Listesi Bilgisi</param>
        /// <param name="klasorAdi">Dosyanın Çıkarılacağı Klasör Bilgisi</param>
        /// <param name="dosyaAdi">Oluşacak Dosya Adı Bilgisi</param>
        /// <returns>Dosya Adı Bilgisi</returns>
        private static byte[] EArsivXMLOlusturVeOnIzlemeYap(GidenFaturaDTO gidenFatura, List<GidenFaturaDetayDTO> gidenFaturaDetayListesiTemp, string klasorAdi, out string dosyaAdi)
        {
            dosyaAdi = XMLIslemleri.EArsivXMLOlustur(gidenFatura, gidenFaturaDetayListesiTemp,
                klasorAdi, true);
            var dosya = DisServisler.EArsivOnIzleme(gidenFatura, dosyaAdi);
            return dosya;
        }

        /// <summary>
        /// E-Fatura Servisine Fatura gönderimi İçin
        /// XML Oluşturan ve bunu ilgili servislere gönderen metottur.
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="gidenFaturaDetayListesiTemp">Giden Fatura Detay Listesi Bilgisi</param>
        /// <param name="klasorAdi">Dosyanın Çıkarılacağı Klasör Bilgisi</param>
        /// <param name="dosyaAdi">Oluşacak Dosya Adı Bilgisi</param>
        /// <returns>Dosya Adı Bilgisi</returns>
        private static string EFaturaXMLOlusturVeServiseGonder(GidenFaturaDTO gidenFatura, List<GidenFaturaDetayDTO> gidenFaturaDetayListesiTemp, string klasorAdi, out string dosyaAdi)
        {
            dosyaAdi = XMLIslemleri.EFaturaXMLOlustur(gidenFatura, gidenFaturaDetayListesiTemp,
                klasorAdi);
            var sonuc = DisServisler.EFaturaGonder(gidenFatura, dosyaAdi, gidenFatura.BelgeOid);
            if (sonuc == MesajSabitler.IslemBasarisiz) return sonuc;
            if (sonuc.Length <= 20)
                gidenFatura.BelgeOid = sonuc;

            return sonuc;
        }

        /// <summary>
        /// E-Arşiv Servisine Fatura gönderimi İçin
        /// XML Oluşturan ve bunu ilgili servislere gönderen metottur.
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="gidenFaturaDetayListesiTemp">Giden Fatura Detay Listesi Bilgisi</param>
        /// <param name="klasorAdi">Dosyanın Çıkarılacağı Klasör Bilgisi</param>
        /// <param name="dosyaAdi">Oluşacak Dosya Adı Bilgisi</param>
        /// <returns>Dosya Adı Bilgisi</returns>
        private static string EArsivXMLOlusturVeServiseGonder(GidenFaturaDTO gidenFatura, List<GidenFaturaDetayDTO> gidenFaturaDetayListesiTemp, string klasorAdi, out string dosyaAdi)
        {
            dosyaAdi = XMLIslemleri.EArsivXMLOlustur(gidenFatura, gidenFaturaDetayListesiTemp,
                klasorAdi, false);
            var sonuc = DisServisler.EArsivGonder(gidenFatura, dosyaAdi);
            return sonuc;
        }

        #endregion
    }
}