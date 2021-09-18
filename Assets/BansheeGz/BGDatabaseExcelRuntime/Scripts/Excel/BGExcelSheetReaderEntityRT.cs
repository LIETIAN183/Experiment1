/*
<copyright file="BGExcelSheetReaderEntityRT.cs" company="BansheeGz">
    Copyright (c) 2019-2021 All Rights Reserved
</copyright>
*/

using System;
using NPOI.SS.UserModel;
using UnityEngine;

namespace BansheeGz.BGDatabase
{
    public class BGExcelSheetReaderEntityRT : BGExcelSheetReaderART
    {
        //================================================================================================
        //                                              Static
        //================================================================================================
        public static void ReadEntities(IWorkbook book, BGBookInfo info, BGRepo repo, BGLogger logger, bool ignoreNew, BGSyncNameMapConfig nameMapConfig, BGExcelIdResolverFactoryRT IdResolverFactory)
        {
            logger.AppendLine("Reading entities: iterating sheets..");
            for (var i = 0; i < book.NumberOfSheets; i++)
            {
                var sheet = book.GetSheetAt(i);

                if (logger.AppendWarning(string.IsNullOrEmpty(sheet.SheetName), "Sheet with empty name at index $", i)) continue;


                logger.SubSection(() =>
                {
                    var meta = nameMapConfig == null ? repo[sheet.SheetName] : nameMapConfig.Map(repo, sheet.SheetName);
                    if (logger.AppendWarning(meta == null, "Sheet [$] is skipped. No meta with such name found or no proper mapping.", sheet.SheetName)) return;
                    if (logger.AppendWarning(info.HasEntitySheet(meta.Id), "Sheet [$] is skipped. Duplicate name, meta [$] was already been processed.", sheet.SheetName, meta.Name)) return;

                    BGExcelSheetReaderEntityRT reader;
                    if (sheet.PhysicalNumberOfRows == 0)
                    {
                        logger.AppendLine("Sheet [$] is mapped ok, but no rows found.", sheet.SheetName);
                        reader = new BGExcelSheetReaderEntityRT(i, meta, ignoreNew, null, logger, sheet.SheetName, null, null);
                    }
                    else
                    {
                        logger.AppendLine("Sheet [$] is mapped ok to [$] meta. $ rows found.", sheet.SheetName, meta.Name, sheet.LastRowNum + 1);
                        var headersRow = sheet.GetRow(0);
                        reader = new BGExcelSheetReaderEntityRT(i, meta, ignoreNew, headersRow, logger, sheet.SheetName, nameMapConfig,
                            IdResolverFactory == null
                                ? new BGExcelIdResolverIdRT(logger)
                                : IdResolverFactory.Create(meta.Id, logger)
                        );
                    }

                    info.AddEntitySheet(meta.Id, reader.Info);

                    if (logger.AppendWarning(!reader.Info.HasAnyData, "No columns found for Sheet [$].", sheet.SheetName)) return;

                    // read data
                    var count = 0;
                    var existingCount = 0;
                    var newCount = 0;
                    ForEachRowNoHeader(sheet, row =>
                    {
                        count++;
                        reader.Read(row, ref existingCount, ref newCount);
                    });
                    logger.AppendLine("Read $ rows. $ existing entities. $ new entities. $ rows are skipped.", count, existingCount, newCount, count - existingCount - newCount);
                }, "Reading sheet $", sheet.SheetName);
            }
        }

        //================================================================================================
        //                                              Fields
        //================================================================================================

        private readonly BGMetaEntity meta;
        private readonly BGEntitySheetInfo info;
        private readonly BGLogger logger;
        private readonly bool ignoreNew;
        private BGExcelIdResolverART idResolver;


        public BGEntitySheetInfo Info
        {
            get { return info; }
        }

        //================================================================================================
        //                                              Constructors
        //================================================================================================

