using QNBFinansGIB.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using static QNBFinansGIB.Utils.Enums;

namespace QNBFinansGIB.Utils
{
    /// <summary>
    /// Giden Fatura ve Müstahsil Makbuzları ile ilgili olarak, Kendisi, Ana Kayda İlişkin Detaylar, Aktarılacak Klasör gibi bilgiler kullanılarak
    /// QNB Finans GİB Web Servislerine gönderilecek XML dosyalarının hazırlanması ile ilgili
    /// Metotların bulunduğu yardımcı sınıftır.
    /// </summary>
    public static class XMLIslemleri
    {
        /// <summary>
        /// Giden Fatura Bilgisi Üzerinden E-Fatura Sistemine Göndermek İçin
        /// XML Dosyası Oluşturmaya yarayan metottur
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="gidenFaturaDetayListesi">Giden Fatura Detay Listesi</param>
        /// <param name="aktarilacakKlasorAdi">Oluşturulan Dosyanın Aktarılacağı Klasör</param>
        /// <returns>Kaydedilen Dosyanın Bilgisayardaki Adresi</returns>
        public static string EFaturaXMLOlustur(GidenFaturaDTO gidenFatura, List<GidenFaturaDetayDTO> gidenFaturaDetayListesi, string aktarilacakKlasorAdi)
        {
            var doc = FaturaXMLDokumaniIlkHaliOlustur(BelgeTur.EFatura, ref gidenFatura, out var root, out var kodSatisTuruKod, out var kodFaturaTuruKod, out var kodFaturaGrupTuruKod);

            if (string.IsNullOrEmpty(gidenFatura.VergiNo) && !string.IsNullOrEmpty(gidenFatura.GercekKisiTcKimlikNo))
                gidenFatura.VergiNo = gidenFatura.GercekKisiTcKimlikNo;

            // Faturanın türü, düzenleme tarihi, UBL Versiyon Numarası, GİB Numarası gibi bilgiler
            // yer almaktadır

            root.RemoveAllAttributes();
            
            EFaturaHeaderBilgisiDuzenle(doc, root, out var xmlnscac, out var xmlnscbc);

            TemelBilgileriDuzenle("TEMELFATURA", gidenFatura, gidenFaturaDetayListesi, root, doc, kodSatisTuruKod, kodFaturaTuruKod, xmlnscbc);

            IrsaliyeBilgisiDuzenle(gidenFatura, gidenFaturaDetayListesi, root, doc, kodFaturaTuruKod, kodFaturaGrupTuruKod, xmlnscbc);

            ParaBirimiVeKayitSayisiDuzenle(gidenFaturaDetayListesi.Count, root, doc, xmlnscbc);

            EFaturaEkDokumanReferansDuzenle(gidenFatura, doc, xmlnscac, xmlnscbc, root);
           
            FaturaMakbuzImzaBilgisiDuzenle(root, doc, xmlnscac, xmlnscbc);

            FaturaKesenFirmaBilgisiDuzenle(gidenFatura, root, doc, xmlnscac, xmlnscbc);

            EFaturaKesilenFirmaBilgileriDuzenle(gidenFatura, doc, xmlnscac, xmlnscbc, root);

            FaturaKesilenKisiBilgisiDuzenle(gidenFatura, doc, xmlnscac, xmlnscbc, root);

            EFaturaBankaBilgileriDuzenle(gidenFatura, doc, xmlnscac, xmlnscbc, root);

            FaturaKdvBelirle(gidenFatura, gidenFaturaDetayListesi, out var faturaKdvListesi);

            GenelVergiBilgileriDuzenle(doc, xmlnscac, xmlnscbc, kodFaturaGrupTuruKod, kodSatisTuruKod, root,
                faturaKdvListesi);

            // Burada vergi bilgileri yer almaktadır (Açıklama Satırı haline getirildi)

            #region Açıklama Satırı (TaxTotal)

            //var taxTotal = doc.CreateElement("cac", "TaxTotal", xmlnscac.Value);
            //var currencyId = doc.CreateAttribute("currencyID");
            //currencyId.Value = "TRY";
            //var taxAmount = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
            //taxAmount.RemoveAllAttributes();
            //taxAmount.Attributes.Append(currencyId);
            //taxAmount.InnerText = Decimal.Round((decimal)gidenFatura.KdvTutari, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".");
            //taxTotal.AppendChild(taxAmount);
            //var taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
            //var taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
            //taxableAmount.RemoveAllAttributes();
            //currencyId = doc.CreateAttribute("currencyID");
            //currencyId.Value = "TRY";
            //taxableAmount.Attributes.Append(currencyId);
            //taxableAmount.InnerText = Decimal.Round((decimal)gidenFatura.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".");
            //taxSubTotal.AppendChild(taxableAmount);
            //var taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
            //taxAmount2.RemoveAllAttributes();
            //currencyId = doc.CreateAttribute("currencyID");
            //currencyId.Value = "TRY";
            //taxAmount2.Attributes.Append(currencyId);
            //taxAmount2.InnerText = Decimal.Round((decimal)gidenFatura.KdvTutari, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".");
            //taxSubTotal.AppendChild(taxAmount2);
            //var percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
            //if (gidenFatura.KdvHaricTutar != 0)
            //    percent.InnerText = Decimal.Round(((decimal)gidenFatura.KdvTutari * 100) / (decimal)gidenFatura.KdvHaricTutar, 0, MidpointRounding.AwayFromZero).ToString();
            //else
            //    percent.InnerText = "0";
            //taxSubTotal.AppendChild(percent);
            //var taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
            //if (gidenFatura.KdvTutari == 0)
            //{
            //    var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
            //    taxExemptionReasonCode.InnerText = "325";
            //    var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
            //    taxExemptionReason.InnerText = "13/ı Yem Teslimleri";
            //    taxCategory.AppendChild(taxExemptionReasonCode);
            //    taxCategory.AppendChild(taxExemptionReason);
            //}
            //if (kodSatisTuruKod == SatisTur.IhracKayitli.GetHashCode())
            //{
            //    var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
            //    taxExemptionReasonCode.InnerText = "701";
            //    var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
            //    taxExemptionReason.InnerText = "3065 sayılı Katma Değer Vergisi kanununun 11/1-c maddesi kapsamında ihraç edilmek şartıyla teslim edildiğinden Katma Değer Vergisi tahsil edilmemiştir.";
            //    taxCategory.AppendChild(taxExemptionReasonCode);
            //    taxCategory.AppendChild(taxExemptionReason);
            //}
            //var taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
            //var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
            //taxTypeCode.InnerText = "0015";
            //taxScheme2.AppendChild(taxTypeCode);
            //taxCategory.AppendChild(taxScheme2);
            //taxSubTotal.AppendChild(taxCategory);
            //taxTotal.AppendChild(taxSubTotal);
            //root.AppendChild(taxTotal);

            #endregion

            FaturaGenelTutarDuzenle(gidenFatura, doc, xmlnscac, xmlnscbc, root, true);

            gidenFaturaDetayListesi.Aggregate(1, (current, item) => EFaturaDetayBilgileriniDuzenle(gidenFatura, item, doc, xmlnscac, xmlnscbc, current, kodFaturaGrupTuruKod, root));

            doc.AppendChild(root);

            return XMLDosyasiniKaydet(gidenFatura.Id, aktarilacakKlasorAdi, doc);
        }

        /// <summary>
        /// Giden Fatura Bilgisi Üzerinden E-Arşiv Sistemine Göndermek İçin
        /// XML Dosyası Oluşturmaya yarayan metottur
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="gidenFaturaDetayListesi">Giden Fatura Detay Listesi</param>
        /// <param name="aktarilacakKlasorAdi">Oluşturulan Dosyanın Aktarılacağı Klasör</param>
        /// <param name="tuzelKisilikMi">Tüzel Kişilik Mi (Şahıs veya Şahıs Şirketi Değil) Bilgisi</param>
        /// <returns>Kaydedilen Dosyanın Bilgisayardaki Adresi</returns>
        public static string EArsivXMLOlustur(GidenFaturaDTO gidenFatura, List<GidenFaturaDetayDTO> gidenFaturaDetayListesi, string aktarilacakKlasorAdi, bool tuzelKisilikMi)
        {
            var doc = FaturaXMLDokumaniIlkHaliOlustur(BelgeTur.EArsiv, ref gidenFatura, out var root, out var kodSatisTuruKod, out var kodFaturaTuruKod, out var kodFaturaGrupTuruKod);
            
            EArsivStandartImzaVeFirmaBilgisiDuzenle(gidenFatura, gidenFaturaDetayListesi, root, doc,
                kodSatisTuruKod, kodFaturaTuruKod, kodFaturaGrupTuruKod, out var xmlnscac, out var xmlnscbc);

            if (!tuzelKisilikMi)
                EArsivSahisSirketineAitKismiHazirla(gidenFatura, doc, xmlnscac, xmlnscbc, root);
            else
                EArsivTuzelKisiyeAitKismiHazirla(gidenFatura, doc, xmlnscac, xmlnscbc, root);

            EArsivBankaBilgileriDuzenle(gidenFatura, doc, xmlnscac, xmlnscbc, root);

            EArsivVergiVeDigerAlanlarDuzenle(gidenFatura, gidenFaturaDetayListesi, doc, xmlnscac, xmlnscbc,
                kodFaturaGrupTuruKod, kodSatisTuruKod, root);

            gidenFaturaDetayListesi.Aggregate(1, (current, item) => EArsivFaturaDetaylariDuzenle(gidenFatura, item, doc, xmlnscac, xmlnscbc, current, root));
            
            doc.AppendChild(root);

            return XMLDosyasiniKaydet(gidenFatura.Id, aktarilacakKlasorAdi, doc);
        }
        
        /// <summary>
        /// Müstahsil Makbuzu Bilgisi Üzerinden E-Müstahsil Sistemine Göndermek İçin
        /// XML Dosyası Oluşturmaya yarayan metottur
        /// </summary>
        /// <param name="mustahsilMakbuzu">Müstahsil Makbuzu Bilgisi</param>
        /// <param name="mustahsilMakbuzuDetayListesi">Müstahsil Makbuzu Detay Bilgisi</param>
        /// <param name="aktarilacakKlasorAdi">Oluşturulan Dosyanın Aktarılacağı Klasör</param>
        public static string EMustahsilXMLOlustur(MustahsilMakbuzuDTO mustahsilMakbuzu, List<MustahsilMakbuzuDetayDTO> mustahsilMakbuzuDetayListesi, string aktarilacakKlasorAdi)
        {
            var doc = new XmlDocument();
            var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            var root = doc.DocumentElement;
            doc.InsertBefore(declaration, root);
            doc.AppendChild(
                doc.CreateProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"general.xslt\""));

            root = doc.CreateElement("CreditNote");

            mustahsilMakbuzu = mustahsilMakbuzu.BosluklariKaldir();

            var kodMakbuzGrupTuruKod = mustahsilMakbuzu.MakbuzGrupTuruKod ?? 0;
            
            root.RemoveAllAttributes();
            
            // Makbuzun türü, kesilme tarihi, UBL Versiyon Numarası, GİB Numarası gibi bilgiler
            // yer almaktadır
            EMustahsilHeaderBilgisiDuzenle(mustahsilMakbuzu, doc, root, out var xmlnscac, out var xmlnscbc);

            IndicatorVeUuidBilgisiDuzenle(mustahsilMakbuzu.Id, root, doc, xmlnscbc);

            TarihSaatDuzenle(mustahsilMakbuzu.IslemTarihi, root, doc, xmlnscbc);

            EMustahsilKrediTuruDuzenle(doc, xmlnscbc, root);

            GonderimSekliBilgisiDuzenle(root, doc, xmlnscbc);

            ParaBirimiVeKayitSayisiDuzenle(mustahsilMakbuzuDetayListesi.Count, root, doc, xmlnscbc);

            EMustahsilEkDokumanReferansDuzenle(mustahsilMakbuzu, doc, xmlnscac, xmlnscbc, root);

            FaturaMakbuzImzaBilgisiDuzenle(root, doc, xmlnscac, xmlnscbc);

            EMustahsilMakbuzKesenFirmaBilgisiDuzenle(mustahsilMakbuzu, doc, xmlnscac, xmlnscbc, root);

            EMustahsilMakbuzKesilenFirmaBilgisiDuzenle(mustahsilMakbuzu, doc, xmlnscac, xmlnscbc, root);

            EMustahsilMakbuzKesilenKisiBilgisiDuzenle(mustahsilMakbuzu, doc, xmlnscac, xmlnscbc, root);

            EMustahsilMakbuzVergiBilgisiDuzenle(mustahsilMakbuzu, doc, xmlnscac, xmlnscbc, kodMakbuzGrupTuruKod, root);

            mustahsilMakbuzuDetayListesi.Aggregate(1, (current, item) => EMustahsilMakbuzDetayBilgileriDuzenle(item, doc, xmlnscac, xmlnscbc, current, root));

            doc.AppendChild(root);

            return XMLDosyasiniKaydet(mustahsilMakbuzu.Id, aktarilacakKlasorAdi, doc);
        }
        
