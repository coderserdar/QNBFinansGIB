using QNBFinansGIB.DTO;
using System;
using System.Collections.Generic;
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
    public static class YardimciSiniflar
    {
        /// <summary>
        /// Giden Fatura Bilgisi Üzerinden E-Fatura Sistemine Göndermek İçin
        /// XML Dosyası Oluşturmaya yarayan metottur
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="gidenFaturaDetayListesi">Giden Fatura Detay Listesi</param>
        /// <param name="aktarilacakKlasorAdi">Oluşturulan Dosyanın Aktarılacağı Klasör</param>
        /// <returns>Kaydedilen Dosyanın Bilgisayardaki Adresi</returns>
        public static string EFaturaXMLOlustur(GidenFaturaDTO gidenFatura,
            List<GidenFaturaDetayDTO> gidenFaturaDetayListesi, string aktarilacakKlasorAdi)
        {
            var doc = new XmlDocument();
            var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            var root = doc.DocumentElement;
            doc.InsertBefore(declaration, root);
            doc.AppendChild(
                doc.CreateProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"general.xslt\""));

            root = doc.CreateElement("Invoice");

            gidenFatura = gidenFatura.BosluklariKaldir();

            var kodSatisTuruKod = 0;
            if (gidenFatura.SatisTuruKod != null)
                kodSatisTuruKod = gidenFatura.SatisTuruKod ?? 0;

            var kodFaturaTuruKod = 0;
            if (gidenFatura.FaturaTuruKod != null)
                kodFaturaTuruKod = gidenFatura.FaturaTuruKod ?? 0;

            var kodFaturaGrupTuruKod = 0;
            if (gidenFatura.FaturaGrupTuruKod != null)
                kodFaturaGrupTuruKod = gidenFatura.FaturaGrupTuruKod ?? 0;

            #region Standart ve Faturaya Bağlı Bilgiler

            // Faturanın türü, düzenleme tarihi, UBL Versiyon Numarası, GİB Numarası gibi bilgiler
            // yer almaktadır

            #region Standart Bilgiler

            root.RemoveAllAttributes();
            var locationAttribute = "xsi:schemaLocation";
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
            var xmlnscac = doc.CreateAttribute("xmlns:cac");
            xmlnscac.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            var xmlnscbc = doc.CreateAttribute("xmlns:cbc");
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
            customizationId.InnerText = "TR1.2";
            root.AppendChild(customizationId);

            var profileId = doc.CreateElement("cbc", "ProfileID", xmlnscbc.Value);
            profileId.InnerText = "TEMELFATURA";
            root.AppendChild(profileId);

            var id = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.GibNumarasi))
                id.InnerText = gidenFatura.GibNumarasi;
            else
                id.InnerText = "ABC" + gidenFatura.DuzenlemeTarihi?.Year +
                               DateTime.UtcNow.Ticks.ToString().Substring(0, 9);
            root.AppendChild(id);

            var copyIndicator = doc.CreateElement("cbc", "CopyIndicator", xmlnscbc.Value);
            copyIndicator.InnerText = "false";
            root.AppendChild(copyIndicator);

            var gidenFaturaId = doc.CreateElement("cbc", "UUID", xmlnscbc.Value);
            gidenFaturaId.InnerText = gidenFatura.GidenFaturaId.Length == 36
                ? gidenFatura.GidenFaturaId.ToUpper()
                : Guid.NewGuid().ToString().ToUpper();
            root.AppendChild(gidenFaturaId);

            var issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
            issueDate.InnerText = gidenFatura.DuzenlemeTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
            root.AppendChild(issueDate);

            var issueTime = doc.CreateElement("cbc", "IssueTime", xmlnscbc.Value);
            issueTime.InnerText = gidenFatura.DuzenlemeTarihi?.ToString("HH:mm:ss") ?? string.Empty;
            root.AppendChild(issueTime);

            var invoiceTypeCode = doc.CreateElement("cbc", "InvoiceTypeCode", xmlnscbc.Value);
            invoiceTypeCode.InnerText = "SATIS";
            if (gidenFatura.KdvTutari == 0 || gidenFaturaDetayListesi.Any(j => j.KdvTutari == 0))
                invoiceTypeCode.InnerText = "ISTISNA";
            if (kodSatisTuruKod == SatisTur.IhracKayitli.GetHashCode())
                invoiceTypeCode.InnerText = "IHRACKAYITLI";
            if (kodFaturaTuruKod == FaturaTur.Iade.GetHashCode())
                invoiceTypeCode.InnerText = "IADE";
            root.AppendChild(invoiceTypeCode);

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

            if (gidenFaturaDetayListesi2.Count > 0)
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
                        irsaliyeNote.InnerText += " İrsaliye Tarihi: " + item.SevkIrsaliyeTarihi?.ToShortDateString();
                    if (item.YuklemeFormuNo != null)
                        irsaliyeNote.InnerText += " Yükleme Formu No: " + item.YuklemeFormuNo;
                    if (item.Tonaj != null)
                        irsaliyeNote.InnerText += " Tonaj: " + item.Tonaj;
                    if ((item.FaturaUrunTuru.Contains("Şartname") || item.FaturaUrunTuru.Contains("Kira")) &&
                        kodFaturaGrupTuruKod == FaturaMakbuzGrupTur.Diger.GetHashCode() &&
                        !string.IsNullOrEmpty(gidenFatura.Aciklama))
                        irsaliyeNote.InnerText += " Açıklama: " + gidenFatura.Aciklama;
                    if (!string.IsNullOrEmpty(irsaliyeNote.InnerText))
                        root.AppendChild(irsaliyeNote);
                }
            }

            #endregion

            var documentCurrencyCode = doc.CreateElement("cbc", "DocumentCurrencyCode", xmlnscbc.Value);
            documentCurrencyCode.InnerText = "TRY";
            root.AppendChild(documentCurrencyCode);

            var lineCountNumeric = doc.CreateElement("cbc", "LineCountNumeric", xmlnscbc.Value);
            lineCountNumeric.InnerText = gidenFaturaDetayListesi.Count.ToString();
            root.AppendChild(lineCountNumeric);

            #region AdditionalDocumentReference

            var additionalDocumentReference = doc.CreateElement("cac", "AdditionalDocumentReference", xmlnscac.Value);
            var additionalDocumentReferenceId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            additionalDocumentReferenceId.InnerText = gidenFatura.GidenFaturaId;
            additionalDocumentReference.AppendChild(additionalDocumentReferenceId);
            issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
            issueDate.InnerText = gidenFatura.DuzenlemeTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
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

            #endregion

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
            partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
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

            postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
            var postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            postalAddressId.InnerText = "3250566851";
            postalAddress.AppendChild(postalAddressId);
            streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
            streetName.InnerText = "Mithatpaşa Caddesi";
            postalAddress.AppendChild(streetName);
            buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
            buildingNumber.InnerText = "14";
            postalAddress.AppendChild(buildingNumber);
            citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
            citySubdivisionName.InnerText = "Çankaya";
            postalAddress.AppendChild(citySubdivisionName);
            cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
            cityName.InnerText = "Ankara";
            postalAddress.AppendChild(cityName);
            postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
            postalZone.InnerText = "06100";
            postalAddress.AppendChild(postalZone);
            country = doc.CreateElement("cac", "Country", xmlnscac.Value);
            countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
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

            // Bu kısımda fatura kesilen firma bilgileri yer almaktadır

            #region AccountingCustomerParty

            var accountingCustomerParty = doc.CreateElement("cac", "AccountingCustomerParty", xmlnscac.Value);
            party = doc.CreateElement("cac", "Party", xmlnscac.Value);
            webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
            party.AppendChild(webSiteUri);
            var accountingCustomerPartyIdAttr = doc.CreateAttribute("schemeID");
            accountingCustomerPartyIdAttr.Value = "TCKN";
            if (!string.IsNullOrEmpty(gidenFatura.VergiNo) && gidenFatura.VergiNo.Length == 10)
                accountingCustomerPartyIdAttr.Value = "VKN";
            partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
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

            postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
            postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            postalAddressId.InnerText = gidenFatura.VergiNo;
            postalAddress.AppendChild(postalAddressId);
            var room = doc.CreateElement("cbc", "Room", xmlnscbc.Value);
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
            electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.EPostaAdresi))
                electronicMail.InnerText = gidenFatura.EPostaAdresi;
            contact.AppendChild(electronicMail);
            party.AppendChild(contact);

            if (!string.IsNullOrEmpty(gidenFatura.VergiNo))
            {
                if (gidenFatura.VergiNo.Length == 11)
                {
                    var liste = gidenFatura.TuzelKisiAd.Split(' ').ToList();
                    if (liste.Count > 1)
                    {
                        var person = doc.CreateElement("cac", "Person", xmlnscac.Value);
                        var firstName = doc.CreateElement("cbc", "FirstName", xmlnscbc.Value);
                        firstName.InnerText = liste[0];
                        person.AppendChild(firstName);
                        var surname = gidenFatura.TuzelKisiAd.Substring(liste[0].Length + 1);
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

            if (!string.IsNullOrEmpty(gidenFatura.VergiNo))
            {
                if (gidenFatura.VergiNo.Length == 10)
                {
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
                    electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(gidenFatura.EPostaAdresi))
                        electronicMail.InnerText = gidenFatura.EPostaAdresi;
                    contact.AppendChild(electronicMail);
                    party.AppendChild(contact);

                    if (gidenFatura.VergiNo.Length == 11)
                    {
                        var liste = gidenFatura.TuzelKisiAd.Split(' ').ToList();
                        if (liste.Count > 1)
                        {
                            var person = doc.CreateElement("cac", "Person", xmlnscac.Value);
                            var firstName = doc.CreateElement("cbc", "FirstName", xmlnscbc.Value);
                            firstName.InnerText = liste[0];
                            person.AppendChild(firstName);
                            var surname = gidenFatura.TuzelKisiAd.Substring(liste[0].Length + 1);
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
            }

            if (!string.IsNullOrEmpty(gidenFatura.BankaAd))
            {
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

            #region KDVlerin Belirlenmesi

            var faturaKdvListesi = new List<FaturaKdvDTO>();
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

            #endregion

            #region TaxTotal

            var sayac = 1;
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
                taxableAmount.InnerText = Decimal.Round((decimal) item.KdvHaricTutar, 2, MidpointRounding.AwayFromZero)
                    .ToString().Replace(",", ".");
                taxSubTotal.AppendChild(taxableAmount);
                var taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                taxAmount2.RemoveAllAttributes();
                currencyIdGeneral = doc.CreateAttribute("currencyID");
                currencyIdGeneral.Value = "TRY";
                taxAmount2.Attributes.Append(currencyIdGeneral);
                taxAmount2.InnerText = Decimal.Round((decimal) item.KdvTutari, 2, MidpointRounding.AwayFromZero)
                    .ToString().Replace(",", ".");
                taxSubTotal.AppendChild(taxAmount2);
                var calculationSequenceNumeric = doc.CreateElement("cbc", "CalculationSequenceNumeric", xmlnscbc.Value);
                calculationSequenceNumeric.InnerText = sayac + ".0";
                taxSubTotal.AppendChild(calculationSequenceNumeric);
                var percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
                percent.InnerText = Decimal.Round((decimal) item.KdvOran, 0, MidpointRounding.AwayFromZero).ToString();
                taxSubTotal.AppendChild(percent);
                var taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
                if (item.KdvTutari == 0)
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
                var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
                taxTypeCode.InnerText = "0015";
                taxScheme2.AppendChild(taxTypeCode);
                taxCategory.AppendChild(taxScheme2);
                taxSubTotal.AppendChild(taxCategory);
                taxTotal.AppendChild(taxSubTotal);
                root.AppendChild(taxTotal);

                #endregion

                sayac++;
            }

            #endregion

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

            sayac = 1;
            // Her bir fatura edtayı için hazırlanan bölümdür
            foreach (var item in gidenFaturaDetayListesi)
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
                currencyId = doc.CreateAttribute("currencyID");
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
            }

            #endregion

            doc.AppendChild(root);

            #region Dosya Kaydı

            var fileName = aktarilacakKlasorAdi + "/" + gidenFatura.GidenFaturaId + ".xml";
            doc.Save(fileName);

            #endregion

            return fileName;
        }

        /// <summary>
        /// Giden Fatura Bilgisi Üzerinden E-Arşiv Sistemine Göndermek İçin
        /// XML Dosyası Oluşturmaya yarayan metottur
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="gidenFaturaDetayListesi">Giden Fatura Detay Listesi</param>
        /// <param name="aktarilacakKlasorAdi">Oluşturulan Dosyanın Aktarılacağı Klasör</param>
        /// <param name="mustahsilMakbuzuMu">Müstahsil Makbuzu Mu Bilgisi</param>
        /// <returns>Kaydedilen Dosyanın Bilgisayardaki Adresi</returns>
        public static string EArsivXMLOlustur(GidenFaturaDTO gidenFatura,
            List<GidenFaturaDetayDTO> gidenFaturaDetayListesi, string aktarilacakKlasorAdi, bool mustahsilMakbuzuMu)
        {
            if (!mustahsilMakbuzuMu)
            {
                #region Şahıs Şirketi veya Personel Kaydından Gelecekse

                var doc = new XmlDocument();
                var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                var root = doc.DocumentElement;
                doc.InsertBefore(declaration, root);
                //doc.AppendChild(doc.CreateProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"general.xslt\""));

                root = doc.CreateElement("Invoice");

                gidenFatura = gidenFatura.BosluklariKaldir();

                var kodSatisTuruKod = 0;
                if (gidenFatura.SatisTuruKod != null)
                    kodSatisTuruKod = gidenFatura.SatisTuruKod ?? 0;

                var kodFaturaTuruKod = 0;
                if (gidenFatura.FaturaTuruKod != null)
                    kodFaturaTuruKod = gidenFatura.FaturaTuruKod ?? 0;

                var kodFaturaGrupTuruKod = 0;
                if (gidenFatura.FaturaGrupTuruKod != null)
                    kodFaturaGrupTuruKod = gidenFatura.FaturaGrupTuruKod ?? 0;

                #region Standart ve Faturaya Bağlı Bilgiler

                // Faturanın türü, düzenleme tarihi, UBL Versiyon Numarası, GİB Numarası gibi bilgiler
                // yer almaktadır

                #region Standart Bilgiler

                root.RemoveAllAttributes();

                var locationAttribute = "xsi:schemaLocation";
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
                var xmlnscac = doc.CreateAttribute("xmlns:cac");
                xmlnscac.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
                var xmlnscbc = doc.CreateAttribute("xmlns:cbc");
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

                var versionId = doc.CreateElement("cbc", "UBLVersionID", xmlnscbc.Value);
                versionId.InnerText = "2.1";
                root.AppendChild(versionId);

                var customizationId = doc.CreateElement("cbc", "CustomizationID", xmlnscbc.Value);
                customizationId.InnerText = "TR1.2";
                root.AppendChild(customizationId);

                var profileId = doc.CreateElement("cbc", "ProfileID", xmlnscbc.Value);
                profileId.InnerText = "EARSIVFATURA";
                root.AppendChild(profileId);

                var id = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.GibNumarasi))
                    id.InnerText = gidenFatura.GibNumarasi;
                else
                    id.InnerText = "ABC" + gidenFatura.DuzenlemeTarihi?.Year +
                                   DateTime.UtcNow.Ticks.ToString().Substring(0, 9);
                root.AppendChild(id);

                var copyIndicator = doc.CreateElement("cbc", "CopyIndicator", xmlnscbc.Value);
                copyIndicator.InnerText = "false";
                root.AppendChild(copyIndicator);

                var gidenFaturaId = doc.CreateElement("cbc", "UUID", xmlnscbc.Value);
                gidenFaturaId.InnerText = gidenFatura.GidenFaturaId.Length == 36
                    ? gidenFatura.GidenFaturaId.ToUpper()
                    : Guid.NewGuid().ToString().ToUpper();
                root.AppendChild(gidenFaturaId);

                var issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
                issueDate.InnerText = gidenFatura.DuzenlemeTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
                root.AppendChild(issueDate);

                var issueTime = doc.CreateElement("cbc", "IssueTime", xmlnscbc.Value);
                issueTime.InnerText = gidenFatura.DuzenlemeTarihi?.ToString("HH:mm:ss") ?? string.Empty;
                root.AppendChild(issueTime);

                var invoiceTypeCode = doc.CreateElement("cbc", "InvoiceTypeCode", xmlnscbc.Value);
                invoiceTypeCode.InnerText = "SATIS";
                if (gidenFatura.KdvTutari == 0 || gidenFaturaDetayListesi.Any(j => j.KdvTutari == 0))
                    invoiceTypeCode.InnerText = "ISTISNA";
                if (kodSatisTuruKod == SatisTur.IhracKayitli.GetHashCode())
                    invoiceTypeCode.InnerText = "IHRACKAYITLI";
                if (kodFaturaTuruKod == FaturaTur.Iade.GetHashCode())
                    invoiceTypeCode.InnerText = "IADE";
                root.AppendChild(invoiceTypeCode);

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

                if (gidenFaturaDetayListesi2.Count > 0)
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
                        if ((item.FaturaUrunTuru.Contains("Şartname") || item.FaturaUrunTuru.Contains("Kira")) &&
                            kodFaturaGrupTuruKod == FaturaMakbuzGrupTur.Diger.GetHashCode() &&
                            !string.IsNullOrEmpty(gidenFatura.Aciklama))
                            irsaliyeNote.InnerText += " Açıklama: " + gidenFatura.Aciklama;
                        if (!string.IsNullOrEmpty(irsaliyeNote.InnerText))
                            root.AppendChild(irsaliyeNote);
                    }
                }

                #endregion

                var sendType = doc.CreateElement("cbc", "Note", xmlnscbc.Value);
                sendType.InnerText = "Gönderim Şekli: ELEKTRONIK";
                root.AppendChild(sendType);

                var documentCurrencyCode = doc.CreateElement("cbc", "DocumentCurrencyCode", xmlnscbc.Value);
                documentCurrencyCode.InnerText = "TRY";
                root.AppendChild(documentCurrencyCode);

                var lineCountNumeric = doc.CreateElement("cbc", "LineCountNumeric", xmlnscbc.Value);
                lineCountNumeric.InnerText = gidenFaturaDetayListesi.Count.ToString();
                root.AppendChild(lineCountNumeric);

                #endregion

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
                partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
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

                postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
                var postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                postalAddressId.InnerText = "3250566851";
                postalAddress.AppendChild(postalAddressId);
                streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
                streetName.InnerText = "Mithatpaşa Caddesi";
                postalAddress.AppendChild(streetName);
                buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
                buildingNumber.InnerText = "14";
                postalAddress.AppendChild(buildingNumber);
                citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
                citySubdivisionName.InnerText = "Çankaya";
                postalAddress.AppendChild(citySubdivisionName);
                cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
                cityName.InnerText = "Ankara";
                postalAddress.AppendChild(cityName);
                postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
                postalZone.InnerText = "06100";
                postalAddress.AppendChild(postalZone);
                country = doc.CreateElement("cac", "Country", xmlnscac.Value);
                countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
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

                // Bu kısımda fatura kesilen firma bilgileri yer almaktadır

                #region AccountingCustomerParty

                var accountingCustomerParty = doc.CreateElement("cac", "AccountingCustomerParty", xmlnscac.Value);
                party = doc.CreateElement("cac", "Party", xmlnscac.Value);
                webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
                party.AppendChild(webSiteUri);
                var accountingCustomerPartyIdAttr = doc.CreateAttribute("schemeID");
                accountingCustomerPartyIdAttr.Value = "TCKN";
                partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
                var accountingCustomerPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                accountingCustomerPartyPartyId.Attributes.Append(accountingCustomerPartyIdAttr);
                accountingCustomerPartyPartyId.InnerText = gidenFatura.GercekKisiTcKimlikNo;
                partyIdentification.AppendChild(accountingCustomerPartyPartyId);
                party.AppendChild(partyIdentification);

                #region Postal Address

                postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
                postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                postalAddressId.InnerText = gidenFatura.GercekKisiTcKimlikNo;
                postalAddress.AppendChild(postalAddressId);
                var room = doc.CreateElement("cbc", "Room", xmlnscbc.Value);
                postalAddress.AppendChild(room);
                streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.IkametgahAdresi))
                    streetName.InnerText = gidenFatura.IkametgahAdresi;
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
                //if (!string.IsNullOrEmpty(gidenFatura.VergiDairesi))
                //    taxSchemeName.InnerText = gidenFatura.VergiDairesi;
                taxScheme.AppendChild(taxSchemeName);
                partyTaxScheme.AppendChild(taxScheme);
                party.AppendChild(partyTaxScheme);

                contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
                telephone = doc.CreateElement("cbc", "Telephone", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.CepTelefonNo))
                    telephone.InnerText = gidenFatura.CepTelefonNo;
                contact.AppendChild(telephone);
                telefax = doc.CreateElement("cbc", "Telefax", xmlnscbc.Value);
                //if (!string.IsNullOrEmpty(gidenFatura.FaksNo))
                //    telefax.InnerText = gidenFatura.FaksNo;
                contact.AppendChild(telefax);
                electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
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

                if (!string.IsNullOrEmpty(gidenFatura.BankaAd))
                {
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

                #region KDVlerin Belirlenmesi

                var faturaKdvListesi = new List<FaturaKdvDTO>();
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

                #endregion

                #region TaxTotal

                var sayac = 1;
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
                    taxTypeName.InnerText = "Katma Değer Vergisi";
                    var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
                    taxTypeCode.InnerText = "0015";
                    taxScheme2.AppendChild(taxTypeName);
                    taxScheme2.AppendChild(taxTypeCode);
                    taxCategory.AppendChild(taxScheme2);
                    taxSubTotal.AppendChild(taxCategory);
                    taxTotal.AppendChild(taxSubTotal);
                    root.AppendChild(taxTotal);

                    #endregion
                }

                #endregion

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
                var allowanceTotalAmount = doc.CreateElement("cbc", "AllowanceTotalAmount", xmlnscbc.Value);
                allowanceTotalAmount.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                allowanceTotalAmount.Attributes.Append(currencyId);
                allowanceTotalAmount.InnerText = "0.00";
                legalMonetaryTotal.AppendChild(allowanceTotalAmount);
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

                sayac = 1;
                // Her bir fatura detayı için hazırlanan bölümdür
                foreach (var item in gidenFaturaDetayListesi)
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
                    currencyId = doc.CreateAttribute("currencyID");
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
                }

                #endregion

                doc.AppendChild(root);

                #region Dosya Kaydı

                var fileName = aktarilacakKlasorAdi + "/" + gidenFatura.GidenFaturaId + ".xml";
                doc.Save(fileName);

                #endregion

                return fileName;

                #endregion
            }
            else
            {
                #region Tüzel Kişi Bilgisinden Gelecekse

                var doc = new XmlDocument();
                var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                var root = doc.DocumentElement;
                doc.InsertBefore(declaration, root);
                //doc.AppendChild(doc.CreateProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"general.xslt\""));

                root = doc.CreateElement("Invoice");

                var kodSatisTuruKod = 0;
                if (gidenFatura.SatisTuruKod != null)
                    kodSatisTuruKod = gidenFatura.SatisTuruKod ?? 0;

                var kodFaturaTuruKod = 0;
                if (gidenFatura.FaturaTuruKod != null)
                    kodFaturaTuruKod = gidenFatura.FaturaTuruKod ?? 0;

                var kodFaturaGrupTuruKod = 0;
                if (gidenFatura.FaturaGrupTuruKod != null)
                    kodFaturaGrupTuruKod = gidenFatura.FaturaGrupTuruKod ?? 0;

                #region Standart ve Faturaya Bağlı Bilgiler

                // Faturanın türü, düzenleme tarihi, UBL Versiyon Numarası, GİB Numarası gibi bilgiler
                // yer almaktadır

                #region Standart Bilgiler

                root.RemoveAllAttributes();

                var locationAttribute = "xsi:schemaLocation";
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
                var xmlnscac = doc.CreateAttribute("xmlns:cac");
                xmlnscac.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
                var xmlnscbc = doc.CreateAttribute("xmlns:cbc");
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

                var versionId = doc.CreateElement("cbc", "UBLVersionID", xmlnscbc.Value);
                versionId.InnerText = "2.1";
                root.AppendChild(versionId);

                var customizationId = doc.CreateElement("cbc", "CustomizationID", xmlnscbc.Value);
                customizationId.InnerText = "TR1.2";
                root.AppendChild(customizationId);

                var profileId = doc.CreateElement("cbc", "ProfileID", xmlnscbc.Value);
                profileId.InnerText = "EARSIVFATURA";
                root.AppendChild(profileId);

                var id = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.GibNumarasi))
                    id.InnerText = gidenFatura.GibNumarasi;
                else
                    id.InnerText = "ABC" + gidenFatura.DuzenlemeTarihi?.Year +
                                   DateTime.UtcNow.Ticks.ToString().Substring(0, 9);
                root.AppendChild(id);

                var copyIndicator = doc.CreateElement("cbc", "CopyIndicator", xmlnscbc.Value);
                copyIndicator.InnerText = "false";
                root.AppendChild(copyIndicator);

                var gidenFaturaId = doc.CreateElement("cbc", "UUID", xmlnscbc.Value);
                gidenFaturaId.InnerText = gidenFatura.GidenFaturaId.Length == 36
                    ? gidenFatura.GidenFaturaId.ToUpper()
                    : Guid.NewGuid().ToString().ToUpper();
                root.AppendChild(gidenFaturaId);

                var issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
                issueDate.InnerText = gidenFatura.DuzenlemeTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
                root.AppendChild(issueDate);

                var issueTime = doc.CreateElement("cbc", "IssueTime", xmlnscbc.Value);
                issueTime.InnerText = gidenFatura.DuzenlemeTarihi?.ToString("HH:mm:ss") ?? string.Empty;
                root.AppendChild(issueTime);

                var invoiceTypeCode = doc.CreateElement("cbc", "InvoiceTypeCode", xmlnscbc.Value);
                invoiceTypeCode.InnerText = "SATIS";
                if (gidenFatura.KdvTutari == 0 || gidenFaturaDetayListesi.Any(j => j.KdvTutari == 0))
                    invoiceTypeCode.InnerText = "ISTISNA";
                if (kodSatisTuruKod == SatisTur.IhracKayitli.GetHashCode())
                    invoiceTypeCode.InnerText = "IHRACKAYITLI";
                if (kodFaturaTuruKod == FaturaTur.Iade.GetHashCode())
                    invoiceTypeCode.InnerText = "IADE";
                root.AppendChild(invoiceTypeCode);

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

                if (gidenFaturaDetayListesi2.Count > 0)
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
                        if ((item.FaturaUrunTuru.Contains("Şartname") || item.FaturaUrunTuru.Contains("Kira")) &&
                            kodFaturaGrupTuruKod == FaturaMakbuzGrupTur.Diger.GetHashCode() &&
                            !string.IsNullOrEmpty(gidenFatura.Aciklama))
                            irsaliyeNote.InnerText += " Açıklama: " + gidenFatura.Aciklama;
                        if (!string.IsNullOrEmpty(irsaliyeNote.InnerText))
                            root.AppendChild(irsaliyeNote);
                    }
                }

                #endregion

                var sendType = doc.CreateElement("cbc", "Note", xmlnscbc.Value);
                sendType.InnerText = "Gönderim Şekli: ELEKTRONIK";
                root.AppendChild(sendType);

                var documentCurrencyCode = doc.CreateElement("cbc", "DocumentCurrencyCode", xmlnscbc.Value);
                documentCurrencyCode.InnerText = "TRY";
                root.AppendChild(documentCurrencyCode);

                var lineCountNumeric = doc.CreateElement("cbc", "LineCountNumeric", xmlnscbc.Value);
                lineCountNumeric.InnerText = gidenFaturaDetayListesi.Count.ToString();
                root.AppendChild(lineCountNumeric);

                #endregion

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
                partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
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

                postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
                var postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                postalAddressId.InnerText = "3250566851";
                postalAddress.AppendChild(postalAddressId);
                streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
                streetName.InnerText = "Mithatpaşa Caddesi";
                postalAddress.AppendChild(streetName);
                buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
                buildingNumber.InnerText = "14";
                postalAddress.AppendChild(buildingNumber);
                citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
                citySubdivisionName.InnerText = "Çankaya";
                postalAddress.AppendChild(citySubdivisionName);
                cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
                cityName.InnerText = "Ankara";
                postalAddress.AppendChild(cityName);
                postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
                postalZone.InnerText = "06100";
                postalAddress.AppendChild(postalZone);
                country = doc.CreateElement("cac", "Country", xmlnscac.Value);
                countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
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

                // Bu kısımda fatura kesilen firma bilgileri yer almaktadır

                #region AccountingCustomerParty

                var accountingCustomerParty = doc.CreateElement("cac", "AccountingCustomerParty", xmlnscac.Value);
                party = doc.CreateElement("cac", "Party", xmlnscac.Value);
                webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
                party.AppendChild(webSiteUri);
                var accountingCustomerPartyIdAttr = doc.CreateAttribute("schemeID");
                accountingCustomerPartyIdAttr.Value = "TCKN";
                if (!string.IsNullOrEmpty(gidenFatura.VergiNo) && gidenFatura.VergiNo.Length == 10)
                    accountingCustomerPartyIdAttr.Value = "VKN";
                partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
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

                postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
                postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                postalAddressId.InnerText = gidenFatura.VergiNo;
                postalAddress.AppendChild(postalAddressId);
                var room = doc.CreateElement("cbc", "Room", xmlnscbc.Value);
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

                if (!string.IsNullOrEmpty(gidenFatura.VergiNo) && gidenFatura.VergiNo.Length == 11)
                {
                    var liste = gidenFatura.TuzelKisiAd.Split(' ').ToList();
                    if (liste.Count > 1)
                    {
                        var person = doc.CreateElement("cac", "Person", xmlnscac.Value);
                        var firstName = doc.CreateElement("cbc", "FirstName", xmlnscbc.Value);
                        firstName.InnerText = liste[0];
                        person.AppendChild(firstName);
                        var surname = gidenFatura.TuzelKisiAd.Substring(liste[0].Length + 1);
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

                if (!string.IsNullOrEmpty(gidenFatura.VergiNo))
                {
                    if (gidenFatura.VergiNo.Length == 10)
                    {
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
                            if (liste.Count > 1)
                            {
                                var person = doc.CreateElement("cac", "Person", xmlnscac.Value);
                                var firstName = doc.CreateElement("cbc", "FirstName", xmlnscbc.Value);
                                firstName.InnerText = liste[0];
                                person.AppendChild(firstName);
                                var surname = gidenFatura.TuzelKisiAd.Substring(liste[0].Length + 1);
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
                }

                if (!string.IsNullOrEmpty(gidenFatura.BankaAd))
                {
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

                #region KDVlerin Belirlenmesi

                var faturaKdvListesi = new List<FaturaKdvDTO>();
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

                #endregion

                #region TaxTotal

                var sayac = 1;
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
                    taxTypeName.InnerText = "Katma Değer Vergisi";
                    var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
                    taxTypeCode.InnerText = "0015";
                    taxScheme2.AppendChild(taxTypeName);
                    taxScheme2.AppendChild(taxTypeCode);
                    taxCategory.AppendChild(taxScheme2);
                    taxSubTotal.AppendChild(taxCategory);
                    taxTotal.AppendChild(taxSubTotal);
                    root.AppendChild(taxTotal);

                    #endregion
                }

                #endregion

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
                var allowanceTotalAmount = doc.CreateElement("cbc", "AllowanceTotalAmount", xmlnscbc.Value);
                allowanceTotalAmount.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                allowanceTotalAmount.Attributes.Append(currencyId);
                allowanceTotalAmount.InnerText = "0.00";
                legalMonetaryTotal.AppendChild(allowanceTotalAmount);
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

                sayac = 1;
                // Her bir fatura edtayı için hazırlanan bölümdür
                foreach (var item in gidenFaturaDetayListesi)
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
                    currencyId = doc.CreateAttribute("currencyID");
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
                }

                #endregion

                doc.AppendChild(root);

                #region Dosya Kaydı

                var fileName = aktarilacakKlasorAdi + "/" + gidenFatura.GidenFaturaId + ".xml";
                doc.Save(fileName);

                #endregion

                return fileName;

                #endregion
            }
        }

        /// <summary>
        /// Müstahsil Makbuzu Bilgisi Üzerinden E-Müstahsil Sistemine Göndermek İçin
        /// XML Dosyası Oluşturmaya yarayan metottur
        /// </summary>
        /// <param name="mustahsilMakbuzu">Müstahsil Makbuzu Bilgisi</param>
        /// <param name="mustahsilMakbuzuDetayListesi">Müstahsil Makbuzu Detay Bilgisi</param>
        /// <param name="aktarilacakKlasorAdi">Oluşturulan Dosyanın Aktarılacağı Klasör</param>
        public static string EMustahsilXMLOlustur(MustahsilMakbuzuDTO mustahsilMakbuzu,
            List<MustahsilMakbuzuDetayDTO> mustahsilMakbuzuDetayListesi, string aktarilacakKlasorAdi)
        {
            var doc = new XmlDocument();
            var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            var root = doc.DocumentElement;
            doc.InsertBefore(declaration, root);
            doc.AppendChild(
                doc.CreateProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"general.xslt\""));

            root = doc.CreateElement("CreditNote");

            mustahsilMakbuzu = mustahsilMakbuzu.BosluklariKaldir();

            var kodMakbuzGrupTuruKod = 0;
            if (mustahsilMakbuzu.MakbuzGrupTuruKod != null)
                kodMakbuzGrupTuruKod = mustahsilMakbuzu.MakbuzGrupTuruKod ?? 0;

            #region Standart ve Müstahsil Makbuzuna Bağlı Bilgiler

            // Makbuzun türü, kesilme tarihi, UBL Versiyon Numarası, GİB Numarası gibi bilgiler
            // yer almaktadır

            #region Standart Bilgiler

            root.RemoveAllAttributes();
            var locationAttribute = "xsi:schemaLocation";
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
            var xmlnscac = doc.CreateAttribute("xmlns:cac");
            xmlnscac.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            var xmlnscbc = doc.CreateAttribute("xmlns:cbc");
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
                id.InnerText = "ABC" + mustahsilMakbuzu.MustahsilMakbuzuTarihi?.Year +
                               DateTime.UtcNow.Ticks.ToString().Substring(0, 9);
            root.AppendChild(id);

            var copyIndicator = doc.CreateElement("cbc", "CopyIndicator", xmlnscbc.Value);
            copyIndicator.InnerText = "false";
            root.AppendChild(copyIndicator);

            var mustahsilMakbuzuId = doc.CreateElement("cbc", "UUID", xmlnscbc.Value);
            mustahsilMakbuzuId.InnerText = mustahsilMakbuzu.MustahsilMakbuzuId.Length == 36
                ? mustahsilMakbuzu.MustahsilMakbuzuId.ToUpper()
                : Guid.NewGuid().ToString().ToUpper();
            root.AppendChild(mustahsilMakbuzuId);

            var issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
            issueDate.InnerText = mustahsilMakbuzu.MustahsilMakbuzuTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
            root.AppendChild(issueDate);

            var issueTime = doc.CreateElement("cbc", "IssueTime", xmlnscbc.Value);
            issueTime.InnerText = mustahsilMakbuzu.MustahsilMakbuzuTarihi?.ToString("HH:mm:ss") ?? string.Empty;
            root.AppendChild(issueTime);

            var creditNoteTypeCode = doc.CreateElement("cbc", "CreditNoteTypeCode", xmlnscbc.Value);
            creditNoteTypeCode.InnerText = "MUSTAHSILMAKBUZ";
            root.AppendChild(creditNoteTypeCode);

            var sendType = doc.CreateElement("cbc", "Note", xmlnscbc.Value);
            sendType.InnerText = "Gönderim Şekli: ELEKTRONIK";
            root.AppendChild(sendType);

            var documentCurrencyCode = doc.CreateElement("cbc", "DocumentCurrencyCode", xmlnscbc.Value);
            documentCurrencyCode.InnerText = "TRY";
            root.AppendChild(documentCurrencyCode);

            var lineCountNumeric = doc.CreateElement("cbc", "LineCountNumeric", xmlnscbc.Value);
            lineCountNumeric.InnerText = mustahsilMakbuzuDetayListesi.Count.ToString();
            root.AppendChild(lineCountNumeric);

            #region AdditionalDocumentReference

            var additionalDocumentReference = doc.CreateElement("cac", "AdditionalDocumentReference", xmlnscac.Value);
            var additionalDocumentReferenceId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            additionalDocumentReferenceId.InnerText = mustahsilMakbuzu.MustahsilMakbuzuId.Length == 36
                ? mustahsilMakbuzu.MustahsilMakbuzuId.ToUpper()
                : Guid.NewGuid().ToString().ToUpper();
            additionalDocumentReference.AppendChild(additionalDocumentReferenceId);
            issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
            issueDate.InnerText = mustahsilMakbuzu.MustahsilMakbuzuTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
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

            #endregion

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
            partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
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

            postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
            var postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            postalAddressId.InnerText = "3250566851";
            postalAddress.AppendChild(postalAddressId);
            streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
            streetName.InnerText = "Mithatpaşa Caddesi";
            postalAddress.AppendChild(streetName);
            buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
            buildingNumber.InnerText = "14";
            postalAddress.AppendChild(buildingNumber);
            citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
            citySubdivisionName.InnerText = "Çankaya";
            postalAddress.AppendChild(citySubdivisionName);
            cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
            cityName.InnerText = "Ankara";
            postalAddress.AppendChild(cityName);
            postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
            postalZone.InnerText = "06100";
            postalAddress.AppendChild(postalZone);
            country = doc.CreateElement("cac", "Country", xmlnscac.Value);
            countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
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

            // Bu kısımda makbuz kesilen firma bilgileri yer almaktadır

            #region AccountingCustomerParty

            var accountingCustomerParty = doc.CreateElement("cac", "AccountingCustomerParty", xmlnscac.Value);
            party = doc.CreateElement("cac", "Party", xmlnscac.Value);
            webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
            party.AppendChild(webSiteUri);
            var accountingCustomerPartyIdAttr = doc.CreateAttribute("schemeID");
            accountingCustomerPartyIdAttr.Value = "TCKN";
            if (mustahsilMakbuzu.VergiNo.Length == 10)
                accountingCustomerPartyIdAttr.Value = "VKN";
            partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
            var accountingCustomerPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            accountingCustomerPartyPartyId.Attributes.Append(accountingCustomerPartyIdAttr);
            accountingCustomerPartyPartyId.InnerText = mustahsilMakbuzu.VergiNo;
            partyIdentification.AppendChild(accountingCustomerPartyPartyId);
            party.AppendChild(partyIdentification);

            if (mustahsilMakbuzu.VergiNo.Length == 10)
            {
                partyName = doc.CreateElement("cac", "PartyName", xmlnscac.Value);
                partyNameReal = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                partyNameReal.InnerText = mustahsilMakbuzu.TuzelKisiAd;
                partyName.AppendChild(partyNameReal);
                party.AppendChild(partyName);
            }

            #region Postal Address

            postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
            postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            postalAddressId.InnerText = mustahsilMakbuzu.VergiNo;
            postalAddress.AppendChild(postalAddressId);
            var room = doc.CreateElement("cbc", "Room", xmlnscbc.Value);
            postalAddress.AppendChild(room);
            streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(mustahsilMakbuzu.Adres))
                streetName.InnerText = mustahsilMakbuzu.Adres;
            postalAddress.AppendChild(streetName);
            buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
            //buildingNumber.InnerText = "";
            postalAddress.AppendChild(buildingNumber);
            citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(mustahsilMakbuzu.IlceAd))
                citySubdivisionName.InnerText = mustahsilMakbuzu.IlceAd;
            postalAddress.AppendChild(citySubdivisionName);
            cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(mustahsilMakbuzu.IlAd))
                cityName.InnerText = mustahsilMakbuzu.IlAd;
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
            if (!string.IsNullOrEmpty(mustahsilMakbuzu.VergiDairesi))
                taxSchemeName.InnerText = mustahsilMakbuzu.VergiDairesi;
            taxScheme.AppendChild(taxSchemeName);
            partyTaxScheme.AppendChild(taxScheme);
            party.AppendChild(partyTaxScheme);

            contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
            electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(mustahsilMakbuzu.EPostaAdresi))
                electronicMail.InnerText = mustahsilMakbuzu.EPostaAdresi;
            contact.AppendChild(electronicMail);
            party.AppendChild(contact);

            if (mustahsilMakbuzu.VergiNo.Length == 11)
            {
                var liste = mustahsilMakbuzu.TuzelKisiAd.Split(' ').ToList();
                if (liste.Count > 1)
                {
                    var person = doc.CreateElement("cac", "Person", xmlnscac.Value);
                    var firstName = doc.CreateElement("cbc", "FirstName", xmlnscbc.Value);
                    firstName.InnerText = liste[0];
                    person.AppendChild(firstName);
                    var surname = mustahsilMakbuzu.TuzelKisiAd.Substring(liste[0].Length + 1);
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

            if (!string.IsNullOrEmpty(mustahsilMakbuzu.VergiNo))
            {
                if (mustahsilMakbuzu.VergiNo.Length == 10)
                {
                    // Bu kısımda makbuz kesilen firma bilgileri yer almaktadır (Eğer sadece Tüzel Kişilikse, şahıs şirketleri için bu yapılmamaktadır)

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
                    buyerCustomerPartyPartyId.InnerText = mustahsilMakbuzu.VergiNo;
                    partyIdentification.AppendChild(buyerCustomerPartyPartyId);
                    party.AppendChild(partyIdentification);

                    if (mustahsilMakbuzu.VergiNo.Length == 10)
                    {
                        partyName = doc.CreateElement("cac", "PartyName", xmlnscac.Value);
                        partyNameReal = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                        partyNameReal.InnerText = mustahsilMakbuzu.TuzelKisiAd;
                        partyName.AppendChild(partyNameReal);
                        party.AppendChild(partyName);
                    }

                    #region Postal Address

                    postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
                    postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                    postalAddressId.InnerText = mustahsilMakbuzu.VergiNo;
                    postalAddress.AppendChild(postalAddressId);
                    room = doc.CreateElement("cbc", "Room", xmlnscbc.Value);
                    postalAddress.AppendChild(room);
                    streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(mustahsilMakbuzu.Adres))
                        streetName.InnerText = mustahsilMakbuzu.Adres;
                    postalAddress.AppendChild(streetName);
                    buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
                    //buildingNumber.InnerText = "";
                    postalAddress.AppendChild(buildingNumber);
                    citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(mustahsilMakbuzu.IlceAd))
                        citySubdivisionName.InnerText = mustahsilMakbuzu.IlceAd;
                    postalAddress.AppendChild(citySubdivisionName);
                    cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(mustahsilMakbuzu.IlAd))
                        cityName.InnerText = mustahsilMakbuzu.IlAd;
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
                    if (!string.IsNullOrEmpty(mustahsilMakbuzu.VergiDairesi))
                        taxSchemeName.InnerText = mustahsilMakbuzu.VergiDairesi;
                    taxScheme.AppendChild(taxSchemeName);
                    partyTaxScheme.AppendChild(taxScheme);
                    party.AppendChild(partyTaxScheme);

                    contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
                    electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(mustahsilMakbuzu.EPostaAdresi))
                        electronicMail.InnerText = mustahsilMakbuzu.EPostaAdresi;
                    contact.AppendChild(electronicMail);
                    party.AppendChild(contact);

                    if (mustahsilMakbuzu.VergiNo.Length == 11)
                    {
                        var liste = mustahsilMakbuzu.TuzelKisiAd.Split(' ').ToList();
                        if (liste.Count > 1)
                        {
                            var person = doc.CreateElement("cac", "Person", xmlnscac.Value);
                            var firstName = doc.CreateElement("cbc", "FirstName", xmlnscbc.Value);
                            firstName.InnerText = liste[0];
                            person.AppendChild(firstName);
                            var surname = mustahsilMakbuzu.TuzelKisiAd.Substring(liste[0].Length + 1);
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
            }

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

            var sayac = 1;
            // Her bir makbuz detayı için hazırlanan bölümdür
            foreach (var item in mustahsilMakbuzuDetayListesi)
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
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                lineExtensionAmountDetail.Attributes.Append(currencyId);
                lineExtensionAmountDetail.InnerText = Decimal
                    .Round((decimal) detayBilgisi.NetTutar, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                creditNoteLine.AppendChild(lineExtensionAmountDetail);

                #endregion

                #region TaxTotal

                taxTotal = doc.CreateElement("cac", "TaxTotal", xmlnscac.Value);
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                taxAmount = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                taxAmount.RemoveAllAttributes();
                taxAmount.Attributes.Append(currencyId);
                taxAmount.InnerText = Decimal
                    .Round((decimal) detayBilgisi.GelirVergisi, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                taxTotal.AppendChild(taxAmount);
                taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
                taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
                taxableAmount.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                taxableAmount.Attributes.Append(currencyId);
                taxableAmount.InnerText = Decimal
                    .Round((decimal) detayBilgisi.NetTutar, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                taxSubTotal.AppendChild(taxableAmount);
                taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                taxAmount2.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                taxAmount2.Attributes.Append(currencyId);
                taxAmount2.InnerText = Decimal
                    .Round((decimal) detayBilgisi.GelirVergisi, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                taxSubTotal.AppendChild(taxAmount2);
                percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
                percent.InnerText = "2";
                taxSubTotal.AppendChild(percent);
                taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
                taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                var taxTypeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                taxTypeName.InnerText = "GV STOPAJI";
                taxScheme2.AppendChild(taxTypeName);
                taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
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
            }

            #endregion

            doc.AppendChild(root);

            #region Dosya Kaydı

            var fileName = aktarilacakKlasorAdi + "/" + mustahsilMakbuzu.MustahsilMakbuzuId + ".xml";
            doc.Save(fileName);

            #endregion

            return fileName;
        }
    }
}