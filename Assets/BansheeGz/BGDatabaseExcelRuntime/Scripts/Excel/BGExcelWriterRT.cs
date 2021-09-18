/*
<copyright file="BGExcelWriterRT.cs" company="BansheeGz">
    Copyright (c) 2019-2021 All Rights Reserved
</copyright>
*/

using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BansheeGz.BGDatabase
{
    public class BGExcelWriterRT
    {
        private readonly BGLogger logger;
        private readonly BGRepo repo;
        private readonly BGRepo sourceRepo;
        private readonly IWorkbook book;
        private readonly BGBookInfo bookInfo;
        private readonly BGMergeSettingsEntity entitySettings;
        private readonly bool transferRowsOrder;

        private readonly BGSyncNameMapConfig nameMapConfig;
        private readonly BGSyncIdConfig idConfig;

        public IWorkbook Book
        {
            get { return book; }
        }

        public BGExcelWriterRT(BGLogger logger, BGRepo sourceRepo, BGRepo repo, bool useXml, bool transferRowsOrder, BGSyncNameMapConfig nameMapConfig = null, BGSyncIdConfig idConfig = null)
        {
            this.logger = logger;
            this.repo = repo;
            this.sourceRepo = sourceRepo;
            book = useXml ? (IWorkbook) new XSSFWorkbook() : new HSSFWorkbook();
            this.transferRowsOrder = transferRowsOrder;
            this.nameMapConfig = nameMapConfig;
            this.idConfig = idConfig;
            bookInfo = new BGBookInfo();
            Write();
        }

        public BGExcelWriterRT(BGLogger logger, BGRepo sourceRepo, BGRepo repo, BGMergeSettingsEntity entitySettings, IWorkbook book, BGBookInfo bookInfo,
            bool transferRowsOrder, BGSyncNameMapConfig nameMapConfig = null, BGSyncIdConfig idConfig = null)
        {
            this.logger = logger;
            this.repo = repo;
            this.sourceRepo = sourceRepo;
            this.entitySettings = entitySettings;
            this.transferRowsOrder = transferRowsOrder;
            this.book = book;
            this.nameMapConfig = nameMapConfig;
            this.idConfig = idConfig;
            this.bookInfo = (BGBookInfo) bookInfo.Clone();
            Write();
        }

        private void Write()
        {
            logger.Section("Writing xls file", () =>
            {
                //entities
                new BGExcelSheetWriterEntityRT(logger, sourceRepo, repo, book, bookInfo, entitySettings, transferRowsOrder, nameMapConfig, idConfig).Write();
            });
        }
    }
}