        /// <summary>
        /// E-Müstahsil Makbuzu XML dosyası hazırlanırken
        /// Temel parametrelerin olduğu
        /// Header bilgilerinin düzenlenmesi için hazırlanmış olan metottur.
        /// </summary>
        /// <param name="mustahsilMakbuzu">Müstahsil Makbuzu Bilgisi</param>
        /// <param name="doc">XML Doküman Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        private static void EMustahsilHeaderBilgisiDuzenle(MustahsilMakbuzuDTO mustahsilMakbuzu, XmlDocument doc, XmlElement root, out XmlAttribute xmlnscac, out XmlAttribute xmlnscbc)
        {
            // var locationAttribute = "xsi:schemaLocation";
            var location = doc.CreateAttribute("xsi", "schemaLocation",
                "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2");
            location.Value =
                "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2 ../xsdrt/maindoc/UBL-CreditNote-2.1.xsd";
            var xmlns = doc.CreateAttribute("xmlns");
            xmlns.Value = "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2";
            var xmlnsn4 = doc.CreateAttribute("xmlns:n4");
            xmlnsn4.Value = "http://www.altova.com/samplexml/other-namespace";
            var xmlnsxsi = doc.CreateAttribute("xmlns:xsi");
            xmlnsxsi.Value = "http://www.w3.org/2001/XMLSchema-instance";
            xmlnscac = doc.CreateAttribute("xmlns:cac");
            xmlnscac.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            xmlnscbc = doc.CreateAttribute("xmlns:cbc");
            xmlnscbc.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            var xmlnsext = doc.CreateAttribute("xmlns:ext");
            xmlnsext.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
            root.Attributes.Append(location);
            root.Attributes.Append(xmlns);
            root.Attributes.Append(xmlnsn4);
            root.Attributes.Append(xmlnsxsi);
            root.Attributes.Append(xmlnscac);
            root.Attributes.Append(xmlnscbc);
            root.Attributes.Append(xmlnsext);
            var extUbl = doc.CreateElement("ext", "UBLExtensions", xmlnsext.Value);
            var extUbl2 = doc.CreateElement("ext", "UBLExtension", xmlnsext.Value);
            var extUbl3 = doc.CreateElement("ext", "ExtensionContent", xmlnsext.Value);
            var extUbl4 = doc.CreateElement("n4", "auto-generated_for_wildcard", xmlnsn4.Value);
            extUbl3.AppendChild(extUbl4);
            extUbl2.AppendChild(extUbl3);
            extUbl.AppendChild(extUbl2);

            root.AppendChild(extUbl);

            var versionId = doc.CreateElement("cbc", "UBLVersionID", xmlnscbc.Value);
            versionId.InnerText = "2.1";
            root.AppendChild(versionId);

            var customizationId = doc.CreateElement("cbc", "CustomizationID", xmlnscbc.Value);
            customizationId.InnerText = "TR1.2.1";
            root.AppendChild(customizationId);

            var profileId = doc.CreateElement("cbc", "ProfileID", xmlnscbc.Value);
            profileId.InnerText = "EARSIVBELGE";
            root.AppendChild(profileId);

            var id = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(mustahsilMakbuzu.MustahsilMakbuzuNo))
                id.InnerText = mustahsilMakbuzu.MustahsilMakbuzuNo;
            else
                id.InnerText = "ABC" + mustahsilMakbuzu.IslemTarihi?.Year +
                               DateTime.UtcNow.Ticks.ToString().Substring(0, 9);
            root.AppendChild(id);
        }
        
        /// <summary>
        /// E-Müstahsil Makbuzu XML dosyası hazırlanırken
        /// Kredi Türü bilgilerinin düzenlendiği metottur.
        /// </summary>
        /// <param name="doc">XML Doküman Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        private static void EMustahsilKrediTuruDuzenle(XmlDocument doc, XmlAttribute xmlnscbc, XmlElement root)
        {
            var creditNoteTypeCode = doc.CreateElement("cbc", "CreditNoteTypeCode", xmlnscbc.Value);
            creditNoteTypeCode.InnerText = "MUSTAHSILMAKBUZ";
            root.AppendChild(creditNoteTypeCode);
        }
        
        /// <summary>
        /// E-Müstahsil Makbuzu XML dosyası hazırlanırken
        /// Varsa bir şablonun çekilerek
        /// Ek Doküman Referans bilgisinin düzenlenmesi için
        /// Hazırlanmış olan metottur
        /// </summary>
        /// <param name="mustahsilMakbuzu">Müstahsil Makbuzu Bilgisi</param>
        /// <param name="doc">XML Doküman Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        private static void EMustahsilEkDokumanReferansDuzenle(MustahsilMakbuzuDTO mustahsilMakbuzu, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc, XmlElement root)
                {
                    #region AdditionalDocumentReference
        
                    var additionalDocumentReference = doc.CreateElement("cac", "AdditionalDocumentReference", xmlnscac.Value);
                    var additionalDocumentReferenceId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                    additionalDocumentReferenceId.InnerText = mustahsilMakbuzu.Id.Length == 36
                        ? mustahsilMakbuzu.Id.ToUpper()
                        : Guid.NewGuid().ToString().ToUpper();
                    additionalDocumentReference.AppendChild(additionalDocumentReferenceId);
                    var issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
                    issueDate.InnerText = mustahsilMakbuzu.IslemTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
                    additionalDocumentReference.AppendChild(issueDate);
                    var documentType = doc.CreateElement("cbc", "DocumentType", xmlnscbc.Value);
                    documentType.InnerText = "XSLT";
                    additionalDocumentReference.AppendChild(documentType);
                    var attachment = doc.CreateElement("cac", "Attachment", xmlnscac.Value);
                    var embeddedDocumentBinaryObject = doc.CreateElement("cbc", "EmbeddedDocumentBinaryObject", xmlnscbc.Value);
                    var characterSetCode = doc.CreateAttribute("characterSetCode");
                    characterSetCode.Value = "UTF-8";
                    var encodingCode = doc.CreateAttribute("encodingCode");
                    encodingCode.Value = "Base64";
                    var mimeCode = doc.CreateAttribute("mimeCode");
                    mimeCode.Value = "application/xml";
                    embeddedDocumentBinaryObject.Attributes.Append(characterSetCode);
                    embeddedDocumentBinaryObject.Attributes.Append(encodingCode);
                    embeddedDocumentBinaryObject.Attributes.Append(mimeCode);
                    attachment.AppendChild(embeddedDocumentBinaryObject);
                    additionalDocumentReference.AppendChild(attachment);
        
                    root.AppendChild(additionalDocumentReference);
        
                    #endregion
                }
        
        /// <summary>
        /// E-Müstahsil Makbuzu XML dosyası hazırlanırken
        /// Makbuz Kesen Firmanın Bilgilerinin
        /// Düzenlendiği metottur.
        /// </summary>
        /// <param name="mustahsilMakbuzu">Müstahsil Makbuzu Bilgisi</param>
        /// <param name="doc">XML Doküman Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        private static void EMustahsilMakbuzKesenFirmaBilgisiDuzenle(MustahsilMakbuzuDTO mustahsilMakbuzu, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc, XmlElement root)
        {
            // Burada sabit değerler verilmekte olup, Makbuzu kesen kuruma veya firmaya ait bilgiler
            // yer almaktadır. Bu kısımda örnek olarak TÜRKŞEKER Fabrikaları Genel Müdürlüğü bilgileri
            // kullanılmıştır. Ancak değiştirip test edilebilir.

            #region AccountingSupplierParty

            var accountingSupplierParty = doc.CreateElement("cac", "AccountingSupplierParty", xmlnscac.Value);
            var party = doc.CreateElement("cac", "Party", xmlnscac.Value);
            var webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
            webSiteUri.InnerText = "https://www.turkseker.gov.tr";
            party.AppendChild(webSiteUri);
            var accountingSupplierPartyIdAttr = doc.CreateAttribute("schemeID");
            accountingSupplierPartyIdAttr.Value = "VKN_TCKN";
            var partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
            var accountingSupplierPartyIdAttr2 = doc.CreateAttribute("schemeID");
            accountingSupplierPartyIdAttr2.Value = "VKN";
            var accountingSupplierPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            accountingSupplierPartyPartyId.Attributes.Append(accountingSupplierPartyIdAttr2);
            accountingSupplierPartyPartyId.InnerText = "3250566851";
            partyIdentification.AppendChild(accountingSupplierPartyPartyId);
            party.AppendChild(partyIdentification);

            if (!string.IsNullOrEmpty(mustahsilMakbuzu.AltBirimAd))
            {
                partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
                accountingSupplierPartyIdAttr2 = doc.CreateAttribute("schemeID");
                accountingSupplierPartyIdAttr2.Value = "SUBENO";
                accountingSupplierPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                accountingSupplierPartyPartyId.Attributes.Append(accountingSupplierPartyIdAttr2);
                accountingSupplierPartyPartyId.InnerText = mustahsilMakbuzu.AltBirimAd;
                partyIdentification.AppendChild(accountingSupplierPartyPartyId);
                party.AppendChild(partyIdentification);
            }

            var partyName = doc.CreateElement("cac", "PartyName", xmlnscac.Value);
            var partyNameReal = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            partyNameReal.InnerText = "TÜRKİYE ŞEKER FABRİKALARI A.Ş.";
            partyName.AppendChild(partyNameReal);
            party.AppendChild(partyName);

            #region Postal Address

            var postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
            var postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            postalAddressId.InnerText = "3250566851";
            postalAddress.AppendChild(postalAddressId);
            var streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
            streetName.InnerText = "Mithatpaşa Caddesi";
            postalAddress.AppendChild(streetName);
            var buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
            buildingNumber.InnerText = "14";
            postalAddress.AppendChild(buildingNumber);
            var citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
            citySubdivisionName.InnerText = "Çankaya";
            postalAddress.AppendChild(citySubdivisionName);
            var cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
            cityName.InnerText = "Ankara";
            postalAddress.AppendChild(cityName);
            var postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
            postalZone.InnerText = "06100";
            postalAddress.AppendChild(postalZone);
            var country = doc.CreateElement("cac", "Country", xmlnscac.Value);
            var countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            countryName.InnerText = "Türkiye";
            country.AppendChild(countryName);
            postalAddress.AppendChild(country);
            party.AppendChild(postalAddress);

            #endregion

            var partyTaxScheme = doc.CreateElement("cac", "PartyTaxScheme", xmlnscac.Value);
            var taxScheme = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
            var taxSchemeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            taxSchemeName.InnerText = "Ankara Kurumlar";
            taxScheme.AppendChild(taxSchemeName);
            partyTaxScheme.AppendChild(taxScheme);
            party.AppendChild(partyTaxScheme);

            var contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
            var telephone = doc.CreateElement("cbc", "Telephone", xmlnscbc.Value);
            telephone.InnerText = "(312) 4585500";
            contact.AppendChild(telephone);
            var telefax = doc.CreateElement("cbc", "Telefax", xmlnscbc.Value);
            telefax.InnerText = "(312) 4585800";
            contact.AppendChild(telefax);
            var electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
            electronicMail.InnerText = "maliisler@turkseker.gov.tr";
            contact.AppendChild(electronicMail);
            party.AppendChild(contact);

            accountingSupplierParty.AppendChild(party);

            root.AppendChild(accountingSupplierParty);

            #endregion
        }
        
        /// <summary>
        /// E-Müstahsil Makbuzu XML dosyası hazırlanırken
        /// Makbuz Kesilen Firmanın Bilgilerinin
        /// Düzenlendiği metottur.
        /// </summary>
        /// <param name="mustahsilMakbuzu">Müstahsil Makbuzu Bilgisi</param>
        /// <param name="doc">XML Doküman Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        private static void EMustahsilMakbuzKesilenFirmaBilgisiDuzenle(MustahsilMakbuzuDTO mustahsilMakbuzu, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc, XmlElement root)
        {
            // Bu kısımda makbuz kesilen firma bilgileri yer almaktadır

            #region AccountingCustomerParty

            var accountingCustomerParty = doc.CreateElement("cac", "AccountingCustomerParty", xmlnscac.Value);
            var party = doc.CreateElement("cac", "Party", xmlnscac.Value);
            var webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
            party.AppendChild(webSiteUri);
            var accountingCustomerPartyIdAttr = doc.CreateAttribute("schemeID");
            accountingCustomerPartyIdAttr.Value = "TCKN";
            if (mustahsilMakbuzu.VergiNo.Length == 10)
                accountingCustomerPartyIdAttr.Value = "VKN";
            var partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
            var accountingCustomerPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            accountingCustomerPartyPartyId.Attributes.Append(accountingCustomerPartyIdAttr);
            accountingCustomerPartyPartyId.InnerText = mustahsilMakbuzu.VergiNo;
            partyIdentification.AppendChild(accountingCustomerPartyPartyId);
            party.AppendChild(partyIdentification);

            if (mustahsilMakbuzu.VergiNo.Length == 10)
            {
                var partyName = doc.CreateElement("cac", "PartyName", xmlnscac.Value);
                var partyNameReal = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                partyNameReal.InnerText = mustahsilMakbuzu.TuzelKisiAd;
                partyName.AppendChild(partyNameReal);
                party.AppendChild(partyName);
            }

            #region Postal Address

            var postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
            var postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            postalAddressId.InnerText = mustahsilMakbuzu.VergiNo;
            postalAddress.AppendChild(postalAddressId);
            var room = doc.CreateElement("cbc", "Room", xmlnscbc.Value);
            postalAddress.AppendChild(room);
            var streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(mustahsilMakbuzu.Adres))
                streetName.InnerText = mustahsilMakbuzu.Adres;
            postalAddress.AppendChild(streetName);
            var buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
            //buildingNumber.InnerText = "";
            postalAddress.AppendChild(buildingNumber);
            var citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(mustahsilMakbuzu.IlceAd))
                citySubdivisionName.InnerText = mustahsilMakbuzu.IlceAd;
            postalAddress.AppendChild(citySubdivisionName);
            var cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(mustahsilMakbuzu.IlAd))
                cityName.InnerText = mustahsilMakbuzu.IlAd;
            postalAddress.AppendChild(cityName);
            var postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
            //postalZone.InnerText = "";
            postalAddress.AppendChild(postalZone);
            var country = doc.CreateElement("cac", "Country", xmlnscac.Value);
            var countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            countryName.InnerText = "Türkiye";
            country.AppendChild(countryName);
            postalAddress.AppendChild(country);
            party.AppendChild(postalAddress);

            #endregion

            var partyTaxScheme = doc.CreateElement("cac", "PartyTaxScheme", xmlnscac.Value);
            var taxScheme = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
            var taxSchemeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(mustahsilMakbuzu.VergiDairesi))
                taxSchemeName.InnerText = mustahsilMakbuzu.VergiDairesi;
            taxScheme.AppendChild(taxSchemeName);
            partyTaxScheme.AppendChild(taxScheme);
            party.AppendChild(partyTaxScheme);

            var contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
            var electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(mustahsilMakbuzu.EPostaAdresi))
                electronicMail.InnerText = mustahsilMakbuzu.EPostaAdresi;
            contact.AppendChild(electronicMail);
            party.AppendChild(contact);

            if (mustahsilMakbuzu.VergiNo.Length == 11)
            {
                var liste = mustahsilMakbuzu.TuzelKisiAd.Split(' ').ToList();
                if (liste.Count >= 1)
                {
                    var person = doc.CreateElement("cac", "Person", xmlnscac.Value);
                    var firstName = doc.CreateElement("cbc", "FirstName", xmlnscbc.Value);
                    firstName.InnerText = liste[0];
                    person.AppendChild(firstName);
                    var surname = liste[0];
                    if (liste.Count > 1)
                        surname = mustahsilMakbuzu.TuzelKisiAd.Substring(liste[0].Length + 1);
                    var familyName = doc.CreateElement("cbc", "FamilyName", xmlnscbc.Value);
                    familyName.InnerText = surname;
                    //familyName.InnerText = liste[1];
                    person.AppendChild(familyName);
                    party.AppendChild(person);
                }
            }

            accountingCustomerParty.AppendChild(party);

            root.AppendChild(accountingCustomerParty);

            #endregion
        }
        
        /// <summary>
        /// E-Müstahsil Makbuzu XML dosyası hazırlanırken
        /// Makbuz Kesilen Kişi veya Firmanın Bilgilerinin
        /// Düzenlendiği metottur.
        /// </summary>
        /// <param name="mustahsilMakbuzu">Müstahsil Makbuzu Bilgisi</param>
        /// <param name="doc">XML Doküman Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        private static void EMustahsilMakbuzKesilenKisiBilgisiDuzenle(MustahsilMakbuzuDTO mustahsilMakbuzu, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc, XmlElement root)
        {
            if (string.IsNullOrEmpty(mustahsilMakbuzu.VergiNo)) return;
            if (mustahsilMakbuzu.VergiNo.Length != 10) return;
            var ortakVeriler = new OrtakVerilerDTO
            {
                Adres = mustahsilMakbuzu.Adres,
                IlAd = mustahsilMakbuzu.IlAd,
                IlceAd = mustahsilMakbuzu.IlceAd,
                VergiDairesi = mustahsilMakbuzu.VergiDairesi,
                VergiNo = mustahsilMakbuzu.VergiNo,
                EPostaAdresi = mustahsilMakbuzu.EPostaAdresi,
                TuzelKisiAd = mustahsilMakbuzu.TuzelKisiAd
            };
            FaturaMakbuzKesilenKisiBilgisiDuzenle(ortakVeriler, doc, xmlnscac, xmlnscbc, root);
        }
        
        /// <summary>
        /// E-Müstahsil Makbuzu XML dosyası hazırlanırken
        /// Ana Vergi Bilgilerinin Düzenlendiği Metottur
        /// </summary>
        /// <param name="mustahsilMakbuzu">Müstahsil Makbuzu Bilgisi</param>
        /// <param name="doc">XML Doküman Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="kodMakbuzGrupTuruKod">Makbuz Grup Türü Kodu</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        private static void EMustahsilMakbuzVergiBilgisiDuzenle(MustahsilMakbuzuDTO mustahsilMakbuzu, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc, int kodMakbuzGrupTuruKod, XmlElement root)
        {
            // Burada vergi bilgileri yer almaktadır

            #region TaxTotal

            var taxTotal = doc.CreateElement("cac", "TaxTotal", xmlnscac.Value);
            var currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            var taxAmount = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
            taxAmount.RemoveAllAttributes();
            taxAmount.Attributes.Append(currencyId);
            taxAmount.InnerText = Decimal
                .Round((decimal) mustahsilMakbuzu.GelirVergisi, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            taxTotal.AppendChild(taxAmount);
            var taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
            var taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
            taxableAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            taxableAmount.Attributes.Append(currencyId);
            taxableAmount.InnerText = Decimal
                .Round((decimal) mustahsilMakbuzu.NetTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            taxSubTotal.AppendChild(taxableAmount);
            var taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
            taxAmount2.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            taxAmount2.Attributes.Append(currencyId);
            taxAmount2.InnerText = Decimal
                .Round((decimal) mustahsilMakbuzu.GelirVergisi, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            taxSubTotal.AppendChild(taxAmount2);
            var percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
            percent.InnerText = mustahsilMakbuzu.NetTutar != 0
                ? Decimal.Round(((decimal) mustahsilMakbuzu.GelirVergisi * 100) / (decimal) mustahsilMakbuzu.NetTutar,
                    0, MidpointRounding.AwayFromZero).ToString()
                : "0";
            taxSubTotal.AppendChild(percent);
            var taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
            if (mustahsilMakbuzu.GelirVergisi == 0)
            {
                if (kodMakbuzGrupTuruKod == FaturaMakbuzGrupTur.Kuspe.GetHashCode())
                {
                    var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                    taxExemptionReasonCode.InnerText = "325";
                    var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                    taxExemptionReason.InnerText = "13/ı Yem Teslimleri";
                    taxCategory.AppendChild(taxExemptionReasonCode);
                    taxCategory.AppendChild(taxExemptionReason);
                }
                else
                {
                    var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                    taxExemptionReasonCode.InnerText = "350";
                    var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                    taxExemptionReason.InnerText = "Diğerleri";
                    taxCategory.AppendChild(taxExemptionReasonCode);
                    taxCategory.AppendChild(taxExemptionReason);
                }
            }

            var taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
            var taxName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            taxName.InnerText = "GV STOPAJI";
            taxScheme2.AppendChild(taxName);
            var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
            taxTypeCode.InnerText = "0003";
            taxScheme2.AppendChild(taxTypeCode);
            taxCategory.AppendChild(taxScheme2);
            taxSubTotal.AppendChild(taxCategory);
            taxTotal.AppendChild(taxSubTotal);
            root.AppendChild(taxTotal);

            #endregion

            // Gelir Vergisi, Net Tutar ve Toplam Tutar bilgileri yer almaktadır

            #region LegalMonetaryTotal

            var legalMonetaryTotal = doc.CreateElement("cac", "LegalMonetaryTotal", xmlnscac.Value);
            var lineExtensionAmount = doc.CreateElement("cbc", "LineExtensionAmount", xmlnscbc.Value);
            lineExtensionAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            lineExtensionAmount.Attributes.Append(currencyId);
            lineExtensionAmount.InnerText = Decimal
                .Round((decimal) mustahsilMakbuzu.NetTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            legalMonetaryTotal.AppendChild(lineExtensionAmount);
            var taxExclusiveAmount = doc.CreateElement("cbc", "TaxExclusiveAmount", xmlnscbc.Value);
            taxExclusiveAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            taxExclusiveAmount.Attributes.Append(currencyId);
            taxExclusiveAmount.InnerText = Decimal
                .Round((decimal) mustahsilMakbuzu.NetTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            legalMonetaryTotal.AppendChild(taxExclusiveAmount);
            var taxInclusiveAmount = doc.CreateElement("cbc", "TaxInclusiveAmount", xmlnscbc.Value);
            taxInclusiveAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            taxInclusiveAmount.Attributes.Append(currencyId);
            taxInclusiveAmount.InnerText = Decimal
                .Round((decimal) mustahsilMakbuzu.NetTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            legalMonetaryTotal.AppendChild(taxInclusiveAmount);
            var payableAmount = doc.CreateElement("cbc", "PayableAmount", xmlnscbc.Value);
            payableAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            payableAmount.Attributes.Append(currencyId);
            payableAmount.InnerText = Decimal
                .Round((decimal) mustahsilMakbuzu.NetTutar - (decimal) mustahsilMakbuzu.GelirVergisi, 2,
                    MidpointRounding.AwayFromZero).ToString().Replace(",", ".");
            legalMonetaryTotal.AppendChild(payableAmount);
            root.AppendChild(legalMonetaryTotal);

            #endregion
        }
        
        /// <summary>
        /// E-Müstahsil Makbuzu XML dosyası hazırlanırken
        /// Makbuz Detay bilgileri üzerinden gerekli işlemlerin
        /// Gerçekleştirildiği metottur.
        /// </summary>
        /// <param name="item">Müstahsil Makbuzu Detay Bilgisi</param>
        /// <param name="doc">XML Doküman Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="sayac">Sayaç Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        private static int EMustahsilMakbuzDetayBilgileriDuzenle(MustahsilMakbuzuDetayDTO item, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc, int sayac, XmlElement root)
        {
            var detayBilgisi = item.BosluklariKaldir();

            #region CreditNoteLine

            #region MetaData

            var creditNoteLine = doc.CreateElement("cac", "CreditNoteLine", xmlnscac.Value);
            var creditNoteLineId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            creditNoteLineId.InnerText = sayac.ToString();
            creditNoteLine.AppendChild(creditNoteLineId);
            var quantity = doc.CreateAttribute("unitCode");
            quantity.Value = "KGM";
            var creditedQuantity = doc.CreateElement("cbc", "CreditedQuantity", xmlnscbc.Value);
            creditedQuantity.RemoveAllAttributes();
            creditedQuantity.Attributes.Append(quantity);
            creditedQuantity.InnerText = Decimal
                .Round((decimal) detayBilgisi.Miktar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            creditNoteLine.AppendChild(creditedQuantity);
            var lineExtensionAmountDetail = doc.CreateElement("cbc", "LineExtensionAmount", xmlnscbc.Value);
            lineExtensionAmountDetail.RemoveAllAttributes();
            var currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            lineExtensionAmountDetail.Attributes.Append(currencyId);
            lineExtensionAmountDetail.InnerText = Decimal
                .Round((decimal) detayBilgisi.NetTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            creditNoteLine.AppendChild(lineExtensionAmountDetail);

            #endregion

            #region TaxTotal

            var taxTotal = doc.CreateElement("cac", "TaxTotal", xmlnscac.Value);
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            var taxAmount = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
            taxAmount.RemoveAllAttributes();
            taxAmount.Attributes.Append(currencyId);
            taxAmount.InnerText = Decimal
                .Round((decimal) detayBilgisi.GelirVergisi, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            taxTotal.AppendChild(taxAmount);
            var taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
            var taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
            taxableAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            taxableAmount.Attributes.Append(currencyId);
            taxableAmount.InnerText = Decimal
                .Round((decimal) detayBilgisi.NetTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            taxSubTotal.AppendChild(taxableAmount);
            var taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
            taxAmount2.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            taxAmount2.Attributes.Append(currencyId);
            taxAmount2.InnerText = Decimal
                .Round((decimal) detayBilgisi.GelirVergisi, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            taxSubTotal.AppendChild(taxAmount2);
            var percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
            percent.InnerText = "2";
            taxSubTotal.AppendChild(percent);
            var taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
            var taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
            var taxTypeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            taxTypeName.InnerText = "GV STOPAJI";
            taxScheme2.AppendChild(taxTypeName);
            var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
            taxTypeCode.InnerText = "0003";
            taxScheme2.AppendChild(taxTypeCode);
            taxCategory.AppendChild(taxScheme2);
            taxSubTotal.AppendChild(taxCategory);
            taxTotal.AppendChild(taxSubTotal);
            creditNoteLine.AppendChild(taxTotal);

            #endregion

            #region Item

            var creditNoteItem = doc.CreateElement("cac", "Item", xmlnscac.Value);
            var itemName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            itemName.InnerText = detayBilgisi.IsinMahiyeti;
            creditNoteItem.AppendChild(itemName);
            creditNoteLine.AppendChild(creditNoteItem);

            #endregion

            #region Price

            var price = doc.CreateElement("cac", "Price", xmlnscac.Value);
            var priceAmount = doc.CreateElement("cbc", "PriceAmount", xmlnscbc.Value);
            priceAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            priceAmount.Attributes.Append(currencyId);
            priceAmount.InnerText = detayBilgisi.BirimFiyat != null
                ? Decimal.Round((decimal) detayBilgisi.BirimFiyat, 4, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".")
                : "0.00";
            price.AppendChild(priceAmount);
            creditNoteLine.AppendChild(price);

            #endregion

            root.AppendChild(creditNoteLine);

            #endregion

            sayac++;
            return sayac;
        }

        /// <summary>
        /// E-Fatura XML dosyası hazırlanırken
        /// XML'in ilk hali ve gerekli temel parametrelerin eklenmesi için
        /// Hazırlanmış olan metottur.
        /// </summary>
        /// <param name="doc">XML Doküman Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        private static void EFaturaHeaderBilgisiDuzenle(XmlDocument doc, XmlElement root, out XmlAttribute xmlnscac, out XmlAttribute xmlnscbc)
        {
            // var locationAttribute = "xsi:schemaLocation";
            var location = doc.CreateAttribute("xsi", "schemaLocation",
                "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
            location.Value =
                "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2 ../xsdrt/maindoc/UBL-Invoice-2.1.xsd";
            //var location = doc.CreateAttribute(schemaLocation);
            //location.Value = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2 ../xsdrt/maindoc/UBL-Invoice-2.1.xsd";
            var xmlns = doc.CreateAttribute("xmlns");
            xmlns.Value = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
            var xmlnsn4 = doc.CreateAttribute("xmlns:n4");
            xmlnsn4.Value = "http://www.altova.com/samplexml/other-namespace";
            var xmlnsxsi = doc.CreateAttribute("xmlns:xsi");
            xmlnsxsi.Value = "http://www.w3.org/2001/XMLSchema-instance";
            xmlnscac = doc.CreateAttribute("xmlns:cac");
            xmlnscac.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            xmlnscbc = doc.CreateAttribute("xmlns:cbc");
            xmlnscbc.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            var xmlnsext = doc.CreateAttribute("xmlns:ext");
            xmlnsext.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
            root.Attributes.Append(location);
            root.Attributes.Append(xmlns);
            root.Attributes.Append(xmlnsn4);
            root.Attributes.Append(xmlnsxsi);
            root.Attributes.Append(xmlnscac);
            root.Attributes.Append(xmlnscbc);
            root.Attributes.Append(xmlnsext);
            var extUbl = doc.CreateElement("ext", "UBLExtensions", xmlnsext.Value);
            var extUbl2 = doc.CreateElement("ext", "UBLExtension", xmlnsext.Value);
            var extUbl3 = doc.CreateElement("ext", "ExtensionContent", xmlnsext.Value);
            var extUbl4 = doc.CreateElement("n4", "auto-generated_for_wildcard", xmlnsn4.Value);
            extUbl3.AppendChild(extUbl4);
            extUbl2.AppendChild(extUbl3);
            extUbl.AppendChild(extUbl2);

            root.AppendChild(extUbl);
        }
        
        /// <summary>
        /// E-Fatura XML dosyası hazırlanırken
        /// Varsa bir şablonun çekilerek
        /// Ek Doküman Referans bilgisinin düzenlenmesi için
        /// Hazırlanmış olan metottur
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="doc">XML Doküman Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        private static void EFaturaEkDokumanReferansDuzenle(GidenFaturaDTO gidenFatura, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc, XmlElement root)
        {
            #region AdditionalDocumentReference

            var additionalDocumentReference = doc.CreateElement("cac", "AdditionalDocumentReference", xmlnscac.Value);
            var additionalDocumentReferenceId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            additionalDocumentReferenceId.InnerText = gidenFatura.Id;
            additionalDocumentReference.AppendChild(additionalDocumentReferenceId);
            var issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
            issueDate.InnerText = gidenFatura.IslemTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
            additionalDocumentReference.AppendChild(issueDate);
            var documentType = doc.CreateElement("cbc", "DocumentType", xmlnscbc.Value);
            documentType.InnerText = "XSLT";
            additionalDocumentReference.AppendChild(documentType);
            issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
            issueDate.InnerText = gidenFatura.IslemTarihi?.Date.ToString("yyyy-MM-dd");
            additionalDocumentReference.AppendChild(issueDate);
            documentType = doc.CreateElement("cbc", "DocumentType", xmlnscbc.Value);
            documentType.InnerText = "XSLT";
            additionalDocumentReference.AppendChild(documentType);
            var attachment = doc.CreateElement("cac", "Attachment", xmlnscac.Value);
            var embeddedDocumentBinaryObject = doc.CreateElement("cbc", "EmbeddedDocumentBinaryObject", xmlnscbc.Value);
            var characterSetCode = doc.CreateAttribute("characterSetCode");
            characterSetCode.Value = "UTF-8";
            var encodingCode = doc.CreateAttribute("encodingCode");
            encodingCode.Value = "Base64";
            // XmlAttribute fileName2 = doc.CreateAttribute("fileName");
            // fileName2.Value = "efatura.xslt";
            var mimeCode = doc.CreateAttribute("mimeCode");
            mimeCode.Value = "application/xml";
            embeddedDocumentBinaryObject.Attributes.Append(characterSetCode);
            embeddedDocumentBinaryObject.Attributes.Append(encodingCode);
            // embeddedDocumentBinaryObject.Attributes.Append(fileName2);
            embeddedDocumentBinaryObject.Attributes.Append(mimeCode);

            #region Base 64 Metin

            var sablonAdresi = Directory.GetFiles("../../Other");
            if (sablonAdresi.Length > 0)
            {
                foreach (var item in sablonAdresi)
                {
                    if (!item.Contains("efatura")) continue;
                    var base64Metin = File.ReadAllText(item);
                    embeddedDocumentBinaryObject.InnerText = base64Metin;
                    break;
                }
            }

            #endregion

            attachment.AppendChild(embeddedDocumentBinaryObject);
            additionalDocumentReference.AppendChild(attachment);

            root.AppendChild(additionalDocumentReference);

            additionalDocumentReference = doc.CreateElement("cac", "AdditionalDocumentReference", xmlnscac.Value);
            additionalDocumentReferenceId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            additionalDocumentReferenceId.InnerText = gidenFatura.Id;
            additionalDocumentReference.AppendChild(additionalDocumentReferenceId);
            issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
            issueDate.InnerText = gidenFatura.IslemTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
            additionalDocumentReference.AppendChild(issueDate);
            documentType = doc.CreateElement("cbc", "DocumentType", xmlnscbc.Value);
            documentType.InnerText = "XSLT";
            additionalDocumentReference.AppendChild(documentType);
            attachment = doc.CreateElement("cac", "Attachment", xmlnscac.Value);
            embeddedDocumentBinaryObject = doc.CreateElement("cbc", "EmbeddedDocumentBinaryObject", xmlnscbc.Value);
            characterSetCode = doc.CreateAttribute("characterSetCode");
            characterSetCode.Value = "UTF-8";
            encodingCode = doc.CreateAttribute("encodingCode");
            encodingCode.Value = "Base64";
            mimeCode = doc.CreateAttribute("mimeCode");
            mimeCode.Value = "application/xml";
            embeddedDocumentBinaryObject.Attributes.Append(characterSetCode);
            embeddedDocumentBinaryObject.Attributes.Append(encodingCode);
            embeddedDocumentBinaryObject.Attributes.Append(mimeCode);
            attachment.AppendChild(embeddedDocumentBinaryObject);
            additionalDocumentReference.AppendChild(attachment);

            #endregion
        }
        
        /// <summary>
        /// E-Fatura XML dosyası hazırlanırken
        /// Fatura Kesilen Firmanın Bilgilerinin Girildiği
        /// Ve XML dosyası içerisinde ele alındığı metottur.
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="doc">XML Doküman Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        private static void EFaturaKesilenFirmaBilgileriDuzenle(GidenFaturaDTO gidenFatura, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc, XmlElement root)
        {
            // Bu kısımda fatura kesilen firma bilgileri yer almaktadır

            #region AccountingCustomerParty

            var accountingCustomerParty = doc.CreateElement("cac", "AccountingCustomerParty", xmlnscac.Value);
            var party = doc.CreateElement("cac", "Party", xmlnscac.Value);
            var webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
            party.AppendChild(webSiteUri);
            var accountingCustomerPartyIdAttr = doc.CreateAttribute("schemeID");
            accountingCustomerPartyIdAttr.Value = "TCKN";
            if (!string.IsNullOrEmpty(gidenFatura.VergiNo) && gidenFatura.VergiNo.Length == 10)
                accountingCustomerPartyIdAttr.Value = "VKN";
            var partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
            var accountingCustomerPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            accountingCustomerPartyPartyId.Attributes.Append(accountingCustomerPartyIdAttr);
            accountingCustomerPartyPartyId.InnerText = gidenFatura.VergiNo;
            partyIdentification.AppendChild(accountingCustomerPartyPartyId);
            party.AppendChild(partyIdentification);

            if (!string.IsNullOrEmpty(gidenFatura.VergiNo) && gidenFatura.VergiNo.Length == 10)
            {
                var partyName = doc.CreateElement("cac", "PartyName", xmlnscac.Value);
                var partyNameReal = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                partyNameReal.InnerText = gidenFatura.TuzelKisiAd;
                partyName.AppendChild(partyNameReal);
                party.AppendChild(partyName);
            }

            #region Postal Address

            var postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
            var postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            postalAddressId.InnerText = gidenFatura.VergiNo;
            postalAddress.AppendChild(postalAddressId);
            var room = doc.CreateElement("cbc", "Room", xmlnscbc.Value);
            postalAddress.AppendChild(room);
            var streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.Adres))
                streetName.InnerText = gidenFatura.Adres;
            postalAddress.AppendChild(streetName);
            var buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
            //buildingNumber.InnerText = "";
            postalAddress.AppendChild(buildingNumber);
            var citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.IlceAd))
                citySubdivisionName.InnerText = gidenFatura.IlceAd;
            postalAddress.AppendChild(citySubdivisionName);
            var cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.IlAd))
                cityName.InnerText = gidenFatura.IlAd;
            postalAddress.AppendChild(cityName);
            var postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
            //postalZone.InnerText = "";
            postalAddress.AppendChild(postalZone);
            var country = doc.CreateElement("cac", "Country", xmlnscac.Value);
            var countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            countryName.InnerText = "Türkiye";
            country.AppendChild(countryName);
            postalAddress.AppendChild(country);
            party.AppendChild(postalAddress);

            #endregion

            var partyTaxScheme = doc.CreateElement("cac", "PartyTaxScheme", xmlnscac.Value);
            var taxScheme = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
            var taxSchemeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.VergiDairesi))
                taxSchemeName.InnerText = gidenFatura.VergiDairesi;
            taxScheme.AppendChild(taxSchemeName);
            partyTaxScheme.AppendChild(taxScheme);
            party.AppendChild(partyTaxScheme);

            var contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
            var electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.EPostaAdresi))
                electronicMail.InnerText = gidenFatura.EPostaAdresi;
            contact.AppendChild(electronicMail);
            party.AppendChild(contact);

            if (!string.IsNullOrEmpty(gidenFatura.VergiNo))
            {
                if (gidenFatura.VergiNo.Length == 11)
                {
                    var liste = gidenFatura.TuzelKisiAd.Split(' ').ToList();
                    if (liste.Count >= 1)
                    {
                        var person = doc.CreateElement("cac", "Person", xmlnscac.Value);
                        var firstName = doc.CreateElement("cbc", "FirstName", xmlnscbc.Value);
                        firstName.InnerText = liste[0];
                        person.AppendChild(firstName);
                        var surname = liste[0];
                        if (liste.Count > 1)
                            surname = gidenFatura.TuzelKisiAd.Substring(liste[0].Length + 1);
                        var familyName = doc.CreateElement("cbc", "FamilyName", xmlnscbc.Value);
                        familyName.InnerText = surname;
                        //familyName.InnerText = liste[1];
                        person.AppendChild(familyName);
                        party.AppendChild(person);
                    }
                }
            }

            accountingCustomerParty.AppendChild(party);

            root.AppendChild(accountingCustomerParty);

            #endregion
        }
        
        /// <summary>
        /// E-Fatura XML dosyası hazırlanırken
        /// Fatura kesilen gerçek veya tüzel kişinin bilgilerinin düzenlenmesi için
        /// Hazırlanmış olan metottur.
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="doc">XML Doküman Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        private static void FaturaKesilenKisiBilgisiDuzenle(GidenFaturaDTO gidenFatura, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc, XmlElement root)
        {
            if (string.IsNullOrEmpty(gidenFatura.VergiNo)) return;
            if (gidenFatura.VergiNo.Length != 10) return;
            var ortakVeriler = new OrtakVerilerDTO
            {
                Adres = gidenFatura.Adres,
                IlAd = gidenFatura.IlAd,
                IlceAd = gidenFatura.IlceAd,
                VergiDairesi = gidenFatura.VergiDairesi,
                VergiNo = gidenFatura.VergiNo,
                EPostaAdresi = gidenFatura.EPostaAdresi,
                TuzelKisiAd = gidenFatura.TuzelKisiAd
            };
            FaturaMakbuzKesilenKisiBilgisiDuzenle(ortakVeriler, doc, xmlnscac, xmlnscbc, root);
        }
        
        /// <summary>
        /// E-Fatura Kısmında
        /// Fatura Detayları ile ilgil olarak
        /// Yapılan işlmeerin tek bir yerde tutulduğu metottur.
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="item">Fatura Detay Bilgisi</param>
        /// <param name="doc">XML Dokümanı Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="sayac">Sayaç Bilgisi</param>
        /// <param name="kodFaturaGrupTuruKod">Fatura Grup Türü Kodu</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        /// <returns>Sayaç Bilgisi</returns>
        private static int EFaturaDetayBilgileriniDuzenle(GidenFaturaDTO gidenFatura, GidenFaturaDetayDTO item, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc, int sayac, int kodFaturaGrupTuruKod, XmlElement root)
        {
            var detayBilgisi = item.BosluklariKaldir();

            #region InvoiceLine

            // Ölçü Birimi, KDV Hariç Tutar, Para Birimi gibi bilgiler yer almaktadır

            #region MetaData

            var invoiceLine = doc.CreateElement("cac", "InvoiceLine", xmlnscac.Value);
            var invoiceLineId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            invoiceLineId.InnerText = sayac.ToString();
            invoiceLine.AppendChild(invoiceLineId);
            var quantity = doc.CreateAttribute("unitCode");
            quantity.Value = !string.IsNullOrEmpty(detayBilgisi.GibKisaltma) ? detayBilgisi.GibKisaltma : "KGM";
            var invoicedQuantity = doc.CreateElement("cbc", "InvoicedQuantity", xmlnscbc.Value);
            invoicedQuantity.RemoveAllAttributes();
            invoicedQuantity.Attributes.Append(quantity);
            invoicedQuantity.InnerText = Decimal
                .Round((decimal) detayBilgisi.Miktar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            invoiceLine.AppendChild(invoicedQuantity);
            var lineExtensionAmountDetail = doc.CreateElement("cbc", "LineExtensionAmount", xmlnscbc.Value);
            lineExtensionAmountDetail.RemoveAllAttributes();
            var currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            lineExtensionAmountDetail.Attributes.Append(currencyId);
            lineExtensionAmountDetail.InnerText = Decimal
                .Round((decimal) detayBilgisi.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            invoiceLine.AppendChild(lineExtensionAmountDetail);

            #endregion

            // Varsa iskonto bilgileri yer almaktadır

            #region AllowanceCharge

            var allowanceCharge = doc.CreateElement("cac", "AllowanceCharge", xmlnscac.Value);
            var chargeIndicator = doc.CreateElement("cbc", "ChargeIndicator", xmlnscbc.Value);
            chargeIndicator.InnerText = "false";
            allowanceCharge.AppendChild(chargeIndicator);
            decimal amount2 = 0;
            var toplamTutar = (decimal) detayBilgisi.KdvHaricTutar;
            var kodIskontoTuruOran = detayBilgisi.IskontoOran ?? 0;
            if (kodIskontoTuruOran > 0)
            {
                toplamTutar = Decimal.Round((decimal) detayBilgisi.Miktar * (decimal) detayBilgisi.BirimFiyat, 4,
                    MidpointRounding.AwayFromZero);
                var toplamTutar2 = Decimal.Round(toplamTutar * (100 - kodIskontoTuruOran) * (decimal) 0.01, 4,
                    MidpointRounding.AwayFromZero);
                amount2 = toplamTutar - toplamTutar2;
            }

            var multiplierFactorNumeric = doc.CreateElement("cbc", "MultiplierFactorNumeric", xmlnscbc.Value);
            multiplierFactorNumeric.InnerText = Decimal
                .Round(kodIskontoTuruOran * (decimal) 0.01, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            allowanceCharge.AppendChild(multiplierFactorNumeric);
            var amount = doc.CreateElement("cbc", "Amount", xmlnscbc.Value);
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            amount.RemoveAllAttributes();
            amount.Attributes.Append(currencyId);
            amount.InnerText = Decimal.Round(amount2, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            allowanceCharge.AppendChild(amount);
            var baseAmount = doc.CreateElement("cbc", "BaseAmount", xmlnscbc.Value);
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            baseAmount.RemoveAllAttributes();
            baseAmount.Attributes.Append(currencyId);
            baseAmount.InnerText = Decimal.Round(toplamTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            allowanceCharge.AppendChild(baseAmount);
            invoiceLine.AppendChild(allowanceCharge);

            #endregion

            // Vergi bilgileri yer almaktadır

            #region TaxTotal

            var taxTotal = doc.CreateElement("cac", "TaxTotal", xmlnscac.Value);
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            var taxAmount = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
            taxAmount.RemoveAllAttributes();
            taxAmount.Attributes.Append(currencyId);
            taxAmount.InnerText = Decimal.Round((decimal) detayBilgisi.KdvTutari, 2, MidpointRounding.AwayFromZero)
                .ToString().Replace(",", ".");
            if (item.KonaklamaVergisi > 0 && gidenFatura.IslemTarihi >= Sabitler.KonaklamaVergiKontrolTarihi)
                taxAmount.InnerText = Decimal
                    .Round((decimal) detayBilgisi.KdvTutari + (decimal) detayBilgisi.KonaklamaVergisi, 2,
                        MidpointRounding.AwayFromZero)
                    .ToString().Replace(",", ".");
            taxTotal.AppendChild(taxAmount);
            var taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
            var taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
            taxableAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            taxableAmount.Attributes.Append(currencyId);
            taxableAmount.InnerText = Decimal
                .Round((decimal) detayBilgisi.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            taxSubTotal.AppendChild(taxableAmount);
            var taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
            taxAmount2.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            taxAmount2.Attributes.Append(currencyId);
            taxAmount2.InnerText = Decimal.Round((decimal) detayBilgisi.KdvTutari, 2, MidpointRounding.AwayFromZero)
                .ToString().Replace(",", ".");
            taxSubTotal.AppendChild(taxAmount2);
            var percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
            if (detayBilgisi.KdvOran != null)
                percent.InnerText = Decimal.Round((decimal) detayBilgisi.KdvOran, 2, MidpointRounding.AwayFromZero)
                    .ToString("N1").Replace(",", ".");
            taxSubTotal.AppendChild(percent);
            var taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
            if (detayBilgisi.KdvTutari == 0)
            {
                if (kodFaturaGrupTuruKod == FaturaMakbuzGrupTur.Kuspe.GetHashCode())
                {
                    var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                    taxExemptionReasonCode.InnerText = "325";
                    var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                    taxExemptionReason.InnerText = "13/ı Yem Teslimleri";
                    taxCategory.AppendChild(taxExemptionReasonCode);
                    taxCategory.AppendChild(taxExemptionReason);
                }
                else
                {
                    var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                    taxExemptionReasonCode.InnerText = "350";
                    var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                    taxExemptionReason.InnerText = "Diğerleri";
                    taxCategory.AppendChild(taxExemptionReasonCode);
                    taxCategory.AppendChild(taxExemptionReason);
                }
            }

            var taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
            var taxTypeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            taxTypeName.InnerText = "KDV";
            taxScheme2.AppendChild(taxTypeName);
            var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
            taxTypeCode.InnerText = "0015";
            taxScheme2.AppendChild(taxTypeCode);
            taxCategory.AppendChild(taxScheme2);
            taxSubTotal.AppendChild(taxCategory);
            taxTotal.AppendChild(taxSubTotal);
            if (item.KonaklamaVergisi > 0 && gidenFatura.IslemTarihi >= Sabitler.KonaklamaVergiKontrolTarihi)
            {
                #region TaxSubTotal (Konaklama Vergisi İçin)

                taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
                taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
                taxableAmount.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                taxableAmount.Attributes.Append(currencyId);
                taxableAmount.InnerText = Decimal
                    .Round((decimal) detayBilgisi.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                taxSubTotal.AppendChild(taxableAmount);
                taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                taxAmount2.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                taxAmount2.Attributes.Append(currencyId);
                taxAmount2.InnerText = Decimal
                    .Round((decimal) detayBilgisi.KonaklamaVergisi, 2, MidpointRounding.AwayFromZero)
                    .ToString().Replace(",", ".");
                taxSubTotal.AppendChild(taxAmount2);
                percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
                percent.InnerText = Decimal
                    .Round(2, 2, MidpointRounding.AwayFromZero)
                    .ToString("N1").Replace(",", ".");
                taxSubTotal.AppendChild(percent);
                taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
                taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                taxTypeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                taxTypeName.InnerText = "Konaklama Vergisi";
                taxScheme2.AppendChild(taxTypeName);
                taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
                taxTypeCode.InnerText = "0059";
                taxScheme2.AppendChild(taxTypeCode);
                taxCategory.AppendChild(taxScheme2);
                taxSubTotal.AppendChild(taxCategory);
                taxTotal.AppendChild(taxSubTotal);
                invoiceLine.AppendChild(taxTotal);

                #endregion
            }

            invoiceLine.AppendChild(taxTotal);

            #endregion

            // Fatura kesilen ürün detayı ve açıklama bilgileri yer almaktadır

            #region Item

            var invoiceItem = doc.CreateElement("cac", "Item", xmlnscac.Value);
            if (!string.IsNullOrEmpty(detayBilgisi.MalzemeFaturaAciklamasi))
            {
                var description = doc.CreateElement("cbc", "Description", xmlnscbc.Value);
                description.InnerText = detayBilgisi.MalzemeFaturaAciklamasi;
                invoiceItem.AppendChild(description);
            }

            var detayBilgisiName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(detayBilgisi.FaturaUrunTuru))
                detayBilgisiName.InnerText = detayBilgisi.FaturaUrunTuru;
            invoiceItem.AppendChild(detayBilgisiName);
            invoiceLine.AppendChild(invoiceItem);

            #endregion

            // Birim fiyat bilgileri yer almaktadır

            #region Price

            var price = doc.CreateElement("cac", "Price", xmlnscac.Value);
            var priceAmount = doc.CreateElement("cbc", "PriceAmount", xmlnscbc.Value);
            priceAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            priceAmount.Attributes.Append(currencyId);
            priceAmount.InnerText = detayBilgisi.BirimFiyat != null
                ? Decimal.Round((decimal) detayBilgisi.BirimFiyat, 4, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".")
                : "0.00";
            price.AppendChild(priceAmount);
            invoiceLine.AppendChild(price);

            #endregion

            root.AppendChild(invoiceLine);

            #endregion

            sayac++;
            return sayac;
        }

        /// <summary>
        /// E-Fatura veya Müstahsil Makbuzu Kesilen Kişi veya Firma Bilgilerinin Düzenlendiği
        /// Metottur.
        /// </summary>
        /// <param name="ortakVeriler">Fatura ve Makbuz Bünyesindeki Ortak Veriler</param>
        /// <param name="doc">XML Dokümanı Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        private static void FaturaMakbuzKesilenKisiBilgisiDuzenle(OrtakVerilerDTO ortakVeriler, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc, XmlElement root)
        {
            // Bu kısımda makbuz kesilen firma bilgileri yer almaktadır (Eğer sadece Tüzel Kişilikse, şahıs şirketleri için bu yapılmamaktadır)

            #region BuyerCustomerParty

            var buyerCustomerParty = doc.CreateElement("cac", "BuyerCustomerParty", xmlnscac.Value);
            var party = doc.CreateElement("cac", "Party", xmlnscac.Value);
            var webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
            party.AppendChild(webSiteUri);
            var buyerCustomerPartyIdAttr = doc.CreateAttribute("schemeID");
            buyerCustomerPartyIdAttr.Value = "VKN";
            var partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
            var buyerCustomerPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            buyerCustomerPartyPartyId.Attributes.Append(buyerCustomerPartyIdAttr);
            buyerCustomerPartyPartyId.InnerText = ortakVeriler.VergiNo;
            partyIdentification.AppendChild(buyerCustomerPartyPartyId);
            party.AppendChild(partyIdentification);

            if (ortakVeriler.VergiNo.Length == 10)
            {
                var partyName = doc.CreateElement("cac", "PartyName", xmlnscac.Value);
                var partyNameReal = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                partyNameReal.InnerText = ortakVeriler.TuzelKisiAd;
                partyName.AppendChild(partyNameReal);
                party.AppendChild(partyName);
            }

            #region Postal Address

            var postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
            var postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            postalAddressId.InnerText = ortakVeriler.VergiNo;
            postalAddress.AppendChild(postalAddressId);
            var room = doc.CreateElement("cbc", "Room", xmlnscbc.Value);
            postalAddress.AppendChild(room);
            var streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(ortakVeriler.Adres))
                streetName.InnerText = ortakVeriler.Adres;
            postalAddress.AppendChild(streetName);
            var buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
            //buildingNumber.InnerText = "";
            postalAddress.AppendChild(buildingNumber);
            var citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(ortakVeriler.IlceAd))
                citySubdivisionName.InnerText = ortakVeriler.IlceAd;
            postalAddress.AppendChild(citySubdivisionName);
            var cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(ortakVeriler.IlAd))
                cityName.InnerText = ortakVeriler.IlAd;
            postalAddress.AppendChild(cityName);
            var postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
            //postalZone.InnerText = "";
            postalAddress.AppendChild(postalZone);
            var country = doc.CreateElement("cac", "Country", xmlnscac.Value);
            var countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            countryName.InnerText = "Türkiye";
            country.AppendChild(countryName);
            postalAddress.AppendChild(country);
            party.AppendChild(postalAddress);

            #endregion

            var partyTaxScheme = doc.CreateElement("cac", "PartyTaxScheme", xmlnscac.Value);
            var taxScheme = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
            var taxSchemeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(ortakVeriler.VergiDairesi))
                taxSchemeName.InnerText = ortakVeriler.VergiDairesi;
            taxScheme.AppendChild(taxSchemeName);
            partyTaxScheme.AppendChild(taxScheme);
            party.AppendChild(partyTaxScheme);

            var contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
            var electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(ortakVeriler.EPostaAdresi))
                electronicMail.InnerText = ortakVeriler.EPostaAdresi;
            contact.AppendChild(electronicMail);
            party.AppendChild(contact);

            if (ortakVeriler.VergiNo.Length == 11)
            {
                var liste = ortakVeriler.TuzelKisiAd.Split(' ').ToList();
                if (liste.Count >= 1)
                {
                    var person = doc.CreateElement("cac", "Person", xmlnscac.Value);
                    var firstName = doc.CreateElement("cbc", "FirstName", xmlnscbc.Value);
                    firstName.InnerText = liste[0];
                    person.AppendChild(firstName);
                    var surname = liste[0];
                    if (liste.Count > 1)
                        surname = ortakVeriler.TuzelKisiAd.Substring(liste[0].Length + 1);
                    var familyName = doc.CreateElement("cbc", "FamilyName", xmlnscbc.Value);
                    familyName.InnerText = surname;
                    //familyName.InnerText = liste[1];
                    person.AppendChild(familyName);
                    party.AppendChild(person);
                }
            }

            buyerCustomerParty.AppendChild(party);

            root.AppendChild(buyerCustomerParty);

            #endregion
        }

        /// <summary>
        /// E-Fatura ve E-Arşiv tarafında
        /// XML dosyasına ilk hali verilmesi
        /// Fatura ile ilgili temel değer atamalarının yapılması için
        /// Hazırlanmış metottur.
        /// </summary>
        /// <param name="belgeTur">E-Fatura veya E-Arşiv Bilgisi</param>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        /// <param name="kodSatisTuruKod">Fatura Satış Türü Bilgisi</param>
        /// <param name="kodFaturaTuruKod">Fatura Türü Bilgisi</param>
        /// <param name="kodFaturaGrupTuruKod">Fatura Grup Türü Bilgisi</param>
        /// <returns>XML Dokümanı</returns>
        private static XmlDocument FaturaXMLDokumaniIlkHaliOlustur(BelgeTur belgeTur, ref GidenFaturaDTO gidenFatura, out XmlElement root, out int kodSatisTuruKod, out int kodFaturaTuruKod, out int kodFaturaGrupTuruKod)
        {
            var doc = new XmlDocument();
            var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            root = doc.DocumentElement;
            doc.InsertBefore(declaration, root);
            if (belgeTur == BelgeTur.EFatura)
                doc.AppendChild(doc.CreateProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"general.xslt\""));
            //doc.AppendChild(doc.CreateProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"general.xslt\""));

            kodSatisTuruKod = gidenFatura.SatisTuruKod ?? 0;
            kodFaturaTuruKod = gidenFatura.FaturaTuruKod ?? 0;
            kodFaturaGrupTuruKod = gidenFatura.FaturaGrupTuruKod ?? 0;

            gidenFatura = gidenFatura.BosluklariKaldir();

            root = doc.CreateElement("Invoice");
            return doc;
        }
        
        /// <summary>
        /// E-Arşiv Kısmında Tüzel Kişi de Olsa Şahıs Şirketi De Olsa
        /// Standart Alanlar, İmza Kısmı ve Faturayı Kesen Firma Kısmı ile ilgili kod blokları
        /// Aynı olduğu için
        /// Burada tek bir yerden güncelleme yapılması için gerekli işlemlerin gerçekleştirildiği metottur.
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="gidenFaturaDetayListesi">Giden Fatura Detay Listesi Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        /// <param name="doc">XML Dokümanı Bilgisi</param>
        /// <param name="kodSatisTuruKod">Fatura Satış Türü Bilgisi</param>
        /// <param name="kodFaturaTuruKod">Fatura Türü Bilgisi</param>
        /// <param name="kodFaturaGrupTuruKod">Fatura Grup Türü Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        private static void EArsivStandartImzaVeFirmaBilgisiDuzenle(GidenFaturaDTO gidenFatura, List<GidenFaturaDetayDTO> gidenFaturaDetayListesi, XmlElement root, XmlDocument doc, int kodSatisTuruKod, int kodFaturaTuruKod, int kodFaturaGrupTuruKod, out XmlAttribute xmlnscac, out XmlAttribute xmlnscbc)
        {
            // Faturanın türü, düzenleme tarihi, UBL Versiyon Numarası, GİB Numarası gibi bilgiler
            // yer almaktadır

            #region Standart Bilgiler

            root.RemoveAllAttributes();

            #region İlk Alanlar
            // var locationAttribute = "xsi:schemaLocation";
            var location = doc.CreateAttribute("xsi", "schemaLocation",
                "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
            location.Value =
                "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2 ../xsdrt/maindoc/UBL-Invoice-2.1.xsd";
            //var location = doc.CreateAttribute(schemaLocation);
            //location.Value = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2 ../xsdrt/maindoc/UBL-Invoice-2.1.xsd";
            var xmlns = doc.CreateAttribute("xmlns");
            xmlns.Value = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
            var xmlnsn4 = doc.CreateAttribute("xmlns:n4");
            xmlnsn4.Value = "http://www.altova.com/samplexml/other-namespace";
            var xmlnsxsi = doc.CreateAttribute("xmlns:xsi");
            xmlnsxsi.Value = "http://www.w3.org/2001/XMLSchema-instance";
            xmlnscac = doc.CreateAttribute("xmlns:cac");
            xmlnscac.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            xmlnscbc = doc.CreateAttribute("xmlns:cbc");
            xmlnscbc.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            var xmlnsext = doc.CreateAttribute("xmlns:ext");
            xmlnsext.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
            var xmlnsxades = doc.CreateAttribute("xmlns:xades");
            //xmlnsxades.Value = "http://uri.etsi.org/01903/v1.3.2#";
            xmlnsxades.Value = "http://uri.etsi.org/01903/v1.3.2";
            var xmlnsds = doc.CreateAttribute("xmlns:ds");
            //xmlnsds.Value = "http://www.w3.org/2000/09/xmldsig#";
            xmlnsds.Value = "http://www.w3.org/2000/09/xmldsig";

            root.Attributes.Append(location);
            root.Attributes.Append(xmlns);
            root.Attributes.Append(xmlnsxsi);
            root.Attributes.Append(xmlnscac);
            root.Attributes.Append(xmlnsext);
            root.Attributes.Append(xmlnsds);
            root.Attributes.Append(xmlnsxades);
            root.Attributes.Append(xmlnscbc);
            root.Attributes.Append(xmlnsn4);

            var extUbl = doc.CreateElement("ext", "UBLExtensions", xmlnsext.Value);
            var extUbl2 = doc.CreateElement("ext", "UBLExtension", xmlnsext.Value);
            var extUbl3 = doc.CreateElement("ext", "ExtensionContent", xmlnsext.Value);
            extUbl2.AppendChild(extUbl3);
            extUbl.AppendChild(extUbl2);

            root.AppendChild(extUbl);
            #endregion

            TemelBilgileriDuzenle("EARSIVFATURA", gidenFatura, gidenFaturaDetayListesi, root, doc, kodSatisTuruKod, kodFaturaTuruKod, xmlnscbc);

            IrsaliyeBilgisiDuzenle(gidenFatura, gidenFaturaDetayListesi, root, doc, kodFaturaTuruKod, kodFaturaGrupTuruKod, xmlnscbc);

            GonderimSekliBilgisiDuzenle(root, doc, xmlnscbc);

            ParaBirimiVeKayitSayisiDuzenle(gidenFaturaDetayListesi.Count, root, doc, xmlnscbc);

            #region Açıklama Satırı

            // #region AdditionalDocumentReference
            //
            // var additionalDocumentReference =
            //     doc.CreateElement("cac", "AdditionalDocumentReference", xmlnscac.Value);
            // var additionalDocumentReferenceId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            // additionalDocumentReferenceId.InnerText = gidenFatura.Id;
            // additionalDocumentReference.AppendChild(additionalDocumentReferenceId);
            // issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
            // issueDate.InnerText = gidenFatura.IslemTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
            // additionalDocumentReference.AppendChild(issueDate);
            // var documentType = doc.CreateElement("cbc", "DocumentType", xmlnscbc.Value);
            // documentType.InnerText = "XSLT";
            // additionalDocumentReference.AppendChild(documentType);
            // issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
            // issueDate.InnerText = gidenFatura.IslemTarihi?.Date.ToString("yyyy-MM-dd");
            // additionalDocumentReference.AppendChild(issueDate);
            // documentType = doc.CreateElement("cbc", "DocumentType", xmlnscbc.Value);
            // documentType.InnerText = "XSLT";
            // additionalDocumentReference.AppendChild(documentType);
            // var attachment = doc.CreateElement("cac", "Attachment", xmlnscac.Value);
            // var embeddedDocumentBinaryObject =
            //     doc.CreateElement("cbc", "EmbeddedDocumentBinaryObject", xmlnscbc.Value);
            // var characterSetCode = doc.CreateAttribute("characterSetCode");
            // characterSetCode.Value = "UTF-8";
            // var encodingCode = doc.CreateAttribute("encodingCode");
            // encodingCode.Value = "Base64";
            // // XmlAttribute fileName2 = doc.CreateAttribute("fileName");
            // // fileName2.Value = "efatura.xslt";
            // var mimeCode = doc.CreateAttribute("mimeCode");
            // mimeCode.Value = "application/xml";
            // embeddedDocumentBinaryObject.Attributes.Append(characterSetCode);
            // embeddedDocumentBinaryObject.Attributes.Append(encodingCode);
            // // embeddedDocumentBinaryObject.Attributes.Append(fileName2);
            // embeddedDocumentBinaryObject.Attributes.Append(mimeCode);
            //
            // #region Base 64 Metin
            //
            // var sablonAdresi = Directory.GetFiles("../../Other");
            // if (sablonAdresi.Length > 0)
            // {
            //     foreach (var item in sablonAdresi)
            //     {
            //         if (item.Contains("earsiv"))
            //         {
            //             var base64Metin = File.ReadAllText(item);
            //             embeddedDocumentBinaryObject.InnerText = base64Metin;
            //             break;
            //         }
            //     }
            // }
            //
            // #endregion
            //
            // attachment.AppendChild(embeddedDocumentBinaryObject);
            // additionalDocumentReference.AppendChild(attachment);
            //
            // root.AppendChild(additionalDocumentReference);
            //
            // additionalDocumentReference = doc.CreateElement("cac", "AdditionalDocumentReference", xmlnscac.Value);
            // additionalDocumentReferenceId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            // additionalDocumentReferenceId.InnerText = gidenFatura.Id;
            // additionalDocumentReference.AppendChild(additionalDocumentReferenceId);
            // issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
            // issueDate.InnerText = gidenFatura.IslemTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
            // additionalDocumentReference.AppendChild(issueDate);
            // documentType = doc.CreateElement("cbc", "DocumentType", xmlnscbc.Value);
            // documentType.InnerText = "XSLT";
            // additionalDocumentReference.AppendChild(documentType);
            // attachment = doc.CreateElement("cac", "Attachment", xmlnscac.Value);
            // embeddedDocumentBinaryObject = doc.CreateElement("cbc", "EmbeddedDocumentBinaryObject", xmlnscbc.Value);
            // characterSetCode = doc.CreateAttribute("characterSetCode");
            // characterSetCode.Value = "UTF-8";
            // encodingCode = doc.CreateAttribute("encodingCode");
            // encodingCode.Value = "Base64";
            // mimeCode = doc.CreateAttribute("mimeCode");
            // mimeCode.Value = "application/xml";
            // embeddedDocumentBinaryObject.Attributes.Append(characterSetCode);
            // embeddedDocumentBinaryObject.Attributes.Append(encodingCode);
            // embeddedDocumentBinaryObject.Attributes.Append(mimeCode);
            // attachment.AppendChild(embeddedDocumentBinaryObject);
            // additionalDocumentReference.AppendChild(attachment);
            //
            // #endregion

            #endregion

            #endregion
          
            FaturaMakbuzImzaBilgisiDuzenle(root, doc, xmlnscac, xmlnscbc);

            FaturaKesenFirmaBilgisiDuzenle(gidenFatura, root, doc, xmlnscac, xmlnscbc);
        }

        /// <summary>
        /// E-Fatura ve Müstahsil Makbuzu tarafında
        /// Gönderim Şekli bilgileri düzenlenmesi için
        /// Gerekli bilgilerin düzenlendiği bir metottur.
        /// </summary>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        /// <param name="doc">XML Doküman Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        private static void GonderimSekliBilgisiDuzenle(XmlElement root, XmlDocument doc, XmlAttribute xmlnscbc)
        {
            var sendType = doc.CreateElement("cbc", "Note", xmlnscbc.Value);
            sendType.InnerText = "Gönderim Şekli: ELEKTRONIK";
            root.AppendChild(sendType);
        }

        /// <summary>
        /// E-Fatura ve E-Arşiv tarafında
        /// Fatura kesen firma bilgileri aynı olduğu için
        /// Bu bilgilerin tek bir yerden girilebilmesi adına
        /// Gerekli işlemlerin gerçekleştirildiği metottur.
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        /// <param name="doc">XML Dokümanı Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        private static void FaturaKesenFirmaBilgisiDuzenle(GidenFaturaDTO gidenFatura, XmlElement root, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc)
        {
            // Burada sabit değerler verilmekte olup, Faturayı kesen kuruma veya firmaya ait bilgiler
            // yer almaktadır. Bu kısımda örnek olarak TÜRKŞEKER Fabrikaları Genel Müdürlüğü bilgileri
            // kullanılmıştır. Ancak değiştirip test edilebilir.

            #region AccountingSupplierParty

            var accountingSupplierParty = doc.CreateElement("cac", "AccountingSupplierParty", xmlnscac.Value);
            var party = doc.CreateElement("cac", "Party", xmlnscac.Value);
            var webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
            webSiteUri.InnerText = "https://www.turkseker.gov.tr";
            party.AppendChild(webSiteUri);
            var accountingSupplierPartyIdAttr = doc.CreateAttribute("schemeID");
            accountingSupplierPartyIdAttr.Value = "VKN_TCKN";
            var partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
            var accountingSupplierPartyIdAttr2 = doc.CreateAttribute("schemeID");
            accountingSupplierPartyIdAttr2.Value = "VKN";
            var accountingSupplierPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            accountingSupplierPartyPartyId.Attributes.Append(accountingSupplierPartyIdAttr2);
            accountingSupplierPartyPartyId.InnerText = "3250566851";
            partyIdentification.AppendChild(accountingSupplierPartyPartyId);
            party.AppendChild(partyIdentification);

            if (!string.IsNullOrEmpty(gidenFatura.AltBirimAd))
            {
                partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
                accountingSupplierPartyIdAttr2 = doc.CreateAttribute("schemeID");
                accountingSupplierPartyIdAttr2.Value = "SUBENO";
                accountingSupplierPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                accountingSupplierPartyPartyId.Attributes.Append(accountingSupplierPartyIdAttr2);
                accountingSupplierPartyPartyId.InnerText = gidenFatura.AltBirimAd;
                partyIdentification.AppendChild(accountingSupplierPartyPartyId);
                party.AppendChild(partyIdentification);
            }

            var partyName = doc.CreateElement("cac", "PartyName", xmlnscac.Value);
            var partyNameReal = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            partyNameReal.InnerText = "TÜRKİYE ŞEKER FABRİKALARI A.Ş.";
            partyName.AppendChild(partyNameReal);
            party.AppendChild(partyName);

            #region Postal Address

            var postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
            var postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            postalAddressId.InnerText = "3250566851";
            postalAddress.AppendChild(postalAddressId);
            var streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
            streetName.InnerText = "Mithatpaşa Caddesi";
            postalAddress.AppendChild(streetName);
            var buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
            buildingNumber.InnerText = "14";
            postalAddress.AppendChild(buildingNumber);
            var citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
            citySubdivisionName.InnerText = "Çankaya";
            postalAddress.AppendChild(citySubdivisionName);
            var cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
            cityName.InnerText = "Ankara";
            postalAddress.AppendChild(cityName);
            var postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
            postalZone.InnerText = "06100";
            postalAddress.AppendChild(postalZone);
            var country = doc.CreateElement("cac", "Country", xmlnscac.Value);
            var countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            countryName.InnerText = "Türkiye";
            country.AppendChild(countryName);
            postalAddress.AppendChild(country);
            party.AppendChild(postalAddress);

            #endregion

            var partyTaxScheme = doc.CreateElement("cac", "PartyTaxScheme", xmlnscac.Value);
            var taxScheme = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
            var taxSchemeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            taxSchemeName.InnerText = "Ankara Kurumlar";
            taxScheme.AppendChild(taxSchemeName);
            partyTaxScheme.AppendChild(taxScheme);
            party.AppendChild(partyTaxScheme);

            var contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
            var telephone = doc.CreateElement("cbc", "Telephone", xmlnscbc.Value);
            telephone.InnerText = "(312) 4585500";
            contact.AppendChild(telephone);
            var telefax = doc.CreateElement("cbc", "Telefax", xmlnscbc.Value);
            telefax.InnerText = "(312) 4585800";
            contact.AppendChild(telefax);
            var electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
            electronicMail.InnerText = "maliisler@turkseker.gov.tr";
            contact.AppendChild(electronicMail);
            party.AppendChild(contact);

            accountingSupplierParty.AppendChild(party);

            root.AppendChild(accountingSupplierParty);

            #endregion
        }

        /// <summary>
        /// E-Fatura ve E-Arşiv tarafında
        /// Dokümanın başında yer alan bazı XML elemanlarının ortak bir şekilde
        /// Düzenlenmesi için hazırlanmış metottur.
        /// </summary>
        /// <param name="faturaTuru">Fatura Türü Bilgisi</param>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="gidenFaturaDetayListesi">Giden Fatura Detay Listesi Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        /// <param name="doc">XML Dokümanı Bilgisi</param>
        /// <param name="kodSatisTuruKod">Fatura Satış Türü Bilgisi</param>
        /// <param name="kodFaturaTuruKod">Fatura Türü Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        private static void TemelBilgileriDuzenle(string faturaTuru, GidenFaturaDTO gidenFatura, List<GidenFaturaDetayDTO> gidenFaturaDetayListesi, XmlElement root, XmlDocument doc, int kodSatisTuruKod, int kodFaturaTuruKod, XmlAttribute xmlnscbc)
        {
            var versionId = doc.CreateElement("cbc", "UBLVersionID", xmlnscbc.Value);
            versionId.InnerText = "2.1";
            root.AppendChild(versionId);

            var customizationId = doc.CreateElement("cbc", "CustomizationID", xmlnscbc.Value);
            customizationId.InnerText = "TR1.2";
            root.AppendChild(customizationId);

            var profileId = doc.CreateElement("cbc", "ProfileID", xmlnscbc.Value);
            profileId.InnerText = faturaTuru;
            root.AppendChild(profileId);

            var id = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.GibNumarasi))
                id.InnerText = gidenFatura.GibNumarasi;
            else
                id.InnerText = "ABC" + gidenFatura.IslemTarihi?.Year +
                               DateTime.UtcNow.Ticks.ToString().Substring(0, 9);
            root.AppendChild(id);

            IndicatorVeUuidBilgisiDuzenle(gidenFatura.Id, root, doc, xmlnscbc);

            TarihSaatDuzenle(gidenFatura.IslemTarihi, root, doc, xmlnscbc);

            var invoiceTypeCode = doc.CreateElement("cbc", "InvoiceTypeCode", xmlnscbc.Value);
            invoiceTypeCode.InnerText = "SATIS";
            if (gidenFatura.KdvTutari == 0 || gidenFaturaDetayListesi.Any(j => j.KdvTutari == 0))
                invoiceTypeCode.InnerText = "ISTISNA";
            if (kodSatisTuruKod == SatisTur.IhracKayitli.GetHashCode())
                invoiceTypeCode.InnerText = "IHRACKAYITLI";
            if (kodFaturaTuruKod == FaturaTur.Iade.GetHashCode())
                invoiceTypeCode.InnerText = "IADE";
            root.AppendChild(invoiceTypeCode);
        }

        /// <summary>
        /// E-Fatura ve Müstahsil Makbuzu tarafında
        /// Copy Indicator ve UUID bilgileri düzenlenmesi için
        /// Gerekli bilgilerin düzenlendiği bir metottur.
        /// </summary>
        /// <param name="id">Fatura veya Makbuz Id Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        /// <param name="doc">XML Doküman Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        private static void IndicatorVeUuidBilgisiDuzenle(string id, XmlElement root, XmlDocument doc, XmlAttribute xmlnscbc)
        {
            var copyIndicator = doc.CreateElement("cbc", "CopyIndicator", xmlnscbc.Value);
            copyIndicator.InnerText = "false";
            root.AppendChild(copyIndicator);

            var nesneId = doc.CreateElement("cbc", "UUID", xmlnscbc.Value);
            nesneId.InnerText = id.Length == 36
                ? id.ToUpper()
                : Guid.NewGuid().ToString().ToUpper();
            root.AppendChild(nesneId);
        }

        /// <summary>
        /// E-Fatura ve Müstahsil Makbuzu tarafında
        /// Tarih ve saat bilgileri düzenlenmesi için
        /// Gerekli bilgilerin düzenlendiği bir metottur.
        /// </summary>
        /// <param name="islemTarihi">Fatura veya Makbuz İşlem Tarihi Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        /// <param name="doc">XML Doküman Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        private static void TarihSaatDuzenle(DateTime? islemTarihi, XmlElement root, XmlDocument doc, XmlAttribute xmlnscbc)
        {
            var issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
            issueDate.InnerText = islemTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
            root.AppendChild(issueDate);

            var issueTime = doc.CreateElement("cbc", "IssueTime", xmlnscbc.Value);
            issueTime.InnerText = islemTarihi?.ToString("HH:mm:ss") ?? string.Empty;
            root.AppendChild(issueTime);
        }

        /// <summary>
        /// E-Fatura olsun E-Arşiv olsun
        /// Fatura Detaylarından İrsaliye bilgisini düzenleyip buna göre
        /// Gerekli işlemlerin gerçekleşmesini sağlayan metottur.
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="gidenFaturaDetayListesi">Giden Fatura Detay Listesi Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        /// <param name="doc">XML Dokümanı Bilgisi</param>
        /// <param name="kodFaturaTuruKod">Fatura Türü Bilgisi</param>
        /// <param name="kodFaturaGrupTuruKod">Fatura Grup Türü Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        private static void IrsaliyeBilgisiDuzenle(GidenFaturaDTO gidenFatura, List<GidenFaturaDetayDTO> gidenFaturaDetayListesi, XmlElement root, XmlDocument doc, int kodFaturaTuruKod, int kodFaturaGrupTuruKod, XmlAttribute xmlnscbc)
        {
            #region İrsaliye Bilgileri

            #region İrsaliye ve Tonaj Düzenlemesi

            var gidenFaturaDetayListesi2 = new List<GidenFaturaDetayDTO>();
            foreach (var item in gidenFaturaDetayListesi)
            {
                if (!string.IsNullOrEmpty(item.IrsaliyeNo) && !string.IsNullOrEmpty(item.PlakaNo))
                {
                    if (!gidenFaturaDetayListesi2.Any(j =>
                            j.IrsaliyeNo == item.IrsaliyeNo && j.PlakaNo == item.PlakaNo))
                    {
                        item.Tonaj = item.Miktar;
                        gidenFaturaDetayListesi2.Add(item);
                    }
                    else
                    {
                        var index = gidenFaturaDetayListesi2.FindIndex(j =>
                            j.IrsaliyeNo == item.IrsaliyeNo && j.PlakaNo == item.PlakaNo);
                        gidenFaturaDetayListesi2[index].Tonaj += item.Miktar;
                    }
                }
                else
                    gidenFaturaDetayListesi2.Add(item);
            }
            //if (gidenFaturaDetayListesi2.Count < 1)
            //    gidenFaturaDetayListesi2.Add(new GidenFaturaDetayDTO());

            #endregion İrsaliye ve Tonaj Düzenlemesi

            if (gidenFaturaDetayListesi2.Count <= 0) return;
            {
                foreach (var item in gidenFaturaDetayListesi2)
                {
                    var irsaliyeNote = doc.CreateElement("cbc", "Note", xmlnscbc.Value);
                    irsaliyeNote.InnerText = "";
                    if (!string.IsNullOrEmpty(item.PlakaNo))
                        irsaliyeNote.InnerText += "Plaka No: " + item.PlakaNo;
                    if (!string.IsNullOrEmpty(item.SevkIrsaliyesiNo))
                        irsaliyeNote.InnerText += " İrsaliye No: " + item.SevkIrsaliyesiNo;
                    if (item.SevkIrsaliyeTarihi != null)
                        irsaliyeNote.InnerText +=
                            " İrsaliye Tarihi: " + item.SevkIrsaliyeTarihi?.ToShortDateString();
                    if (item.YuklemeFormuNo != null)
                        irsaliyeNote.InnerText += " Yükleme Formu No: " + item.YuklemeFormuNo;
                    if (item.Tonaj != null)
                        irsaliyeNote.InnerText += " Tonaj: " + item.Tonaj;
                    if (kodFaturaTuruKod == FaturaTur.Iade.GetHashCode() ||
                        ((item.FaturaUrunTuru.Contains("Şartname") || item.FaturaUrunTuru.Contains("Kira")) &&
                         kodFaturaGrupTuruKod == FaturaMakbuzGrupTur.Diger.GetHashCode()) &&
                        !string.IsNullOrEmpty(gidenFatura.Aciklama))
                        irsaliyeNote.InnerText += " Açıklama: " + gidenFatura.Aciklama;
                    if (!string.IsNullOrEmpty(irsaliyeNote.InnerText))
                        root.AppendChild(irsaliyeNote);
                }
            }

            #endregion
        }
        
        /// <summary>
        /// E-Arşiv faturasında şahıs şirketine ait kısımların hazırlanması için
        /// Gerçekleştirilmiş metottur
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="doc">XML Dokümanı Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        private static void EArsivSahisSirketineAitKismiHazirla(GidenFaturaDTO gidenFatura, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc, XmlElement root)
        {
            #region Şahıs Şirketine Has Bölüm

            // Bu kısımda fatura kesilen firma bilgileri yer almaktadır

            #region AccountingCustomerParty

            var accountingCustomerParty = doc.CreateElement("cac", "AccountingCustomerParty", xmlnscac.Value);
            var party = doc.CreateElement("cac", "Party", xmlnscac.Value);
            var webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
            party.AppendChild(webSiteUri);
            var accountingCustomerPartyIdAttr = doc.CreateAttribute("schemeID");
            accountingCustomerPartyIdAttr.Value = "TCKN";
            var partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
            var accountingCustomerPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            accountingCustomerPartyPartyId.Attributes.Append(accountingCustomerPartyIdAttr);
            accountingCustomerPartyPartyId.InnerText = gidenFatura.GercekKisiTcKimlikNo;
            partyIdentification.AppendChild(accountingCustomerPartyPartyId);
            party.AppendChild(partyIdentification);

            #region Postal Address

            var postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
            var postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            postalAddressId.InnerText = gidenFatura.GercekKisiTcKimlikNo;
            postalAddress.AppendChild(postalAddressId);
            var room = doc.CreateElement("cbc", "Room", xmlnscbc.Value);
            postalAddress.AppendChild(room);
            var streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.IkametgahAdresi))
                streetName.InnerText = gidenFatura.IkametgahAdresi;
            postalAddress.AppendChild(streetName);
            var buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
            //buildingNumber.InnerText = "";
            postalAddress.AppendChild(buildingNumber);
            var citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.IlceAd))
                citySubdivisionName.InnerText = gidenFatura.IlceAd;
            postalAddress.AppendChild(citySubdivisionName);
            var cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.IlAd))
                cityName.InnerText = gidenFatura.IlAd;
            postalAddress.AppendChild(cityName);
            var postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
            //postalZone.InnerText = "";
            postalAddress.AppendChild(postalZone);
            var country = doc.CreateElement("cac", "Country", xmlnscac.Value);
            var countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            countryName.InnerText = "Türkiye";
            country.AppendChild(countryName);
            postalAddress.AppendChild(country);
            party.AppendChild(postalAddress);

            #endregion

            var partyTaxScheme = doc.CreateElement("cac", "PartyTaxScheme", xmlnscac.Value);
            var taxScheme = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
            var taxSchemeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            //if (!string.IsNullOrEmpty(gidenFatura.VergiDairesi))
            //    taxSchemeName.InnerText = gidenFatura.VergiDairesi;
            taxScheme.AppendChild(taxSchemeName);
            partyTaxScheme.AppendChild(taxScheme);
            party.AppendChild(partyTaxScheme);

            var contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
            var telephone = doc.CreateElement("cbc", "Telephone", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.CepTelefonNo))
                telephone.InnerText = gidenFatura.CepTelefonNo;
            contact.AppendChild(telephone);
            var telefax = doc.CreateElement("cbc", "Telefax", xmlnscbc.Value);
            //if (!string.IsNullOrEmpty(gidenFatura.FaksNo))
            //    telefax.InnerText = gidenFatura.FaksNo;
            contact.AppendChild(telefax);
            var electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.EPostaAdresi))
                electronicMail.InnerText = gidenFatura.EPostaAdresi;
            contact.AppendChild(electronicMail);
            party.AppendChild(contact);

            var person = doc.CreateElement("cac", "Person", xmlnscac.Value);
            var firstName = doc.CreateElement("cbc", "FirstName", xmlnscbc.Value);
            firstName.InnerText = gidenFatura.GercekKisiAd;
            person.AppendChild(firstName);
            var familyName = doc.CreateElement("cbc", "FamilyName", xmlnscbc.Value);
            familyName.InnerText = gidenFatura.GercekKisiSoyad;
            person.AppendChild(familyName);
            party.AppendChild(person);

            accountingCustomerParty.AppendChild(party);

            root.AppendChild(accountingCustomerParty);

            #endregion

            #endregion
        }
        
        /// <summary>
        /// E-Arşiv faturasında tüzel kişiye ait kısımların hazırlanması için
        /// Gerçekleştirilmiş metottur
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="doc">XML Dokümanı Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        private static void EArsivTuzelKisiyeAitKismiHazirla(GidenFaturaDTO gidenFatura, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc, XmlElement root)
        {
            XmlElement partyName, partyNameReal;

            #region Tüzel Kişiye Has Bölüm

            // Bu kısımda fatura kesilen firma bilgileri yer almaktadır

            #region AccountingCustomerParty

            var accountingCustomerParty = doc.CreateElement("cac", "AccountingCustomerParty", xmlnscac.Value);
            var party = doc.CreateElement("cac", "Party", xmlnscac.Value);
            var webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
            party.AppendChild(webSiteUri);
            var accountingCustomerPartyIdAttr = doc.CreateAttribute("schemeID");
            accountingCustomerPartyIdAttr.Value = "TCKN";
            if (!string.IsNullOrEmpty(gidenFatura.VergiNo) && gidenFatura.VergiNo.Length == 10)
                accountingCustomerPartyIdAttr.Value = "VKN";
            var partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
            var accountingCustomerPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            accountingCustomerPartyPartyId.Attributes.Append(accountingCustomerPartyIdAttr);
            accountingCustomerPartyPartyId.InnerText = gidenFatura.VergiNo;
            partyIdentification.AppendChild(accountingCustomerPartyPartyId);
            party.AppendChild(partyIdentification);

            if (!string.IsNullOrEmpty(gidenFatura.VergiNo) && gidenFatura.VergiNo.Length == 10)
            {
                partyName = doc.CreateElement("cac", "PartyName", xmlnscac.Value);
                partyNameReal = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                partyNameReal.InnerText = gidenFatura.TuzelKisiAd;
                partyName.AppendChild(partyNameReal);
                party.AppendChild(partyName);
            }

            #region Postal Address

            var postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
            var postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            postalAddressId.InnerText = gidenFatura.VergiNo;
            postalAddress.AppendChild(postalAddressId);
            var room = doc.CreateElement("cbc", "Room", xmlnscbc.Value);
            postalAddress.AppendChild(room);
            var streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.Adres))
                streetName.InnerText = gidenFatura.Adres;
            postalAddress.AppendChild(streetName);
            var buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
            //buildingNumber.InnerText = "";
            postalAddress.AppendChild(buildingNumber);
            var citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.IlceAd))
                citySubdivisionName.InnerText = gidenFatura.IlceAd;
            postalAddress.AppendChild(citySubdivisionName);
            var cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.IlAd))
                cityName.InnerText = gidenFatura.IlAd;
            postalAddress.AppendChild(cityName);
            var postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
            //postalZone.InnerText = "";
            postalAddress.AppendChild(postalZone);
            var country = doc.CreateElement("cac", "Country", xmlnscac.Value);
            var countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            countryName.InnerText = "Türkiye";
            country.AppendChild(countryName);
            postalAddress.AppendChild(country);
            party.AppendChild(postalAddress);

            #endregion

            var partyTaxScheme = doc.CreateElement("cac", "PartyTaxScheme", xmlnscac.Value);
            var taxScheme = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
            var taxSchemeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.VergiDairesi))
                taxSchemeName.InnerText = gidenFatura.VergiDairesi;
            taxScheme.AppendChild(taxSchemeName);
            partyTaxScheme.AppendChild(taxScheme);
            party.AppendChild(partyTaxScheme);

            var contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
            var telephone = doc.CreateElement("cbc", "Telephone", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.TelefonNo))
                telephone.InnerText = gidenFatura.TelefonNo;
            contact.AppendChild(telephone);
            var telefax = doc.CreateElement("cbc", "Telefax", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.FaksNo))
                telefax.InnerText = gidenFatura.FaksNo;
            contact.AppendChild(telefax);
            var electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.EPostaAdresi))
                electronicMail.InnerText = gidenFatura.EPostaAdresi;
            contact.AppendChild(electronicMail);
            party.AppendChild(contact);

            if (!string.IsNullOrEmpty(gidenFatura.VergiNo) && gidenFatura.VergiNo.Length == 11)
            {
                var liste = gidenFatura.TuzelKisiAd.Split(' ').ToList();
                if (liste.Count >= 1)
                {
                    var person = doc.CreateElement("cac", "Person", xmlnscac.Value);
                    var firstName = doc.CreateElement("cbc", "FirstName", xmlnscbc.Value);
                    firstName.InnerText = liste[0];
                    person.AppendChild(firstName);
                    var surname = liste[0];
                    if (liste.Count > 1)
                        surname = gidenFatura.TuzelKisiAd.Substring(liste[0].Length + 1);
                    var familyName = doc.CreateElement("cbc", "FamilyName", xmlnscbc.Value);
                    familyName.InnerText = surname;
                    //familyName.InnerText = liste[1];
                    person.AppendChild(familyName);
                    party.AppendChild(person);
                }
            }

            accountingCustomerParty.AppendChild(party);

            root.AppendChild(accountingCustomerParty);

            #endregion

            if (string.IsNullOrEmpty(gidenFatura.VergiNo)) return;
            {
                if (gidenFatura.VergiNo.Length != 10) return;
                // Bu kısımda fatura kesilen firma bilgileri yer almaktadır (Eğer sadece Tüzel Kişilikse, şahıs şirketleri için bu yapılmamaktadır)

                #region BuyerCustomerParty

                var buyerCustomerParty = doc.CreateElement("cac", "BuyerCustomerParty", xmlnscac.Value);
                party = doc.CreateElement("cac", "Party", xmlnscac.Value);
                webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
                party.AppendChild(webSiteUri);
                var buyerCustomerPartyIdAttr = doc.CreateAttribute("schemeID");
                buyerCustomerPartyIdAttr.Value = "VKN";
                partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
                var buyerCustomerPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                buyerCustomerPartyPartyId.Attributes.Append(buyerCustomerPartyIdAttr);
                buyerCustomerPartyPartyId.InnerText = gidenFatura.VergiNo;
                partyIdentification.AppendChild(buyerCustomerPartyPartyId);
                party.AppendChild(partyIdentification);

                if (gidenFatura.VergiNo.Length == 10)
                {
                    partyName = doc.CreateElement("cac", "PartyName", xmlnscac.Value);
                    partyNameReal = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                    partyNameReal.InnerText = gidenFatura.TuzelKisiAd;
                    partyName.AppendChild(partyNameReal);
                    party.AppendChild(partyName);
                }

                #region Postal Address

                postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
                postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                postalAddressId.InnerText = gidenFatura.VergiNo;
                postalAddress.AppendChild(postalAddressId);
                room = doc.CreateElement("cbc", "Room", xmlnscbc.Value);
                postalAddress.AppendChild(room);
                streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.Adres))
                    streetName.InnerText = gidenFatura.Adres;
                postalAddress.AppendChild(streetName);
                buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
                //buildingNumber.InnerText = "";
                postalAddress.AppendChild(buildingNumber);
                citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.IlceAd))
                    citySubdivisionName.InnerText = gidenFatura.IlceAd;
                postalAddress.AppendChild(citySubdivisionName);
                cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.IlAd))
                    cityName.InnerText = gidenFatura.IlAd;
                postalAddress.AppendChild(cityName);
                postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
                //postalZone.InnerText = "";
                postalAddress.AppendChild(postalZone);
                country = doc.CreateElement("cac", "Country", xmlnscac.Value);
                countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                countryName.InnerText = "Türkiye";
                country.AppendChild(countryName);
                postalAddress.AppendChild(country);
                party.AppendChild(postalAddress);

                #endregion

                partyTaxScheme = doc.CreateElement("cac", "PartyTaxScheme", xmlnscac.Value);
                taxScheme = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                taxSchemeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.VergiDairesi))
                    taxSchemeName.InnerText = gidenFatura.VergiDairesi;
                taxScheme.AppendChild(taxSchemeName);
                partyTaxScheme.AppendChild(taxScheme);
                party.AppendChild(partyTaxScheme);

                contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
                telephone = doc.CreateElement("cbc", "Telephone", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.TelefonNo))
                    telephone.InnerText = gidenFatura.TelefonNo;
                contact.AppendChild(telephone);
                telefax = doc.CreateElement("cbc", "Telefax", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.FaksNo))
                    telefax.InnerText = gidenFatura.FaksNo;
                contact.AppendChild(telefax);
                electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.EPostaAdresi))
                    electronicMail.InnerText = gidenFatura.EPostaAdresi;
                contact.AppendChild(electronicMail);
                party.AppendChild(contact);

                if (gidenFatura.VergiNo.Length == 11)
                {
                    var liste = gidenFatura.TuzelKisiAd.Split(' ').ToList();
                    if (liste.Count >= 1)
                    {
                        var person = doc.CreateElement("cac", "Person", xmlnscac.Value);
                        var firstName = doc.CreateElement("cbc", "FirstName", xmlnscbc.Value);
                        firstName.InnerText = liste[0];
                        person.AppendChild(firstName);
                        var surname = liste[0];
                        if (liste.Count > 1)
                            surname = gidenFatura.TuzelKisiAd.Substring(liste[0].Length + 1);
                        var familyName = doc.CreateElement("cbc", "FamilyName", xmlnscbc.Value);
                        familyName.InnerText = surname;
                        //familyName.InnerText = liste[1];
                        person.AppendChild(familyName);
                        party.AppendChild(person);
                    }
                }

                buyerCustomerParty.AppendChild(party);

                root.AppendChild(buyerCustomerParty);

                #endregion
            }

            #endregion
        }

        /// <summary>
        /// E-Arşiv Kısmında Tüzel Kişi De Olsa Şahıs Şirketi De Olsa
        /// Banka Bilgilerinin hazırlandığı kod bloğu aynı olduğu için
        /// Burada tek bir yerden güncelleme yapılması için gerekli işlemlerin gerçekleştirildiği metottur.
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="doc">XML Dokümanı Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        private static void EArsivBankaBilgileriDuzenle(GidenFaturaDTO gidenFatura, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc, XmlElement root)
        {
            if (string.IsNullOrEmpty(gidenFatura.BankaAd)) return;

            // Eğer Banka bilgileri mevcutsa burada banka bilgileri yer almaktadır

            #region PaymentMeans

            var paymentMeans = doc.CreateElement("cac", "PaymentMeans", xmlnscac.Value);
            var paymentMeansCode = doc.CreateElement("cbc", "PaymentMeansCode", xmlnscbc.Value);
            //paymentMeansCode.InnerText = "1";

            // Burada 1 verildiği zaman Sözleşme Kapsamında yazıyor metinde
            // ZZZ ise Diğer anlamında geliyor
            paymentMeansCode.InnerText = "ZZZ";
            paymentMeans.AppendChild(paymentMeansCode);
            var payeeFinancialAccount = doc.CreateElement("cac", "PayeeFinancialAccount", xmlnscac.Value);
            var payeeFinancialAccountId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.IbanNo))
                payeeFinancialAccountId.InnerText = gidenFatura.IbanNo;
            payeeFinancialAccount.AppendChild(payeeFinancialAccountId);
            var currencyCode = doc.CreateElement("cbc", "CurrencyCode", xmlnscbc.Value);
            currencyCode.InnerText = "TRY";
            payeeFinancialAccount.AppendChild(currencyCode);
            var paymentNote = doc.CreateElement("cbc", "PaymentNote", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.BankaAd) && !string.IsNullOrEmpty(gidenFatura.BankaSube))
                paymentNote.InnerText = gidenFatura.BankaAd + " - " + gidenFatura.BankaSube;
            payeeFinancialAccount.AppendChild(paymentNote);
            paymentMeans.AppendChild(payeeFinancialAccount);
            root.AppendChild(paymentMeans);

            #endregion
        }

        /// <summary>
        /// E-Arşiv Kısmında Tüzel Kişi de Olsa Şahıs Şirketi De Olsa
        /// Vergi, İskonto vb. Kısmı ile ilgili kod blokları
        /// Aynı olduğu için
        /// Burada tek bir yerden güncelleme yapılması için gerekli işlemlerin gerçekleştirildiği metottur.
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="gidenFaturaDetayListesi">Giden Fatura Detay Listesi Bilgisi</param>
        /// <param name="doc">XML Dokümanı Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="kodFaturaGrupTuruKod">Fatura Grup Türü Bilgisi</param>
        /// <param name="kodSatisTuruKod">Fatura Satış Türü Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        private static void EArsivVergiVeDigerAlanlarDuzenle(GidenFaturaDTO gidenFatura, List<GidenFaturaDetayDTO> gidenFaturaDetayListesi, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc, int kodFaturaGrupTuruKod, int kodSatisTuruKod, XmlElement root)
        {
            FaturaKdvBelirle(gidenFatura, gidenFaturaDetayListesi, out var faturaKdvListesi);

            GenelVergiBilgileriDuzenle(doc, xmlnscac, xmlnscbc, kodFaturaGrupTuruKod, kodSatisTuruKod, root,
                faturaKdvListesi);

            // Burada vergi bilgileri yer almaktadır

            #region Açıklama Satırı (TaxTotal)

            //var taxTotal = doc.CreateElement("cac", "TaxTotal", xmlnscac.Value);
            //var currencyId = doc.CreateAttribute("currencyID");
            //currencyId.Value = "TRY";
            //var taxAmount = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
            //taxAmount.RemoveAllAttributes();
            //taxAmount.Attributes.Append(currencyId);
            //taxAmount.InnerText = Decimal.Round((decimal)gidenFatura.KdvTutari, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".");
            //taxTotal.AppendChild(taxAmount);
            //var taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
            //var taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
            //taxableAmount.RemoveAllAttributes();
            //currencyId = doc.CreateAttribute("currencyID");
            //currencyId.Value = "TRY";
            //taxableAmount.Attributes.Append(currencyId);
            //taxableAmount.InnerText = Decimal.Round((decimal)gidenFatura.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".");
            //taxSubTotal.AppendChild(taxableAmount);
            //var taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
            //taxAmount2.RemoveAllAttributes();
            //currencyId = doc.CreateAttribute("currencyID");
            //currencyId.Value = "TRY";
            //taxAmount2.Attributes.Append(currencyId);
            //taxAmount2.InnerText = Decimal.Round((decimal)gidenFatura.KdvTutari, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".");
            //taxSubTotal.AppendChild(taxAmount2);
            //var calculationSequenceNumeric = doc.CreateElement("cbc", "CalculationSequenceNumeric", xmlnscbc.Value);
            //calculationSequenceNumeric.InnerText = "1.0";
            //taxSubTotal.AppendChild(calculationSequenceNumeric);
            //var percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
            //if (gidenFatura.KdvHaricTutar != 0)
            //    percent.InnerText = Decimal.Round(((decimal)gidenFatura.KdvTutari * 100) / (decimal)gidenFatura.KdvHaricTutar, 0, MidpointRounding.AwayFromZero).ToString();
            //else
            //    percent.InnerText = "0";
            //taxSubTotal.AppendChild(percent);
            //var taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
            //if (gidenFatura.KdvTutari == 0)
            //{
            //    var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
            //    taxExemptionReasonCode.InnerText = "325";
            //    var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
            //    taxExemptionReason.InnerText = "13/ı Yem Teslimleri";
            //    taxCategory.AppendChild(taxExemptionReasonCode);
            //    taxCategory.AppendChild(taxExemptionReason);
            //}
            //if (kodSatisTuruKod == SatisTur.IhracKayitli.GetHashCode())
            //{
            //    var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
            //    taxExemptionReasonCode.InnerText = "701";
            //    var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
            //    taxExemptionReason.InnerText = "3065 sayılı Katma Değer Vergisi kanununun 11/1-c maddesi kapsamında ihraç edilmek şartıyla teslim edildiğinden Katma Değer Vergisi tahsil edilmemiştir.";
            //    taxCategory.AppendChild(taxExemptionReasonCode);
            //    taxCategory.AppendChild(taxExemptionReason);
            //}
            //var taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
            //var taxTypeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            //taxTypeName.InnerText = "Katma Değer Vergisi";
            //var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
            //taxTypeCode.InnerText = "0015";
            //taxScheme2.AppendChild(taxTypeName);
            //taxScheme2.AppendChild(taxTypeCode);
            //taxCategory.AppendChild(taxScheme2);
            //taxSubTotal.AppendChild(taxCategory);
            //taxTotal.AppendChild(taxSubTotal);
            //root.AppendChild(taxTotal);

            #endregion

            FaturaGenelTutarDuzenle(gidenFatura, doc, xmlnscac, xmlnscbc, root, false);
        }
        
        /// <summary>
        /// E-Arşiv Kısmında
        /// Tüzel Kişiden Gelse de Şahıs Şirketinden Gelse De
        /// Fatura Detayları XML içerisinde işlenirken aynı işlemler gerçekleştirildiğinden
        /// Bu kod bloklarının tek bir yerde tutulması
        /// Birinde yapılan güncellemenin iki tarafı da etkilemesi sağlandı
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="item">Fatura Detay Bilgisi</param>
        /// <param name="doc">XML Dokümanı Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="sayac">Sayaç Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        /// <returns>Sayaç Bilgisi</returns>
        private static int EArsivFaturaDetaylariDuzenle(GidenFaturaDTO gidenFatura, GidenFaturaDetayDTO item, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc, int sayac, XmlElement root)
        {
            var detayBilgisi = item.BosluklariKaldir();

            #region InvoiceLine

            // Ölçü Birimi, KDV Hariç Tutar, Para Birimi gibi bilgiler yer almaktadır

            #region MetaData

            var invoiceLine = doc.CreateElement("cac", "InvoiceLine", xmlnscac.Value);
            var invoiceLineId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            invoiceLineId.InnerText = sayac.ToString();
            invoiceLine.AppendChild(invoiceLineId);
            var quantity = doc.CreateAttribute("unitCode");
            quantity.Value = !string.IsNullOrEmpty(detayBilgisi.GibKisaltma) ? detayBilgisi.GibKisaltma : "KGM";
            var invoicedQuantity = doc.CreateElement("cbc", "InvoicedQuantity", xmlnscbc.Value);
            invoicedQuantity.RemoveAllAttributes();
            invoicedQuantity.Attributes.Append(quantity);
            invoicedQuantity.InnerText = Decimal
                .Round((decimal) detayBilgisi.Miktar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            invoiceLine.AppendChild(invoicedQuantity);
            var lineExtensionAmountDetail = doc.CreateElement("cbc", "LineExtensionAmount", xmlnscbc.Value);
            lineExtensionAmountDetail.RemoveAllAttributes();
            var currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            lineExtensionAmountDetail.Attributes.Append(currencyId);
            lineExtensionAmountDetail.InnerText = Decimal
                .Round((decimal) detayBilgisi.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            invoiceLine.AppendChild(lineExtensionAmountDetail);

            #endregion

            // Varsa iskonto bilgileri yer almaktadır

            #region AllowanceCharge

            var allowanceCharge = doc.CreateElement("cac", "AllowanceCharge", xmlnscac.Value);
            var chargeIndicator = doc.CreateElement("cbc", "ChargeIndicator", xmlnscbc.Value);
            chargeIndicator.InnerText = "false";
            allowanceCharge.AppendChild(chargeIndicator);
            decimal amount2 = 0;
            var toplamTutar = (decimal) detayBilgisi.KdvHaricTutar;
            var kodIskontoTuruOran = detayBilgisi.IskontoOran ?? 0;
            if (kodIskontoTuruOran > 0)
            {
                toplamTutar = Decimal.Round((decimal) detayBilgisi.Miktar * (decimal) detayBilgisi.BirimFiyat,
                    4, MidpointRounding.AwayFromZero);
                var toplamTutar2 = Decimal.Round(toplamTutar * (100 - kodIskontoTuruOran) * (decimal) 0.01,
                    4, MidpointRounding.AwayFromZero);
                amount2 = toplamTutar - toplamTutar2;
            }

            var multiplierFactorNumeric = doc.CreateElement("cbc", "MultiplierFactorNumeric", xmlnscbc.Value);
            multiplierFactorNumeric.InnerText = Decimal
                .Round(kodIskontoTuruOran * (decimal) 0.01, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            allowanceCharge.AppendChild(multiplierFactorNumeric);
            var amount = doc.CreateElement("cbc", "Amount", xmlnscbc.Value);
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            amount.RemoveAllAttributes();
            amount.Attributes.Append(currencyId);
            amount.InnerText = Decimal.Round(amount2, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            allowanceCharge.AppendChild(amount);
            var baseAmount = doc.CreateElement("cbc", "BaseAmount", xmlnscbc.Value);
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            baseAmount.RemoveAllAttributes();
            baseAmount.Attributes.Append(currencyId);
            baseAmount.InnerText = Decimal.Round(toplamTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            allowanceCharge.AppendChild(baseAmount);
            invoiceLine.AppendChild(allowanceCharge);

            #endregion

            // Vergi bilgileri yer almaktadır

            #region TaxTotal

            var taxTotal = doc.CreateElement("cac", "TaxTotal", xmlnscac.Value);
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            var taxAmount = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
            taxAmount.RemoveAllAttributes();
            taxAmount.Attributes.Append(currencyId);
            taxAmount.InnerText = Decimal
                .Round((decimal) detayBilgisi.KdvTutari, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            if (item.KonaklamaVergisi > 0 && gidenFatura.IslemTarihi >= Sabitler.KonaklamaVergiKontrolTarihi)
                taxAmount.InnerText = Decimal
                    .Round((decimal) detayBilgisi.KdvTutari + (decimal) detayBilgisi.KonaklamaVergisi, 2,
                        MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
            taxTotal.AppendChild(taxAmount);
            var taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
            var taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
            taxableAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            taxableAmount.Attributes.Append(currencyId);
            taxableAmount.InnerText = Decimal
                .Round((decimal) detayBilgisi.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            taxSubTotal.AppendChild(taxableAmount);
            var taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
            taxAmount2.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            taxAmount2.Attributes.Append(currencyId);
            taxAmount2.InnerText = Decimal
                .Round((decimal) detayBilgisi.KdvTutari, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            taxSubTotal.AppendChild(taxAmount2);
            var percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
            if (detayBilgisi.KdvOran != null)
                percent.InnerText = Decimal
                    .Round((decimal) detayBilgisi.KdvOran, 2, MidpointRounding.AwayFromZero).ToString("N1")
                    .Replace(",", ".");
            taxSubTotal.AppendChild(percent);
            var taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
            var taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
            var taxTypeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            taxTypeName.InnerText = "KDV";
            taxScheme2.AppendChild(taxTypeName);
            var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
            taxTypeCode.InnerText = "0015";
            taxScheme2.AppendChild(taxTypeCode);
            taxCategory.AppendChild(taxScheme2);
            taxSubTotal.AppendChild(taxCategory);
            taxTotal.AppendChild(taxSubTotal);
            if (item.KonaklamaVergisi > 0 && gidenFatura.IslemTarihi >= Sabitler.KonaklamaVergiKontrolTarihi)
            {
                #region TaxSubTotal (Konaklama Vergisi İçin)

                taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
                taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
                taxableAmount.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                taxableAmount.Attributes.Append(currencyId);
                taxableAmount.InnerText = Decimal
                    .Round((decimal) detayBilgisi.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                taxSubTotal.AppendChild(taxableAmount);
                taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                taxAmount2.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                taxAmount2.Attributes.Append(currencyId);
                taxAmount2.InnerText = Decimal
                    .Round((decimal) detayBilgisi.KonaklamaVergisi, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                taxSubTotal.AppendChild(taxAmount2);
                percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
                percent.InnerText = Decimal
                    .Round(2, 2, MidpointRounding.AwayFromZero).ToString("N1")
                    .Replace(",", ".");
                taxSubTotal.AppendChild(percent);
                taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
                taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                taxTypeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                taxTypeName.InnerText = "Konaklama Vergisi";
                taxScheme2.AppendChild(taxTypeName);
                taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
                taxTypeCode.InnerText = "0059";
                taxScheme2.AppendChild(taxTypeCode);
                taxCategory.AppendChild(taxScheme2);
                taxSubTotal.AppendChild(taxCategory);
                taxTotal.AppendChild(taxSubTotal);
                invoiceLine.AppendChild(taxTotal);

                #endregion
            }

            invoiceLine.AppendChild(taxTotal);

            #endregion

            // Fatura kesilen ürün detayı ve açıklama bilgileri yer almaktadır

            #region Item

            var invoiceItem = doc.CreateElement("cac", "Item", xmlnscac.Value);
            if (!string.IsNullOrEmpty(detayBilgisi.MalzemeFaturaAciklamasi))
            {
                var description = doc.CreateElement("cbc", "Description", xmlnscbc.Value);
                description.InnerText = detayBilgisi.MalzemeFaturaAciklamasi;
                invoiceItem.AppendChild(description);
            }

            var itemName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(detayBilgisi.FaturaUrunTuru))
                itemName.InnerText = detayBilgisi.FaturaUrunTuru;
            invoiceItem.AppendChild(itemName);
            invoiceLine.AppendChild(invoiceItem);

            #endregion

            // Birim fiyat bilgileri yer almaktadır

            #region Price

            var price = doc.CreateElement("cac", "Price", xmlnscac.Value);
            var priceAmount = doc.CreateElement("cbc", "PriceAmount", xmlnscbc.Value);
            priceAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            priceAmount.Attributes.Append(currencyId);
            priceAmount.InnerText = detayBilgisi.BirimFiyat != null
                ? Decimal.Round((decimal) detayBilgisi.BirimFiyat, 4, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".")
                : "0.00";
            price.AppendChild(priceAmount);
            invoiceLine.AppendChild(price);

            #endregion

            root.AppendChild(invoiceLine);

            #endregion

            sayac++;
            return sayac;
        }
        
        /// <summary>
        /// E-Fatura kısmında Banka Bilgilerinin girildiği
        /// Ve XML içerisine yerleştirildiği metottur.
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="doc">XML Dokümanı Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        private static void EFaturaBankaBilgileriDuzenle(GidenFaturaDTO gidenFatura, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc, XmlElement root)
        {
            if (string.IsNullOrEmpty(gidenFatura.BankaAd)) return;
            // Eğer Banka bilgileri mevcutsa burada banka bilgileri yer almaktadır

            #region PaymentMeans

            var paymentMeans = doc.CreateElement("cac", "PaymentMeans", xmlnscac.Value);
            var paymentMeansCode = doc.CreateElement("cbc", "PaymentMeansCode", xmlnscbc.Value);
            //paymentMeansCode.InnerText = "1";

            // Burada 1 verildiği zaman Sözleşme Kapsamında yazıyor metinde
            // ZZZ ise Diğer anlamında geliyor
            paymentMeansCode.InnerText = "ZZZ";
            paymentMeans.AppendChild(paymentMeansCode);
            var payeeFinancialAccount = doc.CreateElement("cac", "PayeeFinancialAccount", xmlnscac.Value);
            var payeeFinancialAccountId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.IbanNo))
                payeeFinancialAccountId.InnerText = gidenFatura.IbanNo;
            payeeFinancialAccount.AppendChild(payeeFinancialAccountId);
            var currencyCode = doc.CreateElement("cbc", "CurrencyCode", xmlnscbc.Value);
            currencyCode.InnerText = "TRY";
            payeeFinancialAccount.AppendChild(currencyCode);
            var paymentNote = doc.CreateElement("cbc", "PaymentNote", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.BankaAd) && !string.IsNullOrEmpty(gidenFatura.BankaSube))
                paymentNote.InnerText = gidenFatura.BankaAd + " - " + gidenFatura.BankaSube;
            payeeFinancialAccount.AppendChild(paymentNote);
            paymentMeans.AppendChild(payeeFinancialAccount);
            root.AppendChild(paymentMeans);

            #endregion
        }
        
        /// <summary>
        /// E-Fatura olsun E-Arşiv olsun
        /// Fatura detaylarında KDV ile ilgili kısımların belirlenmesi için
        /// Hazırlanan metottur 
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="gidenFaturaDetayListesi">Giden Fatura Detay Listesi Bilgisi</param>
        /// <param name="faturaKdvListesi">Fatura Detay KDV Listesi Bilgisi</param>
        private static void FaturaKdvBelirle(GidenFaturaDTO gidenFatura, List<GidenFaturaDetayDTO> gidenFaturaDetayListesi, out List<FaturaKdvDTO> faturaKdvListesi)
        {
            #region KDVlerin Belirlenmesi

            faturaKdvListesi = new List<FaturaKdvDTO>();
            foreach (var item in gidenFaturaDetayListesi)
            {
                var kodKdvTuruOran = item.KdvOran ?? 0;
                if (faturaKdvListesi.All(j => j.KdvOran != kodKdvTuruOran))
                {
                    var faturaKdv = new FaturaKdvDTO
                    {
                        KdvOran = kodKdvTuruOran,
                        KdvTutari = item.KdvTutari,
                        KdvHaricTutar = item.KdvHaricTutar
                    };
                    faturaKdvListesi.Add(faturaKdv);
                }
                else
                {
                    var index = faturaKdvListesi.FindIndex(j => j.KdvOran == kodKdvTuruOran);
                    faturaKdvListesi[index].KdvTutari += item.KdvTutari;
                    faturaKdvListesi[index].KdvHaricTutar += item.KdvHaricTutar;
                }

                if (faturaKdvListesi.Count > 0)
                    faturaKdvListesi = faturaKdvListesi.OrderBy(j => j.KdvOran).ToList();
            }

            if (!(gidenFaturaDetayListesi.Sum(j => j.KonaklamaVergisi) > 0) || !(gidenFatura.IslemTarihi?.Year >= 2023)) return;
            {
                var faturaKdv = new FaturaKdvDTO
                {
                    KdvOran = 2,
                    KdvTutari = gidenFaturaDetayListesi.Sum(j => j.KonaklamaVergisi),
                    KdvHaricTutar = gidenFaturaDetayListesi.Sum(j => j.KdvHaricTutar)
                };
                faturaKdvListesi.Add(faturaKdv);
            }

            #endregion
        }
        
        /// <summary>
        /// E-Fatura olsun E-Arşiv Olsun
        /// Fatura detaylarından belirlene KDV listesi üzerinden
        /// Ana vergi bilgilerinin XML içerisinde oluşturulması için
        /// Hazırlanmış metottur
        /// </summary>
        /// <param name="doc">XML Dokümanı Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="kodFaturaGrupTuruKod">Fatura Grup Türü Bilgisi</param>
        /// <param name="kodSatisTuruKod">Fatura Satış Türü Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        /// <param name="faturaKdvListesi">Fatura Detay KDV Listesi Bilgisi</param>
        private static void GenelVergiBilgileriDuzenle(XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc, int kodFaturaGrupTuruKod, int kodSatisTuruKod, XmlElement root, List<FaturaKdvDTO> faturaKdvListesi)
        {
            #region TaxTotal

            const int sayac = 1;
            foreach (var item in faturaKdvListesi)
            {
                // Burada vergi bilgileri yer almaktadır

                #region TaxTotal

                var taxTotal = doc.CreateElement("cac", "TaxTotal", xmlnscac.Value);
                var currencyIdGeneral = doc.CreateAttribute("currencyID");
                currencyIdGeneral.Value = "TRY";
                var taxAmount = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                taxAmount.RemoveAllAttributes();
                taxAmount.Attributes.Append(currencyIdGeneral);
                taxAmount.InnerText = Decimal.Round((decimal) item.KdvTutari, 2, MidpointRounding.AwayFromZero)
                    .ToString().Replace(",", ".");
                taxTotal.AppendChild(taxAmount);
                var taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
                var taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
                taxableAmount.RemoveAllAttributes();
                currencyIdGeneral = doc.CreateAttribute("currencyID");
                currencyIdGeneral.Value = "TRY";
                taxableAmount.Attributes.Append(currencyIdGeneral);
                taxableAmount.InnerText = Decimal
                    .Round((decimal) item.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                taxSubTotal.AppendChild(taxableAmount);
                var taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                taxAmount2.RemoveAllAttributes();
                currencyIdGeneral = doc.CreateAttribute("currencyID");
                currencyIdGeneral.Value = "TRY";
                taxAmount2.Attributes.Append(currencyIdGeneral);
                taxAmount2.InnerText = Decimal.Round((decimal) item.KdvTutari, 2, MidpointRounding.AwayFromZero)
                    .ToString().Replace(",", ".");
                taxSubTotal.AppendChild(taxAmount2);
                var calculationSequenceNumeric =
                    doc.CreateElement("cbc", "CalculationSequenceNumeric", xmlnscbc.Value);
                calculationSequenceNumeric.InnerText = sayac + ".0";
                taxSubTotal.AppendChild(calculationSequenceNumeric);
                var percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
                percent.InnerText = Decimal.Round((decimal) item.KdvOran, 0, MidpointRounding.AwayFromZero)
                    .ToString();
                taxSubTotal.AppendChild(percent);
                var taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
                if (item.KdvTutari == 0)
                {
                    if (kodFaturaGrupTuruKod == FaturaMakbuzGrupTur.Kuspe.GetHashCode())
                    {
                        var taxExemptionReasonCode =
                            doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                        taxExemptionReasonCode.InnerText = "325";
                        var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                        taxExemptionReason.InnerText = "13/ı Yem Teslimleri";
                        taxCategory.AppendChild(taxExemptionReasonCode);
                        taxCategory.AppendChild(taxExemptionReason);
                    }
                    else
                    {
                        var taxExemptionReasonCode =
                            doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                        taxExemptionReasonCode.InnerText = "350";
                        var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                        taxExemptionReason.InnerText = "Diğerleri";
                        taxCategory.AppendChild(taxExemptionReasonCode);
                        taxCategory.AppendChild(taxExemptionReason);
                    }
                }

                if (kodSatisTuruKod == SatisTur.IhracKayitli.GetHashCode())
                {
                    var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                    taxExemptionReasonCode.InnerText = "701";
                    var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                    taxExemptionReason.InnerText =
                        "3065 sayılı Katma Değer Vergisi kanununun 11/1-c maddesi kapsamında ihraç edilmek şartıyla teslim edildiğinden Katma Değer Vergisi tahsil edilmemiştir.";
                    taxCategory.AppendChild(taxExemptionReasonCode);
                    taxCategory.AppendChild(taxExemptionReason);
                }

                var taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                var taxTypeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                taxTypeName.InnerText = item.KdvOran != 2 ? "Katma Değer Vergisi" : "Konaklama Vergisi";
                var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
                taxTypeCode.InnerText = item.KdvOran != 2 ? "0015" : "0059";
                taxScheme2.AppendChild(taxTypeName);
                taxScheme2.AppendChild(taxTypeCode);
                taxCategory.AppendChild(taxScheme2);
                taxSubTotal.AppendChild(taxCategory);
                taxTotal.AppendChild(taxSubTotal);
                root.AppendChild(taxTotal);

                #endregion
            }

            #endregion
        }

        /// <summary>
        /// E-Fatura olsun E-Arşiv olsun
        /// Faturadaki genel KDV Tutarı, KDV Hariç Tutar, Toplam Tutar bilgilerinin
        /// Hazırlanması ve XML içerisine yerleştirilmesi için
        /// Geliştirilmiş metottur
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="doc">XML Dokümanı Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        /// <param name="isAllowanceHere">Allowance Elemanı Olacak Mı Bilgisi (E-Fatura için true, E-Arşiv için false olacak)</param>
        private static void FaturaGenelTutarDuzenle(GidenFaturaDTO gidenFatura, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc, XmlElement root, bool isAllowanceHere)
        {
            // KDV, KDV Hariç Tutar ve Toplam Tutar bilgileri yer almaktadır

            #region LegalMonetaryTotal

            var legalMonetaryTotal = doc.CreateElement("cac", "LegalMonetaryTotal", xmlnscac.Value);
            var lineExtensionAmount = doc.CreateElement("cbc", "LineExtensionAmount", xmlnscbc.Value);
            lineExtensionAmount.RemoveAllAttributes();
            var currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            lineExtensionAmount.Attributes.Append(currencyId);
            lineExtensionAmount.InnerText = Decimal
                .Round((decimal) gidenFatura.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            legalMonetaryTotal.AppendChild(lineExtensionAmount);
            var taxExclusiveAmount = doc.CreateElement("cbc", "TaxExclusiveAmount", xmlnscbc.Value);
            taxExclusiveAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            taxExclusiveAmount.Attributes.Append(currencyId);
            taxExclusiveAmount.InnerText = Decimal
                .Round((decimal) gidenFatura.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            legalMonetaryTotal.AppendChild(taxExclusiveAmount);
            var taxInclusiveAmount = doc.CreateElement("cbc", "TaxInclusiveAmount", xmlnscbc.Value);
            taxInclusiveAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            taxInclusiveAmount.Attributes.Append(currencyId);
            taxInclusiveAmount.InnerText = Decimal
                .Round((decimal) gidenFatura.FaturaTutari, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            legalMonetaryTotal.AppendChild(taxInclusiveAmount);
            if (isAllowanceHere)
            {
                var allowanceTotalAmount = doc.CreateElement("cbc", "AllowanceTotalAmount", xmlnscbc.Value);
                allowanceTotalAmount.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                allowanceTotalAmount.Attributes.Append(currencyId);
                allowanceTotalAmount.InnerText = "0.00";
                legalMonetaryTotal.AppendChild(allowanceTotalAmount);
            }

            var payableAmount = doc.CreateElement("cbc", "PayableAmount", xmlnscbc.Value);
            payableAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            payableAmount.Attributes.Append(currencyId);
            payableAmount.InnerText = Decimal
                .Round((decimal) gidenFatura.FaturaTutari, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            legalMonetaryTotal.AppendChild(payableAmount);
            root.AppendChild(legalMonetaryTotal);

            #endregion
        }

        /// <summary>
        /// E-Fatura olsun E-Arşiv olsun E-Müstahsil olsun
        /// Hazırlanan XML dosyasının kaydedilip
        /// Dosya adının döndürülmesi için
        /// Hazırlanan metottur. 
        /// </summary>
        /// <param name="id">Nesnenin Id Bilgisi</param>
        /// <param name="aktarilacakKlasorAdi">Aktarılacak Klasör Adı Bilgisi</param>
        /// <param name="doc">XML Dokümanı Bilgisi</param>
        /// <returns>Kaydedilen dosyanın tüm adı bilgisi</returns>
        private static string XMLDosyasiniKaydet(string id, string aktarilacakKlasorAdi, XmlDocument doc)
        {
            #region Dosya Kaydı

            var fileName = aktarilacakKlasorAdi + "/" + id + ".xml";
            doc.Save(fileName);

            #endregion

            return fileName;
        }
        
        /// <summary>
        /// E-Müstahsil, E-Fatura ve E-Arşiv tarafında
        /// Ana elemanlar arasında Para Birimi ve Kayıt Sayısı Bilgilerinin
        /// Düzenlenmesi için gerekli işlemlerin gerçekleştirildiği metottur.
        /// </summary>
        /// <param name="kayitSayisi">Kayıt Sayısı Bilgisi</param>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        /// <param name="doc">XML Doküman Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        private static void ParaBirimiVeKayitSayisiDuzenle(int kayitSayisi, XmlElement root, XmlDocument doc, XmlAttribute xmlnscbc)
        {
            var documentCurrencyCode = doc.CreateElement("cbc", "DocumentCurrencyCode", xmlnscbc.Value);
            documentCurrencyCode.InnerText = "TRY";
            root.AppendChild(documentCurrencyCode);

            var lineCountNumeric = doc.CreateElement("cbc", "LineCountNumeric", xmlnscbc.Value);
            lineCountNumeric.InnerText = kayitSayisi.ToString();
            root.AppendChild(lineCountNumeric);
        }
        
        /// <summary>
        /// E-Fatura ve E-Arşiv tarafında
        /// Faturanın imza bilgileri aynı olduğu için
        /// Tek bir metot içerisinde ele alınması için
        /// Gerekli işlemlerin gerçekleştirildiği metottur.
        /// </summary>
        /// <param name="root">XML Ana Eleman Bilgisi</param>
        /// <param name="doc">XML Dokümanı Bilgisi</param>
        /// <param name="xmlnscac">XML Attribute Bilgisi</param>
        /// <param name="xmlnscbc">XML Attribute Bilgisi</param>
        private static void FaturaMakbuzImzaBilgisiDuzenle(XmlElement root, XmlDocument doc, XmlAttribute xmlnscac, XmlAttribute xmlnscbc)
        {
            // Faturaya ilişkin imza, adres vb. bilgiler yer almaktadır

            #region Signature

            var signature = doc.CreateElement("cac", "Signature", xmlnscac.Value);
            var signatureIdAttr = doc.CreateAttribute("schemeID");
            signatureIdAttr.Value = "VKN_TCKN";
            var signatureId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            signatureId.Attributes.Append(signatureIdAttr);
            signatureId.InnerText = "3250566851";
            signature.AppendChild(signatureId);
            var signatoryParty = doc.CreateElement("cac", "SignatoryParty", xmlnscac.Value);
            var partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
            var signatureIdAttr2 = doc.CreateAttribute("schemeID");
            signatureIdAttr2.Value = "VKN";
            var signaturePartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            signaturePartyId.Attributes.Append(signatureIdAttr2);
            signaturePartyId.InnerText = "3250566851";
            partyIdentification.AppendChild(signaturePartyId);
            signatoryParty.AppendChild(partyIdentification);

            #region Postal Address

            var postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
            var streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
            streetName.InnerText = "Mithatpaşa Caddesi";
            postalAddress.AppendChild(streetName);
            var buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
            buildingNumber.InnerText = "14";
            postalAddress.AppendChild(buildingNumber);
            var citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
            citySubdivisionName.InnerText = "Çankaya";
            postalAddress.AppendChild(citySubdivisionName);
            var cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
            cityName.InnerText = "Ankara";
            postalAddress.AppendChild(cityName);
            var postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
            postalZone.InnerText = "06100";
            postalAddress.AppendChild(postalZone);
            var country = doc.CreateElement("cac", "Country", xmlnscac.Value);
            var countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            countryName.InnerText = "Türkiye";
            country.AppendChild(countryName);
            postalAddress.AppendChild(country);
            signatoryParty.AppendChild(postalAddress);
            signature.AppendChild(signatoryParty);

            #endregion

            var digitalSignatureAttachment = doc.CreateElement("cac", "DigitalSignatureAttachment", xmlnscac.Value);
            var externalReference = doc.CreateElement("cac", "ExternalReference", xmlnscac.Value);
            var uri = doc.CreateElement("cbc", "URI", xmlnscbc.Value);
            uri.InnerText = "#Signature";
            externalReference.AppendChild(uri);
            digitalSignatureAttachment.AppendChild(externalReference);
            signature.AppendChild(digitalSignatureAttachment);

            root.AppendChild(signature);

            #endregion
        }
    }
}