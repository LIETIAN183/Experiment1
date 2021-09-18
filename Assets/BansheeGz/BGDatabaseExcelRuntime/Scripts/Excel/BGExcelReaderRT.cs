/*
<copyright file="BGExcelReaderRT.cs" company="BansheeGz">
    Copyright (c) 2019-2021 All Rights Reserved
</copyright>
*/

using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BansheeGz.BGDatabase
{
    public class BGExcelReaderRT
    {
        private readonly BGBookInfo info = new BGBookInfo();
        private readonly IWorkbook book;
        private readonly BGLogger logger;
        private BGSyncNameMapConfig nameMapConfig;
        private BGExcelIdResolverFactoryRT idResolver;

        public IWorkbook Book
        {
            get { return book; }
        }

        public BGBookInfo Info
        {
            get { return info; }
        }

        public BGExcelReaderRT(BGLogger logger, byte[] content, bool useXml)
        {
            this.logger = logger;
            logger.AppendLine("Trying to read xls file..");

            using (var stream = new MemoryStream(content)) book = useXml ? (IWorkbook) new XSSFWorkbook(stream) : new HSSFWorkbook(stream);

            logger.AppendLine("Content is ok. $ sheets found", book.NumberOfSheets);
        }

        public BGExcelReaderRT(BGLogger logger, byte[] content, bool useXml, BGSyncNameMapConfig nameMapConfig) : this(logger, content, useXml)
        {
            this.nameMapConfig = nameMapConfig;
        }
        public BGExcelReaderRT(BGLogger logger, byte[] content, bool useXml, BGSyncNameMapConfig nameMapConfig, BGExcelIdResolverFactoryRT idResolver) : this(logger, content, useXml, nameMapConfig)
        {
            this.idResolver = idResolver;
        }

        public void ReadEntities(BGRepo repo, bool ignoreNew)
        {
            BGExcelSheetReaderEntityRT.ReadEntities(book, info, repo, logger, ignoreNew , nameMapConfig, idResolver);
        }
    }
}