        public BGExcelSheetReaderEntityRT(int sheetNumber, BGMetaEntity meta, bool ignoreNew, IRow headersRow, BGLogger logger, string sheetName, BGSyncNameMapConfig nameMapConfig,
            BGExcelIdResolverART idResolver)
        {
            this.meta = meta;
            this.ignoreNew = ignoreNew;
            info = new BGEntitySheetInfo(meta.Id, meta.Name, sheetNumber) {SheetName = sheetName ?? meta.Name};
            this.idResolver = idResolver ?? new BGExcelIdResolverIdRT(logger);
            this.logger = logger;

            if (headersRow == null) return;

            logger.SubSection(() =>
            {
                info.PhysicalColumnCount = headersRow.Cells.Count;

                ForEachCell(headersRow, (i, cell) =>
                {
                    string name;
                    if (cell.CellType == CellType.Formula)
                    {
                        if (logger.AppendWarning(cell.CachedFormulaResultType != CellType.String, "[$]->[error:header is formula, but formula type is not a string (type=$)],",
                            i, cell.CachedFormulaResultType.ToString())) return;
                        name = cell.StringCellValue;
                    }
                    else
                    {
                        if (logger.AppendWarning(cell.CellType != CellType.String, "[$]->[error:not a string and not a formula],", i)) return;
                        name = cell.StringCellValue;
                    }

                    var index = cell.ColumnIndex;

                    if (logger.AppendWarning(string.IsNullOrEmpty(name), "[$]->[error:empty string],", i)) return;

                    switch (name)
                    {
                        case BGBookInfo.IdHeader:
                            //id
                            logger.AppendLine("[column #$ $]->[_id],", i, BGBookInfo.IdHeader);
                            info.IndexId = index;
                            break;
                        default:

                            var field = nameMapConfig == null ? meta.GetField(name, false) : nameMapConfig.Map(meta, name);

                            if (logger.AppendWarning(field == null, "[column #$ $]->[warning: no field with such name or no proper mapping- skipping,", i, name)) return;

                            logger.AppendLine("[column #$ $]->[$],", i, name, field.Name);
                            info.AddField(field.Id, index);
                            break;
                    }
                });
            }, "Mapping for [$]", meta.Name);
        }

        //================================================================================================
        //                                              Read a row
        //================================================================================================

        public void Read(IRow row, ref int existingCount, ref int newCount)
        {
            if (row == null) return;
            if (row.RowNum == 0) return;

            //----------- id
            var entityId = BGId.Empty;
            try
            {
                entityId = idResolver.ResolveId(this, info, row);
            }
            catch (Exception e)
            {
                logger.AppendWarning("Exception while trying to fetch entity's id, row number=$. Error=$", row.RowNum, e.Message);
                throw new ExitException();
            }

            if (!entityId.IsEmpty)
            {
                //entity id is found
                if (info.HasRow(entityId))
                {
                    //duplicate entity
                    logger.AppendWarning("Duplicate entity found. id=$", entityId);
                    throw new ExitException();
                }

                info.AddRow(entityId, row.RowNum);
            }
            else
            {
                //entity id is not set- we assume it's a new row
                if (ignoreNew) return;

                //for now we do not accept empty ID values for field resolver
                if (idResolver is BGExcelIdFieldResolverIRT) return;

                // we need to find at least one non empty cell- otherwise ignore it
                if (IsRowEmpty(row)) return;
            }


            BGEntity entity = null;
            var newEntity = false;
            //------ fields
            info.ForEachField((id, column) =>
            {
                if (entity == null)
                {
                    entity = EnsureEntity(row, entityId);
                    newEntity = entityId == BGId.Empty;
                }

                var cell = row.GetCell(column);
                if (cell == null) return;

                var field = meta.GetField(id);

                //custom
                if (field.CustomStringFormatterTypeAsString != null)
                {
                    var processor = BGExcelSheetWriterEntityRT.GetProcessor(field.CustomStringFormatterTypeAsString);
                    if (processor is BGExcelCellReadProcessorRT)
                    {
                        ((BGExcelCellReadProcessorRT) processor).OnRead(cell, field, entity);
                        return;
                    }
                }

                ReadNotNull(row, column, s =>
                {
                    try
                    {
                        BGUtil.FromString(field, entity.Index, s);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(BGUtil.Format("Can not fetch field $ value for entity with id=$. Value=$. Error=$", field.Name, entityId, s, e.Message));
                        logger.AppendWarning("Can not fetch field $ value for entity with id=$. Value=$. Error=$", field.Name, entityId, s, e.Message);
                    }
                });
            });

            if (entity != null)
            {
                if (newEntity) newCount++;
                else existingCount++;
            }
        }

        private bool IsRowEmpty(IRow row)
        {
            //this is not optimal - but we can not change it without changing BGDatabase package
            var hasValue = false;
            info.ForEachField((id, index) =>
            {
                if (hasValue) return;
                hasValue = !BGExcelSheetWriterART.IsCellEmpty(row, index);
            });
            return !hasValue;
        }

        private BGEntity EnsureEntity(IRow row, BGId entityId)
        {
            // create an entity if required
            BGEntity entity;
            if (entityId != BGId.Empty)
            {
                //--------------------  existing entity
                entity = meta.NewEntity(entityId);
            }
            else
            {
                //-------------------- new entity
                entity = meta.NewEntity();
                if (info.IndexId >= 0)
                {
                    //update id if idcolumn exists
                    var idCell = row.GetCell(info.IndexId) ?? row.CreateCell(info.IndexId);
                    idCell.SetCellType(CellType.String);
                    idCell.SetCellValue(entity.Id.ToString());
                }
            }

            return entity;
        }
    }
}