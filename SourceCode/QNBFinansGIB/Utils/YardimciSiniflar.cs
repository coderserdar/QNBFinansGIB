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
            issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
            issueDate.InnerText = gidenFatura.DuzenlemeTarihi?.Date.ToString("yyyy-MM-dd");
            additionalDocumentReference.AppendChild(issueDate);
            documentType = doc.CreateElement("cbc", "DocumentType", xmlnscbc.Value);
            documentType.InnerText = "XSLT";
            additionalDocumentReference.AppendChild(documentType);
            var attachment = doc.CreateElement("cac", "Attachment", xmlnscac.Value);
            var embeddedDocumentBinaryObject = doc.CreateElement("cbc", "EmbeddedDocumentBinaryObject", xmlnscbc.Value);
            XmlAttribute characterSetCode = doc.CreateAttribute("characterSetCode");
            characterSetCode.Value = "UTF-8";
            XmlAttribute encodingCode = doc.CreateAttribute("encodingCode");
            encodingCode.Value = "Base64";
            XmlAttribute fileName2 = doc.CreateAttribute("fileName");
            fileName2.Value = "efatura.xslt";
            XmlAttribute mimeCode = doc.CreateAttribute("mimeCode");
            mimeCode.Value = "application/xml";
            embeddedDocumentBinaryObject.Attributes.Append(characterSetCode);
            embeddedDocumentBinaryObject.Attributes.Append(encodingCode);
            embeddedDocumentBinaryObject.Attributes.Append(fileName2);
            embeddedDocumentBinaryObject.Attributes.Append(mimeCode);
            #region Base 64 Metin
            embeddedDocumentBinaryObject.InnerText = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiPz4KPHhzbDpzdHlsZXNoZWV0IHZlcnNpb249IjIuMCIgeG1sbnM6eHNsPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5L1hTTC9UcmFuc2Zvcm0iCiAgICAgICAgeG1sbnM6Y2FjPSJ1cm46b2FzaXM6bmFtZXM6c3BlY2lmaWNhdGlvbjp1Ymw6c2NoZW1hOnhzZDpDb21tb25BZ2dyZWdhdGVDb21wb25lbnRzLTIiCiAgICAgICAgeG1sbnM6Y2JjPSJ1cm46b2FzaXM6bmFtZXM6c3BlY2lmaWNhdGlvbjp1Ymw6c2NoZW1hOnhzZDpDb21tb25CYXNpY0NvbXBvbmVudHMtMiIKICAgICAgICB4bWxuczpjY3RzPSJ1cm46dW46dW5lY2U6dW5jZWZhY3Q6ZG9jdW1lbnRhdGlvbjoyIgogICAgICAgIHhtbG5zOmNsbTU0MjE3PSJ1cm46dW46dW5lY2U6dW5jZWZhY3Q6Y29kZWxpc3Q6c3BlY2lmaWNhdGlvbjo1NDIxNzoyMDAxIgogICAgICAgIHhtbG5zOmNsbTU2Mzk9InVybjp1bjp1bmVjZTp1bmNlZmFjdDpjb2RlbGlzdDpzcGVjaWZpY2F0aW9uOjU2Mzk6MTk4OCIKICAgICAgICB4bWxuczpjbG02NjQxMT0idXJuOnVuOnVuZWNlOnVuY2VmYWN0OmNvZGVsaXN0OnNwZWNpZmljYXRpb246NjY0MTE6MjAwMSIKICAgICAgICB4bWxuczpjbG1JQU5BTUlNRU1lZGlhVHlwZT0idXJuOnVuOnVuZWNlOnVuY2VmYWN0OmNvZGVsaXN0OnNwZWNpZmljYXRpb246SUFOQU1JTUVNZWRpYVR5cGU6MjAwMyIKICAgICAgICB4bWxuczpmbj0iaHR0cDovL3d3dy53My5vcmcvMjAwNS94cGF0aC1mdW5jdGlvbnMiIHhtbG5zOmxpbms9Imh0dHA6Ly93d3cueGJybC5vcmcvMjAwMy9saW5rYmFzZSIKICAgICAgICB4bWxuczpuMT0idXJuOm9hc2lzOm5hbWVzOnNwZWNpZmljYXRpb246dWJsOnNjaGVtYTp4c2Q6SW52b2ljZS0yIgogICAgICAgIHhtbG5zOnFkdD0idXJuOm9hc2lzOm5hbWVzOnNwZWNpZmljYXRpb246dWJsOnNjaGVtYTp4c2Q6UXVhbGlmaWVkRGF0YXR5cGVzLTIiCiAgICAgICAgeG1sbnM6dWR0PSJ1cm46dW46dW5lY2U6dW5jZWZhY3Q6ZGF0YTpzcGVjaWZpY2F0aW9uOlVucXVhbGlmaWVkRGF0YVR5cGVzU2NoZW1hTW9kdWxlOjIiCiAgICAgICAgeG1sbnM6eGJybGRpPSJodHRwOi8veGJybC5vcmcvMjAwNi94YnJsZGkiIHhtbG5zOnhicmxpPSJodHRwOi8vd3d3Lnhicmwub3JnLzIwMDMvaW5zdGFuY2UiCiAgICAgICAgeG1sbnM6eGR0PSJodHRwOi8vd3d3LnczLm9yZy8yMDA1L3hwYXRoLWRhdGF0eXBlcyIgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsiCiAgICAgICAgeG1sbnM6eHM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hIiB4bWxuczp4c2Q9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hIgogICAgICAgIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiCiAgICAgICAgZXhjbHVkZS1yZXN1bHQtcHJlZml4ZXM9ImNhYyBjYmMgY2N0cyBjbG01NDIxNyBjbG01NjM5IGNsbTY2NDExIGNsbUlBTkFNSU1FTWVkaWFUeXBlIGZuIGxpbmsgbjEgcWR0IHVkdCB4YnJsZGkgeGJybGkgeGR0IHhsaW5rIHhzIHhzZCB4c2kiPgogICAgICAgIDx4c2w6ZGVjaW1hbC1mb3JtYXQgbmFtZT0iZXVyb3BlYW4iIGRlY2ltYWwtc2VwYXJhdG9yPSIsIiBncm91cGluZy1zZXBhcmF0b3I9Ii4iIE5hTj0iIi8+CiAgICAgICAgPHhzbDpvdXRwdXQgdmVyc2lvbj0iNC4wIiBtZXRob2Q9Imh0bWwiIGluZGVudD0ibm8iIGVuY29kaW5nPSJVVEYtOCIKICAgICAgICBkb2N0eXBlLXB1YmxpYz0iLS8vVzNDLy9EVEQgSFRNTCA0LjAxIFRyYW5zaXRpb25hbC8vRU4iCiAgICAgICAgZG9jdHlwZS1zeXN0ZW09Imh0dHA6Ly93d3cudzMub3JnL1RSL2h0bWw0L2xvb3NlLmR0ZCIvPgogICAgICAgIDx4c2w6cGFyYW0gbmFtZT0iU1ZfT3V0cHV0Rm9ybWF0IiBzZWxlY3Q9IidIVE1MJyIvPgoKICAgICAgICA8eHNsOnRlbXBsYXRlIG5hbWU9InJlcE5MIj4KICAgICAgICAgICAgICAgIDx4c2w6cGFyYW0gbmFtZT0icFRleHQiIHNlbGVjdD0iLiIvPgogICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJub3QoY29udGFpbnMoc3Vic3RyaW5nKHN1YnN0cmluZy1iZWZvcmUoY29uY2F0KCRwVGV4dCwnJiN4QTsnKSwnJiN4QTsnKSwwLDgpLCAnIyMnKSkgYW5kIHN0cmluZy1sZW5ndGgoc3Vic3RyaW5nLWJlZm9yZShjb25jYXQoJHBUZXh0LCcmI3hBOycpLCcmI3hBOycpKT4zIj4KICAgICAgICAgICAgICAgICAgICAgICAgPGI+KiA8L2I+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Y29weS1vZiBzZWxlY3Q9InN1YnN0cmluZy1iZWZvcmUoY29uY2F0KCRwVGV4dCwnJiN4QTsnKSwnJiN4QTsnKSIvPgogICAgICAgICAgICAgICAgICAgICAgICA8YnIgLz4KICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjb250YWlucygkcFRleHQsICcmI3hBOycpIj4KCiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Y2FsbC10ZW1wbGF0ZSBuYW1lPSJyZXBOTCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aXRoLXBhcmFtIG5hbWU9InBUZXh0IiBzZWxlY3Q9CiAgICAic3Vic3RyaW5nLWFmdGVyKCRwVGV4dCwgJyYjeEE7JykiLz4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Y2FsbC10ZW1wbGF0ZT4KICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgIDwveHNsOnRlbXBsYXRlPgoKICAgICAgICA8eHNsOnRlbXBsYXRlIG5hbWU9InJlcE5MMiI+CiAgICAgICAgICAgICAgICA8eHNsOnBhcmFtIG5hbWU9InBUZXh0IiBzZWxlY3Q9Ii4iLz4KICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY29udGFpbnMoc3Vic3RyaW5nKHN1YnN0cmluZy1iZWZvcmUoY29uY2F0KCRwVGV4dCwnJiN4QTsnKSwnJiN4QTsnKSwwLDgpLCAnIyMnKSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0cj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmNvcHktb2Ygc2VsZWN0PSJzdWJzdHJpbmctYWZ0ZXIoc3Vic3RyaW5nLWJlZm9yZShzdWJzdHJpbmctYmVmb3JlKGNvbmNhdCgkcFRleHQsJyYjeEE7JyksJyYjeEE7JyksJzonKSwnIyMnKSIvPjo8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0ZD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Y29weS1vZiBzZWxlY3Q9InN1YnN0cmluZy1hZnRlcihzdWJzdHJpbmctYmVmb3JlKGNvbmNhdCgkcFRleHQsJyYjeEE7JyksJyYjeEE7JyksJzonKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdGQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvdHI+CiAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY29udGFpbnMoJHBUZXh0LCAnJiN4QTsnKSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Y2FsbC10ZW1wbGF0ZSBuYW1lPSJyZXBOTDIiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2l0aC1wYXJhbSBuYW1lPSJwVGV4dCIgc2VsZWN0PQogICAgInN1YnN0cmluZy1hZnRlcigkcFRleHQsICcmI3hBOycpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmNhbGwtdGVtcGxhdGU+CiAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICA8L3hzbDp0ZW1wbGF0ZT4KCiAgICAgICAgPHhzbDp2YXJpYWJsZSBuYW1lPSJYTUwiIHNlbGVjdD0iLyIvPgogICAgICAgIDx4c2w6dGVtcGxhdGUgbmFtZT0icmVtb3ZlTGVhZGluZ1plcm9zIj4KICAgICAgICAgICAgICAgIDx4c2w6cGFyYW0gbmFtZT0ib3JpZ2luYWxTdHJpbmciLz4KICAgICAgICAgICAgICAgIDx4c2w6Y2hvb3NlPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0ic3RhcnRzLXdpdGgoJG9yaWdpbmFsU3RyaW5nLCcwJykiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Y2FsbC10ZW1wbGF0ZSBuYW1lPSJyZW1vdmVMZWFkaW5nWmVyb3MiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aXRoLXBhcmFtIG5hbWU9Im9yaWdpbmFsU3RyaW5nIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9InN1YnN0cmluZy1hZnRlcigkb3JpZ2luYWxTdHJpbmcsJzAnICkiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndpdGgtcGFyYW0+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Y2FsbC10ZW1wbGF0ZT4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpvdGhlcndpc2U+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9IiRvcmlnaW5hbFN0cmluZyIvPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpvdGhlcndpc2U+CiAgICAgICAgICAgICAgICA8L3hzbDpjaG9vc2U+CiAgICAgICAgPC94c2w6dGVtcGxhdGU+CiAgICAgICAgPHhzbDp0ZW1wbGF0ZSBuYW1lPSJUcmFuc3BvcnRNb2RlIj4KICAgICAgICAgICAgICAgIDx4c2w6cGFyYW0gbmFtZT0iVHJhbnNwb3J0TW9kZVR5cGUiIC8+CiAgICAgICAgICAgICAgICA8eHNsOmNob29zZT4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRUcmFuc3BvcnRNb2RlVHlwZT0xIj5EZW5penlvbHU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFRyYW5zcG9ydE1vZGVUeXBlPTIiPkRlbWlyeW9sdTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkVHJhbnNwb3J0TW9kZVR5cGU9MyI+S2FyYXlvbHU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFRyYW5zcG9ydE1vZGVUeXBlPTQiPkhhdmF5b2x1PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRUcmFuc3BvcnRNb2RlVHlwZT01Ij5Qb3N0YTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkVHJhbnNwb3J0TW9kZVR5cGU9NiI+0JPigKFvayBhcmHQk8KnbNCUwrE8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFRyYW5zcG9ydE1vZGVUeXBlPTciPlNhYml0IHRh0JXRn9CUwrFtYSB0ZXNpc2xlcmk8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFRyYW5zcG9ydE1vZGVUeXBlPTgiPtCUwrDQk8KnIHN1IHRh0JXRn9CUwrFtYWPQlMKxbNCUwrHQlNGf0JTCsTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6b3RoZXJ3aXNlPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIkVHJhbnNwb3J0TW9kZVR5cGUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6b3RoZXJ3aXNlPgogICAgICAgICAgICAgICAgPC94c2w6Y2hvb3NlPgogICAgICAgIDwveHNsOnRlbXBsYXRlPgogICAgICAgIDx4c2w6dGVtcGxhdGUgbmFtZT0iUGFja2FnaW5nIj4KICAgICAgICAgICAgICAgIDx4c2w6cGFyYW0gbmFtZT0iUGFja2FnaW5nVHlwZSIgLz4KICAgICAgICAgICAgICAgIDx4c2w6Y2hvb3NlPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzFBJyI+RHJ1bSwgc3RlZWw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzFCJyI+RHJ1bSwgYWx1bWluaXVtPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPScxRCciPkRydW0sIHBseXdvb2Q8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzFGJyI+Q29udGFpbmVyLCBmbGV4aWJsZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nMUcnIj5EcnVtLCBmaWJyZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nMVcnIj5EcnVtLCB3b29kZW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzJDJyI+QmFycmVsLCB3b29kZW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzNBJyI+SmVycmljYW4sIHN0ZWVsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSczSCciPkplcnJpY2FuLCBwbGFzdGljPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSc0MyciPkJhZywgc3VwZXIgYnVsazwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nNDQnIj5CYWcsIHBvbHliYWc8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzRBJyI+Qm94LCBzdGVlbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nNEInIj5Cb3gsIGFsdW1pbml1bTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nNEMnIj5Cb3gsIG5hdHVyYWwgd29vZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nNEQnIj5Cb3gsIHBseXdvb2Q8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzRGJyI+Qm94LCByZWNvbnN0aXR1dGVkIHdvb2Q8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzRHJyI+Qm94LCBmaWJyZWJvYXJkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSc0SCciPkJveCwgcGxhc3RpYzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nNUgnIj5CYWcsIHdvdmVuIHBsYXN0aWM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzVMJyI+QmFnLCB0ZXh0aWxlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSc1TSciPkJhZywgcGFwZXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzZIJyI+Q29tcG9zaXRlIHBhY2thZ2luZywgcGxhc3RpYyByZWNlcHRhY2xlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSc2UCciPkNvbXBvc2l0ZSBwYWNrYWdpbmcsIGdsYXNzIHJlY2VwdGFjbGU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzdBJyI+Q2FzZSwgY2FyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSc3QiciPkNhc2UsIHdvb2RlbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nOEEnIj5QYWxsZXQsIHdvb2RlbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nOEInIj5DcmF0ZSwgd29vZGVuPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSc4QyciPkJ1bmRsZSwgd29vZGVuPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdBQSciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgcmlnaWQgcGxhc3RpYzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQUInIj5SZWNlcHRhY2xlLCBmaWJyZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQUMnIj5SZWNlcHRhY2xlLCBwYXBlcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQUQnIj5SZWNlcHRhY2xlLCB3b29kZW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0FFJyI+QWVyb3NvbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQUYnIj5QYWxsZXQsIG1vZHVsYXIsIGNvbGxhcnMgODBjbXMgKiA2MGNtczwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQUcnIj5QYWxsZXQsIHNocmlua3dyYXBwZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0FIJyI+UGFsbGV0LCAxMDBjbXMgKiAxMTBjbXM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0FJJyI+Q2xhbXNoZWxsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdBSiciPkNvbmU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0FMJyI+QmFsbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQU0nIj5BbXBvdWxlLCBub24tcHJvdGVjdGVkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdBUCciPkFtcG91bGUsIHByb3RlY3RlZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQVQnIj5BdG9taXplcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQVYnIj5DYXBzdWxlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCNCciPkJlbHQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JBJyI+QmFycmVsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCQiciPkJvYmJpbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQkMnIj5Cb3R0bGVjcmF0ZSAvIGJvdHRsZXJhY2s8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JEJyI+Qm9hcmQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JFJyI+QnVuZGxlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCRiciPkJhbGxvb24sIG5vbi1wcm90ZWN0ZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JHJyI+QmFnPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCSCciPkJ1bmNoPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCSSciPkJpbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQkonIj5CdWNrZXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JLJyI+QmFza2V0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCTCciPkJhbGUsIGNvbXByZXNzZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JNJyI+QmFzaW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JOJyI+QmFsZSwgbm9uLWNvbXByZXNzZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JPJyI+Qm90dGxlLCBub24tcHJvdGVjdGVkLCBjeWxpbmRyaWNhbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQlAnIj5CYWxsb29uLCBwcm90ZWN0ZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JRJyI+Qm90dGxlLCBwcm90ZWN0ZWQgY3lsaW5kcmljYWw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JSJyI+QmFyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCUyciPkJvdHRsZSwgbm9uLXByb3RlY3RlZCwgYnVsYm91czwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQlQnIj5Cb2x0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCVSciPkJ1dHQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JWJyI+Qm90dGxlLCBwcm90ZWN0ZWQgYnVsYm91czwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQlcnIj5Cb3gsIGZvciBsaXF1aWRzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCWCciPkJveDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQlknIj5Cb2FyZCwgaW4gYnVuZGxlL2J1bmNoL3RydXNzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCWiciPkJhcnMsIGluIGJ1bmRsZS9idW5jaC90cnVzczwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQ0EnIj5DYW4sIHJlY3Rhbmd1bGFyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDQiciPkNyYXRlLCBiZWVyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDQyciPkNodXJuPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDRCciPkNhbiwgd2l0aCBoYW5kbGUgYW5kIHNwb3V0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDRSciPkNyZWVsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDRiciPkNvZmZlcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQ0cnIj5DYWdlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDSCciPkNoZXN0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDSSciPkNhbmlzdGVyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDSiciPkNvZmZpbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQ0snIj5DYXNrPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDTCciPkNvaWw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0NNJyI+Q2FyZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQ04nIj5Db250YWluZXIsIG5vdCBvdGhlcndpc2Ugc3BlY2lmaWVkIGFzIHRyYW5zcG9ydCBlcXVpcG1lbnQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0NPJyI+Q2FyYm95LCBub24tcHJvdGVjdGVkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDUCciPkNhcmJveSwgcHJvdGVjdGVkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDUSciPkNhcnRyaWRnZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQ1InIj5DcmF0ZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQ1MnIj5DYXNlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDVCciPkNhcnRvbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQ1UnIj5DdXA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0NWJyI+Q292ZXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0NXJyI+Q2FnZSwgcm9sbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQ1gnIj5DYW4sIGN5bGluZHJpY2FsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDWSciPkN5bGluZGVyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDWiciPkNhbnZhczwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nREEnIj5DcmF0ZSwgbXVsdGlwbGUgbGF5ZXIsIHBsYXN0aWM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0RCJyI+Q3JhdGUsIG11bHRpcGxlIGxheWVyLCB3b29kZW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0RDJyI+Q3JhdGUsIG11bHRpcGxlIGxheWVyLCBjYXJkYm9hcmQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0RHJyI+Q2FnZSwgQ29tbW9ud2VhbHRoIEhhbmRsaW5nIEVxdWlwbWVudCBQb29sIChDSEVQKTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nREgnIj5Cb3gsIENvbW1vbndlYWx0aCBIYW5kbGluZyBFcXVpcG1lbnQgUG9vbCAoQ0hFUCksIEV1cm9ib3g8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0RJJyI+RHJ1bSwgaXJvbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nREonIj5EZW1pam9obiwgbm9uLXByb3RlY3RlZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nREsnIj5DcmF0ZSwgYnVsaywgY2FyZGJvYXJkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdETCciPkNyYXRlLCBidWxrLCBwbGFzdGljPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdETSciPkNyYXRlLCBidWxrLCB3b29kZW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0ROJyI+RGlzcGVuc2VyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdEUCciPkRlbWlqb2huLCBwcm90ZWN0ZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0RSJyI+RHJ1bTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRFMnIj5UcmF5LCBvbmUgbGF5ZXIgbm8gY292ZXIsIHBsYXN0aWM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0RUJyI+VHJheSwgb25lIGxheWVyIG5vIGNvdmVyLCB3b29kZW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0RVJyI+VHJheSwgb25lIGxheWVyIG5vIGNvdmVyLCBwb2x5c3R5cmVuZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRFYnIj5UcmF5LCBvbmUgbGF5ZXIgbm8gY292ZXIsIGNhcmRib2FyZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRFcnIj5UcmF5LCB0d28gbGF5ZXJzIG5vIGNvdmVyLCBwbGFzdGljIHRyYXk8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0RYJyI+VHJheSwgdHdvIGxheWVycyBubyBjb3Zlciwgd29vZGVuPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdEWSciPlRyYXksIHR3byBsYXllcnMgbm8gY292ZXIsIGNhcmRib2FyZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRUMnIj5CYWcsIHBsYXN0aWM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0VEJyI+Q2FzZSwgd2l0aCBwYWxsZXQgYmFzZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRUUnIj5DYXNlLCB3aXRoIHBhbGxldCBiYXNlLCB3b29kZW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0VGJyI+Q2FzZSwgd2l0aCBwYWxsZXQgYmFzZSwgY2FyZGJvYXJkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdFRyciPkNhc2UsIHdpdGggcGFsbGV0IGJhc2UsIHBsYXN0aWM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0VIJyI+Q2FzZSwgd2l0aCBwYWxsZXQgYmFzZSwgbWV0YWw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0VJJyI+Q2FzZSwgaXNvdGhlcm1pYzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRU4nIj5FbnZlbG9wZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRkInIj5GbGV4aWJhZzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRkMnIj5DcmF0ZSwgZnJ1aXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0ZEJyI+Q3JhdGUsIGZyYW1lZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRkUnIj5GbGV4aXRhbms8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0ZJJyI+Rmlya2luPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdGTCciPkZsYXNrPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdGTyciPkZvb3Rsb2NrZXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0ZQJyI+RmlsbXBhY2s8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0ZSJyI+RnJhbWU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0ZUJyI+Rm9vZHRhaW5lcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRlcnIj5DYXJ0LCBmbGF0YmVkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdGWCciPkJhZywgZmxleGlibGUgY29udGFpbmVyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdHQiciPkJvdHRsZSwgZ2FzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdHSSciPkdpcmRlcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nR0wnIj5Db250YWluZXIsIGdhbGxvbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nR1InIj5SZWNlcHRhY2xlLCBnbGFzczwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nR1UnIj5UcmF5LCBjb250YWluaW5nIGhvcml6b250YWxseSBzdGFja2VkIGZsYXQgaXRlbXM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0dZJyI+QmFnLCBndW5ueTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nR1onIj5HaXJkZXJzLCBpbiBidW5kbGUvYnVuY2gvdHJ1c3M8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0hBJyI+QmFza2V0LCB3aXRoIGhhbmRsZSwgcGxhc3RpYzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nSEInIj5CYXNrZXQsIHdpdGggaGFuZGxlLCB3b29kZW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0hDJyI+QmFza2V0LCB3aXRoIGhhbmRsZSwgY2FyZGJvYXJkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdIRyciPkhvZ3NoZWFkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdITiciPkhhbmdlcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nSFInIj5IYW1wZXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0lBJyI+UGFja2FnZSwgZGlzcGxheSwgd29vZGVuPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdJQiciPlBhY2thZ2UsIGRpc3BsYXksIGNhcmRib2FyZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nSUMnIj5QYWNrYWdlLCBkaXNwbGF5LCBwbGFzdGljPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdJRCciPlBhY2thZ2UsIGRpc3BsYXksIG1ldGFsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdJRSciPlBhY2thZ2UsIHNob3c8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0lGJyI+UGFja2FnZSwgZmxvdzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nSUcnIj5QYWNrYWdlLCBwYXBlciB3cmFwcGVkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdJSCciPkRydW0sIHBsYXN0aWM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0lLJyI+UGFja2FnZSwgY2FyZGJvYXJkLCB3aXRoIGJvdHRsZSBncmlwLWhvbGVzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdJTCciPlRyYXksIHJpZ2lkLCBsaWRkZWQgc3RhY2thYmxlIChDRU4gVFMgMTQ0ODI6MjAwMik8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0lOJyI+SW5nb3Q8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0laJyI+SW5nb3RzLCBpbiBidW5kbGUvYnVuY2gvdHJ1c3M8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0pCJyI+QmFnLCBqdW1ibzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nSkMnIj5KZXJyaWNhbiwgcmVjdGFuZ3VsYXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0pHJyI+SnVnPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdKUiciPkphcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nSlQnIj5KdXRlYmFnPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdKWSciPkplcnJpY2FuLCBjeWxpbmRyaWNhbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nS0cnIj5LZWc8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0tJJyI+S2l0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdMRSciPkx1Z2dhZ2U8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0xHJyI+TG9nPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdMVCciPkxvdDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nTFUnIj5MdWc8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0xWJyI+TGlmdHZhbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nTFonIj5Mb2dzLCBpbiBidW5kbGUvYnVuY2gvdHJ1c3M8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J01BJyI+Q3JhdGUsIG1ldGFsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdNQiciPkJhZywgbXVsdGlwbHk8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J01DJyI+Q3JhdGUsIG1pbGs8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J01FJyI+Q29udGFpbmVyLCBtZXRhbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nTVInIj5SZWNlcHRhY2xlLCBtZXRhbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nTVMnIj5TYWNrLCBtdWx0aS13YWxsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdNVCciPk1hdDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nTVcnIj5SZWNlcHRhY2xlLCBwbGFzdGljIHdyYXBwZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J01YJyI+TWF0Y2hib3g8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J05BJyI+Tm90IGF2YWlsYWJsZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nTkUnIj5VbnBhY2tlZCBvciB1bnBhY2thZ2VkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdORiciPlVucGFja2VkIG9yIHVucGFja2FnZWQsIHNpbmdsZSB1bml0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdORyciPlVucGFja2VkIG9yIHVucGFja2FnZWQsIG11bHRpcGxlIHVuaXRzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdOUyciPk5lc3Q8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J05UJyI+TmV0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdOVSciPk5ldCwgdHViZSwgcGxhc3RpYzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nTlYnIj5OZXQsIHR1YmUsIHRleHRpbGU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J09BJyI+UGFsbGV0LCBDSEVQIDQwIGNtIHggNjAgY208L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J09CJyI+UGFsbGV0LCBDSEVQIDgwIGNtIHggMTIwIGNtPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdPQyciPlBhbGxldCwgQ0hFUCAxMDAgY20geCAxMjAgY208L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J09EJyI+UGFsbGV0LCBBUyA0MDY4LTE5OTM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J09FJyI+UGFsbGV0LCBJU08gVDExPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdPRiciPlBsYXRmb3JtLCB1bnNwZWNpZmllZCB3ZWlnaHQgb3IgZGltZW5zaW9uPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdPSyciPkJsb2NrPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdPVCciPk9jdGFiaW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J09VJyI+Q29udGFpbmVyLCBvdXRlcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUDInIj5QYW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1BBJyI+UGFja2V0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdQQiciPlBhbGxldCwgYm94IENvbWJpbmVkIG9wZW4tZW5kZWQgYm94IGFuZCBwYWxsZXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1BDJyI+UGFyY2VsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdQRCciPlBhbGxldCwgbW9kdWxhciwgY29sbGFycyA4MGNtcyAqIDEwMGNtczwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUEUnIj5QYWxsZXQsIG1vZHVsYXIsIGNvbGxhcnMgODBjbXMgKiAxMjBjbXM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1BGJyI+UGVuPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdQRyciPlBsYXRlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdQSCciPlBpdGNoZXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1BJJyI+UGlwZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUEonIj5QdW5uZXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1BLJyI+UGFja2FnZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUEwnIj5QYWlsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdQTiciPlBsYW5rPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdQTyciPlBvdWNoPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdQUCciPlBpZWNlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdQUiciPlJlY2VwdGFjbGUsIHBsYXN0aWM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1BUJyI+UG90PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdQVSciPlRyYXk8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1BWJyI+UGlwZXMsIGluIGJ1bmRsZS9idW5jaC90cnVzczwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUFgnIj5QYWxsZXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1BZJyI+UGxhdGVzLCBpbiBidW5kbGUvYnVuY2gvdHJ1c3M8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1BaJyI+UGxhbmtzLCBpbiBidW5kbGUvYnVuY2gvdHJ1c3M8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1FBJyI+RHJ1bSwgc3RlZWwsIG5vbi1yZW1vdmFibGUgaGVhZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUUInIj5EcnVtLCBzdGVlbCwgcmVtb3ZhYmxlIGhlYWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1FDJyI+RHJ1bSwgYWx1bWluaXVtLCBub24tcmVtb3ZhYmxlIGhlYWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1FEJyI+RHJ1bSwgYWx1bWluaXVtLCByZW1vdmFibGUgaGVhZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUUYnIj5EcnVtLCBwbGFzdGljLCBub24tcmVtb3ZhYmxlIGhlYWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1FHJyI+RHJ1bSwgcGxhc3RpYywgcmVtb3ZhYmxlIGhlYWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1FIJyI+QmFycmVsLCB3b29kZW4sIGJ1bmcgdHlwZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUUonIj5CYXJyZWwsIHdvb2RlbiwgcmVtb3ZhYmxlIGhlYWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1FLJyI+SmVycmljYW4sIHN0ZWVsLCBub24tcmVtb3ZhYmxlIGhlYWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1FMJyI+SmVycmljYW4sIHN0ZWVsLCByZW1vdmFibGUgaGVhZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUU0nIj5KZXJyaWNhbiwgcGxhc3RpYywgbm9uLXJlbW92YWJsZSBoZWFkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdRTiciPkplcnJpY2FuLCBwbGFzdGljLCByZW1vdmFibGUgaGVhZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUVAnIj5Cb3gsIHdvb2RlbiwgbmF0dXJhbCB3b29kLCBvcmRpbmFyeTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUVEnIj5Cb3gsIHdvb2RlbiwgbmF0dXJhbCB3b29kLCB3aXRoIHNpZnQgcHJvb2Ygd2FsbHM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1FSJyI+Qm94LCBwbGFzdGljLCBleHBhbmRlZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUVMnIj5Cb3gsIHBsYXN0aWMsIHNvbGlkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdSRCciPlJvZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUkcnIj5SaW5nPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdSSiciPlJhY2ssIGNsb3RoaW5nIGhhbmdlcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUksnIj5SYWNrPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdSTCciPlJlZWw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1JPJyI+Um9sbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUlQnIj5SZWRuZXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1JaJyI+Um9kcywgaW4gYnVuZGxlL2J1bmNoL3RydXNzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdTQSciPlNhY2s8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1NCJyI+U2xhYjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU0MnIj5DcmF0ZSwgc2hhbGxvdzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU0QnIj5TcGluZGxlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdTRSciPlNlYS1jaGVzdDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU0gnIj5TYWNoZXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1NJJyI+U2tpZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU0snIj5DYXNlLCBza2VsZXRvbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU0wnIj5TbGlwc2hlZXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1NNJyI+U2hlZXRtZXRhbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU08nIj5TcG9vbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU1AnIj5TaGVldCwgcGxhc3RpYyB3cmFwcGluZzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU1MnIj5DYXNlLCBzdGVlbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU1QnIj5TaGVldDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU1UnIj5TdWl0Y2FzZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU1YnIj5FbnZlbG9wZSwgc3RlZWw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1NXJyI+U2hyaW5rd3JhcHBlZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU1gnIj5TZXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1NZJyI+U2xlZXZlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdTWiciPlNoZWV0cywgaW4gYnVuZGxlL2J1bmNoL3RydXNzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdUMSciPlRhYmxldDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nVEInIj5UdWI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1RDJyI+VGVhLWNoZXN0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdURCciPlR1YmUsIGNvbGxhcHNpYmxlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdURSciPlR5cmU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1RHJyI+VGFuayBjb250YWluZXIsIGdlbmVyaWM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1RJJyI+VGllcmNlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdUSyciPlRhbmssIHJlY3Rhbmd1bGFyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdUTCciPlR1Yiwgd2l0aCBsaWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1ROJyI+VGluPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdUTyciPlR1bjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nVFInIj5UcnVuazwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nVFMnIj5UcnVzczwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nVFQnIj5CYWcsIHRvdGU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1RVJyI+VHViZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nVFYnIj5UdWJlLCB3aXRoIG5venpsZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nVFcnIj5QYWxsZXQsIHRyaXdhbGw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1RZJyI+VGFuaywgY3lsaW5kcmljYWw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1RaJyI+VHViZXMsIGluIGJ1bmRsZS9idW5jaC90cnVzczwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nVUMnIj5VbmNhZ2VkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdVTiciPlVuaXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1ZBJyI+VmF0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdWRyciPkJ1bGssIGdhcyAoYXQgMTAzMSBtYmFyIGFuZCAxNdCT4oCa0JLCsEMpPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdWSSciPlZpYWw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1ZLJyI+VmFucGFjazwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nVkwnIj5CdWxrLCBsaXF1aWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1ZPJyI+QnVsaywgc29saWQsIGxhcmdlIHBhcnRpY2xlcyAo0JPigJrQstCC0Zpub2R1bGVz0JPigJrQstCC0ZwpPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdWUCciPlZhY3V1bS1wYWNrZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1ZRJyI+QnVsaywgbGlxdWVmaWVkIGdhcyAoYXQgYWJub3JtYWwgdGVtcGVyYXR1cmUvcHJlc3N1cmUpPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdWTiciPlZlaGljbGU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1ZSJyI+QnVsaywgc29saWQsIGdyYW51bGFyIHBhcnRpY2xlcyAo0JPigJrQstCC0ZpncmFpbnPQk+KAmtCy0ILRnCk8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1ZTJyI+QnVsaywgc2NyYXAgbWV0YWw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1ZZJyI+QnVsaywgc29saWQsIGZpbmUgcGFydGljbGVzICjQk+KAmtCy0ILRmnBvd2RlcnPQk+KAmtCy0ILRnCk8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1dBJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdXQiciPldpY2tlcmJvdHRsZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nV0MnIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIHN0ZWVsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdXRCciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgYWx1bWluaXVtPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdXRiciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgbWV0YWw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1dHJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCBzdGVlbCwgcHJlc3N1cmlzZWQgPiAxMCBrcGE8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1dIJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCBhbHVtaW5pdW0sIHByZXNzdXJpc2VkID4gMTAga3BhPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdXSiciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgbWV0YWwsIHByZXNzdXJlIDEwIGtwYTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nV0snIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIHN0ZWVsLCBsaXF1aWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1dMJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCBhbHVtaW5pdW0sIGxpcXVpZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nV00nIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIG1ldGFsLCBsaXF1aWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1dOJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCB3b3ZlbiBwbGFzdGljLCB3aXRob3V0IGNvYXQvbGluZXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1dQJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCB3b3ZlbiBwbGFzdGljLCBjb2F0ZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1dRJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCB3b3ZlbiBwbGFzdGljLCB3aXRoIGxpbmVyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdXUiciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgd292ZW4gcGxhc3RpYywgY29hdGVkIGFuZCBsaW5lcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nV1MnIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIHBsYXN0aWMgZmlsbTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nV1QnIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIHRleHRpbGUgd2l0aCBvdXQgY29hdC9saW5lcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nV1UnIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIG5hdHVyYWwgd29vZCwgd2l0aCBpbm5lciBsaW5lcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nV1YnIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIHRleHRpbGUsIGNvYXRlZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nV1cnIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIHRleHRpbGUsIHdpdGggbGluZXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1dYJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCB0ZXh0aWxlLCBjb2F0ZWQgYW5kIGxpbmVyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdXWSciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgcGx5d29vZCwgd2l0aCBpbm5lciBsaW5lcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nV1onIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIHJlY29uc3RpdHV0ZWQgd29vZCwgd2l0aCBpbm5lciBsaW5lcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWEEnIj5CYWcsIHdvdmVuIHBsYXN0aWMsIHdpdGhvdXQgaW5uZXIgY29hdC9saW5lcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWEInIj5CYWcsIHdvdmVuIHBsYXN0aWMsIHNpZnQgcHJvb2Y8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1hDJyI+QmFnLCB3b3ZlbiBwbGFzdGljLCB3YXRlciByZXNpc3RhbnQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1hEJyI+QmFnLCBwbGFzdGljcyBmaWxtPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdYRiciPkJhZywgdGV4dGlsZSwgd2l0aG91dCBpbm5lciBjb2F0L2xpbmVyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdYRyciPkJhZywgdGV4dGlsZSwgc2lmdCBwcm9vZjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWEgnIj5CYWcsIHRleHRpbGUsIHdhdGVyIHJlc2lzdGFudDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWEonIj5CYWcsIHBhcGVyLCBtdWx0aS13YWxsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdYSyciPkJhZywgcGFwZXIsIG11bHRpLXdhbGwsIHdhdGVyIHJlc2lzdGFudDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWUEnIj5Db21wb3NpdGUgcGFja2FnaW5nLCBwbGFzdGljIHJlY2VwdGFjbGUgaW4gc3RlZWwgZHJ1bTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWUInIj5Db21wb3NpdGUgcGFja2FnaW5nLCBwbGFzdGljIHJlY2VwdGFjbGUgaW4gc3RlZWwgY3JhdGUgYm94PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdZQyciPkNvbXBvc2l0ZSBwYWNrYWdpbmcsIHBsYXN0aWMgcmVjZXB0YWNsZSBpbiBhbHVtaW5pdW0gZHJ1bTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWUQnIj5Db21wb3NpdGUgcGFja2FnaW5nLCBwbGFzdGljIHJlY2VwdGFjbGUgaW4gYWx1bWluaXVtIGNyYXRlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdZRiciPkNvbXBvc2l0ZSBwYWNrYWdpbmcsIHBsYXN0aWMgcmVjZXB0YWNsZSBpbiB3b29kZW4gYm94PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdZRyciPkNvbXBvc2l0ZSBwYWNrYWdpbmcsIHBsYXN0aWMgcmVjZXB0YWNsZSBpbiBwbHl3b29kIGRydW08L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1lIJyI+Q29tcG9zaXRlIHBhY2thZ2luZywgcGxhc3RpYyByZWNlcHRhY2xlIGluIHBseXdvb2QgYm94PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdZSiciPkNvbXBvc2l0ZSBwYWNrYWdpbmcsIHBsYXN0aWMgcmVjZXB0YWNsZSBpbiBmaWJyZSBkcnVtPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdZSyciPkNvbXBvc2l0ZSBwYWNrYWdpbmcsIHBsYXN0aWMgcmVjZXB0YWNsZSBpbiBmaWJyZWJvYXJkIGJveDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWUwnIj5Db21wb3NpdGUgcGFja2FnaW5nLCBwbGFzdGljIHJlY2VwdGFjbGUgaW4gcGxhc3RpYyBkcnVtPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdZTSciPkNvbXBvc2l0ZSBwYWNrYWdpbmcsIHBsYXN0aWMgcmVjZXB0YWNsZSBpbiBzb2xpZCBwbGFzdGljIGJveDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWU4nIj5Db21wb3NpdGUgcGFja2FnaW5nLCBnbGFzcyByZWNlcHRhY2xlIGluIHN0ZWVsIGRydW08L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1lQJyI+Q29tcG9zaXRlIHBhY2thZ2luZywgZ2xhc3MgcmVjZXB0YWNsZSBpbiBzdGVlbCBjcmF0ZSBib3g8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1lRJyI+Q29tcG9zaXRlIHBhY2thZ2luZywgZ2xhc3MgcmVjZXB0YWNsZSBpbiBhbHVtaW5pdW0gZHJ1bTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWVInIj5Db21wb3NpdGUgcGFja2FnaW5nLCBnbGFzcyByZWNlcHRhY2xlIGluIGFsdW1pbml1bSBjcmF0ZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWVMnIj5Db21wb3NpdGUgcGFja2FnaW5nLCBnbGFzcyByZWNlcHRhY2xlIGluIHdvb2RlbiBib3g8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1lUJyI+Q29tcG9zaXRlIHBhY2thZ2luZywgZ2xhc3MgcmVjZXB0YWNsZSBpbiBwbHl3b29kIGRydW08L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1lWJyI+Q29tcG9zaXRlIHBhY2thZ2luZywgZ2xhc3MgcmVjZXB0YWNsZSBpbiB3aWNrZXJ3b3JrIGhhbXBlcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWVcnIj5Db21wb3NpdGUgcGFja2FnaW5nLCBnbGFzcyByZWNlcHRhY2xlIGluIGZpYnJlIGRydW08L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1lYJyI+Q29tcG9zaXRlIHBhY2thZ2luZywgZ2xhc3MgcmVjZXB0YWNsZSBpbiBmaWJyZWJvYXJkIGJveDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWVknIj5Db21wb3NpdGUgcGFja2FnaW5nLCBnbGFzcyByZWNlcHRhY2xlIGluIGV4cGFuZGFibGUgcGxhc3RpYyBwYWNrPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdZWiciPkNvbXBvc2l0ZSBwYWNrYWdpbmcsIGdsYXNzIHJlY2VwdGFjbGUgaW4gc29saWQgcGxhc3RpYyBwYWNrPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdaQSciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgcGFwZXIsIG11bHRpLXdhbGw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1pCJyI+QmFnLCBsYXJnZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWkMnIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIHBhcGVyLCBtdWx0aS13YWxsLCB3YXRlciByZXNpc3RhbnQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1pEJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCByaWdpZCBwbGFzdGljLCB3aXRoIHN0cnVjdHVyYWwgZXF1aXBtZW50LCBzb2xpZHM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1pGJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCByaWdpZCBwbGFzdGljLCBmcmVlc3RhbmRpbmcsIHNvbGlkczwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWkcnIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIHJpZ2lkIHBsYXN0aWMsIHdpdGggc3RydWN0dXJhbCBlcXVpcG1lbnQsIHByZXNzdXJpc2VkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdaSCciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgcmlnaWQgcGxhc3RpYywgZnJlZXN0YW5kaW5nLCBwcmVzc3VyaXNlZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWkonIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIHJpZ2lkIHBsYXN0aWMsIHdpdGggc3RydWN0dXJhbCBlcXVpcG1lbnQsIGxpcXVpZHM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1pLJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCByaWdpZCBwbGFzdGljLCBmcmVlc3RhbmRpbmcsIGxpcXVpZHM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1pMJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCBjb21wb3NpdGUsIHJpZ2lkIHBsYXN0aWMsIHNvbGlkczwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWk0nIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIGNvbXBvc2l0ZSwgZmxleGlibGUgcGxhc3RpYywgc29saWRzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdaTiciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgY29tcG9zaXRlLCByaWdpZCBwbGFzdGljLCBwcmVzc3VyaXNlZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWlAnIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIGNvbXBvc2l0ZSwgZmxleGlibGUgcGxhc3RpYywgcHJlc3N1cmlzZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1pRJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCBjb21wb3NpdGUsIHJpZ2lkIHBsYXN0aWMsIGxpcXVpZHM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1pSJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCBjb21wb3NpdGUsIGZsZXhpYmxlIHBsYXN0aWMsIGxpcXVpZHM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1pTJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCBjb21wb3NpdGU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1pUJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCBmaWJyZWJvYXJkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdaVSciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgZmxleGlibGU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1pWJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCBtZXRhbCwgb3RoZXIgdGhhbiBzdGVlbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWlcnIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIG5hdHVyYWwgd29vZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWlgnIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIHBseXdvb2Q8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1pZJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCByZWNvbnN0aXR1dGVkIHdvb2Q8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOm90aGVyd2lzZT4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iJFBhY2thZ2luZ1R5cGUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6b3RoZXJ3aXNlPgogICAgICAgICAgICAgICAgPC94c2w6Y2hvb3NlPgogICAgICAgIDwveHNsOnRlbXBsYXRlPgogICAgICAgIDx4c2w6dGVtcGxhdGUgbWF0Y2g9Ii8iPgogICAgICAgIDx4c2w6Y29tbWVudD5lRmluYW5zINCV0ZthYmxvbiBUYXNhctCUwrFtIEFyYWPQlMKxINCUwrBsZSBIYXrQlMKxcmxhbm3QlMKx0JXRn3TQlMKxci48L3hzbDpjb21tZW50PgogICAgICAgIDxodG1sPgogICAgICAgIDxoZWFkPgo8bWV0YSBodHRwLWVxdWl2PSJYLVVBLUNvbXBhdGlibGUiIGNvbnRlbnQ9IklFPWVkZ2UiIC8+CiAgICAgICAgPHRpdGxlPmUtRmF0dXJhPC90aXRsZT4KPHN0eWxlIHR5cGU9InRleHQvY3NzIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5waCB7b3ZlcmZsb3c6aGlkZGVuO21heC1oZWlnaHQ6MjUwcHg7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLnBoNyB7dGV4dC1hbGlnbjpjZW50ZXI7bWFyZ2luLWJvdHRvbToxOHB4O30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5waDggaW1nIHttYXJnaW4tYm90dG9tOiAxOHB4O30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5waDEwIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZmxvYXQ6IGxlZnQ7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHdpZHRoOiAyOTVweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgbWFyZ2luLXRvcDogMTRweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgbWFyZ2luLXJpZ2h0OiAxMnB4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBwYWRkaW5nOiA4cHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYm9keSB7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG1hcmdpbjogN3B4IDAgMTBweCAwOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXh0LWFsaWduOiBjZW50ZXI7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJhY2tncm91bmQtY29sb3I6ICNCQkJCQkI7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGZvbnQtZmFtaWx5OkFyaWFsLCBIZWx2ZXRpY2EsIHNhbnMtc2VyaWY7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGZvbnQtc2l6ZTogMTJweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYmFja2dyb3VuZC1pbWFnZTogdXJsKGRhdGE6aW1hZ2UvcG5nO2Jhc2U2NCxpVkJPUncwS0dnb0FBQUFOU1VoRVVnQUFBQmNBQUFBWENBWUFBQURnS3RTZ0FBQUJDRWxFUVZSNDJwMVZXdzdESUF6TC9TL1oxMEUycFpLUlo4VUI5b0VLeGFTSmJkSjRudWR6SE1jWXVjWTczVnZCNVBvOHozY2UxM1VOTUIvU0Fkek9YdHozM1dhR3d6bHlYdUh6SGZEQXZjR3JBMXJ5TGoyWWh3dmFhY0ZVZ0YvTzJnYlhJRndWenp0OUJpMU9URGRYUi9DK1ZoRmRnQjNLK0lrS1k4WG5TbE5tcUIrcnFnOCt4SjVuY1diY01nNjJ6QVNpeXN4Vm9UcDBleGtuVnF3MzQ5aFZFYXE2NDd6eXVEUEFDRDdMcWhKdTlhWkc5Y1hPOXhCc2RnOSszQUtGWGE5Z0I2MTB4cGNXN1dUVmdJTTBBU2RtU1l2TG1DK1BvMlZMVUNjeVY4Rjl2QXplWmJ2YnpMUjVoZXZGREtyV2pPTy9FS3I1dWFILy9JQVZ4M29NV2hnODQ3MTY3eXo4QllKTWYyaXZWYkxKQUFBQUFFbEZUa1N1UW1DQyk7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0YWJsZSB7Zm9udC1zaXplOjEycHg7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmRvY3VtZW50Q29udGFpbmVyIGEge3BvaW50ZXItZXZlbnRzOm5vbmU7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmRvY3VtZW50Q29udGFpbmVyLAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmRvY3VtZW50Q29udGFpbmVyT3V0ZXIgewogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG1hcmdpbjogMCBhdXRvOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIEBtZWRpYSBzY3JlZW4gewogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYm9keSB7Y29sb3I6ICM2NjY7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmRvY3VtZW50Q29udGFpbmVyIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBtYXgtd2lkdGg6IDk0NXB4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG1pbi13aWR0aDogODUwcHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgb3ZlcmZsb3c6aGlkZGVuOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRleHQtYWxpZ246IGxlZnQ7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYm94LXNpemluZzogYm9yZGVyLWJveDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBkaXNwbGF5OmlubGluZS1ibG9jazsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAtd2Via2l0LWJveC1zaGFkb3c6IDAgMXB4IDRweCByZ2JhKDAsIDAsIDAsIDAuMyksIDAgMCA0MHB4IHJnYmEoMCwgMCwgMCwgMC4xKSBpbnNldDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAtbW96LWJveC1zaGFkb3c6IDAgMXB4IDRweCByZ2JhKDAsIDAsIDAsIDAuMyksIDAgMCA0MHB4IHJnYmEoMCwgMCwgMCwgMC4xKSBpbnNldDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBib3gtc2hhZG93OiAwIDFweCA0cHggcmdiYSgwLCAwLCAwLCAwLjMpLCAwIDAgNDBweCByZ2JhKDAsIDAsIDAsIDAuMSkgaW5zZXQ7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYmFja2dyb3VuZC1jb2xvcjogd2hpdGU7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcG9zaXRpb246IHJlbGF0aXZlOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBhZGRpbmc6IDIwcHggMjBweCAyMHB4IDI4cHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAuZG9jdW1lbnRDb250YWluZXI6YmVmb3JlLCAuZG9jdW1lbnRDb250YWluZXI6YWZ0ZXIgewogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjb250ZW50OiAiIjsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcG9zaXRpb246IGFic29sdXRlOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB6LWluZGV4OiAtMTsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLXdlYmtpdC1ib3gtc2hhZG93OiAwIDAgMjBweCByZ2JhKDAsMCwwLDAuOCk7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC1tb3otYm94LXNoYWRvdzogMCAwIDIwcHggcmdiYSgwLDAsMCwwLjgpOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBib3gtc2hhZG93OiAwIDAgMjBweCByZ2JhKDAsMCwwLDAuOCk7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRvcDogNTAlOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBib3R0b206IDA7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGxlZnQ6IDEwcHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJpZ2h0OiAxMHB4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAtbW96LWJvcmRlci1yYWRpdXM6IDEwMHB4IC8gMTBweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYm9yZGVyLXJhZGl1czogMTAwcHggLyAxMHB4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfQoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5kb2N1bWVudENvbnRhaW5lcjphZnRlciB7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJpZ2h0OiAxMHB4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBsZWZ0OiBhdXRvOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAtd2Via2l0LXRyYW5zZm9ybTogc2tldyg4ZGVnKSByb3RhdGUoM2RlZyk7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC1tb3otdHJhbnNmb3JtOiBza2V3KDhkZWcpIHJvdGF0ZSgzZGVnKTsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLW1zLXRyYW5zZm9ybTogc2tldyg4ZGVnKSByb3RhdGUoM2RlZyk7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC1vLXRyYW5zZm9ybTogc2tldyg4ZGVnKSByb3RhdGUoM2RlZyk7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRyYW5zZm9ybTogc2tldyg4ZGVnKSByb3RhdGUoM2RlZyk7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9ICAgICAgICAKCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAjdXN0Qm9sdW17CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG1hcmdpbi10b3A6IDI3cHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG1hcmdpbi1ib3R0b206IDE1cHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5lZmF0dXJhTG9nbyB7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGV4dC1hbGlnbjpjZW50ZXI7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmVmYXR1cmFMb2dvIGltZ3sKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB3aWR0aDo3MHB4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5lZmF0dXJhTG9nbyBoMXsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBmb250LXNpemU6IDE0cHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgbGluZS1oZWlnaHQ6IDE0cHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgbWFyZ2luOiA0cHggMCAwIDA7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmt1dHUge3ZlcnRpY2FsLWFsaWduOiB0b3A7fQoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmt1dHUgdGFibGV7ZmxvYXQ6bm9uZTt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5nb25kZXJpY2kgewogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGRpc3BsYXk6IGlubGluZS1ibG9jazsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBwYWRkaW5nOiA4cHggOHB4IDhweCAwOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJveC1zaXppbmc6IGJvcmRlci1ib3g7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmdvbmRlcmljaSAucGFydHlOYW1lIHtmb250LXdlaWdodDpib2xkO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmFsaWNpIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB3aWR0aDogMzcwcHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcGFkZGluZzogOHB4IDRweCA0cHggMDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBib3gtc2l6aW5nOiBib3JkZXItYm94OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5hbGljaSAuY3VzdG9tZXJUaXRsZSB7Zm9udC13ZWlnaHQ6Ym9sZDt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5hbGljaSAua3V0dSB7IGJvcmRlcjoxcHggc29saWQgI0NDQzsgYmFja2dyb3VuZC1jb2xvcjojRjRGNEY0O30KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNFVFROIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBtYXJnaW4tdG9wOiA3cHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcGFkZGluZzogOHB4IDRweCA0cHggMDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfQoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI2Rlc3BhdGNoVGFibGUgLnBsYWNlSG9sZGVyLmNvbXBhbnlMb2dvIGltZyB7bWFyZ2luLWJvdHRvbToxOHB4O30KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICN0b3BsYW1sYXJDb250YWluZXIge292ZXJmbG93OmhpZGRlbjt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC50b3BsYW1sYXIge2Zsb2F0OnJpZ2h0O3dpZHRoOiAxMDAlO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLnRvcGxhbWxhciB0ciB7dGV4dC1hbGlnbjpyaWdodDt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC50b3BsYW1sYXIgdGgge2ZvbnQtd2VpZ2h0Om5vcm1hbDt0ZXh0LWFsaWduOnJpZ2h0O30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLnRvcGxhbWxhciB0YWJsZSB7d2lkdGg6MjM4cHg7bWFyZ2luLXRvcDogMTRweDtib3JkZXItc3BhY2luZzowO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLnRvcGxhbWxhciB0YWJsZSB0ZCB7Zm9udC13ZWlnaHQ6IGJvbGQ7IHdpZHRoOjI1JTsgd2hpdGUtc3BhY2U6bm93cmFwO21pbi13aWR0aDogNTVweDsgdmVydGljYWwtYWxpZ246IHRvcDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAudG9wbGFtbGFyIHRhYmxlIHRoLAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAudG9wbGFtbGFyIHRhYmxlIHRkewogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJvcmRlci1ib3R0b206IDFweCBzb2xpZCAjY2NjOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJvcmRlci1yaWdodDogMXB4IHNvbGlkICNjY2M7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYm9yZGVyLWxlZnQ6IDFweCBzb2xpZCAjY2NjOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBhZGRpbmc6M3B4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC50b3BsYW1sYXIgdGFibGUgdGgge3doaXRlLXNwYWNlOm5vd3JhcDtib3JkZXItcmlnaHQ6IG5vbmU7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAudG9wbGFtbGFyIHRhYmxlIHRoLmlzLWxvbmctdHJ1ZSwKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI21hbEhpem1ldFRhYmxvc3UgPiB0Ym9keSA+IHRyID4gdGQgLmlzLWxvbmctdHJ1ZSB7d2hpdGUtc3BhY2U6cHJlLWxpbmU7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAudG9wbGFtbGFyIHRhYmxlIHRyOmZpcnN0LWNoaWxkIHRoLAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAudG9wbGFtbGFyIHRhYmxlIHRyOmZpcnN0LWNoaWxkIHRkewogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBib3JkZXItdG9wOjFweCBzb2xpZCAjY2NjOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC50b3BsYW1sYXIgdGFibGUgdGQgc3BhbiB7Zm9udC13ZWlnaHQ6bm9ybWFsO2ZvbnQtc2l6ZTogMTJweDtjb2xvcjogIzdDN0M3Qzt9CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ci5wYXlhYmxlQW1vdW50IHRoOmZpcnN0LWNoaWxkIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBiYWNrZ3JvdW5kLWNvbG9yOiAjZjZmNmY2OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ci5wYXlhYmxlQW1vdW50IHRkIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYmFja2dyb3VuZC1jb2xvcjogI2Y2ZjZmNjsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfQoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLnRvcGxhbWxhciA+IGRpdiB7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZGlzcGxheTogaW5saW5lLWJsb2NrOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC50b3BsYW1UYWJsb3sKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBtYXJnaW4tbGVmdDogMzFweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBmbG9hdDpyaWdodDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI25vdGxhciB7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG1hcmdpbi10b3A6IDE0cHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJvcmRlci10b3A6IDFweCBzb2xpZCAjREREOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBvdmVyZmxvdzogaGlkZGVuOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBwYWRkaW5nLXRvcDogOHB4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBwYWRkaW5nLWJvdHRvbTogMnB4OwoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNub3RsYXIgdGFibGUge2JvcmRlcjpub25lO2JhY2tncm91bmQ6bm9uZTt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAuY2xlYXJmaXg6YmVmb3JlLAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmNsZWFyZml4OmFmdGVyIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgY29udGVudDoiIjsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZGlzcGxheTp0YWJsZTsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAuYm94e2Rpc3BsYXk6IGlubGluZS1ibG9jazsqZGlzcGxheTogaW5saW5lO3pvb206IDE7d2lkdGg6IDMzJTsgYm94LXNpemluZzpib3JkZXItYm94OyB2ZXJ0aWNhbC1hbGlnbjogdG9wO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNiMXt3aWR0aDogNDAlO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNiMnt3aWR0aDogMjUlO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNiM3t3aWR0aDogMzUlO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNrdW55ZSB7Ym9yZGVyLXNwYWNpbmc6MDt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAja3VueWUgdHJ7IGJvcmRlci1ib3R0b206IDFweCBkb3R0ZWQgI0NDQzt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAja3VueWUgdGR7IGJvcmRlcjoxcHggc29saWQgI0NDQztib3JkZXItdG9wOiBub25lO3BhZGRpbmc6M3B4O3dpZHRoOiAxMDAlO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNrdW55ZSB0aHt2ZXJ0aWNhbC1hbGlnbjp0b3A7Zm9udC13ZWlnaHQ6Ym9sZDtwYWRkaW5nOjNweDt3aGl0ZS1zcGFjZTogbm93cmFwO2JvcmRlcjoxcHggc29saWQgI2NjYztib3JkZXItdG9wOiBub25lO2JvcmRlci1yaWdodDogbm9uZTt0ZXh0LWFsaWduOmxlZnQ7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI2t1bnllIHRyOmZpcnN0LWNoaWxkIHRke2JvcmRlci10b3A6IDFweCBzb2xpZCAjY2NjO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNrdW55ZSB0cjpmaXJzdC1jaGlsZCB0aHtib3JkZXItdG9wOiAxcHggc29saWQgI2NjYzt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAja3VueWUgdGQ6bnRoLWNoaWxkKDIpIHt3b3JkLWJyZWFrOiBicmVhay1hbGw7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLnNhdGlybGFyIHttYXJnaW4tdG9wOjIwcHg7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI21hbEhpem1ldFRhYmxvc3UKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgd2lkdGg6MTAwJTsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZm9udC1mYW1pbHk6ICJMdWNpZGEgU2FucyBVbmljb2RlIiwgIkx1Y2lkYSBHcmFuZGUiLCBTYW5zLVNlcmlmOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBiYWNrZ3JvdW5kOiAjZmZmOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBib3JkZXItY29sbGFwc2U6IGNvbGxhcHNlOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXh0LWFsaWduOiByaWdodDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNtYWxIaXptZXRUYWJsb3N1ID4gdGJvZHkgPiB0ciA+IHRoCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGZvbnQtd2VpZ2h0OiBub3JtYWw7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBhZGRpbmc6IDJweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGV4dC1hbGlnbjpyaWdodDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZm9udC1zaXplOiAxMnB4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjb2xvcjogYmxhY2s7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBhZGRpbmctbGVmdDogM3B4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBib3JkZXItYm90dG9tOiAycHggc29saWQgI0FBQUFBQTsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYmFja2dyb3VuZC1jb2xvcjogI0RGREZERjsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYm9yZGVyLXJpZ2h0OiAxcHggc29saWQgI0I4QjhCODsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYm9yZGVyLXRvcDogMXB4IHNvbGlkICNDNUM1QzU7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBhZGRpbmctcmlnaHQ6IDVweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdmVydGljYWwtYWxpZ246IG1pZGRsZTsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgbWluLWhlaWdodDogMzVweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNtYWxIaXptZXRUYWJsb3N1ID4gdGJvZHkgPiB0cjpmaXJzdC1jaGlsZCA+IHRoOmZpcnN0LWNoaWxkIHtib3JkZXItbGVmdDogMXB4IHNvbGlkICNCOEI4Qjg7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI21hbEhpem1ldFRhYmxvc3UgPiB0Ym9keSA+IHRyOmZpcnN0LWNoaWxkID4gdGgubWhDb2x1bW4ge21pbi13aWR0aDo3MnB4O30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNtYWxIaXptZXRUYWJsb3N1ID4gdGJvZHkgPiB0aCA+IC50aFRvcFRpdGxlIHt0ZXh0LWFsaWduOmNlbnRlcjt3aWR0aDogODlweDt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAjbWFsSGl6bWV0VGFibG9zdSA+IHRib2R5ID4gdGggLnRoU3ViVGl0bGUge3dpZHRoOiA0N3B4OyBkaXNwbGF5OiBpbmxpbmUtYmxvY2s7dGV4dC1hbGlnbjogcmlnaHQ7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI21hbEhpem1ldFRhYmxvc3UgPiB0Ym9keSA+IHRoIC50aFN1YlRpdGxlLkhGIHt3aWR0aDozNnB4O30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNtYWxIaXptZXRUYWJsb3N1ID4gdGJvZHkgPiB0ciA+IHRoLmFsaWduTGVmdCB7dGV4dC1hbGlnbjpsZWZ0O3dpZHRoOiAyMiU7fQoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNtYWxIaXptZXRUYWJsb3N1ID4gdGJvZHkgPiB0ciA+IHRkIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZm9udC1zaXplOiAxMnB4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBwYWRkaW5nLWxlZnQ6M3B4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBib3JkZXItYm90dG9tOiAxcHggc29saWQgI2NjYzsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgY29sb3I6ICMwMDA7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJvcmRlci1yaWdodDogMXB4IHNvbGlkICNjY2M7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBhZGRpbmctcmlnaHQ6IDNweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGV4dC1hbGlnbjpyaWdodDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaGVpZ2h0OjI1cHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAjbWFsSGl6bWV0VGFibG9zdSA+IHRib2R5ID4gdHIgPiB0ZC5pc2tvbnRvT3Jhbmkge3BhZGRpbmctbGVmdDowOyBwYWRkaW5nLXJpZ2h0OjA7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI21hbEhpem1ldFRhYmxvc3UgPiB0Ym9keSA+IHRyID4gdGQuaXNrb250b09yYW5pIHRke3RleHQtYWxpZ246IGNlbnRlcjt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAjbWFsSGl6bWV0VGFibG9zdSA+IHRib2R5ID4gdHI6aG92ZXIgPiB0ZCB7Ym9yZGVyLXJpZ2h0OiAxcHggc29saWQgIzk2OTY5Njtib3JkZXItYm90dG9tOiAxcHggc29saWQgIzk2OTY5Njtib3JkZXItdG9wOiAxcHggc29saWQgIzk2OTY5Njt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAjbWFsSGl6bWV0VGFibG9zdSA+IHRib2R5ID4gdHIgPiB0ZC53cmFwIHt3aGl0ZS1zcGFjZTpub3JtYWw7dGV4dC1hbGlnbjpsZWZ0O30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNtYWxIaXptZXRUYWJsb3N1ID4gdGJvZHkgPiB0ciA+IHRkOmZpcnN0LWNoaWxkLAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI21hbEhpem1ldFRhYmxvc3UgPiB0Ym9keSA+IHRyID4gdGg6Zmlyc3QtY2hpbGQge2JvcmRlci1sZWZ0OiAxcHggc29saWQgI0I4QjhCODt9CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI21hbEhpem1ldFRhYmxvc3UgPiB0Ym9keSA+IHRyOmhvdmVyID4gdGQKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYmFja2dyb3VuZC1jb2xvcjogI0QxRDFEMSAhaW1wb3J0YW50OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjdXJzb3I6ZGVmYXVsdDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNtYWxIaXptZXRUYWJsb3N1ID4gdGJvZHkgPiB0cjpudGgtY2hpbGQoZXZlbikgPiB0ZCB7YmFja2dyb3VuZC1jb2xvcjogI0ZGRn0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNtYWxIaXptZXRUYWJsb3N1ID4gdGJvZHkgPiB0cjpudGgtY2hpbGQob2RkKSA+IHRkIHtiYWNrZ3JvdW5kLWNvbG9yOiAjRUVFfQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLnN1bVZhbHVlIHt3aGl0ZS1zcGFjZTpub3dyYXA7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmlza29udG9PcmFuaUhlYWRlciwKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5pc2tvbnRvT3JhbmlEZWdlcmxlciB7d2lkdGg6MTAwJTtib3JkZXItc3BhY2luZzowO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5pc2tvbnRvT3JhbmlIZWFkZXIgdGQge2JvcmRlci10b3A6IDFweCBzb2xpZCAjOTY5Njk2O30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5pc2tvbnRvT3JhbmlIZWFkZXIgdGQsCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAuaXNrb250b09yYW5pRGVnZXJsZXIgdGQKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHtib3JkZXItbGVmdDogMXB4IHNvbGlkICM5Njk2OTY7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmlza29udG9PcmFuaUhlYWRlciB0ZDpmaXJzdC1jaGlsZCwKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5pc2tvbnRvT3JhbmlEZWdlcmxlciB0ZDpmaXJzdC1jaGlsZCB7Ym9yZGVyLWxlZnQ6bm9uZTt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAjaWhyYWNhdEJpbGdpbGVyaXsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYm9yZGVyOjFweCBzb2xpZCAjQ0NDOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBtYXJnaW4tdG9wOjEwcHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBhZGRpbmc6MTBweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcG9zaXRpb246cmVsYXRpdmU7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIEBtZWRpYSBwcmludCB7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJvZHkge2NvbG9yOiAjMDAwO3RleHQtYWxpZ246IGxlZnQ7YmFja2dyb3VuZDpub25lO2JhY2tncm91bmQtY29sb3I6I2ZmZmZmZjttYXJnaW46MDt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5kb2N1bWVudENvbnRhaW5lciB7cGFkZGluZzowO21pbi1oZWlnaHQ6IGluaXRpYWw7d2lkdGg6IDg0NXB4ICFpbXBvcnRhbnQ7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAjbWFsSGl6bWV0VGFibG9zdSB7d2lkdGg6IDg0NXB4O30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgCjwvc3R5bGU+Cgo8L2hlYWQ+Cjxib2R5PgogICAgICAgIDxkaXYgY2xhc3M9ImRvY3VtZW50Q29udGFpbmVyT3V0ZXIiPgogICAgICAgICAgICAgICAgPGRpdiBjbGFzcz0iZG9jdW1lbnRDb250YWluZXIiPgogICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9IiRYTUwiPgogICAgICAgIDxkaXYgaWQ9InVzdEJvbHVtIj4KICAgICAgICAKICAgICAgICA8ZGl2IGlkPSJiMSIgY2xhc3M9ImJveCI+CiAgICAgICAgCiAgICAgICAgPGRpdiBpZD0iQWNjb3VudGluZ1N1cHBsaWVyUGFydHkiIGNsYXNzPSJnb25kZXJpY2kga3V0dSI+CiAgICAgICAgCiAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Im4xOkludm9pY2UiPgogICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6QWNjb3VudGluZ1N1cHBsaWVyUGFydHkiPgogICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpQYXJ0eSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDxkaXYgY2xhc3M9InBhcnR5TmFtZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJub3QoY2FjOlBlcnNvbi9jYmM6Rmlyc3ROYW1lID0nJykgb3Igbm90KGNhYzpQZXJzb24vY2JjOkZhbWlseU5hbWUgPScnKSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOlBlcnNvbiI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6VGl0bGUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6Rmlyc3ROYW1lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOk1pZGRsZU5hbWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6RmFtaWx5TmFtZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpOYW1lU3VmZml4Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0ibm90KGNhYzpQYXJ0eU5hbWUvY2JjOk5hbWUgPScnKSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2FjOlBhcnR5TmFtZS9jYmM6TmFtZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICA8L2Rpdj4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDwveHNsOmZvci1lYWNoPgo8L3hzbDpmb3ItZWFjaD4KCjx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJuMTpJbnZvaWNlIj4KICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOkFjY291bnRpbmdTdXBwbGllclBhcnR5Ij4KICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6UGFydHkiPgogICAgICAgICAgICAgICAgICAgICAgICA8ZGl2IGNsYXNzPSJhZGRyZXMiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6UG9zdGFsQWRkcmVzcyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOlJlZ2lvbiI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpTdHJlZXROYW1lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOkJ1aWxkaW5nTmFtZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjYmM6QnVpbGRpbmdOdW1iZXIiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IE5vOjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpCdWlsZGluZ051bWJlciI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpSb29tIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkthcNCUwrEgTm86PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNiYzpEaXN0cmljdCAhPScnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpEaXN0cmljdCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY2JjOlBvc3RhbFpvbmUgIT0gJycgIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpQb3N0YWxab25lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOkNpdHlTdWJkaXZpc2lvbk5hbWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IC8gPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6Q2l0eU5hbWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgPC9kaXY+CiAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICA8L3hzbDpmb3ItZWFjaD4KPC94c2w6Zm9yLWVhY2g+Cgo8eHNsOmZvci1lYWNoIHNlbGVjdD0ibjE6SW52b2ljZSI+CiAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpBY2NvdW50aW5nU3VwcGxpZXJQYXJ0eSI+CiAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOlBhcnR5Ij4KICAgICAgICAgICAgICAgICAgICAgICAgPGRpdiBjbGFzcz0idGVsRmF4Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOkNvbnRhY3QiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjYmM6VGVsZXBob25lICE9JyciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VGVsOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6VGVsZXBob25lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNiYzpUZWxlZmF4ICE9JyciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEZha3M6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpUZWxlZmF4Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvZGl2PgogICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgPC94c2w6Zm9yLWVhY2g+CjwveHNsOmZvci1lYWNoPgoKPHhzbDpmb3ItZWFjaAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6QWNjb3VudGluZ1N1cHBsaWVyUGFydHkvY2FjOlBhcnR5L2NiYzpXZWJzaXRlVVJJIj4KICAgICAgICA8ZGl2IGNsYXNzPSJXZWJzaXRlVVJJIj4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5XZWIgU2l0ZXNpOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4iLz4KICAgICAgICA8L2Rpdj4KPC94c2w6Zm9yLWVhY2g+Cgo8eHNsOmZvci1lYWNoCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpBY2NvdW50aW5nU3VwcGxpZXJQYXJ0eS9jYWM6UGFydHkvY2FjOkNvbnRhY3QvY2JjOkVsZWN0cm9uaWNNYWlsIj4KICAgICAgICA8ZGl2IGNsYXNzPSJlTWFpbCI+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+ZS1Qb3N0YTogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgPC9kaXY+CjwveHNsOmZvci1lYWNoPgoKPGRpdiBjbGFzcz0idGF4T2ZmaWNlIj4KICAgICAgICA8eHNsOnRleHQ+VmVyZ2kgRGFpcmVzaTogPC94c2w6dGV4dD4KICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkFjY291bnRpbmdTdXBwbGllclBhcnR5L2NhYzpQYXJ0eS9jYWM6UGFydHlUYXhTY2hlbWUvY2FjOlRheFNjaGVtZS9jYmM6TmFtZSIKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPgo8L2Rpdj4KCjx4c2w6Zm9yLWVhY2gKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpBY2NvdW50aW5nU3VwcGxpZXJQYXJ0eS9jYWM6UGFydHkvY2FjOlBhcnR5SWRlbnRpZmljYXRpb24vY2JjOklEIj4KICAgICAgICA8eHNsOmlmIHRlc3Q9IkBzY2hlbWVJRCAhPSAnTVVTVEVSSU5PJyI+CiAgICAgICAgICAgICAgICA8ZGl2IGNsYXNzPSJwYXJ0eUlEIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpjaG9vc2U+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkBzY2hlbWVJRCA9ICdUSUNBUkVUU0lDSUxOTyciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRpY2FyZXQgU2ljaWwgTm88L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkBzY2hlbWVJRCA9ICdNRVJTSVNOTyciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0Pk1FUlPQlMKwUyBObzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOm90aGVyd2lzZT4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJAc2NoZW1lSUQiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpvdGhlcndpc2U+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmNob29zZT4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PjogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4iLz4KICAgICAgICAgICAgICAgIDwvZGl2PgogICAgICAgIDwveHNsOmlmPgo8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAoKPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkFjY291bnRpbmdTdXBwbGllclBhcnR5L2NhYzpQYXJ0eS9jYWM6QWdlbnRQYXJ0eSI+Cjx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJuMTpJbnZvaWNlIj4KICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOkFjY291bnRpbmdTdXBwbGllclBhcnR5Ij4KICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6UGFydHkiPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOkFnZW50UGFydHkiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxkaXYgY2xhc3M9InN1YmUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PtCV0Zt1YmU6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2FjOlBhcnR5TmFtZS9jYmM6TmFtZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6QWNjb3VudGluZ1N1cHBsaWVyUGFydHkvY2FjOlBhcnR5L2NhYzpBZ2VudFBhcnR5L2NhYzpQYXJ0eUlkZW50aWZpY2F0aW9uIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpjaG9vc2U+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9ImNiYzpJRC9Ac2NoZW1lSUQgPSAnU1VCRU5PJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IC0gPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYmM6SUQiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpjaG9vc2U+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L2Rpdj4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICA8L3hzbDpmb3ItZWFjaD4KPC94c2w6Zm9yLWVhY2g+CjwveHNsOmlmPiAKCjwvZGl2PgoKPGRpdiBjbGFzcz0iYWxpY2kga3V0dSI+CiAgICAgICAgCiAgICAgICAgPGRpdiBjbGFzcz0iY3VzdG9tZXJUaXRsZSI+CiAgICAgICAgPHhzbDp0ZXh0PlNBWUlOPC94c2w6dGV4dD4KPC9kaXY+Cgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6UHJvZmlsZUlEICE9ICdZT0xDVUJFUkFCRVJGQVRVUkEnIGFuZCAvL24xOkludm9pY2UvY2JjOlByb2ZpbGVJRCA9ICdJSFJBQ0FUJyI+CiAgICAgICAgPGI+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VW52YW7QlMKxOiA8L3hzbDp0ZXh0PgogICAgICAgIDwvYj4KICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpCdXllckN1c3RvbWVyUGFydHkvY2FjOlBhcnR5L2NhYzpQYXJ0eU5hbWUvY2JjOk5hbWUiPgogICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4iIC8+CiAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgPGJyIC8+CiAgICAgICAgPGI+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+0JTCsGxpOiA8L3hzbDp0ZXh0PgogICAgICAgIDwvYj4KICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpCdXllckN1c3RvbWVyUGFydHkvY2FjOlBhcnR5L2NhYzpQb3N0YWxBZGRyZXNzL2NiYzpDaXR5TmFtZSI+CiAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLiIvPgogICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDxiciAvPgogICAgICAgIDxiPgogICAgICAgICAgICAgICAgPHhzbDp0ZXh0PtCUwrBs0JPCp2VzaTogPC94c2w6dGV4dD4KICAgICAgICA8L2I+CiAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6QnV5ZXJDdXN0b21lclBhcnR5L2NhYzpQYXJ0eS9jYWM6UG9zdGFsQWRkcmVzcy9jYmM6Q2l0eVN1YmRpdmlzaW9uTmFtZSI+CiAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLiIvPgogICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDxiciAvPgogICAgICAgIDxiPgogICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlNva2FrOiA8L3hzbDp0ZXh0PgogICAgICAgIDwvYj4KICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpCdXllckN1c3RvbWVyUGFydHkvY2FjOlBhcnR5L2NhYzpQb3N0YWxBZGRyZXNzL2NiYzpTdHJlZXROYW1lIj4KICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgPGJyIC8+CiAgICAgICAgPGI+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+0JPRmmxrZXNpOiA8L3hzbDp0ZXh0PgogICAgICAgIDwvYj4KICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpCdXllckN1c3RvbWVyUGFydHkvY2FjOlBhcnR5L2NhYzpQb3N0YWxBZGRyZXNzL2NhYzpDb3VudHJ5L2NiYzpOYW1lIj4KICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgPGJyIC8+CiAgICAgICAgPGI+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+UmVzbWkgVW52YW7QlMKxOiA8L3hzbDp0ZXh0PgogICAgICAgIDwvYj4KICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpCdXllckN1c3RvbWVyUGFydHkvY2FjOlBhcnR5L2NhYzpQYXJ0eUxlZ2FsRW50aXR5L2NiYzpSZWdpc3RyYXRpb25OYW1lIj4KICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgPGJyIC8+CiAgICAgICAgPGI+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VmVyZ2kgTnVtYXJhc9CUwrE6IDwveHNsOnRleHQ+CiAgICAgICAgPC9iPgogICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkJ1eWVyQ3VzdG9tZXJQYXJ0eS9jYWM6UGFydHkvY2FjOlBhcnR5TGVnYWxFbnRpdHkvY2JjOkNvbXBhbnlJRCI+CiAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLiIvPgogICAgICAgIDwveHNsOmZvci1lYWNoPgoKPC94c2w6aWY+Cjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpQcm9maWxlSUQgIT0gJ1lPTENVQkVSQUJFUkZBVFVSQScgYW5kIC8vbjE6SW52b2ljZS9jYmM6UHJvZmlsZUlEICE9ICdJSFJBQ0FUJyI+CiAgICAgICAgPGRpdiBjbGFzcz0icGFydHlOYW1lIj4KICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJuMTpJbnZvaWNlIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpBY2NvdW50aW5nQ3VzdG9tZXJQYXJ0eSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpQYXJ0eSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8ZGl2IGNsYXNzPSJwYXJ0eU5hbWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Im5vdChjYWM6UGVyc29uL2NiYzpGaXJzdE5hbWUgPScnKSBvciBub3QoY2FjOlBlcnNvbi9jYmM6RmFtaWx5TmFtZSA9JycpIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOlBlcnNvbiI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOlRpdGxlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOkZpcnN0TmFtZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpNaWRkbGVOYW1lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOkZhbWlseU5hbWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6TmFtZVN1ZmZpeCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Im5vdChjYWM6UGFydHlOYW1lL2NiYzpOYW1lID0nJykiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYWM6UGFydHlOYW1lL2NiYzpOYW1lIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9kaXY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgPC9kaXY+CjwveHNsOmlmPgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6UHJvZmlsZUlEID0gJ1lPTENVQkVSQUJFUkZBVFVSQSciPgogICAgICAgIDxkaXYgY2xhc3M9InBhcnR5TmFtZSI+CiAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0ibjE6SW52b2ljZSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6QWNjb3VudGluZ0N1c3RvbWVyUGFydHkiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6UGFydHkiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5VbnZhbtCUwrE6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYWM6UGFydHlOYW1lL2NiYzpOYW1lIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOlBvc3RhbEFkZHJlc3MiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8Yj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlNva2FrIEFk0JTCsTogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2JjOlN0cmVldE5hbWUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5CaW5hIEFk0JTCsTogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2JjOkJ1aWxkaW5nTmFtZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8Yj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkJpbmEgTm86IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvYj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNiYzpCdWlsZGluZ051bWJlciIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8Yj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PtCUwrBs0JPCp2U6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvYj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNiYzpDaXR5U3ViZGl2aXNpb25OYW1lIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+0JTCsGw6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvYj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNiYzpDaXR5TmFtZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8Yj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlBvc3RhIEtvZHU6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvYj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNiYzpQb3N0YWxab25lIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+0JPRmmxrZTogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2FjOkNvdW50cnkvY2JjOk5hbWUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VmVyZ2kgRGFpcmVzaTogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvYj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYWM6UGFydHlUYXhTY2hlbWUvY2FjOlRheFNjaGVtZS9jYmM6TmFtZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6Q29udGFjdCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VGVsZWZvbjogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2JjOlRlbGVwaG9uZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8Yj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkVwb3N0YTogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2JjOkVsZWN0cm9uaWNNYWlsIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICA8L2Rpdj4KPC94c2w6aWY+Cgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6UHJvZmlsZUlEICE9ICdZT0xDVUJFUkFCRVJGQVRVUkEnIGFuZCAvL24xOkludm9pY2UvY2JjOlByb2ZpbGVJRCAhPSAnSUhSQUNBVCciPgogICAgICAgIDxkaXYgY2xhc3M9ImFkZHJlcyI+CiAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0ibjE6SW52b2ljZSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6QWNjb3VudGluZ0N1c3RvbWVyUGFydHkiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6UGFydHkiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpQb3N0YWxBZGRyZXNzIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpSZWdpb24iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY2JjOlN0cmVldE5hbWUgIT0nJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpTdHJlZXROYW1lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOkJ1aWxkaW5nTmFtZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjYmM6QnVpbGRpbmdOdW1iZXIgIT0nJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpCdWlsZGluZ051bWJlciI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBObzo8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjYmM6Um9vbSAhPScnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOlJvb20iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5LYXDQlMKxIE5vOjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNiYzpEaXN0cmljdCAhPScnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOkRpc3RyaWN0Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY2JjOlBvc3RhbFpvbmUgIT0gJycgIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOlBvc3RhbFpvbmUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjYmM6Q2l0eVN1YmRpdmlzaW9uTmFtZSAhPSAnJyAiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6Q2l0eVN1YmRpdmlzaW9uTmFtZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0Pi8gPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNiYzpDaXR5TmFtZSAhPSAnJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpDaXR5TmFtZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDwvZGl2Pgo8L3hzbDppZj4KCgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6QWNjb3VudGluZ0N1c3RvbWVyUGFydHkvY2FjOlBhcnR5L2NhYzpDb250YWN0L2NiYzpUZWxlcGhvbmUgIT0nJyBvciAvL24xOkludm9pY2UvY2FjOkFjY291bnRpbmdDdXN0b21lclBhcnR5L2NhYzpQYXJ0eS9jYWM6Q29udGFjdC9jYmM6VGVsZWZheCAhPScnIj4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOlByb2ZpbGVJRCAhPSAnWU9MQ1VCRVJBQkVSRkFUVVJBJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpQcm9maWxlSUQgIT0gJ0lIUkFDQVQnIj4KICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0ibjE6SW52b2ljZSI+CiAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOkFjY291bnRpbmdDdXN0b21lclBhcnR5Ij4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpQYXJ0eSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGRpdiBjbGFzcz0idGVsRmF4Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6Q29udGFjdCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY2JjOlRlbGVwaG9uZSAhPScnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UZWw6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6VGVsZXBob25lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY2JjOlRlbGVmYXggIT0nJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEZha3M6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6VGVsZWZheCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9kaXY+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgPC94c2w6Zm9yLWVhY2g+CjwveHNsOmlmPgoKPC94c2w6aWY+IAo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6QWNjb3VudGluZ0N1c3RvbWVyUGFydHkvY2FjOlBhcnR5L2NiYzpXZWJzaXRlVVJJICE9ICcnIj4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOlByb2ZpbGVJRCAhPSAnWU9MQ1VCRVJBQkVSRkFUVVJBJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpQcm9maWxlSUQgIT0gJ0lIUkFDQVQnIj4KICAgICAgICA8eHNsOmZvci1lYWNoCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6QWNjb3VudGluZ0N1c3RvbWVyUGFydHkvY2FjOlBhcnR5L2NiYzpXZWJzaXRlVVJJIj4KICAgICAgICAgICAgICAgIDxkaXYgY2xhc3M9IldlYnNpdGVVUkkiPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+V2ViIFNpdGVzaTogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4iLz4KICAgICAgICAgICAgICAgIDwvZGl2PgogICAgICAgIDwveHNsOmZvci1lYWNoPgo8L3hzbDppZj4KCjwveHNsOmlmPiAKPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkFjY291bnRpbmdDdXN0b21lclBhcnR5L2NhYzpQYXJ0eS9jYWM6Q29udGFjdC9jYmM6RWxlY3Ryb25pY01haWwgIT0gJyciPgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6UHJvZmlsZUlEICE9ICdZT0xDVUJFUkFCRVJGQVRVUkEnIGFuZCAvL24xOkludm9pY2UvY2JjOlByb2ZpbGVJRCAhPSAnSUhSQUNBVCciPgogICAgICAgIDx4c2w6Zm9yLWVhY2gKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpBY2NvdW50aW5nQ3VzdG9tZXJQYXJ0eS9jYWM6UGFydHkvY2FjOkNvbnRhY3QvY2JjOkVsZWN0cm9uaWNNYWlsIj4KICAgICAgICAgICAgICAgIDxkaXYgY2xhc3M9ImVNYWlsIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PmUtUG9zdGE6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgICAgICAgICA8L2Rpdj4KICAgICAgICA8L3hzbDpmb3ItZWFjaD4KPC94c2w6aWY+Cgo8L3hzbDppZj4gCjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpQcm9maWxlSUQgIT0gJ1lPTENVQkVSQUJFUkZBVFVSQScgYW5kIC8vbjE6SW52b2ljZS9jYmM6UHJvZmlsZUlEICE9ICdJSFJBQ0FUJyI+CiAgICAgICAgPGRpdiBjbGFzcz0idGF4T2ZmaWNlIj4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5WZXJnaSBEYWlyZXNpOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpBY2NvdW50aW5nQ3VzdG9tZXJQYXJ0eS9jYWM6UGFydHkvY2FjOlBhcnR5VGF4U2NoZW1lL2NhYzpUYXhTY2hlbWUvY2JjOk5hbWUiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLz4KICAgICAgICA8L2Rpdj4KPC94c2w6aWY+Cgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6UHJvZmlsZUlEICE9ICdZT0xDVUJFUkFCRVJGQVRVUkEnIGFuZCAvL24xOkludm9pY2UvY2JjOlByb2ZpbGVJRCAhPSAnSUhSQUNBVCciPgogICAgICAgIDx4c2w6Zm9yLWVhY2gKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpBY2NvdW50aW5nQ3VzdG9tZXJQYXJ0eS9jYWM6UGFydHkvY2FjOlBhcnR5SWRlbnRpZmljYXRpb24iPgogICAgICAgICAgICAgICAgPGRpdiBjbGFzcz0icGFydHlJRCI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Y2hvb3NlPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJjYmM6SUQvQHNjaGVtZUlEID0gJ1RJQ0FSRVRTSUNJTE5PJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VGljYXJldCBTaWNpbCBObzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iY2JjOklEL0BzY2hlbWVJRCA9ICdNRVJTSVNOTyciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0Pk1FUlPQlMKwUyBObzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOm90aGVyd2lzZT4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYmM6SUQvQHNjaGVtZUlEIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6b3RoZXJ3aXNlPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpjaG9vc2U+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD46IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYmM6SUQiLz4KICAgICAgICAgICAgICAgIDwvZGl2PgogICAgICAgIDwveHNsOmZvci1lYWNoPgo8L3hzbDppZj4KCgo8L2Rpdj4KCjxkaXYgaWQ9IkVUVE4iPgogICAgICAgIAogICAgICAgIDxzcGFuIHN0eWxlPSJmb250LXdlaWdodDpib2xkOyAiPgogICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkVUVE46IDwveHNsOnRleHQ+CiAgICAgICAgPC9zcGFuPgogICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJuMTpJbnZvaWNlIj4KICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6VVVJRCI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDwveHNsOmZvci1lYWNoPgo8L2Rpdj4KCgoKPC9kaXY+Cgo8ZGl2IGlkPSJiMiIgY2xhc3M9ImJveCI+CiAgICAgICAgCiAgICAgICAgPGRpdiBjbGFzcz0iZWZhdHVyYUxvZ28iPgogICAgICAgIAogICAgICAgIDxpbWcgc3R5bGU9IndpZHRoOjkxcHg7IiBhbGlnbj0ibWlkZGxlIiBhbHQ9IkUtRmF0dXJhIExvZ28iCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzcmM9ImRhdGE6aW1hZ2UvanBlZztiYXNlNjQsLzlqLzRRQVlSWGhwWmdBQVNVa3FBQWdBQUFBQUFBQUFBQUFBQVAvc0FCRkVkV05yZVFBQkFBUUFBQUJrQUFELzRRTVphSFIwY0RvdkwyNXpMbUZrYjJKbExtTnZiUzk0WVhBdk1TNHdMd0E4UDNod1lXTnJaWFFnWW1WbmFXNDlJdSs3dnlJZ2FXUTlJbGMxVFRCTmNFTmxhR2xJZW5KbFUzcE9WR042YTJNNVpDSS9QaUE4ZURwNGJYQnRaWFJoSUhodGJHNXpPbmc5SW1Ga2IySmxPbTV6T20xbGRHRXZJaUI0T25odGNIUnJQU0pCWkc5aVpTQllUVkFnUTI5eVpTQTFMall0WXpFek1pQTNPUzR4TlRreU9EUXNJREl3TVRZdk1EUXZNVGt0TVRNNk1UTTZOREFnSUNBZ0lDQWdJQ0krSUR4eVpHWTZVa1JHSUhodGJHNXpPbkprWmowaWFIUjBjRG92TDNkM2R5NTNNeTV2Y21jdk1UazVPUzh3TWk4eU1pMXlaR1l0YzNsdWRHRjRMVzV6SXlJK0lEeHlaR1k2UkdWelkzSnBjSFJwYjI0Z2NtUm1PbUZpYjNWMFBTSWlJSGh0Ykc1ek9uaHRjRTFOUFNKb2RIUndPaTh2Ym5NdVlXUnZZbVV1WTI5dEwzaGhjQzh4TGpBdmJXMHZJaUI0Yld4dWN6cHpkRkpsWmowaWFIUjBjRG92TDI1ekxtRmtiMkpsTG1OdmJTOTRZWEF2TVM0d0wzTlVlWEJsTDFKbGMyOTFjbU5sVW1WbUl5SWdlRzFzYm5NNmVHMXdQU0pvZEhSd09pOHZibk11WVdSdlltVXVZMjl0TDNoaGNDOHhMakF2SWlCNGJYQk5UVHBFYjJOMWJXVnVkRWxFUFNKNGJYQXVaR2xrT2paRE5ESkJORUkyUWpWQ1JERXhSVGhDUWpNMFJFSXdRa1pHTUVReE9EWTBJaUI0YlhCTlRUcEpibk4wWVc1alpVbEVQU0o0YlhBdWFXbGtPalpETkRKQk5FSTFRalZDUkRFeFJUaENRak0wUkVJd1FrWkdNRVF4T0RZMElpQjRiWEE2UTNKbFlYUnZjbFJ2YjJ3OUlrRmtiMkpsSUZCb2IzUnZjMmh2Y0NCRFV6UWdWMmx1Wkc5M2N5SStJRHg0YlhCTlRUcEVaWEpwZG1Wa1JuSnZiU0J6ZEZKbFpqcHBibk4wWVc1alpVbEVQU0l6UkVWRU5rVTFOMEZEUkVWRE5FSkJOemt4TlVNMk0wTkNOMFJFTnpNME55SWdjM1JTWldZNlpHOWpkVzFsYm5SSlJEMGlNMFJGUkRaRk5UZEJRMFJGUXpSQ1FUYzVNVFZETmpORFFqZEVSRGN6TkRjaUx6NGdQQzl5WkdZNlJHVnpZM0pwY0hScGIyNCtJRHd2Y21SbU9sSkVSajRnUEM5NE9uaHRjRzFsZEdFK0lEdy9lSEJoWTJ0bGRDQmxibVE5SW5JaVB6Ny83Z0FPUVdSdlltVUFaTUFBQUFBQi85c0FoQUFCQVFFQkFRRUJBUUVCQVFFQkFRRUJBUUVCQVFFQkFRRUJBUUVCQVFFQkFRRUJBUUVCQVFFQkFRRUJBZ0lDQWdJQ0FnSUNBZ0lEQXdNREF3TURBd01EQVFFQkFRRUJBUUlCQVFJQ0FnRUNBZ01EQXdNREF3TURBd01EQXdNREF3TURBd01EQXdNREF3TURBd01EQXdNREF3TURBd01EQXdNREF3TURBd01EQXdQL3dBQVJDQUJtQUdrREFSRUFBaEVCQXhFQi84UUF0d0FBQWdNQUFRVUJBQUFBQUFBQUFBQUFDQWtBQndvR0FRSUVCUXNEQVFBQkJBSURBUUFBQUFBQUFBQUFBQUFHQUFRRkJ3Z0pBUUlEQ2hBQUFBWUJBd01DQXdVSEF3UURBQUFBQVFJREJBVUdCd0FSQ0NFU0V4UUpNU0lWUVZFeUl4YndZWEdCb1JjS2tiSEIwVkl6SkVJMEp4RUFBZ0VDQkFJSEJBY0dCQVFIQUFBQUFRSURFUVFBSVJJRk1RWkJVV0VpTWhNSGNZRVVDSkdoc2NGQ0l4WHcwVkppTXdseVUzTWs0WUtpRnRKRGc5TlVKUmYvMmdBTUF3RUFBaEVERVFBL0FOL0dsaFltbGhZbWxoWXByTW5JTERYSDZCSllzdlpEcmxLWk9EK0NMWnliMGhweXdQVGJBbEdWcXV0Z1huTEZLTEdNQUVic202NnhoSDhPM1hVanR1MWJodTAva1dNWmRxVkpxQUI3U1NBUHB6Nk1OYmk4dDdaZFV6VTl4KzRIQTRvOGtPUkdXYTlMVCtEZVBDbU82eXlLZFJ0a0xsM0t5bUlHVDFna1F5em1hajhiUlVKWk1ncHhhRGI4MER6S1VFWXdGRURGS1VPL1VwK2s3VlkwaXZybHBMNnREREZIcW9hK0V5YXFWN0FEMVliQzR1N2dGb1VDUlV5WWtmVHBwOXVCNFh1SEpHMTVYaXNOV2YzRE1jNDh2MWhXWk5vaUF3anhOZVNWWmR5RXRUNUhJVWJWbStYTXFTRjdvYnU1dmFCRU9KcENNSTdieXEwVVQxaEdmcHpFT00yWU50RzFuY1l0blkyOGRTenRjblhRT0l5NWlyckNDUmhIcjBhTlowYXRXV0d6QzRhY1FQY2pXZUhjeXJTdEs4SzBGYVZyVE9sTUFoeWN6bnlINDc1UzVCNDhjY3MrVEZ1bjhZNHdyOXV4aWt3REFNQVhMTi9ldjhUdExMVEU0NWZCc2pGVmlOclRITThQS0t1akx1VlBweUw5VVVDcE1qckdLOWkyUFpkNnNiTzkrR2ppaW51Q2twTEZoREhTVXJKMEZ5eGdrVUxRZDdRTlJMZ0NQdXBKN2FTU0xXV1pVcXVYaVBkcU9vVTFLYTU1Vk5LREJlUk9RT1ZkZHR0Qng3RWM5cS9LWkN2ZExpcmRDVmprZHhDVlNvOHV1dlEzZVNKZXNRT1pzWHA0cXJNcExSRlNpbnI5ZnNPNFhRWnRWMVRvR01nc2ttTkd6MnRyV2E5L1N4SmF4U0ZXWkxvNjBHc1JxelFodFNndVZVRXJwMU1CVTFGWDJxNGpkVUU1RWpDb0JUSTVFa0JpS0VnQW1uR2c3TWNyeEI3aFhKRnhVYXZkTXI4UlovSXRGdEdQcVhsSm5rUGlnN2w3KzZhVVRJU0VtNnAwN040anVVUFVyd1U4eXdpRlhYcElWYWVkcElHSWZ4R1RVVE9kcHVQS3UzVzd2YngzYXg3a2swa1JoWlNRSkkyQWRQTnFGN3RmRVJRbnUxcURqbTMzRzdLaDVJeTBCVU5xcUIzVG1EUUQ5dU9HQjRMNVJZRjVLUlRxU3cza212VzV6Rkg4RmlySkYxSXU3MUY2WFlxa2RjS1JMcE1iVlZuNlJ4N1JTZXRFRENQdzNEcUl0dVd6N2x0VGhMMk14MUZRYWhnZmVDUlhzNDhNc3hpVHQ3MjN1RkRSTlUrLzd3TVgvcU53NnhOTEN4TkxDeE5MQ3hOTEN4MEgvWVAyNjZXRmhkZVUrV1Z0eUxrbGJqanhIY1ZCZTdsbVZxbmVNNzNoMGlPTWNYVDZVVTltcEdwMWVIRnl5ZTVuekpIVjVndklmcHlNV0lpeGFvSFdrWFRWTXV4ak95NWZnc2JIOVkzNFA1UVhVa0toaVpGcUZKYVJDZktBWjA4WUZTUXRRVGlIbnZKTGlRVzFrUUdKemJMTHA4TERQZ2VIdHdBWTN6R3VQV3JtNTRQcmVVczg4bkplV2pKY25LbkxWTFlaRHRtVU1WUWwyUFJNMlhIalRCc25Ga05DUnVHYk9vMmFUOWRZMTZPZXhVV3FlUlRpWllpQkJXS2ZnYnU5MFcyN3lRd2JMR3JCYmNQcDhpVm8vTWhqbmtLSlFUcUMwYmVZVVpxSVhqT2FzME1VQTFRQm51U1JWNmVKUWFNeXJVK0FudkNsUU13RzZUNnMrSWNvYzBPSDlIY1d5Zm1zRjhoMUtyYVdqTzBoVkxEWG9vWFZnakxCam0xR3MrSkpxWlpURGpIZVZxWTZVZXB3YzJack1SWkhqTnlkS1BsbUpVMjRyQnVOaHl6ekhLSUVTNzJiekZxbXRXYWlsWkYwVEJTb2xpY0JmTWpCUjlMS0M4VG5WSk5ETGUyUzZpWTdtaHpvUU13Vk5WcjRXR2VrNWlvT1RESzRvYmhqaHlGek5ENTVZSjJDT3Y3Q0Vwc1ZMaEJ5Z1FrQmFIbENxVHVrVmlXbldyUnVNMjRDT3JUMFczMDRKRUloeVZCc281YXVGbXJkVk9NazVrM0tYYkcybHlqV1paeXVvYW1RU09IWUtUM1JWaFhWcDFpckJXQVpnWEFzb1ZtRndLaVNnNFpBMEZCWHA0ZEZhY01zaGp6c204THVOK1hybi9jSytZK0pNWEF6NndTU2swRTVZVzZ5ajZ6WWxmWVFrbHdib3lnTTIvOEErZFB6TmtpSXBwSnBPazBuZ0Y5V2tSY3ZGanpOdlczV3hzN1NiVGJGVlhUcFU1TE1Kd014WCtxQWFuTWlxK0VrWVV0bGJUT0pKRnE0Sk5hbmlWS0gvcE5QcjQ0cSsxKzNkaEdYa2JuWXFoSzNqRzlzdDFUeUJXVXArdlN6S1JXcjd6STJKNFRERXhhWW45UnhzcklGc0RDbFZ4bVJpWTdvVUdpeVpqcHBnQ3l4Vkh0dnpodWtTUlEzS3hUMjhVa2JhV0JBWVJ5dE1FYlNWR2t1emFxQ3BCNDVDbmsrM3dzV1pDeU93SXFPaXFoYWl0YzZBVXdNMHpUYzE0YnpmVzhaOGR5UHJGazNJR1VyeGtHNlAzMWZ5UENZTXhsZytId3ZINE93VXh1czhvUmxXcmZDWTFnR01hOVNyVVc5OWZQV3Rxc2RJSTlJN3Q4em00Ym5iTnkydDczZVNzZGpCYnBHZ0RSbTRsbmFZenpsRnpaR2tZdURLNjZZNFNBZFpDSTdWa21obkVWdUNaWGNrMUIwS29YU3RlZzBGTzZEVm1xY2hVampsS2pjWGMrOGhUNjczRlZ5d1ZsK2pRa25KWW81aFkwbEg5TnpGTHRhdllHZFRjeTkxUWphUENVK1BhV3lVY2xrV2RUY3k5eVlMTUNyb3lLY2UvYXJNMHU5NHQxeXJab3NWd2s5bE13RTl2UmRJMUtXMEpJV2FSZ283cnZvaUdxaFFTSXdjK2F4eDdnN2R3cEt0ZEQ1MXlOS2xhQlJYb0ZXeTQ2U0NCZE9MK1kxN3dqa052eDI1b3kxUW1YUTJ0cGp1aWNycUFkbWpqaTcyOTdHc1ptSW9PWmFzd2NQMU1BNWtrWVdXWnJvTVg2b1JVMERraDJDL2NZRWRSTjl5L0R1Tm91NmJDcnBxVFcxdVZidUtDVTFKSklSNW9aa2J3Vm9hcnhBR084RjdMYXlDMXZhTm5RUFVabWdOQ3FqS2xSeDQ4Y05JQVFNQUNBN2dQVUJEZllRK3orSUNIK3Vnc21oTmVJeE5ZN3RjNFdKcFlXT20vWGJTcDA0V0FhNUVYRzlaVnNUL2psaCswS1VHT2FScWN2eUx6bzJjTjJxdUlhRXVnWjZFQlZKQjJQb0VjbTI5aTJVQkZWWURKUTBjS2o1VU80RzVEbG16Mjl0dGNVZTlYNitaTXgvMjhJcldRaHRMTVN0ZE9nNXFHWHZIZ09uRVZkTkxkT2JXRTZVSGpicTZSa2FWcjJITEMrNXEzd3N2SVl6d1h4UU0xdUhHV2JaM1RHdVBZRGpPOE1sbFdsWjJna2NmM3FJejdsL0p0dGdVVDR5ZXd6aHhLTzB6T3l2R2M1RmVvZHVUVGFrc3ppa3lxRzFlS09iYzkrVVJiMm5seXlOY2dDS1NBK1pFWUlvb3lQT0wwUU1SUjFrb0I1WGx2S1dwYXBXQzFPcTNOVkdqeEJzbTFNeDhOTTZEZ1IvRnFDNGEvZ2pqL0Q0bmhWWDg4U0FzT1NySFpIK1JicllZbUljeGRZSmxDejEySmdzaFdqSEZWazVLY0xqZHJmM1VZZVJsV2tldW1rOWxIN3gwb0hlNVVEUUZ1Mjd5YmhLRmkxcFpSb0k0MUpCZnlsWXRHc2pnTDVwakJDcXpDcW9xcU1sR0phM3QxaFdyVU1oT29rY05SRkNWQkowMTRrRGlTVHhKd1JKakZJVXh6Q0JTbEFSTVkzUUFBT29pSS9ZQUJxR0hiaHlBVGtNempPaDduZnVQUFRPWmZCZUM3UThnMElaeDQ3emtLRGtuTWErSThiZ2d2OEFScTlLTUZXN3BzZHNvQml1VlNIK0lDUVB0MnFEbm5uUnJRL3AyMU9Wa1U5NXgwY01nR1VnanRCLzQ3UVBsRStVMjIzaUtMbnYxRXR4TGFUTFdDMlk1RmFsVEl6dzNJWU4vSTZaRHQ4T2VXWjVmNTJUZUxnbnlnemFCVXpuRDh2S3QyQW9BQmhEOEpacnA4TnZ1MVZMYzQ3K2g3dDI0UHNYL3dBT05sZHA4c1hvMFlWTDh1MjFTUDhBTm42UC9YeFUwN3pyNUlLUG0wSld1UTJmcGlZZnJwczJMWnJsSzlyTHVYSzV3VFNTU2JwelluVU9ZNWdBT24yNlVQTmZORnk2eFEzVDZpYWNFNmZhb3d4M2IwRytYemx5eWszSGR0aXRVdG9sSmJ2M2JVcC9na1kvUURoOXZ0bVlHNVZ6OW9ybVZPUlBJYmtROUZKUkdVaHNkSTVYdWkwSVFpeUNncEZ0aUR1VldJL01JS0ZId0I4aFJENXU3VnpjcGJYdndWTHpkN2xtUEVJUXZ0NG8zM1kxUi9NajZsK2tVcG41YzlNOWlndDR4VkduV1c0Sk5EL2wzTnVDT0I0UDc4YU1NZzQ2dGEySmN1dHVPaTlIeERtL0lVREp1WW5JYmluUnJoQlM5TE5GRW1OanRhYkpxa3JOU1pQSWNpYjEybS9GcXFjRnp0M1pDR2JMWEh0OS9iL0gycmJ5SmJuYTRuQWFQV1FmTEJ6VlNmQ093RmE4QXlrNmhyNG5pWXh5ZkM2VW1ZR2hwMDlaL1krdzhNSmx3ZngweGJTN2RrZUQ1UU1uV084UXl0VWs4WlNPTHMwUkZadG1jYzB1Y3N6VUpiN1ZrM09lVGNaM2l3c2JmakxHdVlTUzVhbmZKcXR3YnhtOGxWektTNlNCVWtsck0zamVMdTl0SVcyZlRjWCtzU2VaRVdXM2hFU3RHc1VFTXNTbEpaSWRCbGhqbGtVaEZwSHFCS3dVRnJHanNMbXF3MHBSczNZc1F4Wm1WalZRMWRMRlFjem5UaWNmSGEvNUc0cjVmaCtFdklLeFM5MXB0b1p5TDdoM24reExDdktYT3N3amNGM21DOG95Nm9wbGRabG8wY2taWm05RXBBc0VPUUZkdlZJT0FNSTdwYTIyOVdEY3diYUFzOGRQaVl4V2lGbTBxNExFYXRmRWhRYWZpenFTOXRYa3RKbHNaalZHcm9QWFFWSXk2dTArekRQZHR2aG9OejkySmJveE92MzZXT0tIcnhUR2Zzc0pZYXhoUFhCSnI5VXNhd3M2NVJLOFVmOEEyTFRrQ3l1azRhbTF0b25zWXlpc3JPdTBTRDJnSmdUN2piRHR0cVQyamIvMUs5V0JpQkFBV2NuaHBYTTlJT2ZESTlQVmh0ZHovRHdseDR6a1BiVUR0d29QTTdmSytMcHpGR0k1Q2F1T0I1cTYybXdNOG44bHNuTW9mSUhDL1BxZVY2bjNXZW41UnBFUkxDWmphN0ZsQjlIMVN1dFpkOVRaRnZDbFZXWVRDKy8weDNZbTFEYnI2SzUzTUpGZGhFVmt0b1MwZDdBMFRaU1F5RmY2YXhCNVhLSk9vT2xYaVdna1NJbEUwSGwyNEppcVNDN1VNVEJod1lBOFN4Q2lwUThTR1BoTEorSy9GdGxnaEN6WFN6T1dNL21MSktxcjI1ekxWcFYxNDZyTVgxaG43cXBqQ2lXS0p4L1FMTk00eXJseHQ4czVpaldCTjdNRVNkZ2tzNU9taWlSTUszN2ZXM1V4MnNBS2JiQUtJdFhxNUNxbm5TSzBraXJLNklnZnk5S1ZXb1VFa21WdExRVytxUnpXZCtKeXlGU2RJSVZTVkJKcHFxYytPQzgwTzRlRENtdmRNNWtud0ZqbFBHTkprU29aTXlLeWNKZW9SVVRGZXVWZzNlM2V5eGlHSVlTT0hRZ1pGdWJwc2Zjd0Q4dWdMbm5tUDlIc1BocmMwdkpSUWRneXFjMUk2ZUdNenZrNzlDVDZvODVEbURlSXczTEczU0tXQlA4QVVsejByM1pvcEFBUURxQVphNUVIT21LM0x1UlZuYXk4T3hjbk9VVkZETzF4UDNLTHJLajNIVU9jZHpIT2N3aUlpSTlSSFdNMTNjTTBsSzU5UDFkbVBvRTVVMkNHeXQwT21pS29DaXBOQUFPblVhOE9uQWYyS1pkZVZHTWpFMTNrcklMSnRXclJ1a1pkeTVkTG44YWFTU2FlNTFGRkRqc0FBSHgxNFcxdTA4cVFKNG1OQisxY1NITlcvd0Jyc0czU1hseTJtM2pRbGpRbklDdlFySGdPZ0hHbFQyc1BhK2o2ZEhsenJuVmkxL1V5Y2NhY1ZDVUtiME5KaDBTZXJWM0tyK1Q5UzhDZmNvb0lmbC9oQVE2NnlCNVA1U2cyaUFYMTcvWHBVOGU3dy9oY2cvUmpSaDh6WHpJYng2bmN4SGs3bFp2L0FLc3lHTlJSRDVyRTBwK2JheE9tZjg5TzNCald6M29jSDhlY3B3TkxoOFJ2WmJGNHlob2VWdjZNczNidmZHZ3FMVVppUGl6czFQVVI1VG1BNGdLeVpoVDNFQUVkZ0h0SjZsMmRydVMyU1JhclF0UXZxWWRuaDhzbjY4U2UwZklKelh2bklFbk5GNWYrVHZ3aDFyYmlHRncxYU1BWmhmS2d5UEhSbDFWeG96cFZ1cjk2ckVIYkt1OVJrSU93UmJLV2pIYUJnT211eWZvRWNObFNtRC91VFVEL0FGMWEwTXlUeExOSG1qQ285aHhycTNYYnJ2YU54bTJ5K0dtNmdrS01LZzBZY2MxSkI5eEk3Y0wvQU9lWEVhaVpNalpiUEI2dldyTFphYlhJMXhjNmxmTGxNMHJGdC9xMUZTdDU0ZHhsS1hnS25kYldXb1k2aHNnMlY3SVJjQzNZdWJiSHVsWWlSV2NzRGcwTWRjcGN4M2RoSXUxQ1NTT0NSem9lTkZlV041TkZmS0RQR211UXhSS3J5RmhDd0VzWVdRYXNEZTRXVWN5bWVnTEFDb0pJVmdLMDFVQk5BR2FvRk5RT2xxakxGTlVuRmVXK1luRlMwVXprMWMvMHB5enRNTFNPUUdQbTdDMVVkd0dCcnhHTS9Maml5WTlwRUJGUkY0b01OVmJ4Rkx4OHExbkRTanB5NEk5YXFTVHJ5TG9wUDc2ODIzWU4vUzQyQ1BYc1VVanc2eXJneTBKcVpKQ1dSMjBzcktZd3E2YWZscURuNEpETGUyWlM5SUYwd0RVcU83N0FLRUNvSXpxYTlKd2JuQ2JrUSs1TDhmNnplYlRGcDFyS2NCSVR1TjgyVXZZQ0wwekwrUHBWeldidkNySUI4eURWeElzZlhzQkgvd0FzYThicWh1VTRhRnVZOXFqMmpkcExlQTZyUTZXUTlZS2cwNGs1RWtabXBBQjZjUE51dXZpclpXYktRWkgzWmRRNDB3V21vUEQ3Q3RPVk9hNnJIY3M4UjFtM2hLdmFSeDRxc2RuT3d3dGVqRlo2ZnRXV3NxM2FPd0Z4eG9rTEFwbUlhU3NWanRsbmtGSTBtNVNnNWJnYzVreWw4cERuWmRybmsyS1VSSVBpTDJvUjJPbFVqZ0RTVE94NkVDSzViL0RsVThJYTVuWDQ1UzNnaDRnY1NaQUFvSGFXSUE0OXRNSEtobFNNc0dWazhSeHJDTWN5a05UWTI5MytMc1R1VGg3SlhJaXdycXBVQ1RnWVZldFBZRzdSMGpOd1VtMGtIRFdXUytqUEdTUlRGVk9zQUVGell2RllmcURsaEcwaFNNcUFWWXFQekF6YWd5RUt5RlFVT3NNZUZNNVB6UTB2a2lsUXRUWGlLOEtDbERtRFhQS21MbTFIWTlzZWhzOWhqYW5YWnF6VEs1V3NWQXhqeVZrSEI5KzFGb3lRT3V1Y2UwcGpmS21RUitBNjhwcFk0STJuazhDQWsrekVodFcyM083N2xCdGRtTlYxY1NxaWpJVlpqUVprZ0Qya2dkWnhnejV0OGxaak0yVU1nWlVrM2FoL3JrbzZqYXczT2M0a1lWZGs0WFJoMnFKREFVVXdGdHNvY0FBQThpaGgxaXB6WnZjdTdiazkyZkFUUlIxQVVISFNDZmVLNCtrYjVjZlNxMDlPK1E5djVhdFYweXJHSG1OVDNwR0lkbUk4MlFBNTBvcmFjcWdDdE1LWW5wWXlhVHArNU9Jbk1Cajl4dW9pT3cvZnZvSkhlSTdjWlZUT3RsYmdkQUgyZlRobS90RWNObHM2NUlWelpjb1ZTUWdLOUlHamFXemRwRk8wZVM0QUJuVW9kTSs0S0ZqMHo3SmROZ09PL3dCbXJrOU8rWFRMSitwVHIzUVJweituZzMyakdwZjU1UFhON1JQK3dkbmxvN3FUT2RQQVZHa2QrM05haXVhU1pWenhxTDl4dHc1NDhlMzFrTjFYeUtNM0VxV0VyVW82YkUyV1RqWngra3pmN21UK1lwVkVGQktJL1p2cXkrZDdoOXY1YWxrZzQwQzE3Q1IxMTltTUhQaysyUzA1eCtZRGJMUGNRSGlCbGxVWmp2SkU3S2U2eWNEUTVtbGVqR0V6UG1RRmJpL2lZeUxGUnljU0pzMmlDWkRDb3M2ZExKbEFwU2lBQ0pqRzJBTll4Uk5KZVhTQlJWeTMzKzdIMEM3NUpiOHNiQk05d2RLckNTZUo4SzhjdGZVZUdQb1ZlMk9XeXh2Rm5GTmN0U2k2a3RCMG1FWnVnWEVmSVJRalJJZkNiY1I2b0ZNQkIrNFExbHh5K2trTzJReFNaSFIyZmRqNWwvV0c4czl4NSszRytzLzZVdHd4QjczUlJmeEFIT25VTU1YRXBURkVwZ0F4VEFKVEZNQUNCZ0VOaEFRSG9JQ0dwd1pjTVZiaExkTHhaVGVFM0pwUzNIcGVjcHluU0ZySEhFYmVrSXZFT01lTjJLNHpQMXZweFdTN2xMOVJ0OHQ1eHlST1NxVUJFeTgwVmxLRVZYandjdXl0am9MdXdzcTQzQzQ1bzJMNGNTMmlYS3A1aFFtZVc1bU51ajFBT2t3d1JJdm1PaWFrSURGVjFBcXVJVklrc2JyWHBrTVpPbXZjVkYxa2NjOVRNVFFFMFBDcHBtY1hGajRuOWd2Yzl5OWo1RUFaVVBtbmlDSnp6WEdSTmtvOXRtakRCb3pIK1Rpc0c1UHl3ZDIybFNjTEpPeEFwUk00WktLbUV4MWgxRlhiZnFmSjF0SW8vTTI5M1Z5VHhFcmlsQjJkd2RKNG5McFVOYmJkcEVIZ21BUHZWYS92NnNORDBGWW1zSURzMlJzTlB1VG5OSCsvMFJhMzFHeTF5T3dweG5qclJUZjFZU2Z4YTU0K2NkSi9rVEJYMkdkVVNPazdpem1ZSEo4SzBQSHVZOHFhN0NTZm91eE9WTkJRRFczYTJlNm5ZZHZtMnA0MXViUzJ1SlFyNk5NcXpUeFF2RVJJUWhWa25iV3JFaGtETFRQQXlza0p1cGZpRllyS3lDb3JWU3Fzd2JMTUVGQlFqZ2FIREh1SUZid3ZJSzNMS2VPdVFHU2VUbGxrMmNGUTVmSTJWSm1JazUrdjF5QVZrN0hDVVdOWlYraTQ2aUl1UGJPYlN1NldVTkhIa1hxcXBCZU9WeklwQWtGOHhUN2lvaXNMdXpnc1lGTE9JNGxZS3pOUldrSmVTUmlUb0FBMWFGb2RDclUxbUxOSWU5TEhJMHJHZ3F4RlFCVWdaQlIwOVZUMGswd2IyaGpEN0N3UGR2eTZ0aTNpRmE0MlBjK25sOGp2MkZLWjlweElxTFI4NFRXbHpKRzNBU21KSEluRGNQaDNhQ3VmZHlOaHNFZ1h4eTBYM1ZGZWc5SDI0eXorVExrbE9jZld1eGt1RnJhV0t2TWY4WGx1RUdUb2VJSnFLOE9CNk1MdVhwb3ppU1FqQ0hIeE5rdzd2dER1RVRiL0FCL250ckZtNmwxdmtjaDk5TWZSaHl4YUNHMkVoOFJBK3dkdUI5VHI4bmZMZFdLRENwbVdrck5OeDBLMlRUTDNHRlorNVNRN2dBQTNNQ1lLZHc5UGdHdlhhN1I3MjlqdDA4VEVmdHhIUjI0Ry9VN21tRGxqbG03M2lZMGp0NFMzQW5PbEFNa2M1bWc4SnhzandmeUE0dmUzRFZLUmg2NVFseGYyU3ZVcXZ2cEFhdkNOWkJvMmNTVEJKYzUzU3AzaUM0dm5DdmNxWUJKMEtZdlhXU1M4d2JKeXRHbTEzUllPaTlBYzFxQWE4R3BYanhPTkVNbm9YNnMvTVZjM0hQOEFzVWNiMmQxTzFOY2xzbENyYVNCV1NGanBJcFV4clhpSzhjV0ZtbjNYdUIvSkxFMTB4QmVLNWxCU3RXK0djeGJ3RnEwM1JjTkJVVE40SHpVNXBBUVRkc2xSS29tYjdERi9scHJmODhjcDdwWVNXYzdQb2tXbmhrOXZRQjBqcndUY2pmS0I4eC9wenpiWmMxN0pCYkp1VnJMcVUrZlpOMkVVZVoxekI0bFRUalRDSnVJZURPRVdST1lFTFNhSkw1UnlGWmlyUzhwVjIxdHJNUEgxdUxid3pkZDRzNGtGbWtvNVhjT0c2SmZ5emVIdEU0QjBENDZDK1V0djVkazNjSlpzWkplSXlrV2dGVFhNME9NbXZtazVyOWM3UDAzK0w1cGpXenNuWUxLQTFoTUdKMGdLREdtc0FFOFFCV3VmREczZkRkRlJvMVdaeDZKU2xFRUNBUGFHeFI2QnNBQnNHd0IyL2RxKzdlTlVqQkhHbjd2M1kwM2JuY3RkWFRPM2lxZjI0REZ3YTk4UitGSWM5NnJqTkhNK01ja3o4N2RvRzd4c013cjlSbnNaNE13UmtxN1ZxZGJUajJXYXVLMWtIa0hIMlNoNHd0MHV6ZkdLMU9XUFJrM1NEWWZUT0RxRlNUTFlIS1YxZWpicDdLSlludEdZczZTejNFVWJycEE3OGRzeVNTb3BGVDNpcWs5NFVxY1JHNFJ4R1pKR0xDUURJcXFNd1BZWEJDayt5cDZNZnR5dWduR0w4cmUxWmtCYXczQzFTMVc1SXU4T3pOcXZ4NFk5M21vTFBlTGJQQVBBczZ0ZGlJS0VKS0wyNkxoanJKczJiWnFVeUhhUklwU2xBUFBacm1LNjJqZklnc2NZbFNKbFdQVm9YeTJkaUYxRm1JNFVMTVQwa2s0OHIxQ2w1YU14WmlDMlpwVTVEalNncjdCaHRHNGZzQTZBOFRlb1lWcHdLZ2EvSTVrNS9LemtVeWYybWxjOUw1WTYrK2V0MGwzMENqYjhLNHppRW4wWXNZQk8wVWtvTVhUWVRGMk1MZFE1TjlqQ0FtZk1Eeng3UHQzbHNSRExic3JBSHhhWFZzeDBnRXFSMmpzeENiWUVNODJyTmxLMHk0VkJIMTRhWm9NeE9ZNkR2OW43Zjc2V0YwZHVNNFB2NzNReUNISHVqbFdNVXF6eTVXWlpEdTJLY0dyZUpqVWptTC84dGhkRzIrN3JxbS9WbTZJanRiVG9PdGg3dEdYRHRIVGphai9iUTJLSzQzWG1IZW1INWtTMnNZTlRscjgrb3BxQXpwL0NmYU1aSExtN002bjVGVVIvQ3Fjb2I3Q0lBWGNOdWdpSFRiK1E2eC9jbG1KSEhHNjdiWXhGWm92N2NjRVo3WTFEL3VUemdvQlhEVXJwbFV6dXJRc1ZRb25USXJIcDlyVVRCdjA3bERodHYwMzIvZHF3ZlR5eitJM2xaV0dhaHZyVTlvNFl3UDhBbm81cmZaUFMrUzBScVBkVHhnbWxhcWswYkVVME4yWjFCOXVISjhxL2JDNWw1WXpiZGNueGVTNmEzaUxsTGd2QlF4VTVVeG91Q0lSTnRHTVZlNXNaSURvTmlBQmdLTzIrK2pYZk9RdHgzWGNIdmpOcERVb05DbWdBQS96QjFkV01XUFJ6NTArUlBUZmtpejVUL1ROYzBXc3MzeEZ3S3M4ak9UcCtCbEFxV09RY2dkRk9HRVdYejlaVUNSdDlabFpOaEl1YXZLeUVBNGsyU1FrUWRPV0s2clJkUkRjcERkdmxUTUFiZ0E5TlV4ZlF5V2R5OW9XcVVOSzBBNmp3eiszRzEvazdkYlBtN2wreTVpaWo4cjR1TFdGMU0xTXlCbVFsZUZmQ1BaZzAvWTNoMzlnNWlXTzJGS1l4cXpWbGtVM0FsRWRscHB3Wm1va1U0ZHdBWTdjVENQN2cxWnZwWmJoOXdrbUk4SS9lT3Z0eHJ3L3VKOHdOSHlsWTdOWEtXVjZqL0RvSU5kUFNSdzFERysyRElZa1d6QS80L0NUdS9qc0FmMDIxa01uaEdOSkxtcms5Wng3YlhiSFhDUStjUEd5Tmg4NVRPV245NGxrNi9teUd2c2Zab2hyN2NXUnVhWTFKcE40MndmaXUyUHlXL0dMdFpwajhWNjdpaUtYaXp6TVEvZG1jTFNaVXp1V2U3UnZhWEt1OVBQdFM3Y2tTK2ZhTkdWWTduRlk2aXNzOHFBcE5uSlJwbkRlVzZBQVIxQ3YzbWdkd3R3bHdaaXgwU0JxanlHbHBWVVU1cjRjbEZOUVBUeEdRNTd6S3BNTFJzY2NBNmpYSDhuSnBTSHVFOFdiRXdjVERkMjBrblByY2prdU00QU5aTk1zeXdhdEdIcUJSYXZWRlhUSm9tVnNaUVFTS0FRVzJ5ejN2NmxjU0JWSzIxRFNoR1ZRT0dSSnBtVnlKcTFNOGQ3NUFyMjZpckRVVG5sMUg5dm93My9RVmlid3JuanNzR092Y3c1NjR0Y205TzF5MVJPUFhKYXFJR0g1WFNLTmVmNGl2S2pZZHhBUlpXQ3JzMUhBYmJnTDVMZm9JYU50NkNTOG43VmRBMWtScmhYN1B6TzcvQU5LOUE5dmJEV2Y1ZTVUeGRpVStqL2poanRmdTFSdGp5Y1lWaXpRYys3ckVrYUhzYmFJbEdrZ3ZCeTVDZDU0dVdTYktxSFlQMHlEdVpGVUNuS0E5UTBESklqMTBFR21DYTYyNitzbzBsdTRuampsQktFL2lBcFduc3FQcEdPVWZ3NmZ2L24rOE5kdFFwWERMR1Y3L0FDREFjdHNyY2RIWnU0R3E5TnVyVXBoRU93RjBwV0ZVTVgvdDdqSnFnUDhBQU5VZjZ1RWlleVkvd3kvYkhqY0YvYkJLdnRITk1QNC9pTEUvVmRZeTJXRGNaT1RFUStLeS93RHB1YmJWR3YzV29NYmdyYksyV25WaG0vc1V4Q1VweSt1aXlwUUU3S21OUlNLYnFPeTh1a2ljZDloNkNYNC9EcHEzUFNrSzE5SWVKQ2o5dnJPTlZmOEFjWHVKRjVZc0l3ZTZaNUs4UDVlekc2ZTJHaWF4am1jc3I1TkJOS3YxZVFrekxLRkxzbVZuSG5XQXc3bEh0NmsxZmM3TERhUEtjbFZTVGpUZHkvYVRicnpEYWJmQ05VczF3aUtNaDRpQlN0UjlKSTlveDg0L1BreUVnMG5wOVVwRTNkbW41U1pYS1Vmd3FTTHB5OU9YZm9JZ0IxUjFoenVNalhFNWw2V0orMnZaajZuT1NiSk5zMkcyMjZMdXhXOEtJT25JS0tjU1QwZEpQdHczWC9IZG9obnN2bFM1cklBSlgxa2lJdHNzSlFNUGhaTW5TeXhDaU8reGZJY045aDFkbnBYWmxMT1M0WWNTUHQ5djNZMUUvd0J3M21BM1BORnB0ZGU3REhKWDJrSWY0UjE5Wnh0SmFrOGJaQWdCMEttVVA0ZFArTlhOalYwYWsxeDVPdU9uQ3hYT1NNdDR6dy9HUmt6ays4VnVoeEV6TXRxOUdTbG9sR3NRd2R6VHRCdzViUnFidDRkSkQxS3pkbXFjcFJNRzVVekQ5bXZHYTR0N1VCNTIwZ21uVDkySmZhTmozVGZabmcycUV6U29oWWdGUmtPclVSVTlRRlNlZ0hBRThzSGFPU2VhL3R5NGtqVlVYN09CdW1WdVRWaUtnY0ZFMDRmSE9PSHRWcUwxUXhSRWgycm16WkJJZEkyNGg1bTVkdnMxWVBMM2wyL0tHOFhqTFZwRmdSRFgrY2h1dm9kZVB1N0E3Y0VlVGRiYUVaR05uMWRtUS9kaG5lNC9jUDhBVC9yb0Z4TllWQno2VVB4NzVBOFF1YzdkTTZGVXBsd2U4YnVRc2lrQStHTXdwbmgzR01ZdTN6Qjl3S2xDVUhKc2JGdkhaeDZKdDF6cWRSVEFwanJsWlYzWGFOdzVaQzZydTRqVm9jNlVaQ1diUElaMFhpUUJuN0RDYmtUYTNjRjkvd0NVcmQvNnFkWjYrQXh4SGpwanlKNHM4dUplc1cyM1lYb3FPV25Gek5peGhFeWJnY2s1K2hwbWFjM0g2emRVQWoyc2VFclRKS1FNMGozQzd0NjZkQTRkSm9pa2tZaVEwelkyNjdUdVJzNUdDbGlRZ3BtNEZTU2FWQXAvTWFuZ01aVmM3YjNjZW8zSXE3L1lRTk9iVXExMDRZS3R1emxVUlZWdERTYXhuK1VqS2dOV05TeHc0eituWC9mY2YzNkxCd3hqZlhyeG5GL3lJYVM3Y1l1d1JrcG9qM0kxaTZUY0hLTEFVZnltay9ITWhiQ1lkaEFBTThZZ0hVUTZqcW4vQUZjdEMrMjI5NVNvamRnZitiUUIwOW5WamFKL2JJNWppdE9lZDU1YWxPZDNiUlNLTzJFVHNUa3A0QTlMQVo4Q2NaRHJJVGVRV1VMc1gxQk84dlVEZmpEZi9uV1BScFd2SEc4QzFKTUFYcS9maGozc2QydGxWdWNiaUVlZ0hmY0tpOFlzeEV3RkFIRWU1UmZoOFRBQWljcFIrOGV1clQ5TEowaDNSb2Z4RWZ2eHJNL3VIYkZOZDhnVys1eCtDQ2RpZUg0bWpIU3c3ZUFQREd4NzNDNzJGQjRTNWptVTFnUmN2cWY5Q1luTVlDaUxxY2NObzlJQzdpWGM0ZzRIYllkL3UxZGZOZDE4Snk3UEtEbVVBSHZJN0RqVmI4c3ZMNDVpOWI5ajIrUVZqRnl6dDdFaWR2NGw2UjBHdlZqNTlHZjM0SXNtYk1EQ0FKb0tLaVVCRWR2bEVDZ0k5UjNFUisvV0o4N2FwVklHWVA3c2ZTdnQ2ZVR0cnQwNkI5bU5SdjhBajE0L0NJNDlzTEFaTTNrczFpbUpnNXpBUFZQdjlNZ0lHSDRsRWlZZ0dzai9BRTh0L0syU0p6K0twSjkvdDkzMTQwRC9BRHViOGR6OVhMdUFIS0FLbjBxSy9oSDMrM0dub0EyQUFEN1AzZjhBR3JJeXJYR0VXT2dqc0FqdjlnaUg4ZzF4a3c3TWM0Unp5eXlOeVF0dktPcFlNTGpLblphd2RZN2xUbDVHdDNERVR5LzR4a2FmTHlvMXl6SExsZENHYlJsUXlMUkNWNXhKQkh1Z1hWRVpZNENZV3pZcTJnN2NiamMzM2RMU05ROW16QVU3dERVRDhSN3dJUFVjWlVjaGJGeUJhZW5jMi9YTTdXM05peE13bEF1QzBkSElYOG9NWVpGZGFDckxwRlJVVnJnZ09IblpuemxqeXA1ZW9rSXZRSUQ2UnhKd0E4S0lIYXU2eGk5NnJJNWpzc1NxUWZUcnNMSmxGUWpFRkNiaHRBQVRjREZPR3J3NW9pajJiWWR2NWJIZDNHSlhlNEdmRmlHakI0cWFLeEhkWStFYWdEUUREdXdkNzIrbjNKenFTUnU1OVlidDZ1STltR2o3RDkvOU5BV1dKekxGYlppeFRUczU0cnlEaDdJTWFuTDBySlZTbktiWkdCdzJNcEdUckJaaXNzMlU2R2J2bWdxZ3MzV0tKVG9ycGtPUVFNVUJCM3QxN1B0MTVIZXdFaWFOcTVaVjZDT0I0Z2tISTVIaGp3bmdTNGlhS1R3SDl2dHdqSEdOY3NWa1JsZU5lWmEzTDVBNXU4QlkrTlF4VzNDMnNxQzQ1VDRDUnRFSEs0anlGK3JYeFBHU09aT2F5elJzS2Fhb3Fwdm1TeVMzL3dCenJJZW9Hd1FYcXc4NGJURHF0TGlwQ2FpQ2pMazlTeloxY01hMDQxQXlLa25ucFB6M2ViQkxMeWpmWFlzdHN1UDZrdmxDYlRSVzBqUUVaanFxRXFHR25WcU5RQ01OSjRkWittc3kxbWJpYlZZYTllN25TSmFSaDd2ZHNmUXp1S3hnbmFUUDFuVHFpVlY5S08xSHRvY1VsZzZiczNra2ltVnM0WElZUkJGWHVSS0hiVGVQZFE2WlcxeXJ4YWxCeDRBQVV5R1ZSV3ZIdHcvOVF1V0lOaHYxbXNvRGJXRXdHaU11WFlCVlVGbUxNWEdzMWFqQmFWMDBCQkE0YjduZUJqOGh1R1dZYVV5YWc3bjQrQ05hcXltQWR4L3JkYlVKS05nVCtZbzk2aFc1eWJCMUVCMiszYlVaemp0bjZ0c00xc0Izd05ROW9JUFdPaXZUVEZnZks1Nmcvd0Q1djYwYlJ2MGphYlJwakZKbFh1eUt5MC9weUhNa0NxclhQSTQrZUZKeWlKU2cyZGlaQjZ5VVVhdUVsUUVweUtJSE1SUWh3TnNKVGtNVVFFQitBaHJFV1JHamJRL2pHUHAyczcrMXViZU83aWI4bVJRd3lQQWp0QVAwZ1lzUGlkbHhEQ1hMTENtVENQQVNqb3k3eERhWE1VK3hSaXBKd1JpOEJRUjMyVEtrdDNEKzR1aVBsSy8vQUU3ZklKbS9pcDdhOUhBOU9NYi9BSm4rVFllZS9TN2ROcmhGWi9JMXhtcHlaU0dKemVNSEt1Uk5Pekd4RDNuTXhRN0RodmptSkxJSmxhNUl0VmZlSk9DS0Q0MVkrSlpGbkNxZkwrTk5VL2pINGgxSDREcTcvVXErUk9YVWlVNXlNT2pvVXFlcnF4cWUrUUxreTZ2dld1ZStaYXZZMnJBaW9HY2lTQ3ZqSERUMi9makVwbSsweDh4SXJDeGNFV1JBaEVFeER1RGNmdzlOdzMyTUkvY0crMnNkRi9PbkIvRFVmZGplUGZrMkcwTkUrVENNMStqMzQzZSt6ZGo4S2J4V3hNek0yOU90K2pvcDB1WHNFZ21WZkluZW1PWVBoM0dJdVhmZnFPc3N1VXJYNGJhSVVIQUo5dVk2VDBZK2FUNWd0N2JlL1VyYzc0TnFScmdqaFR3Z0QrRmVrSG93NWpmcHY4UDIvZnRvcE5mZmlpZUdBYjVuOGk2dGpLdVIrTW8zTkNlSGN2WkZjeHNmUnJXV2xyWkFqcXEvY1RNYXppWDk0aGttNjdlTXFOaW1IS0VRZDA0TWdYdmUvbG5LY3ZjV0czWGNJYmRWdHhMNVYxSjRHMGxxVUlybFNtWU5NL2FPR0xUOU5lVDc3ZUxtVGY1OXMvVXVYYklIem96T0xlcFpXMDBiVUhKUWpYUlFRYWFTUUd3RFZ3WTNQai9qcEhqOWlpQnIxYTU1ODdaZDJwWjRPaDIyMDJiSE9OMmcrcmo4azhqSXlKbWpwaFVxN0d4Q3lqOVJKRWpiMVV5dWsyS3Fvb1VodEhQcDV5L2J3TEp6UHUwWVhiTGFoazd4N3p0bEhRSzFSUm1EWkNsYUE1VjBpbnFsemxMekR1Y2V5V0Z3Wjl1aXFJV0tCQ29ZTDVnT3BGZHFVMDZuTlNGcnhOUzNEQk9HYWR4NncvajNDdENiR2JWUEhkYVkxNk1GVXBmVlBsRUFNdkp6TWljdlJhVm5wVmRkNjdVK0tybHdvY2VwdFIrN2JqY2J2dU11NVhKL05rSXJ3NEFCVkdRQXlVQVZvSzBxY0NGdGJwYlFMQkg0RkgybXA2K2s0dHJVZmozeE5MN01MQUljMCtJY25ueHRTOHRZWXRLT0tPVytDbkQyYndabFVVVHFSeWhucENKejJOc2lOR3hmVTJIR0Y0WWxNMWZzKzRESUhPVndsODVCSW9VY3Q3OHUyTTlqdUtlYnNkelFUUjFwV2xkTEJsQmNhU2RWRklyOUdJMi9zV3VOTThCMDNjWnFwNDlYUVRUbzZjQy94Q3RXTHVSR2NITTVrQXVRT1BuTVBqNUNCWHNnOFRTV1JHdlV1cW5keUswaFo4aVVXQWlXN1pya1NnWlVmUGtWVHl4MVhxUnlKb0ZFcUMrNTFXWE1mSnNlMDNNVzgyckdiYUpLbUdYdzF5R3FxYWl3cG1Lc0FEeEZLZ1lNZHU5U2QwdU9YWDVRY0tvazBpWUVCbWtLc0dRbHRIZG9RS0JXR1ZBY3ExTGpIdkxPdlpkeTFsU2xSTENQL3MvanRSdFRIbVY1Q1RqMjBEWk1tdmtXQ3IyaHdxYngwMmR2bjBRMWVISzY4YmRaRUZnOGZsQlFESjZEYmZjVXU3cDQ5SStGU2xIcjRpUlVpbEFSUTRMOTU1QnVPWE5pMi9jbW1aK1lia3V6V3dRVmhWR29yYXc3S3daYU53RkNTcHFRY0loNUZldzFpTEpGOHN1VDhiWlJ1RGV1MytaZjJobXdyaU5ha2E4MUNYZUt1MWtvVjRrZ2Z6Ui9tVU1LWTk1dzJIYmZwcXZiMzAyMnU1dTN2QTJValY0TWV3NSthUHNHTXllVmZueDlST1dkaHRlWExxRXZKYXhCTldxMldvR1k3djZlMU1qL0FCTWVzNEhFditQWENDNVFVL3VqazBncEhLWXBpeGxmM0tZcGdFREZONmNBQXhUQnVHbXkrbVczcUtxMlk2YU4vd0M3aWR1UDdnUE9sekdZNWJhcWtaL21RWi9SdHd3MEhPdnRjdU9TMkE4STQzdjJaTW1JRGcydEtRRVVzeVpRUjFiSVl5TGR1M2xad2pobW9BU0xaazFLaVVVaEtVU2ZFQkhyb2ozamsySGVMT0MydVh5aEJwa2VtblU2OVhXY1VsNlhmTlB2SHBUek51bS9jdjJsTGpkVEhySG14NWFOZitaYVRBMTh3bkpVcFRwNkZndXY4ZXFBY1NDYXBzblpLV1NTZHBxOWlrYlh3QlVwRkNuTVU0K20zQURFRHFPM1RmVUJGNlpiWkNSS0dyUTlUNTUvNnVMbHYvbjg1MzNHMmVDYTNwclVpdXUzeXI3TnVHTlNYRlhGd1lpeG5BMWRmdVNRZ0lhTmlrbGx1MU1SYlJiRkptbW9yMmxJUW9pUkFCTjltK3JTczRCYXdKRVBDb3BYNnVzNDE3Y3k3cEp2Vzd5M3gvcVN1elU3V05lcGZzR0tteU56OHhqRVptbXVMTmZkdTRmUEQxb3N6cGcycUtVUXEwdk1TdGRaUzFQY1JqbjFTQVRyR3h2WkFXemJzVVJLZFZpNzd6cGtSQXg0eWZmclg0eHRyaGIvQUg5S2NEa1N1b1owMG5JOWRNV0JzL3BCekMvSzhIcUZ1VVEvN1FKMU13ZU9wVlpqQzRLaVVTcjN3UVNFTGRJSFRnSjNGcnMyQW0xQXlUeXNyRVptL3dCd1MxUDdmWCtNR0hLVVdPSEpybXZXb3JKeWVwWkhmMDZUL1JrelU2aktKcVBWWlZ3a0ViRU5paWRKVXlwVHFpVDhpOGxYdStMK3E3Ky9sV3R0VnBKNkErV0NEcDdpT05aYWdHUU9nR3JkQXcwOVUvVWJsN2JwN25sVDBzckZ5cmRwRUdqL0FER0VySUF4SWE2ak0wZW1TdGU4b2VtWGQ0bnh4RTR1V2ZGOGpjYzlaK3NMSElYSzdNNkxKVElOb1lwaU5heDlXbSt6aUh3eml3aTZaWFRHZzFkY3hqblZVLzhBWmxIeGp1VmhBdmdSUkl1WmVZSXR3RWUyYlluazdGYkVpTktsam1hbGl6QVBSam1BeEpGZmNLVDI2eWFDdHpjSFZkeVpzZUh1b0RUNkJnNTlDdUpURTBzTEUwc0xFMHFkT0ZnTU9XUENUR1hLVk91VzFlVHNHS3M4WTNVTyt4SHlFeHc2K2taSW9FZ0FMQ1ZxRHNnbGJXV3BQRkZ6ZXRocEVxN0YwVXcvS1JUWlFwSHNYTWwzc29lMkFFdTJUZjFZalFCeFFpbXJTV1hqK0hqMDF4SFh1M1JYYkxMNFowOExabW51cUFmZmhRL0kxamxpblYxamozM0VNVVQ3bXR3YzVPVHRiNTljUnNmSTIybkt5MC9XWGRNa0xmeUJ3YUVMS3VhVll6UUR4TUFreU5uekp1OFNLWnFzZ0thWW5jN255VHk5emxDcmNzU2lIZEN0UmFzSFlyUWQ2a3NqcXIxQzZ1SnBYaU9HRHZrUDFYNWo5T3R3TTk0bnhPMnlVRWcxSW12VFhSUXJHN0pwTGRBb3d5WUVjQ0x3UmtiS3BKSjNOOFRjaDRRejl3OXgvaU95UitOOGQ0c3RFRllMVXM4cU5NclRMSFZWbkl0NkNWcnIxOWtyT3ErUEtIVmRDM0Z1bVFpeUNicFR2Q3ZyM1lPWnVXNzlvTG1NaTBqRkFuNWZWUVVjRnE1OTQwSkhFVnJsaXlINW05TCtkZVhVYStVd2M1enpGcHJwbXVXeTgzVVQ1U3FzTkJGK1dvQUJyVElEUEJHT2VZT1JzYzI3Q21NTXc4ZW40M2JKY1ZXSGxpbmFkS0pvVWF1eUZwc2JLQVRnNHFTczdlT0pZcDJ0cHZRZHl6RkZjcnRCcVFUTjAzUWlVb3RUdWx4RExGQk5EUjNyVWh1SFR3cG1lamp4d3hnOU85azNmYWIzZTltM01tMnRXVUtyUUh2bG0wa2xqSUNpNWFnU2hKVWlvQnFNZGtSN2hOU3VDTFVhcmoyMlJKbVhJNms0Rm4wYkJHTUpIeUV0NmtzUnZZWXh6QjJBekZPTVZTakFXSzVNczRNa2tvUXgyeHU4QURtMjN1SzVyNVlKQWRRZXdOV2g0ZG5ENjhjYnQ2UWJsc1pqK05uVXJOYnlTSVFvek1XalV1VWhOUHpGN3hBUDh1UFVjdjhBUEhMREdPZU1jMFhBMkhWYjNTNWl0eDlzbTN6V2wyT2JDVWNSbDZnSTZ5VVV0cVl0bEt4VHB1YXFENXlyR3VaVlpxMVNXUk1xcW9LWk93L1hjN25kSWJ5TkxHUFdoRlRtbzZzc3dmdStuRDMwOTViOVB0MjVadTl3NXJ2dmhkeGpiU2hNY3owcUc3d0VicXA2QlFoajBrVXhXdWNXdWJuVXRuZG55enp6aXpDSEVHeDFXWGhxNHluYm5CMWEwb1NLVXJYNSttVDBkSlZkQ3UyVXlSVHQzVWZLTUZaZ3gzZ0FVaWFhcWFwZ0Z4YWN0OHk4eFhrdGdxczlqTVVDS0FtWEFtckJsSTd3ejFzQjBjTWUwUFBYcHB5UHRXejdyeTlhbHVkYlEzSHhFM21YRkhEbGtRbU9aSGhGSW5JSGxLYzZNeERDaHBqQUZ4eXhlcXBUcVh3YXhXYTIyYXVVbGJITXI3aVBJU216Tk5vcWRLUFlIMHVsRll1aEpsQlc1NWNhUWJseVFXQ0NQaGhmSTNLQ3F5ZnpkbGwySEp1eWNveEtlYTV2TTNLSmFpMUFZRndhRlFaSTNkVnBYcnpBcHFGU0JVZk8zcUR1ZlBHNlhFMjFSL0Q3Vk95bWxWY0FoQUhOV2pqWTZtMUVpZ0ZUd1BIRE5lTVhEU2o4ZUg5aXlKTVdHdzVrNURaQVFia3labnpJU3BIZHdzWlVER09sQ1FERklmcEZFcFRGUSt6ZUlpMDBVTmlsTXVaZFVQS0xIZjhBbWU3M3RJN1JGRVcwUVpRd2loMEFnQTkvU3JOVWl2ZXJUZ01DbGp0ME5uV1h4WERlSnM4L2RVZ2U3QmphR2dBTWhpUnhOTEN4TkxDeE5MQ3hOTEN4TkxDeCtLL2c5T3Q2cncrbDhTbnFQUDJlRHdkZytYemVUNVBGNDkrN2ZwdDhkTEN3aDdrekUreXRONVhjTUpXMFF0UTVHTExtQjdPY0xHMlduZVlXc2w1emRoclNoeFhyOWxXTk1BNC84ZjFwb280MzI3T20yclEyQi9VZVBicTdhcGZiYThIOGdkWDhaRWxQcXdOWFM3RTAvZmJUY1Y2cERubDFaWTUvUmVNT2ZWNDlzLzQ5KzVwelRnWVZRb0daUW5LVGpES1pBVlFTRVRDZ21jK1ZNZVlreUpzVU53TjZ0Nm9jUzdiaUhRZE5KZDQyZUkwM1haNEpaT2t4M1lYUHB5aEZQcngyanRwU2Y5dGNrSExqSDdLZUwzWXNodmdYM01RUkJ1VDNCc0JuWUZlQ21NZ1RoWXcrcUtQdTA1UmNLTkU4eGtSTE43Zk1ZdS9kMzlOZWE3N3lIa1JzVGFxLy9NbjkySkUybk1IVGQ5Ly9BRW84Y1V0M0dYa1lSbXM5eng3bG5MV1dpU0ZPWmVKNHpjWVRVSmRZZ0ZNS3laVktOUjh2M2p0TW51QmZTdWt6Z08zYU8rMnZWTjUyS1ZxYlZzMEVVblc5MkdGZWorc0tZWUcya1VmN2k1TEwvcDA2djRUaXI4SlFuczZRR1ZHVWRaYmd0ZDgvcFBBK2x6M094cG1abGUzTXY4RERVRy9LR3RWV0JKTmQ0Q0ovb2pRanJyODNUYlQvQUhoL1VpVGJHTjBubDdYcEdTRzNQZHFLVTBFeVU0Y09qamxqcFpyc2F1QkcycWJySWtIMjVZZWt6OUo2VnQ2SDAvb2ZBbDZQMG5qOUw2YnhsOEhwL0QrVjRQSHQyOXZ5OXUyMnFyT3F2ZThXQ01VcGx3eDVPdU1jNG1saFltbGhZbWxoWS8vWiIvPgogICAgICAgIDxoMT4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5lLUZhdHVyYTwveHNsOnRleHQ+CiAgICAgICAgPC9oMT4KPC9kaXY+CgoKPC9kaXY+Cgo8ZGl2IGlkPSJiMyIgY2xhc3M9ImJveCI+CiAgICAgICAgCiAgICAgICAgPGRpdiBpZD0iZGVzcGF0Y2hUYWJsZSI+CiAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgIDxkaXYgY2xhc3M9InBoIGVtcHR5IHBoNyI+CiAgICAgICAgCiAgICAgICAgPHhzbDp0ZXh0IGRpc2FibGUtb3V0cHV0LWVzY2FwaW5nPSJ5ZXMiPgogICAgICAgICAgICAgICAgPCFbQ0RBVEFbPGltZyBzcmM9ImRhdGE6aW1hZ2UvcG5nO2Jhc2U2NCxpVkJPUncwS0dnb0FBQUFOU1VoRVVnQUFBc2NBQUFMbENBWUFBQUFva2tzVEFBQUFDWEJJV1hNQUFDNGpBQUF1SXdGNHBUOTJBQUFnQUVsRVFWUjRuT3pkVFZJYldkYUg4VlRITzFRRTdoVkFyUUJxcUpHcEZaaGFnZVdwSnBaWFlMeUNnZ2xUd3dvS1ZsQXdZbGhvQllWVzBGWUVjNzF4cVpOMklrdEltWGsvenJuMytVVVFYZDNSWll1VWxQblBrK2VlTzFndWx4VUFvTDNoWkhTODhpOGRWVlgxWnMwZnRQci82K0tocXFwdmEvNjkxZi85NGVuaWZ0My9Ed0N3QThJeEFEUTBBbThkZE4vSVAxZnl6NGVHanRlOHFxcEgrZWZIeGovZjF2L2IwOFg5NDRaL0Z3Q0tSRGdHVUJRSnYyOGE0ZmZJWU9qMWJkNEl6ODBmcXRBQWlrTTRCcENkNFdSMFVGWFZnYlF6MVAvc1F2QWU3M1luZDlLNjhTQS9ydUw4WVBEM0FJQ3RDTWNBekJwT1JuWGx0LzV4SWZndDcyZzBNNmt5MTZINWdUWU5BTllSamdHWTBBakN4NDB3dk0rN3A4NUNndkl0Z1JtQVJZUmpBQ29OSjZObUVENG1DSnUyYUlUbDI2ZUwrOXVNZjFjQXhoR09BYWdnQytXT0c0R1kvdUM4elNRdzMwcGdadUVmQUJVSXh3Q1NXQW5EOUFtRHNBeEFCY0l4Z0NnYWJSTHU1eDFISFZ2VVlmbWFOZ3dBTVJHT0FRUWhDK2hjRUQ2aFp4Z2UzRFRDTWd2OEFBUkRPQWJnamN3WFBwRWZXaVVRU2wxVnZtVGVNZ0RmQ01jQWVwRjJpYkZVaDB2ZVpRNXB1RWtZMTFKUnZ1WTlBTkFYNFJoQWE0MUFmRUs3QkJRaEtBUG9qWEFNWUNjRVloaERVQWJRQ2VFWXdFYlNRenlXSHdJeHJLcUQ4aVdUTHdCc1F6Z0c4SUpNbVhEVjRTazl4TWpRWElMeUdWTXZBS3hET0Fid2JEZ1puVWlGbUJuRUtJV2JlbkVtclJkc09nTGdHZUVZS0JodEU4QjNWN1JkQUtnSXgwQ1pocE5SSFlpWlJReThOSmRxOGlYVlpLQk1oR09nRUZTSmdkYXVwRGVaalVhQWdoQ09nY3dOSjZOakNjVHZlYStCVG1ZU2tpODVmRUQrQ01kQXBxUjFnb2tUZ0Q4TGFiazRvK1VDeUJmaEdNaUlqR0diMGpvQkJPZGFMazRaQndma2gzQU1aRUQ2aVU5bFB2RWU3eWtReloyRVpLWmNBSmtnSEFPR05VSXgvY1JBV25mU2JzRlcxWUJ4aEdQQUlGbGtOMlhERGtDZHVWU1NXYndIR0VVNEJneVJVSHpLZkdKQVBVSXlZQlRoR0RDQVVBeVlSVWdHakNFY0E0b1Jpb0ZzRUpJQkl3akhnRUt5ME82TW5tSWdPeTRrVDFtNEIraEZPQVlVWWZvRVVBeEd3QUZLRVk0QkJScWJkM3ptL1FDSzRrTHltTTFFQUQwSXgwQml3OG5vVklJeG0zY0E1V0xIUFVBSndqR1F5SEF5T3BHK1lyWjVCdUFzNUp6Z05oUDV4aEVCMGlBY0E1RU5KNk1qdVFBeWdRTEFPa3kyQUJJaUhBT1JTRi94R1l2dEFPem9UaVpiUEhEQWdIZ0l4MEFFdzhsb0tsTW82Q3NHME5hVmhHUmFMWUFJQ01kQVFMS0poNnNXSDNLY3MrSDZRdXRLM3JmR1AxZnl6NnNCNXFFWmFvYVQwYzRuM2FlTCswSHp2OHZuYVZYemZ6dVFuL3FmNldmUHgwSmFMYzVLUHhCQWFJUmpJQUJhS014YU5BSnVIWHJyT2JRUFBpcDNmY0p4ajcrekR0Q3UzLzFOSTBRVG9PMmgxUUlJakhBTWVEYWNqTVlTakdtaDBNc0ZqRWY1ZVE3RHNUWmpTQkdPdDVGRm9tK2tDdjFHUXZRUm4ySFZ6cVdTVEtzRjRCbmhHUEJFZHJlN1pBcUZLak1KdjQ5U0FYNU1QVWRXWXpqZVJKNkExRUg1UVA2VHo3Y2ViRVVOQkVBNEJqeGdJdzhWNmlEOElDMFFLcmZsdFJTT041RktjL09Id0p6V2pleXlSeFVaOElCd0RQUWdJZUdTQlhmUkxhUVM3SUx3cmRZZ3ZFNE80WGdkK1M0Y1MyQStwcGM1dW9VRVpLcklRRStFWTZBanFSWi81dmhGTVpjd2ZDdFZZYk9Ma1hJTng2dWt6YWdPeXNmY1FFWkRGUm5vaVhBTXRFUzFPSXBGSXd4ZnArNFQ5cW1VY0x4SytwZGRTRDZoc2h3Y1ZXU2dCOEl4MEFMVjRxQmN6L0MxaE9Gc3gxU1ZHbzVYU1dXNURzcnZkTDI2YkZCRkJqb2dIQU03a0F2NU5kVmlyK3JxY0IySWk3aUFFNDdYRzA1R0oxU1ZnNkNLRExSRU9BYTJZT3RucnhhTk1Gemt4WnB3dkoyMExvM3BWZmFLTGFpQkhSR09nUTJrUi9LU1I3Njl6U1VRWDVhK3E1ZUV2cjliL0N1L2NzeSt0MStNQ2NxOXVlL2lTZW1mS1dBYndqR3dobXkzZTAyMXVETXF4RC9QQXU2NlZmTzhzWlBmZy9WcEhYMUlVQjdMRDYwWDNYMTV1cmcvdGZyaWdkQUl4OENLNFdUa3RuNyt5SEhwNUtyVVFMeXl3T3c0OEkxVnR0TThkdFZvdlJoekU5dkpuVlNSYWJNQVZoQ09BY0dpdTg3Y2xJbXpraGJWMVJSVk1tZU4xcFVTZzNMZGRrRUxWRHNMQ2NobU50RUJZaUFjQXo4dXJwZFVvSGEya09OMVZtZ1lxd094eG0yVDd5UWtYeXA0TFZISk9vR3hiT1ZPMjhYdWFMTUFHZ2pIS0I1dEZLMlVIcnltaHZwZDU0MGJtT0llbmN1NkFmZGV2VmZ3Y2l5Z3pRSVFoR01VUzhMT0xXMFVXOVdMNjA1THJCSlhQelovbVJwOXNyQ1FnRnhrWlpCcWNpdTBXYUI0RmVFWXBXSWF4VTdtTXQrNXVGN2ltclJQbkdZU3F1WXk1N2JZelNDa2ZXcXF0QjFHazA5UEYvZG5wUjhFbEl0d2pPTElwaDUvOE01dmRDZVZ4cEpEMUlHMEpPUVlvdTVreDdRaW53SlVQOTdmVTFvdVhzWFcweWdXNFJqRmtNZXJaMXdRTjdxU1VGejZwaE1sN0lpNGtEYVpvcXVEalQ1eXF5MHpvYzBrSUxOcENJcENPRVlSR05PMlVmSDl4RFVKU3RlRlBYSm5FUlo5eWRzc0pDQVgreVFKNVNFY0kzdXlXY0F0bGFFWEZsSkZMM0tTd2FyQ1B5UHVzM0JNZGZCZm1mV1orMFFmTW9wQk9FYlc1RUwzbFhmNU8wTHhDajRqMzMwb2NVVGZKb1RrdGE2ZUx1N0hDbDhYNEJYaEdObGlmdkVMaE9JMStJejg1UHpwNG42cTdEVWxSVWoreVV5ZU5IQWVRYllJeDhpTzlBOWVzcFhzTTBMeEJzUEo2SkxGbVd0UkhWeURrUHpDWEhyVmFjVkJsZ2pIeUFvYmU3eHdKWE50Q2NVckNNWmJFWkEzS0dTYXlTN1lNQVRaSWh3akd5eTgrKzZLNlJPYkVZeDNSa0RlZ0JGd0w5Q3JqdXdRanBFRmRyeDdkaWVobUVyT0JnVGoxZ2pJcjJCMituZjBxaU1yaEdPWXg3UUJ0Z1hlQlRzamRrWmxjQXQ1YW5WVytMYlUzRWdoRzRSam1GWjQ0Rm5JUXJ0VEJhOUZ0ZUZrZEZKVjFaK2xINGNlZnVPSnhIYnlCT3V5NEVWN2JDcURMQkNPWVZiaGo4aFpiTGNqMlIzeGdkN1FYdHlOMkJGOTdMc1pUa2FuQmZjak0rb041djJIdHhBV0ZSeU1aMUxGRzNQeDJWbnB2ZWcrN01seHhBN2thWTVydGJncDhIaTVTVUczMG84Tm1FVGxHS1lVUE1ONElZdnQyTDYxQlRiNThPNExiVHp0Rk54cXdiYmtNSXR3RERNS25tRjhJeTBVUE5KdVFVTEpYMlplc0IyL0VuamFrMWFMejlaZWQwOEVaSmhFV3dWTUtEUVl1eWtVdno5ZDNKOFFqRHRod2tJWUhOY09wT0wraXl4YUs4V2V0RmdjRmZRN0l3T0VZNmhYYURBK2x3VlE5SGwySUZVNnR2a040MUNPTDFweU43bFBGL2Z1aWNZbnFhcVdnSUFNYzJpcmdHb0ZCbU5YTFI0ek5xczdwbE5FNFlMZEFZdEN1NVBQNldWQnM1RVhjbTdqaGgvcVVUbUdXZ1VHNDdwYVRERHU1NVJnSE55ZWJIcUJqZ3FzSXJ2UHpKK3lhUk9nR3BWanFGUllNS1phN0lsVTQvN0o0cGV4NFJmNjRmc3JzSXJNcm90UWpjb3gxQ2tzR0Y5UkxmYUtYdGk0T040ZUZGaEYva29GR1pwUk9ZWXFCUVZqK3U4OG8ycWNETlZqajJUaDJtVWh4UUVxeUZDSnlqSFVLQ2dZMzhsaUpvS3hYOU9jZmhsRHFBQjY1R1lDUDEzY3U0RDhKWnRmYWpNcXlGQ0p5akZVS0NnWWYyS1h1ekNHazlFM0Z1SWxzWGk2dUdlcjRBQmtJNXZjdHo5bm94Q29RK1VZeVJVU2pPZXlzeGpCT0FDcFBoR00wOWlqK2hlR3JFVTRrRjB5YzhVY1pLaERPRVpTaFFUakcxbDBSMlVrbkpOY2Z6RWpPUDZCdUZuU2JwZE1XYXlYS3dJeVZLR3RBc2tVRW94cG93aE1Qa2YveS9xWHRPRy9iQW9TbG9USDI0eWZrdEJpQVJXb0hDT2xuRmRrMDBZUkQxVkxIWGdmQXBQUWVDQ0xlbk5VVjVBUGNuMFBZUVBoR0VrTUp5TVhqTjlsZXZUdmFLT0lpbENtQSs5REJOSm1jWnp4TkFzWGtLL2xpUkNRQk9FWTBVa3dmcC9wa1Q5M0Z5NGVMMGQxWE5EdnFobnZRMFJQRi9kdUE1YmZNOTAwNUZBcXlBUmtKRUU0UmxTeXFqM0hZTHlRZ2ZiTTJvMUlSbDB4cFVLSFBSWlV4U1d6MHQxM1lKYmhyM2NvWSt5QTZBakhpRWFDOGRjTWovaGNGcEd3MDFOOGhERmRxQjVISnUxYng1bU9lM3NyVHhxQnFBakhpRUlxZkRrRzR4bjl4VWtSeG5UaFppV0J4cmkzOHd4L3ZmZkR5WWlGellpS2NJemc1RkZyam8vSHJ0dzJyL1FYSjBVWTA0WDNJeUZwNi9xUTRhLzJrWTFtRUJQaEdFRTFaaG5uMWhmcTVoZHpzazV2di9RRG9FenUyNytySisxZHYyYTRVTy9yY0RKaUlncWlJQndqbUV5RGNiM3dqc2Q4aWJINFN5ZmVsL1FhZmNpNUxkUzc1UE9GR0FqSENDbTNUVDRXTEx4VGhURlBPdkcrS0pCcFFONWp4QnRpSUJ3akNGbEFrZE1tSDNPMk5WV0hDcEpPdkM5S3lIb0lGNUN2TXZxMUNNZ0lqbkFNNzJUaHhNZU1qaXdUS1hUaTRxZ1Q3NHNpTXNsaW5GbEFkazhrYVcxRE1JUmplQ1g5WURtTmJKdEp4WmlKRkFETWtvQ2MwNWJUYnNUYnFZTFhnUXo5SDI4cWZHa3N3TXZGVmV5SkZESVAra0IranFRSzkyWkw3N1pyK1hpVWY3NXQvT2RENXFHZUNpV2lrL1Bja2JRcnZHbTBrUnhzbVo3aWJyUy95YytEZkdjZllqNlJjbHRPRHllang0d0tHSitIazlHRDdCUUllRE5ZTHBjY1RmVFdDTWE1TE1BTEhvemxtQjAzZmtJY3U3bGNpTjE3Yy8xMGNmKzR3Nzlqd25BeWNyL1QyMXgrbjR4RXY2a01hVGdadWRCN0l0L1RvMERqQSs4YTM5UGIwRGUxbWUxV3VtQTlDSHdqSE1NTDJlTHpmU1pITTlqRlhRTHhpZnlrV0xBNGx3MVpMcTFmVEFqSGF0MDlYZHliM3JsUTJzUEc4ajFOTVV2N1JyNm4xNkdDOG5BeWNodUcvQkhpejA1Z0x1dENhSCtERjRSajlKYlpTYllLMFdjc3crdkh5aVo0ekdWUnk2WEZpd3JoV0MyVGxXT3BFSS9sUjlQbU1qZnlIZlhXT3BEaGs3NHFoNXN5NkVFNFJpOVNZZms3dzZQWU95RExCY2hkYUtjR2RuSnpLOWxQTGJWZHlHS2N6d3BlQ2w3NjRucGJyUndUT1lkTkRUejU4bkl6bTJrd3JwbjY3RUV2d2pFNms1UHNZNFpiUTljNkJXUTVMbFA1c1hac1hPL2oxRUxMQmVGWUxSTUJSUmEvbmhwOCtyQ1FrSHpXOGR5VWF6Q3UvYzRDUGZURktEZjBjWjF4TUs3a0F0SnEyTHdzZEhtVTBHYngyTGlnOExmcklaZkh6RUJXM09kYTFrajhaYlF0WjAvT0w0OXl2dGxKSWNHNGtpMm1PWGVoRjhJeE9wRWQ4RXJvOTl3cElMc3FsQnNwSkN2QWM3aGhjSStZSDVUUEVjMW04a1ptMUQ1MWtNL3pReWFMaDkxNTVxczc3MGhyeUVZRkJlTktqZ3VWWS9SQ1d3VmFrOFZsZnhaMjVOYTJXTWhGNXpTekhRRlh1ZDk5ckszVlFoNkwvNlhncGVDbDM1NHU3bFhOTzVmd2VKbDVPRHlYZFFQcnpsR2xCT09tODZlTCs2bWVsd05McUJ5akZYbGNkVm5nVWZ1cGdpd1gzTnZNZzNFbHYvdmZDcXZJVkk1MTBuWVRkU3FMaG5NUGh4L2xIUFc5aWx4d01IWStTaUVIYUkzS01WckpaSHpXb2tmcncwdzJBempKYkp2c1hia0ZleWRhUnI4Tkp5Tk9ZTW84WGR3UE5Md2lDWWJYQlk3N1c4aGk0T3Vld1hnbXUvNVpiaE5ieVB4amJxVFJDcFZqN0V3cU1OWXZORmR5d3A5MS9QZmRoZVovaFFialN0Ny9yVDJPRVhWOUh4SEduWWJqS3AvUHgwTG5ZTy9KK2VsL1BZUHhzWVJzeStnL1JpZUVZK3hFK2p1dGo4MTYzcHhBcXA3SEJLdk85dVh4cllaSGxtd1pxMHZ5Q3AxTWNMak5mSkpPU04vWFZ6eGQzTHNXdWcvR2Y1OURXVUFPN0l4d2pLM2s4YVQxUHVNWHUzWVJrSHR6d2VQUE5xT2tBaUVjNjVMMC9aRFBZeTRUWTFMNGFlRnhKZ0g1b3hSNGdKMFFqckdMU3dNN3ZMMW10dTd4SUFIWmk2K0pBN0txcVFoSTkzNDBnakc2MmJqcGtRVGtMOGFQNjNXYm1mVW9HK0VZcjVJTHpqdkRSK25WWGU0SXlGNGtDOGd5WG02UjR1L0dUeGFweHYwUmpIdmJ1aHVvN0hwNFplZFgrc2xlb1pPVzBBSGhHQnZKMkRiTHZWcnpYYlovSmlCN2tiS0NUUFZZaHlRTG53akd2ZTI4VGI2MHBsa095TzhVdElMQkFNSXhYbk5wdUhkdjBXYmtHQUhaaTdORVV5eFlqYTVEOVBkQlBtOEU0KzUyRHNZTlUrUG55VE8ybDhZMmhHT3NOWnlNcHNiSElCMjNmY1JMUU81dGI1ZXR0Z01nSE9zUXRZTGYyT0FDM1hRSnhqbWNKMm12d0ZhRVkveEVxakYvR0Q0eUg3cjJQc3FKbnd0dWQzdXhqNSs4WnpjeC8wNzg1Q2JCeGpDTWErdm50dXQ3SnYvZTJIQy8vMXNwQUFGckVZNnhqdVc3NmkreXNyb1RtZDJiKzNiUW9hV1lLMHIxT0syb3gxOCtYeVZ1aWV4VHIvRm1Vb0N3UEI3dGxQWUtiRUk0eGd1eUM1N1ZpODZWcktqdUpKTjV6bHJFbml0S09FNHIydkhuQnRhclh1UE5KQ0JibllGTWV3VTJJaHpqTzJtbnNMb0wzdHBaeGkxZDg1aldxMmh6UldtdFNDcGFTd1Uzc043MURvakdaeURUWG9HMUNNZG9zanEyYmRGbFlVbVRqUGV4dkFCUm83M0lueW1xeDJuRURLdVdKK2hvOWE3dlZ2RHl4TTdxelNudEZmZ0o0UmpQREUrbjhCR00zeGlmNTZ6Wis0anRGWVRqK056R0gxR091M3lPTEc5SXBObWxoNmM4WTZNVExHaXZ3RThJeDZnMysramNxNXZZMU1PdVhHZFVvNEtLY3VHaHRTS0ptRGNrQkpodzl2cGVBK1Q3ZDJKMGdzVmJOZ2RCRStFWWxlRkhsZWQ5SmxOVVAvcXMzL3Q3U1ZoalArS0Zod0FWVjZ5cXNmdjg3Q3Y2dlhQMHNXOTd3ZFBGL2FNRVpJdk9Fc3hvaDFLRTQ4SkpyNW5GZG9xN3A0dDdId3NwYUtlSUk4cVRDWG5FYjNYMnFqWFJXaW9NUDlteXB2Zk41ZFBGL2EzUkJYcXgxMGhBTWNKeHdReXYvRjc0cUU1SUR5T0w4T0tJV1QybTl6Z09xc2I1ZWV0ampZRGhCWG94MTBoQU1jSngyVTZOdGxQMFdvRFhRRFVxcmxqSG0zQWNCMVhqUFBrYWJlWnVhdVlHanhDdFdTQWNsMHA2YlMwTzB2L2tZUUZldlFpUnFuRmNVYXJIdEZaRUVhV2xncXB4RXU5OGpEWXp2RUJ2WHpiRFFzRUl4K1d5MkZ2bE5odnc5Ym81K2FWQjlUZ1BWSTN6NXFWNkxJVU1pNXRzVEpsOVhEYkNjWUdNYm5neGw4ZDB2VW12dGRVVjFkYkY2ajBtSEljVi9PYWFxbkZTM3I2ak1sSElXdjh4aS9NS1J6Z3VqT0VOTDA0OGJsRjd3bHpqcElKWEErV1J2OFYrUnd2bVBscWJka0RWT0owOXp6ZXhGdnVQMzdFNHIxeUU0L0pZWElUM3hmUEZtS3B4V2xTUGJhUFh1QXplenBPTi9tTnJXSnhYS01KeFFhU0h5dG9pdkRzWkMrU0ZWTTdaZ2phOUdGVkJMbXhoeERpdVZJM1RlK2R6VXd3cGNGaWJmOHppdkVJUmpzdGlMU3dzZlBhK0NhckdPZ1N2SHN2Rm1OWUt2NEszVkZBMVZzWHIrVklLSFhmR2pzR1VuZlBLUXpndWhORU5MOGF5SGFsUDlKRHBFYU1pUTJ1RlgxU055eExpZkRrMk50Nk54WGtGSWh5WHcxclYrQ2JRSEZVcXgzcnNSMWp3UW11RlgwR1BKMVZqZGJ4L1A2WGdFV3UzVEYvZU05cXRMSVRqQWhpODRIZ2IyOVlrRzU4d3BVS1hvRlZDYVFHWUdUd3VHczBDUE1sWlpYRW1iczcyUTRSQ0tYeFlHKy9HalhaQkNNZVpNenE2YmV4eGJGdlRVWUEvRS8yOHBYcHNSdWlxc2ZzY0hPWjF5TElRNnZ0cHJiMGl4cmtLU2hDTzh6YzFWaTA5ZjdxNHZ3MzBaeE9PZFFyZFkwcmZzUitoanlPOXhqb0ZPVzlLQWNSYWV3Vzl4NFVnSEdkTXFzYVdIbFBPQTE4Z0NjYzZCYTNJU0NzQXJSWDlCRzJwTUxwZ3VCVEJ6cHNHMnlzT0k4MW9SMktFNDd5ZEdhc2FoMnFucUJHTzlRcGROYVMxb3AvUUZUT3F4bnFGUG05YWE2L2dzMW9Bd25HbVpCSEZlME8vM1ZYQWRvb2FpL0gwQ3QzUFIydEZQOEdPSDFWajlZS2VOdzIyVjhUYTRSTUpFWTd6WmVudWRoRzYvWU9GRkNZRSs4eEtTNEMxMWZGYTNBUitva01sVHJuUTUwK0Q3UlZuYkF5U044SnhoZ3hXalVPM1U4Q0d0ekp1THhTcXg5MVFOVVlNVTBQdEZYdU1IY3diNFRoUGxpb3hkNEUyKzFqRkFIY2JRbDV3Q01mZGhEeHVCQXc4azZjN2xxNWRiQ3VkTWNKeFpveFZqUmNSZTgwSXh6WUUyNGxLbms3UVd0SE9WYWluT3ZJK3Z3djQydUZQbExhMHA0djdNME9UWmFnZVo0eHduQjlMY3hqUEl1eTRCWHRDVm8rb0hyY1Q4bmpSYTR4MUxDMTJvM3FjS2NKeFJveFZZdVpQRi9kY0hMRk95T3J4cGJHeFVTa3RRclU4R1Z3WGdVaGt5L2R6SThlYjZuR21DTWQ1c1JRMkdZV0QxMUE5VG8rcU1WSTVOWFFUUy9VNFE0VGpUQmlyeE54RW1Ha00yNEpWanduSE82TnFqQ1NrejkxS1JaYnFjWVlJeC9td1Vva0pQdE1ZMlFqeW1aWldBVm9yWGpjUE9FV0dxakcya2hhb095TkhpbXRhWmdqSEdUQldpVW0xQ0k4NXl2YWNCSHhjeVhiU3I2TnFqS1pVQzZldDNFanRzV3RlWGdqSGViQnlBcGtubktieGtPanZSWGNoSDFjU2psOFg2dmdRSUd4S0VvNmwvZTdLeUJIamlVaEdDTWZHU1dYTlNpWG1sSjN3MEZLUXhTNnlJbjdPbTdIV1hJNlBWL0krOHZnWmJWbFpuTGRQOVRnZmhHUDdyRnhzWnRKRGxncnpsRzBLV1QxbVlkNTZvYjZuVTNrL1lVK3lKMi9TaG1kbGZqL1Y0MHdRamcwelZvbEoranJaYk1TMFVLT1NhSzFZei90eG9XcHNtNEluZm1lR3FzZFJkaE5FV0lSajI4WkdLakYzU2thM1dkbVdGQzhGcVI1TDZ3Q2ZpWmRtZ1c0a3FScmJsWHhpaExIUmJsU1BNMEE0dHMzS3lVSkxIeGJWWTd0Q2ZZYW9IcjlFMVJpclZKdzNwUzNQd2pxQnR3Rm50Q01Td3JGUnc4bm94RDNDTWZEcXJ4UzFOREN4d3E1UWkxM29PMzRweFBHdzhvUUw2Mms2YjFxcHlsSTlObzV3YkJlUG1OcGpWejdidkgrVzVNYU4xb3AvM1FWc3FZQmRhc0t4b1kxQjNyT2x0RzJFWTRPR2s5R1JlM1JqNEpWcnFocFhiRmx0WHFqcXNaV1Y4S0dGYUtrWUczbkNoUTBVbmpldFZHVzVLVFNNY0d5VGhTL2RRdWxKek1wMnBGZ3Z4R2VLMW9wL2hUZ09QRjYyVGQzNVVzSzZoZk00TTQ4Tkl4d2JZMmpUajFUYlJHOUQ5ZGcyNzlWaldRbC9VK0N4YkxyeFBhNkxxbkVXdE40NFdyanBZbE1Rd3dqSDlsajRzaTBVUDZxbVNtaGZpQ2NucFg4dXFCcGpIWlhGQktySENJMXdiSStGbG9venJkdEVzMjF3Rmc0REROcS9OckxKUUFnTDM3dFhValhPUXBCdHhEMnljUFBGV0RlakNNZUdTQ0N3Y01IUlBqdTI5Q3BoRHJ4ZUdPVm1ydFRQUmFqeGJiQk45WG5jVVBXWWhYa0dFWTV0c2ZBbFV6V2hZZ09tRTlqM05sRDF1RVJlZjI5NVh5eE0wOEhyTEd5UVk2RjZQR2FzbXoyRVl5UGswY3c3QTY5Vy9jbEt3anRUSyt6elhUMHVzYlZpSWIrM1QvUWEyeGRxNXJWWFJxckhiZ09jRXdXdkF5MFFqdTJ3OE9XeVVEV3VzVzJ3ZlZTUCsvUGRhMHpWT0ErV25xNVpPSmZUWm1RTTRkZ09DeTBWWmlwR2h2YnB4K3Q4Znk5S2E3bnhIU3lvR3RzM0QvQTBJUmdqNTNJVzVobERPRGJBeUVLOEcwTlY0eG9YY3Z2ZStiem9GRGJOeE9zMEFrTTdkK0oxRnMrTEZsNHpDL01NSVJ6YllPR1JqTG1LRzlYamJQaStNSmJTV3VINzkrVGliOS9jOTFpL0dPUTFhMTh2UU4reElZUmo1V1NWcS9ZdjFVemgvdnU3b25wczMzdlBqeXhMNlVmM2RrTXJ4OS9DenAxNG5lWHpvZllDamRzeGo0QnNCT0ZZdnhOWjdhcVoyVDVOcVRqTUZMd1U5T1B0b2k2dEJybC9KbWFlMjZDNHliUnZackZxM0dEaE9rUTROb0p3ckovMkw1UEp4M0FyZUJ4c0g5WGpkcno5ZmxTTnMySDZQQ2diK1Z3cGVDbXZlYy9NWXhzSXg0b1ptVzFzUGtSSVM0ajJreXEyODNseHo3M3YyT2Z2UjlYWXZuUERyWEZOVkkvaEJlRllOd3Rmb2x4R1gwMEwzQUFpTjk1Mm9wS1dnMXhiSzd4dDhDREhtNnF4Yll0Y2JuQ2tKVXI3cGlETVBEYUFjS3liOWkvUmxUektNazkrRDA1YXR1MTVyaDduT3ZQWTU5TWVXcExzTzhubFBDNjBQODFrNXJFQmhHT2w1TXR6cVB4bFpoVWVaUEQ5dVlLWGd1Nm1IbnY2Y20ydDhQSjd5WEVtSE51V1N6dkZkMFpHZE5KYW9SemhXQzhMNDl1OGJTQ2d4ZFBGL1pUcEZhWjVxeDVMTmUwbXMrTno0N0ZLT0RVd1NRZWJ6ZVI4bHlQdDFXT2VVaXBIT05aTCs1Y241MjEyVCtnL05vM3E4V1pValZISitlMDQ0eU9oUFJ3ZjBscWhHK0ZZSWRtR1ZYTkx4U0xuMWZ5eVdPbVlnR3pXbnNjbkw5Y1pmUTRXSHNjdVVqVzI2emtZWjlabi9JS2N3N1UvOWFHMVFqSENzVTdhNytpdmN6NnhWajlXUFZNWnM4dkw2bnY1bk9keUkrano5K0N4c0YzakhGdmkxcUMxQXAwUmpuV2lwVUlCcWJKOUtPRjN6WkRicXRYWDk0aHczQ0RIZGQvWGkwSlVIMlRoY2Ziazk5VDgxSWZXQ3NVSXg4b1ltRkl4TDZUcThJeUFiSnF2Nm5FT3JSVUxqNkdJVFQ5cytwREJicVp0YWY5OWFhMVFpbkNzai9ZdlN4RlY0eVlDc2xsVWozL3dFaEtvR3B1MEtEUVlWd2F1VjdSV0tFVTQxa2Y3bDZXSVIzS3I1TUx5SzR2MHpQRlY1YlIrVStnckdGRTF0cVZlZkZkaU1MYXcwK1doeDhrNjhJaHdySWg4U1RTM1ZOejQybmJXSW1rbk9XSU9zaW11ZXR4N2dhdTg5OW8zRnRqRVN5c1VWV056M0hucXFLUTJ1QTFvclVCcmhHTmR0SDlKaXF3YU56WEd2RjNwZVZYWXdsZTEwK3JuMzFjNDRCR3dIVmRTTVM2Mm1ORkFPRVpyaEdOZENNY0d1UEZlVHhmM1krbERwczFDdjdjK3FzY0dMcktiOUg3ZGN2emVCbnVGOEtYdUx4N25QbTV6VndaMnVzeDVNeGF6Q01lNmFQNlNYSEd5ZlVuNitGeWJ4WjJtMTRXMWVsZVA1ZkcwdFphYW1hZnFJYjNHK3QxSkcwV1IvY1ZiYUM3czdBMG5JNnJIeWhDT2xaQXZoK1lkcDZnYXIrR0N4OVBGdmJ1cCtVUVZXYlZTcThkVWpmUG56anVmM0htSU5vcU50RisvcUI0clF6aldRL09YdytlTTFDdzlYZHk3YVFadVJ2VjU2Y2RDTVI4OXM5YStCejVlTDFWanZkejU1a0RPUDlqQVFHc0ZsV05sQ01kNmFBN0hCT01kU0MreTIzTDZGeGJzcWZTKzc0NVVVcG16MGtiVGU3cU1IQytxeHZxNDg4c3Y3bnhEdTl2T05GL0g5dGt0VHhmQ3NRSUdkc1VqSExjZ3JSWmpRckpLUHFxZ1Zsb3JxQnJucHc3Rlkxb29XdE4rSGFONnJBamhXQWRhS2pKRVNGYXBkL1hZME0xaXI5Y3B4K205djVlREhnakZQVEcxQW0wUWpuV2dwU0pqaEdSMWVsVkREVnhrSzJtcDZQdTRuYXB4V2d0Q3NYZWFyMmVFWTBVSXh6cG9mcHh5cStBMVpHRWxKSDlodWtVeUpWU1BlN1YrVURWT2FpSG5od05Dc1hmYVI3b2RLWGdkeGFzSXgrbkpsNEVSYmdXUmtId3EweTBJeVduMG1sd2hzMlMxdm04K1dxR29Hc2ZYRE1XbkxMVHpUNDZwNWdXMTlCMHJRVGhPVC9PamxEdE8wT0hJZEF0Q2NoclQ0V1QwcHVmZnJQWEdzVyt2OFJzdTBsRVJpdU9pdFFKYkVZN1RvOSs0Y0lUa0pOelRtbW5QdjFqcjk2UHZOSTJwOHFkWnVTQVVwNkg1dXNiWVJDVUd5K1d5OUdPUTFIQXkrcWI0UXZRTC9XN3hTZVZ1S28vKzkwdjcvU05hU0REcEhFcUdrOUdqc3Zkby9uUngzN21mV2o1N2o0VGpvT1pWVmJsTk95NEp4R2tvL040Mi9mWjBjYzlhbjhTb0hDZWt2Tjk0VGpCT282NGtTOGo1SUJkVCtKZGo5Ymp2NjZGcUhJNzdIbjl3MzJ1M294M0JPQ2xhSy9BcXduRmF0RlRnVlc3aEZ5RTVxTDViU212YkVLVHo2Mms4c1lCZnpWQnNaUU9aM0dtdXpCS09GU0FjcDZYNVM4QmpIVVVJeWNHNGJWczdCK1NuaS9zSFJlL0hYRjVQVnlkVWpiMGlGQ3VsZkdNcnhya3BRRGhPUysyWGdGM3hkR3FFNU4rVmp5U3lwTy9ZTWkzQjU2em52OC80TmovdXBHK1VVS3liMW8xOG1IZXNBT0U0RVJteXIzVkJBS0ZMT1hmejhuUng3NTQ4L01iNzFWdXY2ckdpY056NWhsWitmeFovOWxPSDRtTVdWSmxBYXdVMkloeW5RMHNGZW5NWFlVS3lGNTJycHJKd2RaYjQ5VXJTVjlVQUFDQUFTVVJCVk05NkxxQ2xhdHdkb2RnbXplOFZsZVBFQ01mcGFQN3djNEkzWmlVa1g1VitQRHB3MWVNK0cxK2tyaDUzYnFtZ2F0eVorNTc5U2lpMlNmcnp0YzZVcDNLY0dPRTRIYTNoZU1HSjNpNEp5UzdzL0VKSWJxM1BwSWJVNGJqUEdnRW1WTFJ6SlRQZ3h6MFhRQ0k5cmRlNmZRODdlS0lId25FNlduZkM0V1NmQWZlSW5aRGMydHZoWk5TcFlpTXphMU10OExucE9qTlhmdDlEL3k4cFM4MVF6QXo0UE5CYWdiVUl4d2wwdlFCSFF0VTRJeXNoK1p5dHFiZnEwM3ViYXNKTG43K1hYdVBYMVZzOEU0cnp4S0k4ckVVNFRvTitZMFFsSWRrOVBqK1FpejBoZWIzTzFlTkU0WGpSOWUrVjMxUHJFNnpVNmxCOElMdFZFb296Uk44eE52ay9qa3dTbXVjYlp4bU9wWC9yeFhFdnNiZGFIcitmRGllak0razFaYnZnbjAyNzNDUzZZenVjak55ajkvY1JYbVB0dXNjMnhGU05mN2FReFkxRmIrKzg1Z2J4SWVQajRiN3I3eFM4amxXMFZTUkVPRTdqUU9uck1qc0tyQkYraitYNDFqOGJWK0VQSjZQbWYzV2p1TDVKei9Xai9HZTJGNFNWa0R5V1FNakVnbis5YzNQSU8xWUxyMk9INHk3L2ttd3lRTlg0aDduY0xQUzUyVENoY2E0OGtuT2srODgzMjNyUEcrZkx1WndqNjU5YjQrZktCNlhoMkcwRzhxYmttN1NVQnN2bHN0emZQcEhoWktUMW9IOXhqeEFWdkk2dDVBUi9JbUg0T0dDd204bko4emIzQzZlTTlEb2xKRCs3a2w3dDFvYVQwYmRJMVhnM1dhYlRpdmJoWkhRWk9jUnI5UnlLYzk3Skx1SzVjaTduU1ZQblNxbVMvNlhncGF6ekc5T2owaUFjUnlZVm03K1Z2cnpmTlc4YjNUakpqeE5Xdlc2a1dwZHRVQ1lrZi9kTGwrcHh4T0I1TG4za3JjanVuUDlFZUgyYVpSMktHK2ZLazRSVlVUUG5TZ3BXV0VWYlJYd3N4bXRKN3V5blNoNTl2Wk9mcjhQSktNdWdMSUhoVWpiRm1CYjgrUDFVYnNUYWloV091d2E3a2krMmR4S0tzNnZHS1FuRVRjMXpwZXZGdjFSODNHZEtSeHJTZDV3SWxlUElocE9SdXpCOVZ2alM1azhYOTZwNm9ZMVZNTE90S012TnlXbWhJYmxyOWZnMjhQRzZreDBSV3ltNGFweGxLRllZaUxkUldiR1h0UmNmRmJ5VVZXNWJlQUp5QWxTTzQ5TTZua1hONWg5R0grdi9WRkhPNVpHdEJJcmpRa1B5dUdPbDlUUndIMlBYNm05cHUrSGR5T1NKYkVLeHdVRGN0Qy9ueUZObElWbnI1bGRzMEpNSWxlUElocFBSbzlMUWw3eTNTY0xYWlVhOXJvdEdOVmx0TDNkYlVuMDhMV1JCMTBKbTNiWitHakNjaks0RGhSZTNJOTVKaDlmelJxWUxsREM2NzByQ1Z6YnppYVhOcWY3SjVUMTBsZVJ4NnBzWDVXdUJXSlNYQUpYaitMUUd2MlJmUGdsYlp3YXJJTnZzU1lCOFA1eU1zZ25LRWpqR2RmVW44NUM4SjlYV0xqZU80d0JoZE42eEQ3b3FaS1oxVnFFNDAwRGM1SzZIZjhuVHRtbXE5ODF0QnJJeTJsTVRyYU5mczBibE9DTGxJMlArbTZKWGRqZ1oxY0dqcEkwb0ZsSWh2NVFkbWt4clZKSnp2WUQzR1psMkpEZWVQbzZMKzl3Y2Qvbk1aRjQxcnI5UFp6bUVZdm5Nak9XbnRQT2l1N0U1Uy9HWFIxZ24wQlVUS3hJZ0hFY2t2YlJmRmI2MDZJdng1R0o5bVdHMXVLMjVWSlROQjJWNVQzUGRkZTlEMS81SVR3RzVjekN1ZEo5NytzaG1ON3RHSUQ1aGhPSnpuL2c0OW51cWVGRmVwellxOVBNZmpsOVVXaCtQUksyMnlJVkE2NjVFc2UzTENmbnY0V1RrSHUxTnBSSnJqcnVZU1lYRHZmNHZFbDV5MFhraHJRVGFZeGtYMVlYNzk0NTYzanpsZEhGZHlPZkw5WUtmV2czRzduc3UzL2RINlhmOVNEQis1cTRMRDNLZGlFbnJVd2ZhS2hLZ2Nod1JqMjJ5cldDRk1KUEsrclhWUjhWU1NjNWxhMm92VDFkYVRtTHhOdllxNHE1OUlabmY0bGx1Zk91TmpKaEVzRjNuSnpadGFXNTdmTHE0SHloNEdVVWhIRWZrS29OS1Q0aFJUa0NLSDF0cFozNkdjaWE3N25ucnk1ZUZWc2N5NVA5SWd1dENucWc4YjFmdWErR20zS1Q4ejhlZmxZanAzZXlNajE3VElFcnhSdm4zcE5POGRYUkhPSTVJOFJhVnY0YnVkNDI0cFc3dVRNOVFOaDZTVFk1VVVyNFErRFV6NlNlMi9Ga25FUHR4OVhSeDMzVkt5ODRVajFwbG5GdGtqSEtMUkhNZktjSFlsT2ZOUnFRS2IyNDBYR05yNnBKMzNjUHJ6TzVtVjhEb3RWVGNPTXdxUWtEV0dvNlBVbzViTFJIaE9CNnQ0WGdlOGc4bkdBZXpPa1BaMUdpNHduZmR3M29tUTdFc0hKc1NpSU9MRVpDMXJndnFORW9TM1JHTzQ5RzZQM3F3UGlhQ2NUUjcwc3Y5Y1RnWm1Sb050eEtTeDN4ZWluUWxuMWN6b1pqUmE4bUVEc2hhMTNSMG5wYURiZ2pIOFdpOTh3dHlRWkxOUFFnNjhlMDNncktaaVJjU2pHNDE3N3BudU9kUDYwMlNxZDNzbURTaGhndklENEUyQ3pHL0tSUDhZRUZlSk1QSjZGcnB3b3hQdms4eTBuZjNwODgvRTczTlpNTUVFeE12R3J2dWFRbkpzNmVMZTYxUGY3WlN0dERJVENodWpDTWtFT3Z6dSsvMUZuTGUrVWZqTDhzNHQ3Z0l4NUVvbm5Ic2RSV3M1KzF5RVlhWjBYQ0tkdDA3ZjdxNG55YjgrM3RSTUViUnpHNTJqRjR6bzlldWtadG9uU3BGT0k2TGNCeUo0aUg4M3VZbnlrWGxsZ3FMR1FzckV5OFVoR1RUYzBZVFZzUXNoZUk2RU5NT1pzZE1BckszejViaS9RZ1k1eFlSUGNmeHFLeWtlcjdnbnhLTVRWbWRlSEV0SVVaZDM1MWMvRTZsQWhwNzE3MHI2d1A0M2VzZlRrWlhFWVBmWEVMeHBlWlEzRmdJeXFRSm13N2x1dVB6cVk3SmpaYmdGNVhqQ0JUM01YblpFcmV5dmRFQWZtWmk0a1drRFVYY1RjT0IxWjBKbTZUNi9oZzRCS3Jmelk1SkUxbnlWbFdWUmNHZkZSNGs3K3VEc0JtVjR6aTB6amoyMlU1aGNoY3JyR1ZpNGtWalE1R1FJZG5ySTl1VTNPOGhyUU1oYm1KVjcyYlhtRFFSODRrRDRuSG5nYU5jdnFzYk1PczRvdjhVODV0aUhWOWhod3RPdnR4anl6L2Nrdy9YaStlQ3FOd01xZUVDbVR3QitVMDJrdkJoSWF2aHN4cnRKTlcxMytYMzgrRk9xblpIMm9LeCs1eTZrWkxTUS9xUGZJNDVUK1ZwMzJOcmhkYStYclc3N09hSXRvb0lGRCttK2ZKMGNYL2E1dy9RUFBvR1FhbWRlT0ZoMXowWCtLYTVCZU1tYVMyNDdMRkdRT1Z1ZGt5YUtGN3ZoYk9LV3dUdm5pN3UyUXdrRXRvcXl1YWpja3dQVkpuZXljK1p6UEJXTS9GaXphNTd1eTYyVXQwYTRKTUUvNk9XTFNscUYyMDJKazJ3c0s1c3AvS2Q3NE9OUUVEbE9BWUZNMFkzNmJXSWdVVjRXS0U1UEIzTEZxd0hLNDhuSCtWaWVKdHpwWGdicVNRZnl6YjNxOGZuVVk2UHRpcnhrVHhLSnhDanFmZmlQS1d6amhkUEYvZjBIVWRDNVRnT3N6dHJiZEdySlFQWmFZNkdtemVDY3ZLRmZQWDIxUGtkY2ova3hrRDl6UUdUSnJDRHFZZnYrbHpoNTRzYndJaW9IRWVnZFhlOFBqdnUwR3VNRnRST3ZJQitqVWtUYk9HTVhmWHFQYzd4bW8xMnFCekhrZU9qRUtyRzJGVTk4ZUtQNFdSMDF3aktETnZIV28yRmRXT2wyKzVETngrOXgrcTQ5akIyeVl1RFVXNXhhS3gyZEI3bEpCY3V0bGhGRnk3b2ZLMnE2bjl1SVo4c0NBT2V5YWhBMTQ3elAvbWNFSXpSeGZ1ZUl5ZFpsRmM0S3NmbDZ2UGxKOURBaCtlSkY3SmdWZFhFQzhURHBBa0VNdTR4VFltbldvVWpIS01Md2pGOFdyZVFUL1hXMWVpSGhYV0lvRTg0MW9xTlFDSWhIQWNtRndHTk90MFp5K0lZRnNVZ2xPYlcxWFBwVDc1a0laOTljdTRZeXcrQkdLRWR1czljeDNPSDFodHp3bkVraE9Qd3RDN0c2L3JsUC9IOE9vQk45bVZueWMvRHlXaldDTW84OGpTQ1NSTkk3S1JqOVpoelRPRUl4MmlMbGdxazBKeDRvWGJyYXJDRk0xVEpzYlVDRVRDdEFqdVRpeDdWSDZUMlRpWVpQQTRubzB0WjBJWEUzUHZnM28vR3BBbUNNVkk3N0RtMVFodDJ5SXVFeW5GNFdqL01YU3B1eHdGZUI5QlZjeUhmUXRvdVZPeklWd3BwbTVoS2hZNUpFOURvV0o0MDdjek5FaDVPUmhwL2wxeDMyMVdIeW5GNFdqL01YWHFPQ2NmUWFrOFc4djB6bkl4T002c1dxZU9Pcnp2T3Nrdm1SNEl4RkNOUW9qWENNZHJnSkFNTDNDSytXOFdUWWt5VDQvb2d4eG5RanFJT1dpTWNvdzEycTRJVmh3UmsvNlMvKzVaUmJEQ0U2eFphSXh4ako5SmJDRml5UjBEMlI0N2puN1JRd0JxdVgyaUxjSXhkY1hLQlJYVkFwZ2U1QndrWHQyWi9BWlN1eS9WclZ2cEJLeG5odUZ4dFYvUlRmWU5WZXpMSkF0MmRVVEdHWVYydVh4cG5xSE1kam9SUmJ1VWF0eHhWTXkzOWdNRzBkOFBKNk5pTmFPSnRiTWNkTjJZV3c3Z3VFMncwUGkzbEJqVVN3bkY0V3RzUldHbU8wb3hwRGVpRVhURmgzUjdYUExSQlcwVjQ5T29DT3JDVFhqZnZMYjVvQU9pS2NBeWdGSHZTSW9BZGNid0FsSWh3REtBa1BNbHBod1ZBQUlwRE9BWlFFc0p4TzR6QUExQWN3akVBQUFBZ0NNY0FTdEoydm5mcE9GNEFpa000QmxBU3dsNDdIQzhBeFNFY0F5akZnazFBMnBIanRiRDBtZ0dnTDhJeGdGSmM4MDUzd25FRFVCUjJ5QXZQUFpaOHEvQjFmV241LzNjYktCd0dlaTFBREdjYzVVNHUyUWdFeHMwNjNPUzVuU0gzZWVQTFJEZ09UMlhQM3RQRi9XbWIvLzl3TXZwV1ZkVWY0VjRSRU5UVjA4WDlBNGU0UGRkYU1aeU1icXFxZW1mdHRRUGk4dW5pdnRYTnNXeUFRemd1RkcwVmhScE9SbTJIK3hNc1lKWHJtWjN5N3ZVeXB2Y1lodVZ5L2VJN0dBbmh1Rnh0aC91emFoMFd1WXZKOGRQRi9UZmV2ZTdrK0IxemNZWlJYYTVmR2pmQW9VZ1ZDZUVZTzNtNnVDY2N3NW82R0hOQjhVQ09Jd0VaMWl3NlhyOVlZMU13d2pIYXVPTm93UWkzQU9lSVlPeFhJeURQY3ZxOWtEWE9BV2lOY0l3Mk9NbEFPMWZWL1BKMGNYL0UwNDR3WEVCMngxY20zbEJGaG5iTU5rZHJoR08wd1VrR1dzMGxyQjIwbmNTQ2J1UTRIOGh4bjNNWW9SUkZIYlRHS0xmd3RINHhqenFFWGNJeE5Gbkk3Tkl6MmlmU2tJVjZMaVNmeWdTY3FjeEUzeXZ4ZUVBbHJsdG9qWEFjbnRaVjhxMVg0cm9MNFhBeW1yRlFBWWxkdVZEOGRISFB6bTJLeUEyS0cvbm1Sa1dlU0VobTh4Q2tkTmRsVW8zTU9OYUlJa0FraEdPMGRjbG1JRWpnUnFyRTE0eGwwMDl1WEs2SGsxRmRTVDVoRXhFa2tOc05OT2UrU0FqSGFPdWFjSXhJWm5JemRra2d0a25ldCtmM2NEZ1pIVWhJSHZQMENaSHdkQW1kRUk3RDAzcFJiN3REM2pNM0FZRFdDZ1EwYndSaXBrMWtSTjVQdDRYdm1RVGxzZnl3UlM5Q21QVTRoMmpjQUFRUkVZNERjMzE0dzhsSTQwdnI4K1YzRjdpdkhsOEx5amFYQ3M4bEMrdktJS0dsdVpDdkRzb3M1SU12WnozK25FN0Zvd2dvR0VSQ09FWVgxM0xpNFVLR3JoYU5IbUllZlJaTWJvaGNiL0swc1pDUGlSZm9ZNUZwU3dYaE9CTENjYms2M3huTDFJcHJWcUtqZ3hzSnhKY2NQS3lxRi9KVi8wNE1HTE9RRHgzMVhiaExXMFhoQ01keHpCWDIxZld0eXB3U2pyR2pPK2tqWnRJRWRpWTNVRzRoMzV2R1FyNjNIRUhzb085R1FGcmJLaEFKNFRpT3g5d1duY2pDdkNzQ01qYVlOUUl4andMUkdSTXYwTkpWcnVlY3A0dDdOalNKaEhCY01EZm92T2VYamVveG11YU5IZXZVWEp5azhuZ3MxYURWNGY3ZlpMRCtkY21MQVdWUjNJa2NvOVZIeXJkeWpHNVRWLzVYSmw3VUMvbE9tSGlCQmgvYnh4OXdRTXMyV0M2WHBSK0Q0SWFUMGFYU0VQbGIzenRSeGI4YjRsQzdoYlBzY2pWdDBiUHF3djFwU2YzUTB0ZDcyaUpjM3NoN3JhcUN4ZGJWRUs1cVBPNTdNSWFUa2NwZzlIUnhQMUR3TW9wQTVUZ09yWTk0Zk53ZFV6MHVqK3BKRXpMeFlOcWhQOVVGeEsreXE5czQ1MHF5aE1uckRoVlhkNlB4YmpnWjNjbU5oSXFRdkdIcmFvSnlXUlkrcXNieXBFbWp1OUxmNEpqK1U4NnZpalY2aDJONXpQbUZnMXNFVnpYODRENDNyanFqTFJpN0t1aHdNbktmeHo5N0x0eHl2YXkzVWxYTmp2eGVmL2RzUlhESDl5OTN2TFVkSi9lNWxPcmhnWHhlYnhTOExJVG5xNTJMeFhpZ3JTSUdlYno3bDhLWGR2NTBjVC8xOFFkSktLSHZMejh6NmZGVU8ybWlRMnRBRzcvbk5JZFpxcXAvQnZpalZiZWtOQ1plVEZuSWw2WDUwOFc5bHo1aHhkZHJMeTBqMkExdEZXWHplWWM4Vm5wQ1FYdnFKMDFJMkpuS1Q4aEg1ODhURW5JWVFTZkhMRlI0clZ0U1R1WHZPTk4wekpoNGtUMmZvWEYxMGE0V1RQMkppSEFjaDlZUHRiZmVLdGQ3T0p5TXpxdXErdWpyejBSVUpyWndqaGlLYTN0U09jK2hZaE5qVjBzWGtqL0xibmRuMmtKeXhjU0xISDFoeEJsOG82MGlrbEpXdnc0bm93ZXFNV2FvblRTeFNxcDlVd2t5S1JaWi9XSjVkcW9jdjM4Uy9OVUxDYUtYMm84ZkV5OU1tajFkM0h2dEVSNU9ScmRLTjV2cFBWMEt1Nk55SE05QzR3bFhIaG43dkdpZHlFeFVMaTQ2cVo0MHNVcENuWWFKS0hXMTJxcFVyMzFQS3NtZlpkT2dVNjBobVlrWDVpemsvUUc4WTFwRlBGb3JjMTZIbmN1Rmp4T1dQcW9uVGF4eW9WaG1hUCtqWkZTZzFqN0VYV240VHJyMzhSLzN2c3BOajFwTXZERGhKTkNObHRZdHlvdmRwQ2dGMmlvaUdVNUcxeTAySTRqcDA5UEYvWm52djA4bUNIeFYrUHVXUlAya2lWV3lVdnhVNHdYSzZnQis2ZFArbjRLWHNrcmxoaUtiTVBGQ2xROGhKcU1vL3E2d0FVaGt0RlhFODZBMEhBY1plTzVPWE5MRHh3Szl1TlJQbWxoSGN5aXVlZGh1UFJXdGMxdFZiaWl5Q1JNdjFEZ1BPREpRNjNkbHB1QTFGSVZ3SEkvV3lsMnd4OFZ1aHJMY2liT0RYbGp6eHNJNlU0dkdldXhtaDN6VUc0cmN5V2RZZlM4OEV5K1N1ZkkxbTM4RHJlSFkvQ2hKYXdqSDhXanRGd3E2VmFicjJ4dE9SaFVCMmJ0RlhjV3l1TTF4NEkwN1lKTUx5VytIazVIcURVVld5ZmR2S3VQcjZxQ2NhcXBLem1Kc2dxRjE2Mmo2alNNakhNZWo5YzR2K0NOQkFySTNwaVpOckVNb3hnNmFHNHFZQ2NuVnowR1ppUmYreE5vZFR1dkNXeXJIa2JFZ0x5S3RzNDZycXZvMVJ2VnhPQm01aThZZm9mK2VETjFJaGRocUlIN1RtRkZzTmhUN1doRFRXTmgxTEk5eG16ZW9NNmtTM2ZvS2hab1hHZTFvM3BpVmJESWtTRkFlSzExM29sMlF4WGZyRENlalI2WG5LR1ljUjBZNGpranhGKy8zV01GTEtvY3hkdXF5N3FaUkpiWWFDR0x2WmhmUy9Pbml2dGY0c1E3SFl5RTl1S2Q5ZnkvRjU1NDI2ZzFGMU8yNnQ2dkdqZEVKUVhrcjkzNUhIVHRaZWdFTFB4Q09JMUs4ODg0WEh4ZmdYVWxmM2lXcnZIOHlhL1FSbTMyTUppdjV4NW1FNGxxdng3cnltYi91R0ZCbkVoSTZYeHhsWm5RdWJVM21RM0wxSXlpUG1YaXhWdS9QZkZzeU1lZXZXSDlmRzR4eGk0OU5RT0xTK2xna2FwK1ZuUERjMzNrVjgrOVZ5bDBFUHNuMnhFZHU1clRocWxoejQ0N1BtVDBkNkZ5OWtrZnF0ejBxdHk0NDNVckE3aXFuUjdMMXJudi9zN0NoeUNidWV5N2ZkL2UrL2lMbkFVWjIvWHRkT0U1UUtkVTZxV0t1NERVVWg4cHhSSXA3Ym5zL011NUtnc05sWVcwV1prZXZyYU5vaStkUUZrOFg5NTFXc1V1Zy9kdlQ2MXIwQ1EzRHllaGJ4dDh6MVZ0VHR5SGZwMm1CbytHaXQxRTBEU2VqTTZWeitlK2VMdTZ0NzlCcERwWGp1TFQyRE8zTEk3N281RVI0VUVBVjJRWGljK2tkYzFzNFQ2MWZ5TjFqU0dWYlBJZlNhUWRKK1U3NXJOanVTUVc1NjNmViswNllpalMzcHRaYUFkeUpPeS9JK2NHZEYzK1Y4MGJ1MWNNcjJkbys1YUpqclo4YkZ1SWx3Q2kzdURRMzFCK2wraEpLRzhGWWdwYnFYZEphV2pRcXhOa3NwckN3bTUxSGl4Nmg4anBBcFhaUC90d3VsYVN6elByQTEzRWgrYjJWWGZlMldSa05kOXpZYkNTWDkxRFQrNlQxZkdiK2FZaEZ0RlZFcHZqUlp0UkZlYTh4UGd2WC9DemlUUW9MeGJWTzN3dHBGL296Mkt2cU9OcEpaZ2QvRHZPU1ZNb2lKSy9LWUlheXFvMWVQTGMvK2NZWXR3U29ITWYzb0RSY3FIbWtKQ2ZNU3duSll3TmhMTnRBWFAyNFdaa1d1S0srVDlVNGRBdkRXY2Z2N0ZsaDRiaTVOZldscFExRlhpUG5tZWR6amJHZ3JQVjlVTnVLUXpCT2c4cHhaSXFiL3BNdHl0dEc3dXFueWs3K1dRZmlpdDNzbkhQWCs5bjJYNG80RXFwcjlUaW5zVzV0bWRxYXVxMUdVTmIwL3FwdkwrTzZqRlZVanVOVHZTaFA0eGd4T2FHT0ZRelF6ejRRVjRUaXBxN1YzeGpiM05aL1Q1ZXFVczZUUmJZeHV6WDFMdXFLc2t4R09rNWNVYmEwa1pIV3lqRWJmeVJDNVRneXpZUEdZKzZVNTROVVNZN2xKOVFqLzd2R2RyNDVCK0tjZHJQem9mT21IeEhYRmZRWk1WZHk5YmhwTHFNa1RXOG9zazNqWEhrVXNFMXRKamRyNXM2VmluZkdVN01XcURTRTR3VDRJb1loTng3dTVQK21zWnAvbHd2QlhGWUVmNU1nN1A3NW9ZVHRPZ25GRy8zU1pkUmVoSVY0cXpyZDBNb3MzWCtDdnpvN3N0aDFiMWR5cmp5UW4vcWNlYkRqMDZJNytjOWJPVmMrV3U2THBXQ0ZkV2lyU0dPbWRIR1Q2VUhqY29KbThjSU9NdDNpMlplckhqT29ZN1ZVMUU2NjdON25mci9oWkhSRjlmaTdldGU5cWZTZlpoMlNXZVQxZ3VhNTJMUlZKTUltSUdsby9jQ1hOS0tyU0psdjhleExuMDAvWXZmQ3YrK3hLVWlXaTlKNnltSnJhclNpdFNpMHlHSEhSNnNJeDJtb3ZSdTB2cnNVMWxzSnhWUUxON3ZyMFU1ekV1TUYrdnA3cFhwNHQ4UC90VlROWGZjSXlmblNHbzZwR2lkRU9FNUQ4NGVlUGR3elFpaHVyVS9QZmV5V2lscWZVTTVpbiswSXlabVM5MVByMHpOYVh4SWlIQ2Vndk4rTGNKd0J0OGhrT0JuZEVvcGJ1ZXY2M1pTTGJLcTJwSGRkV3l1b0hyZFNoK1JiV2NRRit6Uy9qMVNPRXlJY3B6TlQrcm80NlJ2V0NNVi8wVVBlV3A4cWFxcVdpbHFmcWpYVjQzYnFYZmNJeWZZUmpyRVc0VGdkcmRYalBmcU83U0VVOXpicitVUW5WVXRGNzc5ZmZ1KzUzNWRUQkVLeWZWcmZ0em1MOGRJaUhLZEQzekY2Yzd2WkRTZWpSMEp4YjExM3c2dGJLbEtQWmp6czJROUw5Ymk3T2lRL3lPNlNNRUNLUUZwM0FhVnFuQmpoT0IzNmp0RlpJeFIvWlp2bjN1WTl0eEdlS3ZrOStsU1BMNmtlOTNZb1cxTS9FcEpOMEh5ZFl6RmVZb1RqUk9TUnlVTHB5NHM5cXhVN0loUUgwYmRxbXJyZnVOWTNrRkU5OW1PZmtHd0MvY2JZaU8yakV4cE9SdGVLZytodjdLS2tnMHdpcUhlekl4RDc1YXJHbmRzUjVOSHMzNHArbjEvN2JIc3VOMTU4eHZ5YXk0M0hkUWxiVTFzeG5JeSthUjNqOW5SeFAxRHdNb3BHNVRndHplRlRTeldzV0M0VUR5Y2pkMUYxZ2VVUFFrc1FmYXVsV2xvcWFuMHJsWjE3cjdIUnZqenBjWlhrMHg0N0dzSVRXVUNwZGI0eG94VVZJQnluUmQ4eGZySVNpdG5pT1J6WDFuVGQ4MC9YZGhQWjkvVmNLbTczc3E3ZW1wcVFuSjdtNGc5UGJCVWdIQ2ZVNS9GbkJIMVh2Nk1sUW5GMFozMGVjdzhub3hPRjc5Ryt2SzVPNUhoUVBRNkxrSndlaS9Id0tzSnhlcG9mb1ZBOWpxQ3h4VE9oT0o2Rmh4Q290ZnJVOTNXZFVUMk9vaG1TenloR3hLRms5T0pHclBYUmdYQ2NIbjNIaFdxRTRucUxaMEp4UEgycnhtOXlEY2RVajZOejMvdVBzalgxSlNFNU9NMUZIL3FObFNBY3A5ZTM1ekdrZHp6eTgyOU5LRVpjdnFyR1dtOW05anlNRUNNY3AvR2VrQndjL2NiWWluQ2NtUFFkYTM2RVNXdUZKNFJpTlM0OWpOVFMvbFRGUi9YNHl0L0xRVXVFNUFDazJLTjVqai9oV0FuQ3NRNjBWbVNNVUt4T3I2cW9nUXRzNWVtcEQ1dUNwRWRJOWt0MXNZZCtZejBJeHpvUWpqTkVLRmJwU25hbjdNUEtybWQ5cThlUFZJL1ZJQ1Q3b2ZsNmRxUGdOVUFRam5YUTNIZTgxMmMwVklrSXhhcjVxSVphQ2NjK1hpZlZZMTBJeWYzUWI0eWRFSTRWa0FyTlhQRkxKQnp2Z0ZDc1h1K3FzZll4VUN2ZTlnMVFWSS9WSWlTM3BIUXVlUlBoV0JIQ3NSNmFxOGVFNHkxazg0NEhRckZxUHFxZzFyNExQbDd2cFljL0EyRzQ4ODJEbkgvd09zM2YzYm55VGNHS1F6aldRL05kSTYwVkd3d25vNlBoWlBUQTVoM3EzWG5vTlhhbXhuN3YzcTBWc2tpSSthdDZQVzhtNHM1RDdueFUrc0Y0QlMwVjJCbmhXSW1uaTN2TmxlT0s2dkhQaHBPUkMwcC9HM3JNWHJMZWxUVUpIdnZHanVHaHA4QkVaVkkvZHg3Nlc4NUxhRERRVXFIOStsOGN3ckV1bWxlckVvNkZHNUUxbkl6Y3lld1BGUzhJMjl4NUdwRmtaU0hlS3FySFpmbkRuWi9Zd09rRjdkOWRLc2ZLRUk1MW9iVkNPYW5DM1JxWWM0c2ZmRlU5clg3K2ZiMXVxc2QydVBQVExXMFdKdWFTMzNqWWxBaWVFWTUxMGY1b3hXcmx6SXZoWkhRc3daZzJDanU4VkkzbHZiZldVbEhiOXhHUzVEak92TDBxaEhZb0FibjBYVTYxMzlUU1VxRVE0VmdSV1RDaytlTGpZOWN0azRhVGtic3grSXRGZCtiMDJnMnZ3ZnFOb2E4K1ZGL0hFM0c0ODlWZmN2NHFGUzBWYUkxd3JBL1ZZMlhrd3ZLMXROODdBM09QQzEydHR4UjVlZjFQRi9lWHltZXlZNzJ2SlFaa21RSDlWc0ZMMldUbWFZb09QQ01jNjBNNFZvUmdiSnFYSGxrREs5MTM0WFBOQUwzSE5wVVlrTFgvdnN3UVY0cHdySXdNQXRkY21mRTFHa285Z3JGcGM2bHkrcEJMb0tCNmpOSUNzdmJmbFg1anBRakhPbEU5VGt3V3NSQ003ZkpWTmRhKzByMk5FNDlyQnFnZTIvVzFoQUtIUENuUnZJaVdsZ3JGQ01jNmFYL1VrblU0bGo0MTd1anQ4bGsxem1sODRaN242dkhDeDUrRkpHN2xQSmN6N2Q5ZEZ1SXBSamhXeUVCcnhWNnVqK2Frc25iTlZBclRmTjVjNWpiYjIrZnZ3K1FLdTl6NUxkdU5RdVQzZXEvZ3BieUdmbVBGQ01kNjBWcVJ4aGx6akUxYitBcHRVbG5MYmJNWG4rTVl6NmdlbTNhWWNYdU05dXZUVElwZ1VJcHdySmYydThxM3VUMldreDQxN2RVR3ZPN000MjVUdWU0STZhdTE0aHZWWS9NK1pycnpxYSs1M3FGUU5WYU9jS3lVZ2RhS0txZXFnMVRUT0dIWjVxMXFMSEo5T3VJek9GQTl0dTh5cC9ZS0F3dnhLdGEwNkVjNDFrMTdWY2JuNnZmVUx1a3pOczliMVZpZWl1VGFYblBvNjZtUEhHOXVLbTNieSt3OXROQlN3WlFLNVFqSHVtbS91OXpMb2JvbVk5dHk2eTB0RVZYajNiRXdEMDN2NUR4b21wRjFBdHhNR2tBNFZrenVMbWZLWDZiMjNxNWRjTEt5NzhwanIzRlZRRGoyOXZ2SmVlcksxNStIWkhJNEQxcTRIbkc5TVlCd3JKLzJxc3krNVFVZE1wSk9lMzhhdHZQVy95NGJKT1QrbWZDOTB5V2JndGkzYjNsRXA3VDRhWC85TjU1djRoRUk0VmcvQzQzN0pxdkhjakxsa2JCOVY1NTcrRXJaWHBmcU1WWlp2c2taRzFnM3drSThJd2pIeXNsZHB2YUx6bHVqMjVGT1dZU1hCZDhYOUZ4SHVLM3kvWHRTUGJiUGN2VlllNUZtNFhIblRnUkdPTGFCNm5FWXBWUUljK2ExYW14a0RKUXYrejV2YXFrZVo4UGNUWTZSOWppcXhvWVFqZzE0dXJpL05qRHorTDJsVFVIb05jNEdWZU4rZk4vVVVobXp6MkwxMkVKeGhoWStRd2pIZGxpNDZGaXFPRkExdHU4dXdMelEwc0t4MTkvMzZlTCsxcjB2UHY5TUpHSG0vQ2dqNkxUUEpHZTdhR01JeDNaWUNNY21OZ1dSQ3ZkYkJTOEYvWGk5R1pPV2l0SjYwUGNDVEp1aDk5aSt0NGFlQkZyNHZGRTFOb1p3YklSVXlHNlV2OW85STQrM2Nwak5YTG83cVZMNlZPclRCS3JIV0VmOWVWS3F4dG9MSFF2NmplMGhITnRpb1hvOE5WQTlMdTNSZVk1OFY0M2ZGTHhMWW9nblBsU1A3Yk53bnJUd09idG10ckU5aEdORGpDek1VMTA5TG1TRGg5eUZxQnFYZk1PMEY2aDZySDEzVDd6TzZ6UVQzNHhValN0YUttd2lITnREOWJnZnFzYjJoYmpZbFA2NUNQSDdFd3JzTzFiOEcxaW9Hck1RenlqQ3NUMFd3ckhtNm5IcEljaTZ1VHhCOFVZV0hwWGFVbEY3NS91R1ZqWTgwUDZrQzY5VGViNmthb3pRQ01mR0dCcTByNjU2TEs5SCs4Z2Z2QzVFdFlnYnBuK0ZPQTcwSHR1bU5ZQmErRnl4STU1aGhHT2JxQjUzWTNHTGEvd3dEM1N4WWViMXY3eC9YNmtlMnlkVldqV29HaU1Hd3JGQkRUWFphUUFBSUFCSlJFRlVoaGE3YUtzZWErNmZ3M2JlcTBYU1VzSFRoSDhkQnBwdFMvWFlObTNuVFN1Zko2ckdoaEdPN2JKd1Y3cW43RVJHNWRpdVVGVmpXaXBlOG40OHFCNmJwK2E4YWFocWZCVmc5MDVFUkRnMlNpNDRDd092L3FPaW5aYXM3UGlFbjRXNnlXSkRtSmRDdFpoUVJiTkwwM21UcWpHaUlCemJacVduU2NzSmpjZm5OZ1ZaMk1MTTY3VU9BODIyUFROeU00K2ZxVGh2RGllanNaR3FjWWc1N0lpTWNHeWJsUXZPKzlURDVCVlZyOUZlcUp0QUZ1S3Q1LzI0eUE1aExGQXlTc25hRWFyR2lJWndiSmhjY0t6czJaNzZ3a2c0dG1rUjhMTkR2L0Y2b1k0TDFXTzdVaGMzVG8wODVRbTFOZ0tSRVk3dHMzSTMvVmJiU0NDWWNDWTNnVjdKWjVHV2l2V0NiQnRNOVJoZFNOWGF5dG9BSnJOa2duQnNuS0ZOUWFyRWo1c0k1dmFFckJyVFV2RzZVTWVINnJGTktaKzhuY3JrSSszWTlDTWpoT004V0xsYmRSVXBwZ05nVjBHcXhvS1dpdGNGQ2NmeWZoSWc3RWtTam1XdHlFY2pSNHVuSWhraEhHZEFxc2QzUm42VFUyM2JTa090SUJlYjRXUjBZcVFTbGRLZUhLY1FDQkhZbFpVYnFaQlB1WkFBNFRnZlZxckhlNXhFc0lNcnFzYkpCVGxPeGxyQmtJamNuRmtZM1ZZRmZzcUZCQWpIbVpDNWlsYXF4KzlabkljdGd0enN5Vk1Md3ZGdVFoNG5GaTVoSS9tZVdpbWlVRFhPRU9FNEw1WXVPSnhNc0VuSXJWZHBxZGpkbm15ODRCM1ZZMnd4TlRSTmhxcHhoZ2pIR1RGV1BUNWtjUjQyQ0htVFI5VzRIYXJIaUVvVzRYMDJjdFNwR21lS2NKd2ZTeGVjbUl2ekhpTDlQZWduV05WWVBtdnZlSDlhZVJmcU8wcjEySlNZMnlGYm1tWkMxVGhUaE9QTUdLc2U3MFU4RVhJQ3M0R3FzVDVVanhHRnNVVjRWSTB6UmpqT2s2VUx6anNXNTBHRTdEV3UyUGlqczJEaDJOZ1l5cElGTHk3SUV3cXF4bENCY0p3aFk5Vmo1ekowZTRVY0UrZ1dyQW9qZll4V0tsTGF2SlBqRndyVlkrV2VMdTVqdEtWWjJRbXZvbXFjUDhKeHZpd3RkdHVQZElGazIxcTk3Z0pmZ0dtcDZDZGs5ZGphelh4cGdwODM1ZW1obFozd0txckcrU01jWjBxQ2hxWEZMaCtIazlGUjRMK0RSWGw2aGI0NW9xV2luOURIaitxeFhrSFBtd2JiS2FnYUY0QnduRGRyRjV6UUowakNzVTUzSWR0ZXBDWGcwUGd4U3Uwd1pHc0YxV1BWUXA4M1R3M05OSGFtVkkzelJ6ak9tQ3gyT1RmMEc3b0xjTWhBVHpqV2lhcXhEYUZiVTZnZTZ4VHN2R213bldMK2RIRnZxY3FOamdqSCtUczExbXY3T1dCN0JZdnk5QWxhTlJhRVl6K0NIa2VxeDJvRitYNGFiS2VvakszbFFRK0U0OHpKNHg5ci9WRkJwbGRJSloxRmVib0VyUmJLalphbFI3YWFIVVpZRjBEMVdKZDV3UEdLMXRvcDNJMzh0WUxYZ1FnSXgyVnc0WGh1NkRjOURIaVI1T1NtQjFWamUySlVqMmVaSGp1TFFsV05yYlZUVk55NGxZVndYQUNwSGx2N1luOE10RGtJclJWNnhQaE1Nc0xOcnhqSGswa0Flbmd2SnNoVFFXdEZpaXRtNVplRmNGd0lXVVJnclovdk9rQjdCWlZqSFdhaEx6YTBWQVN4SDdxMVFzNVZscDUwNVN6RWQvVFMwR1lmTmFyR2hTRWNsOFhhRjN6UDk0SU5xYUxmK1B3ejBVbU02aUNMWjhLSTBhcENHRW52eHZmSXN1Rms1RDQ3NzR3ZGh5K0J0N1dIUW9UamdraWx6dExHSUpWc1hlczc1RkE5VGl2V09DUmFLc0lJZmx5cEhxdmc5VHdwYzdLdHRjek1hZk1wRStHNFBOWkd1em1uUGgvbHlvV1hxUlhwQks4S0RpZWpFNE9QYnEzWWwrTWJHdFhqZEJZQmJtQ3ZMYlpUc09GSG1RakhoWkhIUTlidWhQY0NqSGRqa0hzYVZJM3pRUFU0YjE2L284UEo2TXpnTHBWM2JQaFJMc0p4Z1o0dTdrOE5YblFPUFlkNkhwV2xFYXNhU0RnT0s5YnhwWHFjaHJmem96eGxzRGEycldMTlF0a0l4K1d5T1AvMXZTem82RTBxNk5iNnI2MkxValdtcFNLS3ZSaXRGVlNQazdqeXRRQk4rb3d0VmwvUG55N3VnMjJiRGYwSXg0V1N4WGtXcHphY2Vldy81cEZaWEZTTjgwTDFPRTgrajdmRlB1TTVuemtRanNzMk5yZ3d6VnYvc2VFYkJJdGlWWTNkNStKOWxrZFFueWpobU9weFZENnJ4cGNHKzR5ZEtZdndRRGd1bU5HZDh5bzU0Zm9LV3ZTVnhVSFZPRDlSV2lzRWxidzR2QnhuYVgremVKUHFGdUV4NmhPRTQ5STlYZHlmR2R3NXIvSTEvMWlxSk9kK1hoSTJpTG5xbTNBY1Y4enE4VXpSNzUyamN4OVZZMmw3czdqZ2VXRjBMUTRDSUJ5ak1sdzkvV000R1IxNytITXN6bjYySkVyVlQxb3FyTzIrWmQzN0FGdThiOEpUbm5BV1ByNm44bG13MkdkY3lVeGpkc0xETThJeEtsbVYrOFhva2JqdXUwQlAya3VvR0lSeExyM2RNVkExVGlOVzlmaVdwenpCakQzMTJicjNhRi9wNy9pYW1UeEZCWjRSanZGTVpoOWJmR3pwWllHZTlKbXhPTSt2Mkt1K0NjZHB4RHp1Rm1lMGEzZmpvOC9XOEFLOGl1SUlWaEdPMFdUMXNhV3ZCWG9XcDNkbzVxc2F0UlV0RlVtOWk5VmF3Vk1lNytZK2pxZmhCWGpPRjJZYVl4WGhHTjhaZjJ6NVRyWW83VXd1dkZRZi9mZ1NzWjJpNG4xTEx0cnhsOCtWMVRZd2JVNzYzc0RLdW8rdlJuLy9tVHcxQlY0Z0hHT1Y1Y2VXSC92dW9FZGZveGQzQ1M0NGhPTzBvaDUvK1h4Wm5MS2pTZTlkNEdTOWgrWFJaenlGd0ZxRVk3eVF3V1BMcjMwbVdNampZUjhUTUVvMWp4MlVhS2xRSVZwclJjTUovY2U5SFBkNXo0eFBwcWhvcDhCckNNZjRTUWFQTFR0TnNKQ1QvYTNoUlNXcExYdzhwdTJBcXJFT3NhdkhkUnNVNndTNmNlZTUyeTRCdVhHdXREaVpvcUtkQXRzUWpyR1c0ZWtWbFZReVdwMzBDY1plbkNTcXhCQ09kWWorUHNqbmpVZmozWFVOeUdmR3o1VjhadkFxd2pGZVkva0Vzbk5BSmhoNzhTSHlBcndtMm1CMFNQSSt5Qml5RHdxUGh4V3RBcktNYkxNNm1jTDVSRHNGdGlFY1l5TTVnWHd5ZklRT0pmUnVSREQyNGtQRTdhRmZrUDV5cXoyUHVkbnJ1eUZQVi9MNUl5QjN0MU5BbGkzN0xRZmpPemI3d0M0SXgzaVZuRWdzcndvL2xFckhUd2pHWGlRTHhpSkpHTU5HeWFyNEJPVGVYZzNJTWdub0QxdS8wZ3NMMmltd0s4SXhkbUY5YzR6M3F3R1pZTnliK3p6OG1qZ1lWNFJqZFpLK0gvSjUvSTFGZXAydERjZ1NqSzNPTXE2NVRZa2VkYndVYUVjNHhsWnlRckYreC8wOUlCT01lM01MTlkrVjlPMFJqblU1U1AxcXBQZjkyUENDNHRSZUJHUnBsYkhlaW5EbFk0dHNsSU53akozSWljWDY1aGp2cFdldVR6QjJGOXoveXVQYkVxdFRONHFDY2NVTmpqcHZOYndnK1h3ZXkrZTFOTzY4OUx1Y3A3cmVJTlFCK1ZqT2w1YjcrdDB4bUNwNEhUQmtzRnd1ZWIrd0V5cXUzeXVtejNOOHBhSnlXY2p4Y0JmY1UwMkxXWWFUa2F0Uy9xUGdwZUNsWHpROXZwWWI0dE5DRm03T1pLVGk4L0hublAzc1Y2WlRvQzBxeDloWlkvZThFaXVtTDRKeEpkV3BwNHY3SStNYnB1ekNMY2c4VXJqS08va2pmS3lsNm4yUnoyMEpiUlp1eDdlajVvMkpuSzlLYmpGaGJCczZJUnlqRlRuUmxQYUk2cWRnM0NRYnB2eHFmS3JIT2d1NXVCd3JYY2hDT05ZcDlqYlNXelZ1WkQ5bGVITi9KOVg2dFR1K0ZSeVFieGpiaHE0SXgyaE5Wb1JmRlhMa1hnM0dOYm40SGtzdjhqenFLd3pEdmI4SHlpOHVoR09kMUM2U2xNL3pRU2JuTDNlZStYMlhtOWNDQS9LY3NXM29nM0NNcnFZRm5HaDNDc1pON3NiaDZlTCt3SEJJdnBJcTFMak43dzFZNFQ3WDd2UHRQdWRHUS9KYzVvc2Z0Sm5BVUZCQVhramZOZWN2ZEVZNFJpZHk0am5KdVArNGRUQnVNaGFTRnl1aG1GbWd5Sjc3bksrRVpPM25zbGtqRkhlYUwxNUlRSjdTWjR5K21GYUJYb2FUa1F2SWYyWjJGSHNGNDNWa0pOSlliaWkwckpxZnlmelNhNHRWbHVGa2RLdGxkQmhldUpMUWFZcE1kaGpMajVicERpNnd1K3J3cGN4djlpTGpLUlltUDN2UWgzQ00zb2FUa1ZzSThqbVRJem1YeVF4QndxSmNsRTdrNXpoQlVKN0orTGxyNnhWaXdyRmFkOUovYjVhTUNheHZabU1IeUlVRTErdVFONjRaQnVTWkxMb0VldnMvRGlINmNxdWtwVEthUTFEWmx3dGlrRzJSNVVKM1dmLzVjdHpxbjZNQVlkbUY0UWU1Q043U01vRUl6SC9HNUh2aWJ2cFBKU2czdjZPK3crUmk1VHZxclVLOHhVRkdpMW9YOHY0QVhsQTVoaGNaVmlIT255N3VvNCtza3d0eGZURiswMWo1LzJiRHNhMHZySldFa3U4L0VTK3lTUXduSTljUzhqSG4zOUdvTDV2R2l1VkNibXJyNytwUlkzemRwaHRjZDVOYVY0QWY1Sjl2NVhzYS9XWkNOakN5dnZOZEV4dDl3Q3ZDTWJ6SjhJUkwvNXBpbWJYejVDVDdjR3paY0RKeTU3U3ZHZjFLSDdvdVVBUTJZVm9GdkpFNzk1ekM1UHZoWk1SSkYwQVdNZ3pHVndSamhFQTRobGN5ZC9OVFJrZlZCZVJiYVJ1Qkxzd3gxWW0rZG9XR2s5RTBzMkI4eDVNOWhFSTRobmV5QzFWT08raTVoWVlFWkgzb01kU0pjS3lNUEFIN0k2TmZhU1lMcDRFZ0NNY0lRdTdvY3hvMDd4YkRQVWhmTlhTZ2Nxd1Q3NHNTN29aK09CbTVwM252TS9xMTNDSmtkdkJFVUlSamhIUnNkQXZsVGZhbGdreEFWb0RWNlRyeHZ1alFtQ0QwTHJOZjdZVFBHRUlqSENPWVRMZVlkcE00L3BhRkxVZ3Y1MjF3TGVMOVVFQnU0Qjh5M0FIdlErNGpLcUVENFJoQnlSMStqc1Badjhvb01hUkZmNnN1VlBRU2t5MzliK1ZKVjA3T21VeUJXQWpIQ0U0QzhvY01qL1JudDlDRmhYcEpVVVhTaFhDY2tFeWsrRE9qV2ZPMXF4U2JNcUZjaEdORUlYZjhPWTE0cTcyWFB1UmN0bUcxaG5Dc0MrOUhJaGxPcEtneHNnM1JzVU1lb3BJVGVFNHJwMnZQZS91elVDUys0V1QwTGNOS21VV0xwNHQ3bnFKRWx1SFcvVTB6T2E4eW1RSlJVVGxHVkZJQnlHa0djcTFlcU1lanYvaW9WdXB3WGZvQmlFMFczajBTakFHL0NNZUlUZ0x5WGFaSC9nLzZrS01qbE9uQVRVcEVNakhuNzB5Zm1qRExHRWtSanBIS1NjWmpuK2hEam90d3JBUHZRd1N5c2NkbFpsdEJOOUdpaHVRSXgwaENLZ0xIR1Fma2VrYzl0amdOVEQ1TE4xbi9rdnJkVU9VTFQ5b29iak5kdDFFUmpLRUY0UmpKRkJDUTNlUE9QNGVUMFptQzE1STdxcFpwTVg4MnNNYjg0aHo3aTJ0amdqRTBZRm9Ga3BQMmc0Zk1KdzdNWk50VE5xMEloS2tWeWN5Zkx1NXBJUXBFMWkrNERZYytadmtML3ZDQlRUNmdCWlZqSkNlQjhUaXpiYVpYMFdZUkhoWDZOQWcwZ1RUYUtBakdRRVNFWTZqUTJHWTY1NEJjdDFrd3pTSU1McTVwY0ZNU2dJeUZ6TDJOb2lJWVF5UGFLcUJLbzFLUysrUHh1YlJaMEYvblVjYWJ6R2gxeGU1bGZzbU5zL3Njdjh2cDk5cUFZQXlWcUJ4RGxVSXF5TTYrYkJweXF1QzE1SVRqR1JmSDJ5TnB1M29rR0FOcFVUbUdTZ1ZWa0N0WnJNY3FiVStvSGtkei9uUnh6NDZRSGhTMDZLNUdNSVpxVkk2aFVrRVY1RXA2Q3RsNjJwOXBJWitibEJaVWpmMFlUa2JITXEySFlBd29RZVVZcWhWV1FhNWtXKzBwVmVSK3BGM2xzK1hmUWJsUFR4ZjNMTVRyb2NCcWNVVXdoaFdFWTZoWFlFQjJ2anhkM0ZPWjYyRTRHVDBVc05JL2hkblR4ZjFSZWIrMlAxSXR2cFMxQjZVZ0dNTU0yaXFnWHFQRkl0ZWQ5TmI1N01LZDNCaWdHNllvaE1GeDdjaFZpNGVUa2R2TjhTK0NNYUFYbFdPWUlZOGhTNWo3dWVyY1BYNlY3YmJSQXUwVjN2RkVvNlBoWkRTV21kQWxQUUZieU1qS1d3V3ZCZGdaNFJpbUZCeVE1OUtMZkszZ3RaaENlNFUzZDA4WDk4ZVovQzdSeU5NZkY0cmZGdklyMTF3d1BtYjlCQ3dpSE1PY2dnTnlKUXYyeHJMbE5uWXduSXdPWkJwQVNSVTczMXpRT2VEcHhlN2tQRFV0OU1rRndSaW1FWTVoVnVIemJMKzRhaFJoWlRleUFPb3ZDNjlWcVY4Sk9ydVRGb3JUd3ZxS2E4eHRoM21FWTVoV2VFQ2VTeTh5QzExMklJSGxxL29YcWcrTHFYWlVjQXRGYlNZVlkyN2FZUnJoR09iSjVobC9GUHhPM2tsSVp0SExGc1BKNkt5d3ViSjlzUUJ2QjlKQ2NWYjR6b3gzc3ZpT1lBenpDTWZJQWxYQloxY1NrdWxIZmdYYlMrL3M2dW5pbnJGdHIyajBGVThMNzJubnM0S3NFSTZSRGVrcnZXYmhGZjNJMnhDUXR5THNiRkY0WDNFVFR4ZVFIY0l4c2lJOWY5ZGNzSjVYaTU4UmtqY2pJRzlFTUg3RmNESTZrZTlXNmVlWWluNTA1SXB3ak93VVB1cHQxVUxtSTNNQlc0Tis5Wjk4ZXJxNFAxUDJtbFNRSjFPbkJTKzJhMkp6RDJTTmNJd3NzVURtSjB5MjJLRFFuY3RXY1JPMUFhSDRKNHhxUS9ZSXg4Z2Eyd2YvaEpDOFJ1SHRPSE9wQWhKMkdnakZhekdSQWtVZ0hDTjdWQWJYSWlTdmtLY043bmk4VS9YQ3dycVJLaUJoUnhDS042SVhIY1VnSEtNSUxOVGJpSVY3SzZRUCtUVHpteW5hS0ZiSVRmU1lVTHdXQys5UUZNSXhpaUdWd1dzdWZtdlZJZm1TT2NuUG41VURPUjQ1VnBHcEZqY3drdTFWQzlueGpwWWJGSVZ3ak9Ld1M5cFdWMUpKTHY2Q0tJL1lMek1KVG5NSnhjVlBHR0R6anAyd0ZUU0tSVGhHa2VoRDNzbWRoT1JyQTY4MUtPUFZSZnJMaGJSWFRabGlzOVg1MDhYOVZQbHJCSUloSEtOWWNxRzhaQjd5VnZOR3kwWFJWU1JqSVpsUUxPZ24zaG05NkNoZVJUaEc2UXFkVU5ESGxZVGtvaC9OeXk1cFk2V2ZteHQ1ajRxdStFdmYrSmpXaVoweHZ4Z1FoR09BbmRLNm9KcjhJNERWUVRubEU0aVozT1JkbDc2Z1V2bU5pMVpYVWpHbXZ4akZxd2pId0ErTWUrdnNTa0labGNxcU9tNzhoUHdjeldXTDlPY2ZBdkh6ZDdkdW5hQkt2RHZhS0lBMUNNZEFBMjBXdmN6bDV1S1NSN1Bmdy9KUjQrZE54NTVYdHpEU1ZmUWU2cC9TdzNDbHEycHZGVzBVd0FhRVkyQU5wbG4wVnJkZEZQK1l2MGtDM1Q4dC9wWC84cWo3QjdsNVBaRWZibUM3TzVmRm1ueTJnRFVJeDhBR0VtU3VxVXIxUmo5c3czQXkydm1rKzNSeFA0anhtalFqRUh1MWtHcHg4ZU1aZ2RjUWpvRXRocE9SRzkzMW1lUGtSUjJVYjB0OW5FczQzbzVBSElScnp6bWhXZ3hzUnpnR2RwRFpUbWxhMUQzSzF5V05oaU1jcnllTDZvN3BJZlp1SVMwVVo1bjlYa0F3aEdOZ1IxTE5PbVhyNlNBV01ubmhPdmZwQzRUamY4bjNxWjd1Y2NLTlp4QXN1Z002SUJ3RExWRkZqbUsyTXFvc20wZkJKWWRqcVE2ZlNDQm10N3F3dmp4ZDNKL20vQXNDb1JDT2dRNm9Ja2VYVFZndUtSdzNXaVhxSDZhL2hFZTFHT2lKY0F6MFFCVTVtVG9zUDFocnc4ZzFITXNOWXpNTVV4bU9qMm94NEFIaEdPaUpLcklLaTJaWWxvMHlWRmFYY3duSGNtTjQxQWpFM0NDbWN5YzczVkV0Qmp3Z0hBT2VTRmc0WTZXOUdtNGF4bU1qTkQ5cUNBL1d3ckhNKzY2M3hqNlNmK1l6cmdPVEtJQUFDTWVBWjh4RlZtOG1vZmxCL3ZNeDVpZzVyZUZZK29QckNSSjFJS1kxUXE4YnFSWVh2N0VPNEJ2aEdBaEFxbTJYaEF0ejNPUHBieEtjSzZrNlZ6N0RjNnB3M0FpL0IydCthSW13WXk2aG1GM3VnRUFJeDBCQXc4bm9SRm90Q0I5NXFGczFLZ25RZFYvelkrTi9kNzV0YXVId0VZNGJyUTYxZWpGYzdianh2OU1Da1k4djduekNMbmRBV0lSaklEQlpzRGVsMVFKQVIzY3lubzBXQ2lBQ3dqRVFDYTBXQUZxaWhRSklnSEFNUk1ac1pBQmJMS1I5Z3BuRlFBS0VZeUFSbVdveFpkY3dBQTFYVWkybXJ4aEloSEFNSk1RR0lnQUVHM2tBU2hDT0FRV2tIOW1GNVBlOEgwQlI1ckxZTHRxc2JRQ3ZJeHdEaWtnLzhpbUw5b0RzeldWM3UwdmVha0FYd2pHZ0VDRVp5QlpiUGdQS0VZNEJ4WWFUMFZoQ01wTXRBTnNXc2lFUW0zZ0F5aEdPQVFNSXlZQlpoR0xBR01JeFlBZ2hHVENEVUF3WVJUZ0dESktRN0dZa0gvTCtBYW9RaWdIakNNZUFZU3pjQTlTWXkzZnhtbEFNMkVZNEJqSWdJZGxWa3QveGZnSlJNWklOeUF6aEdNZ0ltNGtBMGR4SktHYnpEaUF6aEdNZ1E3SXR0YXNrajFtOEIzaDFKYUg0a2NNSzVJbHdER1JPRnUrTjZVc0dPbk90RTVjc3NnUEtRRGdHQ2pHY2pJNmtta3pMQmJBYjF6cHhTVDh4VUJiQ01WQVlhYm1vUjhIUmNnRzh0R2hVaVdtZEFBcEVPQVlLSmxNdXhsU1RBYXJFQVA1Rk9BWlFWNU5QMkZnRWhYRzl4TmRVaVFFMEVZNEJ2Q0RqNEtZU2xtbTdRSTZ1WkxPT2E5NWRBS3NJeHdBMkdrNUdKeEtTM2M4ZVJ3cUczVWlWbUIzc0FMeUtjQXhnSnhLVXgrekNCME5tc3JqdWtrQU1ZRmVFWXdDdE5QcVRUd2pLVUtnT3hOZjBFUVBvZ25BTW9CZGFMNkJBM1RKeFN5QUcwQmZoR0lBM01ocnVoTVY4Q0d4UmgyRjZpQUg0UmpnR0VJUk12YWlETWx0WG82OVpJd3pmY2pRQmhFSTRCaENGdEY4Y3l3K3psTEZOc3pwTXV3U0FhQWpIQUtLVHF2Sng0NGNXREN6cUlDeGgrS0g0SXdJZ0NjSXhnT1FJeTBVaURBTlFpWEFNUUIwWkYrZEM4cEg4SnozTDlybWU0UWZhSkFCb1J6Z0dZTUp3TWpxU3NGei9FSmoxbWplQzhBTUw2QUJZUWpnR1lGWWpNQjgwS3MzTVdvN3JycXFxUnduREJHRUE1aEdPQVdSRldqS2FvYm4rVC9xWXUxdEkrSDJVSHhlQUgybU5BSkFqd2pHQVlzZ21KWlZVbWVzUS9ZYlJjcy9tamZEYi9IbGdrdzBBSlNFY0E4RExpbk1sNGJtU2l2T0IvTFBsbG8wNytjOXZVZ0d1NUQrL1VRRUdnSmNJeHdEUWtveWVPMmo4Vzhkci9vUjEvOXNtQjFLcDNkWGptdjkvSFhhZjBmc0xBTjBRamdFQUFBRHhIdzRFQUFBQThDL0NNUUFBQUNBSXh3QUFBSUFnSEFNQUFBQ0NjQXdBQUFBSXdqRUFBQUFnQ01jQUFBQ0FJQndEQUFBQWduQU1BQUFBQ01JeEFBQUFJQWpIQUFBQWdDQWNBd0FBQUlKd0RBQUFBQWpDTVFBQUFDQUl4d0FBQUlBZ0hBTUFBQUNDY0F3QUFBQUl3akVBQUFBZ0NNY0FBQUNBSUJ3REFBQUFnbkFNQUFBQUNNSXhBQUFBSUFqSEFBQUFnQ0FjQXdBQUFJSndEQUFBQUFqQ01RQUFBQ0FJeHdBQUFJQWdIQU1BQUFDQ2NBd0FBQUFJd2pFQUFBQWdDTWNBQUFDQUlCd0RBQUFBZ25BTUFBQUFDTUl4QUFBQUlBakhBQUFBZ0NBY0F3QUFBSUp3REFBQUFBakNNUUFBQUNBSXh3QUFBSUFnSEFNQUFBQ0NjQXdBQUFBSXdqRUFBQUFnQ01jQUFBQ0FJQndEQUFBQWduQU1BQUFBQ01JeEFBQUFJQWpIQUFBQWdDQWNBd0FBQUlKd0RBQUFBQWpDTVFBQUFDQUl4d0FBQUlBZ0hBTUFBQUNDY0F3QUFBQUl3akVBQUFBZy9vOERBWlJ0TUJnYzczSUFsc3ZsYmVuSEN0Qm0xKzl2VlZVUHkrWHlHMjhnc04xZ3VWeHltSURDREFhRGc2cXF6cXFxZXRmeU41OVhWWFh0L3QzbGN2bkk1d2FJYnpBWXZKSHY3L3VXZjduNy90N0s5L2VCdHc1WWozQU1GR1l3R0J6SkJYS3Y1MjkrWGxYVktkVW9JQjY1c1gzdzhQMjlxYXBxelBjWCtCazl4NWx5SjlEQllIQTVHQXkrRFFhRFpZdWZiL0x2dmVsNlpOeGp2c0ZnY04zeTczVS9qNFBCNExUMDl5NkNhdzhYVnVlakM5bDlQaXNBV3J2MDlQMTFUNDBlNVdZWlFNUE9sV081VzNVL3UvWTNsYzdkMmQrbXVDdjNWQmxjVkZWMTFQYlIrV0F3R0ZkVjliWEgzK3ZNbHNzbEord0FQTDAvcTJidXZFQUZDZ2hMenUxL2UvNUwzTG4rZ084djhNT3JDL0lrRUUrcnFqcXBxbXFmNDliZVlEQTRYeTZYMDhoL3JZL0s0SjcwdEozcytpL0k1OFZIOERvY0RBYlQ1WEo1NXVIUHdrc2hibTRQWFh1Rm5Dc0FoTFB6K2JpRlBibG1VUGdDeE1hMkNubTgvWTg4T2lVWWQvZlJ0U25FK3N1a011anIvWHJYOHBHNXozQTA5dmhuNFllRFFNZmlvOXdjQVFnblZJQjkyMkxxQlpDOW44S3hDME9Ed2NDMUJIem03ZmZtdllUV0dIeWY0TnEwTi9qOHV3ODkvbG40SVdTN0NwVmp3QzRLRW9CNEVZNmxTbmhMTUFraXhPT3dkVkpXNy9qYzZPZGpJYzhtOUlrRFliME4rS2RUT1FiRWF1WDRtb0FUVEt3Vi9iN0Q4VTZ6TUFNOFVwOTUvdk9LRjJGVmVzZ0xONEN3YUo4RXhQZHc3QlpBY1hITGd0Y1RYSXNWekw3RE1TdW4vV1BrR21BVVBjRkFQTS9oV05vcG1DOGJWdkRkeEJKWGI1TlVyTkVLYlE4QUFHeFJWNDVQQXZjaUlrSTREaEJRMjd6bWxIODNkaE82Y3J6Z2ZRQ0NvWElNUkZLSFkxYXBobWN4SExlcDN2cXVTbEk1OWk5MDVaajNETEJyem5zSC9Lc094L1FhaDVkNzVkaDNWWkxLc1graEs4ZjBpUVBoaEs0Y2M4NEZ4SDlvOG84bTkzRHN0U3JaZHR0cTdJVEtNWUJOT09jQzRqK3NZSTlpRVNuc3BRekhQbnZXZWJ3WFJ1aDFCVlNPZ1hCQ1ArRWxIQVBpUDZ4Z2orSTIwdC9qOVVabjEwQWZZSDR1SjJuUElzdzRycWdjQTZaeDNnVUVsZU00cmlQOVBUNDNjR2xUdmZYOUdhSUM2VitNN3pudkd4QkFwSnRid2pFZ3FCeUh0NGdZam4xSzFtOU1CVEtJNE4vejVYTEord2FFRWVQbWxuQU1DQ3JINFoyMTJHV3Vzd0FMSzl1OFppckgrakhqR0xBcnhzMHQ0UmdRLy9IOEtCNHZ1ZGFFczBqSHhIZjRZY1p4WHBoVUFkZ1YrdWFXUmRCQXczODRHTUc0U3RwSmpLcXhTTmtldzlNSC9aaHhETmdWK3Z4TzFSaG9jT0g0amdQaW5Rdkd4OFo3TUpPOTl1VnlHV3U2UjBtb0hBTjJoYjY1SlJ3RERTNGNYM0pBdkpvbENzYStUMjV0QXFyUHY1dWJ0VEJDenpqbTRncUVRK1VZaU9nL3krWFNoZU1yRG5wdnJscjhaYmxjSGlXcUdGOTdYQlIxM3JJZHhPY04xcW5IUHd1TWdRSnl3TTB0RU5GenovRnl1UnhYVmZXaHFxb2JEdjdPRmxMbFBLK3E2dmZsY3ZsbXVWd21DM1lTWnNjZUF2S3NiVUNWTm9oUFBmL2VTbTR1YUtud2p6RlFnRkhjM0FMeERaYkxaYkxEUGhnTVhKajdHdml2K1cvRVJYSEp5WWwwNmhZRHRxdzJ6R1RzWE9jcThHQXdPSkdBL3E3bHYzb2pmemZCT0lEQllPQStEMytFL0R1V3krVWcxZThINUV6R2RQNFYrRmY4aFZGdXdBLy9sL2hZSElUK0Mwb0t4dFdQalJqR2lmN3VhNk1ibnVTT01WQ0FYY3c0QmlKTFBjb3RkRGllQmY3ekFRdENmOCs0c0FMaGNITUxSSlo3T0diMktrQTRCaXhqVWdVUVdlNXRGV1ptcnc0R2d6ZUJUNElQc1ZwTWN2cGRNa0U0QnV4aXhqRVFXZXB3dkIvNHo3Y1VvTnlpaXo4RC92bS90WnhkM0VkT3Ywc09Rbi9QdUxnQzRWQTVCaUpMMWxZaDFjWFFMTzNhbGRNT1p1ekdwc1JnTUFpKzZKV0xLeEFVTTQ2QnlGSldqbVBNYnJSVU9RNTZzeEM1RFNHbjN5VzRSaHZLMFk3SHpsWE5IM2RjWVU0NFZrekdkRlh5UHZsOHI3WTlXZG4xODJOQzR6dlViT25hcGIzcm9YR2RxUDlaemJGaHhyRXRQVDZIajQzM29mN25yTDZqc2NnNWRkZld6dm80LzNTK1RCbU9xUnkvRlBJa0dIdHFoOW5mUmI1WWJiNWN0V21iblJGbEp2U0ovRjF0Mng0K3k1OXh0OFBmRzJOY1l2UVR1RlRFaitYMzIvV21ZcFhyWForR2ZhWC9rcERUL0hrYitLLzh2TU5yYXY3WGV0djJCL201MW5vVEt1LzlrYnovOVRIdFdsMWQrejdJc2JtVGkrZXRmRlpTWEUreTNNQkhRdVJ4eTZMQXFtL0w1ZklrN0N2ZGJPVjNPQTc0T1p3MXZwZTNpVDZIcmZVNFI5KzIyVkJOenEzMXRiVFRlWFV3R0xqTjAwNlh5K1haOS8vUmJRS1M0a2QyWVZ1Ry9FbjF1M1g1a1E5K3FPTnhHL2wzdWJYMHU4aVg5a3lxUmwxZjEvRU9mOCtCaDc5bjljZjlXVWV2L0oyaHYyZVBrVDliWTQvZmxiUEFyL1ZFdGxaL0RIMnVDL0R6VFM0V1dzNlBSL0xkQ1htZTNQcFpsL2R6NjNmZDQrODlEZjE3SmZqKytybytYQ2Y2SEo0bS9oeCtrOC9oaVpidjU1cjN1TS94MmVrYTcvbGFVUDljZnYvekV4N0FzOEFmb0FlTkg1eFhqa2ZJWXhFMEJGaitYZVRpNHlPc2psLzVPOTdJeVN6VU1YRVg3VGNiL3U2UWYrOHkxbzJYVkFWOGgwenZGeGQ1cjArTkJ1SjFQNWUrajFHTFkzbWcrRmcreW10Yis3M3plQXl5dUxtVlVPbTdhREtOOU5yZnlIVWlaU0RlOVBOTnN0UkJxdTlwZ1BmNDFXdUtoT0tRNTRUbjYwTEtBeG15dXJqMUFHdjZrUys1N0QzR0FBQWZCMGxFUVZSZnlHTVJ0UUprNVhmeEhCelh2aTZQNFh2Yno5b0xSWVR2V2ZEd0ZEQWdlTHVnTkVKeGpQYzY5cy9HRzc5QTcvZFJoSnM2WHovZlFvWmsyWEUwNU9zUGZwMlVNQlBpdFc5OFl1YnBkUjhZK2h3dTViVW1DY255bmZWMTdsdDd3eGJvQm12ajM1OXlFeEJtTi82UXpYU0h4Z0tqVUx6OExvUEJ3SjFJM29kNmthNFBhakFZdUMveUh4RldtMWZ5Q0g4ZDB6T081WDNhMmovYndkeFhyL1JnTUpqS2NmZ2M2YjJPYmVmK3Z6NWNqK0pnTUhCaDhPK1EzMDNQOXVSOWY1UjFCTDZadms3SzkvZHJpRDg3Vk8rdDZ5V1cxLzJQb2M5aEphLzFZVEFZUlBtKzFxVG45OWJqdWUrbk5UanlPLzBkWWEzRzg5L3ZmcWVVNGZndzhKOXZLUnlIUGdIbU5OMmg5KzhpWWNiM1NlOTdDSlUvUDlZWGVSdXpNNDREMzhEMG5wTXRGOUdZTjBDcEJQME15WEU4bFREeVR2V1IyTXk5LzMrNmNPOTVUS25aR2NmeW5vYjYvdDd0OFA5cHJYR2pheWtVTnozZnJBMEdnNGNZWXp6bHMrNHpHSy8rK2E3STlCQ29RUEthTjBtbVZVU2FjVXpsK0llWXExdFZWNDdsc3hmaXp2cEEvdXhySmFIWTlJempRRGN3VGIzQ2NZQnFTWkhrT0Y1SHVJbUx4WVg3VzFkRjl2Umt3dVNNWTZtaWh3dzBYamVCa25QbHBaWnp0d2VIVWtVK0RqemQ0aXpFWjdReDNTZkluNytMVkpWalpqZSt4RnpnSFhuNFhVNERmZGtPSkxock9ybWFETWR5b1FyOWFMRHp4WlZnN0VmakNVc3V3YmhXQjVOZTF6bXJNNDZsU0hEcCs4OWRjZTNyRDVJZ3IrM2M3Y09lM0tnRitSekplVHBVQWFOdXgwbDFqdjJXS2h4VE9YNHBweG5ISVN2SFBuNlhzWWMvWTUzOXhCZjVkZFVCcXpPT0x3T2ZGR2RkWDNlaHdkajdJMnhwbWZuRDk1K3JpUHQ4OUcyeHNIcWRERjN0Vy9pcWhrcnJ4NThaZjU5REJ1U1FNK0pEdDkyK3luMitzcTBjRzl0Wkp1UkprSDVqSVJXQ1hFK0M2NDVONkhBODkvMEh5b0xPMEJXY1RsWGowUDExaW5rN2wwcC84YlhobnM0MjlucysvamQzY3h1NG1sanpValVPdU5oWEd4ODNhdXNrMjRBbGhsd3J4OTR2Mm9HRnZFdUtmWk1RTXRqMHJSYUU3b2RPS1VVNER2SFppckZqWGRkSHZ0ZUZ0bEo0NmU5czNGeFlYWFRYeFdHUDZRSG1ibTRqVFRieHNaZzI2TFFpaGZaOXRycklUVkJ1N1ZDMTV5ZGx1VmFPelZTTkl5eE96R2x2OXI1VjhCZzlmS21rYUtzSVVYVUtIWnc2UFpJZERBYmpESHNTZCtXcnYvTXM5ZVBTUkQ1M2ZLeHQ3ZnY3SmxJMXNkZm5zY0JnWEh2bmNkeGdqUFVzU2VWYU9hYmYrSWRveDhMQWpPUHN2OUFyckZXT1EvV0RON1crc0FhY2NHTEJsWThGdlFVSGt0cFpoMzhuOVBmWDl4U0RHRzFyTjMwK2ozS1R5K2V3djV5ZndqNS9MMUtGWTJZYy84Q053dTc2WHFSemZRemtlZ2ZYUFdvME0rTllBcWpXbG9wcHpwK2RWeXg4M0JRUVNKNjk3VkE4Q0IyT2ZhOUhpWEVEMmJscUxNYy95SVlraHV6TDl4R2JQWDh2VW00Q0VoS1Y0eDlpSGd1MTg1b2p6ZnhWdytDTTQybUVxdE44dzAzRU5qRkN1MGJUdmd1MnBKM0FWN1hLdXJhZm85QTNaTjRxeHhLNFFyL2VSZGR3M0poQkR6ODNNVGxYanAvUGVkSERjWVJIN3hXVjR4OGlUKzNRUEs4NTUzQzhic1NkbVhBc1FWNWwxVmd1K2lVdXdqdGZMcGMrRnZDRUhzdG55YnRkYjFvajNkeDZxUnhMOEl4eEEzVGQ0eHJBNS9DSC9VZ3p0SzFLRTQ0am9YTDhyOWhUT3pUUGE4NDVIS2VZVk9IenhpdldMa2hkd2w2Smp5QS9MWmZMM2pjck1xV2h4QVY0cjlsMVFWU004NVd2eW5Hb2paVldkYnBaazBWb0pVMUkyVVhmODFyT2k1T1R0VlVFcnh3Ym0zRWM4aVFZK3pob250ZWNjemhlOXo2YkdBTVY4Y0oxMC9hOElOVzdraVpVdUJ2UVg1ZkxaZThxWU1TbkFkYnNldjJMY1hQclk2R2xLNGg4OVBPS1h0VzFKYXFpcldldHJHY1U5MUZQTThxeGNteHR4bkhJUHEzWTRUaGs1Ymp2YkV2Q2NmaS9zNVZJMjh6V3Vsd2dTN2lBdUQ3T202cXFmbDh1bDBlK2RoNkxXRTIwWnRjYndkRGYzOTY3alVidTQrM1VKeXRQTDBwY1RMdk5mdGZXblVpdHNjbjlYNElYRVByQVdwcHhiSEdUaHRkb3ZoalNWdUdYajg5V3JOM211bGFkWWwwRTdueHR0TEdqQi9uTWZQTVlocitMdEV2YWErYnlPejQwZnRkVlIvSWRPWXI5ZE1DRml4MCtqeFltVlZ4R0NwNkxMdjN2RVNmZ2JMS1E3L1dEbkMvWG5UT1A1SWxyakoxQlZ4MUhMRTVZOFgyci9CVGhPRFJhS242SU9lTTRkSU0vbGVQTjFsVnZWSThJbExtM3NmcFJ1NjdPRHRudXNaQnE5cG1QeDl2S3BKZ0pYUi9QeXgzYlo3NmZUeHFiVjhTcU11NXlMbEk5NDNnd0dKeEY3T1B0MmhZUll3TE9xbnFpeHRtT041NHZybXV5QURqV2hrTmRQMlBaenppdUVyVlZoSDdUQ2NjL1pET3B3b05jSDYxdDZxVlZPMHM4OG9ZUTg0NVZwOUEzZXlmTDVmSTB0MkNjcUdyOHhaMUw1WGkyL2x5Njk4QjlScGJMcFh2dDUyRmU0Z3Nhd25IZmpUUmk5QmxYalp1ZUxtSXZwajJYeitHNDZ4TVorUnk2OFBuSi84djdTUkh0RVMxOS82eFJPVTRycDNBY05FejBXSXlSODR6anhib0xRSVF0eWF1dW42MEVPNlYxcldLRy9EeGY5Zms4S3hjemtMalAvN0hQMWhBM3BXTXdHRlNCdzk4dW9VVGxqR01KeGpFMzB1aDBBeGxwN25MdCtUeThYQzY5OVYrN1JiR0R3ZUJiNEdQZDlUcVJhNmorMEx5NWpsbzVac2J4VDRLR3RweG1IUGVVWXppZVNURFkxRThaV3R2SkQyOFNCT083SHJONlEzNW1jbDQ5SHlzY3o2Uks1NzFuV3NiWUpWdllyWFhHc1h4L1l3YmplWS9KS2JFK2gvVU5tdmVGaVhMdXV0dmgvOW9WWXhiL3RaQmcvT0pha1dQbDJOSmp5cEFud1VYQVAzdWRrSUdzN3dsQ1d6aWVTN2lzcTRlYkZnM1ZWbThxSDdhY2pJUGZxTFM1OFpLTC9YV0NrM0dmM3RkUU4vTHpFSUZPQXhuTEYyV0JsbFRxUXA3clhTajdJOUNmdmUzN3FXckdjV01xUmV3Rlk1MFcwMFVld1hnUytQdDhwbkNjcExiWGM5ZFk4TGhwNFdQdFlPWDc5VTNXS2Z4MExva2RqbVBNT0xaMDRRbDVFb3g5SEtnY3YrNUdMakMzSFNyNmJSL0JoNjRjNzF4Vkd3d0cwMFJqdmM1N3RpNkUranhiZXJMVlZxelJkNTE3T2xzSStlZHZ1MGxVTStOWTJoTmliZExUZE5lakdodnJjL2dsUW50VXJ1MVhmY3psV25vZDh2am5Wam1PWFMzdEsyU1ZKWFlGWGZPTTQxVEJmU1lYbGo3Ym5uYVJmRktGdEZDZEpYcDB0L0F3TVNIVTZ5WWM5OU1uTk8zTVhYU2w5emlGNURPTzVmdDdtcWhLdUhZdFJRc3hXaXJjRTZEZ1UxbmNkV013R0N5MGpFbE5PT080N1JTUTNuS3JISnVwR2tmb0s0dDlMRFRQT0k2OWoveVZQS3BKZGRjZit2ZmRHUEFpanlMYXBOY2o5OENUS3JJTXg5SlNFZU1jVU1KMjNza21WY2o3T0UzOC9lMDBkYVQ2Y1YyTmNVTWVjMXpoUTJFN2RUYk41VmpITGpCbFZ6bW0zL2lIYU1jaVF0QzNVamwydlU5VEJhMDlVU3ZIRWliSFVqbE1QVEx2eWtObFVmTTI2RnJGcUNqZFJWNWtuRXJVd29tY3YrdWIydFRmMzd1ZTI1ZkgrQngyR2crWmlWaVY0NFZjUzVNZDU5amhPUFRkRC8zR1A4UThGdHFuUVlTdUpIZ2Y1ZE5UOE1xSlZJaVA1VWZMRE9tWnB4MnhRbDRBc2x5TUYrbWltY3VVajIwTGpFT2ZUOTlJaGZoWXlRMXRiZUdoTlNkR2EwL3N6MkhPRzFpdGN5WEJPR2toZ2NweE90bFVqaU9NcE9zejR6aEcxWGlxSlJoSCtuMC9SL2c3Mm9veHdRQnJ5R2N1K0Exb3pPOVloRTFnWGhNNnJMNVB2TDMzSmljZXZyOHhidEppbitzMWJXQVYrdmpPM0NZcWdmK09uVVNiY3h5cGtadktzWWo4YUYvem5hMjZtYitCcGJ5b3B4Umpnb0VQMm5lUzdDTEhRSkxrZTVUeGhrWGJmT2k3UmtOdWFFTDN2YzlpdHZZRXprMWJGMlltb0dZNlI0cnRvME9pY3Z5djJGTTdRdjR1ZldjY3g1ajVxMm5jVG83aGE1c1BpbHBhdG9rMVppcW1HRUV5OXZzYnNucjEyazFjaWVINHlsTnZhWXpQWWV3ZTJKQ2Z3eTU1S1hScnJKb01GN090SXZnSGx4bkgzOFUrRHFWWGpqVXA3ZmYxZFdHTjViMVVnMjdsaWNNM3o5L1hid25PZ3pFcXg5RnVRT1g5Q1JrQ1hnc0F4ZldYZW55TUh1UGNGL056ZUJDNC9VVmpYbEx6bW1LRzQ5QVZMV1ljL3hENzdpdmtDVjM3cElxUTIzdDJVVkxsMk9lRk5hYjlrQmM5bWMrN2tBdk43UTQ3S3ZZVk9wVE1ZdldTUy85MDZKdXQxODVwSllWajM5L2YwSi9EUmF3Yno4YXVoQ0cxYWcrSjFCcEw1VGdBWmh6L0VQdFlhRm93c0lyS2NaNit4QmpDYjlpZVZEK2ZLNkFTbUc5ay9yYTNpNjVjeEVQM2VVYXAxalcyT1E5OVBudnQvRnpLelcySUc5c3NNb1o4cDI0akxITFZ1UHVlbWh3WHMrYzQ5SmVlZnVNRW1IR3M3Z1JUd3NYMUE4RzRrM2RWVmYwNUdBd2VaUlNmRHpGdXhvSmVNRjBZR1F3R3AvTDNoQTRrMjZyZ0pkemNmdklkakhPNVNaUHY1V09rNlM5dHYxZkJLOGVhcGcxUk9VNURlNkJzby9RWng5cmsvUHU2Vm9Galkyc0xOSEtWMGE4U0NNYzlGNVRHK1A0SG1RN1EyTHhtSEhHSHoyMVYrNXh2YmtQT2d6YzdsVWcraHllUk4ySFJ1SUI1cnVBMWZCY3pISWMrK1ZBNVRxUDBHY2RxS3NlUmZ0OVVaaklITmNZWXBSSjJZYXZrUXZ6WFlEQTRYeTZYWFRkUGlYSXVjLzJPUGM4Rnh4SThqK1RuT05HVzk5dENTYTQzdDdQQTR4WmpmQTdmeU9mb29XdUZVNEx3Ry9uODFaL0ZGRzJKWGNKeDZNcXhxdk51bEhBY2FhQTZsV01SZWJRWWt5cjB5UFgzN1JQZXVpZ2xITmMreWtYL3VNTkZQOGIzLzYvcVI5OTAxVmhzdU0yQnN2VVFzOWZDWWNZM3R6RjJQSXZ4T2Z5ai9vZVZSYS9iYVBzY1J0MVFwNFh5d25Ha1IwVlVqdE1JK2Q0eTQ3aWQzQzZ1Y3crUC9WdHpmMThqaUpYQ1ZTeHZwVUxiNWx5YTRseTJGMkhlYWdqYnRoM083ZVkyNXJiNktjNTl1WDRPTnduOXU2b0t4N0VXNURIaitDWE5tMmEwcGZtRVR1WFlycnEvT05YTngwME9CN0dsd3c1dFFxWHU2TmJXZklkNTNMbmQzQjVGckZDV3VqTm9XNHNlNFRpMElzTXhNNDVmMGp6NnJLMlE3eTB6anR2SjZlSzZGM2gzcUcyczdMam4yK0ZnTUdoejhjenBYQmJTTHAvbDNBSmV6RllvN09hc1MzdExwQm5IVkk0RHNEVGpPUFN4aUYxcDA3eUFoTXF4YmRPRWZaalhCbSs2ZmZrWTZXSllpcHNkbjREa1ZqbitHR0hVWjgxaWUwTnNNK1VqTUtrY0IyQ3AzemliRTJDRTRNS000M1p5dTdqdXBhbytTWFZGNitQSEdMWmVSQ010dExadTN1SUpTSTdIazNua09peDZQb21MTWVPWXluRUE5QnYvRURPd2FUK1pNK1BZdm1uRTZ0TUxVbVdaWlhoTWQvRjJoL0JiMGxibFhTeGsvT0N1eFpzY2orZjcwRGRSbVkrdzlDWGtHRDBmVk0wNHJpS0c0eFR6SkxWaVVzV09tSEc4dTR3dkVIdUpxMC9qZ3RzcjZCbnRyc3VHTmJuZXpJZCtBc01Uak5kOThMQXdNdlF4VmpjK00zZzRqdlRvVGVNZTRadW9EWlFkTUtsQ2o1eC8zL2NKcThjUGlSY0dwblN5NWUrbUwzbTkxc0U0OCtyblczcllrL213dzVTVVhZVCtmSllYam5uMDlwT2NRb3pta1hTbHpUak9YYkxxc1ZSZGZpK3dncnhIWDNGck14bGgxdllSZHU3SG1kN2p1Rnlid3ErZWduRkY1VGlNNEJVZlFzcDNzVWVMc1R1ZUhqRXJNd3Y1ckoxWFZmVkpnbU5vNzFOV255UWdIeGZZZzB3NDN0Mlg1WEo1MUhGaFVjd2lVdjM5dlpMdjcyOFJidnhjOVhqYms0aXVxRXEvZE43eEJ1MDFvVnRqMVlYakdEdmtNU1QrcFpBWG05aFRPMEtPejJIR3NTNnoxeFoxREFZRGQ2RjlIL2dWbjZhOEVMcmZYUUs2NjhYOW5PcDFSUGJhK1p0eis3L2NaLyswNTJyN0dEY2h2Nzd5L1QyTDhKaytLM2grZUF4MzhqbjBXaXlNOVBTSXluRUExa0pLeUR1d2FLdFJEVHh1cFhMczE3Y3RsWWhwcE9wVDBpcVJtendnVXl4K2tWQ1VlNnZGYXplWnBZZGo5Lzcvc2x3dXh4N0dVSVcrbVo5dCtmNmVSZmdzN3c4R2cxTDc5ME55dTNuK3Rsd3VRKzBvR3VPcEJ1RVkyZEMrbVVscE00NURlL1hHSytKY1lCV3poMTBZY3FGSXptOGZNdDV1bXJhS2wyYlNpdkJmVDZHNEZ2bzR2L3BVVWI2L01hYVQwSHZzaCtzcC9pSTNaeWVCVzB1RG53TzB6VGl1TWduSFprSktoR3ByekxZSzdYMWVwYzA0RHIxRDFDNmZyUmpWcDBOTjFTZXBKRi9LQldvZy9adGZKQ3lYT2lNNUp3dDVMejlKRUhHOW5KMjI0TjBpOU0zODFxZUtzbmdyOUx4WlZ6MG1JSGZqbnBML2YzdG5lOXpHa1lUaG1Tci9KeThDMGhFY0xnTFJFWkNPUUZRRWg0dEFWQVNDSWhBVmdha0lERVpnTUFJREdSQVI3TlhRdmRJU0JFSHNiTCs5UFR2dlU3VmwrNGVKM2Zuc2VhYy9Qb2w3ekhtNndUSXlLdEZqMDEyTzQyRGtjOHphK3o4WmZRRlVCR29jRjVEanVLVENNeG9jczdrK0d2a3VwczFWS3dwYkZSbTN6OGF1cEtIcmlnUzcvNTNEcWFSYW0vTDZlaTl0ZWQ1NWtOLzdJSWZBcFZ6enJnd0xKNkFQODhjYTgybHVmUVcvU3lyc2d6aGdvTmpJZW1NMURqZnQrT3VNd3pGRndPb3lWUVMwY1d5VW01U1pLb3lSZmtVdkRrT3d1QXAyczdBYitlRWUrNzBMdVo1Rit0WS8rUzRxcGltQ0l1b09ZZ05JUmtacTY4K09QbGVUcGZoM3YyQm56SjltelBsdW42ekdOTlE4SGViVG5CSmxGN20rdDJYaFMxR1Exd2ZHNFd4SDlPcTdGajkyK21idDBiMmd4aHpId1VBNXByL3hORUViWTk2RFd3S1Y0LzBZcXNlTEdPTmRRZW9UaEhURkx4czBLbE9JeTNHK1Iwa3JPUXVDdDhNODFlTWoyWE96TUVXeHJrcmxHTzF6ekJ6SDB3U1ZyN0psYUo5YUJCQjRXdFRoeW5IUDcxMFkrSkdkc0x6eEQ1Q2J5NkYrcisyQWlNTFZZZDdJOS9qRVMzQXRlWlBxY2h5SEtSakh4QmE1QXJ3RS82ajd0RWpndisrTlh0OHJoclRGbGVsODRtVjNQWEJvTHFJUGlMWDByY2ZEdkVYUXExWlorS3B2ajVEVW11TTRUTUE0TGkzSE1Yb1FXR3dtRm92bVVFVnFTbGxCamdHZTQ3anYvMkNvUGxFOXhqTG14bFZMR2psM2gzbTVrYlhZWHpVTzBlZ2JqSnBGdmlwekhBY3F4N1lZT050YmJDWm9ZMlNyRUNFK3Bhd2dIc2o5WGd2MStLTlI0Szlua0llak1kMHFhakdPdlI3bUxlYnYrd0lLU3AxVmZFTlZaWTdqTUFIam1QN0d6NEVxaUJJWmowNGRwZEduWHRJaVdlRWh4L0VMak5UandNSUNPTjQ0cUtMbndVa0JocE1HTGcvemh1cnhJTjlqbzdnamRKeU5WNnJNY1J3TWpHUG1PSDRKc2tqQ085UUpWLzZ1aFJFeWFLRmpqbU1JUTA3MlZ1cFR6ZXJ4S045dVpKVFU0RGJqK1RCdk1YODF5c0tqaXcvVld2YTZ5a3dWQVdrY004ZnhxNkFOSy9YTlJBek9wVUhVYWlnaFUwV0ZPWTZ6RnpCUmp5MENHR3VPZkVlSkVNZW9obWpscDRScmQrOWs3emtGK1I2ajk5Vmt3TmVvSHFQRkpyZkJsRWpsdUhZL3dOZEFENGFQbXB0Snh6QzJLTWU4S2NEZk9GQTU3bzJGK25kcGRGQndoUVBEMFdJdTNFM1Y1OU5aQVovWHNKaS83d2FXaGJjUXltNHJQS2lodjlmdFhvbzBqdUdMV2FFNWppMEd3MUxqbEN1TDFkcklNQTVLaWZ5WjQxaVpvUUVUaFVXK2x3YlNjRG1tM3kzVzRETlowNmdnNXpGb3p4SEI0cHZCZXc2WnZ4Yjc2b21NdzVvTzRlamI0aXFWWXk1ays3R2F4SC9FR05ORXZ1Nmp1cVFOS0FYZXhSalhVaVhKd3BXaVJhTWNNSE1jNjZKMWJWNks3Mkl4U0lBc3FqSmVjR1FjQnptZy81VktHMDlNUmZaV3dPYzFMT2J2MlFEMTJHb2NwdjN3enhqajdkVGpISXdPbzI2VlkyVDVhUFFDVmxxTzR4Ykx3ZkJPbnE4eHhvMXNkdXVkVGUrMGM1QkJaejA0aElaTFJXQ09ZM1ZVQWlhU2VoeGp2RGNZWTRzcEg4ekZNTHlTQUNGMFc3NDVIOU9jbGJYRkt2ajZvN2lPZlplYnBxWFhWRkJPVURuTXB6YU9NWDRESDhZU056bGw0YVZzdmNYNjB2SmUvT0h2TytOd2F1NTJGb2RRdDhveDBqaW1jcndIV1dRc041T1dNM25HTklBUG9hRWFCK1k0VmtmVDhFanEwNS9nOS8xM1VwOGtFRkFGTVVobjhveWxXczRranNQS3hTbjA2UHVsZ2RHMHkyVmJxVFBHdUpWNXVaTE5kbGNBc0dDVnFkQzZLK0J6Z0J1RGZqNFRWNkVjcGZwdWhQMnRGYUM2NDdBZGY0OGo3QmRyeGNPaWhZdWkzLzIwYVJySUk0T2lBVDRMMUx1akgxRzNrRzFUNG5PcTBlNEczMzNqYkN3VjliMWlTS0hmZWEzMHJoZXk0ZFk0SHg5N3RsT05iZlJpM0NWVjM5bDhVTjBuUmNTQWo3MmMvVUFPajdXUHdYWWN6aFg2K2diZHorajljY2lEOURtdXJSQkRIMWk4NURuZk5QemltT01ZZ3JZQ1o1RXY5RXo4Y2JOSTR5ajU2NHZLZldud3ZoNDVlbzJTZ0V1M3lmd05PWk5Zano1ajNHVUJud1BjR09RVXppb0xMNHJwZDh3ckZVVWFoNStUWC9UQWw2NDJVMFZBQmVUUlNEbE0welIzM0V5ZW9SWHN3UnpIK3FnYXg3S0JtVVMrNTZ4REVvU3lkdXgrWkVYZnpER3NVdmlUcjQ2Q3RWVDNTWm0vRmpuRjU1bHRXSE8rODEzZUQ4eGFWVzJPNHdETVZsR1ZrWktKbWs5azRYeFI5Skhpb1V3ZmhPK21oU0hWVzMwU3c5aXEySTFudGhuRzhaMkJvbGdTYjQ3eFFuSWM3Mk5ocEI3M1hpY00wMGFXd3BDYk9pckhBR2lrdkEyTjQzOFdXRTFEaVRtT2xVRmtBakJVaitjOTFlTmJHc1pQWkdVTG9IcjhEQzhCNmVyN3BQUzFoVUtiV3hhZTQvQW5ROXpDcXMxeEhFcFdqcDBaS2IwUkErRkxZYSt0elZ5NUg1bmpXQmVrNjQrVmVuelU3NlQ4dWNhWklEeVQxVGROMHl6b0x2YURZNHk2VW5JYzc4TkNQUTVVajhlaDloekhBVndFQk1sVWpCU0w0QWF2Zk5kTXR5WFVsdU1ZL2Iyd2RGaUc2dkYvajFTZkxFcmtsc0JRTnllTGdNc1M4TER4dy9aSlkvVTQ1eEJ4VFRlZkozSVBxMVhuT0E1QTQ3aWszSTJqVWZGVjVMYlFUZFRiU1JlOWdLRnp4YzQ5cUU4U3RFSjNDZ1UzSjFIdGFyOFJDMGY2YkplK1QzcFdqOWM4OEQ2Um14a0xIbERxdldoS3FjcnhaSUtpNUNxeXR2UXpGNFc2eGRTbUhFUG5tU1BmeFNFUjNWUGlXbU5lTmswenI5QUZhWmUrQVkwSW9DbERaYXhZR0tCWlplSGxadExpZHNvenVlTVFiUnk3Vi9WUnhqSGFlSjJFY3R6aHVxTE41RVBCWlRhOXZUZGE3YlRZNEszVXAwTXVQS3ptR2NJblNUR3B4VVhGQnZMOWthNHA2SFNCOEh6NllvQmErSm5uK3NIWHRMZnVzaGt3cDlGN25Yc2JBR1VjbzY5ako1Vk9TMDdnVnhYNFNIMEErQmwzUVIrYVBPVTRSaHQwRDRoTUZidkkyTGR3c1Rta1B0VWVpSmVLOEtpNmQwbS8xbW9nZTNDVjI0aUxpd1ZXOHpmM2Qyb2RoME51NWFyWlMxOERaUnlqSitYVWxPUFdSMnFxazNocllCZ0hnM0VITnhaN2dQWTNOa3VtTCtxR1NXRVFnOThvalMraXJxbFRxWUY4ZjR4UmFwRGoyQ3hWcUh6dko0T2Z5bFdQMjNGWWsvdmlnN2hzWmlGOWloVHI2bFNPNWRvY2VkVXl5VUlNMG00WEUwdERzeEVmWTR2Rkd2b2JGa3BxRDVESzhZTlJmLzFBRERUMHVIOU5QYTR4cXIwOXNFSjlSanVHU1EyK24xc24vdXNiNjBweGN2T0E3dU96WFBVNGpjT21hYTZNakhnUGFCeDRrVzUxMVNySEFlbW9YM3FPNDBQSUpMNll5Q1JPVWVzekt4OWpHUmVvU0hsditWdFJ5dkdZbVVTdUROU2RmVWEvaCtBcFMrNWxYcG9jZ0dSTlMyUHE5d2tmUkxZOUE0MlJ5ckZLWUdWZnBJL1JCdklpcHl4OGl4anh2MDA4SDdkV1hBL3lwcTFhbitQMnFoU3gwVlZ4TlNLVCtEK0Zxc2pwblg5THF0UUlpL1FONkJyWHluL3ZXRkRLOFh5c2dNbU91b05NQmJaUGZhb2wzM2c3THkvR3VBV1JQZUY4Z2lweWF4aDcyUEEvR1BvYXYwQU1aS1N3MDdzcy9DN1NQck9KcXNocTdvdXlSdnhQNDIvdHdmL3RmOU0wc0VmVXJkUUlqZUp6alh4bmo0K29ERXZsZGtROFM5a2tSbTFIMExnYi9idDJ2bEY3UER3RmhUcjZ2aXZ4OFVhTTAvV2UzNXNCZjQvejhtVjduNHVLWDNyYnJrU0ZIM3YrTnA3MlJ0bXpVUE1wclZXbkhJZlBublhPT0R5eWpiVGI1OWJUV3ZUcWR4dE5sRVZOalFwc3g1a00xRWRuazNLQm1wZ0QydXBVcnNzMXZuSGhjQ3hvam9HMFVaODcvTVpUVVhVUjQzMyt5bTlleTdncDNWQnU1Nlc3ZnQxcDczTjV6OUxhZXozRUdGVTJqck1NZEtQK3ZRSDE3UTFnclprWE9BN2JRbUlxaDRVRDdYT3R0QTZySFd6UVQ1UVBoeU5KK0c5RUVlcWJuM1VyQmdxanpRVUpMTHFTRTdwbEtxcXRMTVpwY2Ivem5yTlkyaWt0ZXBjWi8vdFdGbUhUNEpaamlERnFUTnp2TXErOHVZdzhRM3dNcitUSjZjZDliTVZ3Zk5YdFIzNzNRZzZsTTlsQTBibHBjMm5uWlRMc2x5WG1FcGYwaEcwL2UwdXY5eUFiKzFMYWQ5Q2NVWnkvdDhyNXFTR0lLMU83WDJubFovOFY0UjRrNDdEZFg3M045NDBZOENzWmgyWjlMK3ZoWEF6bHM0dy9jUyszazBYRWpKa1p4ODkrOUIrRDVmUkl2OG0wQ0sybUhJU25nYlRwVEpTWWRpUFAzV0RhamVCUkptSDd6Nkw3b2UrNDgydzB4aGh6RDRwUC9lbmRJRDZFYkY2bkNrRk50N21iYXlmcnhhd1RITmw5bnhtd1NFdlhVRnZMdkp4Y0JwL09tdFpkMTdUYmROTkowYmp1L0hzN1B5QnJIdWV2eXZ3MVdhUDM3SzNubWNiaEliWWRQOXpIenI5RHgyRXVJbmFlSDltSHJlM2dLZHZUbTR4aUhCTjczc2lyeWNNSEljcUlFZkRYZ0wvNnBaTko0M0dLQm5BT29tRGxCcVN5SFlrYVEvSlZsM3pBcVFFYXg0UVFvb3dZY09zQlN1ZTlwSFFraEJCaURETFBNU0dFMU1yZEFNTjQ0NlNZQkNHRVZBbU5ZMElJVVVUOFNZY0U4anlDS3lBU1FnZzVBTjBxQ0NGRWlSaGpVbnovVVBwejk1SXVyS2hBRmtJSUtSMGF4NFFRb29CRWNLK1VNeXBzSlNlelNhbG5RZ2doTkk0SklVU0ZHT01LbUo5WHJTd3NJWVNRdzlEbm1CQkNCaEpqdkFVWHJsaUlNazBJSVFRTWpXTkNDQm1BK0JtL0I3ZmhpVlFZSllRUUFvWnVGWVFRb3NDZXFubHRkVGZOYWxyL1lzRWVRZ2pCUXVPWUVFS0FpRHZFN2NEMGJpMzBQU2FFRURCMHF5Q0VFQ0NTaXUxYTZSZVkvNWdRUXNEUU9DYUVFRHhhcmhBMGpna2hCQXlOWTBJSXdhT2xIQk5DQ0FGRDQ1Z1FRb0RFR09jaGhNOXNZMElJS1lOZjJFK0VFS0tMQk9FbG8vaEtPVnNGTTFVUVFnZ1lHc2VFRUtLRWNtYUtmYXpZVjRRUWdvVnVGWVFRb29DNFQvd05OSXdUUy9ZVklZUmdZWjVqUWdnWmlKU1BSbGZKMnpaTmM4cStJb1FRTEZTT0NTRmtBS0lZb3czanhJTDlSQWdoZUtnY0UwSklKdUpqL0xkQisyMVRqbU1wS0VJSUlRUUlsV05DQ01ubnhxanRGalNNQ1NIRUJpckhoQkNTU1l3eHBWWTdBYmZmUTlNMHJJeEhDQ0ZHVURrbWhKQU1Zb3dYRm9aeENPR0MvVU1JSVhiUU9DYUVFSjg4R2NaTjA3RHdCeUdFR0VMam1CQkM4a0FxdXQ5cEdCTkN5RGpRT0NhRUVEOXNRZ2kvTjAxelJjT1lFRUxHZ2NZeElZU01UektLUHpSTmM5NDB6UjM3Z3hCQ3h1TVh0ajBoaEdReHRGcmR2WlNEdm11YVpzVXVJSVFRSHpDVkd5R0VaQkJqdkE0aG5QZjRQNWZ5enpWekZoTkNpRk5DQ1A4SG1RRUJyVmkybEQ4QUFBQUFTVVZPUks1Q1lJST0iIGFsdD0iIiB3aWR0aD0iMTIwIiBoZWlnaHQ9IjEyMCIvPgpdXT4KICAgICAgICA8L3hzbDp0ZXh0Pgo8L2Rpdj4KPGRpdiBzdHlsZT0iY2xlYXI6Ym90aDsiLz4KPHRhYmxlIGlkPSJrdW55ZSI+CiAgICAgICAgPHRib2R5PgogICAgICAgICAgICAgICAgPHRyPgogICAgICAgIDx0aD4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UYXJpaDo8L3hzbDp0ZXh0PgogICAgICAgIDwvdGg+CiAgICAgICAgPHRkPgogICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Im4xOkludm9pY2UiPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOklzc3VlRGF0ZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9InN1YnN0cmluZyguLDksMikiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4sNiwyKSIKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLiwxLDQpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgPC90ZD4KPC90cj4KPHRyPgogICAgICAgIDx0aD4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5GYXR1cmEgTm86PC94c2w6dGV4dD4KICAgICAgICA8L3RoPgogICAgICAgIDx0ZD4KICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJuMTpJbnZvaWNlIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpJRCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDwvdGQ+CjwvdHI+Cjx0cj4KICAgICAgICA8dGggc3R5bGU9IndpZHRoOjEwNXB4OyI+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+0JPigJN6ZWxsZdCV0Z90aXJtZSBObzo8L3hzbDp0ZXh0PgogICAgICAgIDwvdGg+CiAgICAgICAgPHRkIHN0eWxlPSJ3aWR0aDoxMTBweDsiPgogICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Im4xOkludm9pY2UiPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOkN1c3RvbWl6YXRpb25JRCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDwvdGQ+CjwvdHI+Cjx0cj4KICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+U2VuYXJ5bzo8L3hzbDp0ZXh0PgogICAgICAgIDwvdGg+CiAgICAgICAgPHRkPgogICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Im4xOkludm9pY2UiPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOlByb2ZpbGVJRCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDwvdGQ+CjwvdHI+Cjx0cj4KICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+RmF0dXJhIFRpcGk6PC94c2w6dGV4dD4KICAgICAgICA8L3RoPgogICAgICAgIDx0ZD4KICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJuMTpJbnZvaWNlIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpJbnZvaWNlVHlwZUNvZGUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICA8L3RkPgo8L3RyPgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6QWNjb3VudGluZ0Nvc3QgIT0nJyI+Cjx0cj4KICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+RmF0dXJhIEFsdCBUaXBpOjwveHNsOnRleHQ+CiAgICAgICAgPC90aD4KICAgICAgICA8dGQ+CiAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0ibjE6SW52b2ljZSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6QWNjb3VudGluZ0Nvc3QiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICA8L3RkPgo8L3RyPgo8L3hzbDppZj4gCjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpBZGRpdGlvbmFsRG9jdW1lbnRSZWZlcmVuY2VbY2JjOkRvY3VtZW50VHlwZSA9ICfQlMKwYWRlIEVkaWxlbiBGYXR1cmEnXSBvciAvL24xOkludm9pY2UvY2FjOkJpbGxpbmdSZWZlcmVuY2UvY2FjOkludm9pY2VEb2N1bWVudFJlZmVyZW5jZVtjYmM6RG9jdW1lbnRUeXBlID0gJ9CUwrBhZGUgRWRpbGVuIEZhdHVyYSddIj4KPHhzbDpjaG9vc2U+CiAgICAgICAgPHhzbDp3aGVuIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6QmlsbGluZ1JlZmVyZW5jZS9jYWM6SW52b2ljZURvY3VtZW50UmVmZXJlbmNlW2NiYzpEb2N1bWVudFR5cGUgPSAn0JTCsGFkZSBFZGlsZW4gRmF0dXJhJ10iPgogICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICA8dGggc3R5bGU9InZlcnRpY2FsLWFsaWduOnRvcDsiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD7QlMKwYWRlIEZhdHVyYSBObzo8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICA8dGQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6QmlsbGluZ1JlZmVyZW5jZS9jYWM6SW52b2ljZURvY3VtZW50UmVmZXJlbmNlW2NiYzpEb2N1bWVudFR5cGUgPSAn0JTCsGFkZSBFZGlsZW4gRmF0dXJhJ10iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJwb3NpdGlvbigpICE9MSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYmM6SUQiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICAgICAgICAgIDwvdHI+CiAgICAgICAgICAgICAgICA8dHI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0aCBzdHlsZT0idmVydGljYWwtYWxpZ246dG9wOyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PtCUwrBhZGUgRmF0dXJhIFRhcmloaTo8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICA8dGQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6QmlsbGluZ1JlZmVyZW5jZS9jYWM6SW52b2ljZURvY3VtZW50UmVmZXJlbmNlW2NiYzpEb2N1bWVudFR5cGUgPSAn0JTCsGFkZSBFZGlsZW4gRmF0dXJhJ10iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJwb3NpdGlvbigpICE9MSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoY2JjOklzc3VlRGF0ZSw5LDIpIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKGNiYzpJc3N1ZURhdGUsNiwyKSIKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLz4tPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9InN1YnN0cmluZyhjYmM6SXNzdWVEYXRlLDEsNCkiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICAgICAgICAgIDwvdHI+CiAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICA8eHNsOndoZW4gdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpBZGRpdGlvbmFsRG9jdW1lbnRSZWZlcmVuY2VbY2JjOkRvY3VtZW50VHlwZSA9ICfQlMKwYWRlIEVkaWxlbiBGYXR1cmEnXSI+CiAgICAgICAgICAgICAgICA8dHI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0aCBzdHlsZT0idmVydGljYWwtYWxpZ246dG9wOyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PtCUwrBhZGUgRmF0dXJhIE5vOjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvdGg+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0ZD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpBZGRpdGlvbmFsRG9jdW1lbnRSZWZlcmVuY2VbY2JjOkRvY3VtZW50VHlwZSA9ICfQlMKwYWRlIEVkaWxlbiBGYXR1cmEnXSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9InBvc2l0aW9uKCkgIT0xIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNiYzpJRCIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgICAgICAgICAgPC90cj4KICAgICAgICAgICAgICAgIDx0cj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRoIHN0eWxlPSJ2ZXJ0aWNhbC1hbGlnbjp0b3A7Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+0JTCsGFkZSBGYXR1cmEgVGFyaWhpOjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvdGg+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0ZD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpBZGRpdGlvbmFsRG9jdW1lbnRSZWZlcmVuY2VbY2JjOkRvY3VtZW50VHlwZSA9ICfQlMKwYWRlIEVkaWxlbiBGYXR1cmEnXSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9InBvc2l0aW9uKCkgIT0xIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9InN1YnN0cmluZyhjYmM6SXNzdWVEYXRlLDksMikiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoY2JjOklzc3VlRGF0ZSw2LDIpIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKGNiYzpJc3N1ZURhdGUsMSw0KSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgICAgICAgICAgPC90cj4KICAgICAgICA8L3hzbDp3aGVuPgo8L3hzbDpjaG9vc2U+CjwveHNsOmlmPiAKPHhzbDpmb3ItZWFjaAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSJuMTpJbnZvaWNlL2NhYzpEZXNwYXRjaERvY3VtZW50UmVmZXJlbmNlIj4KICAgICAgICA8dHI+CiAgICAgICAgICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD7QlMKwcnNhbGl5ZSBObzo8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgPC90aD4KICAgICAgICAgICAgICAgIDx0ZD4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpjYWxsLXRlbXBsYXRlIG5hbWU9InJlbW92ZUxlYWRpbmdaZXJvcyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aXRoLXBhcmFtIG5hbWU9Im9yaWdpbmFsU3RyaW5nIiBzZWxlY3Q9ImNiYzpJRCIvPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpjYWxsLXRlbXBsYXRlPgogICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICA8L3RyPgogICAgICAgIDx0cj4KICAgICAgICAgICAgICAgIDx0aD4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PtCUwrByc2FsaXllIFRhcmloaTo8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgPC90aD4KICAgICAgICAgICAgICAgIDx0ZD4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpJc3N1ZURhdGUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLiw5LDIpIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4sNiwyKSIKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLz4tPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9InN1YnN0cmluZyguLDEsNCkiLz4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgIDwvdHI+CjwveHNsOmZvci1lYWNoPgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6T3JkZXJSZWZlcmVuY2UiPgo8dHI+CiAgICAgICAgPHRoPgogICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlNpcGFyadCV0Z8gTm86PC94c2w6dGV4dD4KICAgICAgICA8L3RoPgogICAgICAgIDx0ZD4KICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2gKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSJuMTpJbnZvaWNlL2NhYzpPcmRlclJlZmVyZW5jZSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6SUQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICA8L3RkPgo8L3RyPgo8dHI+CiAgICAgICAgPHRoPgogICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlNpcGFyadCV0Z8gVGFyaWhpOjwveHNsOnRleHQ+CiAgICAgICAgPC90aD4KICAgICAgICA8dGQ+CiAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0ibjE6SW52b2ljZS9jYWM6T3JkZXJSZWZlcmVuY2UiPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOklzc3VlRGF0ZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9InN1YnN0cmluZyguLDksMikiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLiw2LDIpIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4sMSw0KSIvPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDwvdGQ+CjwvdHI+CjwveHNsOmlmPiAKPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOlBheW1lbnRNZWFucy9jYmM6UGF5bWVudER1ZURhdGUgb3IgLy9uMTpJbnZvaWNlL2NhYzpQYXltZW50VGVybXMvY2JjOlBheW1lbnREdWVEYXRlIj4KPHhzbDpjaG9vc2U+CiAgICAgICAgPHhzbDp3aGVuIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6UGF5bWVudFRlcm1zL2NiYzpQYXltZW50RHVlRGF0ZSI+CiAgICAgICAgICAgICAgICA8dHI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+U29uINCT4oCTZGVtZSBUYXJpaGk6PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90aD4KICAgICAgICAgICAgICAgICAgICAgICAgPHRkPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJuMTpJbnZvaWNlL2NhYzpQYXltZW50VGVybXMiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpQYXltZW50RHVlRGF0ZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLiw5LDIpIi8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLiw2LDIpIi8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLiwxLDQpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICAgICAgICAgIDwvdHI+CiAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICA8eHNsOm90aGVyd2lzZT4KICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpQYXltZW50TWVhbnMvY2JjOlBheW1lbnREdWVEYXRlIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5Tb24g0JPigJNkZW1lIFRhcmloaTo8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdGg+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRkPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Im4xOkludm9pY2UvY2FjOlBheW1lbnRNZWFucyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6UGF5bWVudER1ZURhdGUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLiw5LDIpIi8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLiw2LDIpIi8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLiwxLDQpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgICAgICAgICAgICAgICAgICA8L3RyPgogICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgPC94c2w6b3RoZXJ3aXNlPgo8L3hzbDpjaG9vc2U+CjwveHNsOmlmPiAKPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOklzc3VlVGltZSI+Cjx0cj4KICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+T2x10JXRn21hIFphbWFu0JTCsTo8L3hzbDp0ZXh0PgogICAgICAgIDwvdGg+CiAgICAgICAgPHRkPgogICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYmM6SXNzdWVUaW1lIi8+CiAgICAgICAgPC90ZD4KPC90cj4KPC94c2w6aWY+IAoKICAgICAgICA8L3Rib2R5Pgo8L3RhYmxlPgoKCiAgICAgICAgPC9kaXY+CiAgICAgICAgCjwvZGl2PgoKCjwvZGl2PgoKPGRpdiBjbGFzcz0ic2F0aXJsYXIiPgogICAgICAgIAogICAgICAgIAogICAgICAgIDx0YWJsZSBpZD0ibWFsSGl6bWV0VGFibG9zdSI+CiAgICAgICAgICAgICAgICA8dGJvZHk+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0cj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGggZGF0YS1pZD0iU0FUSVJMQVJfU0lSQU5PIj4KICAgICAgICA8eHNsOnRleHQ+U9CUwrFyYSBObzwveHNsOnRleHQ+CjwvdGg+Cjx0aCBkYXRhLWlkPSJTQVRJUkxBUl9NQUxISVpNRVQiIGNsYXNzPSJhbGlnbkxlZnQiPgogICAgICAgIDx4c2w6dGV4dD5NYWwgSGl6bWV0PC94c2w6dGV4dD4KPC90aD4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkludm9pY2VMaW5lL2NhYzpJdGVtL2NiYzpEZXNjcmlwdGlvbiI+Cjx0aCBkYXRhLWlkPSJTQVRJUkxBUl9BQ0lLTEFNQSI+CiAgICAgICAgPHhzbDp0ZXh0PkHQk8Kn0JTCsWtsYW1hPC94c2w6dGV4dD4KPC90aD4KPC94c2w6aWY+IAo8dGggZGF0YS1pZD0iU0FUSVJMQVJfTUlLVEFSIj4KICAgICAgICA8eHNsOnRleHQ+TWlrdGFyPC94c2w6dGV4dD4KPC90aD4KPHRoIGRhdGEtaWQ9IlNBVElSTEFSX0JJUklNRklZQVQiPgogICAgICAgIDx4c2w6dGV4dD5CaXJpbSBGaXlhdDwveHNsOnRleHQ+CjwvdGg+Cjx0aCBkYXRhLWlkPSJTQVRJUkxBUl9NSFRVVEFSSSIgY2xhc3M9Im1oQ29sdW1uIj4KICAgICAgICA8eHNsOnRleHQ+TWFsIEhpem1ldCBUdXRhctCUwrE8L3hzbDp0ZXh0Pgo8L3RoPgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6SW52b2ljZUxpbmUvY2FjOlRheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbC9jYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZS9jYmM6VGF4VHlwZUNvZGUgPScwMDE1JyI+Cjx0aCBkYXRhLWlkPSJTQVRJUkxBUl9LRFZPUkFOSSI+CiAgICAgICAgPHhzbDp0ZXh0PktEViBPcmFu0JTCsTwveHNsOnRleHQ+CjwvdGg+CjwveHNsOmlmPiAKPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkludm9pY2VMaW5lL2NhYzpUYXhUb3RhbC9jYWM6VGF4U3VidG90YWwvY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlID0nMDAxNSciPgo8dGggZGF0YS1pZD0iU0FUSVJMQVJfS0RWVFVUQVJJIj4KCTx4c2w6dGV4dD5LRFYgVHV0YXLQlMKxPC94c2w6dGV4dD4KPC90aD4KPC94c2w6aWY+IAo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6UHJvZmlsZUlEPSdJSFJBQ0FUJyI+Cgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6SW52b2ljZUxpbmUvY2FjOkRlbGl2ZXJ5L2NhYzpEZWxpdmVyeVRlcm1zL2NiYzpJRFtAc2NoZW1lSUQ9J0lOQ09URVJNUyddIj4KCQk8dGggZGF0YS1pZD0iU0FUSVJMQVJfSUhSQUNBVCI+CgkJCQk8c3Bhbj4KCQkJCQkJPHhzbDp0ZXh0PlRlc2xpbSDQldGbYXJ00JTCsTwveHNsOnRleHQ+CgkJCQk8L3NwYW4+CgkJPC90aD4KPC94c2w6aWY+Cjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpJbnZvaWNlTGluZS9jYWM6RGVsaXZlcnkvY2FjOlNoaXBtZW50L2NhYzpUcmFuc3BvcnRIYW5kbGluZ1VuaXQvY2FjOkFjdHVhbFBhY2thZ2UvY2JjOlBhY2thZ2luZ1R5cGVDb2RlIj4KCQk8dGg+CgkJCQk8c3Bhbj4KCQkJCQkJPHhzbDp0ZXh0PkXQldGfeWEgS2FwIENpbnNpPC94c2w6dGV4dD4KCQkJCTwvc3Bhbj4KCQk8L3RoPgo8L3hzbDppZj4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkludm9pY2VMaW5lL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOlRyYW5zcG9ydEhhbmRsaW5nVW5pdC9jYWM6QWN0dWFsUGFja2FnZS9jYmM6SUQiPgoJCTx0aD4KCQkJCTxzcGFuPgoJCQkJCQk8eHNsOnRleHQ+S2FwIE5vPC94c2w6dGV4dD4KCQkJCTwvc3Bhbj4KCQk8L3RoPgo8L3hzbDppZj4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkludm9pY2VMaW5lL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOlRyYW5zcG9ydEhhbmRsaW5nVW5pdC9jYWM6QWN0dWFsUGFja2FnZS9jYmM6UXVhbnRpdHkiPgoJCTx0aD4KCQkJCTxzcGFuPgoJCQkJCQk8eHNsOnRleHQ+S2FwIEFkZXQ8L3hzbDp0ZXh0PgoJCQkJPC9zcGFuPgoJCTwvdGg+CjwveHNsOmlmPgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6SW52b2ljZUxpbmUvY2FjOkRlbGl2ZXJ5L2NhYzpEZWxpdmVyeUFkZHJlc3MiPgoJCTx0aD4KCQkJCTxzcGFuPgoJCQkJCQk8eHNsOnRleHQ+VGVzbGltL0JlZGVsINCT4oCTZGVtZSBZZXJpPC94c2w6dGV4dD4KCQkJCTwvc3Bhbj4KCQk8L3RoPgo8L3hzbDppZj4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkludm9pY2VMaW5lL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOlNoaXBtZW50U3RhZ2UvY2JjOlRyYW5zcG9ydE1vZGVDb2RlIj4KCQk8dGg+CgkJCQk8c3Bhbj4KCQkJCQkJPHhzbDp0ZXh0PkfQk8K2bmRlcmlsbWUg0JXRm2VrbGk8L3hzbDp0ZXh0PgoJCQkJPC9zcGFuPgoJCTwvdGg+CjwveHNsOmlmPgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6SW52b2ljZUxpbmUvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYWM6R29vZHNJdGVtL2NiYzpSZXF1aXJlZEN1c3RvbXNJRCI+CgkJPHRoPgoJCQkJPHNwYW4+CgkJCQkJCTx4c2w6dGV4dD5HVNCUwrBQPC94c2w6dGV4dD4KCQkJCTwvc3Bhbj4KCQk8L3RoPgo8L3hzbDppZj4KPC94c2w6aWY+IAoKICAgICAgICAgICAgICAgICAgICAgICAgPC90cj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6SW52b2ljZUxpbmUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzIHNlbGVjdD0iLiIvPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgIDwvdGJvZHk+CiAgICAgICAgPC90YWJsZT4KPC9kaXY+CgoKPC94c2w6Zm9yLWVhY2g+Cgo8ZGl2IGlkPSJ0b3BsYW1sYXJDb250YWluZXIiPgogICAgICAgIAogICAgICAgIDxkaXYgY2xhc3M9InRvcGxhbWxhciI+CiAgICAgICAgCiAgICAgICAgPGRpdiBjbGFzcz0idG9wbGFtVGFibG8iPgoKPHRhYmxlIGlkPSJ0b3BsYW1sYXIiPgoJPHRib2R5PgoJCQkJPHRyPgoJCQk8dGg+CgkJCQk8eHNsOnRleHQ+TWFsIEhpem1ldCBUb3BsYW0gVHV0YXLQlMKxOjwveHNsOnRleHQ+CgkJCTwvdGg+CgkJCTx0ZD4KCQkJCTx4c2w6dmFsdWUtb2YKCQkJCQkJCQkJc2VsZWN0PSJmb3JtYXQtbnVtYmVyKC8vbjE6SW52b2ljZS9jYWM6TGVnYWxNb25ldGFyeVRvdGFsL2NiYzpMaW5lRXh0ZW5zaW9uQW1vdW50LCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+CgkJCQk8eHNsOmlmCgkJCQkJCQkJCXRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6TGVnYWxNb25ldGFyeVRvdGFsL2NiYzpMaW5lRXh0ZW5zaW9uQW1vdW50L0BjdXJyZW5jeUlEIj4KCQkJCQk8eHNsOnRleHQ+IDwveHNsOnRleHQ+CgkJCQkJPHNwYW4+CgkJCQkJCTx4c2w6aWYKCQkJCQkJCQkJCXRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgoJCQkJCQkJPHhzbDp0ZXh0PlRMPC94c2w6dGV4dD4KCQkJCQkJPC94c2w6aWY+CgkJCQkJCTx4c2w6aWYKCQkJCQkJCQkJCXRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgoJCQkJCQkJPHhzbDp2YWx1ZS1vZgoJCQkJCQkJCQkJCXNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpMZWdhbE1vbmV0YXJ5VG90YWwvY2JjOkxpbmVFeHRlbnNpb25BbW91bnQvQGN1cnJlbmN5SUQiCgkJCQkJCQkJCQkvPgoJCQkJCQk8L3hzbDppZj4KCQkJCQk8L3NwYW4+CgkJCQk8L3hzbDppZj4KCQkJPC90ZD4KCQk8L3RyPgoJCTx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpQcm9maWxlSUQ9J0lIUkFDQVQnIj4KCQk8dHI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0Pk5ha2xpeWU6PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0ZD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmNob29zZT4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOkdvb2RzSXRlbS9jYmM6RGVjbGFyZWRGb3JDYXJyaWFnZVZhbHVlQW1vdW50Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOkdvb2RzSXRlbS9jYmM6RGVjbGFyZWRGb3JDYXJyaWFnZVZhbHVlQW1vdW50LCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYWM6R29vZHNJdGVtL2NiYzpEZWNsYXJlZEZvckNhcnJpYWdlVmFsdWVBbW91bnQvQGN1cnJlbmN5SUQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUlknIG9yIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRMPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJZJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOkdvb2RzSXRlbS9jYmM6RGVjbGFyZWRGb3JDYXJyaWFnZVZhbHVlQW1vdW50L0BjdXJyZW5jeUlEIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KCgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KCgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6RGVsaXZlcnkvY2FjOlNoaXBtZW50L2NiYzpEZWNsYXJlZEZvckNhcnJpYWdlVmFsdWVBbW91bnQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iZm9ybWF0LW51bWJlcigvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYmM6RGVjbGFyZWRGb3JDYXJyaWFnZVZhbHVlQW1vdW50LCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYmM6RGVjbGFyZWRGb3JDYXJyaWFnZVZhbHVlQW1vdW50L0BjdXJyZW5jeUlEIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2YKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6RGVsaXZlcnkvY2FjOlNoaXBtZW50L2NiYzpEZWNsYXJlZEZvckNhcnJpYWdlVmFsdWVBbW91bnQvQGN1cnJlbmN5SUQiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgoKCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgoKCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOm90aGVyd2lzZT4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoMCwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2YKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgoKCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpvdGhlcndpc2U+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmNob29zZT4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgoKCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdHI+Cgk8L3hzbDppZj4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOlByb2ZpbGVJRD0nSUhSQUNBVCciPgoJCTx0cj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+U2lnb3J0YTo8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdGg+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRkPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmNob29zZT4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOkdvb2RzSXRlbS9jYmM6SW5zdXJhbmNlVmFsdWVBbW91bnQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iZm9ybWF0LW51bWJlcigvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYWM6R29vZHNJdGVtL2NiYzpJbnN1cmFuY2VWYWx1ZUFtb3VudCwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOkdvb2RzSXRlbS9jYmM6SW5zdXJhbmNlVmFsdWVBbW91bnQvQGN1cnJlbmN5SUQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUlknIG9yIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRMPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJZJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOkdvb2RzSXRlbS9jYmM6SW5zdXJhbmNlVmFsdWVBbW91bnQvQGN1cnJlbmN5SUQiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2JjOkluc3VyYW5jZVZhbHVlQW1vdW50Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2JjOkluc3VyYW5jZVZhbHVlQW1vdW50LCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYmM6SW5zdXJhbmNlVmFsdWVBbW91bnQvQGN1cnJlbmN5SUQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUlknIG9yIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRMPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJZJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2JjOkluc3VyYW5jZVZhbHVlQW1vdW50L0BjdXJyZW5jeUlEIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpvdGhlcndpc2U+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJmb3JtYXQtbnVtYmVyKDAsICcjIyMuIyMwLDAwJywgJ2V1cm9wZWFuJykiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSWScgb3IgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VEw8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUlknIGFuZCAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpvdGhlcndpc2U+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpjaG9vc2U+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KCgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RyPgoJPC94c2w6aWY+CgkJICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpMZWdhbE1vbmV0YXJ5VG90YWwvY2JjOkFsbG93YW5jZVRvdGFsQW1vdW50ICE9MCI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0cj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VG9wbGFtINCUwrBza29udG86IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iZm9ybWF0LW51bWJlcigvL24xOkludm9pY2UvY2FjOkxlZ2FsTW9uZXRhcnlUb3RhbC9jYmM6QWxsb3dhbmNlVG90YWxBbW91bnQsICcjIyMuIyMwLDAwJywgJ2V1cm9wZWFuJykiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpMZWdhbE1vbmV0YXJ5VG90YWwvY2JjOkFsbG93YW5jZVRvdGFsQW1vdW50L0BjdXJyZW5jeUlEIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUlknIG9yIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VEw8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUlknIGFuZCAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2YKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpMZWdhbE1vbmV0YXJ5VG90YWwvY2JjOkFsbG93YW5jZVRvdGFsQW1vdW50L0BjdXJyZW5jeUlEIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdGQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvdHI+CiAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpMZWdhbE1vbmV0YXJ5VG90YWwvY2JjOkNoYXJnZVRvdGFsQW1vdW50ICE9MCI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0cj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VG9wbGFtIEFydNCUwrFy0JTCsW06PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0ZD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2YKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSJmb3JtYXQtbnVtYmVyKC8vbjE6SW52b2ljZS9jYWM6TGVnYWxNb25ldGFyeVRvdGFsL2NiYzpDaGFyZ2VUb3RhbEFtb3VudCwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkxlZ2FsTW9uZXRhcnlUb3RhbC9jYmM6Q2hhcmdlVG90YWxBbW91bnQvQGN1cnJlbmN5SUQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSWScgb3IgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkxlZ2FsTW9uZXRhcnlUb3RhbC9jYmM6Q2hhcmdlVG90YWxBbW91bnQvQGN1cnJlbmN5SUQiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90cj4KICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6VGF4VG90YWwvY2FjOlRheFN1YnRvdGFsW2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZT0nMDAxNSddIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5LRFYgTWF0cmFo0JTCsSA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiglPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYmM6UGVyY2VudCIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0Pik6PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iZm9ybWF0LW51bWJlciguL2NiYzpUYXhhYmxlQW1vdW50Wy4uL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZT0nMDAxNSddLCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRlc3Q9Ii4vY2JjOlRheGFibGVBbW91bnQvQGN1cnJlbmN5SUQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUlknIG9yIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VEw8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJZJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii4vY2JjOlRheGFibGVBbW91bnQvQGN1cnJlbmN5SUQiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90cj4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICA8dGggY2xhc3M9InN1bVRpdGxlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5WZXJnaSBIYXJp0JPCpyBUdXRhcjogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvdGg+CgogICAgICAgICAgICAgICAgICAgICAgICA8dGQgY2xhc3M9InN1bVZhbHVlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iZm9ybWF0LW51bWJlcigvL24xOkludm9pY2UvY2FjOkxlZ2FsTW9uZXRhcnlUb3RhbC9jYmM6VGF4RXhjbHVzaXZlQW1vdW50LCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkxlZ2FsTW9uZXRhcnlUb3RhbC9jYmM6VGF4RXhjbHVzaXZlQW1vdW50Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2YKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvL24xOkludm9pY2UvY2FjOkxlZ2FsTW9uZXRhcnlUb3RhbC9jYmM6VGF4RXhjbHVzaXZlQW1vdW50L0BjdXJyZW5jeUlEIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgICAgICAgICAgPC90cj4KICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJuMTpJbnZvaWNlL2NhYzpUYXhUb3RhbC9jYWM6VGF4U3VidG90YWxbbm90KHN0YXJ0cy13aXRoKC4vY2FjOlRheENhdGVnb3J5L2NiYzpUYXhFeGVtcHRpb25SZWFzb25Db2RlLCc4JykgYW5kIChzdHJpbmctbGVuZ3RoKC4vY2FjOlRheENhdGVnb3J5L2NiYzpUYXhFeGVtcHRpb25SZWFzb25Db2RlKSA9MykpXSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0cj4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRoIGNsYXNzPSJzdW1UaXRsZSBpcy1sb25nLXtzdHJpbmctbGVuZ3RoKGNhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpOYW1lKSA+IDE1fSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+SGVzYXBsYW5hbiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpOYW1lIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+ICglPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYmM6UGVyY2VudCIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PikgPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY2FjOlRheENhdGVnb3J5L2NiYzpUYXhFeGVtcHRpb25SZWFzb25Db2RlID4gMCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgKDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYWM6VGF4Q2F0ZWdvcnkvY2JjOlRheEV4ZW1wdGlvblJlYXNvbkNvZGUiLz4pCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD46IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGQgY2xhc3M9InN1bVZhbHVlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoLi4vLi4vY2JjOlRheEFtb3VudCwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4uLy4uL2NiYzpUYXhBbW91bnQvQGN1cnJlbmN5SUQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSWScgb3IgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRMPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUlknIGFuZCAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4uLy4uL2NiYzpUYXhBbW91bnQvQGN1cnJlbmN5SUQiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgICAgICAgICAgICAgICAgICA8L3RyPgogICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Im4xOkludm9pY2UvY2FjOldpdGhob2xkaW5nVGF4VG90YWwvY2FjOlRheFN1YnRvdGFsW3N0YXJ0cy13aXRoKC4vY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlLCc2JyldIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0aCBjbGFzcz0ic3VtVGl0bGUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRldmtpZmF0YSBUYWJpINCUwrDQldGfbGVtIFR1dGFy0JTCsSAoS0RWKTogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0ZCBjbGFzcz0ic3VtVmFsdWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoc3VtKG4xOkludm9pY2UvY2FjOkludm9pY2VMaW5lL2NhYzpUYXhUb3RhbC9jYWM6VGF4U3VidG90YWwvY2JjOlRheGFibGVBbW91bnRbLi4vLi4vLi4vY2FjOldpdGhob2xkaW5nVGF4VG90YWwvY2FjOlRheFN1YnRvdGFsW3N0YXJ0cy13aXRoKC4vY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlLCc2JyldIGFuZCAuLi9jYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZS9jYmM6VGF4VHlwZUNvZGUgPTAwMTVdKSwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUlknIG9yIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBUTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0ibjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90cj4KCQkJCTwveHNsOmlmPgoJCQkJPHhzbDppZiB0ZXN0PSJuMTpJbnZvaWNlL2NhYzpXaXRoaG9sZGluZ1RheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbFtzdGFydHMtd2l0aCguL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSwnNicpXSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0cj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VGV2a2lmYXRhIFRhYmkg0JTCsNCV0Z9sZW0g0JPRmnplcmluZGVuIEhlcy4gS0RWOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdGg+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRkIGNsYXNzPSJzdW1WYWx1ZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iZm9ybWF0LW51bWJlcihzdW0objE6SW52b2ljZS9jYWM6V2l0aGhvbGRpbmdUYXhUb3RhbC9jYWM6VGF4U3VidG90YWwvY2JjOlRheGFibGVBbW91bnRbc3RhcnRzLXdpdGgoLi4vY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlLCc2JyldKSwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUlknIG9yIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBUTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0ibjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90cj4KCQkJCTwveHNsOmlmPgogICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJuMTpJbnZvaWNlL2NhYzpXaXRoaG9sZGluZ1RheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbFtzdGFydHMtd2l0aCguL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSwnNicpXSI+CgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0ibjE6SW52b2ljZS9jYWM6V2l0aGhvbGRpbmdUYXhUb3RhbC9jYWM6VGF4U3VidG90YWxbc3RhcnRzLXdpdGgoLi9jYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZS9jYmM6VGF4VHlwZUNvZGUsJzYnKV0iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0cj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PktEViBUZXZraWZhdCBUdXRhctCUwrEgKDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSIvPik6CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRkIGNsYXNzPSJzdW1WYWx1ZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJmb3JtYXQtbnVtYmVyKGNiYzpUYXhBbW91bnQsICcjIyMuIyMwLDAwJywgJ2V1cm9wZWFuJykiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSWScgb3IgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gVEw8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJZJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdHI+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Im4xOkludm9pY2UvY2FjOldpdGhob2xkaW5nVGF4VG90YWwvY2FjOlRheFN1YnRvdGFsW3N0YXJ0cy13aXRoKC4vY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlLCc0JyldIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UZXZraWZhdGEgVGFiaSDQlMKw0JXRn2xlbSBUdXRhctCUwrEgKNCT4oCTVFYpPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0ZCBjbGFzcz0ic3VtVmFsdWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoc3VtKG4xOkludm9pY2UvY2FjOkludm9pY2VMaW5lL2NhYzpUYXhUb3RhbC9jYWM6VGF4U3VidG90YWwvY2JjOlRheGFibGVBbW91bnRbLi4vLi4vLi4vY2FjOldpdGhob2xkaW5nVGF4VG90YWwvY2FjOlRheFN1YnRvdGFsW3N0YXJ0cy13aXRoKC4vY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlLCc0JykgYW5kIC4uL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSA9MDA3MV1dKSwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUlknIG9yIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBUTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJuMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgICAgICAgICAgICAgICAgICA8L3RyPgoJCQkJPC94c2w6aWY+CiAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Im4xOkludm9pY2UvY2FjOldpdGhob2xkaW5nVGF4VG90YWwvY2FjOlRheFN1YnRvdGFsW3N0YXJ0cy13aXRoKC4vY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlLCc0JyldIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UZXZraWZhdGEgVGFiaSDQlMKw0JXRn2xlbSDQk9GaemVyaW5kZW4gSGVzLiDQk+KAk1RWPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0ZCBjbGFzcz0ic3VtVmFsdWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoc3VtKG4xOkludm9pY2UvY2FjOldpdGhob2xkaW5nVGF4VG90YWwvY2FjOlRheFN1YnRvdGFsL2NiYzpUYXhhYmxlQW1vdW50W3N0YXJ0cy13aXRoKC4uL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSwnNCcpXSksICcjIyMuIyMwLDAwJywgJ2V1cm9wZWFuJykiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gVEw8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUlknIGFuZCAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0ibjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90cj4KCQkJCTwveHNsOmlmPgogICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJuMTpJbnZvaWNlL2NhYzpXaXRoaG9sZGluZ1RheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbFtzdGFydHMtd2l0aCguL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSwnNCcpXSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJuMTpJbnZvaWNlL2NhYzpXaXRoaG9sZGluZ1RheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbFtzdGFydHMtd2l0aCguL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSwnNCcpXSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+0JPigJNUViBUZXZraWZhdCBUdXRhctCUwrEgKDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSIvPik6CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRkIGNsYXNzPSJzdW1WYWx1ZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuLi9jYmM6VGF4QW1vdW50Ii8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUlknIG9yIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IFRMPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RyPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KCQkJCTwveHNsOmlmPgogICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9InN1bShuMTpJbnZvaWNlL2NhYzpUYXhUb3RhbC9jYWM6VGF4U3VidG90YWxbY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlPTkwMTVdL2NiYzpUYXhhYmxlQW1vdW50KT4wIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UZXZraWZhdGEgVGFiaSDQlMKw0JXRn2xlbSBUdXRhctCUwrE6PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0ZCBjbGFzcz0ic3VtVmFsdWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoc3VtKG4xOkludm9pY2UvY2FjOkludm9pY2VMaW5lW2NhYzpUYXhUb3RhbC9jYWM6VGF4U3VidG90YWwvY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlPTkwMTVdL2NiYzpMaW5lRXh0ZW5zaW9uQW1vdW50KSwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUlknIG9yIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRMPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJZJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Im4xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdGQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvdHI+CgkJCQk8L3hzbDppZj4KICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJzdW0objE6SW52b2ljZS9jYWM6VGF4VG90YWwvY2FjOlRheFN1YnRvdGFsW2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZT05MDE1XS9jYmM6VGF4YWJsZUFtb3VudCk+MCI+CiAgICAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UZXZraWZhdGEgVGFiaSDQlMKw0JXRn2xlbSDQk9GaemVyaW5kZW4gSGVzLiBLRFY6PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0ZCBjbGFzcz0ic3VtVmFsdWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoc3VtKG4xOkludm9pY2UvY2FjOlRheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbFtjYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZS9jYmM6VGF4VHlwZUNvZGU9OTAxNV0vY2JjOlRheGFibGVBbW91bnQpLCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSWScgb3IgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VEw8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUlknIGFuZCAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0ibjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90cj4KCQkJCTwveHNsOmlmPgogICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlZlcmdpbGVyIERhaGlsIFRvcGxhbSBUdXRhcjo8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICA8dGQgY2xhc3M9InN1bVZhbHVlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0ibjE6SW52b2ljZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOkxlZ2FsTW9uZXRhcnlUb3RhbCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6VGF4SW5jbHVzaXZlQW1vdW50Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoLiwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6TGVnYWxNb25ldGFyeVRvdGFsL2NiYzpUYXhJbmNsdXNpdmVBbW91bnQvQGN1cnJlbmN5SUQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUlknIG9yIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRMPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJZJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6TGVnYWxNb25ldGFyeVRvdGFsL2NiYzpUYXhJbmNsdXNpdmVBbW91bnQvQGN1cnJlbmN5SUQiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgICAgICAgICAgPC90cj4KICAgICAgICAgICAgICAgIDx0ciBjbGFzcz0icGF5YWJsZUFtb3VudCI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+0JPigJNkZW5lY2VrIFR1dGFyOjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvdGg+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0ZD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0ibjE6SW52b2ljZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOkxlZ2FsTW9uZXRhcnlUb3RhbCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6UGF5YWJsZUFtb3VudCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSJmb3JtYXQtbnVtYmVyKC4sICcjIyMuIyMwLDAwJywgJ2V1cm9wZWFuJykiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkxlZ2FsTW9uZXRhcnlUb3RhbC9jYmM6UGF5YWJsZUFtb3VudC9AY3VycmVuY3lJRCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSWScgb3IgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VEw8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUlknIGFuZCAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpMZWdhbE1vbmV0YXJ5VG90YWwvY2JjOlBheWFibGVBbW91bnQvQGN1cnJlbmN5SUQiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgICAgICAgICAgPC90cj4KCgk8L3Rib2R5Pgo8L3RhYmxlPgo8L2Rpdj4KCgo8L2Rpdj4KCgo8L2Rpdj4KCjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpQcm9maWxlSUQgIT0gJ1lPTENVQkVSQUJFUkZBVFVSQScgYW5kIC8vbjE6SW52b2ljZS9jYmM6UHJvZmlsZUlEID0gJ0lIUkFDQVQnIj4KICAgICAgICA8ZGl2IGlkPSJpaHJhY2F0QmlsZ2lsZXJpIj4KCiAgICAgICAgICAgICAgICA8dGFibGUgd2lkdGg9IjEwMCUiIGJvcmRlcj0iMCI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0Ym9keT4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dHI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGggYWxpZ249ImxlZnQiPkXQldGfeWEgQmlsZ2lsZXJpPC90aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0aCBhbGlnbj0ibGVmdCI+QWRyZXMgQmlsZ2lsZXJpPC90aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0cj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0ZCB2YWxpZ249InRvcCIgd2lkdGg9IjUwJSI+CgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRlc2xpbSDQldGbYXJ00JTCsTogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6RGVsaXZlcnkvY2FjOkRlbGl2ZXJ5VGVybXMvY2JjOklEW0BzY2hlbWVJRD0nSU5DT1RFUk1TJ10iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIgLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkfQk8K2bmRlcmlsbWUg0JXRm2VrbGk6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYWM6U2hpcG1lbnRTdGFnZS9jYmM6VHJhbnNwb3J0TW9kZUNvZGUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Y2FsbC10ZW1wbGF0ZSBuYW1lPSJUcmFuc3BvcnRNb2RlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2l0aC1wYXJhbSBuYW1lPSJUcmFuc3BvcnRNb2RlVHlwZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aXRoLXBhcmFtPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmNhbGwtdGVtcGxhdGU+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIgLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkdU0JTCsFAgTm86IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYWM6R29vZHNJdGVtL2NiYzpSZXF1aXJlZEN1c3RvbXNJRCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4iLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxiciAvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+RdCV0Z95YSBLYXAgQ2luc2k6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYWM6VHJhbnNwb3J0SGFuZGxpbmdVbml0L2NhYzpBY3R1YWxQYWNrYWdlL2NiYzpQYWNrYWdpbmdUeXBlQ29kZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpjYWxsLXRlbXBsYXRlIG5hbWU9IlBhY2thZ2luZyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndpdGgtcGFyYW0gbmFtZT0iUGFja2FnaW5nVHlwZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aXRoLXBhcmFtPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmNhbGwtdGVtcGxhdGU+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGQgdmFpZ249InRvcCIgd2lkdGg9IjUwJSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD7Qk9GabGtlOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6RGVsaXZlcnlBZGRyZXNzL2NhYzpDb3VudHJ5L2NiYzpOYW1lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLiIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyIC8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD7QldGbZWhpcjogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6RGVsaXZlcnkvY2FjOkRlbGl2ZXJ5QWRkcmVzcy9jYmM6Q2l0eU5hbWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIgLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PtCUwrBs0JPCp2U6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpEZWxpdmVyeUFkZHJlc3MvY2JjOkNpdHlTdWJkaXZpc2lvbk5hbWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIgLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlBvc3RhIEtvZHU6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpEZWxpdmVyeUFkZHJlc3MvY2JjOlBvc3RhbFpvbmUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIgLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlNva2FrOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6RGVsaXZlcnlBZGRyZXNzL2NiYzpTdHJlZXROYW1lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLiIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyIC8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5ObzogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6RGVsaXZlcnkvY2FjOkRlbGl2ZXJ5QWRkcmVzcy9jYmM6QnVpbGRpbmdOdW1iZXIiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIgLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkJpbmEgQWTQlMKxOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6RGVsaXZlcnlBZGRyZXNzL2NiYzpCdWlsZGluZ05hbWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIgLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdGQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90cj4KICAgICAgICAgICAgICAgICAgICAgICAgPC90Ym9keT4KICAgICAgICAgICAgICAgIDwvdGFibGU+CgogICAgICAgIDwvZGl2Pgo8L3hzbDppZj4KPGRpdiBpZD0ibm90bGFyIj4KICAgICAgICAKICAgICAgICA8dGFibGU+CiAgICAgICAgICAgICAgICA8dGJvZHk+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0cj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NiYzpOb3RlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIxPTEiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Y2FsbC10ZW1wbGF0ZSBuYW1lPSJyZXBOTCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndpdGgtcGFyYW0gbmFtZT0icFRleHQiIHNlbGVjdD0iLiIgLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpjYWxsLXRlbXBsYXRlPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJuMTpJbnZvaWNlL2NhYzpUYXhUb3RhbC9jYWM6VGF4U3VidG90YWxbc3RhcnRzLXdpdGgoLi9jYWM6VGF4Q2F0ZWdvcnkvY2JjOlRheEV4ZW1wdGlvblJlYXNvbkNvZGUsJzgnKSBhbmQgKHN0cmluZy1sZW5ndGgoLi9jYWM6VGF4Q2F0ZWdvcnkvY2JjOlRheEV4ZW1wdGlvblJlYXNvbkNvZGUpID0zKV0iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsgIj7Qk+KAk3plbCBNYXRyYWggS29kdTogPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2FjOlRheENhdGVnb3J5L2NiYzpUYXhFeGVtcHRpb25SZWFzb25Db2RlIi8+IC0gPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNhYzpUYXhDYXRlZ29yeS9jYmM6VGF4RXhlbXB0aW9uUmVhc29uIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuIHN0eWxlPSJmb250LXdlaWdodDpib2xkOyAiPtCT4oCTemVsIE1hdHJhaCBEZXRhedCUwrE6IDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpOYW1lIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAoPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSIvPikKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBPcmFuOiAlPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNiYzpQZXJjZW50Ii8+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgVmVyZ2kgVHV0YXLQlMKxOgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iZm9ybWF0LW51bWJlcihjYmM6VGF4QW1vdW50LCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjYmM6VGF4QW1vdW50L0BjdXJyZW5jeUlEID0gJ1RSWScgb3IgY2JjOlRheEFtb3VudC9AY3VycmVuY3lJRCA9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VEw8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjYmM6VGF4QW1vdW50L0BjdXJyZW5jeUlEICE9ICdUUlknIGFuZCBjYmM6VGF4QW1vdW50L0BjdXJyZW5jeUlEICE9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2JjOlRheEFtb3VudC9AY3VycmVuY3lJRCIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgVmVyZ2kgTWF0cmFo0JTCsToKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoY2JjOlRheGFibGVBbW91bnQsICcjIyMuIyMwLDAwJywgJ2V1cm9wZWFuJykiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNiYzpUYXhhYmxlQW1vdW50L0BjdXJyZW5jeUlEID0gJ1RSWScgb3IgY2JjOlRheGFibGVBbW91bnQvQGN1cnJlbmN5SUQgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRMPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY2JjOlRheGFibGVBbW91bnQvQGN1cnJlbmN5SUQgIT0gJ1RSWScgYW5kIGNiYzpUYXhhYmxlQW1vdW50L0BjdXJyZW5jeUlEICE9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2JjOlRheGFibGVBbW91bnQvQGN1cnJlbmN5SUQiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Im4xOkludm9pY2UvY2FjOldpdGhob2xkaW5nVGF4VG90YWwvY2FjOlRheFN1YnRvdGFsW3N0YXJ0cy13aXRoKC4vY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlLCc0Jykgb3Igc3RhcnRzLXdpdGgoLi9jYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZS9jYmM6VGF4VHlwZUNvZGUsJzYnKV0iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZGlzcGxheTppbmxpbmUtYmxvY2s7Zm9udC13ZWlnaHQ6Ym9sZDsgIHZlcnRpY2FsLWFsaWduOiB0b3A7cGFkZGluZy1yaWdodDogNHB4OyI+VEVWS9CUwrBGQVQgREVUQVlJOiA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuIHN0eWxlPSJkaXNwbGF5OmlubGluZS1ibG9jazsiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJuMTpJbnZvaWNlL2NhYzpXaXRoaG9sZGluZ1RheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbFtzdGFydHMtd2l0aCguL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSwnNCcpIG9yIHN0YXJ0cy13aXRoKC4vY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlLCc2JyldIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSIvPiAtIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpOYW1lIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxkaXYgc3R5bGU9ImNsZWFyOmJvdGgiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgoKCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNvdW50KC8vbjE6SW52b2ljZS9jYWM6QWRkaXRpb25hbERvY3VtZW50UmVmZXJlbmNlL2NhYzpBdHRhY2htZW50L2NiYzpFbWJlZGRlZERvY3VtZW50QmluYXJ5T2JqZWN0KSAmZ3Q7IDEiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsiPtCUwrBMQVZFIETQk+KAk0vQk9GaTUFOTEFSPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6QWRkaXRpb25hbERvY3VtZW50UmVmZXJlbmNlIGFuZCAvL24xOkludm9pY2UvY2FjOkFkZGl0aW9uYWxEb2N1bWVudFJlZmVyZW5jZS9jYmM6RG9jdW1lbnRUeXBlIT0nWFNMVCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpBZGRpdGlvbmFsRG9jdW1lbnRSZWZlcmVuY2UiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLi9jYmM6RG9jdW1lbnRUeXBlIT0nWFNMVCcgYW5kIG5vdCguL2NiYzpEb2N1bWVudFR5cGVDb2RlKSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4vY2JjOklEIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4gc3R5bGU9ImZvbnQtd2VpZ2h0OmJvbGQ7Ij4gQmVsZ2UgTm86IDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii4vY2JjOklEIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLi9jYmM6SXNzdWVEYXRlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4gc3R5bGU9ImZvbnQtd2VpZ2h0OmJvbGQ7Ij4gQmVsZ2UgVGFyaWhpOiA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLi9jYmM6SXNzdWVEYXRlLDksMikiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLi9jYmM6SXNzdWVEYXRlLDYsMikiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLi9jYmM6SXNzdWVEYXRlLDEsNCkiLz4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLi9jYmM6RG9jdW1lbnRUeXBlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4gc3R5bGU9ImZvbnQtd2VpZ2h0OmJvbGQ7Ij4gQmVsZ2UgVGlwaTogPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iLi9jYmM6RG9jdW1lbnRUeXBlIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4vY2FjOkF0dGFjaG1lbnQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsiPiBCZWxnZSBBZNCUwrE6IDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuL2NhYzpBdHRhY2htZW50L2NiYzpFbWJlZGRlZERvY3VtZW50QmluYXJ5T2JqZWN0Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iLi9jYWM6QXR0YWNobWVudC9jYmM6RW1iZWRkZWREb2N1bWVudEJpbmFyeU9iamVjdC9AZmlsZW5hbWUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgoKCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6UGF5bWVudE1lYW5zIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4gc3R5bGU9ImZvbnQtd2VpZ2h0OmJvbGQ7Ij7Qk+KAk0RFTUUg0JXRm0VLTNCUwrA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6UGF5bWVudE1lYW5zIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4vY2JjOlBheW1lbnRNZWFuc0NvZGUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4gc3R5bGU9ImZvbnQtd2VpZ2h0OmJvbGQ7Ij7Qk+KAk2RlbWUg0JXRm2VrbGk6IDwvc3Bhbj4KCgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpjaG9vc2U+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIuL2NiYzpQYXltZW50TWVhbnNDb2RlICA9ICdaWlonIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5EadCU0Z9lcjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iLi9jYmM6UGF5bWVudE1lYW5zQ29kZSAgPSAnMjAnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD7Qk+KAoWVrPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIuL2NiYzpQYXltZW50TWVhbnNDb2RlICA9ICc0MiciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkhhdmFsZTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9Ii4vY2JjOlBheW1lbnRNZWFuc0NvZGUgID0gJzYnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5LcmVkaTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9Ii4vY2JjOlBheW1lbnRNZWFuc0NvZGUgID0gJzQ4JyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+S3JlZGkgS2FydNCUwrE8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9Ii4vY2JjOlBheW1lbnRNZWFuc0NvZGUgID0gJzEwJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+TmFraXQ8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9Ii4vY2JjOlBheW1lbnRNZWFuc0NvZGUgID0gJzQ5JyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+T3RvbWF0aWsg0JPigJNkZW1lPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIuL2NiYzpQYXltZW50TWVhbnNDb2RlICA9ICc2MCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlNlbmV0PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIuL2NiYzpQYXltZW50TWVhbnNDb2RlICA9ICcxJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+U9CTwrZ6bGXQldGfbWUgS2Fwc2Ft0JTCsW5kYTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmNob29zZT4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgJiMxNjA7JiMxNjA7JiMxNjA7CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY2JjOlBheW1lbnREdWVEYXRlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuIHN0eWxlPSJmb250LXdlaWdodDpib2xkOyI+U29uINCT4oCTZGVtZSBUYXJpaGk6IDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoY2JjOlBheW1lbnREdWVEYXRlLDksMikiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoY2JjOlBheW1lbnREdWVEYXRlLDYsMikiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoY2JjOlBheW1lbnREdWVEYXRlLDEsNCkiLz4mIzE2MDsmIzE2MDsmIzE2MDsKCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjYmM6UGF5bWVudENoYW5uZWxDb2RlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuIHN0eWxlPSJmb250LXdlaWdodDpib2xkOyI+0JPigJNkZW1lIEthbmFs0JTCsTogPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9ImNiYzpQYXltZW50Q2hhbm5lbENvZGUiLz4mIzE2MDsmIzE2MDsmIzE2MDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjYWM6UGF5ZWVGaW5hbmNpYWxBY2NvdW50L2NiYzpJRCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsiPiBJQkFOIC8gSGVzYXAgTm86IDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYWM6UGF5ZWVGaW5hbmNpYWxBY2NvdW50L2NiYzpJRCIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgKDx4c2w6aWYgdGVzdD0iY2FjOlBheWVlRmluYW5jaWFsQWNjb3VudC9jYmM6Q3VycmVuY3lDb2RlID0gJ1RSWScgb3IgY2FjOlBheWVlRmluYW5jaWFsQWNjb3VudC9jYmM6Q3VycmVuY3lDb2RlID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VEw8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNhYzpQYXllZUZpbmFuY2lhbEFjY291bnQvY2JjOkN1cnJlbmN5Q29kZSAhPSAnVFJZJyBhbmQgY2FjOlBheWVlRmluYW5jaWFsQWNjb3VudC9jYmM6Q3VycmVuY3lDb2RlICE9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNhYzpQYXllZUZpbmFuY2lhbEFjY291bnQvY2JjOkN1cnJlbmN5Q29kZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+KQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNiYzpJbnN0cnVjdGlvbk5vdGUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuIHN0eWxlPSJmb250LXdlaWdodDpib2xkOyI+0JPigJNkZW1lINCV0Ztla2xpIEHQk8Kn0JTCsWtsYW1hc9CUwrE6PC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9ImNiYzpJbnN0cnVjdGlvbk5vdGUiLz4mIzE2MDsmIzE2MDsmIzE2MDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KCgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KCgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkludm9pY2VQZXJpb2QiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsiPkZBVFVSQSBE0JPigJNORU0gQtCUwrBMR9CUwrBMRVLQlMKwPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgoKCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkludm9pY2VQZXJpb2QiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLi9jYmM6U3RhcnREYXRlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuIHN0eWxlPSJmb250LXdlaWdodDpib2xkOyI+QmHQldGfbGFuZ9CUwrHQk8KnIFRhcmloaTo8L3NwYW4+JiMxNjA7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4vY2JjOlN0YXJ0RGF0ZSw5LDIpIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4vY2JjOlN0YXJ0RGF0ZSw2LDIpIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4vY2JjOlN0YXJ0RGF0ZSwxLDQpIi8+JiMxNjA7JiMxNjA7JiMxNjA7CgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4vY2JjOkVuZERhdGUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4gc3R5bGU9ImZvbnQtd2VpZ2h0OmJvbGQ7Ij5CaXRp0JXRnyBUYXJpaGk6PC9zcGFuPiYjMTYwOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9InN1YnN0cmluZyguL2NiYzpFbmREYXRlLDksMikiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLi9jYmM6RW5kRGF0ZSw2LDIpIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4vY2JjOkVuZERhdGUsMSw0KSIvPiYjMTYwOyYjMTYwOyYjMTYwOwoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuL2NiYzpEdXJhdGlvbk1lYXN1cmUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4gc3R5bGU9ImZvbnQtd2VpZ2h0OmJvbGQ7Ij5E0JPCtm5lbSBQZXJpeW9kdSAvIFPQk9GYcmVzaTogPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii4vY2JjOkR1cmF0aW9uTWVhc3VyZSIvPiYjMTYwOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpjaG9vc2U+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIuL2NiYzpEdXJhdGlvbk1lYXN1cmUvQHVuaXRDb2RlICA9ICdNT04nIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5BeTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iLi9jYmM6RHVyYXRpb25NZWFzdXJlL0B1bml0Q29kZSAgPSAnREFZJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+R9CT0ZhuPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIuL2NiYzpEdXJhdGlvbk1lYXN1cmUvQHVuaXRDb2RlICA9ICdIVVInIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5TYWF0PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iLi9jYmM6RHVyYXRpb25NZWFzdXJlL0B1bml0Q29kZSAgPSAnQU5OJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+WdCUwrFsPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Y2hvb3NlPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICYjMTYwOwoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuL2NiYzpEZXNjcmlwdGlvbiI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsiPkZhdHVyYSBE0JPCtm5lbSBB0JPCp9CUwrFrbGFtYXPQlMKxOjwvc3Bhbj4mIzE2MDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuL2NiYzpEZXNjcmlwdGlvbiIvPiYjMTYwOyYjMTYwOyYjMTYwOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpSZWNlaXB0RG9jdW1lbnRSZWZlcmVuY2UiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsiPkFMSU5ESSBC0JTCsExH0JTCsExFUtCUwrA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOlJlY2VpcHREb2N1bWVudFJlZmVyZW5jZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuL2NiYzpJRCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsiPkJlbGdlIE51bWFyYXPQlMKxOiA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLi9jYmM6SUQiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4vY2JjOklzc3VlRGF0ZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsiPiBCZWxnZSBUYXJpaGk6IDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLi9jYmM6SXNzdWVEYXRlLDksMikiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLi9jYmM6SXNzdWVEYXRlLDYsMikiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLi9jYmM6SXNzdWVEYXRlLDEsNCkiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4vY2JjOkRvY3VtZW50VHlwZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsiPiBCZWxnZSBUaXBpOiA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLi9jYmM6RG9jdW1lbnRUeXBlIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpQYXltZW50VGVybXMvY2JjOlBlbmFsdHlTdXJjaGFyZ2VQZXJjZW4gb3IgLy9uMTpJbnZvaWNlL2NhYzpQYXltZW50VGVybXMvY2JjOkFtb3VudCBvciAvL24xOkludm9pY2UvY2FjOlBheW1lbnRUZXJtcy9jYmM6Tm90ZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuIHN0eWxlPSJmb250LXdlaWdodDpib2xkOyI+0JPigJNERU1FIEtP0JXRm1VMTEFSSTwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOlBheW1lbnRUZXJtcyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuL2NiYzpQZW5hbHR5U3VyY2hhcmdlUGVyY2VudCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsiPkdlY2lrbWUgQ2V6YSBPcmFu0JTCsTogPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiAlPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2YKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSJmb3JtYXQtbnVtYmVyKC4vY2JjOlBlbmFsdHlTdXJjaGFyZ2VQZXJjZW50LCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+JiMxNjA7JiMxNjA7JiMxNjA7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuL2NiYzpBbW91bnQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4gc3R5bGU9ImZvbnQtd2VpZ2h0OmJvbGQ7Ij5HZWNpa21lIENlemEgVHV0YXLQlMKxOjwvc3Bhbj4mIzE2MDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJmb3JtYXQtbnVtYmVyKC4vY2JjOkFtb3VudCwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4vY2JjOkFtb3VudC9AY3VycmVuY3lJRCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IFRMPC94c2w6dGV4dD4mIzE2MDsmIzE2MDsmIzE2MDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUlknIGFuZCAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4vY2FjOlByaWNlL2NiYzpQcmljZUFtb3VudC9AY3VycmVuY3lJRCIvPiYjMTYwOyYjMTYwOyYjMTYwOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4vY2JjOk5vdGUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4gc3R5bGU9ImZvbnQtd2VpZ2h0OmJvbGQ7Ij5B0JPCp9CUwrFrbGFtYTogPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii4vY2JjOk5vdGUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpUYXhUb3RhbC9jYWM6VGF4U3VidG90YWwiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNiYzpQZXJjZW50PTAgYW5kIGNhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZT0mYXBvczswMDE1JmFwb3M7IGFuZCBub3QoY2FjOlRheENhdGVnb3J5L2NiYzpUYXhFeGVtcHRpb25SZWFzb25Db2RlID4gMCkiPiAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGI+VmVyZ2kg0JTCsHN0aXNuYSBNdWFmaXlldCBTZWJlYmk6IDwvYj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2FjOlRheENhdGVnb3J5L2NiYzpUYXhFeGVtcHRpb25SZWFzb24iLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6VGF4VG90YWwvY2FjOlRheFN1YnRvdGFsW25vdCguL2NhYzpUYXhDYXRlZ29yeS9jYmM6VGF4RXhlbXB0aW9uUmVhc29uQ29kZT1wcmVjZWRpbmc6OiopXSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY2FjOlRheENhdGVnb3J5L2NiYzpUYXhFeGVtcHRpb25SZWFzb25Db2RlID4gMCBhbmQgbm90KHN0YXJ0cy13aXRoKC4vY2FjOlRheENhdGVnb3J5L2NiYzpUYXhFeGVtcHRpb25SZWFzb25Db2RlLCc4JykgYW5kIChzdHJpbmctbGVuZ3RoKC4vY2FjOlRheENhdGVnb3J5L2NiYzpUYXhFeGVtcHRpb25SZWFzb25Db2RlKSA9MykpIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8Yj5WZXJnaSDQlMKwc3Rpc25hIE11YWZpeWV0IFNlYmViaTogPC9iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYWM6VGF4Q2F0ZWdvcnkvY2JjOlRheEV4ZW1wdGlvblJlYXNvbkNvZGUiLz4gLSA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2FjOlRheENhdGVnb3J5L2NiYzpUYXhFeGVtcHRpb25SZWFzb24iLz4gLSA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2FjOlRheENhdGVnb3J5L2NiYzpOYW1lIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdGQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvdHI+CiAgICAgICAgICAgICAgICA8L3Rib2R5PgogICAgICAgIDwvdGFibGU+CjwvZGl2PgoKCiAgICAgICAgICAgICAgICA8L2Rpdj4KICAgICAgICAgICAgICAgIAogICAgICAgIDwvZGl2Pgo8L2JvZHk+Cgo8L2h0bWw+Cgo8L3hzbDp0ZW1wbGF0ZT4KICAgICAgICAKPHhzbDp0ZW1wbGF0ZSBtYXRjaD0iLy9uMTpJbnZvaWNlL2NhYzpJbnZvaWNlTGluZSI+CiAgICAgICAgCiAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgPHRkPgogICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoLi9jYmM6SUQsICcjJykiLz4KICAgICAgICA8L3NwYW4+CjwvdGQ+Cjx0ZCBjbGFzcz0id3JhcCI+CiAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLi9jYWM6SXRlbS9jYmM6TmFtZSIvPgogICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLi9jYWM6SXRlbS9jYmM6QnJhbmROYW1lIi8+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuL2NhYzpJdGVtL2NiYzpNb2RlbE5hbWUiLz4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4vY2FjOkl0ZW0vY2FjOkNvbW1vZGl0eUNsYXNzaWZpY2F0aW9uL2NiYzpJdGVtQ2xhc3NpZmljYXRpb25Db2RlIi8+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuL2NhYzpJdGVtL2NhYzpJdGVtSW5zdGFuY2UvY2JjOlNlcmlhbElEIi8+CiAgICAgICAgPC9zcGFuPgo8L3RkPgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6SW52b2ljZUxpbmUvY2FjOkl0ZW0vY2JjOkRlc2NyaXB0aW9uIj4KPHRkIGNsYXNzPSJ3cmFwIj4KICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuL2NhYzpJdGVtL2NiYzpEZXNjcmlwdGlvbiIvPgogICAgICAgIDwvc3Bhbj4KPC90ZD4KPC94c2w6aWY+IAo8dGQ+CiAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iZm9ybWF0LW51bWJlciguL2NiYzpJbnZvaWNlZFF1YW50aXR5LCAnIy4jIyMuIyMjLCMjIyMjIyMjJywgJ2V1cm9wZWFuJykiLz4KICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLi9jYmM6SW52b2ljZWRRdWFudGl0eS9AdW5pdENvZGUiPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLi9jYmM6SW52b2ljZWRRdWFudGl0eSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Y2hvb3NlPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnMjYnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRvbjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdTRVQnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlNldDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdCWCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+S3V0dTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdMVFInIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkxUPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0hVUiciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+U2FhdDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdOSVUnIG9yIEB1bml0Q29kZSAgPSAnQzYyJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5BZGV0PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0tHTSciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+S0c8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnS0pPJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5rSjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdHUk0nIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0Pkc8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnTUdNJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5NRzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdOVCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+TmV0IFRvbjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdHVCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+R1Q8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnTVRSJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5NPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ01NVCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+TU08L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnS1RNJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5LTTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdNTFQnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0Pk1MPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ01NUSciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+TU0zPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0NMVCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+Q0w8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnQ01LJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5DTTI8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnQ01RJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5DTTM8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnQ01UJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5DTTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdETVQnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkRNPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ01USyciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+TTI8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnTVRRJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5NMzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdEQVknIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBH0JPRmG48L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnTU9OJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gQXk8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnUEEnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBQYWtldDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdLV0gnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBLV0g8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnRDYxJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gRGFraWthPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0Q2MiciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IFNhbml5ZTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdBTk4nIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBZ0JTCsWw8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnQUZGJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gQUbQlMKwRjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdBWVInIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBBbHTQlMKxbiBBeWFy0JTCsTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdCMzInIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBLRy9NZXRyZSBLYXJlPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0NDVCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IFRvbiBCYdCV0Z/QlMKxbmEgVGHQldGf0JTCsW1hIEthcGFzaXRlc2k8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnQ1BSJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gQWRldC3Qk+KAoWlmdDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdEMzAnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBCctCT0Zh0IEthbG9yaSBEZdCU0Z9lcmk8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnRDQwJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gQmluIExpdHJlPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0dGSSciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEZJU1NJTEUg0JTCsHpvdG9wIEdyYW3QlMKxPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0dNUyciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEfQk9GYbdCT0ZjQldGfPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0dSTSciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEdyYW08L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnSDYyJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gWdCT0Zh6IEFkZXQ8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnSzIwJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gS2lsb2dyYW0gUG90YXN5dW0gT2tzaXQ8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnSzU4JyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gS3VydXR1bG110JXRnyBOZXQgQdCU0Z/QlMKxcmzQlMKxa2zQlMKxIEtnLjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdLNjInIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBLaWxvZ3JhbS1BZGV0PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0tGTyciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IERpZm9zZm9yIFBlbnRhb2tzaXQgS2lsb2dyYW3QlMKxPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0tHTSciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEtpbG9ncmFtPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0tINiciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEtpbG9ncmFtLUJh0JXRnzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdLSE8nIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBIaWRyb2plbiBQZXJva3NpdCBLaWxvZ3JhbdCUwrE8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnS01BJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gTWV0aWwgQW1pbmxlcmluIEtpbG9ncmFt0JTCsTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdLTkknIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBBem90dW4gS2lsb2dyYW3QlMKxPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0tPSCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEtnLiBQb3Rhc3l1bSBIaWRyb2tzaXQ8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnS1BIJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gS2cgUG90YXN5dW0gT2tzaWQ8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnS1BSJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gS2lsb2dyYW0t0JPigKFpZnQ8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnS1NEJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gJTkwIEt1cnUg0JPRmnLQk9GYbiBLZy48L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnS1NIJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gU29keXVtIEhpZHJva3NpdCBLZy48L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnS1VSJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gVXJhbnl1bSBLaWxvZ3JhbdCUwrE8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnS1dIJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gS2lsb3dhdHQgU2FhdDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdLV1QnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBLaWxvd2F0dDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdMUEEnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBTYWYgQWxrb2wgTGl0cmVzaTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdMVFInIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBMaXRyZTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdNVFInIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBNZXRyZTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdOQ0wnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBI0JPRmGNyZSBBZGVkaTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdOQ1InIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBLYXJhdDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdPTVYnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBPVFYgTWFrdHUgVmVyZ2k8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnT1RCJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gT1RWIGJpcmltIGZpeWF00JTCsTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdQUiciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+INCT4oChaWZ0PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ1I5JyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gQmluIE1ldHJlIEvQk9GYcDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdUMyciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEJpbiBBZGV0PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ1RXSCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEJpbiBLaWxvd2F0dCBTYWF0PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgPSAnR1JPJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gR3Jvc2E8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSA9ICdEWk4nIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBE0JPRmHppbmU8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSA9ICdNV0gnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBNRUdBV0FUVCBTQUFUICgxMDAwIGtXLmgpPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KCQkJCQkJCQkJCTx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgPSAnWVJEJyI+CgkJCQkJCQkJCQkJPHNwYW4+CgkJCQkJCQkJCQkJCTx4c2w6dGV4dD4gWWFyZDwveHNsOnRleHQ+CgkJCQkJCQkJCQkJPC9zcGFuPgoJCQkJCQkJCQkJPC94c2w6d2hlbj4KCQkJCQkJCQkJCTx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgPSAnRE1LJyI+CgkJCQkJCQkJCQkJPHNwYW4+CgkJCQkJCQkJCQkJCTx4c2w6dGV4dD4gRGVzaW1ldHJla2FyZTwveHNsOnRleHQ+CgkJCQkJCQkJCQkJPC9zcGFuPgoJCQkJCQkJCQkJPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6b3RoZXJ3aXNlPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iQHVuaXRDb2RlIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOm90aGVyd2lzZT4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpjaG9vc2U+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgPC9zcGFuPgo8L3RkPgo8dGQ+CiAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iZm9ybWF0LW51bWJlciguL2NhYzpQcmljZS9jYmM6UHJpY2VBbW91bnQsICcjIyMuIyMwLCMjIyMjIyMjJywgJ2V1cm9wZWFuJykiLz4KICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLi9jYWM6UHJpY2UvY2JjOlByaWNlQW1vdW50L0BjdXJyZW5jeUlEIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuL2NhYzpQcmljZS9jYmM6UHJpY2VBbW91bnQvQGN1cnJlbmN5SUQiLz4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICA8L3NwYW4+CjwvdGQ+Cjx0ZD4KICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoLi9jYmM6TGluZUV4dGVuc2lvbkFtb3VudCwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuL2NiYzpMaW5lRXh0ZW5zaW9uQW1vdW50L0BjdXJyZW5jeUlEIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuL2NiYzpMaW5lRXh0ZW5zaW9uQW1vdW50L0BjdXJyZW5jeUlEIi8+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgPC9zcGFuPgo8L3RkPgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6SW52b2ljZUxpbmUvY2FjOlRheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbC9jYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZS9jYmM6VGF4VHlwZUNvZGUgPScwMDE1JyI+Cjx0ZD4KICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2gKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSIuL2NhYzpUYXhUb3RhbC9jYWM6VGF4U3VidG90YWwvY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUiPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNiYzpUYXhUeXBlQ29kZT0nMDAxNScgIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuLi8uLi9jYmM6UGVyY2VudCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+ICU8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoLi4vLi4vY2JjOlBlcmNlbnQsICcjIyMuIyMwLDAwJywgJ2V1cm9wZWFuJykiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDwvc3Bhbj4KPC90ZD4KPC94c2w6aWY+IAo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6SW52b2ljZUxpbmUvY2FjOlRheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbC9jYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZS9jYmM6VGF4VHlwZUNvZGUgPScwMDE1JyI+Cjx0ZD4KICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii4vY2FjOlRheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbC9jYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY2JjOlRheFR5cGVDb2RlPScwMDE1JyAiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoLi4vLi4vY2JjOlRheEFtb3VudCwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLi4vLi4vY2JjOlRheEFtb3VudC9AY3VycmVuY3lJRCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4uLy4uL2NiYzpUYXhBbW91bnQvQGN1cnJlbmN5SUQgPSAnVFJZJyBvciAuLi8uLi9jYmM6VGF4QW1vdW50L0BjdXJyZW5jeUlEID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VEw8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4uLy4uL2NiYzpUYXhBbW91bnQvQGN1cnJlbmN5SUQgIT0gJ1RSWScgYW5kIC4uLy4uL2NiYzpUYXhBbW91bnQvQGN1cnJlbmN5SUQgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLi4vLi4vY2JjOlRheEFtb3VudC9AY3VycmVuY3lJRCIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgPC9zcGFuPgo8L3RkPgo8L3hzbDppZj4gCjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpQcm9maWxlSUQ9J0lIUkFDQVQnIj4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkludm9pY2VMaW5lL2NhYzpEZWxpdmVyeS9jYWM6RGVsaXZlcnlUZXJtcy9jYmM6SURbQHNjaGVtZUlEPSdJTkNPVEVSTVMnXSI+CgkJPHRkIGNsYXNzPSJsaW5lVGFibGVUZCIgYWxpZ249InJpZ2h0Ij4KCQkJCTx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgoJCQkJPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpEZWxpdmVyeS9jYWM6RGVsaXZlcnlUZXJtcy9jYmM6SURbQHNjaGVtZUlEPSdJTkNPVEVSTVMnXSI+CgkJCQkJCTx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgoJCQkJCQk8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CgkJCQk8L3hzbDpmb3ItZWFjaD4KCQk8L3RkPgo8L3hzbDppZj4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkludm9pY2VMaW5lL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOlRyYW5zcG9ydEhhbmRsaW5nVW5pdC9jYWM6QWN0dWFsUGFja2FnZS9jYmM6UGFja2FnaW5nVHlwZUNvZGUiPgoJCTx0ZCBjbGFzcz0ibGluZVRhYmxlVGQiIGFsaWduPSJyaWdodCI+CgkJCQk8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KCQkJCTx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6RGVsaXZlcnkvY2FjOlNoaXBtZW50L2NhYzpUcmFuc3BvcnRIYW5kbGluZ1VuaXQvY2FjOkFjdHVhbFBhY2thZ2UvY2JjOlBhY2thZ2luZ1R5cGVDb2RlIj4KCQkJCQkJPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CgkJCQkJCTx4c2w6Y2FsbC10ZW1wbGF0ZSBuYW1lPSJQYWNrYWdpbmciPgoJCQkJCQkJCTx4c2w6d2l0aC1wYXJhbSBuYW1lPSJQYWNrYWdpbmdUeXBlIj4KCQkJCQkJCQkJCTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CgkJCQkJCQkJPC94c2w6d2l0aC1wYXJhbT4KCQkJCQkJPC94c2w6Y2FsbC10ZW1wbGF0ZT4KCQkJCTwveHNsOmZvci1lYWNoPgoJCTwvdGQ+CjwveHNsOmlmPgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6SW52b2ljZUxpbmUvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYWM6VHJhbnNwb3J0SGFuZGxpbmdVbml0L2NhYzpBY3R1YWxQYWNrYWdlL2NiYzpJRCI+CgkJPHRkIGNsYXNzPSJsaW5lVGFibGVUZCIgYWxpZ249InJpZ2h0Ij4KCQkJCTx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgoJCQkJPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOlRyYW5zcG9ydEhhbmRsaW5nVW5pdC9jYWM6QWN0dWFsUGFja2FnZS9jYmM6SUQiPgoJCQkJCQk8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KCQkJCQkJPHhzbDphcHBseS10ZW1wbGF0ZXMvPgoJCQkJPC94c2w6Zm9yLWVhY2g+CgkJPC90ZD4KPC94c2w6aWY+Cjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpJbnZvaWNlTGluZS9jYWM6RGVsaXZlcnkvY2FjOlNoaXBtZW50L2NhYzpUcmFuc3BvcnRIYW5kbGluZ1VuaXQvY2FjOkFjdHVhbFBhY2thZ2UvY2JjOlF1YW50aXR5Ij4KCQk8dGQgY2xhc3M9ImxpbmVUYWJsZVRkIiBhbGlnbj0icmlnaHQiPgoJCQkJPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CgkJCQk8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYWM6VHJhbnNwb3J0SGFuZGxpbmdVbml0L2NhYzpBY3R1YWxQYWNrYWdlL2NiYzpRdWFudGl0eSI+CgkJCQkJCTx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgoJCQkJCQk8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CgkJCQk8L3hzbDpmb3ItZWFjaD4KCQk8L3RkPgo8L3hzbDppZj4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkludm9pY2VMaW5lL2NhYzpEZWxpdmVyeS9jYWM6RGVsaXZlcnlBZGRyZXNzIj4KCQk8dGQgY2xhc3M9ImxpbmVUYWJsZVRkIiBhbGlnbj0icmlnaHQiPgoJCQkJPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CgkJCQk8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOkRlbGl2ZXJ5L2NhYzpEZWxpdmVyeUFkZHJlc3MiPgoJCQkJCQk8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KCQkJCQkJPHhzbDphcHBseS10ZW1wbGF0ZXMvPgoJCQkJPC94c2w6Zm9yLWVhY2g+CgkJPC90ZD4KPC94c2w6aWY+Cjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpJbnZvaWNlTGluZS9jYWM6RGVsaXZlcnkvY2FjOlNoaXBtZW50L2NhYzpTaGlwbWVudFN0YWdlL2NiYzpUcmFuc3BvcnRNb2RlQ29kZSI+CgkJPHRkIGNsYXNzPSJsaW5lVGFibGVUZCIgYWxpZ249InJpZ2h0Ij4KCQkJCTx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgoJCQkJPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOlNoaXBtZW50U3RhZ2UvY2JjOlRyYW5zcG9ydE1vZGVDb2RlIj4KCQkJCQkJPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CgkJCQkJCTx4c2w6Y2FsbC10ZW1wbGF0ZSBuYW1lPSJUcmFuc3BvcnRNb2RlIj4KCQkJCQkJCQk8eHNsOndpdGgtcGFyYW0gbmFtZT0iVHJhbnNwb3J0TW9kZVR5cGUiPgoJCQkJCQkJCQkJPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4iLz4KCQkJCQkJCQk8L3hzbDp3aXRoLXBhcmFtPgoJCQkJCQk8L3hzbDpjYWxsLXRlbXBsYXRlPgoJCQkJPC94c2w6Zm9yLWVhY2g+CgkJPC90ZD4KPC94c2w6aWY+Cjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpJbnZvaWNlTGluZS9jYWM6RGVsaXZlcnkvY2FjOlNoaXBtZW50L2NhYzpHb29kc0l0ZW0vY2JjOlJlcXVpcmVkQ3VzdG9tc0lEIj4KCQk8dGQgY2xhc3M9ImxpbmVUYWJsZVRkIiBhbGlnbj0icmlnaHQiPgoJCQkJPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CgkJCQk8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYWM6R29vZHNJdGVtL2NiYzpSZXF1aXJlZEN1c3RvbXNJRCI+CgkJCQkJCTx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgoJCQkJCQk8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CgkJCQk8L3hzbDpmb3ItZWFjaD4KCQk8L3RkPgo8L3hzbDppZj4KPC94c2w6aWY+IAoKICAgICAgICA8L3RyPgo8L3hzbDp0ZW1wbGF0ZT4KCjwveHNsOnN0eWxlc2hlZXQ+Cg==";
            #endregion
            attachment.AppendChild(embeddedDocumentBinaryObject);
            additionalDocumentReference.AppendChild(attachment);

            root.AppendChild(additionalDocumentReference);

            additionalDocumentReference = doc.CreateElement("cac", "AdditionalDocumentReference", xmlnscac.Value);
            additionalDocumentReferenceId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            additionalDocumentReferenceId.InnerText = gidenFatura.GidenFaturaId;
            additionalDocumentReference.AppendChild(additionalDocumentReferenceId);
            issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
            issueDate.InnerText = gidenFatura.DuzenlemeTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
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