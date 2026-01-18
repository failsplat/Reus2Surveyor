using ClosedXML.Attributes;
using ClosedXML.Excel;
using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Reus2Surveyor
{
    public partial class StatCollector
    {
        public static DataTable NestDictToDataTable<T, T2>(IDictionary<T, Dictionary<T, T2>> input, string indexName)
        {
            // Similar to pandas.DataFrame.from_dict with the "row" orientation
            DataTable output = new();
            output.Columns.Add(indexName);

            List<string> columnHeaders = [.. input.SelectMany(kv => kv.Value.Select(kv => kv.Key.ToString())).Distinct()];
            columnHeaders.Sort();
            foreach (string colHead in columnHeaders)
            {
                output.Columns.Add(colHead);
                output.Columns[colHead].DataType = typeof(T2);
            }

            foreach (KeyValuePair<T, Dictionary<T, T2>> rowKV in input)
            {
                DataRow newRow = output.NewRow();
                newRow[indexName] = rowKV.Key.ToString();
                foreach (KeyValuePair<T, T2> dataKV in rowKV.Value)
                {
                    newRow[dataKV.Key.ToString()] = dataKV.Value;
                }
                output.Rows.Add(newRow);
            }

            return output;
        }
        public static OrderedDictionary<TKey, Dictionary<TKey, double>> NestedCounterToNestedRatioDictionary<TKey>(IDictionary<TKey, Dictionary<TKey, int>> input)
        {
            OrderedDictionary<TKey, Dictionary<TKey, double>> output = [];

            List<KeyValuePair<TKey, int>> flattened = [.. input.SelectMany(kv1 => kv1.Value.ToList())];
            int total = flattened.Select(kv => kv.Value).Sum();
            Dictionary<TKey, int> columnTotals = [];
            List<TKey> columns = [.. flattened.Select(kv => kv.Key).Distinct()];
            foreach (TKey c in columns) columnTotals[c] = 0;
            foreach (KeyValuePair<TKey, int> f in flattened)
            {
                columnTotals[f.Key] += f.Value;
            }
            foreach ((TKey rowKey, Dictionary<TKey, int> row) in input)
            {
                output[rowKey] = [];
                int rowTotal = row.Values.Sum();
                foreach ((TKey colKey, int count) in row)
                {
                    double value = ((double)row[colKey] / (double)columnTotals[colKey]) / ((double)rowTotal / (double)total);
                    if (Double.IsNaN(value)) value = 0;
                    output[rowKey][colKey] = value;
                }
                List<TKey> missingCols = [.. columns.Except(row.Keys)];
                foreach (TKey missingCol in missingCols)
                {
                    output[rowKey][missingCol] = 0;
                }
            }
            return output;
        }

        public static DataTable ExpandToColumns<T>(IEnumerable<T> input, Glossaries glossaryInstance)
        {
            DataTable output = new();

            Type thisType = typeof(T);
            List<MemberInfo> columnMembers = [];

            columnMembers.AddRange(thisType.GetFields());
            columnMembers.AddRange(thisType.GetProperties());

            columnMembers = [.. columnMembers.Where(mi => !(mi.GetCustomAttribute<XLColumnAttribute>() is null || mi.GetCustomAttribute<XLColumnAttribute>().Ignore))];
            columnMembers = [.. columnMembers.OrderBy(mi => mi.GetCustomAttribute<XLColumnAttribute>().Order)];

            // Build table headers
            foreach (MemberInfo mi in columnMembers)
            {
                UnpackToBiomesAttribute biomeAttr = mi.GetCustomAttribute<UnpackToBiomesAttribute>();
                UnpackToSpiritsAttribute spiritAttr = mi.GetCustomAttribute<UnpackToSpiritsAttribute>();
                string headerName;
                XLColumnAttribute xlColAttr = mi.GetCustomAttribute<XLColumnAttribute>();
                if (xlColAttr is null) headerName = mi.Name;
                else if (xlColAttr.Header is null) headerName = mi.Name;
                else headerName = xlColAttr.Header;
                if (biomeAttr is not null)
                {
                    foreach (string biomeName in glossaryInstance.BiomeHashByName.Keys)
                    {
                        string subheader = biomeAttr.Prefix + biomeName + biomeAttr.Suffix;
                        output.Columns.Add(subheader);
                        if (biomeAttr.DefaultValue is not null)
                        {
                            output.Columns[subheader].DataType = biomeAttr.DefaultValue.GetType();
                        }
                        else
                        {
                            output.Columns[subheader].DataType = typeof(string);
                        }
                    }
                }
                else if (spiritAttr is not null)
                {
                    foreach (string spiritName in glossaryInstance.SpiritHashByName.Keys)
                    {
                        string subheader = spiritAttr.Prefix + spiritName + spiritAttr.Suffix;
                        output.Columns.Add(subheader);
                        if (spiritAttr.DefaultValue is not null)
                        {
                            output.Columns[subheader].DataType = spiritAttr.DefaultValue.GetType();
                        }
                        else
                        {
                            output.Columns[subheader].DataType = typeof(string);
                        }
                    }
                }
                else
                {
                    output.Columns.Add(headerName);
                    Type memberType;
                    switch (mi.MemberType)
                    {
                        case (MemberTypes.Property):
                            memberType = ((PropertyInfo)mi).PropertyType;
                            break;
                        case (MemberTypes.Field):
                            memberType = ((FieldInfo)mi).FieldType;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    Type underlyingType = Nullable.GetUnderlyingType(memberType);
                    if (underlyingType is null) output.Columns[headerName].DataType = memberType;
                    else output.Columns[headerName].DataType = underlyingType;
                }
            }

            // Build table rows
            foreach (T entry in input)
            {
                DataRow dr = output.NewRow();

                foreach (MemberInfo mi in columnMembers)
                {
                    UnpackToBiomesAttribute biomeAttr = mi.GetCustomAttribute<UnpackToBiomesAttribute>();
                    UnpackToSpiritsAttribute spiritAttr = mi.GetCustomAttribute<UnpackToSpiritsAttribute>();
                    string headerMain;
                    XLColumnAttribute xlColAttr = mi.GetCustomAttribute<XLColumnAttribute>();
                    if (xlColAttr is null) headerMain = mi.Name;
                    else if (xlColAttr.Header is null) headerMain = mi.Name;
                    else headerMain = xlColAttr.Header;

                    if (biomeAttr is not null)
                    {
                        switch (mi.MemberType)
                        {
                            case MemberTypes.Field:
                                IDictionary fieldDict = ((FieldInfo)mi).GetValue(entry) as IDictionary;
                                List<string> fieldKeys = [];
                                foreach (DictionaryEntry de in fieldDict)
                                {
                                    fieldKeys.Add((string)de.Key);
                                }
                                foreach (string biomeName in glossaryInstance.BiomeHashByName.Keys)
                                {
                                    string subheader = biomeAttr.Prefix + biomeName + biomeAttr.Suffix;
                                    object fieldValue;
                                    if (fieldKeys.Contains(biomeName))
                                    {
                                        fieldValue = fieldDict[biomeName];
                                    }
                                    else
                                    {
                                        fieldValue = biomeAttr.DefaultValue;
                                    }
                                    Type ct = output.Columns[subheader].DataType;
                                    bool blank = false;
                                    if (biomeAttr.NullOnZeroOrBlank)
                                    {
                                        double? asNum = fieldValue as double?;
                                        string asString = fieldValue as string;
                                        blank |= (asNum is not null && asNum == 0);
                                        blank |= (asString is not null && asString.Length == 0);
                                    }
                                    if (blank) fieldValue = DBNull.Value;
                                    dr[subheader] = fieldValue;
                                }
                                break;
                            case MemberTypes.Property:
                                IDictionary propDict = ((FieldInfo)mi).GetValue(entry) as IDictionary;
                                List<string> propKeys = [];
                                foreach (DictionaryEntry de in propDict)
                                {
                                    propKeys.Add((string)de.Key);
                                }
                                foreach (string biomeName in glossaryInstance.BiomeHashByName.Keys)
                                {
                                    string subheader = biomeAttr.Prefix + biomeName + biomeAttr.Suffix;
                                    object propValue;
                                    if (propKeys.Contains(biomeName))
                                    {
                                        propValue = propDict[biomeName];
                                    }
                                    else
                                    {
                                        propValue = biomeAttr.DefaultValue;
                                    }
                                    Type ct = output.Columns[subheader].DataType;
                                    bool blank = false;
                                    if (biomeAttr.NullOnZeroOrBlank)
                                    {
                                        double? asNum = propValue as double?;
                                        string asString = propValue as string;
                                        blank |= (asNum is not null && asNum == 0);
                                        blank |= (asString is not null && asString.Length == 0);
                                    }
                                    if (blank) propValue = DBNull.Value;
                                    dr[subheader] = propValue;
                                }
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    else if (spiritAttr is not null)
                    {
                        switch (mi.MemberType)
                        {
                            case MemberTypes.Field:
                                IDictionary fieldDict = ((FieldInfo)mi).GetValue(entry) as IDictionary;
                                List<string> fieldKeys = [];
                                foreach (DictionaryEntry de in fieldDict)
                                {
                                    fieldKeys.Add((string)de.Key);
                                }
                                foreach (string spiritName in glossaryInstance.SpiritHashByName.Keys)
                                {
                                    string subheader = spiritAttr.Prefix + spiritName + spiritAttr.Suffix;
                                    object fieldValue;
                                    if (fieldKeys.Contains(spiritName))
                                    {
                                        fieldValue = fieldDict[spiritName];
                                    }
                                    else
                                    {
                                        fieldValue = spiritAttr.DefaultValue;
                                    }
                                    Type ct = output.Columns[subheader].DataType;
                                    bool blank = false;
                                    if (spiritAttr.NullOnZeroOrBlank)
                                    {
                                        double? asNum = fieldValue as double?;
                                        string asString = fieldValue as string;
                                        blank |= (asNum is not null && asNum == 0);
                                        blank |= (asString is not null && asString.Length == 0);
                                    }
                                    if (blank) fieldValue = DBNull.Value;
                                    dr[subheader] = fieldValue;
                                }
                                break;
                            case MemberTypes.Property:
                                IDictionary propDict = ((FieldInfo)mi).GetValue(entry) as IDictionary;
                                List<string> propKeys = [];
                                foreach (DictionaryEntry de in propDict)
                                {
                                    propKeys.Add((string)de.Key);
                                }
                                foreach (string biomeName in glossaryInstance.BiomeHashByName.Keys)
                                {
                                    string subheader = biomeAttr.Prefix + biomeName + biomeAttr.Suffix;
                                    object propValue;
                                    if (propKeys.Contains(biomeName))
                                    {
                                        propValue = propDict[biomeName];
                                    }
                                    else
                                    {
                                        propValue = biomeAttr.DefaultValue;
                                    }
                                    Type ct = output.Columns[subheader].DataType;
                                    bool blank = false;
                                    if (biomeAttr.NullOnZeroOrBlank)
                                    {
                                        double? asNum = propValue as double?;
                                        string asString = propValue as string;
                                        blank |= (asNum is not null && asNum == 0);
                                        blank |= (asString is not null && asString.Length == 0);
                                    }
                                    if (blank) propValue = DBNull.Value;
                                    dr[subheader] = propValue;
                                }
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        switch (mi.MemberType)
                        {
                            case MemberTypes.Field:
                                var fieldPoint = ((FieldInfo)mi).GetValue(entry);
                                if (fieldPoint is null) dr[headerMain] = DBNull.Value;
                                else dr[headerMain] = fieldPoint;
                                break;
                            case MemberTypes.Property:
                                var propertyPoint = ((PropertyInfo)mi).GetValue(entry);
                                if (propertyPoint is null) dr[headerMain] = DBNull.Value;
                                else dr[headerMain] = propertyPoint;
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                }

                output.Rows.Add(dr);
            }
            return output;
        }

        public void WriteToExcel(string dstPath, bool heatmaps = false)
        {
            using (XLWorkbook wb = new())
            {
                var planetSummWs = wb.AddWorksheet("Planets");
                DataTable planetDataTable = ExpandToColumns(this.PlanetSummaries, this.glossaryInstance);
                var planetTable = planetSummWs.Cell("A1").InsertTable(planetDataTable, "Planets");
                planetTable.Theme = XLTableTheme.TableStyleMedium4;
                ApplyTableNumberFormats(GetColumnFormats(typeof(PlanetSummaryEntry), this.glossaryInstance), planetTable);

                var cityWs = wb.AddWorksheet("Cities");
                DataTable cityDataTable = ExpandToColumns(this.CitySummaries, this.glossaryInstance);
                var cityTable = cityWs.Cell("A1").InsertTable(cityDataTable, "Cities");
                cityTable.Theme = XLTableTheme.TableStyleLight1;
                ApplyTableNumberFormats(GetColumnFormats(typeof(CitySummaryEntry), this.glossaryInstance), cityTable);

                var spiritWs = wb.AddWorksheet("Spirits");
                DataTable spiritDataTable = ExpandToColumns(this.SpiritStats.Values, this.glossaryInstance);
                var spiritTable = spiritWs.Cell("A1").InsertTable(spiritDataTable, "Spirits");
                spiritTable.Theme = XLTableTheme.TableStyleMedium5;
                ApplyTableNumberFormats(GetColumnFormats(typeof(SpiritStatEntry), this.glossaryInstance), spiritTable);
                spiritWs.SheetView.FreezeColumns(1);

                var bioWs = wb.AddWorksheet("Biotica");
                DataTable bioticaDataTable = ExpandToColumns(this.BioticaStats.Values, this.glossaryInstance);
                var bioticaTable = bioWs.Cell("A1").InsertTable(bioticaDataTable, "Biotica");
                bioticaTable.Theme = XLTableTheme.TableStyleMedium3;
                ApplyTableNumberFormats(GetColumnFormats(typeof(BioticumStatEntry), this.glossaryInstance), bioticaTable);
                bioWs.SheetView.FreezeColumns(1);

                var luxWs = wb.AddWorksheet("Luxuries");
                DataTable luxuryDataTable = ExpandToColumns(this.LuxuryStats.Values, this.glossaryInstance);
                var luxuryTable = luxWs.Cell("A1").InsertTable(luxuryDataTable, "Luxuries");
                luxuryTable.Theme = XLTableTheme.TableStyleMedium7;
                ApplyTableNumberFormats(GetColumnFormats(typeof(LuxuryStatEntry), this.glossaryInstance), luxuryTable);
                luxWs.SheetView.FreezeColumns(1);

                var eraWs = wb.AddWorksheet("Era");
                var eraTable = eraWs.Cell("A1").InsertTable(this.EraStats.Values.OrderBy(ese => (ese.Era, -ese.Count)));
                ApplyTableNumberFormats(GetColumnFormats(typeof(EraStatEntry), this.glossaryInstance), eraTable);
                eraTable.Theme = XLTableTheme.TableStyleMedium6;

                var projectWs = wb.AddWorksheet("Projects");
                DataTable projectDataTable = ExpandToColumns(this.ProjectStats.Values, this.glossaryInstance);
                var projectTable = projectWs.Cell("A1").InsertTable(projectDataTable, "Projects");
                ApplyTableNumberFormats(GetColumnFormats(typeof(ProjectStatEntry), this.glossaryInstance), projectTable);
                projectWs.SheetView.FreezeColumns(1);

                if (heatmaps)
                {
                    var heatmapWs = wb.AddWorksheet("Heatmaps");
                    int xPos = 0; int yPos = 0;
                    int width; int height;
                    int tileSteps = 35;

                    // Column 1: Biotica types by planet
                    Func<double, double, (int a, int b, int c), Color> biotypeShader = TernaryTileHeatmap.MakeCompositionShader(Color.Lime, Color.Red, Color.Blue);
                    List<(double p, double a, double m)> bioTypesOnPlanetAll = [.. this.PlanetSummaries
                        .Select(ps => (ps.PPlant ?? 0, ps.PAnimal ?? 0, ps.PMineral ?? 0))
                        .Where(p => (p.Item1 + p.Item2 + p.Item3 > 0))];

                    using (MemoryStream ms = new MemoryStream())
                    using (Image im = new TernaryTileHeatmap(tileSteps, bioTypesOnPlanetAll).DrawStandardPlot(Color.White, biotypeShader, "Planet Biotica Types\nAll Planets",
                        labelA: "Plant", labelB: "Animal", labelC: "Mineral"))
                    {
                        im.SaveAsPng(ms);
                        var pic = heatmapWs.AddPicture(ms);
                        pic.MoveTo(xPos, yPos);

                        width = pic.Width;
                        height = pic.Height;

                        yPos += height;
                    }

                    foreach (string spiritName in this.glossaryInstance.SpiritHashByName.Keys)
                    {
                        List<(double p, double a, double m)> bioTypePercents = [.. this.PlanetSummaries
                            .Where(ps => ps.Spirit == spiritName)
                            .Select(ps => (ps.PPlant ?? 0, ps.PAnimal ?? 0, ps.PMineral ?? 0))
                            .Where(p => (p.Item1 + p.Item2 + p.Item3 > 0))];
                        Image im = new TernaryTileHeatmap(tileSteps, bioTypePercents).DrawStandardPlot(Color.White, biotypeShader, $"Planet Biotica Types\n{spiritName}",
                        labelA: "Plant", labelB: "Animal", labelC: "Mineral");
                        using (MemoryStream ms = new MemoryStream())
                        {
                            im.SaveAsPng(ms);
                            var pic = heatmapWs.AddPicture(ms);
                            pic.MoveTo(xPos, yPos);

                            yPos += height;
                        }
                    }

                    // Column 2: Biotica types in borders
                    xPos += width;
                    yPos = 0;

                    List<(double p, double a, double m)> bioTypeInBordersAll = [..
                            this.CitySummaries
                            .Select(cs => (cs.PPlant ?? 0, cs.PAnimal ?? 0, cs.PMineral ?? 0))
                            .Where(p => (p.Item1 + p.Item2 + p.Item3 > 0))
                            ];

                    using (MemoryStream ms = new MemoryStream())
                    using (Image im = new TernaryTileHeatmap(tileSteps, bioTypeInBordersAll).DrawStandardPlot(Color.White, biotypeShader, "Biotica Types in Borders\nAll Cities",
                        labelA: "Plant", labelB: "Animal", labelC: "Mineral", plotDots: true))
                    {
                        im.SaveAsPng(ms);
                        var pic = heatmapWs.AddPicture(ms);
                        pic.MoveTo(xPos, yPos);

                        yPos += height;
                    }

                    foreach (string spiritName in this.glossaryInstance.SpiritHashByName.Keys)
                    {
                        List<(double p, double a, double m)> bioTypePercents = [..
                            this.CitySummaries
                            .Where(cs => cs.Char == spiritName)
                            .Select(cs => (cs.PPlant ?? 0, cs.PAnimal ?? 0, cs.PMineral ?? 0))
                            .Where(p => (p.Item1 + p.Item2 + p.Item3 > 0))
                            ];
                        Image im = new TernaryTileHeatmap(tileSteps, bioTypePercents).DrawStandardPlot(Color.White, biotypeShader, $"Biotica Types in Borders\n{spiritName}",
                        labelA: "Plant", labelB: "Animal", labelC: "Mineral");
                        using (MemoryStream ms = new MemoryStream())
                        {
                            im.SaveAsPng(ms);
                            var pic = heatmapWs.AddPicture(ms);
                            pic.MoveTo(xPos, yPos);

                            yPos += height;
                        }
                    }

                    // Column 3: Prosperity composition by planet
                    xPos += width;
                    yPos = 0;

                    Func<double, double, (int a, int b, int c), Color> prosCompositionShader = TernaryTileHeatmap.MakeCompositionShader(Color.Red, Color.Blue, Color.Yellow);
                    List<(double pop, double tech, double wel)> planetProsPercentsAll = [..
                        this.PlanetSummaries
                        .Select(ps => (ps.PPop ?? 0, ps.PTech ?? 0, ps.PWel ?? 0))
                        ];

                    using (MemoryStream ms = new MemoryStream())
                    using (Image im = new TernaryTileHeatmap(tileSteps, planetProsPercentsAll).DrawStandardPlot(Color.White, prosCompositionShader, "Planet Prosperity Composition\nAll Planets",
                        labelA: "Pop", labelB: "Tech", labelC: "Wealth"))
                    {
                        im.SaveAsPng(ms);
                        var pic = heatmapWs.AddPicture(ms);
                        pic.MoveTo(xPos, yPos);

                        yPos += height;
                    }

                    foreach (string spiritName in this.glossaryInstance.SpiritHashByName.Keys)
                    {
                        List<(double pop, double tech, double wel)> prosPercents = [..
                            this.PlanetSummaries
                            .Where(ps => ps.Spirit == spiritName)
                            .Select(ps => (ps.PPop ?? 0, ps.PTech ?? 0, ps.PWel ?? 0))
                            .Where(p => (p.Item1 + p.Item2 + p.Item3 > 0))
                            ];
                        Image im = new TernaryTileHeatmap(tileSteps, prosPercents).DrawStandardPlot(Color.White, prosCompositionShader, $"Planet Prosperity Composition\n{spiritName}",
                            labelA: "Pop", labelB: "Tech", labelC: "Wealth");
                        using (MemoryStream ms = new MemoryStream())
                        {
                            im.SaveAsPng(ms);
                            var pic = heatmapWs.AddPicture(ms);
                            pic.MoveTo(xPos, yPos);

                            yPos += height;
                        }
                    }

                    // Column 4: City prosperity composition
                    xPos += width;
                    yPos = 0;

                    List<(double p, double a, double m)> cityProsPercentsAll = [..
                            this.CitySummaries
                            .Select(ps => (ps.PPop ?? 0, ps.PTech ?? 0, ps.PWel ?? 0))
                            .Where(p => (p.Item1 + p.Item2 + p.Item3 > 0))
                            ];

                    using (MemoryStream ms = new MemoryStream())
                    using (Image im = new TernaryTileHeatmap(tileSteps, cityProsPercentsAll).DrawStandardPlot(Color.White, prosCompositionShader, "City Prosperity Composition\nAll Cities",
                        labelA: "Pop", labelB: "Tech", labelC: "Wealth", plotDots: true))
                    {
                        im.SaveAsPng(ms);
                        var pic = heatmapWs.AddPicture(ms);
                        pic.MoveTo(xPos, yPos);

                        yPos += height;
                    }

                    foreach (string spiritName in this.glossaryInstance.SpiritHashByName.Keys)
                    {
                        List<(double p, double a, double m)> prosPercents = [..
                            this.CitySummaries
                            .Where(cs => cs.Char == spiritName)
                            .Select(ps => (ps.PPop ?? 0, ps.PTech ?? 0, ps.PWel ?? 0))
                            .Where(p => (p.Item1 + p.Item2 + p.Item3 > 0))
                            ];
                        Image im = new TernaryTileHeatmap(tileSteps, prosPercents).DrawStandardPlot(Color.White, prosCompositionShader, $"City Prosperity Composition\n{spiritName}",
                            labelA: "Pop", labelB: "Tech", labelC: "Wealth");
                        using (MemoryStream ms = new MemoryStream())
                        {
                            im.SaveAsPng(ms);
                            var pic = heatmapWs.AddPicture(ms);
                            pic.MoveTo(xPos, yPos);

                            yPos += height;
                        }
                    }
                }

                DataTable bioticaVsSpiritCountDataTable = NestDictToDataTable(this.BioticumVsSpiritCounter, "Bioticum");
                var bioVsCharCountWs = wb.AddWorksheet("BioVsCharC");
                var bioVsCharCountTable = bioVsCharCountWs.Cell("A1").InsertTable(bioticaVsSpiritCountDataTable);

                DataTable bioticaVsSpiritRatioDataTable = NestDictToDataTable(this.BioticumVsSpiritRatios, "Bioticum");
                var bioVsCharRatioWs = wb.AddWorksheet("BioVsCharR");
                var bioVsCharRatioTable = bioVsCharRatioWs.Cell("A1").InsertTable(bioticaVsSpiritRatioDataTable);
                foreach (var col in bioVsCharRatioTable.Columns())
                {
                    col.Style.NumberFormat.Format = "0.0000";
                }

                DataTable bioticaVsPrSpiritCountDataTable = NestDictToDataTable(this.BioticumVsPrSpiritCounter, "Bioticum");
                var bioVsPrCharCountWs = wb.AddWorksheet("BioVsPSpC");
                var bioVsPrCharCountTable = bioVsPrCharCountWs.Cell("A1").InsertTable(bioticaVsPrSpiritCountDataTable);

                DataTable bioticaVsPrSpiritRatioDataTable = NestDictToDataTable(this.BioticumVsPrSpiritRatios, "Bioticum");
                var bioVsPrCharRatioWs = wb.AddWorksheet("BioVsPSpR");
                var bioVsPrCharRatioTable = bioVsPrCharRatioWs.Cell("A1").InsertTable(bioticaVsPrSpiritRatioDataTable);
                foreach (var col in bioVsPrCharRatioTable.Columns())
                {
                    col.Style.NumberFormat.Format = "0.0000";
                }

                wb.SaveAs(dstPath);
            }
        }

        public static void FormatColumn(IXLTable table, string columnName, string numFormat = "0.00%")
        {
            var column = table.FindColumn(c => c.FirstCell().Value.ToString() == columnName);
            column.Style.NumberFormat.Format = numFormat;
        }

        public static void ApplyTableNumberFormats(Dictionary<string, HashSet<string>> columnFormats, IXLTable table)
        {
            foreach (KeyValuePair<string, HashSet<string>> kv in columnFormats)
            {
                string format = kv.Key;
                HashSet<string> columns = [.. kv.Value];

                foreach (string colName in columns)
                {
                    try
                    {
                        var col = table.FindColumn(c => c.FirstCell().Value.ToString() == colName);
                        if (col is null) continue;
                        col.Style.NumberFormat.Format = format;
                    }
                    catch { }
                }
            }
        }

        public static Dictionary<string, HashSet<string>> GetColumnFormats(Type T, Glossaries glossaryInstance)
        {
            List<MemberInfo> allMembers = [];
            allMembers.AddRange(T.GetFields());
            allMembers.AddRange(T.GetProperties());
            Dictionary<string, HashSet<string>> formats = [];

            foreach (MemberInfo mi in allMembers)
            {
                UnpackToBiomesAttribute biomeAttr = mi.GetCustomAttribute<UnpackToBiomesAttribute>();
                if (biomeAttr is not null && biomeAttr.NumberFormat is not null)
                {
                    foreach (string biomeName in glossaryInstance.BiomeHashByName.Keys)
                    {
                        string subheader = biomeAttr.Prefix + biomeName + biomeAttr.Suffix;
                        AddColumnFormat(ref formats, biomeAttr.NumberFormat, subheader);
                    }
                    continue;
                }

                UnpackToSpiritsAttribute spiritAttr = mi.GetCustomAttribute<UnpackToSpiritsAttribute>();
                if (spiritAttr is not null && spiritAttr.NumberFormat is not null)
                {
                    foreach (string spiritName in glossaryInstance.SpiritHashByName.Keys)
                    {
                        string subheader = spiritAttr.Prefix + spiritName + spiritAttr.Suffix;
                        AddColumnFormat(ref formats, spiritAttr.NumberFormat, subheader);
                    }
                    continue;
                }

                ColumnFormatAttribute colFormatAttr = mi.GetCustomAttribute<ColumnFormatAttribute>();
                XLColumnAttribute xlColAttr = mi.GetCustomAttribute<XLColumnAttribute>();
                if (colFormatAttr is not null)
                {
                    if (xlColAttr is not null && xlColAttr.Header is not null)
                    {
                        AddColumnFormat(ref formats, colFormatAttr.Fmt, xlColAttr.Header);
                        continue;
                    }
                    else
                    {
                        AddColumnFormat(ref formats, colFormatAttr.Fmt, mi.Name);
                        continue;
                    }

                }
            }
            return formats;
        }

        private static void AddColumnFormat(ref Dictionary<string, HashSet<string>> formatDictionary, string format, string header)
        {
            if (formatDictionary.TryGetValue(format, out HashSet<string> columns))
            {
                columns.Add(header);
            }
            else
            {
                formatDictionary.Add(format, [header]);
            }
        }
    }
}
